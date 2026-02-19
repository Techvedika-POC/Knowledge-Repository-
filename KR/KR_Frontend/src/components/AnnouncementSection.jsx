import React, { useEffect, useState } from "react";
import api from "../api";
import {
  Megaphone,
  Calendar,
  Flag,
  CheckCircle,
  Presentation,
  Trophy,
  Tag
} from "lucide-react";
import { useNavigate } from "react-router-dom";
const EVENT_TABS = [
  { key: "Ideathon", label: "Ideathons", icon: Tag },
  { key: "Hackathon", label: "Hackathons", icon: Flag },
  { key: "Coding Challenge", label: "Coding Challenges", icon: CheckCircle }
];
export default function AnnouncementSection() {
  const [events, setEvents] = useState([]);
  const [activeTab, setActiveTab] = useState("Ideathon");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchActiveEvents = async () => {
      setLoading(true);
      try {
        const res = await api.get("/Events/active");
        setEvents(res?.data || []);
      } catch {
        setEvents([]);
      } finally {
        setLoading(false);
      }
    };

    fetchActiveEvents();
  }, []);

  const grouped = EVENT_TABS.reduce((acc, t) => {
    acc[t.key] = events.filter(e => e.eventType === t.key);
    return acc;
  }, {});
  const navigate = useNavigate();
  return (
    <section className="mt-1 mb-0">
      <div className="flex justify-center">
        <div
          className="
          w-full max-w-6xl
          px-1
          rounded-3xl
          bg-gradient-to-br from-indigo-50 via-white to-sky-50
        "
        >
          <div className="px-5 py-4">
            <div className="flex items-center justify-between mb-1">
              <div className="flex px-2 items-center gap-4">
                <div className="bg-indigo-500 text-white p-2 rounded-2xl shadow-lg">
                  <Megaphone size={24} />
                </div>

                <div>
                  <h2 className="text-2xl font-bold text-slate-900">
                    Live Event Announcements
                  </h2>
                  <p className="text-sm text-slate-600">
                    Ongoing programs & opportunities you can join now
                  </p>
                </div>
              </div>

              <div className="
              hidden md:flex items-center gap-2
              px-4 py-2 rounded-full
              bg-indigo-100 text-indigo-600
              text-sm font-semibold
            ">
                {events.length} Active Events
              </div>
            </div>
            <div className="
   mb-1
   py-1.5 px-2 rounded-4xl
">
              <div className="grid grid-cols-5 gap-1">
                {EVENT_TABS.map(tab => {
                  const isActive = activeTab === tab.key;
                  const Icon = tab.icon;

                  return (
                    <button
                      key={tab.key}
                      onClick={() => setActiveTab(tab.key)}
                      className={`
  flex items-center justify-center gap-2
  px-4 py-1.5 rounded-xl
  text-sm font-semibold
  transition-all duration-200 ease-out

  ${isActive
                          ? `
        bg-indigo-600
        text-white
        shadow-md
        ring-1 ring-indigo-400/30
      `
                          : `
        bg-white
        text-slate-600
        border border-slate-200
        hover:bg-indigo-50
        hover:text-indigo-700
        hover:border-indigo-300
      `
                        }
`}>
                      <Icon size={16} />
                      {tab.label}
                    </button>
                  );
                })}
              </div>
            </div>
            {loading ? (
              <div className="flex items-center gap-3 bg-white p-5 rounded-2xl text-sm text-slate-500">
                <Calendar className="animate-pulse" size={18} />
                Loading active events…
              </div>
            ) : grouped[activeTab]?.length ? (
              <div className="space-y-6">
                {grouped[activeTab].map(ev => (
                  <AnnouncementCard key={ev.eventId} event={ev} />
                ))}
              </div>
            ) : (
              <div className="flex items-center gap-3 bg-white p-6 rounded-2xl text-sm text-slate-500">
                <Megaphone size={18} />
                No active {activeTab.toLowerCase()} events right now.
              </div>
            )}
          </div>
        </div>
      </div>
    </section>
  );

}

function AnnouncementCard({ event }) {
  const [expanded, setExpanded] = useState(false);
  const format = d => d ? new Date(d).toLocaleDateString() : "-";

  return (
    <div className="
      bg-white
      rounded-2xl px-6 py-4
      border border-slate-200
      shadow-sm hover:shadow-md
      transition
    ">
      <div className="flex flex-col md:flex-row justify-between gap-6">
        <div>
          <div className="
            inline-flex items-center gap-2
            px-3 py-1 rounded-full
            bg-indigo-100 text-indigo-700
            text-xs font-semibold mb-2
          ">
            <Megaphone size={14} />
            {event.eventType}
          </div>

          <h3 className="text-lg font-bold text-slate-900">
            {event.title}
          </h3>

          <p className="text-sm text-slate-600 mt-1 max-w-xl">
            {event.description}
          </p>

          <div className="flex flex-wrap gap-4 mt-3 text-xs text-slate-600">
            <span className="flex items-center gap-1">
              <Calendar size={14} /> Starts {format(event.startDate)}
            </span>
            <span className="flex items-center gap-1">
              <Calendar size={14} /> Ends {format(event.endDate)}
            </span>
          </div>
        </div>
        <div className="flex flex-col justify-between items-end">
          <button
            onClick={() => setExpanded(v => !v)}
            className="
              px-4 py-2 text-sm font-semibold
              rounded-xl bg-indigo-500 text-white
              hover:bg-indigo-700 transition
            "
          >
            {expanded ? "Hide Timeline" : "View Timeline"}
          </button>

          {event.contactEmail && (
            <p className="text-xs text-slate-500 mt-3">
              Contact: {event.contactEmail}
            </p>
          )}
        </div>
      </div>

      {expanded && <EnterpriseTimeline event={event} />}
    </div>
  );
}

function EnterpriseTimeline({ event }) {
  const format = d => d ? new Date(d).toLocaleDateString() : "-";
  const today = new Date();

  const items = [
    { label: "Registration Close", date: event.registrationCloseDate, icon: Calendar },
    { label: "Mentor Checkpoint", range: [event.mentorCheckpointStart, event.mentorCheckpointEnd], icon: Flag },
    { label: "Final Submission", date: event.finalSubmissionDeadline, icon: CheckCircle },
    { label: "Presentations", range: [event.ideaPresentationStart, event.ideaPresentationEnd], icon: Presentation },
    { label: "Winners Announcement", date: event.winnersAnnouncementDate, icon: Trophy }
  ];

  const getStatus = (item) => {
    if (item.range?.[0] && item.range?.[1]) {
      const start = new Date(item.range[0]);
      const end = new Date(item.range[1]);
      if (today < start) return "future";
      if (today > end) return "done";
      return "active";
    }

    if (item.date) {
      const d = new Date(item.date);
      if (d.toDateString() === today.toDateString()) return "active";
      if (d < today) return "done";
      return "future";
    }

    return "future";
  };

  return (
    <div className="mt-2 pt-2 px-6 mb-2 border-t bg-slate-50 rounded-b-2xl">
      <h4 className="text-sm font-bold text-slate-800 mb-4">
        Event Timeline
      </h4>

      <div className="flex flex-col lg:flex-row gap-2">
        {items.map((t, i) => {
          const Icon = t.icon;
          const status = getStatus(t);

          const isDone = status === "done";
          const isActive = status === "active";
          const isFuture = status === "future";

          return (
            <div
              key={i}
              className={`
                relative flex-1 border rounded-xl p-4 transition-all
                ${isDone && "bg-green-50 border-green-200"}
                ${isActive && `
                  bg-gradient-to-br from-indigo-50 via-white to-indigo-100
                  border-indigo-500 shadow-lg ring-2 ring-indigo-200
                `}
                ${isFuture && "bg-amber-50 border-amber-200"}
              `}
            >
              <div className="absolute -top-3 left-4">
                {isDone && (
                  <span className="
                    px-2.5 py-1 rounded-full text-[11px] font-semibold
                    bg-gradient-to-r from-green-500 to-emerald-500
                    text-white shadow-sm
                  ">
                    Completed
                  </span>
                )}

                {isActive && (
                  <span className="
                    px-2.5 py-1 rounded-full text-[11px] font-semibold
                    bg-gradient-to-r from-indigo-600 to-blue-500
                    text-white shadow-md animate-pulse
                  ">
                    Ongoing
                  </span>
                )}

                {isFuture && (
                  <span className="
                    px-2.5 py-1 rounded-full text-[11px] font-semibold
                    bg-gradient-to-r from-amber-400 to-orange-400
                    text-white shadow-sm
                  ">
                    Upcoming
                  </span>
                )}
              </div>
              <div
                className={`
                  rounded-lg p-2 inline-flex mt-3 text-white
                  ${isDone && "bg-green-600"}
                  ${isActive && "bg-indigo-600"}
                  ${isFuture && "bg-amber-500"}
                `}
              >
                <Icon size={18} />
              </div>
              <p className="font-semibold text-sm text-slate-800">
                {t.label}
              </p>

              {t.date && (
                <p className="text-xs text-slate-600 mt-1">
                  {format(t.date)}
                </p>
              )}

              {t.range && (
                <p className="text-xs text-slate-600 mt-1">
                  {format(t.range[0])} — {format(t.range[1])}
                </p>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}



