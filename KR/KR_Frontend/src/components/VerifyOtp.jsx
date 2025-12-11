import React, { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import api from "../api";
import toast from "react-hot-toast";

export default function VerifyOtp() {
  const location = useLocation();
  const navigate = useNavigate();

  const email = location.state?.email;

  const [otp, setOtp] = useState("");
  const [loading, setLoading] = useState(false);

  if (!email) {
    return <p className="text-center mt-10">Invalid page access</p>;
  }

 const handleVerify = async (e) => {
  e.preventDefault();
  setLoading(true);

  try {
    const res = await api.post("/auth/verify-otp", {
      email,
      code: otp,
    });

    toast.success("OTP verified!");

    const resetToken = res.data?.resetToken;

    if (!resetToken) {
      toast.error("Reset token missing from server!");
      return;
    }

    navigate("/reset-password", {
      state: {
        email,
        resetToken,
      },
    });

  } catch (err) {
    toast.error(err.response?.data?.message || "Invalid or expired OTP");
  } finally {
    setLoading(false);
  }
};


  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="max-w-md w-full bg-white p-8 rounded-xl shadow-md">
        <h2 className="text-2xl font-bold text-center mb-6">Verify OTP</h2>

        <form onSubmit={handleVerify} className="space-y-4">
          <div>
            <label>Enter OTP</label>
            <input
              type="text"
              value={otp}
              onChange={(e) => setOtp(e.target.value)}
              required
              className="mt-1 block w-full border rounded-md p-2"
            />
          </div>

          <button
            type="submit"
            className="w-full bg-blue-600 text-white py-2 rounded-md"
            disabled={loading}
          >
            {loading ? "Verifying..." : "Verify OTP"}
          </button>
        </form>
      </div>
    </div>
  );
}
