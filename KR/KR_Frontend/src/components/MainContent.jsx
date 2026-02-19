// MainContent.jsx
import React, { useState, useEffect } from "react";
import Navbar from "./Navbar";
import AppHighlights from "./AppHighlights";
import "../index.css";
import api from "../api";
import { useNavigate } from "react-router-dom";

import SectionTabs from "./SectionTabs";
import FreshPicksSection from "./FreshPicksSection";
import TrendingSection from "./TrendingSection";
import TopicsSection from "./TopicsSection";
import QuickEvents from "./QuickEvents";
import DaySpotlightSection from "./DaySpotlightSection";
import LeaderboardSection from "./LeaderboardSection";
import AnnouncementSection from "./AnnouncementSection";
console.log("dthy");
const MainContent = () => {
  const [activeSection, setActiveSection] = useState("freshPicks");
  const [freshPicks, setFreshPicks] = useState([]);
  const [trending, setTrending] = useState([]);
  const [topics, setTopics] = useState([]);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const navigate = useNavigate();
  const userId = localStorage.getItem("userId");
  useEffect(() => {
    const loadFreshPicks = async () => {
      try {
        setLoading(true);
        setError("");
        const res = await api.get("/FreshPicks?count=6");
        setFreshPicks(res.data);
      } catch (err) {
        setError("Failed to load Fresh Picks");
      } finally {
        setLoading(false);
      }
    };
    loadFreshPicks();
  }, []);

  useEffect(() => {
    api
      .get("/Trending")
      .then((res) => setTrending(res.data || []))
      .catch(() => {});
  }, []);
  useEffect(() => {
    api
      .get("/TopicHighlight/topics?top=10")
      .then((res) => setTopics(res.data))
      .catch(() => {});
  }, []);


  return (
    <div className="flex flex-col w-full">
      <Navbar />

      {/* ANNOUNCEMENT SECTION */}
      <div className="px-3 mt-1">
        <AnnouncementSection />
      </div>

      {/* QUICK EVENTS */}
      <div className="px-8 mt-1 mb-2">
        <QuickEvents navigate={navigate} />
      </div>

      {/* SECTION TABS + CONTENT (Fresh, Trending, Topics) */}
      <div className="px-3 mt-2 mb-1">
        <SectionTabs
          activeSection={activeSection}
          onSectionChange={setActiveSection}
        />

        <div className="mt-2">
          {activeSection === "freshPicks" && (
            <FreshPicksSection freshPicks={freshPicks} userId={userId} />
          )}

          {activeSection === "trending" && (
            <TrendingSection
              trending={trending}
              loading={loading}
              error={error}
              userId={userId}
            />
          )}

          {activeSection === "topics" && topics.length > 0 && (
            <TopicsSection topics={topics} userId={userId} />
          )}
        </div>
      </div>

      {/* DAY SPOTLIGHT*/}
      <DaySpotlightSection />

      {/* LEADERBOARD */}
      <div className="px-6 mt-0 mb-0">
        <LeaderboardSection />
      </div>
    </div>
  );
};

export default MainContent;
