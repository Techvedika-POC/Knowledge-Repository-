// import React, { useEffect, useState } from "react";
// import { Calendar } from "lucide-react";
// import { motion } from "framer-motion";
// import api from "../api";
// import { useNavigate } from "react-router-dom";

// export default function IdeathonPage() {
//   const [event, setEvent] = useState(null);
//   const [loading, setLoading] = useState(true);
//   const [error, setError] = useState("");
//   const [isRegistered, setIsRegistered] = useState(false);
//   const navigate = useNavigate();

//   useEffect(() => {
//     const fetchEvent = async () => {
//       try {
//         setLoading(true);
//         // 1 Fetch Ideathon event
//         const res = await api.get("/Events/type/Ideathon");
//         const eventData = res.data[0] || null;
//         setEvent(eventData);

//         // 2 Check registration for current user
//         if (eventData) {
//           const regRes = await api.get(`/EventRegistration/is-registered/${eventData.eventId}`);
//           setIsRegistered(regRes.data.isRegistered);
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
//       // Re-check registration when button is clicked
//       const regRes = await api.get(`/EventRegistration/is-registered/${event.eventId}`);

//       if (regRes.data.isRegistered) {
//         //  Registered → navigate to submit idea (leader only)
//         navigate("/app/upload-knowledge", { state: { eventId: event.eventId } });
//       } else {
//         //  Not registered → navigate to registration page
//         navigate("/app/events/event-registration", { state: { eventId: event.eventId } });
//       }
//     } catch (err) {
//       console.error("Error checking registration:", err);
//       navigate("/app/event-registration", { state: { eventId: event.eventId } });
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

//       {/* Submit Idea Button */}
//       <div className="text-center mt-6">
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
//           You are not registered yet. Click "Submit Your Idea" to register your team first.
//         </div>
//       )}
//     </motion.div>
//   );
// }
import React, { useEffect, useState } from "react";
import { Calendar, X } from "lucide-react";
import { motion } from "framer-motion";
import api from "../api";
import { useNavigate } from "react-router-dom";
import { toast } from "react-hot-toast";

export default function IdeathonPage() {
  const [event, setEvent] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isRegistered, setIsRegistered] = useState(false);
  const [regLoading, setRegLoading] = useState(false); // registration button loading
  const navigate = useNavigate();

  useEffect(() => {
    const fetchEvent = async () => {
      try {
        setLoading(true);
        // 1 Fetch Ideathon event
        const res = await api.get("/Events/type/Ideathon");
        const eventData = Array.isArray(res.data) && res.data.length > 0 ? res.data[0] : null;
        setEvent(eventData);

        // 2 Check registration for current user (only if event exists)
        if (eventData) {
          try {
            const regRes = await api.get(`/EventRegistration/is-registered/${eventData.eventId}`);
            // assume API returns { isRegistered: boolean } or boolean directly
            const registered = regRes.data && typeof regRes.data === "object"
              ? regRes.data.isRegistered ?? regRes.data === true
              : Boolean(regRes.data);
            setIsRegistered(registered);
          } catch (e) {
            // If checking fails, keep false but don't block UX
            console.warn("Registration check failed:", e);
            setIsRegistered(false);
          }
        }
      } catch (err) {
        console.error("Error fetching Ideathon event or registration:", err);
        setError("Failed to load Ideathon event");
      } finally {
        setLoading(false);
      }
    };
    fetchEvent();
  }, []);

  const handleSubmitIdea = async () => {
    if (!event) return;

    try {
      // Re-check registration when button is clicked (fresh)
      const regRes = await api.get(`/EventRegistration/is-registered/${event.eventId}`);
      const registered = regRes.data && typeof regRes.data === "object"
        ? regRes.data.isRegistered ?? regRes.data === true
        : Boolean(regRes.data);

      if (registered) {
        // Registered → navigate to submit idea (upload)
        navigate("/app/upload-knowledge", { state: { eventId: event.eventId } });
      } else {
        // Not registered → navigate to registration page
        navigate("/app/events/event-registration", { state: { eventId: event.eventId } });
      }
    } catch (err) {
      console.error("Error checking registration:", err);
      // fallback to registration page
      toast.error("Could not verify registration; please register first.");
      navigate("/app/events/event-registration", { state: { eventId: event ? event.eventId : null } });
    }
  };

  const handleRegister = async () => {
    if (!event) return;
    setRegLoading(true);
    setError("");
    try {
      // ASSUMPTION: registration endpoint is POST /EventRegistration/register with body { eventId }
      // If your API expects a different url/payload (e.g. POST /EventRegistration/register/{eventId}),
      // change this call accordingly.
      const payload = { eventId: event.eventId };
      const res = await api.post("/EventRegistration/register", payload);
      // assume response returns { success: true, isRegistered: true } or similar
      const success = res.data && (res.data.success === true || res.data.isRegistered === true || res.status === 200);

      if (success) {
        setIsRegistered(true);
        toast.success("Successfully registered for the Ideathon.");
      } else {
        toast.success("Registration submitted. If you don't see confirmation, check registrations page.");
        setIsRegistered(res.data?.isRegistered ?? true);
      }
    } catch (err) {
      console.error("Registration failed:", err);
      const msg = err?.response?.data?.message ?? err?.message ?? "Registration failed";
      toast.error(String(msg));
      setError(String(msg));
    } finally {
      setRegLoading(false);
    }
  };

  if (loading) return <p className="text-center text-gray-600">Loading event...</p>;
  if (error) return <p className="text-center text-red-500">{error}</p>;
  if (!event) return <p className="text-center text-gray-600">No Ideathon event available.</p>;

  // Build timeline dynamically
  const timeline = [
    event.startDate && {
      phase: "Team Formation & Registration",
      date: `${new Date(event.startDate).toLocaleDateString()} – ${
        event.registrationCloseDate ? new Date(event.registrationCloseDate).toLocaleDateString() : ""
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
      transition={{ duration: 0.6 }}
    >
      {/* Header Section */}
      <div className="flex flex-col gap-4 mb-8">
        <h2 className="text-3xl font-extrabold text-blue-700">
          Innovation Ideathon – {new Date(event.startDate).getFullYear()}
        </h2>
        <p className="text-gray-700 text-lg leading-relaxed">{event.description}</p>
      </div>

      {/* Theme Section */}
      <motion.div
        className="bg-blue-100 border-l-4 border-blue-500 p-4 rounded-lg mb-8"
        initial={{ x: -40, opacity: 0 }}
        animate={{ x: 0, opacity: 1 }}
      >
        <h3 className="text-xl font-semibold text-blue-700 mb-1">Theme of the Month</h3>
        <p className="text-blue-800 font-medium">{event.title}</p>
      </motion.div>

      {/* Event Dates */}
      <div className="flex flex-wrap gap-4 mb-10 text-gray-600">
        {event.startDate && (
          <span className="flex items-center gap-2">
            <Calendar className="w-5 h-5 text-blue-500" />
            <strong>Start:</strong> {new Date(event.startDate).toLocaleDateString()}
          </span>
        )}
        {event.endDate && (
          <span className="flex items-center gap-2">
            <Calendar className="w-5 h-5 text-red-500" />
            <strong>End:</strong> {new Date(event.endDate).toLocaleDateString()}
          </span>
        )}
      </div>

      {/* Timeline */}
      <div className="mb-10">
        <h3 className="text-2xl font-semibold text-gray-800 mb-4">Event Timeline</h3>
        <div className="space-y-4">
          {timeline.map((item, index) => (
            <motion.div
              key={index}
              className="flex justify-between items-center bg-white p-4 rounded-xl shadow-sm border"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.1 }}
            >
              <span className="font-medium text-gray-700">{item.phase}</span>
              <span className="text-sm text-gray-500">{item.date}</span>
            </motion.div>
          ))}
        </div>
      </div>

      {/* Actions: Register + Submit Idea */}
      <div className="flex flex-col sm:flex-row gap-4 items-center justify-center mt-6">
        <button
          onClick={handleRegister}
          disabled={isRegistered || regLoading}
          className={`px-6 py-2 rounded-full font-semibold text-lg transition ${
            isRegistered ? "bg-gray-300 text-gray-700 cursor-default" : "bg-indigo-600 hover:bg-indigo-700 text-white"
          }`}
        >
          {isRegistered ? "Registered" : regLoading ? "Registering..." : "Register"}
        </button>

        <button
          onClick={handleSubmitIdea}
          className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-6 rounded-full font-semibold text-lg"
        >
          Submit Your Idea
        </button>
      </div>

      {/* Optional: Show registration status */}
      {!isRegistered && (
        <div className="text-center mt-2 text-sm text-gray-500">
          You are not registered yet. Click <strong>Register</strong> to register.After Registration you can submit idea.
        </div>
      )}
    </motion.div>
  );
}
