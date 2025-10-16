import React, { useEffect, useState } from "react";
import { Calendar } from "lucide-react";
import { motion } from "framer-motion";
import api from "../api";
import { useNavigate } from "react-router-dom";

export default function IdeathonPage() {
  const [event, setEvent] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  // ✅ Fetch the Ideathon event from backend
  useEffect(() => {
    const fetchEvent = async () => {
      try {
        setLoading(true);
        const res = await api.get("/Events/type/Ideathon");
        setEvent(res.data[0] || null); // Expecting one Ideathon event per month
      } catch (err) {
        console.error("Error fetching Ideathon event:", err);
        setError("Failed to load Ideathon event");
      } finally {
        setLoading(false);
      }
    };
    fetchEvent();
  }, []);

  // ✅ Pass eventId to upload page
  const handleSubmitIdea = (id) => {
    navigate("/app/upload-knowledge", { state: { eventId: id } });
  };

  if (loading)
    return <p className="text-center text-gray-600">Loading event...</p>;
  if (error)
    return <p className="text-center text-red-500">{error}</p>;
  if (!event)
    return <p className="text-center text-gray-600">No Ideathon event available.</p>;

  // ✅ Example timeline data (static)
  const timeline = [
    { phase: "Team Formation & Registration", date: "Oct 5 – Oct 10" },
    { phase: "Idea Submission Deadline", date: "Oct 15" },
    { phase: "Midway Mentor Review", date: "Oct 18" },
    { phase: "Final Presentations", date: "Oct 24" },
    { phase: "Winner Announcement", date: "Oct 25" },
  ];

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
          Innovation Ideathon – October 2025
        </h2>
        <p className="text-gray-700 text-lg leading-relaxed">
          Join the monthly Innovation Ideathon! Form teams, brainstorm creative
          solutions to our sustainability challenges, and pitch your ideas to
          the board. Winning ideas move into project development and get
          published as knowledge items!
        </p>
      </div>

      {/* Theme Section */}
      <motion.div
        className="bg-blue-100 border-l-4 border-blue-500 p-4 rounded-lg mb-8"
        initial={{ x: -40, opacity: 0 }}
        animate={{ x: 0, opacity: 1 }}
      >
        <h3 className="text-xl font-semibold text-blue-700 mb-1">
          Theme of the Month
        </h3>
        <p className="text-blue-800 font-medium">{event.title}</p>
      </motion.div>

      {/* Event Dates */}
      <div className="flex flex-wrap gap-4 mb-10 text-gray-600">
        <span className="flex items-center gap-2">
          <Calendar className="w-5 h-5 text-blue-500" />
          <strong>Start:</strong>{" "}
          {new Date(event.startDate).toLocaleDateString()}
        </span>
        <span className="flex items-center gap-2">
          <Calendar className="w-5 h-5 text-red-500" />
          <strong>End:</strong>{" "}
          {new Date(event.endDate).toLocaleDateString()}
        </span>
      </div>

      {/* Timeline */}
      <div className="mb-10">
        <h3 className="text-2xl font-semibold text-gray-800 mb-4">
          📅 Event Timeline
        </h3>
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

      {/* Submit Idea */}
      <div className="text-center mt-6">
        <button
          onClick={() => handleSubmitIdea(event.eventId)} // ✅ Correctly passes eventId
          className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-6 rounded-full font-semibold text-lg"
        >
          Submit Your Idea
        </button>
      </div>
    </motion.div>
  );
}
