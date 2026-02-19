import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api";
import { motion } from "framer-motion";
import { FaCode, FaClock, FaSignal, FaUsers, FaTrophy } from "react-icons/fa";

export default function CodingChallengeEventsPage() {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [metrics, setMetrics] = useState(null);
  const navigate = useNavigate();
  useEffect(() => {
    loadEvents();
    loadMetrics();
  }, []);

  const loadMetrics = async () => {
    try {
      const res = await api.get("/coding/metrics");
      setMetrics(res.data);
    } catch (err) {
      console.error("Failed to load metrics", err);
    }
  };

  const loadEvents = async () => {
    try {
      const res = await api.get("/Events/type/Coding Challenge");
      setEvents(res.data || []);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-100 flex items-center justify-center text-slate-600">
        Loading Coding Challenges...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-white to-indigo-100">
      <div className="sticky top-0 z-50 backdrop-blur-lg bg-white/80 border-b shadow-sm">
        <div className="max-w-7xl mx-auto px-10 py-4 flex justify-between items-center">
          <div className="flex items-center gap-3">
            <div className="
        w-10 h-8 rounded-xl
        bg-gradient-to-br from-indigo-300 to-violet-300
        flex items-center justify-center text-white font-bold
      ">
              <span className="text-lg">{'</>'}</span>
            </div>

            <div>
              <h1 className="text-xl font-bold bg-gradient-to-r from-indigo-600 to-violet-600 bg-clip-text text-transparent">
                Coding Challenge Arena
              </h1>
              <p className="text-xs text-slate-500">
                Live Engineering Platform
              </p>
            </div>
          </div>
          <div className="flex items-center gap-4">
            <span className="
        px-4 py-1 rounded-full
        bg-indigo-50 text-indigo-700
        text-sm font-medium
      ">
              Challenges
            </span>

            <span
              onClick={() => navigate("/app/coding/leaderboard")}
              className="
    px-4 py-1 rounded-full
    text-sm text-slate-500
    hover:bg-slate-100
    transition cursor-pointer
  "
            >
              Leaderboard
            </span>

          </div>
        </div>
      </div>
      <div className="max-w-7xl mx-auto px-12 py-2">
        <motion.h2
          className="text-xl font-semibold text-slate-800"
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
        >
          Discover & Compete
        </motion.h2>
        <p className="text-slate-600 max-w-2xl mb-8">
          Participate in real-world coding challenges, track your performance,
          and build a professional engineering profile.
        </p>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
          {!metrics ? (
            <>
              <StatCard label="Loading..." value="..." />
              <StatCard label="Loading..." value="..." />
              <StatCard label="Loading..." value="..." />
              <StatCard label="Loading..." value="..." />
            </>
          ) : (
            <>
              <StatCard icon={<FaCode />} label="Active Challenges" value={metrics.activeChallenges} />
              <StatCard icon={<FaUsers />} label="Participants" value={metrics.participants} />
              <StatCard icon={<FaTrophy />} label="Top Score" value={metrics.topScore} />
              <StatCard icon={<FaSignal />} label="Difficulty" value={metrics.difficulty} />
            </>
          )}
        </div>

      </div>
      <div className="max-w-7xl mx-auto px-12 mb-4">
        <h3 className="text-xl font-semibold text-slate-800">
          Available Challenges
        </h3>
        <p className="text-slate-500 text-sm">
          Select a challenge to start coding.
        </p>
      </div>
      <div className="max-w-7xl mx-auto px-12 pb-20 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
        {events.map((evt, index) => (
          <motion.div
            key={evt.eventId}
            whileHover={{ y: -6 }}
            className="
              bg-white rounded-2xl border
              shadow-sm hover:shadow-xl
              transition overflow-hidden
            "
          >
            <div
              className={`
                h-20 flex items-center px-6 text-black
                ${index % 3 === 0 && "bg-gradient-to-r from-indigo-200 to-blue-200"}
                ${index % 3 === 1 && "bg-gradient-to-r from-emerald-200 to-teal-200"}
                ${index % 3 === 2 && "bg-gradient-to-r from-violet-200 to-purple-200"}
              `}
            >
              <FaCode className="text-xl mr-3" />
              <h3 className="font-semibold">
                {evt.title}
              </h3>
            </div>

            {/* CARD BODY */}
            <div className="p-6">
              <p className="text-sl text-sm mb-4 line-clamp-3">
                {evt.description}
              </p>

              <div className="flex justify-between text-xs text-black mb-4">
                <div className="flex items-center gap-2">
                  <FaClock /> 90 mins
                </div>
                <div className="flex items-center gap-2">
                  <FaSignal /> Intermediate
                </div>
              </div>

              <button
                onClick={() =>
                  navigate(`/app/events/coding-challenge/${evt.eventId}`)
                }
                className="
                  w-full py-2 rounded-lg
                  bg-indigo-500 text-white
                  hover:bg-indigo-500
                  transition
                "
              >
                Start Challenge
              </button>
            </div>
          </motion.div>
        ))}
      </div>
    </div>
  );
}
function StatCard({ icon, label, value }) {
  return (
    <div className="bg-white rounded-xl border shadow-sm p-5 flex items-center gap-4">
      <div className="text-indigo-600 text-xl">
        {icon}
      </div>
      <div>
        <p className="text-xs text-slate-500">{label}</p>
        <p className="text-lg font-semibold text-slate-800">{value}</p>
      </div>
    </div>
  );
}
