import React, { useState } from "react";
import Navbar from "./Navbar";


const MainContent = () => {
  const [activeTab, setActiveTab] = useState("Leaderboard");

  return (
    <div className="flex flex-col w-full h-full overflow-y-auto bg-gray-50">
      <Navbar />
      <main className="flex flex-col gap-6 w-full p-6">
      </main>
    </div>
  );
};

export default MainContent;
