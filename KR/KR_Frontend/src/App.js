import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "react-hot-toast";
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
import MentorDashboard from "./components/MentorDashboard";
import Recents from "./components/Recents";
import FavouritesPage from "./components/FavouritesPage";
import EventRegistration from "./components/EventRegistration";
import AppHighlights from "./components/AppHighlights";
import VerifyOtp from "./components/VerifyOtp";
import ResetPassword from "./components/ResetPassword";
import ForgotPassword from "./components/ForgotPassword";
import EventKnowledgeItemsPage from "./components/EventKnowledgeItemsPage";
import JuryDashboard from "./components/JuryDashboard";
// Event Pages
import ManagerDashboard from "./components/ManagerDashboard";
import CodingChallengeLeaderboardPage from "./components/CodingChallengeLeaderboardPage";

import IdeathonPage from "./components/IdeathonPage";
import HackathonPage from "./components/HackathonPage";
import CodingChallengePage from "./components/CodingChallengePage";
import LearningManagement from "./components/LearningManagement";
import CodingChallengeEventsPage from "./components/CodingChallengeEventsPage";

process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
console.log("DEBUG COMPONENTS:", {
  Sidebar,
  UploadKnowledgeItem,
  MainContent,
  Login,
  SignUp,
  ProtectedRoute,
  LandingPage,
  MyContributions,
  AdminDashboard,
  ApproverDashboard,
  MentorDashboard,
  Recents,
  FavouritesPage,
  EventRegistration,
  AppHighlights,
  VerifyOtp,
  ResetPassword,
  ForgotPassword,
  EventKnowledgeItemsPage,
  JuryDashboard,
  IdeathonPage,
  HackathonPage,
  CodingChallengePage,
  LearningManagement
});


function AppShell() {
  return (
    <div className="flex h-screen w-screen overflow-hidden">
      <Sidebar />

      <main className="flex-1 overflow-y-auto bg-[#f9fafe] mt-0 pt-0">
        <Routes>

          <Route path="home" element={<MainContent />} />
          <Route
  path="events/event-registration"
  element={<EventRegistration />}
/>

          <Route path="knowledge-article-upload" element={<UploadKnowledgeItem />} />
          <Route path="my-contributions" element={<MyContributions />} />
          <Route path="event-knowledge-articles" element={<EventKnowledgeItemsPage />} />
          {/* Event Pages */}
          <Route path="events/ideathon" element={<IdeathonPage />} />
          <Route path="events/hackathon" element={<HackathonPage />} />
          <Route
            path="events/coding-challenge"
            element={<CodingChallengeEventsPage />}
          />
          <Route
            path="events/coding-challenge/:eventId"
            element={<CodingChallengePage />}
          />
         <Route
  path="coding/leaderboard"
  element={<CodingChallengeLeaderboardPage />}
/>


          <Route path="events/learning-management" element={<LearningManagement />} />
          <Route path="manager-dashboard" element={
            <ProtectedRoute requireManager>
              <ManagerDashboard />
            </ProtectedRoute>
          } />

          <Route path="mentor-dashboard" element={
            <ProtectedRoute requireMentor>
              <MentorDashboard />
            </ProtectedRoute>
          } />

          <Route path="admin-dashboard" element={
            <ProtectedRoute requireAdmin>
              <AdminDashboard />
            </ProtectedRoute>
          } />

          <Route path="approver-dashboard" element={
            <ProtectedRoute requireApprover>
              <ApproverDashboard />
            </ProtectedRoute>
          } />

          <Route path="jury-dashboard" element={<JuryDashboard />} />
          <Route path="favourites" element={<FavouritesPage />} />
          <Route path="recents" element={<Recents />} />
          <Route path="*" element={<Navigate to="home" replace />} />
        </Routes>
      </main>
    </div>
  );
}


// ------------------- MAIN APP COMPONENT -------------------
export default function App() {
  return (
    <Router>
      {/* Global Toaster */}
      <Toaster position="top-right" toastOptions={{ duration: 3000 }} />

      <Routes>
        {/* Public Pages */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/signup" element={<SignUp />} />
        <Route path="/login" element={<Login />} />
        <Route path="/features" element={<AppHighlights />} />
        <Route path="/forgot-password" element={<ForgotPassword />} />
        <Route path="/verify-otp" element={<VerifyOtp />} />
        <Route path="/reset-password" element={<ResetPassword />} />
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