import React, { useState, useEffect } from "react";
import api from "../api";

const LeaderboardTabs = () => {
  const [activeTab, setActiveTab] = useState("Leaderboard");
  const [leaderboardData] = useState([
    {
      title: "Top Contributor",
      name: "Ava Patel",
      description: "Highest ranker this week.",
      rank: 1,
      bgColor: "#FFE8D6",
      badgeColor: "bg-yellow-400",
    },
    {
      title: "Rising Star",
      name: "Leo Marti",
      description: "Distributed tracing deep-dives.",
      rank: 2,
      bgColor: "#E6F0FF",
      badgeColor: "bg-orange-400",
    },
  ]);

  const [recentActivity, setRecentActivity] = useState([]);
  const [displayCount, setDisplayCount] = useState(3);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const headingColor = "text-indigo-600"; // single heading color
  const borderColor = "border-gray-300"; // single light border color

  useEffect(() => {
    if (activeTab === "Recent Activity" && recentActivity.length === 0) {
      const token = localStorage.getItem("jwtToken");

      if (!token) {
        setError("Authentication required. Please login to see recent activity.");
        return;
      }

      setLoading(true);
      setError("");

      api
        .get("/contributions/my")
        .then((res) => {
          setRecentActivity(res.data || []);
        })
        .catch((err) => {
          console.error("Error fetching recent activity:", err);
          setError("Failed to load recent activity.");
        })
        .finally(() => setLoading(false));
    }
  }, [activeTab, recentActivity.length]);

  const handleViewMore = () => setDisplayCount(displayCount + 3);
  const handleViewLess = () => setDisplayCount(Math.max(3, displayCount - 3));
  const handleReset = () => setDisplayCount(3);

  return (
    <div className="bg-white rounded-2xl p-6 shadow-md">
      {/* Tabs */}
      <div className="flex justify-between items-center mt-0 mb-6">
        {["Leaderboard", "Recent Activity", "Announcements"].map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`pb-2 text-sm font-semibold ${
              activeTab === tab
                ? "border-b-2 border-indigo-600 text-indigo-600"
                : "text-gray-600"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {/* Content */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 mt-0 gap-6">
        {/* Leaderboard Tab */}
        {activeTab === "Leaderboard" &&
          leaderboardData.map((item, index) => (
            <div
              key={index}
              className="rounded-2xl p-5 shadow-lg flex flex-col gap-3"
              style={{ backgroundColor: item.bgColor }}
            >
              <span
                className={`font-bold px-3 py-1 rounded-full text-sm w-max ${item.badgeColor} text-white`}
              >
                #{item.rank}
              </span>
              <h4 className="text-lg font-bold">{item.title}</h4>
              <p className="font-medium text-gray-800">{item.name}</p>
              <p className="text-sm text-gray-600">{item.description}</p>
            </div>
          ))}

        {/* Recent Activity Tab */}
        {activeTab === "Recent Activity" && loading && (
          <p className="text-gray-500 col-span-full">Loading recent activity...</p>
        )}

        {activeTab === "Recent Activity" && !loading && error && (
          <p className="text-red-500 col-span-full">{error}</p>
        )}

        {activeTab === "Recent Activity" && !loading && !error && recentActivity.length === 0 && (
          <p className="text-gray-500 col-span-full">No recent activity found.</p>
        )}

        {activeTab === "Recent Activity" &&
          !loading &&
          !error &&
          recentActivity.slice(0, displayCount).map((activity, index) => (
            <div
              key={index}
              className={`rounded-lg p-4 shadow flex flex-col gap-2 border ${borderColor} bg-white group relative`}
            >
              <h4
                className={`text-md font-semibold truncate ${headingColor}`}
                title={activity.title}
              >
                {activity.title}
              </h4>
              <p
                className="text-sm text-gray-700 truncate"
                title={activity.description}
              >
                {activity.description}
              </p>

              {/* Tooltip */}
              <div className="absolute left-1/2 transform -translate-x-1/2 bottom-full mb-2 hidden group-hover:block w-64 bg-gray-800 text-white text-xs rounded p-2 z-10">
                {activity.description}
              </div>
            </div>
          ))}
      </div>

      {/* View More / View Less / Reset Buttons */}
      {activeTab === "Recent Activity" && recentActivity.length > 3 && (
        <div className="flex justify-center gap-2 mt-4">
          {displayCount < recentActivity.length && (
            <button
              className="px-2 py-1 text-sm text-indigo-600 rounded hover:bg-indigo-50"
              onClick={handleViewMore}
            >
              View More
            </button>
          )}
          {displayCount > 3 && (
            <button
              className="px-2 py-1 text-sm text-gray-600 rounded hover:bg-gray-100"
              onClick={handleViewLess}
            >
              View Less
            </button>
          )}
          <button
            className="px-2 py-1 text-sm text-green-600 rounded hover:bg-green-50"
            onClick={handleReset}
          >
            Reset
          </button>
        </div>
      )}
    </div>
  );
};

export default LeaderboardTabs;
