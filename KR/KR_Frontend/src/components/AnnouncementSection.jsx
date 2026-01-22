import React, { useEffect, useState } from "react";
import api from "../api";

export default function AnnouncementSection({ showLatestOnly = true }) {
  const [ideathon, setIdeathon] = useState(null);
  const [loading, setLoading] = useState(false);
  const [expanded, setExpanded] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    const fetchAnnouncements = async () => {
      setLoading(true);
      setError("");
      try {
        const res = await api.get("/Events/type/Ideathon/current");
        if (!cancelled) {
          const list = res?.data || [];
          setIdeathon(showLatestOnly ? (list[0] || null) : list);
        }
      } catch (err) {
        console.error("Failed to load announcements:", err);
        if (!cancelled) setError("Failed to load announcements.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    };

    fetchAnnouncements();
    return () => {
      cancelled = true;
    };
  }, [showLatestOnly]);

  if (loading) {
    return (
      <div className="text-xs text-slate-500 mt-4">
        Loading announcements…
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-xs text-red-500 mt-4">
        {error}
      </div>
    );
  }

  if (!ideathon) {
    return (
      <div className="w-full mt-8">
        <h3 className="text-lg font-semibold text-slate-900 mb-4">
          Announcements
        </h3>
        <div className="rounded-3xl p-6 shadow-sm border border-slate-100 bg-slate-50 text-slate-500">
          No announcements at the moment.
        </div>
      </div>
    );
  }

  // multi-announcement mode 
  if (!showLatestOnly && Array.isArray(ideathon)) {
    return <MultiAnnouncementList events={ideathon} />;
  }

  // single announcement 
  const ev = ideathon;
  const title = ev.title ?? "New Event";
  const startDate = ev.startDate
    ? new Date(ev.startDate).toLocaleDateString(undefined, {
        year: "numeric",
        month: "short",
        day: "numeric",
      })
    : "";
  const subtitle =
    ev.notes || ev.description?.split(".")[0] || "Don’t miss out — join us!";
  const desc = ev.description || "";

  return (
    <div className="w-full mt-8">
      {/* main announcement card */}
      <article className="w-full rounded-[32px] bg-gradient-to-br from-[#EFEAFF] to-[#E3F0FF] shadow-xl border border-slate-200 px-10 py-8">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          {/* left content */}
          <div className="max-w-3xl">
            <h3 className="text-[20px] font-semibold text-sky-700 mb-6 px-5 py-1 
              bg-sky-50 rounded-full shadow-sm inline-block">
              Announcements
            </h3>

            <div className="flex items-center gap-2 text-[11px] mb-3">
              <span className="px-3 py-1 rounded-full bg-sky-50 text-sky-700 font-semibold uppercase tracking-wide">
                NEW Ideathon
              </span>
            </div>

            <h2 className="text-xl md:text-[20px] font-extrabold text-purple-700 leading-snug">
              {title}
            </h2>

            <p className="text-sm md:text-[13px] text-indigo-600 mt-2">
              {subtitle}
            </p>

            {/* short description */}
            <p className="mt-3 text-sm md:text-[13px] text-slate-700 leading-relaxed">
              {desc.length > 260 ? desc.substring(0, 260).trim() + "…" : desc}
            </p>

            {/* chips */}
            <div className="mt-4 flex flex-wrap gap-3 text-[11px]">
              {ev.contactEmail && (
                <span className="px-3 py-1 rounded-full bg-slate-50 border border-slate-200 text-slate-700">
                  {ev.contactEmail}
                </span>
              )}
              {ev.eventType && (
                <span className="px-3 py-1 rounded-full bg-slate-50 border border-slate-200 text-slate-700">
                  {ev.eventType}
                </span>
              )}
            </div>
          </div>
          <div className="flex flex-col items-end text-right text-xs text-slate-500 mt-2 md:mt-0">
            <span>Starts</span>
            <span className="mt-1 text-sm font-semibold text-slate-800">
              {startDate}
            </span>
          </div>
        </div>
        <div className="mt-6">
          <button
            onClick={() => setExpanded((open) => !open)}
            className="inline-flex items-center justify-center px-4 py-1 rounded-full text-sm font-semibold text-white bg-blue-500 hover:bg-indigo-700 shadow-md transition"
          >
            {expanded ? "Hide Details & Timeline" : "View Details & Timeline"}
          </button>
        </div>
        {expanded && <TimelineSection event={ev} />}
      </article>
    </div>
  );
}

/* -------- TIMELINE + DETAILS SECTION ---------*/

function TimelineSection({ event }) {
  const format = (d) =>
    d
      ? new Date(d).toLocaleDateString(undefined, {
          year: "numeric",
          month: "short",
          day: "numeric",
        })
      : "-";

  const items = [
    { label: "Registration closes", date: event.registrationCloseDate },
    {
      label: "Mentor checkpoint",
      range: [event.mentorCheckpointStart, event.mentorCheckpointEnd],
    },
    { label: "Final submission deadline", date: event.finalSubmissionDeadline },
    {
      label: "Idea presentations",
      range: [event.ideaPresentationStart, event.ideaPresentationEnd],
    },
    { label: "Winners announcement", date: event.winnersAnnouncementDate },
  ];

  return (
    <section className="mt-8 pt-6 border-t border-slate-200">
      {/* section heading */}
      <h3 className="text-lg md:text-xl font-bold text-slate-800 mb-4">
        Event Timeline
      </h3>

      {/* top summary row */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-5">
        <InfoCard
          label="Event duration"
          value={`${format(event.startDate)} → ${format(event.endDate)}`}
        />
        {event.contactEmail && (
          <InfoCard label="Contact" value={event.contactEmail} />
        )}
      </div>

      {/* full description (card) */}
      {event.description && (
        <InfoCard label="Description" value={event.description} />
      )}

      {/* notes (card) */}
      {event.notes && <InfoCard label="Notes" value={event.notes} />}

      {/* timeline cards row */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mt-4">
        {items.map((t, i) => (
          <TimelineCard key={i} label={t.label} date={t.date} range={t.range} />
        ))}
      </div>
    </section>
  );
}

/* -------- INFO CARD -------- */

function InfoCard({ label, value }) {
  return (
    <div className="rounded-2xl bg-gradient-to-br from-slate-50 to-slate-100 border border-slate-200 p-4 shadow-sm">
      <p className="text-sm font-semibold text-indigo-700">{label}</p>
      <p className="text-xs text-slate-700 mt-1 leading-relaxed">{value}</p>
    </div>
  );
}

/* -------- TIMELINE CARD  -------- */

function TimelineCard({ label, date, range }) {
  const format = (d) =>
    d
      ? new Date(d).toLocaleDateString(undefined, {
          year: "numeric",
          month: "short",
          day: "numeric",
        })
      : "-";

  return (
    <div
      className="
        rounded-2xl 
        p-5 
        shadow-md 
        bg-gradient-to-br from-indigo-50 to-purple-50
        border border-indigo-200
      "
    >
      {/* Label */}
      <p className="text-sm font-bold text-indigo-800 mb-1 tracking-wide">
        {label}
      </p>

      {/* Single Date */}
      {date && (
        <p className="text-xs text-slate-700 mt-1 font-medium">
          {format(date)}
        </p>
      )}

      {/* Date Range */}
      {range && (
        <p className="text-xs text-slate-700 mt-1 font-medium">
          {format(range[0])}{" "}
          <span className="text-slate-400">→</span> {format(range[1])}
        </p>
      )}
    </div>
  );
}

/* -------- multi-announcement list  -------- */

function MultiAnnouncementList({ events }) {
  return (
    <div className="w-full mt-8">
      <h3 className="text-lg font-semibold text-slate-900 mb-4">
        Announcements
      </h3>
      <div className="space-y-3">
        {events.map((ev) => (
          <div
            key={ev.eventId}
            className="p-4 bg-white rounded-2xl border border-slate-200 shadow-sm"
          >
            <p className="text-xs font-semibold text-indigo-600 mb-1">
              Announcement
            </p>
            <p className="text-sm font-semibold text-slate-900">
              {ev.title}
            </p>
            {ev.description && (
              <p className="text-[11px] text-slate-600 mt-1">
                {ev.description.substring(0, 120)}…
              </p>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
