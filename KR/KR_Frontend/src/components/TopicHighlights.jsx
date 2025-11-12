import React from "react";
export default function TopicHighlights({
  selectedDomain,
  domainKnowledgeItems,
  onItemClick,
}) {
  if (!selectedDomain) return null;
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 p-4">
      {domainKnowledgeItems.map((item, idx) => (
        <div
          key={idx}
          className="bg-white rounded-2xl shadow-md p-6 flex flex-col border border-gray-200 cursor-pointer group"
          onClick={() => onItemClick(idx)}
        >
          <h4 className="text-lg font-bold text-gray-900 mb-2">{item.title}</h4>
          <p className="text-sm text-gray-700 mb-4 line-clamp-3">{item.description}</p>
          {item.tags?.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {item.tags.map((tag, tIdx) => (
                <span
                  key={tIdx}
                  className="px-2 py-1 text-xs rounded-full bg-purple-100 text-purple-800"
                >
                  {tag}
                </span>
              ))}
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
