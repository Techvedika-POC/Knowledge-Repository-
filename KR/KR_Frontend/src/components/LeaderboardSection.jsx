import React, { useEffect, useState } from "react";
import api from "../api";
import { useNavigate } from "react-router-dom";

export default function LeaderboardSection() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    let cancelled = false;

    const fetchTop3 = async () => {
      setLoading(true);
      setError("");
      try {
        const res = await api.get("/Engagement/top-liked?top=3");
        if (!cancelled) {
          const arr = Array.isArray(res.data) ? res.data : [];
          setItems(arr.slice(0, 3));
        }
      } catch (err) {
        if (!cancelled) setError("Failed to load leaderboard.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    };

    fetchTop3();
    return () => (cancelled = true);
  }, []);

  if (loading) return <div className="text-xs text-gray-500">Loading leaderboard…</div>;
  if (error) return <div className="text-xs text-red-500">{error}</div>;
  if (!items.length) return <div className="text-xs text-gray-500">No leaderboard data.</div>;

  return (
    <section className="w-full">
      {/* Centered Heading */}
      <h3 className="text-2xl font-bold mb-8 text-blue-900 text-center tracking-wide">
        Leaderboard
      </h3>

      {/* Premium Circular Layout */}
      <div className="flex justify-center gap-12 flex-wrap">
        {items.map((it, index) => {
          const imgSrc =
            it.thumbnail || it.imageUrl || it.itemImage || `/assets/Rank${index + 1}.png`;

          return (
            <div
              key={index}
              className="flex flex-col items-center cursor-pointer group"
              onClick={() => {
                if (it.itemId) navigate(`/app/knowledge/${it.itemId}`);
                else if (it.userId) navigate(`/app/profile/${it.userId}`);
              }}
            >
              {/* Circle Wrapper */}
              <div className="relative">
                {/* Rank Badge */}
                <div className="absolute -top-2 -right-2 bg-indigo-600 text-white text-xs font-semibold rounded-full px-2 py-1 shadow-md">
                  #{index + 1}
                </div>

                {/* Circle Image */}
                <div className="w-36 h-36 rounded-full bg-white shadow-xl border-4 border-indigo-200 overflow-hidden flex items-center justify-center group-hover:border-indigo-500 transition-all">
                  <img
                    src={imgSrc}
                    alt="rank"
                    className="w-full h-full object-cover"
                    onError={(e) => {
                      e.currentTarget.src = `/assets/Rank${index + 1}.png`;
                    }}
                  />
                </div>
              </div>

              {/* Name */}
              <div
                className="mt-3 text-base font-semibold text-gray-900 text-center w-36 truncate"
                title={it.userName || it.itemTitle}
              >
                {it.userName || it.itemTitle || "Unknown"}
              </div>
            </div>
          );
        })}
      </div>
    </section>
  );
}
