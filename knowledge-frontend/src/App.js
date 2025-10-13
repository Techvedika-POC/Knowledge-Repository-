import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
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

function AppShell() {
  return (
    <div className="flex h-screen w-screen overflow-hidden">
      <Sidebar />
      <main className="flex-1 p-5 overflow-y-auto bg-[#f9fafe]">
        {/* <main className="flex-1 p-5 overflow-hidden bg-[#f9fafe]"> */}
        {/* <main className="flex-1 p-5 bg-[#f9fafe]"> */}

        <Routes>
          <Route path="home" element={<MainContent />} />
          <Route path="upload" element={<UploadKnowledgeItem />} />
          <Route path="contributions" element={<MyContributions />} />
          <Route
            path="admin"
            element={
              <ProtectedRoute requireAdmin={true}>
                <AdminDashboard />
              </ProtectedRoute>
            }
          />
           <Route
            path="approver"
            element={
              <ProtectedRoute requireApprover={true}>
                <ApproverDashboard />
              </ProtectedRoute>
            }
          />

          <Route path="*" element={<Navigate to="home" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default function App() {
  return (
    <Router>
      <Routes>
        {/* Public Pages */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/signup" element={<SignUp />} />
        <Route path="/login" element={<Login />} />

        {/* Protected App */}
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
