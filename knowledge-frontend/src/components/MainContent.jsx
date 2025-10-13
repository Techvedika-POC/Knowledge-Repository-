import React, { useState } from "react";
import Navbar from "./Navbar";
import FreshPicks from "./FreshPicks";
import Trending from "./Trending";
import Topics from "./Topics";
import DaySpotlight from "./DaySpotlight";
import QuickEvents from "./QuickEvents";
import LeaderboardTabs from "./LeaderboardTabs";
import SectionTabs from "./SectionTabs";

const MainContent = () => {
  const [activeTab, setActiveTab] = useState("Leaderboard");
  // const tabs = ["Leaderboard", "Recent Activity", "Announcements"];

  return (
    <div className="flex flex-col w-full h-full overflow-y-auto bg-gray-50">
      <Navbar />

      {/* Scrollable main section */}
      <main className="flex flex-col gap-6 w-full p-6">
        {/* <SectionTabs
          tabs={tabs}
          activeTab={activeTab}
          onTabChange={setActiveTab}
        /> */}
        <FreshPicks />
        <Trending />
        <Topics />
        <DaySpotlight />
        <QuickEvents />
        <LeaderboardTabs />
      </main>
    </div>
  );
};

export default MainContent;
