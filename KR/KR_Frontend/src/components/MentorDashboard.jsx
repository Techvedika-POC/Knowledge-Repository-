
// import React, { useEffect, useState } from "react";
// import { motion } from "framer-motion";
// import { toast } from "react-hot-toast";
// import api from "../api";
// import KnowledgeCardsDisplay from "../components/KnowledgeCardsDisplay";
// import PreviewModal from "../components/PreviewModal";

// export default function MentorDashboard() {

//   const mentorId = localStorage.getItem("userId");
//   const [groups, setGroups] = useState([]);
//   const [loading, setLoading] = useState(true);
//   const [error, setError] = useState("");
//   const [expandedTeamId, setExpandedTeamId] = useState(null);
//   const [expandedTeamDetails, setExpandedTeamDetails] = useState(null);
//   const [loadingTeamDetails, setLoadingTeamDetails] = useState(false);
//   const [activeTab, setActiveTab] = useState("submissions");
//   const [previewItem, setPreviewItem] = useState(null);

//   // Feedback / chat state
//   const [feedbacks, setFeedbacks] = useState([]);
//   const [newFeedbackText, setNewFeedbackText] = useState("");
//   const [newFeedbackRating, setNewFeedbackRating] = useState("");
//   const [chatMessages, setChatMessages] = useState([]);
//   const [chatText, setChatText] = useState("");
//   const [isPosting, setIsPosting] = useState(false);

//   useEffect(() => {
//     const fetchGroups = async () => {
//       if (!mentorId) {
//         setError("Mentor id not found. Please login.");
//         setLoading(false);
//         return;
//       }
//       try {
//         setLoading(true);
//         // This endpoint expects the token; some dev setups allow a GET /Mentor/{userId}/teams fallback.
//         const res = await api.get(`/Mentor/${mentorId}/teams`);
//         const data = res && res.data ? res.data : [];
//         setGroups(Array.isArray(data) ? data : []);
//       } catch (err) {
//         console.error("Failed to fetch mentor teams", err);
//         setError("Failed to load assigned teams.");
//         toast.error("Failed to load assigned teams. See console.");
//       } finally {
//         setLoading(false);
//       }
//     };
//     fetchGroups();
//   }, [mentorId]);

//   const handleSelectTeam = async (teamId) => {
//     if (!teamId) return;

//     // toggle collapse
//     if (expandedTeamId === teamId) {
//       setExpandedTeamId(null);
//       setExpandedTeamDetails(null);
//       setFeedbacks([]);
//       setChatMessages([]);
//       return;
//     }

//     try {
//       setLoadingTeamDetails(true);
//       const res = await api.get(`/Mentor/team/${teamId}`);
//       const data = res && res.data ? res.data : null;
//       setExpandedTeamId(teamId);
//       setExpandedTeamDetails(data);
//       setActiveTab("submissions");

//       // Preload feedbacks and chat
//       await Promise.all([loadFeedbacks(teamId), loadChat(teamId)]);

//       setTimeout(() => {
//         const el = document.getElementById(`team-details-${teamId}`);
//         if (el) el.scrollIntoView({ behavior: "smooth", block: "center" });
//       }, 80);
//     } catch (err) {
//       console.error("Failed to load team details", err);
//       toast.error("Failed to load team details. See console.");
//     } finally {
//       setLoadingTeamDetails(false);
//     }
//   };

//   const closeExpanded = () => {
//     setExpandedTeamId(null);
//     setExpandedTeamDetails(null);
//     setFeedbacks([]);
//     setChatMessages([]);
//   };

//   // --- Feedback APIs ---
//   const loadFeedbacks = async (teamId) => {
//     if (!teamId || !mentorId) return;
//     try {
//       // server determines caller from token; we send userId as dev fallback only
//       const res = await api.get(`/Communication/team/${teamId}/feedbacks`, {
//         params: { userId: mentorId },
//       });
//       const data = res && res.data ? res.data : [];
//       setFeedbacks(Array.isArray(data) ? data : []);
//     } catch (err) {
//       console.error("Failed to load feedbacks", err);
//       setFeedbacks([]);
//     }
//   };

//   const createFeedback = async () => {
//     if (!expandedTeamId) return toast.error("No team selected.");
//     if (!newFeedbackText.trim()) return toast.error("Please write feedback.");
//     setIsPosting(true);

//     try {
//       // Important: do NOT send MentorId here. Server will derive the correct mentor record
//       // from the JWT (or dev userId query fallback).
//       const payload = {
//         TeamId: expandedTeamId,
//         EventId: expandedTeamDetails?.eventId || null,
//         FeedbackText: newFeedbackText,
//         ProgressRating: newFeedbackRating ? parseInt(newFeedbackRating, 10) : null,
//       };

//       // In dev you may keep the query fallback, in prod the token should be present and used
//       const res = await api.post(`/Communication/feedback`, payload, {
//         params: { userId: mentorId },
//       });

//       toast.success("Feedback posted.");
//       setNewFeedbackText("");
//       setNewFeedbackRating("");
//       await loadFeedbacks(expandedTeamId);
//     } catch (err) {
//       console.error("Failed to create feedback", err, err?.response?.data);

//       // Better extraction of the server error message (403 returns { message: "..."} from server)
//       const serverBody = err?.response?.data;
//       let serverMsg = null;
//       if (serverBody) {
//         if (typeof serverBody === "string") serverMsg = serverBody;
//         else if (serverBody.message) serverMsg = serverBody.message;
//         else if (serverBody.Message) serverMsg = serverBody.Message;
//         else serverMsg = JSON.stringify(serverBody).slice(0, 300);
//       }

//       if (serverMsg) {
//         // If server says "Mentor not assigned..." show it
//         toast.error(`Server: ${serverMsg}`);
//       } else {
//         toast.error("Failed to create feedback. Check console/network tab.");
//       }
//     } finally {
//       setIsPosting(false);
//     }
//   };

//   // --- Chat APIs ---
//   const loadChat = async (teamId) => {
//     if (!teamId || !mentorId) return;
//     try {
//       const res = await api.get(`/Communication/team/${teamId}/chat`, { params: { userId: mentorId } });
//       const data = res && res.data ? res.data : [];
//       setChatMessages(Array.isArray(data) ? data : []);
//     } catch (err) {
//       console.error("Failed to load chat messages", err);
//       setChatMessages([]);
//     }
//   };

//   const postChatMessage = async () => {
//     if (!expandedTeamId) return toast.error("No team selected.");
//     if (!chatText.trim()) return;
//     setIsPosting(true);

//     try {
//       const payload = { MessageText: chatText, SenderName: localStorage.getItem("userName") || "Mentor" };
//       const res = await api.post(`/Communication/team/${expandedTeamId}/chat`, payload, { params: { userId: mentorId } });

//       const created = res && res.data ? res.data : null;
//       if (created) {
//         setChatMessages((prev) => [...prev, created]);
//       } else {
//         await loadChat(expandedTeamId);
//       }
//       setChatText("");
//     } catch (err) {
//       console.error("Failed to post chat message", err, err?.response?.data);
//       const serverMsg = err?.response?.data ?? null;
//       if (serverMsg) toast.error(`Server: ${String(serverMsg).slice(0, 200)}`);
//       else toast.error("Failed to send message. See console.");
//     } finally {
//       setIsPosting(false);
//     }
//   };

//   // small helper to render chat bubble
//   const ChatBubble = ({ m }) => {
//     const senderName = m.senderName || m.sender || m.sender_name || "Unknown";
//     const senderId = m.senderId || m.sender_id || m.sender;
//     const created = m.createdOn || m.created_on || m.createdOn;
//     const isOwn = senderId && String(senderId).toLowerCase() === String(mentorId).toLowerCase();

//     return (
//       <div className={`flex ${isOwn ? "justify-end" : "justify-start"} mb-3`}>
//         <div className={`${isOwn ? "bg-indigo-600 text-white" : "bg-gray-100 text-gray-900"} rounded-xl p-3 max-w-[78%]`}>
//           <div className="text-xs font-semibold">{senderName}</div>
//           <div className="mt-1 whitespace-pre-wrap">{m.messageText || m.message_text || m.message}</div>
//           <div className={`mt-2 text-[11px] ${isOwn ? "text-right" : "text-left"} text-gray-400`}>
//             <span className="text-[10px]">{created ? new Date(created).toLocaleString() : ""}</span>
//           </div>
//         </div>
//       </div>
//     );
//   };

//   if (loading) return <div className="py-12 text-center text-gray-600">Loading your assigned teams...</div>;
//   if (error) return <div className="py-12 text-center text-red-500">{error}</div>;

//   return (
//     <div className="min-h-screen bg-gradient-to-b from-white to-slate-50 p-8">
//       <motion.h1 className="text-3xl font-extrabold text-center text-gray-900 mb-8" initial={{ opacity: 0, y: -8 }} animate={{ opacity: 1, y: 0 }}>
//         Mentor Dashboard
//       </motion.h1>

//       <div className="max-w-7xl mx-auto space-y-8">
//         {groups.length === 0 ? (
//           <div className="text-center text-gray-500">You don’t have any assigned teams yet.</div>
//         ) : (
//           groups.map((grp) => (
//             <section key={`${grp.year}-${grp.month}`} className="space-y-4">
//               <div className="flex items-center justify-between">
//                 <h2 className="text-xl font-semibold text-gray-700">{grp.monthLabel}</h2>
//                 <div className="text-sm text-gray-500">{grp.teams && grp.teams.length ? grp.teams.length : 0} teams</div>
//               </div>

//               <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
//                 {(grp.teams || []).map((team) => (
//                   <div
//                     key={team.teamId}
//                     onClick={() => handleSelectTeam(team.teamId)}
//                     className={`cursor-pointer p-4 rounded-2xl shadow-sm bg-white border transition hover:shadow-lg ${expandedTeamId === team.teamId ? "ring-2 ring-indigo-100" : ""}`}
//                   >
//                     <div className="flex items-start justify-between">
//                       <div className="space-y-1">
//                         <h3 className="text-lg font-semibold text-gray-900">{team.teamName}</h3>
//                         <p className="text-sm text-gray-600 line-clamp-3">{team.description || "No description available"}</p>
//                       </div>
//                     </div>

//                     <div className="mt-4 flex items-center justify-between text-xs text-gray-500">
//                       <div>{team.members && team.members.length ? `${team.members.length} members` : "members"}</div>
//                       <div>{team.submissions && team.submissions.length ? `${team.submissions.length} submissions` : "0 submissions"}</div>
//                     </div>
//                   </div>
//                 ))}
//               </div>

//               {grp.teams && grp.teams.some((t) => t.teamId === expandedTeamId) && expandedTeamDetails && (
//                 <div id={`team-details-${expandedTeamId}`} className="mt-6 bg-white rounded-2xl shadow-lg overflow-hidden border">
//                   <div className="flex items-center justify-between p-6 bg-white relative">
//                     <div className="absolute left-0 top-0 bottom-0 w-1.5 bg-indigo-200 rounded-l" />
//                     <div className="pl-4 flex-1">
//                       <h3 className="text-2xl font-bold text-gray-900">{expandedTeamDetails.teamName}</h3>
//                       <p className="text-sm text-gray-600 mt-1 max-w-3xl">{expandedTeamDetails.description || "No description provided."}</p>
//                       <div className="text-xs text-gray-400 mt-2">Event ID: {expandedTeamDetails.eventId || "N/A"}</div>
//                     </div>
//                     <div className="flex items-center gap-3 pr-4">
//                       <button onClick={closeExpanded} className="px-3 py-1 rounded-full text-sm bg-white border text-gray-700 hover:bg-gray-50">Close</button>
//                     </div>
//                   </div>

//                   <div className="p-6 border-t bg-slate-50">
//                     <div className="flex items-center gap-3">
//                       <button onClick={() => setActiveTab("overview")} className={`px-3 py-1 rounded-full text-sm font-medium ${activeTab === "overview" ? "bg-indigo-600 text-white" : "bg-white text-gray-700 border"}`}>Overview</button>
//                       <button onClick={() => setActiveTab("submissions")} className={`px-3 py-1 rounded-full text-sm font-medium ${activeTab === "submissions" ? "bg-indigo-600 text-white" : "bg-white text-gray-700 border"}`}>Submissions</button>
//                       <button onClick={() => { setActiveTab("feedback"); loadFeedbacks(expandedTeamId); loadChat(expandedTeamId); }} className={`px-3 py-1 rounded-full text-sm font-medium ${activeTab === "feedback" ? "bg-indigo-600 text-white" : "bg-white text-gray-700 border"}`}>Feedback</button>
//                     </div>

//                     <div className="mt-6">
//                       {activeTab === "overview" && (
//                         <div className="grid grid-cols-1 lg:grid-cols-1 gap-6">
//                           <div className="bg-white p-6 rounded-xl shadow-sm border">
//                             <h4 className="text-lg font-semibold text-gray-700 mb-4">Team Members</h4>
//                             {expandedTeamDetails.members && expandedTeamDetails.members.length > 0 ? (
//                               <ul className="divide-y divide-gray-100">
//                                 {expandedTeamDetails.members.map((m) => (
//                                   <li key={m.userId} className="py-3 flex items-center justify-between">
//                                     <div>
//                                       <div className="font-medium text-gray-800">{m.name}</div>
//                                       <div className="text-sm text-gray-500">{m.role || "Member"}</div>
//                                     </div>
//                                     <div className="text-sm text-gray-500">{m.email}</div>
//                                   </li>
//                                 ))}
//                               </ul>
//                             ) : (
//                               <div className="text-sm text-gray-500">No members found.</div>
//                             )}
//                           </div>
//                         </div>
//                       )}

//                       {activeTab === "submissions" && (
//                         <div className="grid grid-cols-1 lg:grid-cols-1 gap-6">
//                           <div className="bg-white p-6 rounded-xl shadow-sm border">
//                             <h4 className="text-lg font-semibold text-gray-700 mb-4">Team Submissions</h4>
//                             {expandedTeamDetails.submissions && expandedTeamDetails.submissions.length > 0 ? (
//                               <KnowledgeCardsDisplay items={expandedTeamDetails.submissions.map((s) => ({ itemId: s.itemId, title: s.title, description: s.description, tags: s.tags || [], ownerName: s.ownerName || s.submittedBy, ...s }))} userId={mentorId} onPreview={(it) => setPreviewItem(it)} />
//                             ) : (
//                               <div className="text-sm text-gray-500">No submissions yet.</div>
//                             )}
//                             <div className="mt-6 flex justify-center gap-4">
//                               <button onClick={() => { const btn = document.querySelector("#kc-view-more"); if (btn) btn.click(); }} className="px-5 py-2 bg-indigo-100 text-indigo-700 rounded-full">View More</button>
//                               <button onClick={() => window.location.reload()} className="px-5 py-2 bg-slate-100 text-gray-700 rounded-full">Reset</button>
//                             </div>
//                           </div>
//                         </div>
//                       )}

//                       {activeTab === "feedback" && (
//                         <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
//                           <div className="bg-white p-6 rounded-xl shadow-sm border h-[540px] overflow-auto">
//                             <div className="flex items-center justify-between mb-4">
//                               <h4 className="text-lg font-semibold text-gray-700">Feedback Threads</h4>
//                               <div className="text-sm text-gray-500">{feedbacks.length} items</div>
//                             </div>

//                             {feedbacks.length === 0 ? (
//                               <div className="text-sm text-gray-500">No feedback posted yet.</div>
//                             ) : (
//                               feedbacks.map((fb) => (
//                                 <div key={fb.feedbackId || fb.feedback_id} className="border border-gray-100 rounded-xl p-4 mb-3 bg-gray-50">
//                                   <div className="flex justify-between items-start">
//                                     <div>
//                                       <div className="font-medium text-gray-800">{fb.mentorName || fb.mentor_name || "Mentor"}</div>
//                                       <div className="text-xs text-gray-500 mt-1">{new Date(fb.createdOn || fb.created_on).toLocaleString()}</div>
//                                     </div>
//                                     <div className="text-sm text-gray-600">Rating: {fb.progressRating ?? "N/A"}</div>
//                                   </div>
//                                   <div className="mt-3 text-gray-700 whitespace-pre-wrap">{fb.feedbackText || fb.feedback_text}</div>

//                                   {(fb.replies && fb.replies.length > 0) && (
//                                     <div className="mt-3 pl-3 border-l border-gray-200">
//                                       {fb.replies.map((r) => (
//                                         <div key={r.replyId || r.reply_id} className="mb-2">
//                                           <div className="text-sm font-semibold text-gray-700">{r.userName || r.user_name}</div>
//                                           <div className="text-xs text-gray-500">{new Date(r.createdOn || r.created_on).toLocaleString()}</div>
//                                           <div className="text-gray-700 mt-1">{r.replyText || r.reply_text}</div>
//                                         </div>
//                                       ))}
//                                     </div>
//                                   )}
//                                 </div>
//                               ))
//                             )}

//                             <div className="mt-4 border-t pt-4">
//                               <h5 className="text-sm font-semibold text-gray-700 mb-2">Post new feedback</h5>
//                               <textarea value={newFeedbackText} onChange={(e) => setNewFeedbackText(e.target.value)} rows={3} className="w-full p-3 border rounded-md mb-2" placeholder="Write feedback..." />
//                               <div className="flex items-center gap-3">
//                                 <input type="number" min="0" max="10" value={newFeedbackRating} onChange={(e) => setNewFeedbackRating(e.target.value)} className="w-28 p-2 border rounded-md" placeholder="Rating" />
//                                 <button onClick={createFeedback} disabled={isPosting} className="px-4 py-2 rounded-md bg-indigo-600 text-white">{isPosting ? "Posting..." : "Post Feedback"}</button>
//                               </div>
//                             </div>
//                           </div>

//                           <div className="bg-white p-6 rounded-xl shadow-sm border h-[540px] flex flex-col">
//                             <div className="flex items-center justify-between mb-4">
//                               <h4 className="text-lg font-semibold text-gray-700">Team Chat</h4>
//                               <button onClick={() => loadChat(expandedTeamId)} className="text-sm px-3 py-1 bg-slate-100 rounded-full">Refresh</button>
//                             </div>

//                             <div className="flex-1 overflow-auto pr-2 mb-4">
//                               {chatMessages.length === 0 ? (
//                                 <div className="text-sm text-gray-500">No messages. Start the conversation.</div>
//                               ) : (
//                                 chatMessages.map((m) => <ChatBubble key={m.messageId || m.message_id || Math.random()} m={m} />)
//                               )}
//                             </div>

//                             <div className="pt-3 border-t">
//                               <textarea value={chatText} onChange={(e) => setChatText(e.target.value)} rows={2} className="w-full p-3 border rounded-md mb-2" placeholder="Write a message..." />
//                               <div className="flex items-center justify-end gap-2">
//                                 <button onClick={() => { setChatText(""); }} className="px-4 py-2 bg-slate-100 rounded-md">Clear</button>
//                                 <button onClick={postChatMessage} disabled={isPosting} className="px-4 py-2 bg-indigo-600 text-white rounded-md">{isPosting ? "Sending..." : "Send"}</button>
//                               </div>
//                             </div>
//                           </div>
//                         </div>
//                       )}
//                     </div>
//                   </div>
//                 </div>
//               )}
//             </section>
//           ))
//         )}
//       </div>

//       {previewItem && <PreviewModal item={previewItem} onClose={() => setPreviewItem(null)} />}
//     </div>
//   );
// }
import React, { useEffect, useState } from "react";
import { motion } from "framer-motion";
import { toast } from "react-hot-toast";
import {
  FiUsers,
  FiFileText,
  FiCalendar,
  FiMessageSquare,
  FiRefreshCw,
} from "react-icons/fi";
import api from "../api";
import KnowledgeCardsDisplay from "../components/KnowledgeCardsDisplay";
import PreviewModal from "../components/PreviewModal";

export default function MentorDashboard() {
  const mentorId = localStorage.getItem("userId");

  const [groups, setGroups] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [expandedTeamId, setExpandedTeamId] = useState(null);
  const [expandedTeamDetails, setExpandedTeamDetails] = useState(null);
  const [loadingTeamDetails, setLoadingTeamDetails] = useState(false);
  const [activeTab, setActiveTab] = useState("submissions");
  const [previewItem, setPreviewItem] = useState(null);

  const [feedbacks, setFeedbacks] = useState([]);
  const [newFeedbackText, setNewFeedbackText] = useState("");
  const [newFeedbackRating, setNewFeedbackRating] = useState("");
  const [chatMessages, setChatMessages] = useState([]);
  const [chatText, setChatText] = useState("");
  const [isPosting, setIsPosting] = useState(false);

  // ---------- LOAD GROUPS ----------
  useEffect(() => {
    const fetchGroups = async () => {
      if (!mentorId) {
        setError("Mentor id not found. Please login again.");
        setLoading(false);
        return;
      }
      try {
        setLoading(true);
        const res = await api.get(`/Mentor/${mentorId}/teams`);
        const data = res?.data ?? [];
        setGroups(Array.isArray(data) ? data : []);
      } catch (err) {
        console.error("Failed to fetch mentor teams", err);
        setError("Failed to load assigned teams.");
        toast.error("Failed to load assigned teams.");
      } finally {
        setLoading(false);
      }
    };
    fetchGroups();
  }, [mentorId]);

  // ---------- TEAM SELECTION ----------
  const handleSelectTeam = async (teamId) => {
    if (!teamId) return;

    if (expandedTeamId === teamId) {
      closeExpanded();
      return;
    }

    try {
      setLoadingTeamDetails(true);
      const res = await api.get(`/Mentor/team/${teamId}`);
      const data = res?.data ?? null;

      setExpandedTeamId(teamId);
      setExpandedTeamDetails(data);
      setActiveTab("submissions");

      await Promise.all([loadFeedbacks(teamId), loadChat(teamId)]);

      setTimeout(() => {
        const el = document.getElementById(`team-details-${teamId}`);
        if (el) el.scrollIntoView({ behavior: "smooth", block: "start" });
      }, 80);
    } catch (err) {
      console.error("Failed to load team details", err);
      toast.error("Failed to load team details.");
    } finally {
      setLoadingTeamDetails(false);
    }
  };

  const closeExpanded = () => {
    setExpandedTeamId(null);
    setExpandedTeamDetails(null);
    setFeedbacks([]);
    setChatMessages([]);
  };

  // ---------- FEEDBACK ----------
  const loadFeedbacks = async (teamId) => {
    if (!teamId || !mentorId) return;
    try {
      const res = await api.get(`/Communication/team/${teamId}/feedbacks`, {
        params: { userId: mentorId },
      });
      const data = res?.data ?? [];
      setFeedbacks(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error("Failed to load feedbacks", err);
      setFeedbacks([]);
    }
  };

  const createFeedback = async () => {
    if (!expandedTeamId) return toast.error("Select a team first.");
    if (!newFeedbackText.trim()) return toast.error("Write some feedback.");
    setIsPosting(true);

    try {
      const payload = {
        TeamId: expandedTeamId,
        EventId: expandedTeamDetails?.eventId || null,
        FeedbackText: newFeedbackText,
        ProgressRating: newFeedbackRating
          ? parseInt(newFeedbackRating, 10)
          : null,
      };

      await api.post(`/Communication/feedback`, payload, {
        params: { userId: mentorId },
      });

      toast.success("Feedback added.");
      setNewFeedbackText("");
      setNewFeedbackRating("");
      await loadFeedbacks(expandedTeamId);
    } catch (err) {
      console.error("Failed to create feedback", err, err?.response?.data);
      const body = err?.response?.data;
      let msg = null;
      if (body) {
        if (typeof body === "string") msg = body;
        else if (body.message) msg = body.message;
        else if (body.Message) msg = body.Message;
      }
      toast.error(msg || "Failed to create feedback.");
    } finally {
      setIsPosting(false);
    }
  };

  // ---------- CHAT ----------
  const loadChat = async (teamId) => {
    if (!teamId || !mentorId) return;
    try {
      const res = await api.get(`/Communication/team/${teamId}/chat`, {
        params: { userId: mentorId },
      });
      const data = res?.data ?? [];
      setChatMessages(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error("Failed to load chat", err);
      setChatMessages([]);
    }
  };

  const postChatMessage = async () => {
    if (!expandedTeamId) return toast.error("Select a team first.");
    if (!chatText.trim()) return;
    setIsPosting(true);

    try {
      const payload = {
        MessageText: chatText,
        SenderName: localStorage.getItem("userName") || "Mentor",
      };
      const res = await api.post(
        `/Communication/team/${expandedTeamId}/chat`,
        payload,
        { params: { userId: mentorId } }
      );

      const created = res?.data ?? null;
      if (created) setChatMessages((prev) => [...prev, created]);
      else await loadChat(expandedTeamId);

      setChatText("");
    } catch (err) {
      console.error("Failed to send message", err, err?.response?.data);
      toast.error("Failed to send message.");
    } finally {
      setIsPosting(false);
    }
  };

  const ChatBubble = ({ m }) => {
    const senderName = m.senderName || m.sender || m.sender_name || "Unknown";
    const senderId = m.senderId || m.sender_id || m.sender;
    const created = m.createdOn || m.created_on || m.createdAt;
    const isOwn =
      senderId &&
      String(senderId).toLowerCase() === String(mentorId).toLowerCase();

    return (
      <div className={`flex ${isOwn ? "justify-end" : "justify-start"} mb-3`}>
        <div
          className={`${isOwn ? "bg-indigo-600 text-white" : "bg-slate-100 text-slate-900"
            } rounded-2xl px-4 py-3 max-w-[78%] shadow-sm`}
        >
          <div className="text-xs font-semibold opacity-90">{senderName}</div>
          <div className="mt-1 text-sm whitespace-pre-wrap">
            {m.messageText || m.message_text || m.message}
          </div>
          <div
            className={`mt-2 text-[11px] ${isOwn ? "text-indigo-100 text-right" : "text-slate-400 text-left"
              }`}
          >
            {created ? new Date(created).toLocaleString() : ""}
          </div>
        </div>
      </div>
    );
  };

  // ---------- HEADER STATS ----------
  const totalTeams = groups.reduce(
    (acc, g) => acc + (g.teams?.length || 0),
    0
  );
  const totalSubmissions = groups.reduce(
    (acc, g) =>
      acc +
      (g.teams || []).reduce(
        (tAcc, t) => tAcc + (t.submissions?.length || 0),
        0
      ),
    0
  );
  const totalMonths = groups.length;

  if (loading)
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#f9fafe]">
        <p className="text-slate-500 text-sm">Loading mentor dashboard…</p>
      </div>
    );

  if (error)
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#f9fafe]">
        <p className="text-red-500 text-sm">{error}</p>
      </div>
    );

  return (
    <div className="min-h-screen bg-[#f9fafe] px-8 py-6">
      <motion.div
        className="max-w-10xl mx-auto mb-8"
        initial={{ opacity: 0, y: -8 }}
        animate={{ opacity: 1, y: 0 }}
      >
        <div
          className="
      rounded-3xl 
      border border-slate-200 
      shadow-sm 
      px-8 py-6 
      flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6
      bg-gradient-to-r 
      from-[#f5f3ff] 
      via-[#eef5ff] 
      to-[#e7fcff]
    "
        >
          <div className="flex-1">
            <div className="flex items-center gap-5">
              <div
                className="
            w-12 h-12 
            rounded-2xl 
            bg-white/80 
            shadow-md 
            flex items-center justify-center
            border border-slate-200
          "
              >
                <FiUsers className="w-6 h-6 text-indigo-600" />
              </div>

              {/* Title + workspace label */}
              <div className="flex flex-col">
                <p className="text-[11px] uppercase tracking-[0.12em] font-semibold text-indigo-500">
                  Mentor Workspace
                </p>

                <h1 className="text-2xl font-extrabold text-blue-600 leading-tight">
                  Mentor Dashboard
                </h1>
              </div>
            </div>

            {/* Inline monitoring labels (inside header, not below) */}
            <div className="mt-3 flex flex-wrap gap-2">
              <span className="px-3 py-1 rounded-full bg-white/80 text-[11px] font-medium text-purple-600 border border-purple-300">
                Monitor teams
              </span>
              <span className="px-3 py-1 rounded-full bg-white/80 text-[11px] font-medium text-purple-600 border border-purple-300">
                Track submissions
              </span>
              <span className="px-3 py-1 rounded-full bg-white/80 text-[11px] font-medium text-purple-600 border border-purple-300">
                Share feedback & chat
              </span>
            </div>

          </div>

          {/* Right Stats Section */}
          <div className="grid grid-cols-3 gap-3 w-full lg:w-auto">
            <div
              className="
          rounded-2xl 
          bg-white/90 
          backdrop-blur-sm 
          border border-slate-200 
          px-4 py-3 
          shadow-sm 
        "
            >
              <div className="flex items-center gap-2 text-[11px] font-semibold text-slate-500 uppercase tracking-wide">
                <FiCalendar className="w-4 h-4 text-sky-500" />
                Months
              </div>
              <div className="mt-1 text-xl font-bold text-slate-800">
                {totalMonths}
              </div>
            </div>

            <div
              className="
          rounded-2xl 
          bg-white/90 
          backdrop-blur-sm 
          border border-slate-200 
          px-4 py-3 
          shadow-sm 
        "
            >
              <div className="flex items-center gap-2 text-[11px] font-semibold text-slate-500 uppercase tracking-wide">
                <FiUsers className="w-4 h-4 text-indigo-500" />
                Teams
              </div>
              <div className="mt-1 text-xl font-bold text-slate-800">
                {totalTeams}
              </div>
            </div>

            <div
              className="
          rounded-2xl 
          bg-white/90 
          backdrop-blur-sm 
          border border-slate-200
          px-4 py-3 
          shadow-sm
        "
            >
              <div className="flex items-center gap-2 text-[11px] font-semibold text-slate-500 uppercase tracking-wide">
                <FiFileText className="w-4 h-4 text-purple-500" />
                Submissions
              </div>
              <div className="mt-1 text-xl font-bold text-slate-800">
                {totalSubmissions}
              </div>
            </div>
          </div>
        </div>
      </motion.div>




      <div className="max-w-7xl mx-auto space-y-10">
        {groups.length === 0 ? (
          <div className="text-center text-slate-500 text-sm bg-white rounded-2xl py-10 shadow-sm border border-slate-100">
            No teams assigned yet.
          </div>
        ) : (
          groups.map((grp, idx) => (
            <motion.section
              key={`${grp.year}-${grp.month}`}
              className="space-y-4"
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: idx * 0.04 }}
            >
              {/* month header */}
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="flex items-center gap-2">
                    <span className="w-2 h-2 rounded-full bg-indigo-500" />
                    <h2 className="text-lg font-semibold text-slate-800">
                      {grp.monthLabel}
                    </h2>
                  </div>
                  <span className="text-[11px] px-2 py-0.5 rounded-full bg-slate-100 text-slate-500 border border-slate-200">
                    {grp.year}
                  </span>
                </div>
                <p className="text-xs text-slate-500">
                  {(grp.teams && grp.teams.length) || 0} teams
                </p>
              </div>



              {/* team cards – same vibe as header, light coloured background */}
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                {(grp.teams || []).map((team) => {
                  const isSelected = expandedTeamId === team.teamId;
                  const submissionsCount = team.submissions?.length || 0;
                  const membersCount = team.members?.length || 0;

                  return (
                    <button
                      key={team.teamId}
                      type="button"
                      onClick={() => handleSelectTeam(team.teamId)}
                      className={`
          group
          text-left cursor-pointer
          rounded-2xl
          p-4
          border
          shadow-sm
          transition-all
          hover:shadow-md
          focus:outline-none focus:ring-2 focus:ring-indigo-200
          bg-gradient-to-br
          from-[#f7f4ff] via-[#f8fbff] to-[#f5fff9]
          ${isSelected ? "border-indigo-400" : "border-slate-200 hover:border-indigo-200"}
        `}
                    >

                      {/* header: title + submissions pill */}
                      <div className="flex items-start justify-between mb-2">
                        <h3 className="text-sm font-semibold text-slate-900 group-hover:text-indigo-700">
                          {team.teamName}
                        </h3>

                        <span className="px-2.5 py-1 rounded-full text-[10px] font-medium bg-purple-50 text-purple-600 border border-purple-100">
                          {submissionsCount} submissions
                        </span>
                      </div>

                      {/* description */}
                      <p className="text-xs text-slate-600 line-clamp-3">
                        {team.description || "No description available."}
                      </p>

                      {/* footer meta */}
                      <div className="mt-3 flex items-center justify-between">
                        <span className="inline-flex items-center gap-2 text-[11px] text-emerald-700 bg-emerald-50 px-2.5 py-1 rounded-full">
                          <span className="w-1.5 h-1.5 rounded-full bg-emerald-400" />
                          {membersCount ? `${membersCount} members` : "Members"}
                        </span>


                      </div>
                    </button>
                  );
                })}
              </div>
              {/* expanded card */}
              {grp.teams?.some((t) => t.teamId === expandedTeamId) &&
                expandedTeamDetails && (
                  <motion.div
                    id={`team-details-${expandedTeamId}`}
                    className="mt-5 rounded-2xl bg-white border border-slate-200 shadow-sm overflow-hidden"
                    initial={{ opacity: 0, y: 6 }}
                    animate={{ opacity: 1, y: 0 }}
                  >
                    {/* header */}
                    <div className="flex items-start justify-between px-6 py-4 border-b border-slate-100">
                      <div className="flex-1">
                        <div className="mb-2 flex items-center gap-2">
                          <span className="text-[11px] px-2 py-1 rounded-full bg-indigo-50 text-indigo-700 font-medium">
                            Mentored team
                          </span>
                          {loadingTeamDetails && (
                            <span className="text-[11px] text-slate-400">
                              Refreshing…
                            </span>
                          )}
                        </div>
                        <h3 className="text-lg font-semibold text-slate-900">
                          {expandedTeamDetails.teamName}
                        </h3>
                        <p className="mt-1 text-sm text-slate-600 max-w-3xl">
                          {expandedTeamDetails.description ||
                            "No description provided."}
                        </p>
                        <div className="mt-2 text-[11px] text-slate-500 flex flex-wrap gap-4">
                          
                          <span>
                            Members:{" "}
                            <span className="font-medium text-slate-700">
                              {expandedTeamDetails.members?.length || 0}
                            </span>
                          </span>
                          <span>
                            Submissions:{" "}
                            <span className="font-medium text-slate-700">
                              {expandedTeamDetails.submissions?.length || 0}
                            </span>
                          </span>
                        </div>
                      </div>
                      <button
                        onClick={closeExpanded}
                        className="ml-4 text-xs px-3 py-1.5 rounded-full bg-slate-50 text-slate-700 border border-slate-200 hover:bg-slate-100"
                      >
                        Close
                      </button>
                    </div>

                    {/* tabs */}
                    <div className="px-6 pt-3 pb-1 bg-slate-50 border-b border-slate-100">
                      <div className="inline-flex bg-white rounded-full p-1 border border-slate-100">
                        {["overview", "submissions", "feedback"].map((tab) => (
                          <button
                            key={tab}
                            onClick={() => {
                              setActiveTab(tab);
                              if (tab === "feedback") {
                                loadFeedbacks(expandedTeamId);
                                loadChat(expandedTeamId);
                              }
                            }}
                            className={`px-4 py-1.5 text-xs font-medium rounded-full transition ${activeTab === tab
                                ? "bg-indigo-600 text-white"
                                : "text-slate-600 hover:bg-slate-50"
                              }`}
                          >
                            {tab === "overview" && "Overview"}
                            {tab === "submissions" && "Submissions"}
                            {tab === "feedback" && (
                              <span className="inline-flex items-center gap-1">
                                <FiMessageSquare className="w-3.5 h-3.5" />
                                Feedback & chat
                              </span>
                            )}
                          </button>
                        ))}
                      </div>
                    </div>

                    {/* tab content */}
                    <div className="p-6 bg-slate-50">
                      {/* overview */}
                      {activeTab === "overview" && (
                        <div className="bg-white rounded-xl border border-slate-100 shadow-sm p-6">
                          <h4 className="text-sm font-semibold text-slate-800 mb-4">
                            Team members
                          </h4>
                          {expandedTeamDetails.members?.length ? (
                            <ul className="divide-y divide-slate-100">
                              {expandedTeamDetails.members.map((m) => (
                                <li
                                  key={m.userId}
                                  className="py-3 flex items-center justify-between text-sm"
                                >
                                  <div>
                                    <p className="font-medium text-slate-900">
                                      {m.name}
                                    </p>
                                    <p className="text-xs text-slate-500">
                                      {m.role || "Member"}
                                    </p>
                                  </div>
                                  <p className="text-xs text-slate-500">
                                    {m.email}
                                  </p>
                                </li>
                              ))}
                            </ul>
                          ) : (
                            <p className="text-xs text-slate-500">
                              No members found.
                            </p>
                          )}
                        </div>
                      )}

                      {/* submissions */}
                      {activeTab === "submissions" && (
                        <div className="bg-white rounded-xl border border-slate-100 shadow-sm p-6">
                          <div className="flex items-center justify-between mb-4">
                            <h4 className="text-sm font-semibold text-slate-800">
                              Team submissions
                            </h4>
                            <p className="text-xs text-slate-500">
                              {expandedTeamDetails.submissions?.length || 0}{" "}
                              items
                            </p>
                          </div>
                          {expandedTeamDetails.submissions?.length ? (
                            <KnowledgeCardsDisplay
                              items={expandedTeamDetails.submissions.map(
                                (s) => ({
                                  itemId: s.itemId,
                                  title: s.title,
                                  description: s.description,
                                  tags: s.tags || [],
                                  ownerName: s.ownerName || s.submittedBy,
                                  ...s,
                                })
                              )}
                              userId={mentorId}
                              onPreview={(it) => setPreviewItem(it)}
                            />
                          ) : (
                            <p className="text-xs text-slate-500">
                              No submissions yet.
                            </p>
                          )}
                        </div>
                      )}

                      {/* feedback + chat */}
                      {activeTab === "feedback" && (
                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                          {/* feedback */}
                          <div className="bg-white rounded-xl border border-slate-100 shadow-sm p-6 h-[520px] flex flex-col">
                            <div className="flex items-center justify-between mb-4">
                              <h4 className="text-sm font-semibold text-slate-800">
                                Feedback
                              </h4>
                              <span className="text-xs text-slate-500">
                                {feedbacks.length} items
                              </span>
                            </div>

                            <div className="flex-1 overflow-auto pr-1">
                              {feedbacks.length === 0 ? (
                                <p className="text-xs text-slate-500">
                                  No feedback yet.
                                </p>
                              ) : (
                                feedbacks.map((fb) => (
                                  <div
                                    key={fb.feedbackId || fb.feedback_id}
                                    className="border border-slate-100 rounded-lg p-4 mb-3 bg-slate-50"
                                  >
                                    <div className="flex justify-between items-start gap-2">
                                      <div>
                                        <p className="text-sm font-medium text-slate-900">
                                          {fb.mentorName ||
                                            fb.mentor_name ||
                                            "Mentor"}
                                        </p>
                                        <p className="text-[11px] text-slate-500 mt-1">
                                          {new Date(
                                            fb.createdOn || fb.created_on
                                          ).toLocaleString()}
                                        </p>
                                      </div>
                                      <p className="text-xs text-slate-600">
                                        Rating: {fb.progressRating ?? "N/A"}
                                      </p>
                                    </div>
                                    <p className="mt-3 text-sm text-slate-800 whitespace-pre-wrap">
                                      {fb.feedbackText || fb.feedback_text}
                                    </p>

                                    {fb.replies?.length > 0 && (
                                      <div className="mt-3 pl-3 border-l border-slate-200">
                                        {fb.replies.map((r) => (
                                          <div
                                            key={r.replyId || r.reply_id}
                                            className="mb-2"
                                          >
                                            <p className="text-xs font-semibold text-slate-800">
                                              {r.userName || r.user_name}
                                            </p>
                                            <p className="text-[11px] text-slate-500">
                                              {new Date(
                                                r.createdOn || r.created_on
                                              ).toLocaleString()}
                                            </p>
                                            <p className="text-sm text-slate-800 mt-1">
                                              {r.replyText || r.reply_text}
                                            </p>
                                          </div>
                                        ))}
                                      </div>
                                    )}
                                  </div>
                                ))
                              )}
                            </div>

                            <div className="mt-4 pt-4 border-t border-slate-100">
                              <h5 className="text-xs font-semibold text-slate-700 mb-2">
                                Add feedback
                              </h5>
                              <textarea
                                value={newFeedbackText}
                                onChange={(e) =>
                                  setNewFeedbackText(e.target.value)
                                }
                                rows={3}
                                className="w-full p-3 border border-slate-200 rounded-lg text-sm mb-2 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                                placeholder="Share feedback with the team…"
                              />
                              <div className="flex items-center gap-3">
                                <input
                                  type="number"
                                  min="0"
                                  max="10"
                                  value={newFeedbackRating}
                                  onChange={(e) =>
                                    setNewFeedbackRating(e.target.value)
                                  }
                                  className="w-24 p-2 border border-slate-200 rounded-lg text-sm focus:outline-none focus:ring-1 focus:ring-indigo-500"
                                  placeholder="Rating"
                                />
                                <button
                                  onClick={createFeedback}
                                  disabled={isPosting}
                                  className="px-4 py-2 rounded-lg text-xs font-medium bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-60"
                                >
                                  {isPosting ? "Posting…" : "Post feedback"}
                                </button>
                              </div>
                            </div>
                          </div>

                          {/* chat */}
                          <div className="bg-white rounded-xl border border-slate-100 shadow-sm p-6 h-[520px] flex flex-col">
                            <div className="flex items-center justify-between mb-4">
                              <h4 className="text-sm font-semibold text-slate-800">
                                Team chat
                              </h4>
                              <button
                                onClick={() => loadChat(expandedTeamId)}
                                className="flex items-center gap-1 text-[11px] px-3 py-1 rounded-full bg-slate-50 border border-slate-200 text-slate-600 hover:bg-slate-100"
                              >
                                <FiRefreshCw className="w-3.5 h-3.5" />
                                Refresh
                              </button>
                            </div>

                            <div className="flex-1 overflow-auto pr-1 mb-4">
                              {chatMessages.length === 0 ? (
                                <p className="text-xs text-slate-500">
                                  No messages yet. Start the conversation.
                                </p>
                              ) : (
                                chatMessages.map((m) => (
                                  <ChatBubble
                                    key={
                                      m.messageId ||
                                      m.message_id ||
                                      Math.random()
                                    }
                                    m={m}
                                  />
                                ))
                              )}
                            </div>

                            <div className="pt-3 border-t border-slate-100">
                              <textarea
                                value={chatText}
                                onChange={(e) => setChatText(e.target.value)}
                                rows={2}
                                className="w-full p-3 border border-slate-200 rounded-lg text-sm mb-2 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                                placeholder="Write a message…"
                              />
                              <div className="flex items-center justify-end gap-2">
                                <button
                                  onClick={() => setChatText("")}
                                  className="px-4 py-2 text-xs font-medium bg-slate-50 border border-slate-200 rounded-lg hover:bg-slate-100"
                                >
                                  Clear
                                </button>
                                <button
                                  onClick={postChatMessage}
                                  disabled={isPosting}
                                  className="px-4 py-2 text-xs font-medium bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-60"
                                >
                                  {isPosting ? "Sending…" : "Send"}
                                </button>
                              </div>
                            </div>
                          </div>
                        </div>
                      )}
                    </div>
                  </motion.div>
                )}
            </motion.section>
          ))
        )}
      </div>

      {previewItem && (
        <PreviewModal item={previewItem} onClose={() => setPreviewItem(null)} />
      )}
    </div>
  );
}

/* small header stat chip */
function HeaderStat({ icon, label, value }) {
  return (
    <div className="flex-1 rounded-xl bg-slate-50 px-4 py-3 border border-slate-200 flex flex-col items-start justify-center">
      <div className="flex items-center gap-2 text-[11px] font-semibold text-slate-500 uppercase tracking-wide">
        {icon}
        {label}
      </div>
      <div className="mt-1 text-lg font-bold text-slate-900">{value}</div>
    </div>
  );
}

