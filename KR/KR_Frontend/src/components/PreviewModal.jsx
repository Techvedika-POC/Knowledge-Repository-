import React, { useState } from "react";
import { BookOpen, Tag, Calendar, User, Layers, Globe, Code2, X } from "lucide-react";

export default function PreviewModal({ item, onClose }) {
  const [showFullDesc, setShowFullDesc] = useState(false);
  if (!item) return null;

  const parseDbJsonArray = (str) => {
    if (!str) return [];
    try {
      let cleaned = str.trim();
      if (cleaned.startsWith('"') && cleaned.endsWith('"')) {
        cleaned = cleaned.slice(1, -1);
      }
      cleaned = cleaned.replace(/""/g, '"');
      const parsed = JSON.parse(cleaned);
      return Array.isArray(parsed) ? parsed.filter(Boolean) : [];
    } catch {
      return [];
    }
  };

  const parseDbJsonObject = (str) => {
    if (!str) return {};
    try {
      let cleaned = str.trim();
      if (cleaned.startsWith('"') && cleaned.endsWith('"')) cleaned = cleaned.slice(1, -1);
      cleaned = cleaned.replace(/""/g, '"');
      const parsed = JSON.parse(cleaned);
      return typeof parsed === "object" && parsed !== null ? parsed : {};
    } catch {
      return {};
    }
  };

  const languages = parseDbJsonArray(item.language);
  const frameworks = parseDbJsonArray(item.framework);
  const metadataObj = parseDbJsonObject(item.metadata);
  const description = item.description || "No description available.";

  const details = [
    { label: "Owner", value: item.ownerName || "Unknown", icon: <User size={14} /> },
    { label: "Created", value: item.createdOn ? new Date(item.createdOn).toLocaleString() : "N/A", icon: <Calendar size={14} /> },
    { label: "Category", value: item.categoryName || "N/A", icon: <Layers size={14} /> },
    { label: "Domain", value: item.domainName || "N/A", icon: <Globe size={14} /> },
    { label: "Languages", value: languages.join(", ") || "N/A", icon: <Code2 size={14} /> },
    { label: "Frameworks", value: frameworks.join(", ") || "N/A", icon: <Layers size={14} /> },
  ];

  return (
    <div className="fixed inset-0 z-50 bg-black/40 flex items-center justify-center p-4">
      <div className="bg-gray-50 w-full max-w-5xl rounded-xl shadow-2xl overflow-hidden">

        <div className="flex items-center justify-between px-6 py-2 border-b bg-white">
          <div className="flex items-center gap-3">
            <div className="p-1 rounded-md bg-indigo-50 text-indigo-600">
              <BookOpen size={18} />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-gray-900">
                {item.title || "Untitled Knowledge Article"}
              </h2>
              <p className="text-xs text-gray-500">
                Knowledge Preview
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-md hover:bg-gray-100"
          >
            <X size={18} />
          </button>
        </div>
        <div className="p-5 space-y-2 overflow-y-auto max-h-[75vh]">
          {item.feedback && (
            <section className="mb-4 rounded-lg border border-red-200 bg-red-50 p-4">
              <h3 className="text-sm font-semibold text-red-700 mb-1">
                Reviewer Feedback
              </h3>
              <p className="text-sm text-red-800 leading-relaxed">
                {item.feedback}
              </p>
            </section>
          )}
          <section className="bg-white rounded-lg border p-2">
            <h3 className="text-xs font-semibold text-gray-500 uppercase mb-1">
              Description
            </h3>
            <p className="text-sm text-gray-700 leading-relaxed">
              {showFullDesc ? description : description.slice(0, 300)}
              {description.length > 300 && !showFullDesc && "…"}
            </p>
            {description.length > 300 && (
              <button
                onClick={() => setShowFullDesc(!showFullDesc)}
                className="mt-1 text-xs text-indigo-600 hover:underline"
              >
                {showFullDesc ? "Show less" : "Show more"}
              </button>
            )}
          </section>

          <section className="bg-white rounded-lg border p-4">
            <h3 className="text-xs font-semibold text-gray-500 uppercase mb-3">
              Details
            </h3>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
              {details.map((d, idx) => (
                <div
                  key={idx}
                  className="flex items-start gap-2 p-2 rounded-md bg-gray-50"
                >
                  <div className="text-indigo-600 mt-0.5">{d.icon}</div>
                  <div>
                    <p className="text-[11px] text-gray-500">{d.label}</p>
                    <p className="text-sm font-medium text-gray-800 leading-tight">
                      {d.value}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </section>

          {item.tags?.length > 0 && (
            <section className="bg-white rounded-lg border p-4">
              <h3 className="text-xs font-semibold text-gray-500 uppercase mb-2">
                Tags
              </h3>
              <div className="flex flex-wrap gap-1.5">
                {item.tags.map((tag, idx) => (
                  <span
                    key={idx}
                    className="px-2 py-0.5 text-[11px] rounded-full bg-indigo-50 text-indigo-700 border border-indigo-100"
                  >
                    {tag}
                  </span>
                ))}
              </div>
            </section>
          )}

          {Object.keys(metadataObj).length > 0 && (
            <section className="bg-white rounded-lg border p-4">
              <h3 className="text-xs font-semibold text-gray-500 uppercase mb-2">
                Metadata
              </h3>
              <pre className="text-[11px] bg-gray-50 p-3 rounded-md overflow-x-auto text-gray-700">
                {JSON.stringify(metadataObj, null, 2)}
              </pre>
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
