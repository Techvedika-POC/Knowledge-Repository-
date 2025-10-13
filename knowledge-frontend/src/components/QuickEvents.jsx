import React from "react";
import { Lightbulb, Zap, Code, BookOpen } from "lucide-react";

const QuickEvents = () => {
  const quickEvents = [
    { name: "Ideathons", icon: <Lightbulb className="w-5 h-5 text-blue-500" /> },
    { name: "Hackathons", icon: <Zap className="w-5 h-5 text-red-500" /> },
    { name: "Coding Challenges", icon: <Code className="w-5 h-5 text-green-500" /> },
    { name: "Knowledge Quest", icon: <BookOpen className="w-5 h-5 text-purple-500" /> },
  ];

  return (
    <div className="bg-[#FFD873] rounded-2xl p-6 shadow-md mb-6">
      <h3 className="text-lg font-semibold text-[#2D2D2D] mb-4">Quick Events</h3>
      <div className="flex flex-wrap gap-4">
        {quickEvents.map((event, index) => (
          <button
            key={index}
            className="flex items-center gap-2 px-6 py-3 rounded-full text-sm font-medium text-[#2D2D2D] shadow-sm hover:shadow-md transition"
            style={{ backgroundColor: "#ffffff" }}
          >
            {event.icon}
            {event.name}
          </button>
        ))}
      </div>
    </div>
  );
};

export default QuickEvents;
