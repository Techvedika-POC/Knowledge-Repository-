import React, { useEffect, useState, useRef } from "react";
import toast from "react-hot-toast";
import { DocumentArrowUpIcon } from "@heroicons/react/24/outline";

import {
  PlusIcon,
  PencilIcon,
  TrashIcon,
  FolderOpenIcon,
  XMarkIcon,
  CheckCircleIcon,
  ScaleIcon,
  InformationCircleIcon,
  CpuChipIcon
} from "@heroicons/react/24/outline";
import {
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer
} from "recharts";

import PlanModal from "./PlanModal";
import WeekModal from "./WeekModal";
import ModuleModal from "./ModuleModal";
import api from "../api";

export default function LearningPlanManager() {
  const [plans, setPlans] = useState([]);
  const [selectedPlan, setSelectedPlan] = useState(null);
  const [showPlanModal, setShowPlanModal] = useState(false);
  const [weeks, setWeeks] = useState([]);
  const [selectedWeek, setSelectedWeek] = useState(null);
  const [showWeekModal, setShowWeekModal] = useState(false);
  const [modules, setModules] = useState([]);
  const [selectedModule, setSelectedModule] = useState(null);
  const [showModuleModal, setShowModuleModal] = useState(false);
  const [pipelineReport, setPipelineReport] = useState(null);
  const [openedModuleDetail, setOpenedModuleDetail] = useState(null);
  const [showUploadModal, setShowUploadModal] = useState(false);
  const [uploadFile, setUploadFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [recentPlanId, setRecentPlanId] = useState(null);
  const moduleDetailRef = useRef(null);
  const weeksSectionRef = useRef(null);
  const modulesSectionRef = useRef(null);
  const [enrichedJson, setEnrichedJson] = useState(null);
  const [enriching, setEnriching] = useState(false);
  const [isEnriched, setIsEnriched] = useState(false);
  const [evaluationPayload, setEvaluationPayload] = useState(null);
  const [saving, setSaving] = useState(false);
  const [enrichedFields, setEnrichedFields] = useState([]);

  const normalizePipelineResult = (pipelineResult, phase) => ({
    phase,

    summary: {
      isValid: pipelineResult?.isValid,
      score: pipelineResult?.finalScore,
    },

    ruleBased: {
      ...pipelineResult?.ruleBased,
      missingFields: pipelineResult?.ruleBased?.missingFields ?? [],
      expectedStructure: pipelineResult?.ruleBased?.expectedData ?? {},
    },

    ml: {
      ...pipelineResult?.ml,
      metrics: pipelineResult?.ml?.metrics ?? {},
    },

    hybrid: {
      isValid: pipelineResult?.hybrid?.isValid,
      FinalScore: pipelineResult?.hybrid?.score,
      metrics: pipelineResult?.hybrid?.metrics ?? {},
    },
  });

  const uploadLearningPlan = async () => {
    if (!uploadFile) {
      toast.error("Please select a DOCX file");
      return;
    }

    const formData = new FormData();
    formData.append("file", uploadFile);

    setUploading(true);

    try {
      const { data } = await api.post(
        "/training-plans/upload-docx",
        formData,
        { headers: { "Content-Type": "multipart/form-data" } }
      );
      setPipelineReport({
        phase: data.phase,

        summary: {
          isValid: data.summary?.isValid,
          score: data.summary?.finalScore,
        },

        llmStructuredOutput: data.llmStructuredOutput,

        ruleBased: {
          ...data.ruleBased,
          missingFields: data.ruleBased?.missingFields ?? [],
          expectedStructure: data.ruleBased?.expectedStructure ?? {},
        },

        ml: {
          ...data.ml,
          metrics: data.ml?.metrics ?? {},
        },

        hybrid: {
          isValid: data.hybrid?.isValid,
          FinalScore: data.hybrid?.finalScore,
          metrics: data.hybrid?.metrics ?? {},
        },
      });
      setEvaluationPayload({
        rawText: data.payload?.rawText,
        structuredText: data.payload?.structuredText,
        llmJson: data.payload?.llmJson,
      });

      setIsEnriched(false);
      setEnrichedJson(null);
      setShowUploadModal(false);
      setUploadFile(null);

      toast.success("Evaluation completed successfully");
    } catch (err) {
      console.error(err);
      toast.error("Failed to upload learning plan");
    } finally {
      setUploading(false);
    }
  };
  const enrichLearningPlan = async () => {
    if (!evaluationPayload || !pipelineReport?.ruleBased) {
      toast.error("Evaluation data missing");
      return;
    }
    const missingFieldsBefore = pipelineReport.ruleBased.missingFields ?? [];
    setEnriching(true);

    try {
      const { data } = await api.post("/training-plans/enrich", {
        RawText: evaluationPayload.rawText,
        StructuredText: evaluationPayload.structuredText,
        LlmJson: evaluationPayload.llmJson,
        MissingFields: pipelineReport.ruleBased.MissingFields ?? [],
      });

      const enrichedString =
        typeof data.updatedJson === "string"
          ? data.updatedJson
          : JSON.stringify(data.updatedJson);

      setEnrichedJson(enrichedString);
      setIsEnriched(true);
      setEnrichedFields(data.enrichedFields ?? []);

      setPipelineReport(
        normalizePipelineResult(data.evaluation, "Enrichment")
      );

      toast.success("AI enrichment completed successfully");
    } catch (err) {
      console.error("Enrichment error:", err);
      toast.error("Enrichment failed");
    } finally {
      setEnriching(false);
    }
  };
  const savePlan = async () => {
    if (!isEnriched || !enrichedJson) {
      toast.error("No enriched training plan to save");
      return;
    }

    try {
      const payload =
        typeof enrichedJson === "string"
          ? JSON.parse(enrichedJson)
          : enrichedJson;

      const res = await fetch(
        "https://localhost:7001/api/training-plans/persist",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(payload),
        }
      );

      if (!res.ok) {
        const err = await res.text();
        throw new Error(err);
      }

      toast.success("Training plan saved successfully");

      setPipelineReport(null);
      setIsEnriched(false);
      setEnrichedJson(null);
      setEvaluationPayload(null);

      await loadPlans();
    } catch (err) {
      console.error(err);
      toast.error("Failed to save training plan");
    }
  };
  const SectionTitle = ({ icon: Icon, title, badge }) => (
    <div className="flex items-center gap-3 mb-4">
      <Icon className="w-5 h-5 text-slate-700" />
      <h3 className="text-lg font-semibold text-slate-800">{title}</h3>
      {badge && (
        <span className="text-xs bg-emerald-100 text-emerald-700 px-2 py-0.5 rounded-full">
          {badge}
        </span>
      )}
    </div>
  );
  const loadPlans = async () => {
    try {
      const res = await api.get("/LearningPlan");
      setPlans(res.data || []);
    } catch {
      toast.error("Failed to load plans");
    }
  };
  useEffect(() => {
    loadPlans();
  }, []);
  const managePlan = async (plan) => {
    const planId = plan?.planId ?? plan?.PlanId;

    if (!planId) {
      console.error("managePlan called with invalid plan:", plan);
      toast.error("Invalid plan selected");
      return;
    }
    const normalizedPlan = {
      planId,
      title: plan.title ?? plan.Title,
      overview: plan.overview ?? plan.Overview,
      description: plan.description ?? plan.Description,
      durationWeeks: plan.durationWeeks ?? plan.DurationWeeks,
      totalDays: plan.totalDays ?? plan.TotalDays,
    };

    setSelectedPlan(normalizedPlan);
    setSelectedWeek(null);
    setModules([]);
    setOpenedModuleDetail(null);
    setRecentPlanId(planId);

    try {
      const res = await api.get(`/Week/full/plan/${planId}`);
      setWeeks(res.data || []);

      setTimeout(() => {
        weeksSectionRef.current?.scrollIntoView({
          behavior: "smooth",
          block: "start",
        });
      }, 100);
    } catch (err) {
      console.error("Failed to load weeks:", err);
      toast.error("Failed to load weeks");
    }
  };
  const deletePlan = async (planId) => {
    if (!window.confirm("Delete this plan?")) return;
    try {
      await api.delete(`/LearningPlan/${planId}`);
      loadPlans();
      setSelectedPlan(null);
      setWeeks([]);
      setModules([]);
    } catch {
      toast.error("Failed to delete plan");
    }
  };
  const manageWeek = async (week) => {
    const weekId = week.weekId ?? week.WeekId;

    if (!weekId) {
      toast.error("Invalid week selected");
      return;
    }
    setSelectedWeek({
      weekId,
      title: week.title ?? week.Title,
      overview: week.overview ?? week.Overview,
      weekNumber: week.weekNumber ?? week.WeekNumber,
    });

    setOpenedModuleDetail(null);

    try {
      const res = await api.get(`/Module/week/${weekId}`);
      const normalizedModules = (res.data || []).map(m => ({
        moduleId: m.moduleId ?? m.ModuleId,
        moduleName: m.moduleName ?? m.ModuleName,
        description: m.description ?? m.Description,
        orderNo: m.orderNo ?? m.OrderNo,
        durationDays: m.durationDays ?? m.DurationDays,
      }));

      setModules(normalizedModules);

      setTimeout(() => {
        modulesSectionRef.current?.scrollIntoView({
          behavior: "smooth",
          block: "start",
        });
      }, 100);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load modules");
    }
  };
  const deleteWeek = async (weekId) => {
    if (!window.confirm("Delete this week?")) return;
    try {
      await api.delete(`/Week/${weekId}`);
      managePlan(selectedPlan);
    } catch {
      toast.error("Failed to delete week");
    }
  };
  const deleteModule = async (id) => {
    if (!window.confirm("Delete this module?")) return;
    try {
      await api.delete(`/Module/${id}`);
      manageWeek(selectedWeek);
    } catch {
      toast.error("Failed to delete module");
    }
  };
  const openModuleDetail = async (mod) => {
    const moduleId = mod.moduleId;

    if (!moduleId) {
      toast.error("Invalid module selected");
      return;
    }
    try {
      const res = await api.get(`/Module/${moduleId}`, {
        params: { userId: "00000000-0000-0000-0000-000000000001" },
      });
      setOpenedModuleDetail(res.data);
      setSelectedModule(mod);
      setTimeout(() => {
        moduleDetailRef.current?.scrollIntoView({ behavior: "smooth" });
      }, 100);
    } catch (err) {
      console.error("Module detail error:", err);
      toast.error("Failed to load module details");
    }
  };
  const sortedPlans = [...plans].sort((a, b) => {
    if (a.planId === recentPlanId) return -1;
    if (b.planId === recentPlanId) return 1;
    return 0;
  });
  const StatusBadge = ({ ok, text }) => (
    <span
      className={`px-2 py-0.5 rounded-full text-xs font-semibold ${ok
        ? "bg-emerald-100 text-emerald-700"
        : "bg-red-100 text-red-700"
        }`}
    >
      {text}
    </span>
  );
  const ml = pipelineReport?.ml ?? null;
  const mlConfidence =
    ml?.metrics?.ML_Confidence != null
      ? Math.round(ml.metrics.ML_Confidence * 100)
      : 0;
  // ===== QUALITY SIGNALS =====
  const qualitySignals = ml
    ? [
      { label: "Missing Fields", value: ml.metrics?.MissingFields ?? 0 },
      { label: "Structural Issues", value: ml.metrics?.StructuralErrors ?? 0 },
      { label: "Empty Content", value: ml.metrics?.EmptyContent ?? 0 },
      { label: "Uncertainty Signals", value: ml.metrics?.UncertaintySignals ?? 0 },
    ]
    : [];

  const hasQualitySignals = qualitySignals.some(s => s.value > 0);

  // ===== HYBRID DECISION LOGIC =====
  const hybridMetrics = pipelineReport?.hybrid?.metrics ?? {};

  // ===== SCORE COMPOSITION (ONLY IF BOTH WEIGHTS EXIST) =====
  const hasWeights =
    hybridMetrics.RuleWeight != null &&
    hybridMetrics.MLWeight != null;

  const scoreCompositionData = hasWeights
    ? [
      { name: "Structure Rules", value: hybridMetrics.RuleWeight * 100 },
      { name: "AI Quality", value: hybridMetrics.MLWeight * 100 },
    ]
    : [];
  console.log("PIPELINE REPORT HYBRID:", pipelineReport?.hybrid);

  return (
    <div className="min-h-screen bg-slate-50 mt-2 flex flex-col">
      {uploading && (
        <div className="fixed inset-0 z-[10000] bg-black/60 flex items-center justify-center">
          <div className="bg-white px-6 py-4 rounded-xl shadow-xl text-center space-y-3">
            <div className="animate-spin rounded-full h-10 w-10 border-4 border-blue-600 border-t-transparent mx-auto" />
            <div className="font-semibold text-slate-800">
              Processing learning plan… please wait
            </div>
          </div>
        </div>
      )}
      {/* ================= HEADER ================= */}
      <header >
        <div className="max-w-7xl mx-auto  px-6 py-1 flex justify-between items-center">
          <div>
            <h1 className="text-xl font-semibold text-slate-900">
              Learning Platform Manager
            </h1>
            <p className="text-sm text-slate-500">
              Plans → Weeks → Modules → Lessons
            </p>
          </div>

          <div className="flex gap-3">
            <button
              onClick={() => setShowUploadModal(true)}
              className="flex items-center gap-2 px-3 py-1 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              <DocumentArrowUpIcon className="w-5 h-5" />
              Upload Plan
            </button>

            <button
              onClick={() => {
                setSelectedPlan(null);
                setShowPlanModal(true);
              }}
              className="flex items-center gap-2 px-3 py-1 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              <PlusIcon className="w-5 h-5" />
              Create Plan
            </button>
            {/* ================= AI EVALUATION REPORT ================= */}
            {pipelineReport && (
              <div className="fixed inset-0 z-[9999] bg-black/60 flex items-center justify-center">
                <div className="bg-white max-w-6xl w-full rounded-2xl p-8 mt-1 overflow-y-auto max-h-[90vh] shadow-2xl">

                  {/* ================= HEADER ================= */}
                  <div className="flex justify-between items-center mb-3 border-b pb-2">
                    <div className="flex items-center gap-3">
                      <ScaleIcon className="w-7 h-7 text-indigo-600" />
                      <div>
                        <h2 className="text-xl font-bold text-slate-900">
                          Training Plan {pipelineReport.phase}
                        </h2>
                        <p className="text-xs text-slate-500">
                          {pipelineReport.phase === "Enrichment"
                            ? "AI-generated enhancements preview"
                            : "Automated quality & completeness check"}
                        </p>
                      </div>
                    </div>

                    <button onClick={() => setPipelineReport(null)}>
                      <XMarkIcon className="w-6 h-6 text-slate-500 hover:text-slate-800" />
                    </button>
                  </div>
                  {/* ================= RULE-BASED VALIDATION ================= */}
                  <section className="mb-2">
                    <SectionTitle
                      icon={ScaleIcon}
                      title="Content & Structure Review"
                      subtitle="Checks whether all required sections are present and clearly defined"
                    />

                    {/* ================= REVIEW SUMMARY STRIP ================= */}
                    <div className="mt-1 rounded-lg border border-slate-200 bg-slate-50 px-8 py-3 flex items-center justify-between">
                      <div>
                        <p className="text-sm text-slate-600">Overall Structure Quality</p>
                        <p className="text-md font-semibold text-slate-900">
                          Score: {pipelineReport.ruleBased?.score} / 100
                        </p>
                      </div>

                      <StatusBadge
                        ok={pipelineReport.ruleBased?.isValid}
                        text={
                          pipelineReport.ruleBased?.isValid
                            ? "Structure is acceptable"
                            : "Structure needs attention"
                        }
                      />
                    </div>

                    {/* ================= ISSUES OVERVIEW ================= */}
                    <div className="mt-2 grid grid-cols-2 gap-4">
                      <div className="rounded-lg border border-red-100 bg-red-50 px-5 py-2 mb-2">
                        <p className="text-sm font-semibold text-red-700 mb-2">
                          Critical Issues
                        </p>
                        <p className="text-xs text-red-600">
                          Must be fixed before the plan can be finalized
                        </p>
                        <p className="mt-2 text-2xl font-bold text-red-700">
                          {pipelineReport.ruleBased?.errors?.length ?? 0}
                        </p>
                      </div>

                      <div className="rounded-lg border border-amber-100 bg-amber-50 px-5 py-4">
                        <p className="text-sm font-semibold text-amber-700 mb-1">
                          Recommendations
                        </p>
                        <p className="text-xs text-amber-600">
                          Optional improvements for better quality
                        </p>
                        <p className="mt-2 text-2xl font-bold text-amber-700">
                          {pipelineReport.ruleBased?.warnings?.length ?? 0}
                        </p>
                      </div>
                    </div>

                    {/* ================= DETAILED ISSUES ================= */}
                    {pipelineReport.ruleBased?.errors?.length > 0 && (
                      <div className="mt-8">
                        <h4 className="font-semibold text-slate-900 mb-2">
                          Critical Issues to Fix
                        </h4>
                        <ul className="space-y-2">
                          {pipelineReport.ruleBased.errors.map((e, i) => (
                            <li
                              key={i}
                              className="flex items-start gap-3 rounded-md border border-red-100 bg-white px-4 py-2 text-sm text-slate-700"
                            >
                              <span className="mt-1 h-2 w-2 rounded-full bg-red-500" />
                              {e}
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}

                    {pipelineReport.ruleBased?.warnings?.length > 0 && (
                      <div className="mt-2">
                        <h4 className="font-semibold text-slate-900 mb-1">
                          Suggested Improvements
                        </h4>
                        <ul className="space-y-1">
                          {pipelineReport.ruleBased.warnings.map((w, i) => (
                            <li
                              key={i}
                              className="flex items-start gap-3 rounded-md border border-amber-100 bg-white px-4 py-2 text-sm text-slate-700"
                            >
                              <span className="mt-1 h-2 w-2 rounded-full bg-amber-500" />
                              {w}
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}

                    {/* ================= MISSING SECTIONS ================= */}
                    {pipelineReport.ruleBased?.missingFields?.length > 0 && (
                      <div className="mt-4">
                        <h4 className="font-semibold text-slate-900 mb-2">
                          Missing or Incomplete Sections
                        </h4>

                        <p className="text-sm text-slate-600 mb-2">
                          These sections were expected but not found or were incomplete.
                          They can be automatically filled during AI enhancement.
                        </p>

                        <div className="overflow-hidden rounded-lg border border-slate-200">
                          <table className="w-full text-sm">
                            <thead className="bg-slate-100 text-slate-700">
                              <tr>
                                <th className="px-4 py-2 text-left">Level</th>
                                <th className="px-4 py-2 text-left">Location</th>
                                <th className="px-4 py-2 text-left">Missing</th>
                                <th className="px-4 py-2 text-left">Priority</th>
                              </tr>
                            </thead>
                            <tbody>
                              {pipelineReport.ruleBased.missingFields.map((f, i) => (
                                <tr
                                  key={i}
                                  className="border-t last:border-b bg-white"
                                >
                                  <td className="px-4 py-2">{f.level}</td>
                                  <td className="px-4 py-2 text-slate-600">{f.scope}</td>
                                  <td className="px-4 py-2 font-medium">{f.fieldName}</td>
                                  <td className="px-4 py-2">
                                    {f.isCritical ? (
                                      <span className="text-red-600 font-semibold">
                                        Required
                                      </span>
                                    ) : (
                                      <span className="text-amber-600">
                                        Recommended
                                      </span>
                                    )}
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}
                  </section>

                  {/* ================= ML QUALITY ASSESSMENT ================= */}
                  {pipelineReport.ml && (
                    <section className="mb-6 border-t pt-3">
                      <h3 className="text-lg font-semibold text-slate-900 mb-1">
                        AI Learning Quality Analysis
                      </h3>
                      <p className="text-sm text-slate-600 mb-6">
                        Visual summary of AI confidence and detected learning quality signals.
                      </p>

                      <div className="grid grid-cols-2 gap-8">

                        {/* ===== CONFIDENCE DONUT ===== */}
                        <div className="bg-white border rounded-xl p-6">
                          <h4 className="font-medium text-slate-800 mb-1">
                            Overall AI Confidence
                          </h4>
                          <p className="text-xs text-slate-500 mb-2">
                            How confident the AI is that this learning plan meets quality standards.
                          </p>

                          <ResponsiveContainer width="100%" height={220}>
                            <PieChart>
                              <Pie
                                data={[
                                  { name: "Confidence", value: mlConfidence },
                                  { name: "Remaining", value: 100 - mlConfidence },
                                ]}
                                dataKey="value"
                                innerRadius={60}
                                outerRadius={90}
                                startAngle={90}
                                endAngle={-270}
                              >
                                <Cell fill={pipelineReport.ml.isValid ? "#22c55e" : "#f59e0b"} />
                                <Cell fill="#e5e7eb" />
                              </Pie>
                              <Tooltip formatter={(v) => `${v}%`} />
                            </PieChart>
                          </ResponsiveContainer>

                          <p className="text-center mt-2 font-semibold text-slate-800">
                            {mlConfidence}% Confidence
                          </p>

                          <p className="text-xs text-center text-slate-500 mt-1">
                            {pipelineReport.ml.isValid
                              ? "AI indicates strong learning quality"
                              : "AI detected quality concerns"}
                          </p>
                        </div>

                        {/* ===== QUALITY SIGNALS ===== */}
                        <div className="bg-white border rounded-xl p-6">
                          <h4 className="font-medium text-slate-800 mb-1">
                            Quality Signals Detected
                          </h4>
                          <p className="text-xs text-slate-500 mb-4">
                            Higher bars indicate stronger concern detected by the AI.
                          </p>

                          <ResponsiveContainer width="100%" height={220}>
                            <BarChart
                              data={qualitySignals.map(s => ({
                                name: s.label,
                                value: s.value,
                              }))}
                            >
                              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                              <YAxis allowDecimals={false} />
                              <Tooltip />
                              <Bar dataKey="value" fill="#6366f1" radius={[6, 6, 0, 0]} />
                            </BarChart>
                          </ResponsiveContainer>

                          {!hasQualitySignals && (
                            <div className="mt-3 text-xs text-emerald-700 bg-emerald-50 rounded-md p-2 text-center">
                              No AI quality issues were detected.
                            </div>
                          )}
                        </div>
                      </div>
                    </section>
                  )}
                  {/* ================= FINAL DECISION ================= */}
                  <section className="mb-6 border-t pt-3">
                    <h3 className="text-lg font-semibold text-slate-900 mb-4">
                      Final Evaluation Decision
                    </h3>

                    <div className="grid grid-cols-2 gap-8">

                      {/* ===== DECISION BASIS ===== */}
                      <div className="bg-white border rounded-xl p-6">
                        <h4 className="font-medium text-slate-800 mb-2">
                          How the Decision Was Made
                        </h4>

                        {hasWeights ? (
                          <ResponsiveContainer width="100%" height={240}>
                            <PieChart>
                              <Pie
                                data={scoreCompositionData}
                                dataKey="value"
                                innerRadius={60}
                                outerRadius={90}
                                label={({ name, value }) => `${name}: ${value}%`}
                              >
                                <Cell fill="#3b82f6" />
                                <Cell fill="#8b5cf6" />
                              </Pie>
                              <Tooltip formatter={(v) => `${v}%`} />
                            </PieChart>
                          </ResponsiveContainer>
                        ) : (
                          <div className="flex flex-col items-center justify-center h-[220px] text-center">
                            <div className="text-sm font-medium text-slate-700 mb-2">
                              Structure-Based Decision
                            </div>
                            <div className="text-xs text-slate-500 max-w-xs">
                              AI quality analysis was completed, but required structural sections
                              are missing. Structure rules take priority over AI scoring.
                            </div>
                          </div>
                        )}
                      </div>

                      {/* ===== FINAL RESULT ===== */}
                      <div
                        className={`rounded-xl p-6 border ${pipelineReport?.hybrid?.IsValid
                          ? "bg-emerald-50 border-emerald-200"
                          : "bg-amber-50 border-amber-200"
                          }`}
                      >
                        <StatusBadge
                          ok={pipelineReport?.hybrid?.IsValid}
                          text={
                            pipelineReport?.hybrid?.IsValid
                              ? "Approved for Enhancement & Saving"
                              : "Improvements Recommended"
                          }
                        />

                        <p className="text-sm text-slate-700 mt-4">
                          {pipelineReport?.hybrid?.IsValid
                            ? "This learning plan meets quality and structure standards."
                            : "The plan has strong learning quality, but some required sections are missing. AI enhancement can fix these automatically."}
                        </p>

                        <div className="mt-4 flex items-center gap-2 text-sm">
                          <span className="text-slate-500">Final Score:</span>
                          <span className="font-semibold text-slate-900">
                            {pipelineReport?.hybrid?.FinalScore}
                          </span>
                          <span className="text-xs text-slate-500">(out of 100)</span>
                        </div>
                      </div>

                    </div>
                  </section>
                  {/* ================= AI ENRICHMENT SUMMARY ================= */}
                  {pipelineReport.phase === "Enrichment" && enrichedJson && (
                    <section className="mb-10 border-t pt-8">
                      <SectionTitle
                        icon={CpuChipIcon}
                        title="AI Enrichment Summary"
                        badge="Generated"
                      />

                      <div className="space-y-4">
                        {enrichedFields.length === 0 && (
                          <div className="text-sm text-slate-500 italic">
                            No missing fields required enrichment.
                          </div>
                        )}
                        {enrichedFields.map((f, i) => (
                          <div key={i} className="rounded-lg border border-indigo-200 bg-indigo-50 p-4">
                            <div className="flex justify-between items-center mb-2">
                              <div className="font-semibold text-indigo-900">
                                {f.fieldName}
                              </div>

                              <span className="text-xs px-2 py-0.5 rounded-full bg-amber-100 text-amber-700">
                                Generated
                              </span>
                            </div>

                            <div className="bg-white border rounded-md p-3 text-sm whitespace-pre-wrap">
                              {Array.isArray(f.generatedValue)
                                ? f.generatedValue.map((v, idx) => <div key={idx}>• {v}</div>)
                                : f.generatedValue}
                            </div>
                          </div>
                        ))}

                      </div>

                      {/* ===== OPTIONAL RAW JSON (COLLAPSIBLE) ===== */}
                      <details className="mt-6">
                        <summary className="text-xs cursor-pointer text-slate-500">
                          View raw enriched JSON
                        </summary>
                        <pre className="mt-3 bg-slate-900 text-green-300 rounded-xl p-4 text-xs overflow-auto max-h-[300px]">
                          {JSON.stringify(JSON.parse(enrichedJson), null, 2)}
                        </pre>
                      </details>
                    </section>
                  )}
                  {/* ================= ACTIONS ================= */}
                  <div className="flex justify-end gap-4 mt-10">
                    {pipelineReport.phase === "Evaluation" && (
                      <button
                        onClick={enrichLearningPlan}
                        disabled={enriching}
                        className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:opacity-60"
                      >
                        {enriching ? "Enhancing…" : "Enhance with AI"}
                      </button>
                    )}

                    {pipelineReport.phase === "Enrichment" && (
                      <button
                        onClick={savePlan}
                        className="px-6 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700"
                      >
                        Confirm & Save
                      </button>
                    )}
                  </div>

                </div>
              </div>
            )}
          </div>
        </div>
      </header>
      <section>
        <div className="space-y-2 mt-3">
          {sortedPlans.map((plan) => (
            <div
              key={plan.planId}
              onClick={() => managePlan(plan)}
              className={`border rounded-xl p-3 bg-white transition cursor-pointer hover:shadow-lg
          ${selectedPlan?.planId === plan.planId
                  ? "ring-2 ring-blue-500 bg-blue-50"
                  : "hover:border-blue-300"}
        `}
            >
              <div className="flex justify-between items-start gap-6">

                {/* LEFT */}
                <div className="flex-1">
                  <h3 className="text-lg font-semibold text-slate-900">
                    {plan.title}
                  </h3>

                  <p className="text-sm text-slate-600 mt-2 line-clamp-3">
                    {plan.overview || plan.description || "No overview provided"}
                  </p>

                  <div className="mt-4 flex gap-4 text-xs text-slate-500">
                    <span>📅 <strong>{plan.durationWeeks}</strong> weeks</span>
                    <span>⏱️ <strong>{plan.totalDays}</strong> days</span>
                  </div>
                </div>

                {/* ACTIONS */}
                <div className="flex gap-3">
                  <button
                    title="View Plan"
                    onClick={(e) => {
                      e.stopPropagation();
                      managePlan(plan);
                    }}
                    className="p-2 rounded-lg hover:bg-slate-100"
                  >
                    <FolderOpenIcon className="w-5 h-5 text-slate-700" />
                  </button>

                  <button
                    title="Edit Plan"
                    onClick={(e) => {
                      e.stopPropagation();
                      setSelectedPlan(plan);
                      setShowPlanModal(true);
                    }}
                    className="p-2 rounded-lg hover:bg-blue-100"
                  >
                    <PencilIcon className="w-5 h-5 text-blue-600" />
                  </button>

                  <button
                    title="Delete Plan"
                    onClick={(e) => {
                      e.stopPropagation();
                      deletePlan(plan.planId);
                    }}
                    className="p-2 rounded-lg hover:bg-red-100"
                  >
                    <TrashIcon className="w-5 h-5 text-red-600" />
                  </button>
                </div>

              </div>
            </div>
          ))}
        </div>
      </section>

      {selectedPlan && (
        <section ref={weeksSectionRef} className="mt-4">
          <div className="flex justify-between items-center mb-2">
            <div>
              <h2 className="text-xl font-bold text-slate-800">
                Weeks · {selectedPlan.title}
              </h2>
              <p className="text-sm text-slate-500">
                Manage weekly structure and modules
              </p>
            </div>

            <button
              onClick={() => {
                setSelectedWeek(null);
                setShowWeekModal(true);
              }}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg"
            >
              + Add Week
            </button>
          </div>

          <div className="space-y-2 pl-4 border-l-4 border-blue-100">
            {weeks.map((week) => (
              <div
                key={week.weekId}
                className="border rounded-xl p-3 bg-white hover:shadow-md"
              >
                <div className="flex justify-between mb-0 items-start gap-3">
                  {/* WEEK INFO */}
                  <div className="flex-1">
                    <h4 className="text-lg font-semibold text-slate-900">
                      Week {week.weekNumber}: {week.title}
                    </h4>

                    {week.overview && (
                      <p className="text-sm text-slate-600 mt-2">
                        {week.overview}
                      </p>
                    )}

                    <div className="mt-4 text-xs text-slate-500">
                      📦 Modules: {week.modules?.length ?? 0}
                    </div>
                  </div>

                  {/* ACTIONS */}
                  <div className="flex gap-3">
                    <button
                      onClick={() => manageWeek(week)}
                      className="p-2 hover:bg-slate-100 rounded-lg"
                    >
                      <FolderOpenIcon className="w-5 h-5" />
                    </button>

                    <button
                      onClick={() => {
                        setSelectedWeek(week);
                        setShowWeekModal(true);
                      }}
                      className="p-2 hover:bg-purple-100 rounded-lg"
                    >
                      <PencilIcon className="w-5 h-5 text-purple-700" />
                    </button>

                    <button
                      onClick={() => deleteWeek(week.weekId)}
                      className="p-2 hover:bg-red-100 rounded-lg"
                    >
                      <TrashIcon className="w-5 h-5 text-red-600" />
                    </button>
                  </div>

                </div>
              </div>
            ))}
          </div>
        </section>
      )}
      {selectedWeek && (
        <section ref={modulesSectionRef} className="mt-4">
          {/* HEADER */}
          <div className="flex justify-between items-center mb-1">
            <div>
              <h2 className="text-xl font-bold text-slate-800">
                Modules · {selectedWeek.title}
              </h2>
              <p className="text-sm text-slate-500 mt-1">
                Modules for this week
              </p>
            </div>

            <button
              onClick={() => {
                setSelectedModule(null);
                setShowModuleModal(true);
              }}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              + Add Module
            </button>
          </div>

          {/* MODULE LIST */}
          <div className="space-y-2">
            {modules.map((mod) => (
              <div
                key={mod.moduleId}
                className={`border rounded-xl p-3 bg-white flex justify-between items-start
            ${selectedModule?.moduleId === mod.moduleId
                    ? "ring-2 ring-emerald-500 bg-emerald-50"
                    : "hover:shadow-md"}
          `}
              >
                <div>
                  <h4 className="font-semibold text-slate-900">
                    {mod.moduleName}
                  </h4>

                  {mod.description && (
                    <p className="text-sm text-slate-600 mt-1 max-w-2xl">
                      {mod.description}
                    </p>
                  )}
                </div>

                <div className="flex gap-3">
                  {/* VIEW */}
                  <button
                    title="View Module"
                    onClick={() => openModuleDetail(mod)}
                    className="p-2 rounded-lg hover:bg-slate-100"
                  >
                    <FolderOpenIcon className="w-5 h-5 text-slate-700" />
                  </button>

                  {/* EDIT */}
                  <button
                    title="Edit Module"
                    onClick={() => {
                      setSelectedModule(mod);
                      setShowModuleModal(true);
                    }}
                    className="p-2 rounded-lg hover:bg-emerald-100"
                  >
                    <PencilIcon className="w-5 h-5 text-emerald-700" />
                  </button>

                  {/* DELETE */}
                  <button
                    title="Delete Module"
                    onClick={() => deleteModule(mod.moduleId)}
                    className="p-2 rounded-lg hover:bg-red-100"
                  >
                    <TrashIcon className="w-5 h-5 text-red-600" />
                  </button>
                </div>
              </div>
            ))}

            {modules.length === 0 && (
              <div className="text-sm text-slate-500 italic">
                No modules found for this week.
              </div>
            )}
          </div>
        </section>
      )}
      {openedModuleDetail && (
        <section
          ref={moduleDetailRef}
          className="mt-16 bg-white border rounded-2xl shadow-xl p-8 relative"
        >
          {/* CLOSE */}
          <button
            onClick={() => setOpenedModuleDetail(null)}
            className="absolute top-4 right-4"
          >
            <XMarkIcon className="w-6 h-6 text-slate-600 hover:text-slate-900" />
          </button>

          {/* HEADER */}
          <h2 className="text-2xl font-bold text-slate-900">
            {openedModuleDetail.moduleName}
          </h2>

          {openedModuleDetail.description && (
            <p className="text-slate-600 mt-3 max-w-4xl">
              {openedModuleDetail.description}
            </p>
          )}

          {/* LESSONS */}
          {openedModuleDetail.lessons?.length > 0 && (
            <section className="mt-10">
              <h3 className="text-lg font-semibold text-slate-800 mb-4">
                Lessons
              </h3>

              <div className="space-y-4">
                {openedModuleDetail.lessons.map((lesson) => (
                  <div
                    key={lesson.lessonId}
                    className="border rounded-xl p-5 bg-slate-50"
                  >
                    <div className="font-semibold text-slate-900">
                      {lesson.title}
                    </div>
                    {lesson.content && (
                      <div className="text-sm text-slate-600 mt-2">
                        {lesson.content}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </section>
          )}

          {/* RESOURCES */}
          {openedModuleDetail.resources?.length > 0 && (
            <section className="mt-10">
              <h3 className="text-lg font-semibold text-slate-800 mb-3">
                Resources
              </h3>

              <ul className="space-y-2">
                {openedModuleDetail.resources.map((res) => (
                  <li
                    key={res.resourceId}
                    className="border rounded-lg p-3 bg-slate-50"
                  >
                    <a
                      href={res.url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-blue-600 hover:underline font-medium"
                    >
                      {res.title}
                    </a>
                    <div className="text-xs text-slate-500">
                      {res.resourceType}
                    </div>
                  </li>
                ))}
              </ul>
            </section>
          )}

          {/* ASSESSMENTS */}
          {openedModuleDetail.assessments?.length > 0 && (
            <section className="mt-10">
              <h3 className="text-lg font-semibold text-slate-800 mb-3">
                Assessments
              </h3>

              <div className="space-y-3">
                {openedModuleDetail.assessments.map((a) => (
                  <div
                    key={a.assessmentId}
                    className="border rounded-lg p-4 bg-indigo-50"
                  >
                    <div className="font-semibold text-indigo-900">
                      {a.title}
                    </div>
                    <div className="text-xs text-indigo-700">
                      Difficulty: {a.difficulty}
                    </div>
                  </div>
                ))}
              </div>
            </section>
          )}

          {/* EMPTY STATE */}
          {!openedModuleDetail.lessons?.length &&
            !openedModuleDetail.resources?.length &&
            !openedModuleDetail.assessments?.length && (
              <div className="mt-10 text-sm text-slate-500 italic">
                No module details available.
              </div>
            )}
        </section>
      )}
      {/* ================= MODALS (UNCHANGED) ================= */}
      {showUploadModal && (
        <div className="fixed inset-0 z-50 bg-black/40 flex items-center justify-center">
          <div className="bg-white rounded-2xl w-full max-w-lg shadow-xl overflow-hidden">

            {/* HEADER */}
            <div className="flex justify-between items-center px-6 py-4 border-b">
              <h2 className="text-lg font-semibold text-gray-900">
                Upload Learning Plan
              </h2>
              <button onClick={() => setShowUploadModal(false)}>
                <XMarkIcon className="w-5 h-5 text-gray-500 hover:text-gray-700" />
              </button>
            </div>

            {/* BODY */}
            <div className="px-6 py-4 space-y-4">

              <p className="text-sm text-gray-600">
                Upload a <strong>.docx</strong> document.
                Our AI will read it, structure it, and help you refine it later in the UI.
              </p>

              {/* GUIDELINES (COMPACT + FRIENDLY) */}
              <div className="rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm space-y-3">

                {/* TITLE */}
                <div className="flex items-center gap-2 text-slate-800 font-medium">
                  <InformationCircleIcon className="w-5 h-5 text-slate-500" />
                  <span>Upload guidance</span>
                </div>

                {/* GUIDELINES */}
                <ul className="space-y-2 text-slate-600">
                  <li className="flex gap-2">
                    <CheckCircleIcon className="w-4 h-4 text-slate-400 mt-0.5" />
                    <span>
                      Clear headings such as <strong>Week 1</strong>, <strong>Module</strong>, and <strong>Lesson</strong> help improve AI accuracy.
                    </span>
                  </li>

                  <li className="flex gap-2">
                    <CheckCircleIcon className="w-4 h-4 text-slate-400 mt-0.5" />
                    <span>
                      A simple hierarchy (plan → weeks → modules → lessons) works best.
                    </span>
                  </li>

                  <li className="flex gap-2">
                    <CheckCircleIcon className="w-4 h-4 text-slate-400 mt-0.5" />
                    <span>
                      Images or scanned content are ignored.
                    </span>
                  </li>
                </ul>

                {/* STRUCTURE SUMMARY */}
                <div className="rounded-md border border-slate-200 bg-white px-3 py-2 text-xs text-slate-600">
                  <span className="font-medium text-slate-700">Recommended structure:</span>{" "}
                  Learning Plan → Weeks → Modules → Lessons / Resources / Assessments
                </div>

                {/* REASSURANCE */}
                <p className="text-xs text-slate-500">
                  You can review and edit all generated content after upload.
                </p>
              </div>
              {/* FILE INPUT */}
              <input
                type="file"
                accept=".docx"
                onChange={(e) => setUploadFile(e.target.files[0])}
                className="w-full border rounded-md p-2 text-sm"
              />
            </div>

            {/* FOOTER */}
            <div className="px-6 py-4 border-t">
              <button
                onClick={uploadLearningPlan}
                disabled={uploading}
                className="w-full bg-blue-600 hover:bg-blue-700 disabled:opacity-60 text-white py-2 rounded-md"
              >
                {uploading ? "Generating Learning Plan..." : "Upload & Generate"}
              </button>
            </div>
          </div>
        </div>
      )}
      {showPlanModal && (
        <PlanModal
          plan={selectedPlan}
          onClose={() => { setShowPlanModal(false); loadPlans(); }}
        />
      )}

      {showWeekModal && (
        <WeekModal
          week={selectedWeek}
          planId={selectedPlan?.planId}
          onClose={() => { setShowWeekModal(false); managePlan(selectedPlan); }}
        />
      )}

      {showModuleModal && (
        <ModuleModal
          moduleData={selectedModule}
          weekId={selectedWeek?.weekId}
          onClose={() => { setShowModuleModal(false); manageWeek(selectedWeek); }}
        />
      )}
    </div>
  );
}
