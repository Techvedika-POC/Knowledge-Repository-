import React, { useState, useEffect } from "react";
import toast from "react-hot-toast";
import api from "../api";

export default function WeekModal({ planId, week, onClose, onWeekCreated }) {
  const [title, setTitle] = useState("");
  const [weekNumber, setWeekNumber] = useState(1);
  const [learningObjectives, setLearningObjectives] = useState("");
  const [prerequisites, setPrerequisites] = useState("");
  useEffect(() => {
    if (week) {
      setTitle(week.title || "");
      setWeekNumber(week.weekNumber || 1);
      setLearningObjectives(week.learningObjectives || "");
      setPrerequisites(week.prerequisites || "");
    } else {
      setTitle("");
      setWeekNumber(1);
      setLearningObjectives("");
      setPrerequisites("");
    }
  }, [week]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!title.trim()) {
      toast.error("Week title is required");
      return;
    }

    try {
      if (week) {
        await api.put(`/Week/${week.weekId}`, {
          title,
          weekNumber,
          learningObjectives,
          prerequisites,
        });

        toast.success("Week updated");
      }

      else {
        const res = await api.post(`/Week/${planId}`, {
          title,
          weekNumber,
          learningObjectives,
          prerequisites,
        });

        toast.success("Week created");

        if (onWeekCreated) onWeekCreated(res.data);
      }

      onClose();
    } catch (err) {
      console.error(err);
      toast.error("Operation failed");
    }
  };

return (
  <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex justify-center items-center z-50">
    <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto p-8 animate-fadeIn">

      {/* HEADER */}
      <h2 className="text-2xl font-bold mb-6 text-gray-800 tracking-tight">
        {week ? "Edit Week" : "Create New Week"}
      </h2>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="relative">
          <label className="text-sm font-semibold text-gray-700 mb-1 block">
            Week Title
          </label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="e.g. Week 1 – Introduction to RAG"
            className="w-full border border-gray-300 px-4 py-3 rounded-lg 
                       text-gray-800 bg-gray-50
                       focus:bg-white focus:ring-2 focus:ring-blue-400 focus:border-blue-400 
                       transition shadow-sm"
          />
        </div>
        <div className="relative">
          <label className="text-sm font-semibold text-gray-700 mb-1 block">
            Week Number
          </label>
          <input
            type="number"
            value={weekNumber}
            onChange={(e) => setWeekNumber(Number(e.target.value))}
            min={1}
            placeholder="e.g. 1"
            className="w-full border border-gray-300 px-4 py-3 rounded-lg 
                       bg-gray-50 text-gray-800
                       focus:bg-white focus:ring-2 focus:ring-blue-400 focus:border-blue-400 
                       transition shadow-sm"
          />
        </div>
        <div>
          <label className="text-sm font-semibold text-gray-700 mb-1 block">
            Learning Objectives
          </label>
          <textarea
            value={learningObjectives}
            onChange={(e) => setLearningObjectives(e.target.value)}
            placeholder="Describe the goals for this week (e.g., 'Understand RAG basics, build vector search pipeline...')"
            className="w-full border border-gray-300 px-4 py-3 rounded-lg h-28 
                       bg-gray-50 text-gray-800
                       focus:bg-white focus:ring-2 focus:ring-blue-400 focus:border-blue-400 
                       transition shadow-sm"
          />
        </div>
        <div>
          <label className="text-sm font-semibold text-gray-700 mb-1 block">
            Prerequisites
          </label>
          <textarea
            value={prerequisites}
            onChange={(e) => setPrerequisites(e.target.value)}
            placeholder="e.g., Python basics, API knowledge, vector embeddings understanding..."
            className="w-full border border-gray-300 px-4 py-3 rounded-lg h-28 
                       bg-gray-50 text-gray-800
                       focus:bg-white focus:ring-2 focus:ring-blue-400 focus:border-blue-400 
                       transition shadow-sm"
          />
        </div>
        <div className="flex justify-end gap-4 mt-6 pt-4 border-t">

          <button
            type="button"
            onClick={onClose}
            className="px-5 py-2.5 rounded-lg border border-gray-300 bg-gray-100 
                       text-gray-700 hover:bg-gray-200 hover:border-gray-400
                       transition font-medium shadow-sm"
          >
            Cancel
          </button>

          <button
            type="submit"
            className="px-6 py-2.5 rounded-lg bg-blue-600 text-white font-medium 
                       hover:bg-blue-700 transition shadow-md
                       active:scale-95"
          >
            {week ? "Update Week" : "Create Week"}
          </button>

        </div>

      </form>
    </div>
  </div>
);

}