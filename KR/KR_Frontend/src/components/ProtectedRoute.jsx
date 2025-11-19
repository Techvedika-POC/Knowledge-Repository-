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
  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (requireAdmin && !roles.includes("Admin")) {
    return <Navigate to={redirectPath} replace />;
  }
  if (requireManager && !roles.includes("Manager")) {
    return <Navigate to={redirectPath} replace />;
  }
  if (requireApprover && !roles.includes("Approver")) {
    return <Navigate to={redirectPath} replace />;
  }
  return children;
}
