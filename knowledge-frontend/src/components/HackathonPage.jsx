import React, { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../api"; 

export default function HackathonPage() {
  const [events, setEvents] = useState([]);
  const [loadingEvents, setLoadingEvents] = useState(true);
  const [errorEvents, setErrorEvents] = useState("");
  const navigate = useNavigate();

  // Fetch Hackathon Events
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

  // Handle submit idea click with registration check
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
        className="relative bg-cover bg-center h-screen"
        style={{
          backgroundImage:
            "url('https://images.unsplash.com/photo-1518770660439-4636190af475')",
        }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="relative z-10 flex flex-col items-center justify-center h-full text-center text-white px-4">
          <h1 className="text-5xl md:text-6xl font-bold mb-4">Hackathon 2025</h1>
          <p className="text-xl md:text-2xl mb-8">
            Innovate, Collaborate, Create
          </p>
          {/* Centralized Register Button */}
          <Link
            to="/app/events/event-registration"
            className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-6 rounded-full text-lg"
          >
            Register for Events
          </Link>
        </div>
      </section>

      {/* About Section */}
      <section className="py-16 px-6 text-center">
        <h2 className="text-3xl font-semibold mb-4">About the Hackathon</h2>
        <p className="text-lg max-w-3xl mx-auto text-gray-700">
          Join us for an exciting 48-hour hackathon where developers, designers,
          and innovators come together to solve real-world problems.
        </p>
      </section>

      {/* Timeline Section */}
      <section className="bg-gray-100 py-16 px-6">
        <h2 className="text-3xl font-semibold text-center mb-8">Event Timeline</h2>
        <div className="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
          <div className="text-center bg-white shadow-lg rounded-xl p-6">
            <h3 className="text-xl font-semibold mb-2 text-blue-700">Day 1</h3>
            <p>Kickoff & Team Formation</p>
          </div>
          <div className="text-center bg-white shadow-lg rounded-xl p-6">
            <h3 className="text-xl font-semibold mb-2 text-blue-700">Day 2</h3>
            <p>Development & Mentorship</p>
          </div>
          <div className="text-center bg-white shadow-lg rounded-xl p-6">
            <h3 className="text-xl font-semibold mb-2 text-blue-700">Day 3</h3>
            <p>Final Presentations & Judging</p>
          </div>
        </div>
      </section>

      {/* Guidelines */}
      <section className="py-8 px-6 text-center max-w-4xl mx-auto">
        <p className="text-lg text-gray-700">
          Please make sure to register for your team before submitting your idea. 
          Clicking "Submit Idea" without registration will redirect you to the registration page.
        </p>
      </section>

      {/* Hackathon Events Section */}
      <section className="py-16 px-6 max-w-7xl mx-auto">
        <h2 className="text-3xl font-bold text-center mb-12 text-gray-800">
          Hackathon Events
        </h2>

        {loadingEvents ? (
          <p className="text-center text-gray-600">Loading events...</p>
        ) : errorEvents ? (
          <p className="text-center text-red-500">{errorEvents}</p>
        ) : events.length === 0 ? (
          <p className="text-center text-gray-600">No events available currently.</p>
        ) : (
          <div className="space-y-6 max-w-3xl mx-auto">
            {events.map((e, i) => (
              <div
                key={e.eventId || i}
                className="bg-white shadow-lg rounded-xl p-6 border-l-4 border-blue-600"
              >
                <h3 className="text-2xl font-bold text-blue-700 mb-2">{e.title}</h3>
                <p className="text-gray-700 text-lg mb-4">{e.description}</p>

                {/* Submit Idea Button */}
                <div className="text-center mt-4">
                  <button
                    onClick={() => handleSubmitIdea(e.eventId)}
                    className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-6 rounded-full font-semibold text-lg"
                  >
                    Submit Idea
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Footer */}
      <footer className="bg-gray-800 text-white py-8 text-center">
        <p>&copy; 2025 Hackathon. All rights reserved.</p>
        <div className="mt-4">
          <a href="#" className="text-blue-400 mx-2">
            Facebook
          </a>
          <a href="#" className="text-blue-400 mx-2">
            Twitter
          </a>
          <a href="#" className="text-blue-400 mx-2">
            LinkedIn
          </a>
        </div>
      </footer>
    </div>
  );
}
