import React, { useEffect, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { toast } from "react-hot-toast";
import api from "../api";
import KnowledgeCardsDisplay from "../components/KnowledgeCardsDisplay";

/**
 * MentorDashboard.jsx
 * - Clean Behance-like header (muted gradient)
 * - Team cards with Details (inline expand) and View Details (full view)
 * - Full team view with Tabs: Overview | Submissions | Feedback
 * - Feedback supports 3 view modes: Timeline | Cards | Chat
 * - Add Feedback form toggles open only when requested (with rating slider)
 */

export default function MentorDashboard() {
  const mentorId = localStorage.getItem("userId");

  // Data
  const [teams, setTeams] = useState([]);
  const [loadingTeams, setLoadingTeams] = useState(true);

  // Card inline expansion
  const [expandedCardId, setExpandedCardId] = useState(null);

  // Full team view
  const [selectedTeam, setSelectedTeam] = useState(null);
  const [teamTab, setTeamTab] = useState("overview"); // overview | submissions | feedback

  // Feedback state
  const [feedbackText, setFeedbackText] = useState("");
  const [rating, setRating] = useState(5);
  const [isGivingFeedback, setIsGivingFeedback] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Feedback display mode: timeline | cards | chat
  const [feedbackViewMode, setFeedbackViewMode] = useState("timeline");

  // Load teams
  useEffect(() => {
    let mounted = true;
    const fetchTeams = async () => {
      try {
        const res = await api.get(`/Mentor/${mentorId}/teams`);
        if (mounted) setTeams(res.data || []);
      } catch (err) {
        console.error(err);
        toast.error("Failed to load teams.");
      } finally {
        if (mounted) setLoadingTeams(false);
      }
    };
    if (mentorId) fetchTeams();
    return () => (mounted = false);
  }, [mentorId]);

  // Open full team view
  const openTeamView = async (teamId) => {
    try {
      const res = await api.get(`/Mentor/team/${teamId}`);
      setSelectedTeam(res.data || null);
      setTeamTab("overview");
      setIsGivingFeedback(false);
      setFeedbackViewMode("timeline");
      window.scrollTo({ top: 0, behavior: "smooth" });
    } catch (err) {
      console.error(err);
      toast.error("Failed to load team details.");
    }
  };

  // Submit feedback
  const submitFeedback = async () => {
    if (!selectedTeam) return toast.error("No team selected.");
    if (!feedbackText.trim()) return toast.error("Please enter feedback.");
    const eventId = selectedTeam?.eventId;
    if (!eventId) return toast.error("Invalid event.");

    setIsSubmitting(true);
    try {
      await api.post(`/Mentor/feedback/add`, {
        mentorId,
        teamId: selectedTeam.teamId,
        eventId,
        feedbackText,
        progressRating: rating,
      });
      toast.success("Feedback submitted");
      setFeedbackText("");
      setRating(5);
      setIsGivingFeedback(false);
      // refresh
      openTeamView(selectedTeam.teamId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to submit feedback.");
    } finally {
      setIsSubmitting(false);
    }
  };

  // Utils
  const fmt = (iso) => {
    try {
      return new Date(iso).toLocaleString();
    } catch {
      return iso;
    }
  };

  const initials = (name = "") =>
    name
      .split(" ")
      .map((p) => p[0]?.toUpperCase())
      .slice(0, 2)
      .join("");

  // Render feedback items in different modes
  const FeedbackRender = ({ feedbacks = [] }) => {
    if (!feedbacks || feedbacks.length === 0)
      return <div className="text-sm text-slate-500">No feedback yet.</div>;

    if (feedbackViewMode === "timeline") {
      return (
        <div className="space-y-6">
          {feedbacks.map((fb) => (
            <div key={fb.feedbackId} className="flex gap-4">
              <div className="flex flex-col items-center">
                <div className="w-3 h-3 rounded-full bg-indigo-600 mt-2" />
                <div className="flex-1 w-px bg-slate-200 mt-2" />
              </div>

              <div className="flex-1">
                <div className="bg-white border rounded-md p-3 shadow-sm">
                  <div className="flex items-start justify-between">
                    <div className="text-sm text-slate-800 font-medium">{fb.feedbackText}</div>
                    <div className="text-xs text-slate-400">{fmt(fb.createdOn)}</div>
                  </div>
                  <div className="text-xs text-slate-500 mt-2">
                    Rating: <span className="font-semibold">{fb.progressRating ?? "N/A"}</span>
                  </div>

                  {/* replies */}
                  {fb.replies?.length > 0 && (
                    <div className="mt-3 pl-4 border-l-2 border-indigo-100 space-y-2">
                      {fb.replies.map((r) => (
                        <div key={r.replyId} className="bg-slate-50 p-2 rounded-md border">
                          <div className="text-sm text-slate-700">{r.replyText}</div>
                          <div className="text-xs text-slate-400 mt-1">— {r.userName || "Team"}, {fmt(r.createdOn)}</div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      );
    }

    if (feedbackViewMode === "cards") {
      return (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {feedbacks.map((fb) => (
            <div key={fb.feedbackId} className="bg-white rounded-lg border p-3 shadow-sm">
              <div className="flex justify-between items-start">
                <div className="text-sm font-medium text-slate-800">{fb.feedbackText}</div>
                <div className="text-xs text-slate-400">{fmt(fb.createdOn)}</div>
              </div>
              <div className="text-xs text-slate-500 mt-2">Rating: {fb.progressRating ?? "N/A"}</div>

              {fb.replies?.length > 0 && (
                <div className="mt-3 space-y-2">
                  {fb.replies.map((r) => (
                    <div key={r.replyId} className="flex gap-3 items-start">
                      <div className="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-xs font-semibold text-slate-600">
                        {initials(r.userName)}
                      </div>
                      <div>
                        <div className="text-sm text-slate-700">{r.replyText}</div>
                        <div className="text-xs text-slate-400 mt-1">— {r.userName || "Team"}, {fmt(r.createdOn)}</div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>
      );
    }

    // chat mode
    return (
      <div className="space-y-3">
        {feedbacks.map((fb) => (
          <div key={fb.feedbackId} className="space-y-2">
            <div className="flex items-start gap-3">
              <div className="w-8 h-8 rounded-full bg-indigo-600 text-white flex items-center justify-center text-sm font-semibold">
                M
              </div>
              <div className="bg-white p-3 rounded-lg border shadow-sm">
                <div className="text-sm text-slate-800">{fb.feedbackText}</div>
                <div className="text-xs text-slate-400 mt-1">{fmt(fb.createdOn)} • Rating: {fb.progressRating ?? "N/A"}</div>
              </div>
            </div>

            {/* replies as right-aligned */}
            {fb.replies?.map((r) => (
              <div key={r.replyId} className="flex items-start gap-3 justify-end">
                <div className="bg-slate-50 p-3 rounded-lg border text-sm text-slate-700 w-3/4">
                  <div>{r.replyText}</div>
                  <div className="text-xs text-slate-400 mt-1">— {r.userName || "Team"}, {fmt(r.createdOn)}</div>
                </div>
                <div className="w-8 h-8 rounded-full bg-slate-200 text-slate-600 flex items-center justify-center text-xs font-semibold">
                  {initials(r.userName)}
                </div>
              </div>
            ))}
          </div>
        ))}
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-slate-100 text-slate-800">
      {/* Header */}
      <header className="sticky top-0 z-30">
        <div className="max-w-7xl mx-auto px-6 py-5" style={{ background: "linear-gradient(90deg, rgba(239,246,255,0.9), rgba(255,255,255,0.9))" }}>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <div className="w-12 h-12 rounded-lg bg-gradient-to-br from-indigo-600 to-indigo-800 flex items-center justify-center text-white font-extrabold shadow-lg">
                MD
              </div>
              <div>
                <h1 className="text-2xl md:text-3xl font-extrabold">Mentor Dashboard</h1>
                <p className="text-sm text-slate-500 -mt-1">Manage teams, feedback & submissions</p>
              </div>
            </div>

            <nav className="flex items-center gap-4">
              <button
                onClick={() => {
                  // keep any useful action here if needed later
                  toast("You are on the Mentor Dashboard");
                }}
                className="hidden md:inline-block text-sm text-slate-600 px-3 py-1 rounded-md"
              >
                {/* intentionally subtle */}
                Dashboard
              </button>

              <div className="text-sm text-slate-500">Signed in</div>
            </nav>
          </div>
        </div>
      </header>

      {/* Main */}
      <main className="max-w-7xl mx-auto px-6 py-8 space-y-8">
        {/* small stats row */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div className="rounded-2xl bg-white p-4 border shadow-sm">
            <div className="text-xs text-slate-500">Assigned teams</div>
            <div className="text-2xl font-bold mt-2">{teams.length}</div>
          </div>

          <div className="rounded-2xl bg-white p-4 border shadow-sm">
            <div className="text-xs text-slate-500">Pending feedback</div>
            <div className="text-2xl font-bold mt-2">—</div>
          </div>

          <div className="rounded-2xl bg-white p-4 border shadow-sm">
            <div className="text-xs text-slate-500">Latest activity</div>
            <div className="text-2xl font-bold mt-2">—</div>
          </div>
        </div>

        {/* Teams cards */}
        {!selectedTeam && (
          <section>
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Your Teams</h2>
              <div className="text-sm text-slate-500">Click Details or Open to proceed</div>
            </div>

            {loadingTeams ? (
              <div className="rounded-lg bg-white p-6 border shadow-sm">Loading…</div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {teams.map((t) => (
                  <article key={t.teamId} className="rounded-2xl bg-white p-5 border shadow-sm">
                    <div className="flex justify-between items-start gap-4">
                      <div>
                        <h3 className="text-lg font-semibold text-slate-900">{t.teamName}</h3>
                        <p className="text-sm text-slate-500 mt-1">{t.projectTitle || ""}</p>
                      </div>

                      <div className="text-xs text-slate-400">{/* event id hidden per request */}</div>
                    </div>

                    <p className="text-sm text-slate-500 mt-3 line-clamp-3">{t.description || ""}</p>

                    <div className="mt-4 flex gap-2">
                      <button
                        onClick={() => openTeamView(t.teamId)}
                        className="flex-1 rounded-md bg-indigo-600 text-white py-2 text-sm font-medium hover:bg-indigo-700"
                      >
                        View Details
                      </button>

                      <button
                        onClick={() => setExpandedCardId((cur) => (cur === t.teamId ? null : t.teamId))}
                        className="rounded-md border border-slate-200 px-3 py-2 text-sm text-slate-700"
                      >
                        Details
                      </button>
                    </div>

                    {/* inline details */}
                    <AnimatePresence>
                      {expandedCardId === t.teamId && (
                        <motion.div
                          initial={{ opacity: 0, height: 0 }}
                          animate={{ opacity: 1, height: "auto" }}
                          exit={{ opacity: 0, height: 0 }}
                          transition={{ duration: 0.22 }}
                          className="mt-4 p-4 bg-slate-50 border rounded-lg"
                        >
                          <div className="text-sm text-slate-600 mb-2">Project</div>
                          <div className="text-sm text-slate-800 mb-3">{t.projectTitle || "—"}</div>

                          <div className="text-sm text-slate-600 mb-2">Members</div>
                          <ul className="text-sm text-slate-700 space-y-1 mb-3">
                            {(t.members || []).slice(0, 4).map((m) => (
                              <li key={m.userId} className="flex justify-between">
                                <span>{m.name}</span>
                                <span className="text-xs text-slate-500">({m.role || "Member"})</span>
                              </li>
                            ))}
                            {(t.members || []).length > 4 && (
                              <li className="text-xs text-slate-400">and {(t.members || []).length - 4} more…</li>
                            )}
                          </ul>

                          <div className="flex gap-2">
                            <button onClick={() => openTeamView(t.teamId)} className="rounded-md bg-indigo-600 text-white px-3 py-1 text-sm">Open</button>
                            <button onClick={() => setExpandedCardId(null)} className="rounded-md border px-3 py-1 text-sm">Close</button>
                          </div>
                        </motion.div>
                      )}
                    </AnimatePresence>
                  </article>
                ))}
              </div>
            )}
          </section>
        )}

        {/* Full team view */}
        {selectedTeam && (
          <section className="space-y-6">
            <div className="flex items-start justify-between gap-6">
              <div className="flex-1 rounded-2xl bg-white p-6 border shadow-sm">
                <div className="flex items-start justify-between">
                  <div>
                    <h2 className="text-2xl font-bold">{selectedTeam.teamName}</h2>
                    <p className="text-sm text-slate-500 mt-1">{selectedTeam.projectTitle}</p>
                  </div>

                  <div className="flex items-center gap-3">
                    <button
                      onClick={() => setSelectedTeam(null)}
                      className="text-sm text-slate-600 px-3 py-1 rounded-md border"
                    >
                      ← Back
                    </button>
                  </div>
                </div>

                <p className="text-sm text-slate-600 mt-4">{selectedTeam.description}</p>

                {/* Tabs */}
                <div className="mt-6 flex gap-3">
                  {["overview", "submissions", "feedback"].map((tb) => (
                    <button
                      key={tb}
                      onClick={() => {
                        setTeamTab(tb);
                        if (tb !== "feedback") {
                          setIsGivingFeedback(false);
                        }
                      }}
                      className={`px-3 py-2 rounded-full text-sm font-semibold ${
                        teamTab === tb ? "bg-indigo-600 text-white" : "bg-slate-100 text-slate-700 hover:bg-slate-200"
                      }`}
                    >
                      {tb === "overview" ? "Overview" : tb === "submissions" ? "Submissions" : "Feedback"}
                    </button>
                  ))}
                </div>

                {/* Tab content */}
                <div className="mt-6">
                  {teamTab === "overview" && (
                    <div className="space-y-4">
                      <div>
                        <h4 className="text-sm font-semibold text-slate-700">Members</h4>
                        <ul className="mt-2 divide-y rounded-md border bg-white">
                          {(selectedTeam.members || []).map((m) => (
                            <li key={m.userId} className="flex items-center justify-between p-3">
                              <div className="flex items-center gap-3">
                                <div className="w-9 h-9 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-700 font-semibold">
                                  {initials(m.name)}
                                </div>
                                <div>
                                  <div className="text-sm font-medium text-slate-800">{m.name}</div>
                                  <div className="text-xs text-slate-500">{m.email}</div>
                                </div>
                              </div>
                              <div className="text-xs text-slate-500">{m.role || "Member"}</div>
                            </li>
                          ))}
                        </ul>
                      </div>

                      <div>
                        <h4 className="text-sm font-semibold text-slate-700">Project</h4>
                        <div className="mt-2 text-sm text-slate-600">{selectedTeam.projectTitle || "—"}</div>
                      </div>
                    </div>
                  )}

                  {teamTab === "submissions" && (
                    <div>
                      {selectedTeam.submissions?.length > 0 ? (
                        <KnowledgeCardsDisplay
                          items={selectedTeam.submissions.map((s) => ({
                            itemId: s.itemId,
                            title: s.title,
                            description: s.description,
                            tags: s.tags || [],
                            ownerName: s.submittedBy || s.ownerName,
                          }))}
                          userId={mentorId}
                        />
                      ) : (
                        <div className="text-sm text-slate-500">No submissions yet.</div>
                      )}
                    </div>
                  )}

                  {teamTab === "feedback" && (
                    <div>
                      {/* controls */}
                      <div className="flex items-center justify-between mb-4 gap-3">
                        <div className="flex items-center gap-2 text-sm text-slate-600">
                          <span>View:</span>
                          <select
                            value={feedbackViewMode}
                            onChange={(e) => setFeedbackViewMode(e.target.value)}
                            className="text-sm rounded-md border px-2 py-1"
                          >
                            <option value="timeline">Timeline</option>
                            <option value="cards">Cards</option>
                            <option value="chat">Chat</option>
                          </select>
                        </div>

                        <div>
                          <button
                            onClick={() => setIsGivingFeedback((s) => !s)}
                            className="rounded-md bg-indigo-600 text-white px-3 py-1 text-sm hover:bg-indigo-700"
                          >
                            {isGivingFeedback ? "Close" : "Add Feedback"}
                          </button>
                        </div>
                      </div>

                      {/* add feedback form hidden by default */}
                      <AnimatePresence>
                        {isGivingFeedback && (
                          <motion.div
                            initial={{ opacity: 0, height: 0 }}
                            animate={{ opacity: 1, height: "auto" }}
                            exit={{ opacity: 0, height: 0 }}
                            transition={{ duration: 0.2 }}
                            className="mb-4 p-4 bg-indigo-50 border border-indigo-100 rounded-md"
                          >
                            <textarea
                              value={feedbackText}
                              onChange={(e) => setFeedbackText(e.target.value)}
                              rows={3}
                              placeholder="Write feedback..."
                              className="w-full p-3 rounded-md border focus:ring-2 focus:ring-indigo-300"
                            />
                            <div className="mt-3 flex items-center gap-3">
                              <div className="flex items-center gap-2">
                                <label className="text-sm text-slate-600">Rating</label>
                                <input
                                  type="range"
                                  min="0"
                                  max="10"
                                  value={rating}
                                  onChange={(e) => setRating(parseInt(e.target.value))}
                                  className="w-36"
                                />
                                <div className="text-sm font-semibold">{rating}</div>
                              </div>

                              <div className="ml-auto flex items-center gap-2">
                                <button
                                  onClick={submitFeedback}
                                  disabled={isSubmitting}
                                  className={`px-4 py-2 rounded-md text-white ${isSubmitting ? "bg-indigo-400" : "bg-indigo-600 hover:bg-indigo-700"}`}
                                >
                                  {isSubmitting ? "Submitting..." : "Submit"}
                                </button>
                                <button
                                  onClick={() => {
                                    setIsGivingFeedback(false);
                                    setFeedbackText("");
                                  }}
                                  className="px-3 py-2 rounded-md border"
                                >
                                  Cancel
                                </button>
                              </div>
                            </div>
                          </motion.div>
                        )}
                      </AnimatePresence>

                      {/* feedback listing */}
                      <div>
                        <FeedbackRender feedbacks={selectedTeam.feedbacks || []} />
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* small summary rail */}
            <div className="rounded-2xl bg-white p-4 border shadow-sm">
              <div className="text-sm text-slate-600 font-semibold">Summary</div>
              <div className="mt-3 text-sm text-slate-700">
                <div className="flex justify-between">
                  <span>Members</span>
                  <span>{selectedTeam.members?.length || 0}</span>
                </div>

                <div className="flex justify-between mt-2">
                  <span>Submissions</span>
                  <span>{selectedTeam.submissions?.length || 0}</span>
                </div>

                <div className="flex justify-between mt-2">
                  <span>Feedback count</span>
                  <span>{selectedTeam.feedbacks?.length || 0}</span>
                </div>
              </div>
            </div>
          </section>
        )}
      </main>
    </div>
  );
}
