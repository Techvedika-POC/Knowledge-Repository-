/* Sidebar.jsx */
import React from "react";
import {
  Home,
  Upload,
  Bot,
  Search,
  Star,
  Bookmark,
  Clock,
} from "lucide-react";

export default function Sidebar({ onSelect, active }) {
  return (
    <div className="flex flex-col h-full w-[250px] bg-gradient-to-b from-[#0b2239] via-[#102c47] to-[#133553] text-white font-inter box-border p-4 flex-shrink-0">
      
      {/* Top content */}
      <div className="flex flex-col flex-1 overflow-hidden">
        <h1 className="text-[20px] font-semibold text-[#22d3ee] mb-6">
          KnowLedger Synaptix
        </h1>

        {/* Primary */}
        <div>
          <p className="text-[11px] font-medium uppercase tracking-[0.5px] text-[#94a3b8] mb-2">Primary</p>
          <nav className="flex flex-col gap-2">
            <MenuItem icon={<Home size={18} />} label="Home" active={active==="home"} onClick={()=>onSelect("home")}/>
            <MenuItem icon={<Upload size={18} />} label="Upload" active={active==="upload"} onClick={()=>onSelect("upload")}/>
            <MenuItem icon={<Bot size={18} />} label="AI Assistant" active={active==="ai"} onClick={()=>onSelect("ai")}/>
            <MenuItem icon={<Search size={18} />} label="Search" active={active==="search"} onClick={()=>onSelect("search")}/>
            <MenuItem icon={<Star size={18} />} label="Favorites" active={active==="favorites"} onClick={()=>onSelect("favorites")}/>
          </nav>
        </div>

        {/* Secondary */}
        <div className="mt-6">
          <p className="text-[11px] font-medium uppercase tracking-[0.5px] text-[#94a3b8] mb-2">Secondary</p>
          <nav className="flex flex-col gap-2">
            <MenuItem icon={<Bookmark size={18} />} label="Bookmarks" active={active==="bookmarks"} onClick={()=>onSelect("bookmarks")}/>
            <MenuItem icon={<Clock size={18} />} label="Recents" active={active==="recents"} onClick={()=>onSelect("recents")}/>
          </nav>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="flex flex-col gap-2 mt-4">
        <button className="w-[90%] mx-auto px-2 py-2 rounded-[8px] text-[13px] font-medium bg-[#06b6d4] hover:bg-[#0891b2] text-white transition-all">New Upload</button>
        <button className="w-[90%] mx-auto px-2 py-2 rounded-[8px] text-[13px] font-medium border border-[#22d3ee] text-[#22d3ee] hover:bg-[#15314d] hover:text-white hover:border-[#0891b2] transition-all">Knowledge Quest</button>
      </div>
    </div>
  );
}

function MenuItem({ icon, label, active, onClick }) {
  return (
    <div
      onClick={onClick}
      className={`flex items-center gap-3 px-3 py-2 rounded-[8px] text-[14px] cursor-pointer transition-all ${
        active ? "bg-[#15314d] text-[#22d3ee]" : "text-[#f1f5f9] hover:bg-white/10"
      }`}
    >
      {icon}
      <span>{label}</span>
    </div>
  );
}
