import React, { useState, useEffect } from "react";
import {
    FaFilter,
    FaCalendarAlt,
    FaFolderOpen,
    FaLayerGroup,
    FaRobot,
    FaSearch,
} from "react-icons/fa";
import { Lightbulb, Zap, Code, BookOpen } from "lucide-react";
import Navbar from "./Navbar";
import "../index.css";
import axios from "axios";
import api from "../api";
import { Swiper, SwiperSlide } from "swiper/react";
import { Navigation, Pagination, Autoplay } from "swiper/modules";
import "swiper/css";
import "swiper/css/navigation";
import "swiper/css/pagination";
import { useNavigate } from "react-router-dom";
import SectionTabs from "./SectionTabs";
import FreshPicksSection from "./FreshPicksSection";
import TrendingSection from "./TrendingSection";
import TopicsSection from "./TopicsSection";
import Navbar from "./Navbar";
import DaySpotlightSection from "./DaySpotlightSection";
import { useNavigate } from "react-router-dom";
import QuickEvents from "./QuickEvents";
import AppHighlights from "./AppHighlights";

const MainContent = () => {
    const quickEvents = [
        { name: "Ideathons", icon: <Lightbulb className="w-5 h-5 text-blue-500" />, path: "/app/events/ideathon" },
        { name: "Hackathons", icon: <Zap className="w-5 h-5 text-red-500" />, path: "/app/events/hackathon" },
        { name: "Coding Challenges", icon: <Code className="w-5 h-5 text-green-500" />, path: "/app/events/coding-challenge" },
        { name: "Knowledge Quest", icon: <BookOpen className="w-5 h-5 text-purple-500" />, path: "/app/events/knowledge-quest" },
    ];
    const [activeTab, setActiveTab] = useState("Leaderboard");
    const [showAllItems, setShowAllItems] = useState(false);
    const [freshPicks, setFreshPicks] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [domains, setDomains] = useState([]);
    const [selectedDomain, setSelectedDomain] = useState("");
    const [keyword, setKeyword] = useState("");
    const [searchResults, setSearchResults] = useState([]);
    const [isSearching, setIsSearching] = useState(false);
  const [showLeaderboard, setShowLeaderboard] = useState(false);
  const [showActivity, setShowActivity] = useState(false);
  const [showAnnouncements, setShowAnnouncements] = useState(false);
    const [activeSection, setActiveSection] = useState("freshPicks");

  const [freshPicks, setFreshPicks] = useState([]);
    const [trending, setTrending] = useState([]);
    const [topics, setTopics] = useState([]);
    const [daySpotlight, setDaySpotlight] = useState(null);
    const [domainKnowledgeItems, setDomainKnowledgeItems] = useState([]);
    const [selectedItem, setSelectedItem] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [allKnowledgeItems, setAllKnowledgeItems] = useState([]);
    const [showAllFreshPicks, setShowAllFreshPicks] = useState(false);
    const [showAllTrending, setShowAllTrending] = useState(false);
    const [showLeaderboard, setShowLeaderboard] = useState(false);
    const [showActivity, setShowActivity] = useState(false);
    const [showAnnouncements, setShowAnnouncements] = useState(false);
    const [isTopicPopupOpen, setIsTopicPopupOpen] = React.useState(false);
    const [currentPage, setCurrentPage] = useState(null);
  const [leaderboardData, setLeaderboardData] = useState([]);
  const [loadingLeaderboard, setLoadingLeaderboard] = useState(false);
  const [errorLeaderboard, setErrorLeaderboard] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [userContributions, setUserContributions] = useState([]);
  const [ideathonEvent, setIdeathonEvent] = useState(null);

    const navigate = useNavigate();
    const userId = localStorage.getItem("userId");

    const handleTopicClick = async (topicName) => {
        setSelectedDomain(topicName);

  // ------------------- API Calls -------------------
  useEffect(() => {
    const fetchEvent = async () => {
        try {
            const res = await api.get(`/TopicHighlight/knowledge`, {
                params: { domain: topicName, top: 10 },
            });

            setDomainKnowledgeItems(res.data || []);
            setIsTopicPopupOpen(true);
        setLoading(true);
        const res = await api.get("/Events/type/Ideathon");
        setIdeathonEvent(res.data[0] || null);
        } catch (err) {
            console.error("Error fetching knowledge items:", err);
            setDomainKnowledgeItems([]);
        console.error("Error fetching Ideathon event:", err);
        setError("Failed to load Ideathon event");
      } finally {
        setLoading(false);
        }
    };
    fetchEvent();
  }, []);

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

  useEffect(() => {
    const fetchUserContributions = async () => {
        try {
            const response = await api.get(`/GlobalSearch`, {
                params: { keyword },
            });

            if (response.data?.length > 0) {
                setSearchResults(response.data);
            } else {
                setError("No matching results found.");
            }
        const res = await api.get(`/Contributions/user/contributions/month`);
        const contributions = res.data.map(item => ({
          title: item.title,
          description: item.description
        }));
        setUserContributions(contributions);
        } catch (err) {
            console.error("Search API error:", err);
            setError(
                err.response
                    ? `Error: ${err.response.status} ${err.response.statusText}`
                    : "Something went wrong. Please try again."
            );
        } finally {
            setIsSearching(false);
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
        return () => {
            isMounted = false;
        };
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
                const response = await axios.get("https://localhost:7098/api/TopicHighlight/topics?top=10");
                setTopics(response.data);
            } catch (error) {
                console.error("Error fetching topics:", error);
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
                const res = await axios.get("https://localhost:7098/api/TopicHighlight/knowledge?domain=all");
        const res = await api.get("/TopicHighlight/knowledge?domain=all");
                setAllKnowledgeItems(res.data || []);
            } catch (error) {
                console.error("Error fetching knowledge items:", error);
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
        let isMounted = true;
        const fetchDomains = async () => {
    const fetchLeaderboard = async () => {
            try {
        setLoadingLeaderboard(true);
        setErrorLeaderboard("");
        const res = await api.get("/Engagement/top-liked?top=5");
        setLeaderboardData(res.data);
                const res = await api.get("/Domains");
                if (isMounted) setDomains(res.data);
            } catch (err) {
        console.error("Failed to load leaderboard:", err);
        setErrorLeaderboard("Failed to load leaderboard");
      } finally {
        setLoadingLeaderboard(false);
                console.error("Error fetching domains:", err);
            }
        };
        fetchDomains();
        return () => {
            isMounted = false;
        };
    fetchLeaderboard();
    }, []);

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
  // ------------------- Engagement Handlers -------------------
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
            const response = await fetch(
                `/api/engagement/favourite/${item.itemId}?userId=${userId}`,
        {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                }
            );

            if (!response.ok) throw new Error("Failed to favourite item");

            const data = await response.json();
            console.log("Favourite API response:", data);
            return true; // success
        } catch (error) {
            console.error("Error favouriting item:", error);
            return false; // failed
        }
    };

  const leaderboardNumberColors = ["bg-blue-100 text-blue-900", "bg-green-100 text-green-900", "bg-pink-100 text-pink-900"];

  // ------------------- JSX -------------------
    return (
        <div className="flex flex-col gap-7 w-full p-2">
            <div className="mt-2">
    <div className="flex flex-col w-full">

      {/* Navbar + Highlights tightly stacked */}
                <Navbar />
      <AppHighlights />

      {/* Section Tabs & Content */}
      <div className="px-6 mt-4">
                <SectionTabs activeSection={activeSection} onSectionChange={setActiveSection} />

                <div className="mt-4">
                    {/* Fresh Picks Section */}
                    {activeSection === "freshPicks" && (
                        <FreshPicksSection
                            freshPicks={freshPicks}
                            userId={userId} // only pass userId
              handleLike={handleLike}
              handleFavourite={handleFavourite}
              handleComment={handleComment}
              userId={userId}
                        />
                    )}

                    {/* Trending Section */}
                    {activeSection === "trending" && (
                        <TrendingSection
                            trending={trending}
                            showAllTrending={showAllTrending}
                            setShowAllTrending={setShowAllTrending}
                            setSelectedItem={setSelectedItem}
                            setIsModalOpen={setIsModalOpen}
                            userId={userId} // pass userId only
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

                    {/* Topics Section */}
                    {activeSection === "topics" && topics.length > 0 && (
                        <TopicsSection
                            topics={topics}
                            userId={userId} // pass userId only
                        />
            <TopicsSection topics={topics} userId={userId} />
                    )}

                    {/* Day Spotlight Section */}
                    {activeSection === "daySpotlight" && daySpotlight && (
                        <DaySpotlightSection daySpotlight={daySpotlight} />
                    )}
                </div>
      </div>

      {/* Quick Events immediately below content */}
      <div className="px-6 mt-2">
        <QuickEvents navigate={navigate} />
            </div>

      {/* Leaderboard / Recent Activity / Announcements */}
      <div className="bg-white rounded-2xl p-6 shadow-md mt-4">

        {/* Tabs */}
        <div className="flex justify-center mb-6 flex-wrap gap-4">
          {[
            { key: "Leaderboard", label: "Leaderboard", color: "bg-blue-100 text-blue-800 hover:bg-blue-200" },
            { key: "Recent Activity", label: "Recent Activity", color: "bg-green-100 text-green-800 hover:bg-green-200" },
            { key: "Announcements", label: "Announcements", color: "bg-pink-100 text-pink-800 hover:bg-pink-200" },
          ].map((tab) => (
                            <button
              key={tab.key}
              onClick={() => setActiveTab(tab.key)}
              className={`px-5 py-2 rounded-full font-semibold text-lg transition-all ${activeTab === tab.key ? "bg-indigo-600 text-white shadow-md" : tab.color}`}
                            >
              {tab.label}
                            </button>
                        ))}
                    </div>

        {/* Tab Content */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 mb-6">
          {/* Leaderboard */}
          {activeTab === "Leaderboard" &&
            (loadingLeaderboard ? (
              <p>Loading leaderboard...</p>
            ) : errorLeaderboard ? (
              <p className="text-red-500">{errorLeaderboard}</p>
            ) : (showLeaderboard ? leaderboardData : leaderboardData.slice(0, 3)).map((item, index) => (
              <div key={index} className="rounded-2xl p-5 shadow-sm bg-gray-50 hover:shadow-md transition-all flex flex-col gap-3 border border-gray-100">
                <span className={`font-semibold px-3 py-1 rounded-full text-sm w-max ${leaderboardNumberColors[index] || "bg-gray-200 text-gray-800"}`}>
                  #{index + 1}
                </span>
                <h4 className="text-2xl font-bold text-gray-900">{item.userName}</h4>
                <p className="text-md text-black font-medium">{item.itemTitle}</p>
              </div>
            )))}

          {/* Recent Activity */}
          {activeTab === "Recent Activity" &&
            (showActivity ? userContributions : userContributions.slice(0, 3)).map((activity, idx) => (
              <div key={idx} className="rounded-2xl p-5 shadow-sm bg-gray-50 hover:shadow-md transition-all flex flex-col gap-2 border border-gray-100">
                <h5 className="text-2xl font-semibold text-blue-800">{activity.title}</h5>
                <p className="text-md text-gray-700">{activity.description.length > 100 ? activity.description.substring(0, 100) + "..." : activity.description}</p>
                </div>
            ))}

          {/* Announcements */}
          {activeTab === "Announcements" && ideathonEvent && (
            <div className="rounded-2xl p-5 shadow-sm bg-gray-50 hover:shadow-md transition-all flex flex-col gap-2 border border-gray-100">
              <span className="font-semibold px-3 py-1 rounded-full text-sm w-max bg-pink-200 text-pink-900">Ideathon</span>
              <h5 className="text-2xl font-semibold text-pink-800">{ideathonEvent.name}</h5>
              <p className="text-md text-gray-700">{ideathonEvent.description.length > 100 ? ideathonEvent.description.substring(0, 100) + "..." : ideathonEvent.description}</p>
            </div>
            {/* Leaderboard */}
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

        {/* View More Buttons */}
        <div className="flex justify-center mt-2 gap-4 flex-wrap">
                    {activeTab === "Leaderboard" && (
            <button onClick={() => setShowLeaderboard(!showLeaderboard)} className="px-5 py-2 text-sm font-semibold text-gray-800 bg-gray-200 hover:bg-gray-300 rounded-lg transition-colors">
                            {showLeaderboard ? "View Less" : "View More"}
                        </button>
                    )}
                    {activeTab === "Recent Activity" && (
            <button onClick={() => setShowActivity(!showActivity)} className="px-5 py-2 text-sm font-semibold text-gray-800 bg-gray-200 hover:bg-gray-300 rounded-lg transition-colors">
                            {showActivity ? "View Less" : "View More"}
                        </button>
                    )}
                    {activeTab === "Announcements" && (
            <button onClick={() => setShowAnnouncements(!showAnnouncements)} className="px-5 py-2 text-sm font-semibold text-gray-800 bg-gray-200 hover:bg-gray-300 rounded-lg transition-colors">
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
        </div >
    );
};

export default MainContent;
