import React from "react";
import { Navigate } from "react-router-dom";

export default function ProtectedRoute({ children }) {
  const token = localStorage.getItem("jwtToken"); 
  return token ? children : <Navigate to="/login" replace />;
}
