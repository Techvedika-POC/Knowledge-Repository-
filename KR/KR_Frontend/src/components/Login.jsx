import React, { useState } from "react";
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

      // Support both camelCase and PascalCase responses
      const token = res?.data?.token ?? res?.data?.Token ?? null;
      const userId = res?.data?.userId ?? res?.data?.UserId ?? null;

      if (!token) {
        console.warn("Auth response missing token:", res?.data);
        throw new Error("Authentication token not received from server.");
      }

      // persist
      localStorage.setItem("jwtToken", token);
      if (userId) localStorage.setItem("userId", userId);

      if (res.data?.name ?? res.data?.Name) localStorage.setItem("userName", res.data?.name ?? res.data?.Name);
      if (res.data?.email ?? res.data?.Email) localStorage.setItem("userEmail", res.data?.email ?? res.data?.Email);
      if (res.data?.roles ?? res.data?.Roles) localStorage.setItem("userRoles", JSON.stringify(res.data?.roles ?? res.data?.Roles));

      toast.success("Login successful!");
      navigate("/app/home");
    } catch (err) {
      console.error("Login failed", err);
      // prefer server message when available
      const serverMsg = err?.response?.data?.message ?? err?.response?.data?.Message;
      toast.error(serverMsg || "Invalid email or password.");
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

        <form onSubmit={handleSubmit} className="space-y-4" aria-label="Login form">
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700">Email address</label>
            <input
              id="email"
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              required
              className="mt-1 block w-full border rounded-md p-2"
              aria-required="true"
              aria-label="email"
            />
          </div>

          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700">Password</label>
            <input
              id="password"
              type="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              required
              className="mt-1 block w-full border rounded-md p-2"
              aria-required="true"
              aria-label="password"
            />

            {/* Forgot Password Link (passes email to the page for convenience) */}
            <div className="text-right mt-1">
              <button
                type="button"
                onClick={() => navigate("/forgot-password", { state: { email: form.email } })}
                className="text-sm text-blue-600 hover:text-blue-800"
                disabled={loading}
                aria-disabled={loading}
              >
                Forgot Password?
              </button>
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-2 px-4 bg-blue-600 text-white rounded-md disabled:opacity-60"
            aria-busy={loading}
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
