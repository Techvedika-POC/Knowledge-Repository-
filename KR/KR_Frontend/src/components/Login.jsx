import React, { useState } from "react";
import axios from "axios";
import api from "../api";
import { useNavigate } from "react-router-dom";
import toast from "react-hot-toast";

export default function Login() {
  const navigate = useNavigate();
  const [form, setForm] = useState({ email: "", password: "" });
  const [loading, setLoading] = useState(false);

  const handleChange = (e) =>
    setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await api.post("/auth/login", form);
      const token = res.data.token;
      const userId = res.data.userId;
      localStorage.setItem("jwtToken", token);
      localStorage.setItem("userId", userId);
      if (res.data.name) localStorage.setItem("userName", res.data.name);
      if (res.data.email) localStorage.setItem("userEmail", res.data.email);
      if (res.data.roles) {
        localStorage.setItem("userRoles", JSON.stringify(res.data.roles));
      }
      if (res.data.userId) {
        localStorage.setItem("userId", res.data.userId);
      } else if (res.data.id) {
        localStorage.setItem("userId", res.data.id);
      } else {
        console.warn("userId missing from login response");
      }

      toast.success("Login successful!");
      navigate("/app/home");
    } catch (err) {
      console.error("Login failed", err);
      toast.error(err.response?.data?.message || "Invalid email or password.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="max-w-md w-full bg-white p-8 rounded-xl shadow-md">
        <h2 className="text-2xl font-bold text-gray-900 text-center mb-6">
          Login to Your Account
        </h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label>Email address</label>
            <input
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              required
              className="mt-1 block w-full border rounded-md p-2"
            />
          </div>

          <div>
            <label>Password</label>
            <input
              type="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              required
              className="mt-1 block w-full border rounded-md p-2"
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-2 px-4 bg-blue-600 text-white rounded-md"
          >
            {loading ? "Logging in..." : "Login"}
          </button>
        </form>
        <p className="mt-6 text-center text-sm">
          Don’t have an account?{" "}
          <button
            type="button"
            onClick={() => navigate("/signup")}
            className="text-blue-600 hover:text-blue-700"
          >
            Signup
          </button>
        </p>
      </div>
    </div>
  );
}
