// import React, { useEffect, useState } from "react";
// import { Calendar, X } from "lucide-react";
// import { motion } from "framer-motion";
// import api from "../api";
// import { useNavigate } from "react-router-dom";
// import { toast } from "react-hot-toast";

// export default function IdeathonPage() {
//   const [event, setEvent] = useState(null);
//   const [loading, setLoading] = useState(true);
//   const [error, setError] = useState("");
//   const [isRegistered, setIsRegistered] = useState(false);
//   const [regLoading, setRegLoading] = useState(false); // registration button loading
//   const navigate = useNavigate();

//   useEffect(() => {
//     const fetchEvent = async () => {
//       try {
//         setLoading(true);
//         // 1 Fetch Ideathon event
//         const res = await api.get("/Events/type/Ideathon");
//         const eventData = Array.isArray(res.data) && res.data.length > 0 ? res.data[0] : null;
//         setEvent(eventData);

//         // 2 Check registration for current user (only if event exists)
//         if (eventData) {
//           try {
//             const regRes = await api.get(`/EventRegistration/is-registered/${eventData.eventId}`);
//             // assume API returns { isRegistered: boolean } or boolean directly
//             const registered = regRes.data && typeof regRes.data === "object"
//               ? regRes.data.isRegistered ?? regRes.data === true
//               : Boolean(regRes.data);
//             setIsRegistered(registered);
//           } catch (e) {
//             // If checking fails, keep false but don't block UX
//             console.warn("Registration check failed:", e);
//             setIsRegistered(false);
//           }
//         }
//       } catch (err) {
//         console.error("Error fetching Ideathon event or registration:", err);
//         setError("Failed to load Ideathon event");
//       } finally {
//         setLoading(false);
//       }
//     };
//     fetchEvent();
//   }, []);

//   const handleSubmitIdea = async () => {
//     if (!event) return;

//     try {
//       // Re-check registration when button is clicked (fresh)
//       const regRes = await api.get(`/EventRegistration/is-registered/${event.eventId}`);
//       const registered = regRes.data && typeof regRes.data === "object"
//         ? regRes.data.isRegistered ?? regRes.data === true
//         : Boolean(regRes.data);

//       if (registered) {
//         // Registered → navigate to submit idea (upload)
//         navigate("/app/upload-knowledge", { state: { eventId: event.eventId } });
//       } else {
//         // Not registered → navigate to registration page
//         navigate("/app/events/event-registration", { state: { eventId: event.eventId } });
//       }
//     } catch (err) {
//       console.error("Error checking registration:", err);
//       // fallback to registration page
//       toast.error("Could not verify registration; please register first.");
//       navigate("/app/events/event-registration", { state: { eventId: event ? event.eventId : null } });
//     }
//   };

//   const handleRegister = async () => {
//     if (!event) return;
//     setRegLoading(true);
//     setError("");
//     try {
//       // ASSUMPTION: registration endpoint is POST /EventRegistration/register with body { eventId }
//       // If your API expects a different url/payload (e.g. POST /EventRegistration/register/{eventId}),
//       // change this call accordingly.
//       const payload = { eventId: event.eventId };
//       const res = await api.post("/EventRegistration/register", payload);
//       // assume response returns { success: true, isRegistered: true } or similar
//       const success = res.data && (res.data.success === true || res.data.isRegistered === true || res.status === 200);

//       if (success) {
//         setIsRegistered(true);
//         toast.success("Successfully registered for the Ideathon.");
//       } else {
//         toast.success("Registration submitted. If you don't see confirmation, check registrations page.");
//         setIsRegistered(res.data?.isRegistered ?? true);
//       }
//     } catch (err) {
//       console.error("Registration failed:", err);
//       const msg = err?.response?.data?.message ?? err?.message ?? "Registration failed";
//       toast.error(String(msg));
//       setError(String(msg));
//     } finally {
//       setRegLoading(false);
//     }
//   };

//   if (loading) return <p className="text-center text-gray-600">Loading event...</p>;
//   if (error) return <p className="text-center text-red-500">{error}</p>;
//   if (!event) return <p className="text-center text-gray-600">No Ideathon event available.</p>;

//   // Build timeline dynamically
//   const timeline = [
//     event.startDate && {
//       phase: "Team Formation & Registration",
//       date: `${new Date(event.startDate).toLocaleDateString()} – ${
//         event.registrationCloseDate ? new Date(event.registrationCloseDate).toLocaleDateString() : ""
//       }`,
//     },
//     event.finalSubmissionDeadline && {
//       phase: "Idea Submission Deadline",
//       date: new Date(event.finalSubmissionDeadline).toLocaleDateString(),
//     },
//     event.mentorCheckpointStart && {
//       phase: "Midway Mentor Review",
//       date: event.mentorCheckpointEnd
//         ? `${new Date(event.mentorCheckpointStart).toLocaleDateString()} – ${new Date(
//             event.mentorCheckpointEnd
//           ).toLocaleDateString()}`
//         : new Date(event.mentorCheckpointStart).toLocaleDateString(),
//     },
//     event.ideaPresentationStart && {
//       phase: "Final Presentations",
//       date: event.ideaPresentationEnd
//         ? `${new Date(event.ideaPresentationStart).toLocaleDateString()} – ${new Date(
//             event.ideaPresentationEnd
//           ).toLocaleDateString()}`
//         : new Date(event.ideaPresentationStart).toLocaleDateString(),
//     },
//     event.winnersAnnouncementDate && {
//       phase: "Winner Announcement",
//       date: new Date(event.winnersAnnouncementDate).toLocaleDateString(),
//     },
//   ].filter(Boolean);

//   return (
//     <motion.div
//       className="p-8 bg-gradient-to-br from-blue-50 to-white rounded-3xl shadow-xl"
//       initial={{ opacity: 0, y: 40 }}
//       animate={{ opacity: 1, y: 0 }}
//       transition={{ duration: 0.6 }}
//     >
//       {/* Header Section */}
//       <div className="flex flex-col gap-4 mb-8">
//         <h2 className="text-3xl font-extrabold text-blue-700">
//           Innovation Ideathon – {new Date(event.startDate).getFullYear()}
//         </h2>
//         <p className="text-gray-700 text-lg leading-relaxed">{event.description}</p>
//       </div>

//       {/* Theme Section */}
//       <motion.div
//         className="bg-blue-100 border-l-4 border-blue-500 p-4 rounded-lg mb-8"
//         initial={{ x: -40, opacity: 0 }}
//         animate={{ x: 0, opacity: 1 }}
//       >
//         <h3 className="text-xl font-semibold text-blue-700 mb-1">Theme of the Month</h3>
//         <p className="text-blue-800 font-medium">{event.title}</p>
//       </motion.div>

//       {/* Event Dates */}
//       <div className="flex flex-wrap gap-4 mb-10 text-gray-600">
//         {event.startDate && (
//           <span className="flex items-center gap-2">
//             <Calendar className="w-5 h-5 text-blue-500" />
//             <strong>Start:</strong> {new Date(event.startDate).toLocaleDateString()}
//           </span>
//         )}
//         {event.endDate && (
//           <span className="flex items-center gap-2">
//             <Calendar className="w-5 h-5 text-red-500" />
//             <strong>End:</strong> {new Date(event.endDate).toLocaleDateString()}
//           </span>
//         )}
//       </div>

//       {/* Timeline */}
//       <div className="mb-10">
//         <h3 className="text-2xl font-semibold text-gray-800 mb-4">Event Timeline</h3>
//         <div className="space-y-4">
//           {timeline.map((item, index) => (
//             <motion.div
//               key={index}
//               className="flex justify-between items-center bg-white p-4 rounded-xl shadow-sm border"
//               initial={{ opacity: 0, y: 20 }}
//               animate={{ opacity: 1, y: 0 }}
//               transition={{ delay: index * 0.1 }}
//             >
//               <span className="font-medium text-gray-700">{item.phase}</span>
//               <span className="text-sm text-gray-500">{item.date}</span>
//             </motion.div>
//           ))}
//         </div>
//       </div>

//       {/* Actions: Register + Submit Idea */}
//       <div className="flex flex-col sm:flex-row gap-4 items-center justify-center mt-6">
//         <button
//           onClick={handleRegister}
//           disabled={isRegistered || regLoading}
//           className={`px-6 py-2 rounded-full font-semibold text-lg transition ${
//             isRegistered ? "bg-gray-300 text-gray-700 cursor-default" : "bg-indigo-600 hover:bg-indigo-700 text-white"
//           }`}
//         >
//           {isRegistered ? "Registered" : regLoading ? "Registering..." : "Register"}
//         </button>

//         <button
//           onClick={handleSubmitIdea}
//           className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-6 rounded-full font-semibold text-lg"
//         >
//           Submit Your Idea
//         </button>
//       </div>

//       {/* Optional: Show registration status */}
//       {!isRegistered && (
//         <div className="text-center mt-2 text-sm text-gray-500">
//           You are not registered yet. Click <strong>Register</strong> to register.After Registration you can submit idea.
//         </div>
//       )}
//     </motion.div>
//   );
// }
import React, { useEffect, useState } from "react";
import { Calendar, Users, Star } from "lucide-react";
import { motion } from "framer-motion";
import api from "../api";
import { useNavigate } from "react-router-dom";
import KnowledgeCardsDisplay from "../components/KnowledgeCardsDisplay";
import PreviewModal from "../components/PreviewModal";

export default function IdeathonPage() {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [activeTab, setActiveTab] = useState({});
  const [userId] = useState(localStorage.getItem("userId"));
  const navigate = useNavigate();

  // State for preview modal
  const [previewItem, setPreviewItem] = useState(null);

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        setLoading(true);
        const res = await api.get("/Events/type/Ideathon");
        const eventsData = res?.data || [];
        setEvents(eventsData);

        if (eventsData.length > 0) {
          setActiveTab({ [eventsData[0].eventId]: "overview" });
        }
      } catch (err) {
        console.error("Error fetching Ideathon events:", err);
        setError("Failed to load Ideathon events.");
      } finally {
        setLoading(false);
      }
    };
    fetchEvents();
  }, []);

  const handleTabChange = (eventId, tab) => {
    setActiveTab((prev) => ({ ...prev, [eventId]: tab }));
  };

  const handleSubmitIdea = async (eventId, isRegistered) => {
    if (!userId) {
      navigate("/login");
      return;
    }
    try {
      if (isRegistered) {
        navigate("/app/upload-knowledge", { state: { eventId } });
      } else {
        navigate("/app/events/event-registration", { state: { eventId } });
      }
    } catch (err) {
      console.error("Error handling submission:", err);
      navigate("/app/events/event-registration", { state: { eventId } });
    }
  };

  if (loading)
    return <p className="text-center text-gray-600">Loading events...</p>;
  if (error) return <p className="text-center text-red-500">{error}</p>;
  if (events.length === 0)
    return <p className="text-center text-gray-600">No Ideathon events available.</p>;

  return (
    <div className="space-y-12 p-8">
      {events.map((event) => (
        <IdeathonCard
          key={event.eventId}
          event={event}
          userId={userId}
          activeTab={activeTab[event.eventId] || "overview"}
          onTabChange={(tab) => handleTabChange(event.eventId, tab)}
          onSubmitIdea={handleSubmitIdea}
          onPreview={(item) => setPreviewItem(item)} // Pass preview handler
        />
      ))}

      {previewItem && (
        <PreviewModal
          item={previewItem}
          onClose={() => setPreviewItem(null)}
        />
      )}
    </div>
  );
}

function IdeathonCard({ event, userId, activeTab, onTabChange, onSubmitIdea, onPreview }) {
  const [isRegistered, setIsRegistered] = useState(false);
  const [submissions, setSubmissions] = useState([]);
  const [feedback, setFeedback] = useState([]);
  const [insight, setInsight] = useState(null);
  const [replyText, setReplyText] = useState("");
  const [subFeedbackTab, setSubFeedbackTab] = useState("submissions"); // nested tab

  useEffect(() => {
    const fetchData = async () => {
      try {
        if (userId) {
          const regRes = await api.get(
            `/EventRegistration/is-registered/${event.eventId}?userId=${userId}`
          );
          setIsRegistered(regRes?.data?.isRegistered || false);

          const insightRes = await api.get(
            `/Events/${event.eventId}/user/${userId}/insight`
          );
          const data = insightRes?.data?.data;
          if (data) {
            setSubmissions(data.submissions || []);
            setFeedback(data.feedbacks || []);
            setInsight(data);
          }
        }
      } catch (err) {
        console.error("Error fetching user data for event:", err);
      }
    };
    fetchData();
  }, [event.eventId, userId]);

  const handleReply = async (feedbackId) => {
    if (!replyText.trim()) return;
    try {
      await api.post(
        `/EventInsight/feedback/${feedbackId}/reply?userId=${userId}`,
        replyText,
        { headers: { "Content-Type": "application/json" } }
      );
      setReplyText("");
      alert("Reply submitted successfully!");
    } catch (err) {
      console.error("Error posting reply:", err);
    }
  };

  const timeline = [
    event.startDate && {
      phase: "Team Formation & Registration",
      date: `${new Date(event.startDate).toLocaleDateString()} – ${
        event.registrationCloseDate
          ? new Date(event.registrationCloseDate).toLocaleDateString()
          : ""
      }`,
    },
    event.finalSubmissionDeadline && {
      phase: "Idea Submission Deadline",
      date: new Date(event.finalSubmissionDeadline).toLocaleDateString(),
    },
    event.mentorCheckpointStart && {
      phase: "Midway Mentor Review",
      date: event.mentorCheckpointEnd
        ? `${new Date(event.mentorCheckpointStart).toLocaleDateString()} – ${new Date(
            event.mentorCheckpointEnd
          ).toLocaleDateString()}`
        : new Date(event.mentorCheckpointStart).toLocaleDateString(),
    },
    event.ideaPresentationStart && {
      phase: "Final Presentations",
      date: event.ideaPresentationEnd
        ? `${new Date(event.ideaPresentationStart).toLocaleDateString()} – ${new Date(
            event.ideaPresentationEnd
          ).toLocaleDateString()}`
        : new Date(event.ideaPresentationStart).toLocaleDateString(),
    },
    event.winnersAnnouncementDate && {
      phase: "Winner Announcement",
      date: new Date(event.winnersAnnouncementDate).toLocaleDateString(),
    },
  ].filter(Boolean);

  return (
    <motion.div
      className="p-8 bg-gradient-to-br from-blue-50 to-white rounded-3xl shadow-xl"
      initial={{ opacity: 0, y: 40 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
    >
      {/* Header */}
      <div className="flex flex-col gap-4 mb-6">
        <h2 className="text-2xl md:text-3xl font-extrabold text-blue-700">
          {event.title} – {new Date(event.startDate).getFullYear()}
        </h2>
        <p className="text-gray-700">{event.description}</p>
      </div>

      {/* Top-level Tabs */}
      <div className="flex gap-4 border-b mb-6">
        <button
          className={`pb-2 px-4 font-semibold ${
            activeTab === "overview"
              ? "border-b-4 border-blue-600 text-blue-700"
              : "text-gray-500"
          }`}
          onClick={() => onTabChange("overview")}
        >
          Overview
        </button>
        <button
          className={`pb-2 px-4 font-semibold ${
            activeTab === "submissions"
              ? "border-b-4 border-blue-600 text-blue-700"
              : "text-gray-500"
          }`}
          onClick={() => onTabChange("submissions")}
        >
          My Submissions & Feedback
        </button>
      </div>

      {/* Overview */}
      {activeTab === "overview" && (
        <>
          <motion.div
            className="bg-blue-100 border-l-4 border-blue-500 p-4 rounded-lg mb-6"
            initial={{ x: -40, opacity: 0 }}
            animate={{ x: 0, opacity: 1 }}
          >
            <h3 className="text-xl font-semibold text-blue-700 mb-1">Event Theme</h3>
            <p className="text-blue-800 font-medium">{event.title}</p>
          </motion.div>

          <div className="flex flex-wrap gap-4 mb-6 text-gray-600">
            {event.startDate && (
              <span className="flex items-center gap-2">
                <Calendar className="w-5 h-5 text-blue-500" /> Start:{" "}
                {new Date(event.startDate).toLocaleDateString()}
              </span>
            )}
            {event.endDate && (
              <span className="flex items-center gap-2">
                <Calendar className="w-5 h-5 text-red-500" /> End:{" "}
                {new Date(event.endDate).toLocaleDateString()}
              </span>
            )}
          </div>

          <div className="mb-6">
            <h3 className="text-xl font-semibold text-gray-800 mb-4">Event Timeline</h3>
            <div className="space-y-3">
              {timeline.map((item, idx) => (
                <motion.div
                  key={idx}
                  className="flex justify-between items-center bg-white p-3 rounded-xl shadow-sm border"
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: idx * 0.05 }}
                >
                  <span className="font-medium text-gray-700">{item.phase}</span>
                  <span className="text-sm text-gray-500">{item.date}</span>
                </motion.div>
              ))}
            </div>
          </div>

          <div className="text-center mt-4">
            <button
              onClick={() => onSubmitIdea(event.eventId, isRegistered)}
              className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-6 rounded-full font-semibold"
            >
              Submit Your Idea
            </button>
          </div>
          {!isRegistered && (
            <div className="text-center mt-2 text-sm text-gray-500">
              You are not registered yet. Click "Submit Your Idea" to register your team first.
            </div>
          )}
        </>
      )}

      {/* Submissions & Feedback Nested Tabs */}
      {activeTab === "submissions" && (
        <div className="mt-6 space-y-6">
          {insight && (
            <motion.div
              className="bg-gradient-to-r from-blue-50 to-blue-100 border-l-4 border-blue-500 p-4 rounded-2xl shadow-sm"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
            >
              <p className="flex items-center gap-3 text-gray-800 font-medium">
                <Users className="w-5 h-5 text-blue-600" />
                Team: <strong>{insight.teamName}</strong> |{" "}
                <Star className="w-5 h-5 text-yellow-500" /> Avg Rating:{" "}
                <span className="font-semibold">{insight.averageRating || "N/A"}</span>
              </p>
            </motion.div>
          )}

          {/* Nested Tabs */}
          <div className="flex gap-4 border-b mb-4">
            <button
              className={`pb-2 px-4 font-semibold ${
                subFeedbackTab === "submissions"
                  ? "border-b-4 border-blue-600 text-blue-700"
                  : "text-gray-500"
              }`}
              onClick={() => setSubFeedbackTab("submissions")}
            >
              My Submissions
            </button>
            <button
              className={`pb-2 px-4 font-semibold ${
                subFeedbackTab === "feedback"
                  ? "border-b-4 border-blue-600 text-blue-700"
                  : "text-gray-500"
              }`}
              onClick={() => setSubFeedbackTab("feedback")}
            >
              Feedback
            </button>
          </div>

          {/* Nested Tab Content */}
          {subFeedbackTab === "submissions" && (
            <div className="max-h-[650px] overflow-y-auto">
              {submissions.length > 0 ? (
                <KnowledgeCardsDisplay
                  items={submissions.map((s) => ({
                    itemId: s.itemId,
                    title: s.itemTitle,
                    description: s.description || s.itemDescription,
                    tags: s.tags || [],
                    ownerName: s.submittedBy || s.createdByName,
                  }))}
                  userId={userId}
                  onPreview={onPreview} // Pass preview callback
                />
              ) : (
                <p className="text-gray-500 text-center mt-4">No submissions yet.</p>
              )}
            </div>
          )}

          {subFeedbackTab === "feedback" && (
            <div className="max-h-[650px] overflow-y-auto bg-gray-50 rounded-2xl shadow-inner border p-6">
              {feedback.length > 0 ? (
                <div className="space-y-4">
                  {feedback.map((fb) => (
                    <motion.div
                      key={fb.feedbackId}
                      className="bg-white border rounded-xl shadow-sm p-4 hover:shadow-md transition-shadow duration-200"
                    >
                      <div className="flex items-center justify-between mb-2">
                        <p className="font-semibold text-blue-700">{fb.mentorName}</p>
                        {fb.progressRating && (
                          <p className="text-sm text-yellow-600">⭐ {fb.progressRating}/5</p>
                        )}
                      </div>
                      <p className="text-gray-700 mb-3">{fb.feedbackText}</p>
                      <div className="flex gap-2 mt-3">
                        <input
                          type="text"
                          placeholder="Write a reply..."
                          className="flex-1 border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-300"
                          value={replyText}
                          onChange={(e) => setReplyText(e.target.value)}
                        />
                        <button
                          onClick={() => handleReply(fb.feedbackId)}
                          className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
                        >
                          Reply
                        </button>
                      </div>
                    </motion.div>
                  ))}
                </div>
              ) : (
                <p className="text-gray-500 text-center mt-4">No feedback received yet.</p>
              )}
            </div>
          )}
        </div>
      )}
    </motion.div>
  );
}