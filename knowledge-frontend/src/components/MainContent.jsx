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
import "../index.css";
import axios from "axios";
import api from "../api";
import { Swiper, SwiperSlide } from "swiper/react";
import { Navigation, Pagination, Autoplay } from "swiper/modules";
import "swiper/css";
import "swiper/css/navigation";
import "swiper/css/pagination";
import SectionTabs from "./SectionTabs";
import FreshPicksSection from "./FreshPicksSection";
import TrendingSection from "./TrendingSection";
import TopicsSection from "./TopicsSection";
import DaySpotlightSection from "./DaySpotlightSection";
import { useNavigate } from "react-router-dom";
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
    const [activeSection, setActiveSection] = useState("freshPicks");
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
    const navigate = useNavigate();
    const userId = localStorage.getItem("userId");
    const handleTopicClick = async (topicName) => {
        setSelectedDomain(topicName);

        try {
            const res = await api.get(`/TopicHighlight/knowledge`, {
                params: { domain: topicName, top: 10 },
            });

            setDomainKnowledgeItems(res.data || []);
            setIsTopicPopupOpen(true);
        } catch (err) {
            console.error("Error fetching knowledge items:", err);
            setDomainKnowledgeItems([]);
        }
    };

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
                params: { keyword },
            });

            if (response.data?.length > 0) {
                setSearchResults(response.data);
            } else {
                setError("No matching results found.");
            }
        } catch (err) {
            console.error("Search API error:", err);
            setError(
                err.response
                    ? `Error: ${err.response.status} ${err.response.statusText}`
                    : "Something went wrong. Please try again."
            );
        } finally {
            setIsSearching(false);
        }
    };

    useEffect(() => {
        let isMounted = true;
        const loadFreshPicks = async () => {
            try {
                setLoading(true);
                setError("");
                const res = await axios.get("https://localhost:7098/api/FreshPicks?count=6");
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
            }
        };
        fetchTopics();
    }, []);

    useEffect(() => {
        const fetchAllKnowledgeItems = async () => {
            try {
                const res = await axios.get("https://localhost:7098/api/TopicHighlight/knowledge?domain=all");
                setAllKnowledgeItems(res.data || []);
            } catch (error) {
                console.error("Error fetching knowledge items:", error);
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
            try {
                const res = await api.get("/Domains");
                if (isMounted) setDomains(res.data);
            } catch (err) {
                console.error("Error fetching domains:", err);
            }
        };
        fetchDomains();
        return () => {
            isMounted = false;
        };
    }, []);

    const handleLike = async (item) => {
        try {
            if (!item?.itemId) {
                console.error("Invalid item: missing itemId");
                return;
            }

            const url = `/api/engagement/like/${item.itemId}?userId=${userId}`;
            console.log("Calling Like URL:", url);

            const res = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
            });

            if (!res.ok) {
                const errMsg = await res.text();
                throw new Error(errMsg);
            }

            console.log("Liked item successfully:", item.itemId);
        } catch (error) {
            console.error("Error liking item:", error);
        }
    };

    const handleComment = async (item, commentText) => {
        try {
            if (!item?.itemId || !commentText.trim()) return;

            const url = `/api/engagement/comment/${item.itemId}?userId=${userId}`;
            console.log("Calling Comment URL:", url);

            const res = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ commentText: commentText.trim() }), // send as JSON object
            });

            if (!res.ok) {
                const errMsg = await res.text();
                throw new Error(errMsg);
            }

            console.log("Commented successfully on item:", item.itemId);
        } catch (error) {
            console.error("Error commenting:", error);
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
            <div className="p-6">
                {/* Section Tabs */}
                <SectionTabs activeSection={activeSection} onSectionChange={setActiveSection} />
                <div className="mt-4">
                    {/* Fresh Picks Section */}
                    {activeSection === "freshPicks" && (
                        <FreshPicksSection
                            freshPicks={freshPicks}
                            handleLike={(itemId) => handleLike(itemId)}
                            handleFavourite={(itemId) => handleFavourite(itemId)}
                            handleComment={(itemId, comment) => handleComment(itemId, comment)}
                            userId={userId} // ← Add this
                        />
                    )}
                    {activeSection === "trending" && (
                        <TrendingSection
                            trending={trending}
                            showAllTrending={showAllTrending}
                            setShowAllTrending={setShowAllTrending}
                            setSelectedItem={setSelectedItem}
                            setIsModalOpen={setIsModalOpen}
                            handleLike={(itemId) => handleLike(itemId)}
                            handleFavourite={(itemId) => handleFavourite(itemId)}
                            handleComment={(itemId, comment) => handleComment(itemId, comment)}
                            loading={loading}
                            error={error}
                        />
                    )}

                    {activeSection === "topics" && topics.length > 0 && (
                        <TopicsSection
                            topics={topics}
                            userId={userId}
                        />
                    )}

                    {/* Day Spotlight Section */}
                    {activeSection === "daySpotlight" && daySpotlight && (
                        <DaySpotlightSection daySpotlight={daySpotlight} />
                    )}
                </div>
            </div>
            <div className="p-4">
                {/* Quick Events */}
                <div className="bg-[#FFD873] rounded-2xl p-6 shadow-md mb-6">
                    <h3 className="text-lg font-semibold text-[#2D2D2D] mb-4">Quick Events</h3>
                    <div className="flex flex-wrap gap-4">
                        {quickEvents.map((event, index) => (
                            <button
                                key={index}
                                onClick={() => navigate(event.path)}
                                className="flex items-center gap-2 px-6 py-3 rounded-full text-sm font-medium text-[#2D2D2D] shadow-sm hover:shadow-md transition"
                                style={{ backgroundColor: "#ffffff" }}
                            >
                                {event.icon}
                                {event.name}
                            </button>
                        ))}
                    </div>
                </div>
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
        </div >
    );
};

export default MainContent;
