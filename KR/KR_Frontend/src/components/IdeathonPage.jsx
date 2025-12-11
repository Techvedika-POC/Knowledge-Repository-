import React, { useEffect, useMemo, useRef, useState } from "react";
import { Calendar } from "lucide-react";
import { motion } from "framer-motion";
import api from "../api";
import { useNavigate } from "react-router-dom";
import KnowledgeCardsDisplay from "../components/KnowledgeCardsDisplay";
import PreviewModal from "../components/PreviewModal";

export default function IdeathonPage() {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedEventId, setSelectedEventId] = useState(null);
  const [userId] = useState(localStorage.getItem("userId"));
  const [previewItem, setPreviewItem] = useState(null);
  const [viewMode, setViewMode] = useState("current"); // "current" | "past" | "all"
  const navigate = useNavigate();

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        setLoading(true);
        const res = await api.get("/Events/type/Ideathon");
        const eventsData = res?.data || [];
        setEvents(eventsData);
        setSelectedEventId(null);
      } catch (err) {
        console.error(err);
        setError("Failed to load Ideathon events.");
      } finally {
        setLoading(false);
      }
    };
    fetchEvents();
  }, []);

  const now = () => new Date();
  const isCurrentEvent = (event) => {
    if (!event) return false;
    const n = now();
    if (event.startDate && event.endDate) return new Date(event.startDate) <= n && n <= new Date(event.endDate);
    if (event.finalSubmissionDeadline) return n <= new Date(event.finalSubmissionDeadline);
    if (event.startDate) return new Date(event.startDate) <= n;
    return false;
  };

  const isPastEvent = (event) => {
    if (!event) return false;
    const n = now();
    if (event.endDate) return n > new Date(event.endDate);
    if (event.finalSubmissionDeadline) return n > new Date(event.finalSubmissionDeadline);
    return false;
  };

  const isFinished = (event) => isPastEvent(event);

  const filteredEvents = useMemo(() => {
    const list = events.filter((ev) => {
      if (viewMode === "current") return isCurrentEvent(ev);
      if (viewMode === "past") return isPastEvent(ev);
      return true; // all
    });
    return list;
  }, [events, viewMode]);

  const selectedEvent = events.find((e) => e.eventId === selectedEventId) || null;

  const goToRegistration = (eventId) => navigate("/app/events/event-registration", { state: { eventId } });
  const goToUpload = (eventId) => navigate("/app/upload-knowledge", { state: { eventId } });

  const handleSubmitIdea = (eventId, isRegistered, eventObj) => {
    if (!eventObj) return;
    if (eventObj.finalSubmissionDeadline) {
      const deadline = new Date(eventObj.finalSubmissionDeadline);
      if (new Date() > deadline) {
        alert("Submissions are closed for this event (deadline passed).");
        return;
      }
    }
    if (!userId) return navigate("/login");
    if (isRegistered) goToUpload(eventId);
    else goToRegistration(eventId);
  };

  // scroll ref for selected detail
  const detailRef = useRef(null);
  useEffect(() => {
    if (selectedEventId && detailRef.current) {
      detailRef.current.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  }, [selectedEventId]);

  if (loading) return <p className="text-center text-gray-600">Loading events...</p>;
  if (error) return <p className="text-center text-red-500">{error}</p>;
  if (events.length === 0) return <p className="text-center text-gray-600">No Ideathon events available.</p>;

  return (
    <div className="p-6 space-y-6">
      {/* Header with subtle gradient */}
      <header className="rounded-2xl p-6 bg-gradient-to-r from-indigo-50 via-sky-50 to-emerald-50 shadow-sm">
        <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
          <div>
            <h1 className="text-3xl font-extrabold text-gray-900">Ideathon <span className="text-indigo-600">Hub</span></h1>
            <p className="text-sm text-gray-600 mt-1 max-w-2xl">Browse ideathons — switch between <span className="font-semibold text-sky-600">Current</span>, <span className="font-semibold text-gray-600">Past</span> or <span className="font-semibold text-indigo-600">All</span>. Click an event to open full details below.</p>
          </div>

          {/* Segmented control (centered) */}
          <div className="w-full md:w-2/5 lg:w-1/3">
            <nav className="grid grid-cols-3 gap-6 bg-white p-2 rounded-3xl shadow-md">
              {[
                { key: "current", label: `Current (${events.filter(isCurrentEvent).length})` },
                { key: "past", label: `Past (${events.filter(isPastEvent).length})` },
                { key: "all", label: `All (${events.length})` },
              ].map((t) => (
                <button
                  key={t.key}
                  onClick={() => { setViewMode(t.key); setSelectedEventId(null); }}
                  className={`py-3 rounded-2xl font-semibold text-sm transition-shadow text-center ${viewMode === t.key ? "bg-gradient-to-r from-blue-600 to-sky-500 text-white shadow-lg" : "bg-gray-50 text-gray-700 hover:bg-gray-100"}`}
                  type="button"
                >
                  {t.label}
                </button>
              ))}
            </nav>
          </div>
        </div>
      </header>

      {/* Selected event detail (full width) */}
      <div ref={detailRef}>
        {selectedEvent && (
          <motion.div initial={{ opacity: 0, y: -6 }} animate={{ opacity: 1, y: 0 }} className="w-full">
            <EventDetail
              event={selectedEvent}
              userId={userId}
              onPreview={(item) => setPreviewItem(item)}
              onRegister={() => goToRegistration(selectedEvent.eventId)}
              onSubmitIdea={(eid, isReg) => handleSubmitIdea(eid, isReg, selectedEvent)}
              fullWidth
              isFinished={isFinished(selectedEvent)}
            />
          </motion.div>
        )}
      </div>

      {/* Cards grid: hide selected event's card so detail is canonical */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredEvents
          .filter((ev) => ev.eventId !== selectedEventId)
          .map((ev) => (
            <motion.div
              key={ev.eventId}
              onClick={() => setSelectedEventId(ev.eventId)}
              whileHover={{ translateY: -4 }}
              className={`group text-left p-5 rounded-2xl shadow-sm border transition-all bg-white hover:shadow-lg cursor-pointer ${selectedEventId === ev.eventId ? "ring-2 ring-indigo-100" : ""}`}
              role="button"
              tabIndex={0}
              onKeyDown={(e) => {
                if (e.key === "Enter" || e.key === " ") {
                  e.preventDefault();
                  setSelectedEventId(ev.eventId);
                }
              }}
            >
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <h3 className="font-semibold text-lg text-gray-800 group-hover:text-indigo-600">{ev.title}</h3>
                  <p className="mt-2 text-sm text-gray-600 line-clamp-3">{ev.description}</p>
                </div>
                <div className="ml-2 flex flex-col items-end gap-2">
                  <div className={`text-xs px-2 py-1 rounded-full font-medium ${isCurrentEvent(ev) ? 'bg-sky-50 text-sky-700' : isPastEvent(ev) ? 'bg-gray-100 text-gray-600' : 'bg-amber-50 text-amber-700'}`}>{isCurrentEvent(ev) ? 'Ongoing' : isPastEvent(ev) ? 'Finished' : 'Upcoming'}</div>
                  <div className="text-xs text-gray-400">{ev.startDate ? new Date(ev.startDate).toLocaleDateString() : ''}</div>
                </div>
              </div>

              <div className="mt-4 flex items-center justify-between">
                <div className="flex items-center gap-3 text-xs text-gray-500">
                  {ev.finalSubmissionDeadline && (
                    <div className="flex items-center gap-1"><Calendar className="w-4 h-4 text-gray-400" /> <span>Deadline: <span className="font-medium text-gray-700">{new Date(ev.finalSubmissionDeadline).toLocaleDateString()}</span></span></div>
                  )}
                </div>

                <div className="flex items-center gap-2">
                  <button
                    onClick={(e) => { e.stopPropagation(); setSelectedEventId(ev.eventId); }}
                    className="px-3 py-1 rounded-full border text-sm text-gray-700 hover:bg-gray-50"
                    type="button"
                  >
                    Open
                  </button>
                  <button
                    onClick={(e) => { e.stopPropagation(); if (isCurrentEvent(ev)) navigate('/app/upload-knowledge', { state: { eventId: ev.eventId } }); else alert('Submission not allowed'); }}
                    className="px-3 py-1 rounded-full bg-indigo-600 text-white text-sm hover:bg-indigo-700"
                    type="button"
                  >
                    Submit
                  </button>
                </div>
              </div>
            </motion.div>
          ))}
      </div>

      {previewItem && <PreviewModal item={previewItem} onClose={() => setPreviewItem(null)} />}
    </div>
  );
}

/* Updated EventDetail — tab label renamed to "Feedback" (UI only); internal tab key remains "chat" */
function EventDetail({ event, userId, onPreview, onRegister, onSubmitIdea, fullWidth = false, isFinished = false }) {
  const [isRegistered, setIsRegistered] = useState(false);
  const [submissions, setSubmissions] = useState([]);
  const [insight, setInsight] = useState(null);

  // Chat state
  const [teamId, setTeamId] = useState(null);
  const [chatMessages, setChatMessages] = useState([]);
  const [chatText, setChatText] = useState("");
  const [loadingChat, setLoadingChat] = useState(false);
  const [postingChat, setPostingChat] = useState(false);

  const [tab, setTab] = useState("overview");

  // auto-scroll reference for chat bottom
  const chatBottomRef = useRef(null);
  const chatScrollContainerRef = useRef(null);

  useEffect(() => {
    const fetch = async () => {
      try {
        if (userId) {
          const regRes = await api.get(`/EventRegistration/is-registered/${event.eventId}?userId=${userId}`);
          setIsRegistered(regRes?.data?.isRegistered || false);

          const insightRes = await api.get(`/Events/${event.eventId}/user/${userId}/insight`);
          const data = insightRes?.data?.data;
          if (data) {
            setSubmissions(data.submissions || []);
            setInsight(data);
            if (data.teamId) setTeamId(data.teamId);
          } else {
            setSubmissions([]);
            setInsight(null);
          }
        }
      } catch (err) {
        console.warn(err);
      }
    };
    fetch();
  }, [event.eventId, userId]);

  // Chat helpers
  const fetchTeamChat = async (tid) => {
    if (!tid) return;
    try {
      setLoadingChat(true);
      const res = await api.get(`/Communication/team/${tid}/chat?userId=${userId}`);
      setChatMessages(res?.data || []);
    } catch (err) {
      console.warn("Failed to load team chat", err);
    } finally {
      setLoadingChat(false);
    }
  };

  useEffect(() => {
    const tid = teamId || insight?.teamId || null;
    if (tid) fetchTeamChat(tid);
  }, [teamId, insight]);

  // auto scroll to bottom when messages change
  useEffect(() => {
    if (chatBottomRef.current) {
      chatBottomRef.current.scrollIntoView({ behavior: "smooth", block: "end" });
    } else if (chatScrollContainerRef.current) {
      chatScrollContainerRef.current.scrollTop = chatScrollContainerRef.current.scrollHeight;
    }
  }, [chatMessages.length]);

  const handleSendChat = async () => {
    const tid = teamId || insight?.teamId;
    if (!tid) return alert("Team not available for chat yet.");
    if (!chatText.trim()) return;
    try {
      setPostingChat(true);
      const body = { MessageText: chatText, SenderName: "" }; // adapt if your DTO differs
      await api.post(`/Communication/team/${tid}/chat?userId=${userId}`, body);
      setChatText("");
      await fetchTeamChat(tid);
    } catch (err) {
      console.error("Failed to send chat", err);
      alert("Failed to send message");
    } finally {
      setPostingChat(false);
    }
  };

  // Build timeline (unchanged)
  const timelineEntries = [];
  if (event.startDate) timelineEntries.push({ key: "start", label: "Start", date: new Date(event.startDate) });
  if (event.registrationCloseDate) timelineEntries.push({ key: "regClose", label: "Registration Close", date: new Date(event.registrationCloseDate) });
  if (event.mentorCheckpointStart) timelineEntries.push({ key: "mentorStart", label: "Mentor Checkpoint Start", date: new Date(event.mentorCheckpointStart) });
  if (event.mentorCheckpointEnd) timelineEntries.push({ key: "mentorEnd", label: "Mentor Checkpoint End", date: new Date(event.mentorCheckpointEnd) });
  if (event.finalSubmissionDeadline) timelineEntries.push({ key: "submission", label: "Final Submission Deadline", date: new Date(event.finalSubmissionDeadline) });
  if (event.ideaPresentationStart) timelineEntries.push({ key: "presentationStart", label: "Idea Presentation Start", date: new Date(event.ideaPresentationStart) });
  if (event.ideaPresentationEnd) timelineEntries.push({ key: "presentationEnd", label: "Idea Presentation End", date: new Date(event.ideaPresentationEnd) });
  if (event.endDate) timelineEntries.push({ key: "end", label: "End", date: new Date(event.endDate) });
  if (event.winnersAnnouncementDate) timelineEntries.push({ key: "winners", label: "Winners Announcement", date: new Date(event.winnersAnnouncementDate) });
  timelineEntries.sort((a, b) => a.date - b.date);

  return (
    <div className={`rounded-2xl p-6 shadow-md border bg-white ${fullWidth ? "" : ""}`}>
      <div className="flex flex-col md:flex-row items-start justify-between gap-4">
        <div className="flex-1">
          <h2 className="text-2xl font-bold text-gray-800">{event.title} <span className="text-indigo-600">• {event.category || "Ideathon"}</span></h2>
          <p className="text-sm text-gray-500 mt-1">{event.description}</p>
          <div className="mt-3 flex items-center gap-3 text-xs text-gray-500">
            {event.finalSubmissionDeadline && <div className="flex items-center gap-1"><Calendar className="w-4 h-4 text-gray-400" /> <span className="text-gray-700">Deadline: <span className="font-semibold">{new Date(event.finalSubmissionDeadline).toLocaleString()}</span></span></div>}
            <div className="px-2 py-0.5 rounded-full bg-sky-50 text-sky-700">{event.startDate ? new Date(event.startDate).getFullYear() : ""}</div>
          </div>
        </div>

        <div className="flex flex-col items-end gap-2">
          <div className={`text-sm px-3 py-1 rounded-full font-semibold ${!isFinished ? "bg-indigo-600 text-white" : "bg-gray-200 text-gray-700"}`}>
            {!isFinished ? "Open" : "Finished"}
          </div>
        </div>
      </div>

      {/* small nav */}
      <div className="mt-6 flex gap-3 border-b pb-3">
        <button onClick={() => setTab("overview")} className={`px-3 py-2 text-sm rounded-md ${tab === "overview" ? "bg-indigo-50 text-indigo-700 font-semibold" : "text-gray-600"}`} type="button">Overview</button>
        <button onClick={() => setTab("submissions")} className={`px-3 py-2 text-sm rounded-md ${tab === "submissions" ? "bg-indigo-50 text-indigo-700 font-semibold" : "text-gray-600"}`} type="button">Submissions ({submissions.length})</button>
        {/* label changed from "Group Chat" to "Feedback" */}
        <button onClick={() => setTab("chat")} className={`px-3 py-2 text-sm rounded-md ${tab === "chat" ? "bg-indigo-50 text-indigo-700 font-semibold" : "text-gray-600"}`} type="button">Feedback</button>
      </div>

      <div className="mt-4">
        {tab === "overview" && (
          <div>
            <h4 className="text-sm font-semibold text-gray-700">Full Timeline</h4>
            <div className="mt-3 space-y-2">
              {timelineEntries.map((t) => (
                <div key={t.key} className="flex items-center gap-4 bg-gray-50 rounded-md p-3">
                  <div className="w-44 text-xs text-gray-500">{t.label}</div>
                  <div className="text-sm text-gray-700">{t.date.toLocaleString()}</div>
                </div>
              ))}
            </div>

            {!isFinished && (
              <div className="mt-6 flex gap-3">
                <button onClick={() => onSubmitIdea(event.eventId, isRegistered, event)} className="px-5 py-2 rounded-full bg-gradient-to-r from-indigo-600 to-sky-500 text-white font-semibold shadow" type="button">Submit Your Idea</button>
                {!isRegistered && <button onClick={() => onRegister()} className="px-5 py-2 rounded-full border font-semibold" type="button">Register</button>}
              </div>
            )}
          </div>
        )}

        {tab === "submissions" && (
          <div className="mt-3">
            {submissions.length > 0 ? (
              <KnowledgeCardsDisplay items={submissions.map((s) => ({ itemId: s.itemId, title: s.itemTitle, description: s.description || s.itemDescription, tags: s.tags || [], ownerName: s.submittedBy || s.createdByName }))} userId={userId} onPreview={onPreview} />
            ) : (
              <div className="text-center text-gray-500 p-6">No submissions yet.</div>
            )}
          </div>
        )}

        {tab === "chat" && (
          <div className="mt-3">
            <div className="flex items-center justify-between mb-3">
            
              <h4 className="text-sm font-semibold text-gray-700">Feedback</h4>
              <div className="text-xs text-gray-500">{loadingChat ? "Loading..." : `${chatMessages.length} messages`}</div>
            </div>

            <div className="flex flex-col h-full">
              <div
                ref={chatScrollContainerRef}
                className="flex-1 overflow-auto p-3 space-y-3 border rounded-lg bg-gray-50"
                style={{ maxHeight: 420 }}
              >
                {loadingChat ? (
                  <div className="text-center text-gray-500 p-6">Loading chat...</div>
                ) : chatMessages.length > 0 ? (
                  chatMessages.map((m) => {
                    const isMine = String(m.senderId) === String(userId);
                    const rawName = m.senderName || m.sentByName || m.sender || "Member";
                    const compactName = rawName.split(" ")[0];

                    const timeStr = m.createdOn ? new Date(m.createdOn).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : "";
                    return (
                      <div key={m.messageId || m.id || `${m.createdOn}-${Math.random()}`} className={`w-full flex ${isMine ? "justify-end" : "justify-start"}`}>
                        <div
                          className={`p-3 rounded-xl break-words ${isMine ? "bg-indigo-600 text-white rounded-br-2xl" : "bg-white text-gray-800 rounded-bl-2xl"}`}
                          style={{ maxWidth: "70%" }}
                        >
                          <div className="flex items-center justify-between gap-2">
                            <div className="text-[11px] font-medium truncate" title={rawName}>{compactName}</div>
                            <div className="text-[10px] text-gray-400 ml-2">{timeStr}</div>
                          </div>
                          <div className="mt-1 text-sm leading-snug">{m.messageText || m.message || m.text}</div>
                        </div>
                      </div>
                    );
                  })
                ) : (
                  <div className="text-center text-gray-500 p-6">No messages yet. Say hi 👋</div>
                )}

                <div ref={chatBottomRef} />
              </div>

              <div className="mt-3 flex items-center gap-2">
                <input
                  value={chatText}
                  onChange={(e) => setChatText(e.target.value)}
                  placeholder="Write a message to the team..."
                  className="flex-1 rounded-md border px-3 py-2"
                />
                <button onClick={handleSendChat} disabled={postingChat} className="px-4 py-2 rounded-full bg-indigo-600 text-white" type="button">
                  {postingChat ? "Sending..." : "Send"}
                </button>
                <button onClick={() => {
                  const tid = teamId || insight?.teamId;
                  if (tid) fetchTeamChat(tid);
                }} className="px-3 py-2 rounded-full border text-sm" type="button">Refresh</button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
