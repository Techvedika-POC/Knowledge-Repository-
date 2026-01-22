import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import api from "../api";
import { toast } from "react-hot-toast";

const safeData = (res) => {
  if (!res) return [];
  if (Array.isArray(res)) return res;
  if (res.data === undefined) return res;
  if (Array.isArray(res.data)) return res.data;
  if (res.data.data !== undefined) return res.data.data;
  return res.data;
};

export default function JuryDashboard() {
  const [user] = useState(() => {
    try { return JSON.parse(localStorage.getItem("user") || "null"); }
    catch { return null; }
  });

  const juryId = user?.userId ?? localStorage.getItem("userId") ?? null;
  const [events, setEvents] = useState([]);
  const [selectedEventId, setSelectedEventId] = useState("");
  const [teams, setTeams] = useState([]);
  const [scoreMap, setScoreMap] = useState({});
  const [chatMessages, setChatMessages] = useState([]);
  const [chatText, setChatText] = useState("");
  const [replyTarget, setReplyTarget] = useState(null);
  const [loadingEvents, setLoadingEvents] = useState(false);
  const [loadingTeams, setLoadingTeams] = useState(false);
  const chatScrollRef = useRef(null);

  const fetchEvents = useCallback(async () => {
    setLoadingEvents(true);
    try {
      const res = await api.get("/Events/type/Ideathon");
      setEvents(safeData(res) || []);
    } catch (err) {
      toast.error("Failed to load events");
    } finally {
      setLoadingEvents(false);
    }
  }, []);

  useEffect(() => { fetchEvents(); }, [fetchEvents]);


  const loadTeams = useCallback(async (eventId) => {
    if (!eventId) return;

    setLoadingTeams(true);
    try {
      const res = await api.get(`/JuryPanel/Teams/${eventId}`);
      const raw = safeData(res) || [];

      const normalized = raw.map((t) => ({
        ...t,
        teamId: t.teamId ?? t.TeamId ?? t.id,
        teamName: t.teamName ?? t.TeamName ?? t.name,
        members: t.members ?? []
      }));

      setTeams(normalized);
    } catch (err) {
      toast.error("Failed to load teams");
    } finally {
      setLoadingTeams(false);
    }
  }, []);

  useEffect(() => {
    if (selectedEventId) {
      loadTeams(selectedEventId);
      loadJuryChat(selectedEventId);
    } else {
      setTeams([]);
      setChatMessages([]);
    }
  }, [selectedEventId, loadTeams]);

  // ---------------- SCORE SUBMISSION ----------------
  const submitScore = async (teamId) => {
    const data = scoreMap[teamId] || {};
    const total = data.totalScore;

    if (isNaN(Number(total))) return toast.error("Enter a valid numeric score");

    try {
      const payload = {
        eventId: selectedEventId,
        teamId,
        approvedBy: juryId,
        totalScore: Number(total),
        remarks: data.remarks || ""
      };

      await api.post("/JuryPanel/FinalScore", payload);
      toast.success("Score submitted");
    } catch (err) {
      const status = err?.response?.status;
      const serverMsg = err?.response?.data?.error || err?.response?.data || err?.message;
      if (status === 409) {
        toast.error(serverMsg || "You have already submitted a score for this team.");
      } else {
        console.error("submitScore error:", err?.response || err);
        toast.error("Failed to submit score");
      }
    }
  };

  // ---------------- JURY GROUP CHAT ----------------
  const loadJuryChat = useCallback(async (eventId) => {
    try {
      const res = await api.get(`/JuryPanel/JuryChat/${eventId}`);

      setChatMessages(safeData(res) || []);
    } catch (err) {
      setChatMessages([]);
    }
  }, []);


  const sortedChatMessages = useMemo(() => {
    const arr = Array.isArray(chatMessages) ? [...chatMessages] : [];
    arr.sort((a, b) => {
      const ta = a?.createdOn ? new Date(a.createdOn).getTime() : 0;
      const tb = b?.createdOn ? new Date(b.createdOn).getTime() : 0;
      return ta - tb;
    });
    return arr;
  }, [chatMessages]);


  useEffect(() => {
    if (!chatScrollRef.current) return;
    try {
      chatScrollRef.current.scrollIntoView({ behavior: "smooth", block: "end" });
    } catch {

    }
  }, [sortedChatMessages.length]);

  const postJuryChat = async () => {
    if (!chatText.trim()) return toast.error("Enter a message");
    if (!selectedEventId) return toast.error("Select an event first");

    try {
      const payload = {
        eventId: selectedEventId,
        senderJuryId: juryId,
        message: chatText.trim(),

        replyToMessageId: replyTarget?.messageId ?? replyTarget?.MessageId ?? null
      };


      await api.post("/JuryPanel/JuryChat", payload);


      setChatText("");
      setReplyTarget(null);
      await loadJuryChat(selectedEventId);
      toast.success("Message sent");
    } catch (err) {
      console.error("postJuryChat error:", err?.response || err);
      toast.error("Failed to send message");
    }
  };


  const handleChatKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      postJuryChat();
    }
  };

  const renderReplyPreview = (replyObj) => {
    if (!replyObj) return null;

    const reply = replyObj;
    const previewText = reply.message ?? reply.Message ?? "";
    const sender = reply.senderName ?? reply.SenderName ?? reply.senderEmail ?? reply.SenderEmail ?? "unknown";
    const created = reply.createdOn ?? reply.CreatedOn ?? null;

    return (
      <div className="mb-2 p-2 bg-white/60 border-l-2 border-indigo-400 rounded text-xs text-gray-700">
        <div className="font-semibold text-[11px]">{sender}</div>
        <div className="truncate" title={previewText}>{previewText}</div>
        {created && (
          <div className="text-[10px] opacity-60 mt-1">
            {new Date(created).toLocaleString()}
          </div>
        )}
      </div>
    );
  };

  // ---------------- UI ----------------
  return (
    <div className="p-6 max-w-7xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-3xl font-bold">Jury Panel</h1>
      </div>

      {/* Event Selector */}
      <div className="mb-6">
        <label className="block text-sm font-medium">Assigned Ideathon Events</label>
        <div className="flex gap-3 mt-2 items-center">
          <select
            className="border p-2 rounded-md w-full max-w-lg"
            value={selectedEventId}
            onChange={(e) => setSelectedEventId(e.target.value)}
          >
            <option value="">-- select event --</option>
            {events.map((ev) => (
              <option key={ev.eventId} value={ev.eventId}>
                {ev.title}
              </option>
            ))}
          </select>
          <div className="text-sm text-gray-500">
            {loadingEvents ? "loading..." : `${events.length} events`}
          </div>
        </div>
      </div>

      {!selectedEventId ? (
        <div className="p-6 bg-white border rounded text-center text-gray-600">
          Select an event to view teams and group chat.
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">

          {/* Teams + Scoring */}
          <div className="lg:col-span-6">
            <h2 className="text-xl font-semibold mb-3">Teams ({teams.length})</h2>

            {teams.map((team) => (
              <div key={team.teamId} className="border p-3 rounded bg-white shadow-sm mb-4">
                <div className="font-bold">{team.teamName}</div>

                {/* Members */}
                <ul className="mt-2 text-sm ml-5 list-disc">
                  {team.members.map((m) => (
                    <li key={m.userId}>
                      {m.name} — <span className="text-gray-500">{m.email}</span>
                    </li>
                  ))}
                </ul>

                {/* Scoring */}
                <div className="mt-3 flex gap-2 items-center">
                  <input
                    type="number"
                    placeholder="score"
                    className="border p-1 rounded w-20 text-sm"
                    value={scoreMap[team.teamId]?.totalScore ?? ""}
                    onChange={(e) =>
                      setScoreMap((s) => ({
                        ...s,
                        [team.teamId]: { ...(s[team.teamId] || {}), totalScore: e.target.value }
                      }))
                    }
                  />

                  <button
                    onClick={() => submitScore(team.teamId)}
                    className="px-3 py-1 bg-green-600 text-white rounded text-sm"
                  >
                    Submit Score
                  </button>
                </div>
              </div>
            ))}
          </div>

          {/* Jury Group Chat */}
          <div className="lg:col-span-6 flex flex-col">
            <h4 className="text-lg font-semibold mb-2">Jury Group Chat</h4>

            {/* Chat messages container */}
            <div className="flex-1 overflow-y-auto p-4 bg-white border rounded-lg shadow-sm">
              <div className="flex flex-col gap-3">
                {sortedChatMessages.map((m) => {

                  const senderId =
                    (m.senderJuryId ?? m.senderId ?? (m.sender && (m.sender.id || m.sender.userId)) ?? "").toString();
                  const mine = !!(senderId && juryId && senderId === juryId.toString());

                  const replyObj =
                    m.replyTo ??
                    m.ReplyTo ??
                    m.repliedTo ??
                    m.RepliedTo ??
                    (m.replyToMessage ?? null);

                  const displaySender = m.senderName ?? m.SenderName ?? m.senderEmail ?? m.SenderEmail ?? "Unknown";

                  return (
                    <div
                      key={m.messageId}
                      className={`flex ${mine ? "justify-end" : "justify-start"} items-start`}
                    >
                      <div
                        className={`max-w-[75%] p-3 rounded-2xl shadow break-words ${mine ? "bg-blue-500 text-white rounded-br-none" : "bg-gray-100 text-gray-800 rounded-bl-none"
                          }`}
                      >

                        {replyObj && (
                          <div className={`p-2 rounded mb-2 ${mine ? "bg-white/10 border" : "bg-white border"}`}>
                            <div className="text-[11px] font-semibold opacity-90">
                              {replyObj.senderName ?? replyObj.SenderName ?? replyObj.senderEmail ?? replyObj.SenderEmail ?? "Unknown"}
                            </div>
                            <div className="text-sm text-gray-700 truncate" title={replyObj.message ?? replyObj.Message}>
                              {replyObj.message ?? replyObj.Message}
                            </div>
                          </div>
                        )}

                        <div className="text-xs font-semibold opacity-90">
                          {displaySender}
                        </div>

                        <div className="mt-1 whitespace-pre-wrap">{m.message ?? m.Message}</div>

                        <div className="flex items-center justify-between mt-2">
                          <div className="text-[10px] opacity-60">
                            {m.createdOn ? new Date(m.createdOn).toLocaleString() : (m.CreatedOn ? new Date(m.CreatedOn).toLocaleString() : "")}
                          </div>

                          <button
                            onClick={() => setReplyTarget(m)}
                            className={`text-xs ml-3 ${mine ? "text-indigo-200 hover:underline" : "text-indigo-600 hover:underline"}`}
                          >
                            Reply
                          </button>
                        </div>
                      </div>
                    </div>
                  );
                })}
                <div ref={chatScrollRef} />
              </div>
            </div>
            <div className="mt-3">
              {replyTarget && (
                <div className="mb-2 p-2 bg-gray-50 border-l-4 border-indigo-500 rounded flex items-start justify-between">
                  <div className="flex-1 min-w-0">
                    <div className="text-[12px] font-semibold">
                      Replying to {replyTarget.senderName || replyTarget.SenderName || replyTarget.senderEmail || replyTarget.SenderEmail || "Unknown"}
                    </div>
                    <div className="text-sm text-gray-700 truncate" title={replyTarget.message ?? replyTarget.Message}>
                      {replyTarget.message ?? replyTarget.Message}
                    </div>
                  </div>
                  <button
                    onClick={() => setReplyTarget(null)}
                    className="text-sm text-red-500 ml-3"
                    aria-label="Cancel reply"
                  >
                    Cancel
                  </button>
                </div>
              )}

              <div className="flex gap-2 items-center bg-white p-2 rounded-lg border shadow-sm">
                <textarea
                  value={chatText}
                  onChange={(e) => setChatText(e.target.value)}
                  onKeyDown={handleChatKeyDown}
                  placeholder="Type a message..."
                  rows={1}
                  className="flex-1 p-2 rounded-full border bg-gray-50 resize-none focus:outline-none text-sm"
                />

                <button
                  onClick={postJuryChat}
                  className="px-4 py-1.5 bg-green-600 text-white rounded-md font-semibold text-sm hover:bg-indigo-700"
                >
                  Send
                </button>
              </div>
              <div className="text-xs text-gray-400 mt-1">Press Enter to send (Shift+Enter for newline).</div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
