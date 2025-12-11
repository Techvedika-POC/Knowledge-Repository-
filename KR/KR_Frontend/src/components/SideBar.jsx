import React, { useState, useEffect, useRef } from "react";
import {
  Home,
  Upload,
  Bot,
  Star,
  Clock,
  FileText,
  Briefcase,
  User,
} from "lucide-react";
import { useNavigate, useLocation } from "react-router-dom";

export default function Sidebar() {
  const [user, setUser] = useState({ name: "", email: "" });
  const [menuOpen, setMenuOpen] = useState(false);
  const [roles, setRoles] = useState([]);
  const [editOpen, setEditOpen] = useState(false);

  const [formData, setFormData] = useState({
    name: "",
    email: ""
  });

  const menuRef = useRef(null);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const name = localStorage.getItem("userName");
    const email = localStorage.getItem("userEmail");
    const storedRoles = JSON.parse(localStorage.getItem("userRoles")) || [];

    setUser({ name: name || "Guest", email: email || "" });
    setRoles(storedRoles);
  }, []);

  useEffect(() => {
    function handleClickOutside(e) {
      if (menuRef.current && !menuRef.current.contains(e.target)) {
        setMenuOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleLogout = () => {
    localStorage.clear();
    navigate("/");
  };

  const openEditProfile = () => {
    setFormData({
      name: user.name || "",
      email: user.email || ""
    });
    setEditOpen(true);
  };

  // ⭐ FIXED Update Profile API Call
  const saveProfile = async () => {
    try {
      const userId = localStorage.getItem("userId");
      const token = localStorage.getItem("jwtToken");

      if (!userId) {
        alert("User ID missing.");
        return;
      }

      const response = await fetch(
        `https://localhost:7001/api/users/${userId}/profile`,
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify(formData),
        }
      );

      if (!response.ok) throw new Error("Profile update failed");

      // Update local values
      localStorage.setItem("userName", formData.name);
      localStorage.setItem("userEmail", formData.email);

      setUser({
        name: formData.name,
        email: formData.email,
      });

      setEditOpen(false);
      alert("Profile updated successfully!");

    } catch (error) {
      console.error(error);
      alert("Error updating profile.");
    }
  };

  return (
    <div className="flex flex-col h-full w-[240px] bg-gradient-to-b from-[#0b2239] via-[#102c47] to-[#133553] text-white font-inter p-3 flex-shrink-0">

      {/* Header */}
      <div className="text-center mb-5 flex-shrink-0">
        <h1 className="text-[18px] font-semibold text-[#22d3ee]">
          KnowLedger Synaptix
        </h1>
        <p className="text-[10px] font-medium text-[#94a3b8] mt-1">
          Innovate • Collaborate • Build
        </p>
      </div>

      {/* Primary Menu */}
      <div className="flex-1 flex flex-col">
        <p className="text-[10px] font-medium uppercase tracking-[0.5px] text-[#94a3b8] mb-2 flex-shrink-0">
          Primary
        </p>

        <nav className="flex-1 flex flex-col gap-[5px] text-[13px] overflow-hidden">

          <MenuItem
            icon={<Home size={16} />}
            label="Home"
            active={location.pathname === "/app/home"}
            onClick={() => navigate("/app/home")}
          />

          <MenuItem
            icon={<Upload size={16} />}
            label="Upload"
            active={location.pathname === "/app/upload-knowledge"}
            onClick={() => navigate("/app/upload-knowledge")}
          />

          <MenuItem
            icon={<Bot size={16} />}
            label="AI Assistant"
            active={location.pathname === "/app/ai"}
            onClick={() => navigate("/app/ai")}
          />

          <MenuItem
            icon={<FileText size={16} />}
            label="My Contributions"
            active={location.pathname === "/app/contributions"}
            onClick={() => navigate("/app/contributions")}
          />

          {roles.includes("Mentor") && (
            <MenuItem
              icon={<Briefcase size={16} />}
              label="Mentor Dashboard"
              active={location.pathname === "/app/mentor-dashboard"}
              onClick={() => navigate("/app/mentor-dashboard")}
            />
          )}

          {roles.includes("Admin") && (
            <MenuItem
              icon={<Star size={16} />}
              label="Admin Dashboard"
              active={location.pathname === "/app/admin"}
              onClick={() => navigate("/app/admin")}
            />
          )}

          {roles.includes("Approver") && (
            <MenuItem
              icon={<Star size={16} />}
              label="Approver Dashboard"
              active={location.pathname === "/app/approver"}
              onClick={() => navigate("/app/approver")}
            />
          )}
        </nav>
      </div>

      {/* Secondary */}
      <div className="mt-3 flex-shrink-0">
        <p className="text-[10px] font-medium uppercase tracking-[0.5px] text-[#94a3b8] mb-2">
          Secondary
        </p>

        <nav className="flex flex-col gap-[5px] text-[13px]">
          <MenuItem
            icon={<Star size={16} />}
            label="Favourites"
            active={location.pathname === "/app/favorites"}
            onClick={() => navigate("/app/favorites")}
          />
          <MenuItem
            icon={<Clock size={16} />}
            label="Recents"
            active={location.pathname === "/app/recents"}
            onClick={() => navigate("/app/recents")}
          />
        </nav>
      </div>

      {/* Quick Upload Button */}
      <div className="flex flex-col gap-2 mt-3 mb-3 flex-shrink-0">
        <button
          onClick={() => navigate("/app/upload-knowledge")}
          className="w-[90%] mx-auto px-2 py-[6px] rounded-[8px] text-[12px] font-medium bg-[#06b6d4] hover:bg-[#0891b2] text-white transition-all"
        >
          New Upload
        </button>
      </div>

      {/* User Section */}
      <div ref={menuRef} className="mt-auto relative w-full flex-shrink-0">
        <button
          onClick={() => setMenuOpen(!menuOpen)}
          className="flex items-center w-[90%] mx-auto p-2 rounded-lg bg-[#102c47] hover:bg-[#15314d] transition-all gap-2"
        >
          <img
            src={`https://ui-avatars.com/api/?name=${encodeURIComponent(
              user.name || "G"
            )}&background=random&size=64`}
            alt="user"
            className="w-7 h-7 rounded-full object-cover"
          />
          <div className="flex flex-col text-left">
            <span className="text-[13px] font-medium text-white">{user.name}</span>
            <span className="text-[10px] text-gray-300">{user.email}</span>
          </div>
        </button>

        {menuOpen && (
          <div className="absolute bottom-14 left-1/2 -translate-x-1/2 bg-white text-black rounded-lg shadow-lg w-44 p-3 z-50">
            <p className="font-medium text-sm">{user.name}</p>
            <p className="text-xs text-gray-500">{user.email}</p>

            {/* Edit Profile Button */}
            <button
              onClick={() => {
                setMenuOpen(false);
                openEditProfile();
              }}
              className="mt-2 w-full bg-blue-500 text-white py-1 rounded hover:bg-blue-600 text-sm flex items-center justify-center gap-1"
            >
              <User size={14} /> Edit Profile
            </button>

            {/* Logout Button */}
            <button
              onClick={handleLogout}
              className="mt-2 w-full bg-red-500 text-white py-1 rounded hover:bg-red-600 text-sm"
            >
              Logout
            </button>
          </div>
        )}
      </div>

      {/* Edit Profile Modal */}
      {editOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-[350px] p-5">

            <h2 className="text-lg font-semibold mb-4 text-gray-700">
              Edit Profile
            </h2>

            {/* Name */}
            <label className="text-sm text-gray-600">Name</label>
            <input
              type="text"
              className="w-full p-2 border rounded mb-3 text-black"
              value={formData.name}
              onChange={(e) =>
                setFormData({ ...formData, name: e.target.value })
              }
            />

            {/* Email */}
            <label className="text-sm text-gray-600">Email</label>
            <input
              type="email"
              className="w-full p-2 border rounded mb-4 text-black"
              value={formData.email}
              onChange={(e) =>
                setFormData({ ...formData, email: e.target.value })
              }
            />

            <div className="flex justify-end gap-2">
              <button
                onClick={() => setEditOpen(false)}
                className="px-3 py-1 bg-gray-300 rounded hover:bg-gray-400 text-sm"
              >
                Cancel
              </button>

              <button
                onClick={saveProfile}
                className="px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-700 text-sm"
              >
                Save
              </button>
            </div>

          </div>
        </div>
      )}
    </div>
  );
}

function MenuItem({ icon, label, active, onClick }) {
  return (
    <div
      onClick={onClick}
      className={`flex items-center gap-3 px-3 py-[6px] rounded-[8px] text-[13px] cursor-pointer transition-all ${
        active
          ? "bg-[#15314d] text-[#22d3ee]"
          : "text-[#f1f5f9] hover:bg-white/10"
      }`}
    >
      {icon}
      <span className="truncate">{label}</span>
    </div>
  );
}
