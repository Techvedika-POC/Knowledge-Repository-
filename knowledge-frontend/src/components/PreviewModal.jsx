import React, { useState } from "react";
import { FileText, Tag, Calendar, User, Layers, Globe, Type } from "lucide-react";

export default function PreviewModal({ item, onClose }) {
  const [showFullDesc, setShowFullDesc] = useState(false);
  if (!item) return null;

 
  const parseDbJsonArray = (str) => {
    if (!str) return [];
    try {
      // Remove outer quotes if they exist
      let cleaned = str.trim();
      if (cleaned.startsWith('"') && cleaned.endsWith('"')) {
        cleaned = cleaned.slice(1, -1);
      }
      // Replace double quotes with single quotes for valid JSON
      cleaned = cleaned.replace(/""/g, '"');
      const parsed = JSON.parse(cleaned);
      return Array.isArray(parsed) ? parsed.filter(Boolean) : [];
    } catch (err) {
      console.error("Error parsing DB array", err, str);
      return [];
    }
  };

  const parseDbJsonObject = (str) => {
    if (!str) return {};
    try {
      let cleaned = str.trim();
      if (cleaned.startsWith('"') && cleaned.endsWith('"')) {
        cleaned = cleaned.slice(1, -1);
      }
      cleaned = cleaned.replace(/""/g, '"');
      const parsed = JSON.parse(cleaned);
      return typeof parsed === "object" && parsed !== null ? parsed : {};
    } catch (err) {
      console.error("Error parsing DB object", err, str);
      return {};
    }
  };

  const languages = parseDbJsonArray(item.language);
  const frameworks = parseDbJsonArray(item.framework);
  const metadataObj = parseDbJsonObject(item.metadata);

  const description = item.description || item.Description || "No description available.";

  const details = [
    { label: "Title", value: item.title || item.Title || "Untitled", icon: <Type className="w-4 h-4 text-purple-500" /> },
    { label: "Owner", value: item.ownerName || "Unknown", icon: <User className="w-4 h-4 text-purple-500" /> },
    { label: "Created On", value: item.createdOn ? new Date(item.createdOn).toLocaleString() : "Unknown", icon: <Calendar className="w-4 h-4 text-indigo-500" /> },
    { label: "Category", value: item.categoryName || "N/A", icon: <Layers className="w-4 h-4 text-pink-500" /> },
    { label: "Domain", value: item.domainName || "N/A", icon: <Globe className="w-4 h-4 text-green-500" /> },
    { label: "Engagement Score", value: item.engagementScore ?? "0", icon: <Tag className="w-4 h-4 text-yellow-500" /> },
    { label: "Languages", value: languages.length ? languages.join(", ") : "N/A", icon: <FileText className="w-4 h-4 text-indigo-500" /> },
    { label: "Frameworks", value: frameworks.length ? frameworks.join(", ") : "N/A", icon: <Layers className="w-4 h-4 text-purple-500" /> },
  ];

  return (
    <div className="fixed inset-0 z-50 flex justify-center items-start bg-black bg-opacity-50 backdrop-blur-sm animate-fadeIn">
      <div className="relative bg-white rounded-3xl shadow-2xl w-full max-w-4xl mt-20 mb-20 flex flex-col overflow-hidden">

        {/* Header */}
        <div className="flex justify-between items-center px-6 py-4 bg-gradient-to-r from-indigo-600 to-purple-600 shadow-md sticky top-0 z-10">
          <h2 className="text-2xl font-bold text-white">Knowledge Article Preview</h2>
          <button onClick={onClose} className="text-white text-2xl font-bold hover:text-gray-300 transition">&times;</button>
        </div>

        {/* Content */}
        <div className="p-8 overflow-y-auto max-h-[80vh] space-y-10">

          {/* Description */}
          <div>
            <div className="flex items-center gap-3 mb-3">
              <FileText className="w-5 h-5 text-indigo-600" />
              <h3 className="text-lg font-semibold text-indigo-700">Description</h3>
            </div>
            <p className="text-gray-700 text-sm mb-2 break-words">
              {showFullDesc ? description : `${description.slice(0, 150)}${description.length > 150 ? "..." : ""}`}
            </p>
            {description.length > 150 && (
              <button
                className="text-blue-600 font-medium text-sm hover:underline"
                onClick={() => setShowFullDesc(!showFullDesc)}
              >
                {showFullDesc ? "View Less" : "View More"}
              </button>
            )}
          </div>

          {/* Details Grid */}
          <div>
            <div className="flex items-center gap-3 mb-3">
              <Layers className="w-5 h-5 text-indigo-600" />
              <h3 className="text-lg font-semibold text-indigo-700">Details</h3>
            </div>
            <div className="grid sm:grid-cols-2 md:grid-cols-3 gap-4">
              {details.map((d, idx) => (
                <div key={idx} className="flex flex-col gap-2 bg-white border border-gray-100 rounded-xl shadow-sm hover:shadow-md p-4 transition transform hover:scale-[1.02]">
                  <div className="flex items-center gap-2">
                    {d.icon}
                    <span className="text-xs font-semibold text-gray-500 uppercase">{d.label}</span>
                  </div>
                  <span className="text-sm font-bold text-gray-800 break-words">{d.value}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Tags */}
          {item.tags?.length > 0 && (
            <div>
              <div className="flex items-center gap-3 mb-3">
                <Tag className="w-5 h-5 text-pink-600" />
                <h3 className="text-lg font-semibold text-pink-700">Tags</h3>
              </div>
              <div className="flex flex-wrap gap-2">
                {item.tags.map((tag, idx) => (
                  <span key={idx} className="px-4 py-1 text-sm font-medium rounded-full bg-gradient-to-r from-pink-50 to-purple-50 text-purple-800 shadow-sm border border-purple-100">{tag}</span>
                ))}
              </div>
            </div>
          )}

          {/* Metadata */}
          {Object.keys(metadataObj).length > 0 && (
            <div>
              <div className="flex items-center gap-3 mb-3">
                <FileText className="w-5 h-5 text-green-600" />
                <h3 className="text-lg font-semibold text-green-700">Metadata</h3>
              </div>
              <pre className="p-4 bg-gray-50 rounded-xl shadow-inner text-sm text-gray-700 overflow-x-auto">
                {JSON.stringify(metadataObj, null, 2)}
              </pre>
            </div>
          )}

        </div>
      </div>
    </div>
  );
}
