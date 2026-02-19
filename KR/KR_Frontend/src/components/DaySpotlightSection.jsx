import React, { useEffect, useState } from "react";
import api from "../api";
import { Sparkles, Lightbulb, Quote } from "lucide-react";

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
      } catch {
        setError("Failed to load Day Spotlight.");
      } finally {
        setLoading(false);
      }
    };
    fetchSpotlight();
  }, []);

  if (loading) return <p className="px-6 mt-4 text-xs text-gray-500">Loading Day Spotlight…</p>;
  if (error) return <p className="px-6 mt-4 text-xs text-red-500">{error}</p>;
  if (!daySpotlight) return null;

  return (
    <div className="px-14 mt-1 mb-2">

      <section className="
        max-w-5xl mx-auto
        rounded-xl
        overflow-hidden
        shadow-md
        border border-slate-200
        bg-gradient-to-br
        from-white
        via-indigo-50/40
        to-white
      ">
        <div className="
          flex items-center justify-between
          px-6 py-2
          border-b border-indigo-100
          bg-gradient-to-r
          from-indigo-300
          via-sky-50
          to-indigo-400
        ">
          <div className="
            flex items-center gap-2
            px-3 py-1.5
            rounded-md
            bg-gradient-to-r
            from-white
            to-indigo-100
            border border-indigo-100
            shadow-sm
          ">
            <Sparkles className="w-4 h-4 text-indigo-600" />

            <h3 className="text-sm font-semibold text-indigo-800">
              Day Spotlight
            </h3>
          </div>

        </div>
        <div className="grid md:grid-cols-3 gap-6 px-6 py-2">
          <div className="md:col-span-2 space-y-3">

            <h2 className="
              text-xl
              font-bold
              text-indigo-900
              leading-snug
            ">
              {daySpotlight.resourceTitle}
            </h2>

            <div className="flex items-center gap-2 text-xs text-slate-500">
              <span className="w-2 h-2 bg-emerald-500 rounded-full animate-pulse"/>
              Today’s focus
            </div>

            <p className="text-sm text-slate-700 leading-relaxed">
              {daySpotlight.tip}
            </p>

          </div>
          <div className="space-y-2">

            <InfoCard
              icon={Lightbulb}
              title="Tip"
              variant="tip"
            >
              {daySpotlight.tip}
            </InfoCard>

            <InfoCard
              icon={Quote}
              title="Insight"
              variant="insight"
            >
              “{daySpotlight.quote}”
            </InfoCard>

          </div>

        </div>
      </section>
    </div>
  );

function InfoCard({ icon: Icon, title, children, variant }) {

  const styles = {
    tip: `
      bg-gradient-to-r
      from-yellow-50
      to-yellow-100
      border-yellow-200
      text-yellow-700
    `,
    insight: `
      bg-gradient-to-r
      from-indigo-50
      to-indigo-100
      border-indigo-200
      text-indigo-700
    `
  };

  return (
    <div className={`
      p-2 rounded-lg border shadow-sm
      ${styles[variant]}
    `}>

      <div className="flex items-center gap-2 text-xs font-semibold uppercase mb-1">
        <Icon size={14}/>
        {title}
      </div>

      <p className="text-sm text-slate-700 leading-relaxed">
        {children}
      </p>

    </div>
  );
}

}
