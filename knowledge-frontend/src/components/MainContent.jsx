import React, { useState } from "react";
import {
  FaFilter,
  FaCalendarAlt,
  FaFolderOpen,
  FaLayerGroup,
  FaRobot,
} from "react-icons/fa";
import "../index.css";

const MainContent = () => {
  const cards = [
    {
      title: "Zero Trust Security Playbook",
      description:
        "Architectural guidelines and reference implementations for zero trust deployments.",
      tags: ["Security", "Architecture", "Best Practices"],
    },
    {
      title: "Designing Robust APIs",
      description:
        "Principles for versioning, schema evolution, and scaling REST/GraphQL services.",
      tags: ["APIs", "Scalability", "Guides"],
    },
    {
      title: "Cloud Database Patterns",
      description:
        "Trade-offs across SQL/NoSQL, sharding, replication, and consistency models.",
      tags: ["Database", "Cloud", "Patterns"],
    },
    {
      title: "AI Governance Framework",
      description:
        "Standards and policies for ethical AI deployment and risk management.",
      tags: ["AI", "Governance", "Policy"],
    },
  ];

  const quickEvents = [
    "Ideathons",
    "Hackathons",
    "Coding Challenges",
    "Knowledge Quest",
  ];

  const leaderboard = [
    {
      title: "Top Contributor: Ava Patel",
      description: "Highest rank this week with API governance patterns.",
      rank: 1,
    },
    {
      title: "Rising Star: Leo Marti",
      description: "Distributed tracing deep-dives and examples.",
      rank: 2,
    },
    {
      title: "Community MVP: Mei Chen",
      description: "Data contracts and governance insights.",
      rank: 3,
    },
    {
      title: "Knowledge Curator: Omar Ali",
      description: "Microservices handbooks and examples.",
      rank: 4,
    },
  ];

  const highlightList = [
    "Fresh Picks",
    "Trending",
    "Topic Highlights",
    "Day Spotlight",
  ];

  const [showAllCards, setShowAllCards] = useState(false);

  return (
    <div className="flex flex-col gap-7 w-full p-2">
      {/* Header */}
      <div className="relative flex justify-center items-center p-2">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900">
            KnowLedger Synaptix
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            Innovate • Collaborate • Build
          </p>
        </div>
      </div>

      {/* Filters + Ask Assistant */}
      <div className="bg-white rounded-xl p-4 shadow flex flex-col lg:flex-row gap-6">
        <div className="flex flex-wrap gap-3 items-center flex-1">
          <button className="px-4 py-2 rounded-full bg-gray-100 text-gray-700 text-sm flex items-center gap-2">
            <FaLayerGroup /> Domain
          </button>
          <button className="px-4 py-2 rounded-full bg-gray-100 text-gray-700 text-sm flex items-center gap-2">
            <FaFolderOpen /> Category
          </button>
          <button className="px-4 py-2 rounded-full bg-gray-100 text-gray-700 text-sm flex items-center gap-2">
            <FaCalendarAlt /> Date
          </button>
          <button className="px-4 py-2 rounded-full bg-gray-100 text-gray-700 text-sm flex items-center gap-2">
            <FaFilter /> Browse All
          </button>
        </div>

        {/* Ask Assistant */}
        <div className="flex-none w-full lg:w-80">
          <div className="bg-indigo-50 rounded-xl p-4 flex flex-col gap-3">
            <div className="flex items-center gap-2">
              <FaRobot className="text-indigo-600 text-lg" />
              <p className="font-semibold text-gray-800">Ask Assistant</p>
            </div>
            <div className="flex justify-between gap-3">
              <div className="flex flex-col gap-2 flex-1">
                <button className="px-3 py-2 bg-indigo-100 rounded-md text-sm text-left">
                  Ask about Zero Trust
                </button>
                <button className="px-3 py-2 bg-indigo-100 rounded-md text-sm text-left">
                  Find latest API guides
                </button>
                <button className="px-3 py-2 bg-indigo-100 rounded-md text-sm text-left">
                  Who leads observability?
                </button>
              </div>
              <button className="bg-indigo-600 text-white px-4 py-2 rounded-md text-sm self-start">
                Ask
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Knowledge Section */}
      <div className="bg-white rounded-xl p-4 shadow flex flex-col gap-4">
        {/* Highlights */}
        <div className="flex justify-between items-center gap-3">
          <div className="flex gap-3 flex-wrap">
            {highlightList.map((hl, i) => (
              <span
                key={i}
                className={`px-3 py-1 rounded-full text-sm font-medium ${
                  i === 0
                    ? "bg-green-100"
                    : i === 1
                    ? "bg-blue-100"
                    : i === 2
                    ? "bg-purple-100"
                    : "bg-yellow-100"
                }`}
              >
                {hl}
              </span>
            ))}
          </div>
          <button
            className="px-3 py-1 bg-indigo-600 text-white rounded-md text-sm"
            onClick={() => setShowAllCards(!showAllCards)}
          >
            {showAllCards ? "View Less" : "View More"}
          </button>
        </div>

        {/* Cards */}
        <div className="flex flex-wrap gap-6">
          {(showAllCards ? cards : cards.slice(0, 3)).map((card, idx) => (
            <div
              key={idx}
              className="bg-white rounded-xl p-5 shadow w-72 flex flex-col"
            >
              <h4 className="text-lg font-semibold mb-2">{card.title}</h4>
              <p className="text-sm text-gray-600 mb-3">{card.description}</p>
              <div className="flex flex-wrap gap-2 mb-4">
                {card.tags.map((tag, t) => (
                  <span
                    key={t}
                    className="bg-gray-100 px-2 py-1 rounded text-xs"
                  >
                    {tag}
                  </span>
                ))}
              </div>
              <div className="flex gap-2 mt-auto">
                <button className="px-3 py-1 bg-gray-100 rounded-md text-sm">
                  Like
                </button>
                <button className="px-3 py-1 bg-gray-100 rounded-md text-sm">
                  Dislike
                </button>
                <button className="px-3 py-1 bg-gray-100 rounded-md text-sm">
                  Comment
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Quick Events */}
      <div className="bg-white rounded-xl p-4 shadow flex gap-3 flex-wrap">
        {quickEvents.map((ev, i) => (
          <button
            key={i}
            className="px-4 py-2 bg-gray-100 rounded-md text-sm"
          >
            {ev}
          </button>
        ))}
      </div>

      {/* Leaderboard */}
      <div className="bg-white rounded-xl p-4 shadow flex flex-col gap-3">
        {leaderboard.map((item, idx) => (
          <div
            key={idx}
            className="flex items-center gap-4 bg-gray-50 p-3 rounded-lg"
          >
            <span className="bg-yellow-400 text-black px-3 py-1 rounded-full font-bold text-sm flex-shrink-0">
              #{item.rank}
            </span>
            <div>
              <h5 className="text-sm font-semibold">{item.title}</h5>
              <p className="text-xs text-gray-600">{item.description}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default MainContent;
