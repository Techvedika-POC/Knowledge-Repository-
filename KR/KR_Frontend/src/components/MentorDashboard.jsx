import React, { useEffect, useState } from "react";
import { motion } from "framer-motion";
import { toast } from "react-hot-toast";
import api from "../api";
import KnowledgeCardsDisplay from "../components/KnowledgeCardsDisplay";

export default function MentorDashboard() {
  const mentorId = localStorage.getItem("userId");
  const [teams, setTeams] = useState([]);
  const [selectedTeam, setSelectedTeam] = useState(null);
  const [feedbackText, setFeedbackText] = useState("");
  const [progressRating, setProgressRating] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [loadingTeams, setLoadingTeams] = useState(true);
  const [activeTab, setActiveTab] = useState("teams");

  // Fetch mentor’s assigned teams
  useEffect(() => {
    const fetchTeams = async () => {
      try {
        const res = await api.get(`/Mentor/${mentorId}/teams`);
        setTeams(res.data);
      } catch (err) {
        console.error(err);
        toast.error("Failed to load teams. Please try again.");
      } finally {
        setLoadingTeams(false);
      }
    };
    if (mentorId) fetchTeams();
  }, [mentorId]);

  // Fetch specific team details including submissions
  const handleSelectTeam = async (teamId) => {
    try {
      const res = await api.get(`/Mentor/team/${teamId}`);
      setSelectedTeam(res.data);
      setActiveTab("feedbacks");
    } catch (err) {
      console.error(err);
      toast.error("Failed to load team details.");
    }
  };

  // Submit new feedback
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
      handleSelectTeam(selectedTeam.teamId); // refresh team details
    } catch (err) {
      console.error(err);
      toast.error("Failed to submit feedback.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 p-6 sm:p-8">
      <motion.h1
        className="text-3xl font-bold text-gray-800 mb-8 text-center"
        initial={{ opacity: 0, y: -10 }}
        animate={{ opacity: 1, y: 0 }}
      >
        Mentor Dashboard
      </motion.h1>

      {/* Tabs */}
      <div className="flex justify-center border-b mb-6 space-x-8">
        {["teams", "feedbacks"].map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`pb-2 text-lg font-medium transition-all ${
              activeTab === tab
                ? "text-blue-600 border-b-2 border-blue-600"
                : "text-gray-500 hover:text-blue-500"
            }`}
          >
            {tab === "teams" ? "My Teams" : "Team Feedback & Submissions"}
          </button>
        ))}
      </div>

      {/* --- TEAMS TAB --- */}
      {activeTab === "teams" && (
        <div>
          {loadingTeams ? (
            <p className="text-gray-500 text-center mt-8">
              Loading your assigned teams...
            </p>
          ) : teams.length === 0 ? (
            <p className="text-gray-500 text-center mt-8">
              You don’t have any assigned teams yet.
            </p>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
              {teams.map((team) => (
                <motion.div
                  key={team.teamId}
                  onClick={() => handleSelectTeam(team.teamId)}
                  whileHover={{ scale: 1.02 }}
                  className="p-5 bg-white rounded-2xl shadow hover:shadow-lg cursor-pointer transition-all"
                >
                  <h3 className="text-xl font-semibold text-gray-800 mb-1">
                    {team.teamName}
                  </h3>
                  <p className="text-gray-600 text-sm mb-2">
                    {team.projectTitle || "No project title"}
                  </p>
                  <p className="text-sm text-gray-400">
                    Event: {team.description || "No description available"}
                  </p>
                </motion.div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* --- FEEDBACK & SUBMISSIONS TAB --- */}
      {activeTab === "feedbacks" && (
        <div>
          {!selectedTeam ? (
            <p className="text-gray-500 text-center mt-10">
              Select a team from “My Teams” to view its submissions and feedback.
            </p>
          ) : (
            <motion.div
              key={selectedTeam.teamId}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className="max-w-4xl mx-auto space-y-6"
            >
              {/* Team Overview */}
              <div className="bg-white p-6 rounded-2xl shadow">
                <h2 className="text-2xl font-semibold text-gray-800 mb-1">
                  {selectedTeam.teamName}
                </h2>
                <p className="text-gray-600 mb-2">
                  Event Description: {selectedTeam.description || "N/A"}
                </p>
                <p className="text-sm text-gray-400">
                  Event ID: {selectedTeam.eventId || "N/A"}
                </p>
              </div>

              {/* Team Members */}
              <div className="bg-white p-6 rounded-2xl shadow">
                <h3 className="text-lg font-semibold mb-3 text-gray-700">
                  Team Members
                </h3>
                {selectedTeam.members?.length ? (
                  <ul className="divide-y divide-gray-100">
                    {selectedTeam.members.map((m) => (
                      <li
                        key={m.userId}
                        className="py-2 flex justify-between items-center"
                      >
                        <div>
                          <span className="font-medium">{m.name}</span>{" "}
                          <span className="text-gray-500 text-sm">
                            ({m.role || "Member"})
                          </span>
                        </div>
                        <span className="text-gray-400 text-sm">{m.email}</span>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="text-gray-500 text-sm">No members found.</p>
                )}
              </div>

              {/* Team Submissions */}
              <div className="bg-white p-6 rounded-2xl shadow">
                <h3 className="text-lg font-semibold mb-3 text-gray-700">
                  Team Submissions
                </h3>
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
                  <p className="text-gray-500 text-sm">No submissions yet.</p>
                )}
              </div>

              {/* Feedback History */}
             {/* Feedback History */}
<div className="bg-white p-6 rounded-2xl shadow">
  <h3 className="text-lg font-semibold mb-3 text-gray-700">
    Previous Feedback
  </h3>

  {selectedTeam.feedbacks?.length ? (
    <div className="space-y-4">
      {selectedTeam.feedbacks.map((fb) => (
        <div
          key={fb.feedbackId}
          className="border border-gray-200 rounded-xl p-4 bg-gray-50"
        >
          {/* Main Feedback */}
          <p className="text-gray-800 font-medium">{fb.feedbackText}</p>

          <p className="text-sm text-gray-500 mt-1">
            Rating: {fb.progressRating ?? "N/A"} |{" "}
            {new Date(fb.createdOn).toLocaleString()}
          </p>

          {/* Replies Section */}
          {fb.replies?.length > 0 && (
            <div className="mt-4 pl-4 border-l-4 border-blue-200 space-y-3">
              <h4 className="text-sm font-semibold text-blue-700">
                Team Replies
              </h4>

              {fb.replies.map((reply) => (
                <div
                  key={reply.replyId}
                  className="bg-white p-3 rounded-lg shadow-sm border border-gray-100"
                >
                  <p className="text-gray-700">{reply.replyText}</p>
                  <p className="text-xs text-gray-500 mt-1">
                    — {reply.userName || "Team Member"},{" "}
                    {new Date(reply.createdOn).toLocaleString()}
                  </p>
                </div>
              ))}
            </div>
          )}
        </div>
      ))}
    </div>
  ) : (
    <p className="text-gray-500 text-sm">No feedback provided yet.</p>
  )}
</div>


              {/* Add Feedback */}
              <div className="bg-white p-6 rounded-2xl shadow">
                <h3 className="text-lg font-semibold mb-3 text-gray-700">
                  Add New Feedback
                </h3>
                <textarea
                  placeholder="Write your feedback..."
                  value={feedbackText}
                  onChange={(e) => setFeedbackText(e.target.value)}
                  rows={3}
                  className="w-full p-3 border border-gray-300 rounded-lg mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <div className="flex items-center gap-4 mb-4">
                  <input
                    type="number"
                    min={0}
                    max={10}
                    value={progressRating}
                    onChange={(e) =>
                      setProgressRating(parseInt(e.target.value) || 0)
                    }
                    className="w-32 p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="Rating (0–10)"
                  />
                  <button
                    disabled={isSubmitting}
                    onClick={handleAddFeedback}
                    className={`px-6 py-2 rounded-lg text-white font-medium ${
                      isSubmitting
                        ? "bg-blue-400 cursor-not-allowed"
                        : "bg-blue-600 hover:bg-blue-700"
                    }`}
                  >
                    {isSubmitting ? "Submitting..." : "Submit Feedback"}
                  </button>
                </div>
              </div>
            </motion.div>
          )}
        </div>
      )}
    </div>
  );
}
