import React, { useEffect, useState } from "react";
import api from "../api";
import { useNavigate } from "react-router-dom";
import { toast } from "react-hot-toast";

export default function EventRegistration() {
  const [events, setEvents] = useState([]);
  const [selectedEvent, setSelectedEvent] = useState("");
  const [teamName, setTeamName] = useState("");
  const [memberEmails, setMemberEmails] = useState("");
  const [loading, setLoading] = useState(false);

  const navigate = useNavigate();

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        const res = await api.get("/Events");
        setEvents(res.data || []);
      } catch (err) {
        console.error("Error fetching events:", err);
        toast.error("Failed to load events");
      }
    };
    fetchEvents();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!selectedEvent || !teamName || !memberEmails) {
      toast.error("Please fill in all required fields.");
      return;
    }

    const membersArray = memberEmails
      .split(",")
      .map((email) => email.trim().toLowerCase())
      .filter(Boolean);

    const uniqueMembers = [...new Set(membersArray)];
    if (uniqueMembers.length !== membersArray.length) {
      toast.error("Duplicate member emails found.");
      return;
    }

    try {
      setLoading(true);
      const payload = {
        eventId: selectedEvent,
        teamName,
        teamMemberEmails: uniqueMembers,
      };
      await api.post("/EventRegistration/register-team", payload);

      toast.success("Team registered successfully!");
      navigate("/app/events/ideathon");
    } catch (err) {
      console.error("Registration failed:", err);
      toast.error(err.response?.data?.message || "Registration failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 via-white to-blue-100 px-3 py-6">
      <div className="w-full max-w-lg bg-white rounded-2xl shadow-xl p-6 border border-blue-100">
        {/* Header */}
        <div className="text-center mb-6">
          <h1 className="text-2xl font-bold text-blue-700 mb-1">
            Team Registration
          </h1>
          <p className="text-gray-600 text-sm">
            Register your team to participate in the events.
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Event Dropdown */}
          <div>
            <label className="block text-sm font-medium text-gray-800 mb-1">
              Select Event <span className="text-red-500">*</span>
            </label>
            <select
              className="w-full border border-gray-300 rounded-md p-2 text-sm focus:ring-2 focus:ring-blue-400 focus:outline-none transition"
              value={selectedEvent}
              onChange={(e) => setSelectedEvent(e.target.value)}
              required
            >
              <option value="">-- Choose Event --</option>
              {events.map((ev) => (
                <option key={ev.eventId} value={ev.eventId}>
                  {ev.title} ({ev.eventType})
                </option>
              ))}
            </select>
          </div>

          {/* TeamName */}
          <div>
            <label className="block text-sm font-medium text-gray-800 mb-1">
              Team Name <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={teamName}
              onChange={(e) => setTeamName(e.target.value)}
              placeholder="e.g. Code Warriors"
              className="w-full border border-gray-300 rounded-md p-2 text-sm focus:ring-2 focus:ring-blue-400 focus:outline-none transition"
              required
            />
          </div>

          {/* Team Members */}
          <div>
            <label className="block text-sm font-medium text-gray-800 mb-1">
              Team Members’ Emails <span className="text-red-500">*</span>
            </label>
            <textarea
              value={memberEmails}
              onChange={(e) => setMemberEmails(e.target.value)}
              placeholder="leader@example.com, member1@example.com"
              className="w-full border border-gray-300 rounded-md p-2 text-sm focus:ring-2 focus:ring-blue-400 focus:outline-none transition"
              rows={3}
              required
            />
            <p className="text-xs text-gray-500 mt-1">
              Separate emails with commas. Include the team leader in the list.
            </p>
          </div>

          {/* Submit Button */}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-md text-sm font-medium shadow-md transition-all duration-300 hover:scale-[1.01] disabled:opacity-70"
          >
            {loading ? "Registering..." : "Register Team"}
          </button>
        </form>

        {/* Footer Note */}
        <div className="text-center mt-6 text-xs text-gray-500">
          Once registered, you can submit your project idea.
        </div>
      </div>
    </div>
  );
}