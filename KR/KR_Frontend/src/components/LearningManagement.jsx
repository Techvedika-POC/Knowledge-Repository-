import React, { useEffect, useState, useRef } from "react";
import api from "../api";
import toast from "react-hot-toast";
import { motion } from "framer-motion";
import {
  Lock,
  CheckCircle2,
  EyeIcon,
  ArrowLeft,
  Paperclip,
  ClipboardCheck,
  PlayCircle,
  RotateCcw,
  X,
  BookOpen,
  FileText,
  AlertTriangle,
  Info,
  Target,
  Wrench,
  ListChecks,
  Layers,
  RefreshCcw,
  ChevronDown,
  TrendingUp, Flame, Clock
} from "lucide-react";
export default function LearningManagement() {
  const [plans, setPlans] = useState([]);
  const [selectedPlan, setSelectedPlan] = useState(null);
  const [planDetails, setPlanDetails] = useState(null);
  const [planDetailsVisible, setPlanDetailsVisible] = useState(false);
  const [weeks, setWeeks] = useState([]);
  const [selectedWeek, setSelectedWeek] = useState(null);
  const [weekDetails, setWeekDetails] = useState(null);
  const [weekDetailsVisible, setWeekDetailsVisible] = useState(false);
  const [modules, setModules] = useState([]);
  const [selectedModule, setSelectedModule] = useState(null);
  const [moduleDetail, setModuleDetail] = useState(null);
  const [activeLesson, setActiveLesson] = useState(null);
  const [activeAssessment, setActiveAssessment] = useState(null);
  const [lessonModalOpen, setLessonModalOpen] = useState(false);
  const [assessmentModalOpen, setAssessmentModalOpen] = useState(false);
  const [answers, setAnswers] = useState({});
  const [testTaken, setTestTaken] = useState(false);
  const [testScore, setTestScore] = useState(null);
  const [testPassed, setTestPassed] = useState(null);
  const [loading, setLoading] = useState(false);
  const [userId] = useState(() => localStorage.getItem("userId") || null);
  const [userProgressModules, setUserProgressModules] = useState([]);
  const [fullScreenResource, setFullScreenResource] = useState(null);
  const [planProgress, setPlanProgress] = useState(null);
  const weeksPanelRef = useRef(null);
  const modulesPanelRef = useRef(null);
  const safeProgressModules = planProgress?.modules || [];


  useEffect(() => {
    loadPlans();
    const onResize = () => equalizeHeights();
    window.addEventListener("resize", onResize);
    return () => window.removeEventListener("resize", onResize);
  }, []);

  useEffect(() => {
    setTimeout(equalizeHeights, 120);
  }, [weeks, modules, moduleDetail, weekDetails, planDetails]);
  const normalizePlanProgress = (data) => {
    if (!data) return null;

    return {
      userId: data.userId,
      planId: data.planId,
      modules: (data.modules || []).map(m => ({
        moduleId: m.moduleId,
        isUnlocked: m.isUnlocked,
        isCompleted: m.isCompleted,
        lessonProgressPercent: m.lessonProgressPercent ?? 0,

        lessons: (m.lessons || []).map(l => ({
          lessonId: l.lessonId,
          isCompleted: l.isCompleted
        })),

        assessments: (m.assessments || []).map(a => ({
          assessmentId: a.assessmentId,
          latestResult: a.latestResult
        }))
      }))
    };
  };
  const refreshProgress = async (planIdOverride) => {
    const planId = planIdOverride || selectedPlan?.planId;
    if (!planId || !userId) return null;

    const res = await fetch(
      `/api/UserProgressAggregate/${userId}/plan/${planId}`
    );

    if (!res.ok) {
      console.error("Progress API failed:", res.status);
      setPlanProgress(null);
      return null;
    }

    const text = await res.text();

    if (!text || !text.trim()) {
      // 👇 No progress yet (new user / new plan)
      const empty = {
        userId,
        planId,
        modules: []
      };
      setPlanProgress(empty);
      return empty;
    }

    let data;
    try {
      data = JSON.parse(text);
    } catch (e) {
      console.error("Invalid JSON from progress API:", text);
      return null;
    }

    const normalized = normalizePlanProgress(data);
    setPlanProgress(normalized);
    return normalized;
  };
  const equalizeHeights = () => {
    try {
      const left = weeksPanelRef.current;
      const right = modulesPanelRef.current;
      if (!left || !right) return;
      left.style.minHeight = "";
      right.style.minHeight = "";
      const lh = left.getBoundingClientRect().height;
      const rh = right.getBoundingClientRect().height;
      const mh = Math.max(lh, rh, 360);
      left.style.minHeight = `${mh}px`;
      right.style.minHeight = `${mh}px`;
    } catch (e) {
      // ignore
    }
  };
  const loadPlans = async () => {
    try {
      setLoading(true);
      const res = await api.get("/LearningPlan");
      setPlans(res.data || []);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load plans");
    } finally {
      setLoading(false);
      setTimeout(equalizeHeights, 120);
    }
  };
  const fetchPlanDetails = async (plan) => {
    if (!plan?.planId) return;
    try {
      setLoading(true);
      const res = await api.get(`/LearningPlan/${plan.planId}/hierarchy`, {
        params: { userId },
      });
      setPlanDetails(res.data || null);
    } catch (err) {
      console.error("Failed fetching plan details", err);
      toast.error("Failed to load plan details");
    } finally {
      setLoading(false);
    }
  };
  const selectPlan = async (p) => {
    setSelectedPlan(p);
    setSelectedWeek(null);
    setSelectedModule(null);
    setModuleDetail(null);

    try {
      setLoading(true);

      await refreshProgress(p.planId);   

      const weeksRes = await api.get(`/Week/full/plan/${p.planId}`, {
        params: { userId }
      });

      setWeeks(weeksRes.data || []);
      await fetchPlanDetails(p);

    } finally {
      setLoading(false);
    }
  };
  const fetchWeekDetails = async (week) => {
    if (!week?.weekId) return;
    try {
      setLoading(true);
      const res = await api.get(`/Week/full/${week.weekId}`, {
        params: { userId },
      });
      setWeekDetails(res.data || null);
    } catch (err) {
      console.error("Failed fetching week details", err);
      toast.error("Failed to load week details");
    } finally {
      setLoading(false);
    }
  };
  const selectWeek = async (w) => {
    if (!w.isUnlocked) {
      toast.error("Week locked");
      return;
    }

    setSelectedWeek(w);
    setSelectedModule(null);
    setModuleDetail(null);

    try {
      setLoading(true);

      const res = await api.get(`/Module/week/${w.weekId}`, {
        params: { userId }
      });

      const merged = (res.data || []).map(m => {
        const p = planProgress?.modules?.find(x => x.moduleId === m.moduleId);
        return {
          ...m,
          isCompleted: p?.isCompleted ?? m.isCompleted,
          lessonProgressPercent: p?.lessonProgressPercent ?? m.lessonProgressPercent
        };
      });

      setModules(merged);


      setModules(res.data || []);

    } catch (err) {
      console.error(err);
      toast.error("Failed to load modules");
    } finally {
      setLoading(false);
    }
  };

  function attachResourcesAndAssessments(detail) {
    if (!detail || !Array.isArray(detail.lessons)) return detail;
    const lessonsMap = new Map();
    detail.lessons.forEach((lesson) => {
      lessonsMap.set(String(lesson.lessonId).toLowerCase(), {
        ...lesson,
        resources: [],
        assessments: [],
      });
    });
    (detail.resources || []).forEach((r) => {
      const key = String(r.topicId || r.TopicId || r.topicID || "").toLowerCase();
      const target = lessonsMap.get(key);
      if (target) target.resources.push(r);
    });
    (detail.assessments || []).forEach((a) => {
      const key = String(a.topicId || a.TopicId || a.TopicID || "").toLowerCase();
      const target = lessonsMap.get(key);
      if (target) target.assessments.push(a);
    });
    const resultLessons = (detail.lessons || []).map((ls) =>
      lessonsMap.get(String(ls.lessonId).toLowerCase())
    );
    return { ...detail, lessons: resultLessons };
  }
  const openModule = async (m) => {
    if (!m.isUnlocked) {
      toast.error("This module is locked.");
      return;
    }

    setSelectedModule(m);
    setLoading(true);

    try {
      // content
      const res = await api.get(`/Module/${m.moduleId}`, {
        params: { userId }
      });

      // progress
      const progress = planProgress?.modules
        ?.find(x => x.moduleId === m.moduleId);

      const content = attachResourcesAndAssessments(res.data || {});

      // 🔑 MERGE CONTENT + PROGRESS
      const merged = {
        ...content,
        isCompleted: progress?.isCompleted ?? false,
        lessonProgressPercent: progress?.lessonProgressPercent ?? 0,
        lessons: content.lessons.map(l => ({
          ...l,
          isCompleted:
            progress?.lessons?.find(p => p.lessonId === l.lessonId)
              ?.isCompleted ?? false
        })),
        assessments: content.assessments.map(a => ({
          ...a,
          latestResult:
            progress?.assessments?.find(p => p.assessmentId === a.assessmentId)
              ?.latestResult ?? null
        }))
      };

      setModuleDetail(merged);

    } finally {
      setLoading(false);
    }
  };
  const refreshAndReopenModule = async () => {
    const updated = await refreshProgress();
    if (!updated || !selectedModule) return;

    const freshModule =
      updated.modules.find(m => m.moduleId === selectedModule.moduleId);

    if (freshModule) {
      await openModule(freshModule); 
    }
  };
  const startAssessment = (assessment) => {
    if (assessment.isLocked) {
      toast.error("Assessment locked.");
      return;
    }
    if (assessment.isCompleted) {
      toast.success("You already completed this assessment.");
      return;
    }
    setActiveAssessment(assessment);
    setAnswers({});
    setTestTaken(false);
    setTestScore(null);
    setTestPassed(null);
    setAssessmentModalOpen(true);
  };
  const currentUserId = localStorage.getItem("userId");
  const handleShowPlanDetails = async (plan) => {
    await fetchPlanDetails(plan);
    setPlanDetailsVisible(true);
  };
  const PlansBar = ({ plans = [], selected, onSelect, onShowDetails, loadPlans }) => {
    const sortedPlans = [...plans].reverse();

    return (
      <section className="rounded-2xl border border-slate-200 bg-slate-50 shadow-sm overflow-hidden">
        <div className="flex items-center justify-between px-5 py-2 bg-slate-100 border-b">
          <div>
            <h2 className="text-xl font-bold text-slate-900">Learning Plans</h2>
            <p className="text-xs text-slate-500">Select a plan to continue</p>
          </div>

          <button
            onClick={loadPlans}
            className="inline-flex items-center justify-center
             px-3 py-1.5
             text-slate-600
             transition"
            title="Refresh"
          >
            <RefreshCcw className="w-4 h-4" />
          </button>

        </div>

        <div className="p-4 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {sortedPlans.map((p) => (
            <motion.div
              key={p.planId}
              whileHover={{ y: -2 }}
              className={`rounded-xl border p-4 flex flex-col justify-between
              ${selected?.planId === p.planId ? "border-blue-500 bg-blue-50" : "bg-white"}`}
            >
              <div>
                <div className="text-xs text-gray-500">{p.category || "Program"}</div>
                <div className="font-semibold">{p.title}</div>
                <div className="text-xs text-gray-600 line-clamp-2">{p.description}</div>
              </div>

              <div className="mt-4 flex justify-between">
                <button
                  onClick={() => onSelect(p)}
                  className="inline-flex items-center gap-1.5 px-2 py-1 text-xs bg-blue-600 text-white rounded-md"
                >
                  <EyeIcon className="w-4 h-4" />
                  Open
                </button>

                <button
                  onClick={() => onShowDetails(p)}
                  className="inline-flex items-center gap-1.5 px-2 py-1 text-xs bg-slate-900 text-white rounded-md"
                >
                  <Info className="w-4 h-4" />
                  Details
                </button>

              </div>
            </motion.div>
          ))}
        </div>
      </section>
    );
  };

  const PlanDetailsPanel = ({ details, onClose }) => {
    const [activeTab, setActiveTab] = useState("overview");

    return (
      <div className="bg-white border border-slate-200 rounded-2xl shadow-md">
        {/* Header */}
        <div className="flex justify-between items-start px-6 py-4 border-b border-slate-100">
          <div>
            <h3 className="text-xl font-semibold text-slate-800">
              {details.title}
            </h3>
            <p className="text-sm text-slate-500 mt-1">
              {details.description}
            </p>
          </div>

          <button
            onClick={onClose}
            className="p-1.5 rounded-md text-slate-400 hover:text-slate-600 hover:bg-slate-100"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Tabs */}
        <div className="px-6 mt-2 border-b border-slate-100">
          <div className="flex mt-0 py-0 gap-6 text-sm font-medium text-slate-500">
            <TabButton active={activeTab === "overview"} onClick={() => setActiveTab("overview")} icon={<Info />} label="Overview" />
            <TabButton active={activeTab === "objectives"} onClick={() => setActiveTab("objectives")} icon={<Target />} label="Objectives" />
            <TabButton active={activeTab === "requirements"} onClick={() => setActiveTab("requirements")} icon={<Wrench />} label="Requirements" />
            <TabButton active={activeTab === "weeks"} onClick={() => setActiveTab("weeks")} icon={<Layers />} label="Structure" />
          </div>
        </div>

        {/* Content  */}
        <div
          className="px-6 py-5 space-y-6 max-h-[70vh] overflow-y-auto"
          style={{
            scrollbarWidth: "none",
            msOverflowStyle: "none",
          }}
        >
          <style>
            {`
            /* Chrome / Safari */
            div::-webkit-scrollbar {
              display: none;
            }
          `}
          </style>

          {/* OVERVIEW */}
          {activeTab === "overview" && (
            <Section
              icon={<Info className="text-blue-500" />}
              title="Overview"
              value={details.overview}
            />
          )}

          {/* OBJECTIVES */}
          {activeTab === "objectives" && (
            <Section
              icon={<Target className="text-emerald-500" />}
              title="Learning Objectives"
              value={details.objectives}
            />
          )}

          {/* REQUIREMENTS */}
          {activeTab === "requirements" && (
            <div className="space-y-6">
              <LeftAccent icon={<ListChecks />} color="border-blue-400 text-blue-600" title="Prerequisites" value={details.prerequisites} />
              <LeftAccent icon={<Wrench />} color="border-purple-400 text-purple-600" title="Technical Requirements" value={details.technicalRequirements} />
              <LeftAccent icon={<ListChecks />} color="border-emerald-400 text-emerald-600" title="Self-Assessment Checklist" value={details.selfAssessmentChecklist} />
            </div>
          )}

          {/* WEEKS */}
          {activeTab === "weeks" && (
            <div className="space-y-4">
              {details.weeks?.map((w) => (
                <details key={w.weekId} className="border-b border-slate-100 pb-3">
                  <summary className="flex justify-between items-center cursor-pointer list-none">
                    <div className="text-sm font-medium text-slate-800">
                      Week {w.weekNumber}: {w.title}
                    </div>
                    <ChevronDown className="w-4 h-4 text-slate-400" />
                  </summary>
                  <div className="mt-3 text-sm text-slate-600">
                    {w.overview}
                  </div>
                </details>
              ))}
            </div>
          )}
        </div>
      </div>
    );
  };
  const TabButton = ({ active, onClick, icon, label }) => (
    <button
      onClick={onClick}
      className={`flex items-center gap-1.5 pb-3 border-b-2 transition ${active
        ? "border-blue-500 text-blue-600"
        : "border-transparent hover:text-slate-600"
        }`}
    >
      {icon}
      {label}
    </button>
  );

  const Section = ({ icon, title, value }) => (
    <div className="flex gap-3">
      <div className="mt-1">{icon}</div>
      <div>
        <div className="text-xs font-medium text-slate-500 uppercase">{title}</div>
        <div className="text-sm text-slate-700 mt-1 whitespace-pre-wrap">
          {value || "—"}
        </div>
      </div>
    </div>
  );

  const LeftAccent = ({ icon, title, value, color }) => (
    <div className={`pl-4 border-l-2 ${color}`}>
      <div className="flex items-center gap-2 text-sm font-semibold">
        {icon}
        {title}
      </div>
      <div className="mt-1 text-sm text-slate-700 whitespace-pre-wrap">
        {value || "—"}
      </div>
    </div>
  );
  const WeeksPanel = ({ weeks, onSelect, selected }) => {
    return (
      <section
        ref={weeksPanelRef}
        className="bg-white rounded-2xl border border-slate-200 shadow-sm h-full flex flex-col"
      >
        {/* Header */}
        <div className="px-6 py-4 border-b border-slate-100">
          <h3 className="text-lg font-semibold text-slate-800">
            Learning Timeline
          </h3>
          <p className="text-xs text-slate-500">
            Progress through the course week by week
          </p>
        </div>

        {/* Timeline */}
        <div className="flex-1 overflow-y-auto scrollbar-hide px-6 py-4">
          <div className="relative space-y-6">
            {/* Vertical line */}
            <div className="absolute left-[18px] top-0 bottom-0 w-px bg-slate-200" />

            {weeks.map((w, index) => {
              const isSelected = selected?.weekId === w.weekId;

              return (
                <div
                  key={w.weekId}
                  className="relative flex items-start gap-4"
                >
                  {/* Timeline node */}
                  <div className="relative z-10 mt-1">
                    <div
                      className={`w-3.5 h-3.5 rounded-full border-2 ${w.isUnlocked
                        ? "bg-blue-500 border-blue-500"
                        : "bg-white border-slate-300"
                        }`}
                    />
                  </div>

                  {/* Content */}
                  <div
                    onClick={() => {
                      if (!w.isUnlocked) {
                        toast.error(
                          "This week is locked. Complete previous week to unlock."
                        );
                        return;
                      }
                      onSelect(w);
                    }}
                    className={`flex-1 rounded-xl border px-4 py-3 cursor-pointer transition
                    ${isSelected
                        ? "border-blue-400 bg-blue-50"
                        : "border-slate-200 hover:bg-slate-50"
                      }`}
                  >
                    <div className="flex justify-between gap-4">
                      {/* Main info */}
                      <div>
                        <div className="flex items-center gap-2">
                          <span className="text-xs font-semibold text-slate-500">
                            Week {w.weekNumber || index + 1}
                          </span>
                          {!w.isUnlocked && (
                            <Lock className="w-3.5 h-3.5 text-slate-400" />
                          )}
                        </div>

                        <div className="text-sm font-semibold text-slate-800 mt-0.5">
                          {w.title}
                        </div>
                      </div>

                      {/* Action */}
                      <button
                        onClick={async (e) => {
                          e.stopPropagation();
                          if (!w.isUnlocked) {
                            toast.error(
                              "This week is locked. Complete previous week to unlock."
                            );
                            return;
                          }

                          if (selected?.weekId !== w.weekId) {
                            await onSelect(w);
                            await fetchWeekDetails(w);
                            setWeekDetailsVisible(true);
                          } else {
                            setWeekDetailsVisible((v) => !v);
                          }
                        }}
                        className="self-center inline-flex items-center gap-1.5
                                 px-3 py-1.5 text-xs font-semibold
                                 rounded-md bg-slate-900 text-white
                                 hover:bg-slate-800 transition"
                      >
                        <EyeIcon className="w-4 h-4" />
                        View
                      </button>
                    </div>
                  </div>
                </div>
              );
            })}

            {!weeks.length && (
              <div className="text-sm text-slate-400 text-center py-6">
                No weeks available
              </div>
            )}
          </div>
        </div>
      </section>
    );
  };

  const ModulesGrid = ({ modules, onOpen }) => {
    const ProgressRing = ({ percent }) => (
      <div className="relative w-12 h-12">
        <svg className="w-full h-full rotate-[-90deg]">
          <circle
            cx="24"
            cy="24"
            r="20"
            stroke="#e5e7eb"
            strokeWidth="4"
            fill="none"
          />
          <circle
            cx="24"
            cy="24"
            r="20"
            stroke="#3b82f6"
            strokeWidth="4"
            fill="none"
            strokeDasharray={2 * Math.PI * 20}
            strokeDashoffset={
              2 * Math.PI * 20 * (1 - percent / 100)
            }
          />
        </svg>
        <div className="absolute inset-0 flex items-center justify-center text-xs font-bold">
          {percent}%
        </div>
      </div>
    );

    return (
      <section
        ref={modulesPanelRef}
        className="bg-white rounded-2xl border border-slate-200 shadow-sm h-full flex flex-col"
      >
        {/* Header */}
        <div className="px-6 py-4 border-b border-slate-100">
          <h3 className="text-lg font-semibold text-slate-800">
            Week Modules
          </h3>
          <p className="text-xs text-slate-500">
            Complete modules in sequence to progress
          </p>
        </div>

        {/* Modules list */}
        <div className="flex-1 overflow-y-auto scrollbar-hide px-6 py-4">
          <div className="space-y-3">
            {modules.map((m, idx) => (
              <div
                key={m.moduleId}
                className="flex items-start justify-between gap-4
                         rounded-xl border border-slate-200 px-4 py-3
                         hover:bg-slate-50 transition"
              >
                {/* Left: index + info */}
                <div className="flex gap-4">
                  <div className="text-xs font-semibold text-slate-400 mt-1">
                    {idx + 1}
                  </div>

                  <div>
                    <div className="text-sm font-semibold text-slate-800">
                      {m.moduleName}
                    </div>

                    <div className="text-sm text-slate-600 mt-0.5 line-clamp-2">
                      {m.description}
                    </div>

                    <div className="flex gap-4 text-xs text-slate-500 mt-1">
                      {m.duration && <span>{m.duration} mins</span>}
                      <span>
                        {m.lessonProgressPercent ?? 0}% completed
                      </span>

                    </div>
                  </div>
                </div>

                {/* Right: status / action */}
                <div className="flex flex-col items-end gap-2">
                  {!m.isUnlocked ? (
                    <div className="flex items-center gap-1 text-xs text-slate-400">
                      <Lock className="w-4 h-4" />
                      Locked
                    </div>
                  ) : (
                    <>
                      <button
                        onClick={() => onOpen(m)}
                        className="inline-flex items-center gap-2
             px-3 py-1.5 text-xs font-semibold
             rounded-md bg-blue-600 text-white
             hover:bg-blue-700 transition"
                      >
                        <EyeIcon className="w-4 h-4" />
                        Open
                      </button>


                      {m.isCompleted && (
                        <div className="flex items-center gap-1 text-xs text-emerald-600">
                          <CheckCircle2 className="w-4 h-4" />
                          Completed
                        </div>
                      )}
                    </>
                  )}
                </div>
              </div>
            ))}

            {!modules.length && (
              <div className="text-sm text-slate-400 text-center py-6">
                No modules in this week
              </div>
            )}
          </div>
        </div>
      </section>
    );
  };
  async function submitAssessmentApi(payload) {
    const res = await fetch(
      `/api/assessment/${payload.assessmentId}/submit`,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      }
    );

    if (!res.ok) {
      throw new Error("Assessment scoring failed");
    }

    return await res.json();
  }

  const ModuleDetailInline = ({ detail, userId, refreshProgress, planProgress }) => {
    const [activeLessonLocal, setActiveLessonLocal] = useState(null);
    const [assessmentModalOpenLocal, setAssessmentModalOpenLocal] = useState(false);
    const [assessmentReviewMode, setAssessmentReviewMode] = useState(false);
    const [activeAssessmentLocal, setActiveAssessmentLocal] = useState(null);
    const [answersLocal, setAnswersLocal] = useState({});
    const [assessmentResult, setAssessmentResult] = useState(null);
    const [showIncompleteAssessmentsModalLocal, setShowIncompleteAssessmentsModalLocal] =
      useState(false);
    const [incompleteAssessmentsLocal, setIncompleteAssessmentsLocal] = useState([]);
    const allAssessments = detail.assessments || [];


    if (!detail) return null;

    const completeLessonLocal = async (lessonId) => {
      try {
        const res = await fetch(
          `/api/user-progress/${userId}/lesson/${lessonId}/complete`,
          { method: "POST" }
        );

        if (!res.ok) throw new Error();

        toast.success("Lesson completed");
        await refreshAndReopenModule();

      } catch {
        toast.error("Failed to mark lesson complete");
      }
    };
    const pendingAssessments =
      allAssessments.filter(
        a => !a.latestResult || a.latestResult.isCompleted !== true
      );
    const startAssessmentLocal = async (assessment) => {
      if (assessment.latestResult?.passed === true) {
        toast.success("Assessment already completed. Review only.");
        reviewAssessmentLocal(assessment);
        return;
      }

      try {
        await fetch(
          `/api/user-progress/${userId}/assessment/${assessment.assessmentId}/start?moduleId=${detail.moduleId}`,
          { method: "POST" }
        );

        setAssessmentReviewMode(false);
        setActiveAssessmentLocal(assessment);
        setAssessmentModalOpenLocal(true);
        setAssessmentResult(null);
        setAnswersLocal({});
      } catch (err) {
        toast.error("Failed to start assessment");
      }
    };


    const reviewAssessmentLocal = async (assessment) => {
      const result = assessment.latestResult;
      if (!result) return;

      setAssessmentReviewMode(true);
      setActiveAssessmentLocal(assessment);
      setAssessmentModalOpenLocal(true);
      setAssessmentResult(result);

      try {
        const parsed = JSON.parse(result.userAnswers || "{}");
        const map = {};
        assessment.questions.forEach((q, qi) => {
          map[qi] = parsed[q.questionId] || "";
        });
        setAnswersLocal(map);
      } catch {
        setAnswersLocal({});
      }
    };
    const submitAssessmentLocal = async () => {
      if (!activeAssessmentLocal) return;

      const answers = {};
      activeAssessmentLocal.questions.forEach((q, i) => {
        if (answersLocal[i]) {
          answers[q.questionId] = answersLocal[i];
        }
      });

      if (Object.keys(answers).length === 0) {
        toast.error("Please answer at least one question");
        return;
      }

      try {
        const result = await submitAssessmentApi({
          userId,
          assessmentId: activeAssessmentLocal.assessmentId,
          userAnswers: JSON.stringify(answers)
        });

        toast.success(
          result.passed
            ? `Passed (${Math.round(result.scorePercentage)
            }%)`
            : `Failed (${Math.round(result.scorePercentage)
            }%)`
        );
        await refreshProgress();
        await openModule({
          moduleId: detail.moduleId,
          isUnlocked: true
        });

        setAssessmentModalOpenLocal(false);
        setActiveAssessmentLocal(null);
        setAssessmentReviewMode(false);
        setAnswersLocal({});
        setAssessmentResult(null);

      } catch (e) {
        console.error(e);
        toast.error("Failed to submit assessment");
      }
    };
    const lessonsDone =
      (detail.lessons || []).length === 0 ||
      detail.lessons.every(l => l.isCompleted === true);
    const assessmentsDone = allAssessments.every(
      a => a.latestResult?.isCompleted === true
    );

    const canCompleteModule = lessonsDone && assessmentsDone;
    const handleMarkModuleCompleteLocal = async () => {
      if (!canCompleteModule) {
        setIncompleteAssessmentsLocal(pendingAssessments);
        setShowIncompleteAssessmentsModalLocal(true);
        return;
      }

      try {
        const res = await fetch(
          `/api/user-progress/${userId}/module/${detail.moduleId}/complete
`,
          { method: "POST" }
        );

        if (!res.ok) {
          toast.error("Cannot complete module");
          return;
        }

        toast.success("Module completed ");
        await refreshAndReopenModule();

      } catch (err) {
        console.error(err);
        toast.error("Failed to mark module complete");
      }
    };


    return (
      <div
        className="bg-white rounded-2xl shadow-sm p-5 border border-slate-200 mt-2 space-y-4"
        id="module-detail-inline"
      >
        <div className="flex justify-between items-start gap-6">
          <div className="space-y-2">
            <h3 className="text-2xl font-semibold text-slate-800">
              {detail.moduleName}
            </h3>
            <p className="text-sm text-slate-500 max-w-2xl">
              {detail.description}
            </p>
            <div className="mt-4 space-y-2">
            </div>
          </div>

          {detail.isCompleted ? (
            <div className="inline-flex items-center gap-2 text-emerald-600 font-semibold">
              <CheckCircle2 className="w-5 h-5" />
              Completed
            </div>
          ) : (
            <button
              onClick={handleMarkModuleCompleteLocal}
              disabled={!canCompleteModule}
              className={`inline-flex items-center gap-2 px-4 py-2 text-sm font-semibold rounded-lg
      ${canCompleteModule
                  ? "bg-emerald-600 text-white"
                  : "bg-slate-300 text-slate-500 cursor-not-allowed"
                }`}
            >
              <CheckCircle2 className="w-4 h-4" />
              Mark Complete
            </button>
          )}
        </div>
        {!activeLessonLocal && (
          <div className="space-y-4">
            {detail.lessons.map((ls, idx) => (
              <div
                key={ls.lessonId}
                className="flex justify-between items-center
                       p-5 rounded-xl border border-slate-200
                       hover:border-slate-300 transition"
              >
                <div className="space-y-1">
                  <div className="font-medium text-slate-800">
                    {idx + 1}. {ls.title}
                  </div>
                  <div className="text-xs text-slate-500">
                    {ls.overview}
                  </div>
                </div>

                <button
                  onClick={() => setActiveLessonLocal(ls)}
                  className="inline-flex items-center gap-2
                         px-3 py-1.5 text-xs font-semibold
                         bg-slate-900 text-white rounded-md"
                >
                  <EyeIcon className="w-4 h-4" />
                  View
                </button>
              </div>
            ))}
          </div>
        )}
        {activeLessonLocal && (
          <div className="space-y-5">

            <div className="space-y-1">
              <button
                onClick={async () => {
                  await completeLessonLocal(activeLessonLocal.lessonId);
                  setActiveLessonLocal(null);
                }}
              >
                <ArrowLeft className="w-4 h-4" />
              </button>
              <h3 className="text-xl font-semibold text-slate-800">
                {activeLessonLocal.title}
              </h3>
              <p className="text-sm text-slate-500">
                {activeLessonLocal.overview}
              </p>
            </div>
            <div className="border border-slate-200 rounded-xl overflow-hidden bg-white">
              {activeLessonLocal.content?.includes("youtube") ? (
                <iframe
                  className="w-full h-[460px]"
                  src={`https://www.youtube.com/embed/${(
                    activeLessonLocal.content.match(
                      /(?:v=|\/)([0-9A-Za-z_-]{11})/
                    ) || []
                  )[1]}`}
                  allowFullScreen
                />
              ) : (
                <div
                  className="prose max-w-none p-6"
                  dangerouslySetInnerHTML={{
                    __html: activeLessonLocal.content ?? "<i>No content</i>",
                  }}
                />
              )}
            </div>

            {/* ================= RESOURCES ================= */}
            <div className="space-y-3">
              <h4 className="text-sm font-semibold uppercase text-slate-600 flex items-center gap-2">
                <Paperclip className="w-4 h-4" />
                Resources
              </h4>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {(activeLessonLocal.resources || []).map((r) => (
                  <div key={r.resourceId}>
                    {renderResourcePreview(r)}
                  </div>
                ))}
              </div>
            </div>

            {/* ================= ASSESSMENTS ================= */}
            <div className="space-y-4">
              <h4 className="text-sm font-semibold uppercase text-slate-600 flex items-center gap-2">
                <ClipboardCheck className="w-4 h-4" />
                Assessments
              </h4>
              {(detail.assessments || []).map(a => {
                const r = a.latestResult;

                return (
                  <div
                    key={a.assessmentId}
                    className="flex justify-between items-center p-5 border rounded-xl"
                  >
                    {/* LEFT SIDE */}
                    <div>
                      <div className="font-medium">{a.title}</div>

                      {!r && (
                        <div className="text-xs text-slate-400">Not taken</div>
                      )}

                      {r && (
                        <div
                          className={`text-xs ${r.passed ? "text-green-600" : "text-red-600"
                            }`}
                        >
                          {r.passed ? "Passed" : "Failed"} • {r.scorePercentage}%
                        </div>
                      )}
                    </div>
                    <div className="flex gap-2">
                      {!r && (
                        <button onClick={() => startAssessmentLocal(a)}>
                          Take Test
                        </button>
                      )}

                      {r?.passed === true && (
                        <button onClick={() => reviewAssessmentLocal(a)}>
                          Review
                        </button>
                      )}

                      {r?.passed === false && (
                        <button onClick={() => startAssessmentLocal(a)}>
                          Retake
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}
        {/* INCOMPLETE ASSESSMENTS MODAL */}
        {showIncompleteAssessmentsModalLocal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div
              className="absolute inset-0 bg-black/50"
              onClick={() => setShowIncompleteAssessmentsModalLocal(false)}
            />

            <div className="relative max-w-xl w-full mx-4 bg-white rounded-2xl shadow-xl p-6 space-y-4">
              <h3 className="text-xl font-semibold text-red-600 flex items-center gap-2">
                <AlertTriangle className="w-5 h-5" />
                Complete Required Tests
              </h3>

              <p className="text-sm text-slate-600">
                You must complete all assessments before completing this module.
              </p>

              <div className="space-y-3 max-h-[50vh] overflow-auto scrollbar-hide">
                {incompleteAssessmentsLocal.map((a) => (
                  <div
                    key={a.assessmentId}
                    className="flex justify-between items-center border border-slate-200 rounded-lg p-4"
                  >
                    <div>
                      <div className="font-medium text-slate-800">{a.title}</div>
                      <div className="text-xs text-slate-500">
                        {a.description || "Assessment pending"}
                      </div>
                    </div>

                    <button
                      onClick={() => {
                        setShowIncompleteAssessmentsModalLocal(false);
                        startAssessmentLocal(a);
                      }}
                      className="inline-flex items-center gap-2 px-3 py-1.5 text-xs font-semibold bg-blue-600 text-white rounded-md"
                    >
                      <PlayCircle className="w-4 h-4" />
                      Take Test
                    </button>
                  </div>
                ))}
              </div>

              <div className="flex justify-end">
                <button
                  onClick={() => setShowIncompleteAssessmentsModalLocal(false)}
                  className="px-4 py-2 bg-slate-100 rounded-md"
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        )}
        {/* ================= ASSESSMENT MODAL ================= */}
        {assessmentModalOpenLocal && activeAssessmentLocal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div
              className="absolute inset-0 bg-black/50"
              onClick={() => setAssessmentModalOpenLocal(false)}
            />

            <div className="relative bg-white rounded-2xl shadow-2xl p-6 max-w-3xl w-full">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="text-lg font-semibold text-slate-800">
                    {activeAssessmentLocal.title}
                  </h3>
                  <p className="text-sm text-slate-500">
                    {activeAssessmentLocal.description}
                  </p>
                </div>

                <button
                  onClick={() => setAssessmentModalOpenLocal(false)}
                  className="px-3 py-1 bg-slate-100 rounded-md text-sm"
                >
                  Close
                </button>
              </div>

              <div className="space-y-4 max-h-[60vh] overflow-auto pr-2">
                {(activeAssessmentLocal.questions || []).map((q, qi) => {
                  const options = JSON.parse(q.options || "[]");
                  const userAnswer = answersLocal[qi];
                  const isCorrect = userAnswer === q.correctAnswer;

                  return (
                    <div key={qi} className="border rounded-lg p-4">
                      <div className="font-medium text-slate-800 mb-2">
                        {qi + 1}. {q.question}
                      </div>

                      <div className="space-y-2">
                        {options.map((opt, oi) => {
                          const optionKey = String.fromCharCode(65 + oi);
                          const checked = userAnswer === optionKey;

                          return (
                            <label
                              key={oi}
                              className={`flex items-center gap-2 text-sm ${assessmentReviewMode
                                ? optionKey === q.correctAnswer
                                  ? "text-green-600 font-semibold"
                                  : checked
                                    ? "text-red-600"
                                    : ""
                                : ""
                                }`}
                            >
                              <input
                                type="radio"
                                disabled={assessmentReviewMode}
                                checked={checked}
                                onChange={() =>
                                  !assessmentReviewMode &&
                                  setAnswersLocal(prev => ({
                                    ...prev,
                                    [qi]: optionKey
                                  }))
                                }
                              />
                              {opt}
                            </label>
                          );
                        })}
                      </div>

                      {assessmentReviewMode && (
                        <div className="mt-3 text-xs">
                          <div className="text-green-600">
                            Correct answer: {q.correctAnswer}
                          </div>
                          <div className="text-slate-500">
                            Your answer: {userAnswer || "—"}
                          </div>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
              <div className="mt-6 flex justify-end gap-3">
                <button
                  onClick={() => setAssessmentModalOpenLocal(false)}
                  className="px-4 py-2 bg-slate-200 rounded-md"
                >
                  Close
                </button>

                {!assessmentReviewMode && (
                  <button
                    onClick={submitAssessmentLocal}
                    className="px-4 py-2 bg-blue-600 text-white rounded-md"
                  >
                    Submit
                  </button>
                )}
              </div>

            </div>
          </div>
        )}

      </div>
    );

  };

  function renderResourcePreview(resource) {
    if (!resource) return null;

    const url = resource.url || "";
    if (!url) return null;

    const isImage = /\.(png|jpg|jpeg|gif|webp)$/i.test(url);
    const isVideo = url.toLowerCase().endsWith(".mp4");
    const isPDF = url.toLowerCase().endsWith(".pdf");
    const isDoc = /\.(docx|pptx|xlsx|csv)$/i.test(url);
    const isYouTube = url.includes("youtube.com") || url.includes("youtu.be");

    const ytId = (url.match(/(?:v=|\/)([0-9A-Za-z_-]{11})/) || [])[1];

    const openFull = () => setFullScreenResource(resource);

    return (
      <div onClick={openFull} className="cursor-pointer bg-white rounded-xl border shadow-sm hover:shadow-md transition overflow-hidden">
        <div className="w-full h-28 bg-gray-100 flex items-center justify-center overflow-hidden">
          {isImage && <img src={url} alt={resource.title} className="w-full h-full object-cover pointer-events-none" />}

          {isVideo && <video src={url} muted className="w-full h-full object-cover pointer-events-none" />}

          {isPDF && (
            <div className="w-full h-full overflow-hidden">
              <iframe src={url} className="w-full h-full scale-[1.1] origin-top pointer-events-none" title={resource.title || "PDF preview"} />
            </div>
          )}

          {isDoc && (
            <div className="w-full h-full overflow-hidden">
              <iframe src={`https://view.officeapps.live.com/op/embed.aspx?src=${encodeURIComponent(url)}`} className="w-full h-full scale-[1.1] origin-top pointer-events-none" title={resource.title || "Document preview"} />
            </div>
          )}

          {isYouTube && ytId && <iframe className="w-full h-full pointer-events-none" src={`https://www.youtube.com/embed/${ytId}`} title={resource.title || "YouTube preview"} />}

          {!isImage && !isVideo && !isPDF && !isDoc && !(isYouTube && ytId) && <div className="text-4xl text-gray-500">📁</div>}
        </div>

        <div className="px-2 py-2">
          <div className="text-xs font-semibold text-gray-800 truncate">{resource.title || "Untitled file"}</div>
          <div className="text-[10px] text-gray-500 truncate">{resource.url}</div>
        </div>
      </div>
    );
  }

  function renderFullScreenResource(resource) {
    if (!resource) return null;

    const url = resource.url || "";
    const isImage = /\.(png|jpg|jpeg|gif|webp)$/i.test(url);
    const isVideo = url.toLowerCase().endsWith(".mp4");
    const isPDF = url.toLowerCase().endsWith(".pdf");
    const isDoc = /\.(docx|pptx|xlsx|csv)$/i.test(url);
    const isYouTube = url.includes("youtube.com") || url.includes("youtu.be");
    const ytId = (url.match(/(?:v=|\/)([0-9A-Za-z_-]{11})/) || [])[1];

    return (
      <div className="w-full h-full flex items-center justify-center">
        {isImage && <img src={url} alt={resource.title} className="max-w-full max-h-[90vh] object-contain rounded-lg shadow-lg" />}

        {isVideo && <video src={url} controls className="max-w-full max-h-[90vh] object-contain rounded-lg shadow-lg bg-black" />}

        {isPDF && <iframe src={url} className="w-full h-[90vh] rounded-lg shadow-lg bg-white" title={resource.title || "PDF"} />}

        {isDoc && <iframe src={`https://view.officeapps.live.com/op/embed.aspx?src=${encodeURIComponent(url)}`} className="w-full h-[90vh] rounded-lg shadow-lg bg-white" title={resource.title || "Document"} />}

        {isYouTube && ytId && <iframe className="w-[80vw] h-[75vh] rounded-lg shadow-lg" src={`https://www.youtube.com/embed/${ytId}`} allowFullScreen title={resource.title || "YouTube video"} />}

        {!isImage && !isVideo && !isPDF && !isDoc && !(isYouTube && ytId) && (
          <a href={url} target="_blank" rel="noreferrer" className="px-4 py-2 bg-blue-600 text-white rounded-lg shadow">Open Resource</a>
        )}
      </div>
    );
  }
  const overallPercent =
    !planProgress || planProgress.modules.length === 0
      ? 0
      : Math.round(
        (planProgress.modules.filter(m => m.isCompleted).length /
          planProgress.modules.length) * 100
      );

  const streak =
    planProgress?.modules?.filter(
      m => (m.lessonProgressPercent || 0) > 0
    ).length || 0;

  // ================== MAIN RENDER ==================
  return (
    <div className="min-h-screen bg-gradient-to-b from-white to-blue-50 pb-32">
      <div className="max-w-7xl mx-auto p-4 space-y-4">
        <motion.div initial={{ opacity: 0, y: -8 }} animate={{ opacity: 1, y: 0 }} className="text-center">
          <h1 className="text-3xl font-extrabold text-[#123A63]"> Learning Platform</h1>
          <p className="mt-1 text-gray-600">A modern learning experience — plans, weeks, modules, lessons and assessments.</p>
        </motion.div>
        <PlansBar
          plans={plans}
          selected={selectedPlan}
          loadPlans={loadPlans}
          onSelect={selectPlan}
          onShowDetails={handleShowPlanDetails}
        />
        {planDetailsVisible && (
          <div className="mt-6">
            {planDetails ? (
              <PlanDetailsPanel
                details={planDetails}
                onClose={() => setPlanDetailsVisible(false)}
              />
            ) : (
              <div className="bg-white rounded-xl border shadow p-8 text-center text-sm text-gray-500">
                Loading plan details…
              </div>
            )}
          </div>
        )}

        {planProgress && (
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4 }}
            className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4"
          >
            {/* PROGRESS */}
            <motion.div
              whileHover={{ y: -4 }}
              className="bg-gradient-to-br from-blue-50 to-indigo-100
                 border border-blue-100
                 rounded-2xl p-5 shadow-sm"
            >
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-xl bg-blue-100 text-blue-600">
                  <TrendingUp className="w-5 h-5" />
                </div>
                <div>
                  <div className="text-xs uppercase tracking-wide text-blue-500">
                    Overall Progress
                  </div>
                  <div className="text-2xl font-bold text-blue-700">
                    {overallPercent}%
                  </div>
                </div>
              </div>

              <div className="mt-3 h-2 bg-blue-100 rounded-full overflow-hidden">
                <motion.div
                  className="h-2 bg-blue-500 rounded-full"
                  initial={{ width: 0 }}
                  animate={{ width: `${overallPercent}%` }}
                  transition={{ duration: 0.8 }}
                />
              </div>
            </motion.div>

            {/* STREAK */}
            <motion.div
              whileHover={{ y: -4 }}
              className="bg-gradient-to-br from-orange-50 to-amber-100
                 border border-orange-100
                 rounded-2xl p-5 shadow-sm"
            >
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-xl bg-orange-100 text-orange-600">
                  <Flame className="w-5 h-5" />
                </div>
                <div>
                  <div className="text-xs uppercase tracking-wide text-orange-500">
                    Active Streak
                  </div>
                  <div className="text-2xl font-bold text-orange-700">
                    {streak}
                  </div>
                  <div className="text-xs text-orange-600">
                    Active modules
                  </div>
                </div>
              </div>
            </motion.div>

            {/* COMPLETED */}
            <motion.div
              whileHover={{ y: -4 }}
              className="bg-gradient-to-br from-emerald-50 to-teal-100
                 border border-emerald-100
                 rounded-2xl p-5 shadow-sm"
            >
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-xl bg-emerald-100 text-emerald-600">
                  <CheckCircle2 className="w-5 h-5" />
                </div>
                <div>
                  <div className="text-xs uppercase tracking-wide text-emerald-500">
                    Completed
                  </div>
                  <div className="text-2xl font-bold text-emerald-700">
                    {planProgress.modules.filter(m => m.isCompleted).length}
                  </div>
                  <div className="text-xs text-emerald-600">
                    Modules done
                  </div>
                </div>
              </div>
            </motion.div>

            {/* REMAINING */}
            <motion.div
              whileHover={{ y: -4 }}
              className="bg-gradient-to-br from-purple-50 to-fuchsia-100
                 border border-purple-100
                 rounded-2xl p-5 shadow-sm"
            >
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-xl bg-purple-100 text-purple-600">
                  <Clock className="w-5 h-5" />
                </div>
                <div>
                  <div className="text-xs uppercase tracking-wide text-purple-500">
                    Remaining
                  </div>
                  <div className="text-2xl font-bold text-purple-700">
                    {planProgress.modules.filter(m => !m.isCompleted).length}
                  </div>
                  <div className="text-xs text-purple-600">
                    Modules left
                  </div>
                </div>
              </div>
            </motion.div>
          </motion.div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-1">
            <WeeksPanel weeks={weeks} onSelect={selectWeek} selected={selectedWeek} />
          </div>
          <div className="lg:col-span-2 space-y-6">
            <ModulesGrid modules={modules} onOpen={openModule} />
          </div>
        </div>
        {selectedModule && moduleDetail && (
          <div className="col-span-full">
            <ModuleDetailInline
              detail={moduleDetail}
              userId={userId}
              refreshProgress={refreshProgress}
              planProgress={planProgress}
            />
          </div>
        )}

      </div>
      {activeLesson && (
        <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-8 mt-6 space-y-8">
          {/* HEADER */}
          <div className="flex justify-between items-start gap-6">
            <div className="space-y-1">
              <h3 className="text-2xl font-semibold text-slate-800 flex items-center gap-2">
                <BookOpen className="w-5 h-5 text-blue-600" />
                {activeLesson.title}
              </h3>
              <p className="text-sm text-slate-500">{activeLesson.overview}</p>
            </div>

            <button
              onClick={() => setActiveLesson(null)}
              className="inline-flex items-center gap-2 px-4 py-2
                   text-sm font-semibold rounded-md
                   bg-slate-100 hover:bg-slate-200"
            >
              <X className="w-4 h-4" />
              Close
            </button>
          </div>

          {/* CONTENT */}
          <div className="border border-slate-200 rounded-xl overflow-hidden">
            {activeLesson.content ? (
              activeLesson.content.includes("youtube") ||
                activeLesson.content.includes("youtu.be") ? (
                <iframe
                  className="w-full h-[460px]"
                  src={`https://www.youtube.com/embed/${(
                    activeLesson.content.match(/(?:v=|\/)([0-9A-Za-z_-]{11})/) || []
                  )[1] || ""}`}
                  allowFullScreen
                />
              ) : (
                <div
                  className="prose max-w-none p-6"
                  dangerouslySetInnerHTML={{ __html: activeLesson.content }}
                />
              )
            ) : (
              <div className="p-6 text-sm text-slate-400">
                No content available
              </div>
            )}
          </div>

          {/* RESOURCES */}
          <div className="space-y-3">
            <h4 className="text-sm font-semibold uppercase text-slate-600 flex items-center gap-2">
              <Paperclip className="w-4 h-4" />
              Resources
            </h4>

            {!activeLesson.resources?.length && (
              <div className="text-sm text-slate-400">No resources</div>
            )}

            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
              {(activeLesson.resources || []).map((r) => (
                <div key={r.resourceId}>{renderResourcePreview(r)}</div>
              ))}
            </div>
          </div>

          {/* ASSESSMENTS */}
          <div className="space-y-4">
            <h4 className="text-sm font-semibold uppercase text-slate-600 flex items-center gap-2">
              <ClipboardCheck className="w-4 h-4" />
              Assessments
            </h4>

            {!activeLesson.assessments?.length && (
              <div className="text-sm text-slate-400">No assessments</div>
            )}

            <div className="space-y-3">
              {(activeLesson.assessments || []).map((a) => (
                <div
                  key={a.assessmentId}
                  className="flex justify-between items-center
                       border border-slate-200 rounded-xl p-4"
                >
                  <div className="space-y-1">
                    <div className="font-medium text-slate-800">
                      {a.title}
                    </div>
                    <div className="text-xs text-slate-500">
                      {a.description}
                    </div>
                  </div>

                  <button
                    onClick={() => startAssessment(a)}
                    className="inline-flex items-center gap-2
                         px-4 py-2 text-xs font-semibold
                         bg-blue-600 text-white rounded-md"
                  >
                    <PlayCircle className="w-4 h-4" />
                    Take Test
                  </button>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* FULLSCREEN RESOURCE */}
      {fullScreenResource && (
        <div className="fixed inset-0 z-[100] bg-black/80 flex flex-col">
          <div className="flex justify-between items-center px-5 py-3 bg-black/60 text-white">
            <div className="text-sm truncate flex items-center gap-2">
              <FileText className="w-4 h-4" />
              {fullScreenResource.title || fullScreenResource.url}
            </div>

            <button
              onClick={() => setFullScreenResource(null)}
              className="inline-flex items-center gap-2
                   px-3 py-1.5 text-xs rounded-md
                   bg-white/10 hover:bg-white/20"
            >
              <X className="w-4 h-4" />
              Close
            </button>
          </div>

          <div className="flex-1 flex items-center justify-center p-4">
            {renderFullScreenResource(fullScreenResource)}
          </div>
        </div>
      )}
    </div>
  );
}
