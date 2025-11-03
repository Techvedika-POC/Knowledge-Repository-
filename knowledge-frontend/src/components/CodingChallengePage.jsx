import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Swiper, SwiperSlide } from "swiper/react";
import { Navigation, Autoplay } from "swiper/modules";
import "swiper/css";
import "swiper/css/navigation";
import api from "../api";

const staticChallenges = [
  { title: "Algorithm Sprint", desc: "Sharpen your problem-solving skills in time-bound algorithm challenges.", image: "algorithm.png" },
  { title: "Frontend Flex", desc: "Design responsive dashboards with modern UI patterns for business use.", image: "Frontendflex.png" },
  { title: "Backend Battle", desc: "Build APIs and backend solutions with efficiency and security.", image: "backendbattle.png" },
  { title: "UX/UI Challenge", desc: "Create an intuitive, modern interface for a business application.", image: "ux.png" },
  { title: "Cloud Integration", desc: "Deploy scalable services using cloud platforms.", image: "Frontendflex.png" },
  { title: "Data Analytics", desc: "Analyze datasets to extract business insights.", image: "ai.png" },
  { title: "Security Sprint", desc: "Identify vulnerabilities and improve system security.", image: "ux.png" },
];

export default function CodingChallengePage() {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        setLoading(true);
        const res = await api.get("/Events/type/Coding Challenge");
        setEvents(res.data || []);
      } catch (err) {
        console.error("Error fetching events:", err);
        setError("Failed to load events");
      } finally {
        setLoading(false);
      }
    };
    fetchEvents();
  }, []);

  const handleSubmitIdea = (eventId) => {
    navigate("/app/upload-knowledge", { state: { eventId } });
  };

  return (
    <div className="bg-gradient-to-br from-gray-50 via-white to-gray-50 min-h-screen font-sans">
      {/* Hero Section */}
      <section className="relative py-10 px-6 text-center bg-gradient-to-r from-blue-50 to-indigo-50">
        <h1 className="text-3xl md:text-4xl font-bold text-gray-800 mb-3">
          Coding Challenge 2025
        </h1>
        <p className="text-base md:text-lg text-gray-700 max-w-2xl mx-auto mb-5">
          Join professionally curated coding challenges designed for innovation, skill growth, and business success.
        </p>
        <a
          href="/register"
          className="bg-blue-600 text-white px-6 py-2 rounded-full font-semibold hover:bg-blue-700 transition text-sm"
        >
          Register Now
        </a>
      </section>

      {/* About Section */}
      <section className="py-8 px-6 bg-white text-center">
        <h2 className="text-2xl font-semibold text-blue-700 mb-3">
          About the Challenge
        </h2>
        <p className="max-w-3xl mx-auto text-gray-600 text-base">
          The Coding Challenge brings together top talents to solve real-world
          problems. Test your skills, network with professionals, and showcase
          your expertise.
        </p>
      </section>

      {/* Carousel Section */}
      <section className="py-10 px-6 max-w-7xl mx-auto bg-gradient-to-br from-blue-50 via-purple-50 to-pink-50">
        <h2 className="text-2xl font-bold text-center mb-8 text-gray-800">
          Explore the Challenges
        </h2>

        <Swiper
          modules={[Navigation, Autoplay]}
          navigation
          autoplay={{ delay: 3000, disableOnInteraction: false }}
          loop
          spaceBetween={20}
          slidesPerView={1}
          className="max-w-5xl mx-auto"
        >
          {staticChallenges.map((c, i) => (
            <SwiperSlide key={i}>
              <div className="flex flex-col md:flex-row items-stretch bg-white rounded-2xl shadow-xl overflow-hidden h-80">
                {/* Image */}
                <div className="md:w-1/2 h-full">
                  <img
                    src={`/assets/${c.image}`}
                    alt={c.title}
                    className="w-full h-full object-cover hover:scale-105 transition-transform duration-500"
                  />
                </div>

                {/* Text */}
                <div className="md:w-1/2 p-6 flex flex-col justify-center bg-gradient-to-br from-white to-blue-50">
                  <h3 className="text-xl font-semibold text-blue-700 mb-2">
                    {c.title}
                  </h3>
                  <p className="text-gray-600 text-sm mb-4">{c.desc}</p>
                  <span className="inline-block bg-blue-100 text-blue-800 px-4 py-1 rounded-full font-medium text-sm hover:bg-blue-200 transition cursor-pointer">
                    Learn More
                  </span>
                </div>
              </div>
            </SwiperSlide>
          ))}
        </Swiper>
      </section>

      {/* Benefits Section */}
      <section className="py-8 px-6 bg-gradient-to-r from-blue-50 to-indigo-50 text-center">
        <h2 className="text-2xl font-bold text-blue-700 mb-6">Why Participate?</h2>
        <div className="grid md:grid-cols-3 gap-6 max-w-5xl mx-auto">
          {[
            { title: "Skill Growth", text: "Enhance your coding skills through real challenges and feedback." },
            { title: "Networking", text: "Collaborate and connect with professionals in the industry." },
            { title: "Career Boost", text: "Showcase your skills and gain valuable career opportunities." },
          ].map((b, i) => (
            <div key={i} className="bg-white rounded-xl shadow-md p-6">
              <h3 className="font-semibold text-lg mb-2 text-blue-700">{b.title}</h3>
              <p className="text-gray-600 text-sm">{b.text}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Dynamic Coding Challenge Events Section */}
      <section className="py-10 px-6 bg-gradient-to-r from-blue-50 to-indigo-50 text-center">
        <h2 className="text-2xl font-bold text-blue-700 mb-6">Coding Challenge Events</h2>

        {loading ? (
          <p className="text-center text-gray-600 text-sm">Loading events...</p>
        ) : error ? (
          <p className="text-center text-red-500 text-sm">{error}</p>
        ) : events.length === 0 ? (
          <p className="text-center text-gray-600 text-sm">No events available currently.</p>
        ) : (
          <div className="grid md:grid-cols-2 gap-6 max-w-5xl mx-auto">
            {events.map((e, i) => (
              <div
                key={e.eventId || i}
                className="bg-white rounded-xl shadow-md p-5 flex flex-col justify-between h-full"
              >
                <div>
                  <h3 className="text-lg font-semibold text-blue-700 mb-1">{e.title}</h3>
                  <p className="text-gray-700 text-sm">{e.description}</p>
                </div>
                <button
                  onClick={() => handleSubmitIdea(e.eventId)}
                  className="mx-auto mt-3 bg-blue-600 text-white text-xs px-4 py-1.5 rounded-full font-medium shadow hover:bg-blue-700 transition"
                >
                  Submit
                </button>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Call-to-Action Section */}
      <section className="py-10 px-6 bg-blue-50 text-center">
        <h2 className="text-2xl font-bold text-blue-800 mb-3">
          Ready to Take the Challenge?
        </h2>
        <p className="text-gray-700 text-sm max-w-2xl mx-auto mb-4">
          Submit now and prove your skills in a business-driven environment
          designed to push innovation.
        </p>
      </section>

      {/* Footer */}
      <footer className="bg-white text-gray-700 py-5 text-center border-t text-sm">
        <p>&copy; 2025 Coding Challenge. All rights reserved.</p>
      </footer>
    </div>
  );
}
