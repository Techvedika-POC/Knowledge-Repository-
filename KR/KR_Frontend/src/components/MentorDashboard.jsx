import React, { useEffect, useState } from "react";
import { motion } from "framer-motion";
import { toast } from "react-hot-toast";
import api from "../api";
import KnowledgeCardsDisplay from "../components/KnowledgeCardsDisplay";

export default function MentorDashboard() {
  const mentorId = localStorage.getItem("userId");
  const [groups, setGroups] = useState([]);
  const [selectedTeam, setSelectedTeam] = useState(null);
  const [feedbackText, setFeedbackText] = useState("");
  const [progressRating, setProgressRating] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [loadingGroups, setLoadingGroups] = useState(true);
  const [activeTab, setActiveTab] = useState("teams");
  const [activeSubTab, setActiveSubTab] = useState("overview");

  useEffect(() => {
    const fetchGroups = async () => {
      try {
        setLoadingGroups(true);
        const res = await api.get(`/Mentor/${mentorId}/teams`);
        setGroups(Array.isArray(res.data) ? res.data : []);
      } catch (err) {
        console.error(err);
        toast.error("Failed to load teams. Please try again.");
      } finally {
        setLoadingGroups(false);
      }
    };
    if (mentorId) fetchGroups();
  }, [mentorId]);

  const handleSelectTeam = async (teamId) => {
    try {
      const res = await api.get(`/Mentor/team/${teamId}`);
      setSelectedTeam(res.data);
      setActiveTab("feedbacks");
      setActiveSubTab("overview");
      window.scrollTo({ top: 0, behavior: "smooth" });
    } catch (err) {
      console.error(err);
      toast.error("Failed to load team details.");
    }
  };

  const handleAddFeedback = async () => {
    if (!selectedTeam || !feedbackText.trim()) {
      toast.error("Please enter feedback before submitting.");
      return;
    }
    const eventId = selectedTeam.eventId;
    if (!eventId) {
      toast.error("This team has no valid event assigned.");
      return;
    }
    setIsSubmitting(true);
    try {
      await api.post(`/Mentor/feedback/add`, {
        mentorId,
        teamId: selectedTeam.teamId,
        eventId,
        feedbackText,
        progressRating,
      });
      toast.success("Feedback added successfully!");
      setFeedbackText("");
      setProgressRating(0);
      const resTeam = await api.get(`/Mentor/team/${selectedTeam.teamId}`);
      setSelectedTeam(resTeam.data);
      const res = await api.get(`/Mentor/${mentorId}/teams`);
      setGroups(Array.isArray(res.data) ? res.data : []);
    } catch (err) {
      console.error(err);
      const message = err?.response?.data?.message || "Failed to submit feedback.";
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const formatDate = (iso) => {
    if (!iso) return "";
    try { return new Date(iso).toLocaleString(); } catch { return iso; }
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-blue-50 via-white to-blue-50 p-6 sm:p-10">
      <motion.h1
        className="text-3xl font-bold text-black mb-10 text-center"
        initial={{ opacity: 0, y: -15 }}
        animate={{ opacity: 1, y: 0 }}
      >
        Mentor Dashboard
      </motion.h1>

      {/* Main Tabs */}
      <div className="flex justify-center border-b mb-8 space-x-8">
        {["teams", "feedbacks"].map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`pb-2 text-lg font-semibold transition-all ${
              activeTab === tab
                ? "text-blue-600 border-b-4 border-blue-600"
                : "text-gray-400 hover:text-blue-500"
            }`}
          >
            {tab === "teams" ? "My Teams" : "Feedback & Submissions"}
          </button>
        ))}
      </div>

      {/* Teams Tab */}
      {activeTab === "teams" && (
        <div>
          {loadingGroups ? (
            <p className="text-gray-400 text-center mt-10 text-lg">Loading your assigned teams...</p>
          ) : groups.length === 0 ? (
            <p className="text-gray-400 text-center mt-10 text-lg">You don’t have any assigned teams yet.</p>
          ) : (
            <div className="space-y-8">
              {groups.map((group) => (
                <section
                  key={`${group.year}-${group.month}`}
                  className="bg-white p-6 rounded-3xl shadow-md hover:shadow-xl transition-shadow duration-300"
                >
                  <div className="flex items-center justify-between mb-4">
                    <h2 className="text-2xl font-bold text-black">{group.monthLabel}</h2>
                    <span className="text-sm text-gray-500">{group.teams?.length ?? 0} team{group.teams && group.teams.length !== 1 ? "s" : ""}</span>
                  </div>
                  {group.teams?.length > 0 ? (
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                      {group.teams.map((team) => (
                        <motion.div
                          key={team.teamId}
                          onClick={() => handleSelectTeam(team.teamId)}
                          whileHover={{ scale: 1.03 }}
                          className="p-5 bg-gradient-to-tr from-blue-50 to-white rounded-2xl shadow hover:shadow-lg cursor-pointer transition-all"
                        >
                          <h3 className="text-xl font-semibold text-black mb-1">{team.teamName}</h3>
                          <p className="text-gray-500">{team.description || "No description available"}</p>
                        </motion.div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-gray-400 text-sm">No teams in this month.</p>
                  )}
                </section>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Feedback & Submissions Tab */}
      {activeTab === "feedbacks" && (
        <div>
          {!selectedTeam ? (
            <p className="text-gray-500 text-center mt-10 text-lg">
              Select a team from “My Teams” to view its submissions and feedback.
            </p>
          ) : (
            <motion.div
              key={selectedTeam.teamId}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className="max-w-6xl mx-auto space-y-6"
            >
              {/* Team Header */}
              <div className="bg-white p-6 rounded-3xl shadow-md flex items-center justify-between border-l-4 border-blue-300">
                <div>
                  <h2 className="text-3xl  text-black">{selectedTeam.teamName}</h2>
                  <p className="text-gray-600 mt-1">{selectedTeam.description || "No description available."}</p>
                </div>
                <button
                  onClick={() => { setSelectedTeam(null); setActiveSubTab("overview"); }}
                  className="px-4 py-2 rounded-xl bg-blue-100 hover:bg-blue-200 text-blue-700 font-semibold"
                >
                  Close
                </button>
              </div>

              {/* Sub-Tabs */}
              <div className="bg-white rounded-3xl shadow-md">
                <div className="flex gap-3 px-5 py-3 border-b">
                  {["overview", "submissions", "feedbacks"].map((st) => (
                    <button
                      key={st}
                      onClick={() => setActiveSubTab(st)}
                      className={`px-5 py-2 rounded-xl font-medium text-sm transition ${
                        activeSubTab === st
                          ? "bg-blue-500 text-white shadow-md"
                          : "bg-blue-50 text-blue-700 hover:bg-blue-100"
                      }`}
                    >
                      {st === "overview" ? "Overview" : st === "submissions" ? "Submissions" : "Feedback"}
                    </button>
                  ))}
                </div>

                <div className="p-6 space-y-6">
                  {/* Overview */}
                  {activeSubTab === "overview" && (
                    <div className="space-y-4">
                      <div className="bg-blue-50 p-5 rounded-2xl shadow-sm">
                        <h3 className="text-xl font-semibold text-black mb-2">Team Members</h3>
                        {selectedTeam.members?.length ? (
                          <ul className="divide-y divide-blue-100">
                            {selectedTeam.members.map(m => (
                              <li key={m.userId} className="py-3 flex justify-between items-center">
                                <div>
                                  <div className="font-medium text-black">{m.name}</div>
                                  <div className="text-xs text-blue-700">{m.role}</div>
                                </div>
                                <div className="text-sm text-blue-700">{m.email}</div>
                              </li>
                            ))}
                          </ul>
                        ) : (
                          <p className="text-blue-400">No members found.</p>
                        )}
                      </div>
                    </div>
                  )}

                  {/* Submissions */}
                  {activeSubTab === "submissions" && (
                    <div>
                      <h3 className="text-xl font-semibold text-black mb-4">Team Submissions</h3>
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
                        <p className="text-blue-400">No submissions yet.</p>
                      )}
                    </div>
                  )}

                  {/* Feedback */}
                  {activeSubTab === "feedbacks" && (
                    <div className="space-y-6">
                      <div>
                        <h3 className="text-xl font-semibold text-black mb-3">Previous Feedback</h3>
                        {selectedTeam.feedbacks?.length > 0 ? (
                          <div className="space-y-4">
                            {selectedTeam.feedbacks.map(fb => (
                              <div key={fb.feedbackId} className="bg-blue-50 p-4 rounded-2xl shadow-sm border border-blue-100">
                                <p className="text-black font-medium">{fb.feedbackText}</p>
                                <p className="text-sm text-blue-700 mt-1">Rating: {fb.progressRating ?? "N/A"} • {formatDate(fb.createdOn)}</p>
                                {fb.replies?.length > 0 && (
                                  <div className="mt-3 pl-4 border-l-4 border-blue-200 space-y-2">
                                    {fb.replies.map(reply => (
                                      <div key={reply.replyId} className="bg-white p-3 rounded-xl shadow-sm border border-blue-50">
                                        <p className="text-black">{reply.replyText}</p>
                                        <p className="text-xs text-blue-400 mt-1">— {reply.userName || "Team Member"}, {formatDate(reply.createdOn)}</p>
                                      </div>
                                    ))}
                                  </div>
                                )}
                              </div>
                            ))}
                          </div>
                        ) : (
                          <p className="text-blue-400">No feedback provided yet.</p>
                        )}
                      </div>

                      {/* Add Feedback */}
                      <div className="bg-white p-5 rounded-2xl shadow-md">
                        <h4 className="text-lg font-semibold text-black mb-3">Add New Feedback</h4>
                        <textarea
                          placeholder="Write your feedback..."
                          value={feedbackText}
                          onChange={(e) => setFeedbackText(e.target.value)}
                          rows={4}
                          className="w-full p-3 border border-blue-200 rounded-xl mb-3 focus:outline-none focus:ring-2 focus:ring-blue-400"
                        />
                        <div className="flex items-center gap-4">
                          <input
                            type="number"
                            min={0}
                            max={10}
                            value={progressRating}
                            onChange={(e) => setProgressRating(Number.isNaN(parseInt(e.target.value)) ? 0 : parseInt(e.target.value))}
                            className="w-32 p-2 border border-blue-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-400"
                            placeholder="Rating (0–10)"
                          />
                          <button
                            disabled={isSubmitting}
                            onClick={handleAddFeedback}
                            className={`px-6 py-2 rounded-xl font-semibold text-white transition ${
                              isSubmitting ? "bg-blue-300 cursor-not-allowed" : "bg-blue-500 hover:bg-blue-600"
                            }`}
                          >
                            {isSubmitting ? "Submitting..." : "Submit Feedback"}
                          </button>
                        </div>
                      </div>
                    </div>
                  )}

                </div>
              </div>

            </motion.div>
          )}
        </div>
      )}
    </div>
  );
}
