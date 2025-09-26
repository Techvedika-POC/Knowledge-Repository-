/* App.jsx */
import React, { useState } from "react";
import Sidebar from "./components/SideBar";
import UploadKnowledgeItem from "./components/UploadKnowledgeItem";
import MainContent from "./components/MainContent";

export default function App() {
  const [activePage, setActivePage] = useState("home");

  return (
    <div className="flex h-screen w-screen overflow-hidden">
      {/* Sidebar */}
      <Sidebar onSelect={setActivePage} active={activePage} />

      {/* Main Content */}
      <main className="flex-1 p-5 overflow-y-auto bg-[#f9fafe]">
        {activePage === "upload" ? <UploadKnowledgeItem /> : <MainContent />}
      </main>
    </div>
  );
}
