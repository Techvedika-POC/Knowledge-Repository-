import React, { useState } from "react";
import { Toaster } from "react-hot-toast";
import EventManagement from "./EventManagement";
import DomainManagement from "./DomainCategoryManagement";
import RoleManagement from "./RoleManagement";
import KnowledgeQuestManagement from "./KnowledgeQuestManagement";
import IdeathonEventPage from "./IdeathonEventPage"; // Placeholder page

export default function AdminDashboard() {
  const [activeTab, setActiveTab] = useState("events");

  const renderContent = () => {
    switch (activeTab) {
      case "events":
        return <EventManagement />;
      case "domains":
        return <DomainManagement />;
      case "roles":
        return <RoleManagement />;
      case "knowledgeQuest":
        return <KnowledgeQuestManagement />;
      case "ideathon":
        return <IdeathonEventPage />; // Render Ideathon placeholder
      default:
        return <EventManagement />;
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 text-gray-800 flex flex-col">
      <Toaster position="top-right" />

      <nav className="bg-blue-100 text-blue-800 p-4 flex justify-between items-center shadow-sm border-b border-blue-200">
        <h1 className="text-xl font-semibold">Admin Dashboard</h1>

        <div className="space-x-3 flex flex-wrap">
          <button
            onClick={() => setActiveTab("events")}
            className={`px-4 py-2 rounded-md ${
              activeTab === "events"
                ? "bg-blue-300 text-blue-900"
                : "bg-white border border-blue-200 hover:bg-blue-50"
            }`}
          >
            Events
          </button>
          <button
            onClick={() => setActiveTab("domains")}
            className={`px-4 py-2 rounded-md ${
              activeTab === "domains"
                ? "bg-blue-300 text-blue-900"
                : "bg-white border border-blue-200 hover:bg-blue-50"
            }`}
          >
            Domains
          </button>
          <button
            onClick={() => setActiveTab("roles")}
            className={`px-4 py-2 rounded-md ${
              activeTab === "roles"
                ? "bg-blue-300 text-blue-900"
                : "bg-white border border-blue-200 hover:bg-blue-50"
            }`}
          >
            Roles
          </button>
          <button
            onClick={() => setActiveTab("knowledgeQuest")}
            className={`px-4 py-2 rounded-md ${
              activeTab === "knowledgeQuest"
                ? "bg-blue-300 text-blue-900"
                : "bg-white border border-blue-200 hover:bg-blue-50"
            }`}
          >
            Knowledge Quest
          </button>
          <button
            onClick={() => setActiveTab("ideathon")}
            className={`px-4 py-2 rounded-md ${
              activeTab === "ideathon"
                ? "bg-blue-300 text-blue-900"
                : "bg-white border border-blue-200 hover:bg-blue-50"
            }`}
          >
            Ideathon
          </button>
        </div>
      </nav>

      <main className="flex-1 p-8">{renderContent()}</main>
    </div>
  );
}

