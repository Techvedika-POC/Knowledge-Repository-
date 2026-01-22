import React, { useEffect, useState } from "react";
import api from "../api";
import KnowledgeCardsDisplay from "../components/KnowledgeCardsDisplay";
import PreviewModal from "../components/PreviewModal";
import { Sparkles, Flag, CheckCircle2 } from "lucide-react";

export default function EventKnowledgeItemsPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedItem, setSelectedItem] = useState(null);

  const [userId] = useState(localStorage.getItem("userId"));

  useEffect(() => {
    const fetchEventItems = async () => {
      setLoading(true);
      setError("");
      try {
        const res = await api.get("/KnowledgeItem/event-items/approved");
        const data = res.data || [];

        const mapped = data.map((k) => ({
          ...k,
          id: k.itemId || k.id,
          title: k.title,
          description: k.description,
          tags: k.tags || [],
          ownerName: k.ownerName || k.submittedBy || "Unknown",
        }));

        setItems(mapped);
      } catch (err) {
        console.error("Failed to load approved event items", err);
        setError("Failed to load event-related knowledge articles.");
      } finally {
        setLoading(false);
      }
    };

    fetchEventItems();
  }, []);

  const totalItems = items.length;

  return (
    <div className="min-h-screen w-full bg-slate-50 px-6 py-6">
      {/* Top hero / header */}
      <div className="max-w-6xl mx-auto mb-6">
        <div className="rounded-3xl border border-slate-200 bg-gradient-to-r from-indigo-50 via-sky-50 to-emerald-50 px-6 py-5 shadow-sm">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            {/* Left: title + copy */}
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-3 py-1 text-[15px] font-medium text-emerald-700 border border-emerald-200 mb-2">
                <Sparkles className="w-3.5 h-3.5 text-emerald-600" />
                Event Knowledge Stream
              </div>

              <h4 className="text-xl font-extrabold leading-tight bg-gradient-to-r from-indigo-600 to-purple-600 text-transparent bg-clip-text">
                Event Knowledge Articles
              </h4>

              <p className="mt-1 text-sm text-slate-600 max-w-2xl">
                These are{" "}
                <span className="font-semibold text-indigo-600">
                  approved submissions
                </span>{" "}
                from Ideathons and internal events. Shortlisted ideas can move
                to the <span className="font-semibold">next level</span> of
                reuse, learning and implementation.
              </p>
            </div>

            {/* Right: small stats */}
            <div className="flex flex-col items-end gap-2">
              <p className="text-[11px] text-slate-500 flex items-center gap-1">
                <Flag className="w-3.5 h-3.5 text-amber-500" />
                Curated from event submissions
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Inner content card */}
      <div className="max-w-6xl mx-auto">
        {/* Section heading bar */}
        <div className="mb-4 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="w-1.5 h-6 rounded-full bg-indigo-500" />
            <div className="flex flex-col">
              <h2 className="text-sm font-semibold text-slate-800">
                Event-sourced knowledge items
              </h2>
              <p className="text-xs text-slate-500">
                Articles created from hackathons, ideathons and internal events.
              </p>
            </div>
          </div>
        </div>
        {loading && (
          <div className="rounded-2xl bg-white border border-slate-100 py-10 text-center text-slate-500 text-sm shadow-sm">
            Loading event articles…
          </div>
        )}

        {error && (
          <div className="rounded-2xl bg-white border border-red-100 py-4 px-4 text-center text-red-500 text-sm shadow-sm">
            {error}
          </div>
        )}

        {!loading && !error && items.length === 0 && (
          <div className="rounded-2xl bg-white border border-slate-100 py-10 text-center text-slate-500 text-sm shadow-sm">
            No approved event knowledge articles found yet.
            <br />
            <span className="text-[11px] text-slate-400">
              Once event submissions are approved, they will appear here.
            </span>
          </div>
        )}
        {!loading && !error && items.length > 0 && (
          <div className="rounded-3xl bg-white border border-slate-100 shadow-sm mt-3">
            <div className="px-4 pt-4 pb-2 flex items-center justify-between">
              <p className="text-xs text-slate-500">
                These cards represent{" "}
                <span className="font-medium text-slate-800">
                  event-derived contributions
                </span>{" "}
                that are ready to be shared across the organization.
              </p>
            </div>

            <div className="border-t border-slate-100 mt-1">
              <KnowledgeCardsDisplay
                items={items}
                userId={userId}
                onPreview={(item) => setSelectedItem(item)}
              />
            </div>
          </div>
        )}
      </div>

      {/* global PreviewModal */}
      {selectedItem && (
        <PreviewModal
          item={selectedItem}
          onClose={() => setSelectedItem(null)}
        />
      )}
    </div>
  );
}
