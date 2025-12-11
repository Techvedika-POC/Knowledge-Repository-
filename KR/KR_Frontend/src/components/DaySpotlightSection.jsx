import React from "react";

export default function DaySpotlightSection({ daySpotlight }) {
  if (!daySpotlight) return null;

  return (
    <div className="relative bg-gradient-to-br from-green-50 via-yellow-50 to-pink-50 rounded-3xl shadow-2xl p-6 max-w-xl mx-auto border-l-8 border-gradient-to-b from-green-400 to-pink-400 hover:scale-105 transform transition duration-300 cursor-pointer">
      <span className="text-sm font-bold text-indigo-500 uppercase tracking-wide mb-2 block">🌟 Day Spotlight</span>

      <h4 className="text-2xl font-extrabold text-indigo-900 mb-4 flex items-center gap-2">
        <span className="inline-block w-4 h-4 bg-gradient-to-tr from-green-400 to-pink-400 rounded-full animate-pulse"></span>
        Resource to Explore: {daySpotlight.resourceTitle}
      </h4>

      <div className="mb-4 p-4 bg-white rounded-xl shadow-inner border-l-4 border-yellow-300">
        <h5 className="font-semibold text-indigo-700 mb-2">💡 Tip of the Day</h5>
        <p className="text-gray-700">{daySpotlight.tip}</p>
      </div>

      <div className="p-4 bg-indigo-50 rounded-xl border-l-4 border-indigo-400">
        <h5 className="font-semibold text-indigo-700 mb-2">📜 Quote / Insight</h5>
        <blockquote className="italic text-gray-600">“{daySpotlight.quote}”</blockquote>
      </div>
    </div>
  );
}
