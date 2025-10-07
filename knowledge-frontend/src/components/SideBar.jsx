import React, { useState, useEffect, useRef } from "react";
import { Home, Upload, Bot, Star, Clock, FileText } from "lucide-react";
import { useNavigate, useLocation } from "react-router-dom";

export default function Sidebar() {
  const [user, setUser] = useState({ name: "", email: "" });
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef(null);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const name = localStorage.getItem("userName");
    const email = localStorage.getItem("userEmail");
    setUser({ name: name || "Guest", email: email || "" });
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
    navigate("/"); // safer than window.location.href
  };

  return (
    <div className="flex flex-col h-full w-[250px] bg-gradient-to-b from-[#0b2239] via-[#102c47] to-[#133553] text-white font-inter p-4 flex-shrink-0">
      {/* Top Content */}
      <div className="flex flex-col flex-1 overflow-hidden">
        <h1 className="text-[20px] font-semibold text-[#22d3ee] mb-6">
          KnowLedger Synaptix
        </h1>

        {/* Primary */}
        <div>
          <p className="text-[11px] font-medium uppercase tracking-[0.5px] text-[#94a3b8] mb-2">
            Primary
          </p>
          <nav className="flex flex-col gap-2">
            <MenuItem
              icon={<Home size={18} />}
              label="Home"
              active={location.pathname === "/app/home"}
              onClick={() => navigate("/app/home")}
            />
            <MenuItem
              icon={<Upload size={18} />}
              label="Upload"
              active={location.pathname === "/app/upload"}
              onClick={() => navigate("/app/upload")}
            />
            <MenuItem
              icon={<Bot size={18} />}
              label="AI Assistant"
              active={location.pathname === "/app/ai"}
              onClick={() => navigate("/app/ai")}
            />
            <MenuItem
              icon={<FileText size={18} />}
              label="My Contributions"
              active={location.pathname === "/app/contributions"}
              onClick={() => navigate("/app/contributions")}
            />
            <MenuItem
              icon={<Star size={18} />}
              label="Admin Dashboard"
              active={location.pathname === "/app/admin"}
              onClick={() => navigate("/app/admin")}
            />

          </nav>
        </div>

        {/* Secondary */}
        <div className="mt-6">
          <p className="text-[11px] font-medium uppercase tracking-[0.5px] text-[#94a3b8] mb-2">
            Secondary
          </p>
          <nav className="flex flex-col gap-2">
            <MenuItem
              icon={<Star size={18} />}
              label="Favorites"
              active={location.pathname === "/app/favorites"}
              onClick={() => navigate("/app/favorites")}
            />
            <MenuItem
              icon={<Clock size={18} />}
              label="Recents"
              active={location.pathname === "/app/recents"}
              onClick={() => navigate("/app/recents")}
            />
          </nav>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="flex flex-col gap-2 mt-4 mb-3">
        <button
          onClick={() => navigate("/app/upload")}
          className="w-[90%] mx-auto px-2 py-2 rounded-[8px] text-[13px] font-medium bg-[#06b6d4] hover:bg-[#0891b2] text-white transition-all"
        >
          New Upload
        </button>
      </div>

      {/* User Profile */}
      <div ref={menuRef} className="mt-auto relative w-full">
        <button
          onClick={() => setMenuOpen(!menuOpen)}
          className="flex items-center w-[90%] mx-auto p-2 rounded-lg bg-[#102c47] hover:bg-[#15314d] transition-all gap-3"
        >
          <img
            src={`https://ui-avatars.com/api/?name=${encodeURIComponent(
              user.name || "G"
            )}&background=random&size=64`}
            alt="user"
            className="w-8 h-6 rounded-full object-cover"
          />
          <div className="flex flex-col text-left">
            <span className="text-sm font-medium text-white">{user.name}</span>
            <span className="text-xs text-gray-300">
              {user.email || "Free"}
            </span>
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
      className={`flex items-center gap-3 px-3 py-2 rounded-[8px] text-[14px] cursor-pointer transition-all ${active
          ? "bg-[#15314d] text-[#22d3ee]"
          : "text-[#f1f5f9] hover:bg-white/10"
        }`}
    >
      {icon}
      <span>{label}</span>
    </div>
  );
}
