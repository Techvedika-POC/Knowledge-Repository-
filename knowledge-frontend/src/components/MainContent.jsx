import React, { useState,useEffect } from "react";
import {
  FaFilter,
  FaCalendarAlt,
  FaFolderOpen,
  FaLayerGroup,
  FaRobot,
  FaSearch,
} from "react-icons/fa";
import { Lightbulb, Zap, Code, BookOpen } from "lucide-react";
import "../index.css";
import axios from "axios";
import api from "../api"; 


const MainContent = () => {
  const highlightPills = [
    { label: "Fresh Picks", color: "bg-green-100 text-green-700" },
    { label: "Trending", color: "bg-blue-100 text-blue-700" },
    { label: "Topic Highlights", color: "bg-purple-100 text-purple-700" },
    { label: "Day Spotlight", color: "bg-yellow-100 text-yellow-700" },
  ];

  const highlightsData = [
    {
      title: "Zero Trust Security Playbook",
      description:
        "Architectural guidelines and reference implementations for zero trust deployments.",
      tags: [
        { label: "Security", color: "bg-blue-100 text-blue-700" },
        { label: "Architecture", color: "bg-green-100 text-green-700" },
        { label: "Best Practices", color: "bg-purple-100 text-purple-700" },
      ],
    },
    {
      title: "Designing Robust APIs",
      description:
        "Principles for versioning, schema evolution, and scaling REST/GraphQL services.",
      tags: [
        { label: "APIs", color: "bg-blue-100 text-blue-700" },
        { label: "Scalability", color: "bg-green-100 text-green-700" },
        { label: "Guides", color: "bg-purple-100 text-purple-700" },
      ],
    },
    {
      title: "Cloud Database Patterns",
      description:
        "Trade-offs across SQL/NoSQL, sharding, replication, and consistency models.",
      tags: [
        { label: "Database", color: "bg-blue-100 text-blue-700" },
        { label: "Cloud", color: "bg-green-100 text-green-700" },
        { label: "Patterns", color: "bg-yellow-100 text-yellow-700" },
      ],
    },
    {
      title: "AI Governance Framework",
      description:
        "Standards and policies for ethical AI deployment and risk management.",
      tags: [
        { label: "AI", color: "bg-pink-100 text-pink-700" },
        { label: "Governance", color: "bg-indigo-100 text-indigo-700" },
        { label: "Policy", color: "bg-yellow-100 text-yellow-700" },
      ],
    },
    {
      title: "Microservices Best Practices",
      description:
        "Design patterns and operational guidance for building scalable microservices.",
      tags: [
        { label: "Microservices", color: "bg-green-100 text-green-700" },
        { label: "Architecture", color: "bg-purple-100 text-purple-700" },
        { label: "Best Practices", color: "bg-blue-100 text-blue-700" },
      ],
    },
    {
      title: "DevOps Pipeline Strategies",
      description:
        "End-to-end approaches for CI/CD and automated delivery pipelines.",
      tags: [
        { label: "DevOps", color: "bg-red-100 text-red-700" },
        { label: "CI/CD", color: "bg-green-100 text-green-700" },
        { label: "Automation", color: "bg-yellow-100 text-yellow-700" },
      ],
    },
    {
      title: "Data Privacy Compliance",
      description:
        "Guidelines for meeting GDPR, CCPA, and other data privacy regulations.",
      tags: [
        { label: "Privacy", color: "bg-blue-100 text-blue-700" },
        { label: "Compliance", color: "bg-purple-100 text-purple-700" },
        { label: "Data", color: "bg-green-100 text-green-700" },
      ],
    },
  ];

  const quickEvents = [
    { name: "Ideathons", icon: <Lightbulb className="w-5 h-5 text-blue-500" /> },
    { name: "Hackathons", icon: <Zap className="w-5 h-5 text-red-500" /> },
    { name: "Coding Challenges", icon: <Code className="w-5 h-5 text-green-500" /> },
    { name: "Knowledge Quest", icon: <BookOpen className="w-5 h-5 text-purple-500" /> },
  ];

  const leaderboardData = [
    {
      title: "Top Contributor",
      name: "Ava Patel",
      description: "Highest ranker this week with API governance patterns.",
      rank: 1,
      bgColor: "#FFE8D6",
      badgeColor: "bg-yellow-400",
    },
    {
      title: "Rising Star",
      name: "Leo Marti",
      description: "Distributed tracing deep-dives and examples.",
      rank: 2,
      bgColor: "#E6F0FF",
      badgeColor: "bg-orange-400",
    },
    {
      title: "Community MVP",
      name: "Mei Chen",
      description: "Data contracts and governance insights.",
      rank: 3,
      bgColor: "#F3E8FF",
      badgeColor: "bg-purple-400",
    },
    {
      title: "Knowledge Curator",
      name: "Omar Ali",
      description: "Microservices handbooks and examples.",
      rank: 4,
      bgColor: "#E9FCE9",
      badgeColor: "bg-teal-400",
    },
  ];

  const recentActivityData = [
    {
      user: "Ava Patel",
      activity: "Commented on 'Zero Trust Security Playbook'",
      time: "2 hours ago",
      bgColor: "#F0F9FF",
      badgeColor: "bg-blue-400",
    },
    {
      user: "Leo Marti",
      activity: "Uploaded 'Distributed Tracing Patterns'",
      time: "5 hours ago",
      bgColor: "#FFF7ED",
      badgeColor: "bg-orange-400",
    },
    {
      user: "Mei Chen",
      activity: "Liked 'Cloud Database Patterns'",
      time: "1 day ago",
      bgColor: "#F9F5FF",
      badgeColor: "bg-purple-400",
    },
    {
      user: "Omar Ali",
      activity: "Started 'Microservices Handbook'",
      time: "2 days ago",
      bgColor: "#ECFDF5",
      badgeColor: "bg-teal-400",
    },
  ];

  const announcementsData = [
    {
      title: "Platform Maintenance",
      description: "Scheduled maintenance on Oct 5th from 2 AM to 4 AM UTC.",
      date: "Sep 28, 2025",
      bgColor: "#FEF3C7",
      badgeColor: "bg-yellow-400",
    },
    {
      title: "New API Released",
      description: "Version 2.1 of the Knowledge API is now live.",
      date: "Sep 27, 2025",
      bgColor: "#EFF6FF",
      badgeColor: "bg-blue-400",
    },
    {
      title: "Hackathon Announcement",
      description: "Join our upcoming hackathon starting Oct 10th.",
      date: "Sep 25, 2025",
      bgColor: "#FDE8F6",
      badgeColor: "bg-pink-400",
    },
    {
      title: "Feature Update",
      description: "Improved search functionality now available.",
      date: "Sep 24, 2025",
      bgColor: "#ECFDF5",
      badgeColor: "bg-green-400",
    },
  ];

  const [showAllCards, setShowAllCards] = useState(false);
  const [activeTab, setActiveTab] = useState("Leaderboard");
  const [showLeaderboard, setShowLeaderboard] = useState(false);
  const [showActivity, setShowActivity] = useState(false);
  const [showAnnouncements, setShowAnnouncements] = useState(false);
    // Search state
    const [domains, setDomains] = useState([]);
const [selectedDomain, setSelectedDomain] = useState("");
  const [keyword, setKeyword] = useState("");
  const [searchResults, setSearchResults] = useState([]);
  const [isSearching, setIsSearching] = useState(false);
  const [error, setError] = useState("");

   const handleSearch = async (e) => {
  e.preventDefault();

  if (!keyword.trim()) {
    setError("Please enter a search keyword.");
    setSearchResults([]);
    return;
  }

  setIsSearching(true);
  setError("");
  setSearchResults([]);

  try {
    const response = await api.get(`/GlobalSearch`, {
      params: { keyword }, // cleaner way to pass query
    });

    if (response.data && response.data.length > 0) {
      setSearchResults(response.data);
    } else {
      setError("No matching results found.");
    }
  } catch (err) {
    console.error("Search API error:", err);

    if (err.response) {
      if (err.response.status === 404) {
        setError("No matching results found.");
      } else {
        setError(`Error: ${err.response.status} ${err.response.statusText}`);
      }
    } else if (err.request) {
      setError("No response from server. Please check if backend is running.");
    } else {
      setError("Something went wrong. Please try again.");
    }
  } finally {
    setIsSearching(false);
  }
};
useEffect(() => {
  const fetchDomains = async () => {
    try {
      const res = await api.get("/Domains");
      setDomains(res.data);
    } catch (err) {
      console.error("Error fetching domains:", err);
    }
  };
  fetchDomains();
}, []);
const handleDomainChange = async (e) => {
  const domainName = e.target.value;
  setSelectedDomain(domainName);

  if (!domainName) {
    setSearchResults([]);
    return;
  }

  setIsSearching(true);
  setError("");
  setSearchResults([]);

  try {
    const res = await api.get(`/GlobalSearch`, {
      params: { keyword: domainName }, // domain filter works like keyword search
    });

    if (res.data && res.data.length > 0) {
      setSearchResults(res.data);
    } else {
      setError("No results found for this domain.");
    }
  } catch (err) {
    console.error("Domain search error:", err);
    setError("Something went wrong. Please try again.");
  } finally {
    setIsSearching(false);
  }
};
  return (
    <div className="flex flex-col gap-7 w-full p-2">
      {/* Header */}
      <div className="relative flex justify-center items-center p-2">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900">
            KnowLedger Synaptix
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            Innovate • Collaborate • Build
          </p>
        </div>
      </div>

       {/* Search Bar */}
      <form onSubmit={handleSearch} className="relative w-full">
        <FaSearch className="absolute left-4 top-1/2 transform -translate-y-1/2 text-violet-500" />
        <input
          type="text"
          placeholder="Search knowledge"
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          className="w-full border border-gray-200 rounded-full px-12 py-3 shadow-inner focus:outline-none focus:ring-2 focus:ring-violet-400 text-violet-600 placeholder-violet-500 bg-white"
        />
      </form>

   {/* Search Results */}
{isSearching && <p className="text-gray-500 text-sm mt-2">🔍 Searching...</p>}
{error && <p className="text-red-500 text-sm mt-2">{error}</p>}

{searchResults.length > 0 && (
  <div className="bg-gradient-to-r from-blue-100 via-purple-100 to-pink-100 rounded-xl p-6 shadow-md flex flex-col gap-4">
    <h3 className="text-xl font-bold text-gray-800 border-b border-gray-300 pb-2">
      Search Results
    </h3>
    <div className="flex flex-wrap gap-6 justify-start">
      {searchResults.map((item, idx) => (
        <div
          key={idx}
          className="bg-white rounded-2xl shadow-md hover:shadow-lg p-5 flex flex-col w-full sm:w-[48%] lg:w-[30%] transition-transform transform hover:-translate-y-1"
        >
          <h4 className="text-lg font-bold text-indigo-700 mb-2">
            {item.title || item.name}
          </h4>
          <p className="text-sm text-gray-600 mb-3 line-clamp-3">
            {item.description || item.snippet}
          </p>
          <div className="flex gap-4 text-sm mt-auto">
            <button className="px-3 py-1 rounded-lg bg-indigo-100 text-indigo-700 hover:bg-indigo-200 transition">
              👍 Like
            </button>
            <button className="px-3 py-1 rounded-lg bg-pink-100 text-pink-700 hover:bg-pink-200 transition">
              💬 Comment
            </button>
          </div>
        </div>
      ))}
    </div>
  </div>
)}


      {/* Filters + Ask Assistant */}
      <div className="bg-white rounded-xl p-4 shadow flex flex-col lg:flex-row gap-6 items-center">
        {/* Filters Section */}
        <div className="flex flex-wrap gap-3 items-center flex-1">
          <button className="px-4 py-2 rounded-full bg-cyan-100 text-cyan-700 text-sm flex items-center gap-2 hover:bg-cyan-200 transition">
            <FaLayerGroup /> Domain
          </button>
          <button className="px-4 py-2 rounded-full bg-blue-100 text-blue-700 text-sm flex items-center gap-2 hover:bg-blue-200 transition">
            <FaFolderOpen /> Category
          </button>
          <button className="px-4 py-2 rounded-full bg-gray-100 text-gray-700 text-sm flex items-center gap-2 hover:bg-gray-200 transition">
            <FaCalendarAlt /> Date
          </button>
          <button className="px-4 py-2 rounded-full bg-indigo-100 text-indigo-700 text-sm flex items-center gap-2 hover:bg-indigo-200 transition">
            <FaFilter /> Browse All
          </button>
        </div>

        {/* Ask Assistant Section */}
        <div className="flex-none w-full lg:w-80">
          <div className="bg-white rounded-xl p-4 flex flex-col gap-3 border border-gray-100">
            {/* Ask Anything Title */}
            <div className="flex items-center gap-2">
              <FaRobot className="text-purple-500 text-lg" />
              <p className="font-medium text-gray-700">Ask Anything</p>
            </div>

            {/* Suggestions + Ask Button */}
            <div className="flex justify-between gap-3 items-center">
              <div className="flex flex-wrap gap-2 flex-1">
                <span className="px-3 py-1 rounded-full text-sm bg-purple-100 text-purple-700 hover:border hover:border-purple-300 cursor-pointer transition">
                  Ask about Zero Trust
                </span>
                <span className="px-3 py-1 rounded-full text-sm bg-purple-100 text-purple-700 hover:border hover:border-purple-300 cursor-pointer transition">
                  Find latest API guides
                </span>
                <span className="px-3 py-1 rounded-full text-sm bg-purple-100 text-purple-700 hover:border hover:border-purple-300 cursor-pointer transition">
                  Who leads observability?
                </span>
              </div>

              <button className="bg-purple-500 text-white px-4 py-2 rounded-full text-sm font-medium hover:bg-purple-600 transition">
                Ask
              </button>
            </div>
          </div>
        </div>
      </div>


      {/* Highlights Section */}
      <div className="bg-white rounded-xl p-4 shadow flex flex-col gap-4">
        {/* Pills + View More/Less button */}
        <div className="flex flex-wrap items-center justify-between">
          <div className="flex flex-wrap gap-3">
            {highlightPills.map((pill, idx) => (
              <span
                key={idx}
                className={`px-3 py-1 rounded-full text-sm font-medium ${pill.color}`}
              >
                {pill.label}
              </span>
            ))}
          </div>

          <button
            onClick={() => setShowAllCards(!showAllCards)}
            className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm hover:bg-indigo-700 transition"
          >
            {showAllCards ? "View Less" : "View More"}
          </button>
        </div>

        {/* Cards */}
        <div className="flex flex-wrap gap-6 justify-start">
          {(showAllCards ? highlightsData : highlightsData.slice(0, 3)).map(
            (card, idx) => (
              <div
                key={idx}
                className="bg-white rounded-2xl shadow p-5 flex flex-col w-full sm:w-[48%] lg:w-[30%]"
              >
                <h4 className="text-lg font-bold text-gray-800 mb-2">
                  {card.title}
                </h4>
                <p className="text-sm text-gray-600 mb-3">
                  {card.description}
                </p>
                <div className="flex flex-wrap gap-2 mb-4">
                  {card.tags.map((tag, tIdx) => (
                    <span
                      key={tIdx}
                      className={`px-2 py-1 rounded-full text-xs ${tag.color}`}
                    >
                      {tag.label}
                    </span>
                  ))}
                </div>
                <div className="flex gap-4 text-gray-600 text-sm">
                  <button className="hover:underline">Like</button>
                  <button className="hover:underline">Dislike</button>
                  <button className="hover:underline">Comment</button>
                </div>
              </div>
            )
          )}
        </div>
      </div>

      {/* Quick Events */}
      <div className="bg-[#FFD873] rounded-2xl p-6 shadow-md mb-6">
        <h3 className="text-lg font-semibold text-[#2D2D2D] mb-4">
          Quick Events
        </h3>
        <div className="flex flex-wrap gap-4">
          {quickEvents.map((event, index) => (
            <button
              key={index}
              className="flex items-center gap-2 px-6 py-3 rounded-full text-sm font-medium text-[#2D2D2D] shadow-sm hover:shadow-md transition"
              style={{ backgroundColor: "#ffffff" }}
            >
              {event.icon}
              {event.name}
            </button>
          ))}
        </div>
      </div>

      {/* Leaderboard / Tabs Section */}
      <div className="bg-white rounded-2xl p-6 shadow-md">
        {/* Tabs + View More button */}
        <div className="flex justify-between items-center mb-6">
          <div className="flex gap-6">
            {["Leaderboard", "Recent Activity", "Announcements"].map(
              (tab) => (
                <button
                  key={tab}
                  onClick={() => setActiveTab(tab)}
                  className={`pb-2 text-sm font-semibold ${activeTab === tab
                    ? "border-b-2 border-indigo-600 text-indigo-600"
                    : "text-gray-600"
                    }`}
                >
                  {tab}
                </button>
              )
            )}
          </div>

          {/* View More / View Less for each tab */}
          {activeTab === "Leaderboard" && (
            <button
              onClick={() => setShowLeaderboard(!showLeaderboard)}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm hover:bg-indigo-700 transition"
            >
              {showLeaderboard ? "View Less" : "View More"}
            </button>
          )}
          {activeTab === "Recent Activity" && (
            <button
              onClick={() => setShowActivity(!showActivity)}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm hover:bg-indigo-700 transition"
            >
              {showActivity ? "View Less" : "View More"}
            </button>
          )}
          {activeTab === "Announcements" && (
            <button
              onClick={() => setShowAnnouncements(!showAnnouncements)}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm hover:bg-indigo-700 transition"
            >
              {showAnnouncements ? "View Less" : "View More"}
            </button>
          )}
        </div>

        {/* Tab Content */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {activeTab === "Leaderboard" &&
            (showLeaderboard
              ? leaderboardData
              : leaderboardData.slice(0, 3)
            ).map((item, index) => (
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

          {activeTab === "Recent Activity" &&
            (showActivity
              ? recentActivityData
              : recentActivityData.slice(0, 3)
            ).map((activity, idx) => (
              <div
                key={idx}
                className="rounded-2xl p-5 shadow-lg flex flex-col gap-2"
                style={{ backgroundColor: activity.bgColor }}
              >
                <span
                  className={`font-bold px-3 py-1 rounded-full text-sm w-max ${activity.badgeColor} text-white`}
                >
                  Activity
                </span>
                <p className="font-semibold">{activity.user}</p>
                <p className="text-sm text-gray-600">{activity.activity}</p>
                <span className="text-xs text-gray-500">{activity.time}</span>
              </div>
            ))}

          {activeTab === "Announcements" &&
            (showAnnouncements
              ? announcementsData
              : announcementsData.slice(0, 3)
            ).map((announcement, idx) => (
              <div
                key={idx}
                className="rounded-2xl p-5 shadow-lg flex flex-col gap-2"
                style={{ backgroundColor: announcement.bgColor }}
              >
                <span
                  className={`font-bold px-3 py-1 rounded-full text-sm w-max ${announcement.badgeColor} text-white`}
                >
                  Announcement
                </span>
                <h5 className="font-semibold">{announcement.title}</h5>
                <p className="text-sm text-gray-600">{announcement.description}</p>
                <span className="text-xs text-gray-500">{announcement.date}</span>
              </div>
            ))}
        </div>
      </div>
    </div>
  );
};

export default MainContent;
