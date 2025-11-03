import React, { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../api";

export default function HackathonPage() {
  const [events, setEvents] = useState([]);
  const [loadingEvents, setLoadingEvents] = useState(true);
  const [errorEvents, setErrorEvents] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        setLoadingEvents(true);
        const res = await api.get("/Events/type/Hackathon");
        setEvents(res.data || []);
      } catch (err) {
        console.error("Error fetching events:", err);
        setErrorEvents("Failed to load events");
      } finally {
        setLoadingEvents(false);
      }
    };
    fetchEvents();
  }, []);

  const handleSubmitIdea = async (eventId) => {
    try {
      const regRes = await api.get(`/EventRegistration/is-registered/${eventId}`);
      if (regRes.data.isRegistered) {
        navigate("/app/upload-knowledge", { state: { eventId } });
      } else {
        navigate("/app/events/event-registration", { state: { eventId } });
      }
    } catch (err) {
      console.error("Error checking registration:", err);
      navigate("/app/events/event-registration", { state: { eventId } });
    }
  };

  return (
    <div className="bg-gray-50 font-sans">
      {/* Hero Section */}
      <section
        className="relative bg-cover bg-center h-[80vh]"
        style={{
          backgroundImage:
            "url('https://images.unsplash.com/photo-1518770660439-4636190af475')",
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="relative z-10 flex flex-col items-center justify-center h-full text-center text-white px-4">
          <h1 className="text-4xl md:text-5xl font-bold mb-3">Hackathon 2025</h1>
          <p className="text-lg md:text-xl mb-6">Innovate • Collaborate • Create</p>
          <Link
            to="/app/events/event-registration"
            className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-5 rounded-full text-base"
          >
            Register Now
          </Link>
        </div>
      </section>

      {/* About Section */}
      <section className="py-10 px-6 text-center">
        <h2 className="text-2xl font-semibold mb-3">About the Hackathon</h2>
        <p className="text-base max-w-2xl mx-auto text-gray-700">
          Join an exciting 48-hour hackathon where developers, designers, and innovators
          collaborate to solve real-world problems.
        </p>
      </section>

      {/* Timeline Section */}
      <section className="bg-gray-100 py-10 px-6">
        <h2 className="text-2xl font-semibold text-center mb-6">Event Timeline</h2>
        <div className="grid md:grid-cols-3 gap-6 max-w-5xl mx-auto">
          {[
            { day: "Day 1", desc: "Kickoff & Team Formation" },
            { day: "Day 2", desc: "Development & Mentorship" },
            { day: "Day 3", desc: "Final Presentations & Judging" },
          ].map((item, i) => (
            <div key={i} className="text-center bg-white shadow-md rounded-xl p-5">
              <h3 className="text-lg font-semibold mb-1 text-blue-700">{item.day}</h3>
              <p className="text-gray-600 text-sm">{item.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Guidelines */}
      <section className="py-6 px-6 text-center max-w-3xl mx-auto">
        <p className="text-base text-gray-700">
          Please register your team before submitting an idea. If not registered, you'll
          be redirected to the registration page.
        </p>
      </section>

      {/* Hackathon Events */}
      <section className="py-10 px-6 max-w-6xl mx-auto">
        <h2 className="text-2xl font-bold text-center mb-8 text-gray-800">
          Hackathon Events
        </h2>

        {loadingEvents ? (
          <p className="text-center text-gray-600 text-sm">Loading events...</p>
        ) : errorEvents ? (
          <p className="text-center text-red-500 text-sm">{errorEvents}</p>
        ) : events.length === 0 ? (
          <p className="text-center text-gray-600 text-sm">No events available currently.</p>
        ) : (
          <div className="space-y-5 max-w-3xl mx-auto">
            {events.map((e, i) => (
              <div
                key={e.eventId || i}
                className="bg-white shadow-md rounded-lg p-5 border-l-4 border-blue-600"
              >
                <h3 className="text-xl font-bold text-blue-700 mb-2">{e.title}</h3>
                <p className="text-gray-700 text-sm mb-3">{e.description}</p>
                <div className="text-center">
                  <button
                    onClick={() => handleSubmitIdea(e.eventId)}
                    className="bg-blue-600 hover:bg-blue-700 text-white py-1.5 px-5 rounded-full font-medium text-sm"
                  >
                    Submit Idea
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
