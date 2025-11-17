import React, { useState, useEffect, useRef } from "react";
import {
  Home,
  Upload,
  Bot,
  Star,
  Clock,
  FileText,
  BookOpenCheck,
  Briefcase,
  GraduationCap, 
} from "lucide-react";
import { useNavigate, useLocation } from "react-router-dom";

export default function Sidebar() {
  const [user, setUser] = useState({ name: "", email: "" });
  const [menuOpen, setMenuOpen] = useState(false);
  const [roles, setRoles] = useState([]);
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

      {/* Manager */}
      {roles.includes("Mentor") && (
        <MenuItem
          icon={<Briefcase size={16} />}
          label="Mentor Dashboard"
          active={location.pathname === "/app/mentor-dashboard"}
          onClick={() => navigate("/app/mentor-dashboard")}
        />
      )}

      {/* Admin */}
      <MenuItem
        icon={<Star size={16} />}
        label="Admin Dashboard"
        active={location.pathname === "/app/admin"}
        onClick={() => navigate("/app/admin")}
      />

      {/* Approver */}
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

  {/* Secondary Menu */}
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

  {/* Quick Actions */}
  <div className="flex flex-col gap-2 mt-3 mb-3 flex-shrink-0">
    <button
      onClick={() => navigate("/app/upload-knowledge")}
      className="w-[90%] mx-auto px-2 py-[6px] rounded-[8px] text-[12px] font-medium bg-[#06b6d4] hover:bg-[#0891b2] text-white transition-all"
    >
      New Upload
    </button>
  </div>

  {/* User Profile */}
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
        <span className="text-[10px] text-gray-300">{user.email || "Free"}</span>
      </div>
    </button>
    {menuOpen && (
      <div className="absolute bottom-14 left-1/2 -translate-x-1/2 bg-white text-black rounded-lg shadow-lg w-44 p-3 z-50">
        <p className="font-medium text-sm">{user.name}</p>
        <p className="text-xs text-gray-500">{user.email || "Free"}</p>
        <button
          onClick={handleLogout}
          className="mt-2 w-full bg-red-500 text-white py-1 rounded hover:bg-red-600 text-sm"
        >
          Logout
        </button>
      </div>
    )}
  </div>
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
