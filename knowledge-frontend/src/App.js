import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";

// Components
import Sidebar from "./components/SideBar";
import UploadKnowledgeItem from "./components/UploadKnowledgeItem";
import MainContent from "./components/MainContent";
import Login from "./components/Login";
import SignUp from "./components/SignUp";
import ProtectedRoute from "./components/ProtectedRoute";
import LandingPage from "./components/LandingPage";
import MyContributions from "./components/MyContributions";
import AdminDashboard from "./components/AdminDashboard";
import ApproverDashboard from "./components/ApproverDashboard";

// Event Pages (placeholders, replace with real components)
import IdeathonPage from "./components/IdeathonPage";
import HackathonPage from "./components/HackathonPage";
import CodingChallengePage from "./components/CodingChallengePage";
import KnowledgeQuestPage from "./components/KnowledgeQuestPage";

// Protected App Shell
function AppShell() {
  return (
    <div className="flex h-screen w-screen overflow-hidden">
      {/* Sidebar */}
      <Sidebar />

      {/* Main content */}
      {/* <main className="flex-1 p-5 overflow-y-auto bg-[#f9fafe]"> */}
      <main class="flex-1 overflow-y-auto bg-[#f9fafe] p-0 pt-0 pl-0">

        <Routes>
          {/* Default MainContent */}
          <Route path="home" element={<MainContent />} />

          {/* Quick Event Pages */}
          <Route path="events/ideathon" element={<IdeathonPage />} />
          <Route path="/upload-knowledge" element={<UploadKnowledgeItem />} />
          <Route path="events/hackathon" element={<HackathonPage />} />
          <Route path="events/coding-challenge" element={<CodingChallengePage />} />
          <Route path="events/knowledge-quest" element={<KnowledgeQuestPage />} />

          {/* Other Pages */}
          <Route path="upload" element={<UploadKnowledgeItem />} />
          <Route path="contributions" element={<MyContributions />} />

          {/* Admin Dashboard */}
          <Route
            path="admin"
            element={
              <ProtectedRoute requireAdmin={true}>
                <AdminDashboard />
              </ProtectedRoute>
            }
          />

          {/* Approver Dashboard */}
          <Route
            path="approver"
            element={
              <ProtectedRoute requireApprover={true}>
                <ApproverDashboard />
              </ProtectedRoute>
            }
          />

          {/* Redirect unknown /app routes to /app/home */}
          <Route path="*" element={<Navigate to="home" replace />} />
        </Routes>
      </main>
    </div>
  );
}

// Main App Component
export default function App() {
  return (
    <Router>
      <Routes>
        {/* Public Pages */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/signup" element={<SignUp />} />
        <Route path="/login" element={<Login />} />

        {/* Protected App Shell */}
        <Route
          path="/app/*"
          element={
            <ProtectedRoute>
              <AppShell />
            </ProtectedRoute>
          }
        />

        {/* Redirect unknown routes */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Router>
  );
}
