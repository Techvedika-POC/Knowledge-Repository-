import React from "react";
import { Navigate } from "react-router-dom";

export default function ProtectedRoute({
  children,
  requireAdmin = false,
  requireManager = false,
  requireApprover = false,
  redirectPath = "/app/home",
}) {
  const token = localStorage.getItem("jwtToken");
  const roles = JSON.parse(localStorage.getItem("userRoles") || "[]");

  // Not logged in → go to login
  if (!token) {
    return <Navigate to="/login" replace />;
  }

  // Admin access only
  if (requireAdmin && !roles.includes("Admin")) {
    return <Navigate to={redirectPath} replace />;
  }

  // Manager access only
  if (requireManager && !roles.includes("Manager")) {
    return <Navigate to={redirectPath} replace />;
  }

  // Approver access only
  if (requireApprover && !roles.includes("Approver")) {
    return <Navigate to={redirectPath} replace />;
  }

  // Otherwise → allow access
  return children;
}
