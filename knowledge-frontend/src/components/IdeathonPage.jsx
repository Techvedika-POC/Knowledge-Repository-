import React from "react";
import { Calendar, Users, Trophy, FileText, Rocket } from "lucide-react";
import { motion } from "framer-motion";

export default function IdeathonPage() {
  const eventDetails = {
    title: "🌍 Innovation Ideathon – October 2025",
    description:
      "Join the monthly Innovation Ideathon! Form teams, brainstorm creative solutions to our sustainability challenges, and pitch your ideas to the board. Winning ideas move into project development and get published as knowledge items!",
    startDate: "October 5, 2025",
    endDate: "October 25, 2025",
    owners: ["Innovation Council", "Tech Excellence Board"],
    theme: "Sustainable AI for a Greener Future",
  };

  const timeline = [
    { phase: "Team Formation & Registration", date: "Oct 5 – Oct 10" },
    { phase: "Idea Submission Deadline", date: "Oct 15" },
    { phase: "Midway Mentor Review", date: "Oct 18" },
    { phase: "Final Presentations", date: "Oct 24" },
    { phase: "Winner Announcement", date: "Oct 25" },
  ];

  const challenges = [
    {
      title: "AI for Carbon Footprint Reduction",
      desc: "Develop an idea using AI to optimize energy usage and reduce emissions.",
    },
    {
      title: "Circular Economy Tech",
      desc: "Propose technology-driven models to promote waste reuse and recycling.",
    },
    {
      title: "Smart Agriculture with AIoT",
      desc: "Leverage sensors and machine learning to support sustainable farming.",
    },
  ];

  return (
    <motion.div
      className="p-8 bg-gradient-to-br from-blue-50 to-white rounded-3xl shadow-xl"
      initial={{ opacity: 0, y: 40 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.6 }}
    >
      {/* Header Section */}
      <div className="flex flex-col gap-4 mb-8">
        <h2 className="text-3xl font-extrabold text-blue-700">{eventDetails.title}</h2>
        <p className="text-gray-700 text-lg leading-relaxed">
          {eventDetails.description}
        </p>
        <div className="flex flex-wrap gap-4 mt-4 text-gray-600">
          <span className="flex items-center gap-2">
            <Calendar className="w-5 h-5 text-blue-500" />
            <strong>Start:</strong> {eventDetails.startDate}
          </span>
          <span className="flex items-center gap-2">
            <Calendar className="w-5 h-5 text-red-500" />
            <strong>End:</strong> {eventDetails.endDate}
          </span>
          <span className="flex items-center gap-2">
            <Users className="w-5 h-5 text-green-500" />
            <strong>Owners:</strong> {eventDetails.owners.join(", ")}
          </span>
        </div>
      </div>

      {/* Theme */}
      <motion.div
        className="bg-blue-100 border-l-4 border-blue-500 p-4 rounded-lg mb-8"
        initial={{ x: -40, opacity: 0 }}
        animate={{ x: 0, opacity: 1 }}
      >
        <h3 className="text-xl font-semibold text-blue-700 mb-1">Theme of the Month</h3>
        <p className="text-blue-800 font-medium">{eventDetails.theme}</p>
      </motion.div>

      {/* Timeline Section */}
      <div className="mb-10">
        <h3 className="text-2xl font-semibold text-gray-800 mb-4">📅 Event Timeline</h3>
        <div className="space-y-4">
          {timeline.map((item, index) => (
            <motion.div
              key={index}
              className="flex justify-between items-center bg-white p-4 rounded-xl shadow-sm border"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.1 }}
            >
              <span className="font-medium text-gray-700">{item.phase}</span>
              <span className="text-sm text-gray-500">{item.date}</span>
            </motion.div>
          ))}
        </div>
      </div>

      {/* Challenge Cards */}
      <div className="mb-10">
        <h3 className="text-2xl font-semibold text-gray-800 mb-4">💡 Featured Challenges</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {challenges.map((c, i) => (
            <motion.div
              key={i}
              className="bg-white rounded-2xl p-6 shadow-md hover:shadow-lg transition flex flex-col justify-between"
              whileHover={{ scale: 1.03 }}
            >
              <div>
                <h4 className="text-lg font-semibold text-blue-700 mb-2">{c.title}</h4>
                <p className="text-gray-600 text-sm">{c.desc}</p>
              </div>
              <button className="mt-4 bg-blue-600 text-white px-4 py-2 rounded-full text-sm font-medium hover:bg-blue-700 transition">
                Submit Idea
              </button>
            </motion.div>
          ))}
        </div>
      </div>

      {/* CTA Section */}
      <div className="flex flex-wrap gap-4 justify-center mt-10">
        <button className="flex items-center gap-2 px-6 py-3 bg-green-600 text-white rounded-full font-semibold hover:bg-green-700 transition">
          <Rocket className="w-5 h-5" /> Join Event
        </button>
        <button className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-full font-semibold hover:bg-blue-700 transition">
          <FileText className="w-5 h-5" /> Submit Idea
        </button>
        <button className="flex items-center gap-2 px-6 py-3 bg-yellow-500 text-white rounded-full font-semibold hover:bg-yellow-600 transition">
          <Trophy className="w-5 h-5" /> View Submissions
        </button>
      </div>
    </motion.div>
  );
}
