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
  const [checkingRegistration, setCheckingRegistration] = useState(false);
  const [isRegistered, setIsRegistered] = useState(false);

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

  useEffect(() => {
    if (!selectedEvent) {
      setIsRegistered(false);
      return;
    }

    let cancelled = false;
    const checkRegistration = async () => {
      try {
        setCheckingRegistration(true);
        const res = await api.get(`/EventRegistration/is-registered/${selectedEvent}`);
        const isReg = res.data?.isRegistered ?? false;
        if (!cancelled) setIsRegistered(Boolean(isReg));
      } catch (err) {
        console.error("Failed to check registration:", err);
        toast.error("Could not confirm registration status. You may still attempt to register.");
        if (!cancelled) setIsRegistered(false);
      } finally {
        if (!cancelled) setCheckingRegistration(false);
      }
    };

    checkRegistration();

    return () => {
      cancelled = true;
    };
  }, [selectedEvent]);

  const extractEmailsFromText = (text = "") => {
    const re = /[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-z]{2,}/g;
    const matches = text.match(re);
    return Array.isArray(matches) ? Array.from(new Set(matches.map(m => m.toLowerCase()))) : [];
  };

  const handleBackendError = (err) => {
    const backendMessage = err?.response?.data?.message || err?.message || "Registration failed";
    const conflictingEmails = extractEmailsFromText(backendMessage);

    if (conflictingEmails.length > 0) {
      toast.error(
        <div>
          <div className="font-medium">Registration failed</div>
          <div className="text-xs mt-1">
            These email(s) are conflicting: <b>{conflictingEmails.join(", ")}</b>
          </div>
          <div className="text-xs mt-1">Remove or replace them and try again.</div>
        </div>,
        { duration: 8000 }
      );
      return;
    }

    if (/not registered users/i.test(backendMessage)) {
      toast.error(`Some emails are not registered users. ${backendMessage}`);
      return;
    }

    if (/identical team/i.test(backendMessage) || /same members/i.test(backendMessage)) {
      toast.error(`An identical team already exists. ${backendMessage}`);
      return;
    }

    toast.error(backendMessage);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!selectedEvent || !teamName || !memberEmails) {
      toast.error("Please fill in all required fields.");
      return;
    }

    if (isRegistered) {
      toast.error("You are already registered for this event.");
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
      const res = await api.post("/EventRegistration/register-team", payload);
      if (res.data?.success === false || res.status >= 400) {
        const serverMsg = res.data?.message || "Registration failed";
        throw new Error(serverMsg);
      }

      toast.success("Team registered successfully!");
      navigate("/app/events/ideathon");
    } catch (err) {
      console.error("Registration failed:", err);
      handleBackendError(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 via-white to-blue-100 px-3 py-6">
      <div className="w-full max-w-lg bg-white rounded-2xl shadow-xl p-6 border border-blue-100">
        {/* Header */}
        <div className="text-center mb-6">
          <h1 className="text-2xl font-bold text-blue-700 mb-1">Team Registration</h1>
          <p className="text-gray-600 text-sm">Register your team to participate in the events.</p>
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
                  {ev.title} {ev.eventType ? `(${ev.eventType})` : ""}
                </option>
              ))}
            </select>
            <div className="mt-2 text-xs">
              {checkingRegistration ? (
                <span className="text-gray-500">Checking registration status…</span>
              ) : isRegistered ? (
                <span className="text-amber-600">You are already registered for this event. Registration disabled.</span>
              ) : selectedEvent ? (
                <span className="text-green-600">You are not registered for this event yet.</span>
              ) : null}
            </div>
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
            disabled={loading || checkingRegistration || isRegistered}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-md text-sm font-medium shadow-md transition-all duration-300 hover:scale-[1.01] disabled:opacity-70"
          >
            {loading ? "Registering..." : isRegistered ? "Already Registered" : "Register Team"}
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
