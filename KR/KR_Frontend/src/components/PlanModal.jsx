import React, { useState, useEffect } from "react";
import toast from "react-hot-toast";
import api from "../api";

export default function PlanModal({ plan, onClose, onPlanSaved }) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [durationWeeks, setDurationWeeks] = useState(0);
  const [totalDays, setTotalDays] = useState(0);
  const [overview, setOverview] = useState("");
  const [objectives, setObjectives] = useState("");
  const [prerequisites, setPrerequisites] = useState("");
  const [technicalRequirements, setTechnicalRequirements] = useState("");
  const [selfAssessmentChecklist, setSelfAssessmentChecklist] = useState("");

  useEffect(() => {
    if (!plan) return;

    setTitle(plan.title ?? "");
    setDescription(plan.description ?? "");
    setDurationWeeks(plan.durationWeeks ?? 0);
    setTotalDays(plan.totalDays ?? 0);
    setOverview(plan.overview ?? "");
    setObjectives(plan.objectives ?? "");
    setPrerequisites(plan.prerequisites ?? "");
    setTechnicalRequirements(plan.technicalRequirements ?? "");
    setSelfAssessmentChecklist(plan.selfAssessmentChecklist ?? "");
  }, [plan]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!title.trim()) {
      toast.error("Please provide a plan title");
      return;
    }

    const payload = {
      title,
      description,
      durationWeeks,
      totalDays,
      overview,
      objectives,
      prerequisites,
      technicalRequirements,
      selfAssessmentChecklist,
      weeks: []
    };

    try {
      let res;

      if (plan?.planId) {
        res = await api.put(`/LearningPlan/${plan.planId}`, payload);
        toast.success("Plan updated successfully");
      } else {
        res = await api.post(`/LearningPlan/create-full`, payload);
        toast.success("Plan created successfully");
      }

      if (onPlanSaved) onPlanSaved(res.data || payload);
      onClose();
    } catch (err) {
      console.error(err);
      toast.error(`Failed to save plan: ${err.response?.data || err.message}`);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex justify-center items-center z-50 p-5">
      <div className="
        bg-white/90 p-8 rounded-3xl shadow-2xl 
        w-full max-w-3xl max-h-[92vh] overflow-y-auto 
        border border-gray-200
      ">

        {/* HEADER */}
        <div className="text-black px-2 py-2 mb-3 rounded-xl text-center">
          <h2 className="text-2xl font-bold tracking-wide">
            {plan ? "Update Learning Plan" : "Create Learning Plan"}
          </h2>
          <p className="text-xs opacity-80 mt-1">
            Define the structure and outcomes for this learning journey.
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">

          {/* TITLE */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              Plan Title
            </label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g., Generative AI Mastery Program"
              className="w-full p-2.5 rounded-lg bg-gray-50 border border-gray-300 shadow-sm
                         focus:ring-2 focus:ring-indigo-300 focus:border-indigo-400 text-sm"
              required
            />
          </div>

          {/* DESCRIPTION */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              Short Description
            </label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="A brief overview describing the purpose and scope of this learning plan."
              className="w-full p-2.5 rounded-lg bg-gray-50 border border-gray-300 shadow-sm
                         focus:ring-2 focus:ring-indigo-300 focus:border-indigo-400 h-20 text-sm"
            />
          </div>

          {/* WEEKS & DAYS */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-1">
                Duration (Weeks)
              </label>
              <input
                type="number"
                value={durationWeeks}
                min={1}
                placeholder="e.g., 6"
                onChange={(e) => setDurationWeeks(Number(e.target.value))}
                className="w-full p-2.5 rounded-lg bg-gray-50 border border-gray-300 shadow-sm 
                           focus:ring-2 focus:ring-indigo-300 focus:border-indigo-400 text-sm"
              />
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-1">
                Total Days
              </label>
              <input
                type="number"
                value={totalDays}
                min={1}
                placeholder="e.g., 30"
                onChange={(e) => setTotalDays(Number(e.target.value))}
                className="w-full p-2.5 rounded-lg bg-gray-50 border border-gray-300 shadow-sm 
                           focus:ring-2 focus:ring-indigo-300 focus:border-indigo-400 text-sm"
              />
            </div>
          </div>

          {/* OVERVIEW */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              Full Overview
            </label>
            <textarea
              value={overview}
              onChange={(e) => setOverview(e.target.value)}
              placeholder="Describe the full journey learners will experience and key concepts covered."
              className="w-full p-2.5 rounded-lg bg-gray-50 border border-gray-300 shadow-sm
                         focus:ring-2 focus:ring-indigo-300 h-24 text-sm"
            />
          </div>

          {/* OBJECTIVES */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              Learning Objectives
            </label>
            <textarea
              value={objectives}
              onChange={(e) => setObjectives(e.target.value)}
              placeholder="List the measurable outcomes learners should achieve by completing this plan."
              className="w-full p-2.5 rounded-lg bg-gray-50 border border-gray-300 shadow-sm 
                         focus:ring-2 focus:ring-indigo-300 h-24 text-sm"
            />
          </div>

          {/* PREREQUISITES */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              Prerequisites
            </label>
            <textarea
              value={prerequisites}
              onChange={(e) => setPrerequisites(e.target.value)}
              placeholder="Mention any required prior knowledge (e.g., Python basics, ML fundamentals)."
              className="w-full p-2.5 rounded-lg bg-gray-50 border border-gray-300 shadow-sm 
                         focus:ring-2 focus:ring-indigo-300 h-20 text-sm"
            />
          </div>

          {/* TECHNICAL REQUIREMENTS */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              Technical Requirements
            </label>
            <textarea
              value={technicalRequirements}
              onChange={(e) => setTechnicalRequirements(e.target.value)}
              placeholder="List required tools (e.g., VS Code, API keys, GPUs, Python 3.12, internet access)."
              className="w-full p-2.5 rounded-lg bg-gray-50 border border-gray-300 shadow-sm 
                         focus:ring-2 focus:ring-indigo-300 h-20 text-sm"
            />
          </div>

          {/* SELF-ASSESSMENT */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              Self-Assessment Checklist
            </label>
            <textarea
              value={selfAssessmentChecklist}
              onChange={(e) => setSelfAssessmentChecklist(e.target.value)}
              placeholder={`e.g.,
✓ I understand the basics of LLMs  
✓ I can write Python scripts  
✓ I can work with APIs`}
              className="w-full p-2.5 rounded-lg bg-gray-50 border border-gray-300 shadow-sm 
                         focus:ring-2 focus:ring-indigo-300 h-20 text-sm whitespace-pre-line"
            />
          </div>

          {/* BUTTONS */}
          <div className="flex justify-end gap-3 pt-3 border-t border-gray-200">
            <button
              type="button"
              onClick={onClose}
              className="px-5 py-2.5 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300 text-sm"
            >
              Cancel
            </button>

            <button
              type="submit"
              className="px-6 py-2.5 rounded-lg bg-indigo-600 text-white text-sm
                         hover:bg-indigo-700 shadow-md active:scale-95 transition"
            >
              {plan ? "Save Changes" : "Create Plan"}
            </button>
          </div>

        </form>
      </div>
    </div>
  );
}
