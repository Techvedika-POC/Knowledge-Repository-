import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "react-hot-toast";

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
import Recents from "./components/Recents";
import FavouritesPage from "./components/FavouritesPage";
import EventRegistration from "./components/EventRegistration";
import AppHighlights from "./components/AppHighlights";
// Event Pages
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

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto bg-[#f9fafe] p-5 mt-0 pt-0">

        <Routes>
          {/* Default MainContent */}
          <Route path="home" element={<MainContent />} />

          {/* Event Pages */}
          <Route path="events/ideathon" element={<IdeathonPage />} />
          <Route path="events/hackathon" element={<HackathonPage />} />
          <Route path="events/coding-challenge" element={<CodingChallengePage />} />
          <Route path="events/knowledge-quest" element={<KnowledgeQuestPage />} />
          <Route path="events/event-registration" element={<EventRegistration/>}/>

          {/* Upload & Contributions */}
          <Route path="upload-knowledge" element={<UploadKnowledgeItem />} />
          <Route path="contributions" element={<MyContributions />} />
          <Route path="recents"element={<Recents/>}/>
          <Route path="favorites"element={<FavouritesPage/>}/>
         
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

          {/* Default redirect for unknown /app routes */}
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
      {/*  Toaster is added globally here */}
      <Toaster position="top-right" toastOptions={{ duration: 3000 }} />

      <Routes>
        {/* Public Pages */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/signup" element={<SignUp />} />
        <Route path="/login" element={<Login />} />
 <Route path="features" element={<AppHighlights/>}/>
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
