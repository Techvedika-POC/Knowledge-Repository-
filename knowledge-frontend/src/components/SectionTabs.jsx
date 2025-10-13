import React from "react";

const SectionTabs = ({ tabs, activeTab, onTabChange }) => {
  return (
    <div className="flex gap-4 mb-4">
      {tabs.map((tab, idx) => (
        <button
          key={idx}
          onClick={() => onTabChange(tab)}
          className={`px-4 py-2 rounded-full text-sm font-medium ${
            activeTab === tab ? "bg-indigo-600 text-white" : "bg-gray-100 text-gray-600"
          }`}
        >
          {tab}
        </button>
      ))}
    </div>
  );
};

export default SectionTabs;
