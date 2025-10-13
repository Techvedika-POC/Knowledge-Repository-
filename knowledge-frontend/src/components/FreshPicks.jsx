import React from "react";

const FreshPicks = () => {
  const highlightsData = [
    {
      title: "Zero Trust Security Playbook",
      description:
        "Architectural guidelines and reference implementations for zero trust deployments.",
      tags: [
        { label: "Security", color: "bg-blue-100 text-blue-700" },
        { label: "Architecture", color: "bg-green-100 text-green-700" },
        { label: "Best Practices", color: "bg-purple-100 text-purple-700" },
      ],
    },
    {
      title: "Designing Robust APIs",
      description:
        "Principles for versioning, schema evolution, and scaling REST/GraphQL services.",
      tags: [
        { label: "APIs", color: "bg-blue-100 text-blue-700" },
        { label: "Scalability", color: "bg-green-100 text-green-700" },
        { label: "Guides", color: "bg-purple-100 text-purple-700" },
      ],
    },
  ];

  return (
    <div className="bg-gradient-to-b from-indigo-50 to-white rounded-2xl p-6 shadow-md border border-indigo-100">
      {/* ==== Four Small Headings Row ==== */}
      <div className="flex flex-wrap justify-evenly items-center gap-6 mb-6 text-center">
        <h2 className="text-base font-semibold bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-500 text-transparent bg-clip-text">
          Fresh Picks
        </h2>

        <h2 className="text-base font-semibold bg-gradient-to-r from-pink-600 via-rose-500 to-orange-400 text-transparent bg-clip-text">
          Trending Topics
        </h2>

        <h2 className="text-base font-semibold bg-gradient-to-r from-green-500 via-emerald-400 to-teal-500 text-transparent bg-clip-text">
          Day Spotlight
        </h2>

        <h2 className="text-base font-semibold bg-gradient-to-r from-blue-500 via-sky-400 to-cyan-400 text-transparent bg-clip-text">
          Top Picks
        </h2>
      </div>

      {/* ==== Section Header ==== */}
      <h3 className="text-lg font-semibold text-gray-800 mb-4">
        Latest Highlights
      </h3>

      {/* ==== Cards ==== */}
      <div className="flex flex-wrap gap-6 justify-start">
        {highlightsData.map((card, idx) => (
          <div
            key={idx}
            className="bg-white border border-indigo-100 rounded-2xl shadow-sm hover:shadow-lg transition-all duration-300 p-5 flex flex-col w-full sm:w-[48%] lg:w-[30%]"
          >
            <h4 className="text-lg font-semibold text-gray-800 mb-2">
              {card.title}
            </h4>
            <p className="text-sm text-gray-600 mb-3 leading-relaxed">
              {card.description}
            </p>
            <div className="flex flex-wrap gap-2 mt-auto">
              {card.tags.map((tag, tIdx) => (
                <span
                  key={tIdx}
                  className={`px-2.5 py-1 rounded-full text-xs font-medium ${tag.color}`}
                >
                  {tag.label}
                </span>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default FreshPicks;
