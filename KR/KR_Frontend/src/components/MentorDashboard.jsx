// import React, { useEffect, useState } from "react";
// import { motion } from "framer-motion";
// import { toast } from "react-hot-toast";
// import api from "../api";
// import KnowledgeCardsDisplay from "../components/KnowledgeCardsDisplay";

// export default function MentorDashboard() {
//   const mentorId = localStorage.getItem("userId");
//   const [groups, setGroups] = useState([]); 
//   const [selectedTeam, setSelectedTeam] = useState(null);
//   const [feedbackText, setFeedbackText] = useState("");
//   const [progressRating, setProgressRating] = useState(0);
//   const [isSubmitting, setIsSubmitting] = useState(false);
//   const [loadingGroups, setLoadingGroups] = useState(true);
//   const [activeTab, setActiveTab] = useState("teams");


//   useEffect(() => {
//     const fetchGroups = async () => {
//       try {
//         setLoadingGroups(true);
//         const res = await api.get(`/Mentor/${mentorId}/teams`);
//         setGroups(Array.isArray(res.data) ? res.data : []);
//       } catch (err) {
//         console.error(err);
//         toast.error("Failed to load teams. Please try again.");
//       } finally {
//         setLoadingGroups(false);
//       }
//     };
//     if (mentorId) fetchGroups();
//   }, [mentorId]);

//   const handleSelectTeam = async (teamId) => {
//     try {
//       const res = await api.get(`/Mentor/team/${teamId}`);
//       setSelectedTeam(res.data);
//       setActiveTab("feedbacks");
//       window.scrollTo({ top: 0, behavior: "smooth" });
//     } catch (err) {
//       console.error(err);
//       toast.error("Failed to load team details.");
//     }
//   };
//   const handleAddFeedback = async () => {
//     if (!selectedTeam || !feedbackText.trim()) {
//       toast.error("Please enter feedback before submitting.");
//       return;
//     }

//     const eventId = selectedTeam.eventId;
//     if (!eventId) {
//       toast.error("This team has no valid event assigned.");
//       return;
//     }

//     setIsSubmitting(true);
//     try {
     
//       await api.post(`/Mentor/feedback/add`, {
//         mentorId,
//         teamId: selectedTeam.teamId,
//         eventId,
//         feedbackText,
//         progressRating,
//       });
//       toast.success("Feedback added successfully!");
//       setFeedbackText("");
//       setProgressRating(0);
     
//       await handleSelectTeam(selectedTeam.teamId);
 
//       const res = await api.get(`/Mentor/${mentorId}/teams`);
//       setGroups(Array.isArray(res.data) ? res.data : []);
//     } catch (err) {
//       console.error(err);
   
//       const message = err?.response?.data?.message || "Failed to submit feedback.";
//       toast.error(message);
//     } finally {
//       setIsSubmitting(false);
//     }
//   };

//   return (
//     <div className="min-h-screen bg-gray-50 p-6 sm:p-8">
//       <motion.h1
//         className="text-3xl font-bold text-gray-800 mb-8 text-center"
//         initial={{ opacity: 0, y: -10 }}
//         animate={{ opacity: 1, y: 0 }}
//       >
//         Mentor Dashboard
//       </motion.h1>

//       {/* Tabs */}
//       <div className="flex justify-center border-b mb-6 space-x-8">
//         {["teams", "feedbacks"].map((tab) => (
//           <button
//             key={tab}
//             onClick={() => setActiveTab(tab)}
//             className={`pb-2 text-lg font-medium transition-all ${
//               activeTab === tab
//                 ? "text-blue-600 border-b-2 border-blue-600"
//                 : "text-gray-500 hover:text-blue-500"
//             }`}
//           >
//             {tab === "teams" ? "My Teams" : "Team Feedback & Submissions"}
//           </button>
//         ))}
//       </div>

//       {/* --- TEAMS TAB (groups by month) --- */}
//       {activeTab === "teams" && (
//         <div>
//           {loadingGroups ? (
//             <p className="text-gray-500 text-center mt-8">Loading your assigned teams...</p>
//           ) : groups.length === 0 ? (
//             <p className="text-gray-500 text-center mt-8">You don’t have any assigned teams yet.</p>
//           ) : (
//             <div className="space-y-6">
//               {groups.map((group) => (
//                 <section key={`${group.year}-${group.month}`} className="bg-white p-5 rounded-2xl shadow">
//                   <div className="flex items-center justify-between mb-4">
//                     <h2 className="text-xl font-semibold text-gray-800">{group.monthLabel}</h2>
//                     <div className="text-sm text-gray-500">
//                       {group.teams?.length ?? 0} team{group.teams && group.teams.length !== 1 ? "s" : ""}
//                     </div>
//                   </div>

//                   {group.teams && group.teams.length > 0 ? (
//                     <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
//                       {group.teams.map((team) => (
//                         <motion.div
//                           key={team.teamId}
//                           onClick={() => handleSelectTeam(team.teamId)}
//                           whileHover={{ scale: 1.02 }}
//                           className="p-4 bg-gray-50 rounded-xl shadow-sm hover:shadow-md cursor-pointer transition-all"
//                         >
//                           <h3 className="text-lg font-semibold text-gray-800 mb-1">{team.teamName}</h3>
//                           <p className="text-sm text-gray-600 mb-2">{team.projectTitle || "No project title"}</p>
//                           <p className="text-sm text-gray-400">{team.description || "No description available"}</p>
//                         </motion.div>
//                       ))}
//                     </div>
//                   ) : (
//                     <p className="text-gray-500 text-sm">No teams in this month.</p>
//                   )}
//                 </section>
//               ))}
//             </div>
//           )}
//         </div>
//       )}

//       {/* --- FEEDBACK & SUBMISSIONS TAB --- */}
//       {activeTab === "feedbacks" && (
//         <div>
//           {!selectedTeam ? (
//             <p className="text-gray-500 text-center mt-10">
//               Select a team from “My Teams” to view its submissions and feedback.
//             </p>
//           ) : (
//             <motion.div key={selectedTeam.teamId} initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="max-w-4xl mx-auto space-y-6">
//               {/* Team Overview */}
//               <div className="bg-white p-6 rounded-2xl shadow">
//                 <h2 className="text-2xl font-semibold text-gray-800 mb-1">{selectedTeam.teamName}</h2>
//                 <p className="text-gray-600 mb-2">Event Description: {selectedTeam.description || "N/A"}</p>
//                 <p className="text-sm text-gray-400">Event ID: {selectedTeam.eventId || "N/A"}</p>
//               </div>

//               {/* Team Members */}
//               <div className="bg-white p-6 rounded-2xl shadow">
//                 <h3 className="text-lg font-semibold mb-3 text-gray-700">Team Members</h3>
//                 {selectedTeam.members?.length ? (
//                   <ul className="divide-y divide-gray-100">
//                     {selectedTeam.members.map((m) => (
//                       <li key={m.userId} className="py-2 flex justify-between items-center">
//                         <div>
//                           <span className="font-medium">{m.name}</span>{" "}
//                           <span className="text-gray-500 text-sm">({m.role || "Member"})</span>
//                         </div>
//                         <span className="text-gray-400 text-sm">{m.email}</span>
//                       </li>
//                     ))}
//                   </ul>
//                 ) : (
//                   <p className="text-gray-500 text-sm">No members found.</p>
//                 )}
//               </div>

//               {/* Team Submissions */}
//               <div className="bg-white p-6 rounded-2xl shadow">
//                 <h3 className="text-lg font-semibold mb-3 text-gray-700">Team Submissions</h3>
//                 {selectedTeam.submissions?.length > 0 ? (
//                   <KnowledgeCardsDisplay
//                     items={selectedTeam.submissions.map((s) => ({
//                       itemId: s.itemId,
//                       title: s.title,
//                       description: s.description,
//                       tags: s.tags || [],
//                       ownerName: s.submittedBy || s.ownerName,
//                     }))}
//                     userId={mentorId}
//                   />
//                 ) : (
//                   <p className="text-gray-500 text-sm">No submissions yet.</p>
//                 )}
//               </div>

//               {/* Feedback History */}
//               <div className="bg-white p-6 rounded-2xl shadow">
//                 <h3 className="text-lg font-semibold mb-3 text-gray-700">Previous Feedback</h3>
//                 {selectedTeam.feedbacks?.length ? (
//                   <div className="space-y-4">
//                     {selectedTeam.feedbacks.map((fb) => (
//                       <div key={fb.feedbackId} className="border border-gray-200 rounded-xl p-4 bg-gray-50">
//                         <p className="text-gray-800 font-medium">{fb.feedbackText}</p>
//                         <p className="text-sm text-gray-500 mt-1">
//                           Rating: {fb.progressRating ?? "N/A"} | {new Date(fb.createdOn).toLocaleString()}
//                         </p>

//                         {fb.replies?.length > 0 && (
//                           <div className="mt-4 pl-4 border-l-4 border-blue-200 space-y-3">
//                             <h4 className="text-sm font-semibold text-blue-700">Team Replies</h4>
//                             {fb.replies.map((reply) => (
//                               <div key={reply.replyId} className="bg-white p-3 rounded-lg shadow-sm border border-gray-100">
//                                 <p className="text-gray-700">{reply.replyText}</p>
//                                 <p className="text-xs text-gray-500 mt-1">
//                                   — {reply.userName || "Team Member"}, {new Date(reply.createdOn).toLocaleString()}
//                                 </p>
//                               </div>
//                             ))}
//                           </div>
//                         )}
//                       </div>
//                     ))}
//                   </div>
//                 ) : (
//                   <p className="text-gray-500 text-sm">No feedback provided yet.</p>
//                 )}
//               </div>

//               {/* Add Feedback */}
//               <div className="bg-white p-6 rounded-2xl shadow">
//                 <h3 className="text-lg font-semibold mb-3 text-gray-700">Add New Feedback</h3>
//                 <textarea
//                   placeholder="Write your feedback..."
//                   value={feedbackText}
//                   onChange={(e) => setFeedbackText(e.target.value)}
//                   rows={3}
//                   className="w-full p-3 border border-gray-300 rounded-lg mb-4 focus:outline-none focus:ring-2 focus:ring-blue-500"
//                 />
//                 <div className="flex items-center gap-4 mb-4">
//                   <input
//                     type="number"
//                     min={0}
//                     max={10}
//                     value={progressRating}
//                     onChange={(e) => setProgressRating(Number.isNaN(parseInt(e.target.value)) ? 0 : parseInt(e.target.value))}
//                     className="w-32 p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
//                     placeholder="Rating (0–10)"
//                   />
//                   <button
//                     disabled={isSubmitting}
//                     onClick={handleAddFeedback}
//                     className={`px-6 py-2 rounded-lg text-white font-medium ${
//                       isSubmitting ? "bg-blue-400 cursor-not-allowed" : "bg-blue-600 hover:bg-blue-700"
//                     }`}
//                   >
//                     {isSubmitting ? "Submitting..." : "Submit Feedback"}
//                   </button>
//                 </div>
//               </div>
//             </motion.div>
//           )}
//         </div>
//       )}
//     </div>
//   );
// }

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

  // NEW: which sub-panel inside Feedbacks (overview | submissions | feedbacks)
  const [activeSubTab, setActiveSubTab] = useState("overview");

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
      setActiveSubTab("overview"); // reset to overview on selection
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

      // reload selected team and groups
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

      {/* --- TEAMS TAB (unchanged) --- */}
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
            <div className="space-y-6">
              {groups.map((group) => (
                <section key={`${group.year}-${group.month}`} className="bg-white p-5 rounded-2xl shadow">
                  <div className="flex items-center justify-between mb-4">
                    <h2 className="text-xl font-semibold text-gray-800">{group.monthLabel}</h2>
                    <div className="text-sm text-gray-500">
                      {group.teams?.length ?? 0} team{group.teams && group.teams.length !== 1 ? "s" : ""}
                    </div>
                  </div>

                  {group.teams && group.teams.length > 0 ? (
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                      {group.teams.map((team) => (
                        <motion.div
                          key={team.teamId}
                          onClick={() => handleSelectTeam(team.teamId)}
                          whileHover={{ scale: 1.02 }}
                          className="p-4 bg-gray-50 rounded-xl shadow-sm hover:shadow-md cursor-pointer transition-all"
                        >
                          <h3 className="text-lg font-semibold text-gray-800 mb-1">{team.teamName}</h3>
                          <p className="text-sm text-gray-400">{team.description || "No description available"}</p>
                        </motion.div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-gray-500 text-sm">No teams in this month.</p>
                  )}
                </section>
              ))}
            </div>
          )}
        </div>
      )}

      {/* --- FEEDBACK & SUBMISSIONS TAB (CHANGED) --- */}
      {activeTab === "feedbacks" && (
        <div>
          {!selectedTeam ? (
            <p className="text-gray-500 text-center mt-10">
              Select a team from “My Teams” to view its submissions and feedback.
            </p>
          ) : (
            <motion.div key={selectedTeam.teamId} initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="max-w-5xl mx-auto space-y-6">

              {/* Header */}
              <div className="bg-white p-6 rounded-2xl shadow flex items-center justify-between">
                <div>
                  <h2 className="text-2xl font-semibold text-gray-800 mb-1">{selectedTeam.teamName}</h2>
               
                </div>
                <div className="flex items-center gap-3">
                  <div className="text-sm text-gray-500">Members: <span className="font-semibold text-gray-800">{selectedTeam.members?.length ?? 0}</span></div>
                  <button onClick={() => { setSelectedTeam(null); setActiveSubTab("overview"); }} className="px-3 py-1 rounded bg-slate-100">Close</button>
                </div>
              </div>

              {/* Sub-tabs (full width) */}
              <div className="bg-white rounded-2xl shadow">
                <div className="flex gap-3 px-4 py-3 border-b">
                  {["overview", "submissions", "feedbacks"].map((st) => (
                    <button
                      key={st}
                      onClick={() => setActiveSubTab(st)}
                      className={`px-4 py-2 rounded-md text-sm font-medium transition ${
                        activeSubTab === st ? "bg-blue-600 text-white" : "bg-transparent text-gray-600 hover:bg-gray-100"
                      }`}
                    >
                      {st === "overview" ? "Overview" : st === "submissions" ? "Submissions" : "Feedback"}
                    </button>
                  ))}
                </div>

                {/* Content area - occupies full width below tabs */}
                <div className="p-6">
                  {/* Overview */}
                  {activeSubTab === "overview" && (
                    <div className="space-y-6">
                      <div className="bg-gray-50 p-4 rounded-lg">
                        <h3 className="text-lg font-semibold text-gray-800">Team Overview</h3>
                        <p className="text-gray-600 mt-2">{selectedTeam.description || "No description available."}</p>
                      </div>

                      <div className="bg-white p-4 rounded-lg shadow">
                        <h4 className="text-md font-semibold text-gray-700 mb-2">Team Members</h4>
                        {selectedTeam.members?.length ? (
                          <ul className="divide-y">
                            {selectedTeam.members.map(m => (
                              <li key={m.userId} className="py-3 flex justify-between items-center">
                                <div>
                                  <div className="font-medium text-gray-800">{m.name}</div>
                                  <div className="text-xs text-gray-500">{m.role}</div>
                                </div>
                                <div className="text-sm text-gray-500">{m.email}</div>
                              </li>
                            ))}
                          </ul>
                        ) : <div className="text-gray-500">No members found.</div>}
                      </div>
                    </div>
                  )}

                  {/* Submissions */}
                  {activeSubTab === "submissions" && (
                    <div>
                      <h3 className="text-lg font-semibold text-gray-800 mb-4">Team Submissions</h3>

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
                        <div className="text-gray-500">No submissions yet.</div>
                      )}
                    </div>
                  )}

                  {/* Feedback */}
                  {activeSubTab === "feedbacks" && (
                    <div className="space-y-6">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-800">Previous Feedback</h3>
                        <div className="mt-3 space-y-4">
                          {selectedTeam.feedbacks && selectedTeam.feedbacks.length > 0 ? (
                            selectedTeam.feedbacks.map(fb => (
                              <div key={fb.feedbackId} className="border border-gray-200 rounded-xl p-4 bg-gray-50">
                                <p className="text-gray-800 font-medium">{fb.feedbackText}</p>
                                <p className="text-sm text-gray-500 mt-1">Rating: {fb.progressRating ?? "N/A"} • {formatDate(fb.createdOn)}</p>

                                {fb.replies?.length > 0 && (
                                  <div className="mt-4 pl-4 border-l-4 border-blue-200 space-y-2">
                                    {fb.replies.map(reply => (
                                      <div key={reply.replyId} className="bg-white p-3 rounded-lg shadow-sm">
                                        <p className="text-gray-700">{reply.replyText}</p>
                                        <div className="text-xs text-gray-400 mt-1">— {reply.userName || "Team Member"}, {formatDate(reply.createdOn)}</div>
                                      </div>
                                    ))}
                                  </div>
                                )}
                              </div>
                            ))
                          ) : (
                            <div className="text-gray-500">No feedback provided yet.</div>
                          )}
                        </div>
                      </div>

                      {/* Add feedback (full-width) */}
                      <div className="bg-white p-4 rounded-lg shadow">
                        <h4 className="text-md font-semibold text-gray-700 mb-2">Add New Feedback</h4>
                        <textarea
                          placeholder="Write your feedback..."
                          value={feedbackText}
                          onChange={(e) => setFeedbackText(e.target.value)}
                          rows={4}
                          className="w-full p-3 border border-gray-300 rounded-lg mb-3 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                        <div className="flex items-center gap-4 mb-3">
                          <input
                            type="number"
                            min={0}
                            max={10}
                            value={progressRating}
                            onChange={(e) => setProgressRating(Number.isNaN(parseInt(e.target.value)) ? 0 : parseInt(e.target.value))}
                            className="w-32 p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Rating (0–10)"
                          />
                          <button
                            disabled={isSubmitting}
                            onClick={handleAddFeedback}
                            className={`px-6 py-2 rounded-lg text-white font-medium ${isSubmitting ? "bg-blue-400 cursor-not-allowed" : "bg-blue-600 hover:bg-blue-700"}`}
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