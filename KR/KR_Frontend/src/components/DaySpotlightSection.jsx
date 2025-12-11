import React, { useEffect, useState } from "react";
import api from "../api";

export default function DaySpotlightSection() {
  const [daySpotlight, setDaySpotlight] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchSpotlight = async () => {
      try {
        setLoading(true);
        setError("");
        const res = await api.get("/DaySpotlight");
        setDaySpotlight(res.data || null);
      } catch (err) {
        setError("Failed to load Day Spotlight.");
      } finally {
        setLoading(false);
      }
    };
    fetchSpotlight();
  }, []);

  if (loading) return <p className="px-6 mt-6 text-xs text-slate-500">Loading Day Spotlight…</p>;
  if (error) return <p className="px-6 mt-6 text-xs text-red-500">{error}</p>;
  if (!daySpotlight) return null;

  return (
    <div className="px-2  mb-8">
      <article className="
        w-full max-w-5xl mx-auto rounded-[32px]
        bg-gradient-to-br from-white via-slate-50 to-indigo-50
        border border-slate-200 shadow-xl 
        px-10 py-8
      ">
        <div className="flex flex-col md:flex-row md:justify-between gap-10">

          {/* LEFT BLOCK */}
          <div className="flex-1">

            {/* ⭐ Expanded Label */}
            <div className="inline-flex items-center gap-2 px-4 py-1.5 
              rounded-full bg-indigo-100 border border-indigo-200 shadow-sm mb-4">
              <span className="text-xs font-bold uppercase text-indigo-700 tracking-wide">
                🌟 Day Spotlight
              </span>
            </div>

            {/* TITLE */}
            <h2 className="text-2xl md:text-[26px] font-extrabold text-slate-900">
              Resource to Explore:
            </h2>

            {/* RESOURCE TITLE */}
            <p className="mt-1 text-lg font-semibold text-indigo-800 leading-snug">
              {daySpotlight.resourceTitle}
            </p>

            {/* FOCUS */}
            <div className="mt-3 flex items-center gap-2 text-sm text-slate-600">
              <span className="w-2 h-2 bg-emerald-500 rounded-full"></span>
              <span className="font-medium">Today&apos;s focus</span>
            </div>

            {/* MAIN DESCRIPTION */}
            <p className="mt-3 text-[14px] text-slate-700 leading-relaxed max-w-2xl">
              {daySpotlight.tip}
            </p>
          </div>

          {/* RIGHT BLOCK (TIP + QUOTE) */}
          <div className="w-full md:w-[320px] flex flex-col gap-4">

            {/* ✨ Tip Card */}
            <div className="
              bg-white rounded-2xl border border-yellow-200 shadow-md px-5 py-4
            ">
              <p className="text-xs font-bold uppercase text-yellow-700 tracking-wide flex items-center gap-2 mb-1">
                <span>💡</span> Tip of the Day
              </p>
              <p className="text-sm text-slate-700 leading-relaxed">
                {daySpotlight.tip}
              </p>
            </div>

            {/* ✨ Quote Card */}
            <div className="
              bg-indigo-50 rounded-2xl border border-indigo-200 shadow-md px-5 py-4
            ">
              <p className="text-xs font-bold uppercase text-indigo-700 tracking-wide flex items-center gap-2 mb-1">
                <span>📜</span> Quote / Insight
              </p>
              <blockquote className="text-sm italic text-slate-700 leading-relaxed">
                “{daySpotlight.quote}”
              </blockquote>
            </div>

          </div>
        </div>
      </article>
    </div>
  );
}
