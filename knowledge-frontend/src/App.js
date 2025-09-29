import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import Sidebar from "./components/SideBar";
import UploadKnowledgeItem from "./components/UploadKnowledgeItem";
import MainContent from "./components/MainContent";
import Login from "./components/Login";
import SignUp from "./components/SignUp";
import ProtectedRoute from "./components/ProtectedRoute";
import LandingPage from "./components/LandingPage"; // Landing page contains hero + carousel

// AppShell for logged-in users
function AppShell() {
  const [activePage, setActivePage] = React.useState("home");

  return (
    <div className="flex h-screen w-screen overflow-hidden">
      <Sidebar onSelect={setActivePage} active={activePage} />
      <main className="flex-1 p-5 overflow-y-auto bg-[#f9fafe]">
        {activePage === "upload" ? <UploadKnowledgeItem /> : <MainContent />}
      </main>
    </div>
  );
}

export default function App() {
  return (
    <Router>
      <Routes>
        {/* Public Landing Page */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/signup" element={<SignUp />} />
        <Route path="/login" element={<Login />} />

        {/* Protected AppShell */}
        <Route
          path="/app"
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
