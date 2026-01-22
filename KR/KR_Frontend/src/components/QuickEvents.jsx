import React from "react";
import { Lightbulb, Zap, Code, BookOpen } from "lucide-react";

const QuickEvents = ({ navigate }) => {
  const quickEvents = [
    {
      name: "Ideathons",
      icon: <Lightbulb className="w-5 h-5 text-blue-400" />,
      path: "/app/events/ideathon",
    },
    {
      name: "Hackathons",
      icon: <Zap className="w-5 h-5 text-red-400" />,
      path: "/app/events/hackathon",
    },
    {
      name: "Coding Challenges",
      icon: <Code className="w-5 h-5 text-green-400" />,
      path: "/app/events/coding-challenge",
    },
    {
      name: "Learning Management",
      icon: <BookOpen className="w-5 h-5 text-purple-400" />,
      path: "/app/events/learning-management",
    },
  ];

  return (
    <div className="bg-gradient-to-r from-[#E0F7FA] to-[#E0F2FE] rounded-2xl p-10 shadow-md mt-0">
      <h3 className="text-[20px] font-semibold text-[#0284c7] mb-4">
        Quick Events
      </h3>
      <div className="flex flex-wrap gap-4">
        {quickEvents.map((event, index) => (
          <button
            key={index}
            onClick={() => navigate(event.path)}
            className="flex items-center gap-2 px-6 py-3 rounded-full text-[15px] font-medium text-[#1E293B] shadow-sm hover:shadow-md hover:bg-[#F1F5F9] transition"
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
