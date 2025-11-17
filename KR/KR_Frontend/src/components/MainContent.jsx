import React, { useState, useEffect } from "react";
import Navbar from "./Navbar";
import "../index.css";
import api from "../api";
import { useNavigate } from "react-router-dom";
import SectionTabs from "./SectionTabs";
import FreshPicksSection from "./FreshPicksSection";
import TrendingSection from "./TrendingSection";
import TopicsSection from "./TopicsSection";
import DaySpotlightSection from "./DaySpotlightSection";
import QuickEvents from "./QuickEvents";


const MainContent = () => {
  const [activeTab, setActiveTab] = useState("Leaderboard");
  const [showLeaderboard, setShowLeaderboard] = useState(false);
  const [showActivity, setShowActivity] = useState(false);
  const [showAnnouncements, setShowAnnouncements] = useState(false);
  const [activeSection, setActiveSection] = useState("freshPicks");

  const [freshPicks, setFreshPicks] = useState([]);
  const [trending, setTrending] = useState([]);
  const [topics, setTopics] = useState([]);
  const [daySpotlight, setDaySpotlight] = useState(null);
  const [allKnowledgeItems, setAllKnowledgeItems] = useState([]);
  const [leaderboardData, setLeaderboardData] = useState([]);
  const [loadingLeaderboard, setLoadingLeaderboard] = useState(false);
  const [errorLeaderboard, setErrorLeaderboard] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [userContributions, setUserContributions] = useState([]);
  const [ideathonEvent, setIdeathonEvent] = useState(null);

  const navigate = useNavigate();
  const userId = localStorage.getItem("userId");

  useEffect(() => {
    const fetchEvent = async () => {
      try {
        setLoading(true);
        const res = await api.get("/Events/type/Ideathon");
        setIdeathonEvent(res.data[0] || null);
      } catch (err) {
        console.error("Error fetching Ideathon event:", err);
        setError("Failed to load Ideathon event");
      } finally {
        setLoading(false);
      }
    };
    fetchEvent();
  }, []);

  useEffect(() => {
    const fetchUserContributions = async () => {
      try {
        const res = await api.get(`/Contributions/user/contributions/month`);
        const contributions = res.data.map(item => ({
          title: item.title,
          description: item.description
        }));
        setUserContributions(contributions);
      } catch (err) {
        console.error("Error fetching user contributions:", err);
      }
    };
    fetchUserContributions();
  }, []);

  useEffect(() => {
    let isMounted = true;
    const loadFreshPicks = async () => {
      try {
        setLoading(true);
        setError("");
        const res = await api.get("/FreshPicks?count=6");
        if (isMounted) setFreshPicks(res.data);
      } catch (err) {
        console.error("Failed to load Fresh Picks:", err);
        if (isMounted) setError("Failed to load Fresh Picks");
      } finally {
        if (isMounted) setLoading(false);
      }
    };
    loadFreshPicks();
    return () => { isMounted = false; };
  }, []);

  useEffect(() => {
    const fetchTrending = async () => {
      try {
        const res = await api.get("/Trending");
        setTrending(res.data || []);
      } catch (err) {
        console.error("Error fetching trending:", err);
      }
    };
    fetchTrending();
  }, []);

  useEffect(() => {
    const fetchTopics = async () => {
      try {
        const res = await api.get("/TopicHighlight/topics?top=10");
        setTopics(res.data);
      } catch (err) {
        console.error("Error fetching topics:", err);
      }
    };
    fetchTopics();
  }, []);

  useEffect(() => {
    const fetchAllKnowledgeItems = async () => {
      try {
        const res = await api.get("/TopicHighlight/knowledge?domain=all");
        setAllKnowledgeItems(res.data || []);
      } catch (err) {
        console.error("Error fetching knowledge items:", err);
      }
    };
    fetchAllKnowledgeItems();
  }, []);

  useEffect(() => {
    const fetchDaySpotlight = async () => {
      try {
        const res = await api.get("/DaySpotlight");
        setDaySpotlight(res.data || null);
      } catch (err) {
        console.error("Error fetching day spotlight:", err);
      }
    };
    fetchDaySpotlight();
  }, []);

  useEffect(() => {
    const fetchLeaderboard = async () => {
      try {
        setLoadingLeaderboard(true);
        setErrorLeaderboard("");
        const res = await api.get("/Engagement/top-liked?top=5");
        setLeaderboardData(res.data);
      } catch (err) {
        console.error("Failed to load leaderboard:", err);
        setErrorLeaderboard("Failed to load leaderboard");
      } finally {
        setLoadingLeaderboard(false);
      }
    };
    fetchLeaderboard();
  }, []);

  const handleLike = async (item) => {
    try {
      if (!item?.itemId) return;
      const res = await fetch(`/api/engagement/like/${item.itemId}?userId=${userId}`, { method: "POST" });
      if (!res.ok) throw new Error(await res.text());
    } catch (err) {
      console.error("Error liking item:", err);
    }
  };

  const handleComment = async (item, commentText) => {
    try {
      if (!item?.itemId || !commentText.trim()) return;
      const res = await fetch(`/api/engagement/comment/${item.itemId}?userId=${userId}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ commentText: commentText.trim() }),
      });
      if (!res.ok) throw new Error(await res.text());
    } catch (err) {
      console.error("Error commenting:", err);
    }
  };

  const handleFavourite = async (item) => {
    try {
      const res = await fetch(`/api/engagement/favourite/${item.itemId}?userId=${userId}`, { method: "POST", headers: { "Content-Type": "application/json" } });
      if (!res.ok) throw new Error("Failed to favourite item");
      return true;
    } catch (err) {
      console.error("Error favouriting item:", err);
      return false;
    }
  };

  const leaderboardNumberColors = ["bg-blue-100 text-blue-900", "bg-green-100 text-green-900", "bg-pink-100 text-pink-900"];


  return (
    <div className="flex flex-col w-full">

      {/* Navbar  */}
      <Navbar />
   

      {/* Section Tabs & Content */}
      <div className="px-6 mt-4">
        <SectionTabs activeSection={activeSection} onSectionChange={setActiveSection} />

        <div className="mt-4">
          {activeSection === "freshPicks" && (
            <FreshPicksSection
              freshPicks={freshPicks}
              handleLike={handleLike}
              handleFavourite={handleFavourite}
              handleComment={handleComment}
              userId={userId}
            />
          )}
          {activeSection === "trending" && (
            <TrendingSection
              trending={trending}
              showAllTrending={false}
              setShowAllTrending={() => {}}
              setSelectedItem={() => {}}
              setIsModalOpen={() => {}}
              handleLike={handleLike}
              handleFavourite={handleFavourite}
              handleComment={handleComment}
              loading={loading}
              error={error}
            />
          )}
          {activeSection === "topics" && topics.length > 0 && (
            <TopicsSection topics={topics} userId={userId} />
          )}
          {activeSection === "daySpotlight" && daySpotlight && (
            <DaySpotlightSection daySpotlight={daySpotlight} />
          )}
        </div>
      </div>

      {/* Quick Events  */}
      <div className="px-6 mt-2">
        <QuickEvents navigate={navigate} />
      </div>

      {/* Leaderboard / Recent Activity / Announcements */}
   <div className="bg-white rounded-2xl p-4 shadow-md mt-4">
  {/* Tabs */}
  <div className="flex justify-center mb-4 flex-wrap gap-3">
    {[
      { key: "Leaderboard", label: "Leaderboard", color: "bg-blue-100 text-blue-800 hover:bg-blue-200" },
      { key: "Recent Activity", label: "Recent Activity", color: "bg-green-100 text-green-800 hover:bg-green-200" },
      { key: "Announcements", label: "Announcements", color: "bg-pink-100 text-pink-800 hover:bg-pink-200" },
    ].map((tab) => (
      <button
        key={tab.key}
        onClick={() => setActiveTab(tab.key)}
        className={`px-3 py-1 rounded-full font-semibold text-sm transition-all ${activeTab === tab.key ? "bg-indigo-600 text-white shadow-md" : tab.color}`}
      >
        {tab.label}
      </button>
    ))}
  </div>

  {/* Tab Content */}
  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 mb-4">
    {/* Leaderboard */}
    {activeTab === "Leaderboard" &&
      (loadingLeaderboard ? (
        <p className="text-xs">Loading leaderboard...</p>
      ) : errorLeaderboard ? (
        <p className="text-red-500 text-xs">{errorLeaderboard}</p>
      ) : (showLeaderboard ? leaderboardData : leaderboardData.slice(0, 3)).map((item, index) => (
        <div key={index} className="rounded-2xl p-3 shadow-sm bg-gray-50 hover:shadow-md transition-all flex flex-col gap-1 border border-gray-100">
          <span className={`font-semibold px-2 py-1 rounded-full text-[10px] w-max ${leaderboardNumberColors[index] || "bg-gray-200 text-gray-800"}`}>
            #{index + 1}
          </span>
          <h4 className="text-base font-bold text-gray-900">{item.userName}</h4>
          <p className="text-xs text-black font-medium">{item.itemTitle}</p>
        </div>
      )))}

    {/* Recent Activity */}
    {activeTab === "Recent Activity" &&
      (showActivity ? userContributions : userContributions.slice(0, 3)).map((activity, idx) => (
        <div key={idx} className="rounded-2xl p-3 shadow-sm bg-gray-50 hover:shadow-md transition-all flex flex-col gap-1 border border-gray-100">
          <h5 className="text-base font-semibold text-blue-800">{activity.title}</h5>
          <p className="text-xs text-gray-700">{activity.description.length > 100 ? activity.description.substring(0, 100) + "..." : activity.description}</p>
        </div>
      ))}

    {/* Announcements */}
    {activeTab === "Announcements" && ideathonEvent && (
      <div className="rounded-2xl p-3 shadow-sm bg-gray-50 hover:shadow-md transition-all flex flex-col gap-1 border border-gray-100">
        <span className="font-semibold px-2 py-1 rounded-full text-[10px] w-max bg-pink-200 text-pink-900">Ideathon</span>
        <h5 className="text-base font-semibold text-pink-800">{ideathonEvent.name}</h5>
        <p className="text-xs text-gray-700">{ideathonEvent.description.length > 100 ? ideathonEvent.description.substring(0, 100) + "..." : ideathonEvent.description}</p>
      </div>
    )}
  </div>

  {/* View More Buttons */}
  <div className="flex justify-center mt-2 gap-3 flex-wrap">
    {activeTab === "Leaderboard" && (
      <button onClick={() => setShowLeaderboard(!showLeaderboard)} className="px-4 py-1 text-xs font-semibold text-gray-800 bg-gray-200 hover:bg-gray-300 rounded-lg transition-colors">
        {showLeaderboard ? "View Less" : "View More"}
      </button>
    )}
    {activeTab === "Recent Activity" && (
      <button onClick={() => setShowActivity(!showActivity)} className="px-4 py-1 text-xs font-semibold text-gray-800 bg-gray-200 hover:bg-gray-300 rounded-lg transition-colors">
        {showActivity ? "View Less" : "View More"}
      </button>
    )}
    {activeTab === "Announcements" && (
      <button onClick={() => setShowAnnouncements(!showAnnouncements)} className="px-4 py-1 text-xs font-semibold text-gray-800 bg-gray-200 hover:bg-gray-300 rounded-lg transition-colors">
        {showAnnouncements ? "View Less" : "View More"}
      </button>
    )}
  </div>
</div>

    </div>
  );
};

export default MainContent;