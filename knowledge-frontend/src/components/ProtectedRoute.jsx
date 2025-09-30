import React from "react";
import { Navigate } from "react-router-dom";
export default function ProtectedRoute({
  children,
  requireAdmin = false,
  redirectPath = "/app/home",
}) {
  const token = localStorage.getItem("jwtToken");
  const role = localStorage.getItem("userRole") || "user";

  if (!token) return <Navigate to="/login" replace />;

  if (requireAdmin && role !== "admin") return <Navigate to="/app/home" replace />;

  return children;
}
