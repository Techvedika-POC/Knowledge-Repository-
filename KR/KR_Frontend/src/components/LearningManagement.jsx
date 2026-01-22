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
  ChevronDown
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
  const refreshProgress = async () => {
    if (!selectedPlan || !userId) return null;

    const res = await fetch(
      `/api/UserProgressAggregate/${userId}/plan/${selectedPlan.planId}`
    );
    if (!res.ok) throw new Error("Failed to refresh progress");

    const data = await res.json();
    setPlanProgress(data);

    return data;
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
  const buildProgressMap = (progressList = []) => {
    const map = new Map();
    if (!Array.isArray(progressList)) return map;
    progressList.forEach((p) => {
      const moduleId =
        p.moduleId || p.ModuleId || p.moduleID || p.ModuleID || p.ModuleId;
      if (!moduleId) return;
      map.set(String(moduleId).toLowerCase(), {
        isUnlocked: !!(p.isUnlocked ?? p.IsUnlocked ?? p.IsUnlocked === true),
        isCompleted: !!(
          p.isCompleted ??
          p.IsCompleted ??
          (p.Status === "Completed") ??
          (p.status === "Completed")
        ),
        testStatus:
          p.testStatus ?? p.TestStatus ?? p.teststatus ?? p.Teststatus ?? "NotTaken",
        weekId: p.weekId ?? p.WeekId ?? p.topicId ?? p.TopicId ?? null,
      });
    });
    return map;
  };
  const mergeModuleProgressToModules = (modulesList = [], progressMap) => {
    const pm =
      progressMap instanceof Map ? progressMap : buildProgressMap(progressMap || []);
    return (Array.isArray(modulesList) ? modulesList : []).map((m, idx) => {
      const moduleId = m.moduleId ?? m.ModuleId ?? m.ModuleID ?? m.ModuleId;
      const key = moduleId ? String(moduleId).toLowerCase() : null;
      const prog = key ? pm.get(key) : null;
      const isUnlocked = prog ? !!prog.isUnlocked : !!m.isUnlocked;
      const isCompleted = prog ? !!prog.isCompleted : !!m.isCompleted;
      const testStatus = prog?.testStatus || "NotTaken";
      const percentCompleted = testStatus === "Passed" ? 100 : 0;

      return {
        ...m,
        isUnlocked,
        isLocked: !isUnlocked,
        isCompleted,
        _progress: { ...(prog || {}), percentCompleted, testStatus },
        orderNo:
          m.orderNo ??
          m.OrderNo ??
          m.OrderIndex ??
          m.orderIndex ??
          idx + 1,
      };
    });
  };
  const applyWeekUnlocks = async (weeksArr = [], progressMap, planId = null) => {
    if (!Array.isArray(weeksArr) || weeksArr.length === 0) return weeksArr;
    const weeksClone = weeksArr.map((w) => ({
      ...w,
      isUnlocked: false,
      isCompleted: !!w.isCompleted,
    }));
    weeksClone.sort((a, b) => {
      const na = a.weekNumber ?? a.WeekNumber ?? 99999;
      const nb = b.weekNumber ?? b.WeekNumber ?? 99999;
      return na - nb;
    });
    if (weeksClone.length > 0) weeksClone[0].isUnlocked = true;

    const fetchModules = async (weekId) => {
      if (!weekId) return [];
      try {
        const res = await api.get(`/Module/week/${weekId}`);
        const arr = Array.isArray(res.data) ? res.data : [];
        return arr.map((m) => ({ ...m, weekId }));
      } catch {
        return [];
      }
    };

    const pm =
      progressMap instanceof Map ? progressMap : buildProgressMap(progressMap || []);

    for (let i = 1; i < weeksClone.length; i++) {
      const prevWeek = weeksClone[i - 1];
      if (!prevWeek.isUnlocked) {
        weeksClone[i].isUnlocked = false;
        continue;
      }
      const prevModules = await fetchModules(
        prevWeek.weekId ?? prevWeek.id ?? prevWeek.weekId
      );
      if (prevModules.length === 0) {
        weeksClone[i].isUnlocked = true;
        continue;
      }
      let allCompleted = true;
      for (const m of prevModules) {
        const moduleId = m.moduleId ?? m.ModuleId ?? m.ModuleID;
        if (!moduleId) {
          allCompleted = false;
          break;
        }
        const key = String(moduleId).toLowerCase();
        const p = pm.get(key);
        if (!p || p.isCompleted !== true) {
          allCompleted = false;
          break;
        }
      }
      weeksClone[i].isUnlocked = allCompleted;
    }
    return weeksClone;
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
      const weeksRes = await api.get(`/Week/full/plan/${p.planId}`, {
        params: { userId }
      });
      const weeksData = weeksRes.data || [];
      const progress = await refreshProgress();
      const progressMap = new Map(
        (progress?.modules || []).map(m => [
          String(m.moduleId).toLowerCase(),
          m
        ])
      );
      const unlockedWeeks = await applyWeekUnlocks(
        weeksData,
        progressMap,
        p.planId
      );
      setWeeks(unlockedWeeks);
      await fetchPlanDetails(p);
    } catch (err) {
      toast.error("Failed to load plan");
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

    const res = await api.get(`/Module/week/${w.weekId}`);
    const modulesData = res.data || [];

    const progressMap = new Map(
      (planProgress?.modules || []).map(m => [
        String(m.moduleId).toLowerCase(),
        m
      ])
    );

    let merged = mergeModuleProgressToModules(
      modulesData,
      progressMap
    );
    if (merged.length > 0) {
      merged = merged.map((m, idx) => ({
        ...m,
        isUnlocked: idx === 0 ? true : m.isUnlocked,
        isLocked: idx === 0 ? false : !m.isUnlocked
      }));
    }
    setModules(merged);
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
    setModuleDetail(null);
    setLoading(true);

    try {
      const res = await api.get(`/Module/${m.moduleId}`, {
        params: { userId }
      });

      const content = attachResourcesAndAssessments(res.data || {});
      const pm =
        planProgress?.modules?.find(
          x => String(x.moduleId).toLowerCase() === String(m.moduleId).toLowerCase()
        ) || null;
      if (!pm) {
        setModuleDetail(content);
        return;
      }
      const assessmentProgressMap = new Map(
        (pm.assessments || []).map(a => [
          String(a.assessmentId).toLowerCase(),
          a.latestResult || null
        ])
      );
      const lessonProgressMap = new Map(
        (pm.lessons || []).map(l => [
          String(l.lessonId).toLowerCase(),
          l.isCompleted === true
        ])
      );
      const merged = {
        ...content,
        isUnlocked: pm.isUnlocked,
        isCompleted: pm.isCompleted,
        lessonProgressPercent: pm.lessonProgressPercent ?? 0,

        lessons: content.lessons.map(ls => ({
          ...ls,

          isCompleted:
            lessonProgressMap.get(
              String(ls.lessonId).toLowerCase()
            ) === true,

          assessments: (ls.assessments || []).map(a => ({
            ...a,
            latestResult: assessmentProgressMap.get(
              String(a.assessmentId).toLowerCase()
            )
          }))
        }))
      };
      setModuleDetail(merged);

      setTimeout(() => {
        document
          .getElementById("module-detail-inline")
          ?.scrollIntoView({ behavior: "smooth" });
      }, 100);

    } catch (e) {
      console.error(e);
      toast.error("Failed to load module");
    } finally {
      setLoading(false);
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
  async function submitAssessmentApi(payload) {
    try {
      const res = await fetch("/api/assessment/submit", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
      if (!res.ok) {
        let text = await res.text().catch(() => "");
        console.error("Submit API error body:", text);
        return null;
      }

      const text = await res.text();
      if (!text || !text.trim()) return null;

      try {
        return JSON.parse(text);
      } catch (err) {
        console.warn("Submit returned invalid JSON:", text);
        return null;
      }
    } catch (err) {
      console.error("submitAssessmentApi failed:", err);
      return null;
    }
  }

  async function getAssessmentResultApi(assessmentId, userId) {
    if (!userId) return null;
    try {
      const res = await fetch(
        `/api/assessment/${assessmentId}/user/${userId}/result`
      );

      if (res.status === 404) return null;
      if (!res.ok) {
        let text = await res.text().catch(() => "");
        console.error("Assessment result error:", text);
        return null;
      }

      const text = await res.text();
      if (!text || !text.trim()) return null;

      try {
        return JSON.parse(text);
      } catch (err) {
        console.warn("Assessment result invalid JSON:", text);
        return null;
      }
    } catch (err) {
      console.error("getAssessmentResultApi() crashed:", err);
      return null;
    }
  }
  const handleShowPlanDetails = async (plan) => {
    await fetchPlanDetails(plan);
    setPlanDetailsVisible(true);
  };

  // --------------------- UI COMPONENTS ---------------------
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
                      {m._progress && (
                        <span>
                          {m._progress.percentCompleted ?? 0}% completed
                        </span>
                      )}
                    </div>
                  </div>
                </div>

                {/* Right: status / action */}
                <div className="flex flex-col items-end gap-2">
                  {m.isLocked || m.isUnlocked === false ? (
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

  const ModuleDetailInline = ({ detail, userId, refreshProgress, planProgress }) => {
    const [activeLessonLocal, setActiveLessonLocal] = useState(null);
    const [assessmentModalOpenLocal, setAssessmentModalOpenLocal] = useState(false);
    const [assessmentReviewMode, setAssessmentReviewMode] = useState(false);
    const [activeAssessmentLocal, setActiveAssessmentLocal] = useState(null);
    const [answersLocal, setAnswersLocal] = useState({});
    const [assessmentResult, setAssessmentResult] = useState(null);
    const [testTakenLocal, setTestTakenLocal] = useState(false);
    const [testScoreLocal, setTestScoreLocal] = useState(null);
    const [testPassedLocal, setTestPassedLocal] = useState(false);
    const [showIncompleteAssessmentsModalLocal, setShowIncompleteAssessmentsModalLocal] =
      useState(false);
    const [incompleteAssessmentsLocal, setIncompleteAssessmentsLocal] = useState([]);
    const autoCompleteRef = useRef(false);
    const allAssessments =
      (detail.lessons || []).flatMap(
        lesson => lesson.assessments || []
      );

    if (!detail) return null;
    const allLessons = detail.lessons || [];
    const completedLessons = allLessons.filter((l) => l.isCompleted).length;
    const completedAssessments = allAssessments.filter((a) => a.latestResult?.passed).length;

    const totalLessons = allLessons.length;
    const totalAssessments = allAssessments.length;

    const percent = Math.round(
      ((completedLessons + completedAssessments) / (totalLessons + totalAssessments || 1)) * 100
    );
    const completeLessonLocal = async (lessonId) => {
      try {
        const res = await fetch(
          `/api/userprogress/${userId}/module/${detail.moduleId}/lesson/${lessonId}/complete`,
          { method: "POST" }
        );

        if (!res.ok) throw new Error();

        toast.success("Lesson completed");

        // 🔑 backend is source of truth
        await refreshProgress();
        await openModule({ moduleId: detail.moduleId, isUnlocked: true });

      } catch {
        toast.error("Failed to mark lesson complete");
      }
    };
    const pendingAssessments =
      allAssessments.filter(
        a => !a.latestResult || a.latestResult.passed !== true
      );
    const ensureLatestResultLoaded = async (assessment) => {
      if (assessment.latestResult) return assessment.latestResult;
      const fetched = await getAssessmentResultApi(assessment.assessmentId, userId);
      if (fetched) assessment.latestResult = fetched;
      return fetched;
    };
    const startAssessmentLocal = async (assessment) => {
      await ensureLatestResultLoaded(assessment);
      setAssessmentReviewMode(false);
      setActiveAssessmentLocal(assessment);
      setAssessmentModalOpenLocal(true);
      setAssessmentResult(null);
      setAnswersLocal({});
      setTestTakenLocal(false);
      setTestScoreLocal(null);
      setTestPassedLocal(false);
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
        if (answersLocal[i]) answers[q.questionId] = answersLocal[i];
      });

      const result = await submitAssessmentApi({
        userId,
        assessmentId: activeAssessmentLocal.assessmentId,
        weekId: detail.weekId,
        userAnswers: JSON.stringify(answers)
      });

      if (!result) {
        toast.error("Failed to submit test");
        return;
      }

      toast.success("Test submitted");

      setAssessmentModalOpenLocal(false);

      if (!autoCompleteRef.current) {
        autoCompleteRef.current = true;

        for (const l of detail.lessons || []) {
          if (!l.isCompleted) {
            await fetch(
              `/api/userprogress/${userId}/module/${detail.moduleId}/lesson/${l.lessonId}/complete`,
              { method: "POST" }
            );
          }
        }
      }

      const freshProgress = await refreshProgress();

      if (selectedWeek && freshProgress) {
        const updatedModulesRes = await api.get(
          `/Module/week/${selectedWeek.weekId}`
        );

        const progressMap = new Map(
          (freshProgress.modules || []).map(m => [
            String(m.moduleId).toLowerCase(),
            m
          ])
        );

        let rebuilt = mergeModuleProgressToModules(
          updatedModulesRes.data,
          progressMap
        );

        // 🔑 always unlock first module
        if (rebuilt.length > 0) {
          rebuilt = rebuilt.map((m, idx) => ({
            ...m,
            isUnlocked: idx === 0 ? true : m.isUnlocked,
            isLocked: idx === 0 ? false : !m.isUnlocked
          }));
        }

        setModules(rebuilt);
      }

      await openModule({ moduleId: detail.moduleId, isUnlocked: true });

    };
    const lessonsDone =
      (detail.lessons || []).length === 0 ||
      detail.lessons.every(l => l.isCompleted === true);

    const assessmentsDone =
      allAssessments.length === 0 ||
      allAssessments.every(a => a.latestResult?.passed === true);

    const canCompleteModule = lessonsDone && assessmentsDone;
    const handleMarkModuleCompleteLocal = async () => {
      if (!canCompleteModule) {
        setIncompleteAssessmentsLocal(pendingAssessments);
        setShowIncompleteAssessmentsModalLocal(true);
        return;
      }

      try {
        // 1️⃣ Ensure all lessons completed
        for (const l of detail.lessons || []) {
          if (!l.isCompleted) {
            await fetch(
              `/api/userprogress/${userId}/module/${detail.moduleId}/lesson/${l.lessonId}/complete`,
              { method: "POST" }
            );
          }
        }

        // 2️⃣ Mark module complete
        const res = await fetch(
          `/api/userprogress/${userId}/module/${detail.moduleId}/complete`,
          { method: "POST" }
        );

        if (!res.ok) {
          toast.error("Lessons or assessments incomplete");
          return;
        }

        toast.success("Module completed 🎉");

        // 🔑 1️⃣ Get fresh progress from backend
        const freshProgress = await refreshProgress();

        if (selectedWeek && freshProgress) {
          const updatedModulesRes = await api.get(
            `/Module/week/${selectedWeek.weekId}`
          );

          const progressMap = new Map(
            (freshProgress.modules || []).map(m => [
              String(m.moduleId).toLowerCase(),
              m
            ])
          );

          let rebuilt = mergeModuleProgressToModules(
            updatedModulesRes.data,
            progressMap
          );

          // 🔑 always unlock first module
          if (rebuilt.length > 0) {
            rebuilt = rebuilt.map((m, idx) => ({
              ...m,
              isUnlocked: idx === 0 ? true : m.isUnlocked,
              isLocked: idx === 0 ? false : !m.isUnlocked
            }));
          }

          setModules(rebuilt);
        }
      } catch (err) {
        console.error(err);
        toast.error("Failed to mark module complete");
      }
    };
    return (
      <div
        className="bg-white rounded-2xl shadow-sm p-8 border border-slate-200 mt-6 space-y-8"
        id="module-detail-inline"
      >
        {/* ================= HEADER ================= */}
        <div className="flex justify-between items-start gap-6">
          <div className="space-y-2">
            <h3 className="text-2xl font-semibold text-slate-800">
              {detail.moduleName}
            </h3>
            <p className="text-sm text-slate-500 max-w-2xl">
              {detail.description}
            </p>

            {/* Progress */}
            <div className="mt-4 space-y-2">
              <div className="flex items-center justify-between text-xs text-slate-500">
                <span>📘 Lessons {completedLessons}/{totalLessons}</span>
                <span>📝 Tests {completedAssessments}/{totalAssessments}</span>
                <span className="font-semibold text-blue-600">{percent}%</span>
              </div>

              <div className="h-2 w-full bg-slate-200 rounded-full overflow-hidden">
                <div
                  className="h-2 bg-gradient-to-r from-blue-500 to-emerald-500 rounded-full"
                  style={{ width: `${percent}%` }}
                />
              </div>
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

        {/* ================= LESSON LIST ================= */}
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

        {/* ================= LESSON DETAIL ================= */}
        {activeLessonLocal && (
          <div className="space-y-8">
            <button
              onClick={async () => {
                await completeLessonLocal(activeLessonLocal.lessonId);
                setActiveLessonLocal(null);
              }}
            >
              <ArrowLeft className="w-4 h-4" />
              Back to lessons
            </button>

            <div className="space-y-1">
              <h3 className="text-xl font-semibold text-slate-800">
                {activeLessonLocal.title}
              </h3>
              <p className="text-sm text-slate-500">
                {activeLessonLocal.overview}
              </p>
            </div>

            {/* CONTENT */}
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

              {(activeLessonLocal.assessments || []).map((a) => {
                const r = a.latestResult;
                return (
                  <div
                    key={a.assessmentId}
                    className="flex justify-between items-center
                           border border-slate-200 rounded-xl p-5"
                  >
                    <div className="space-y-1">
                      <div className="font-medium text-slate-800">
                        {a.title}
                      </div>
                      {r && (
                        <div
                          className={`text-xs ${r.passed ? "text-emerald-600" : "text-red-600"
                            }`}
                        >
                          {r.passed ? "Passed" : "Failed"} •{" "}
                          {r.scorePercentage?.toFixed(0)}%
                        </div>
                      )}
                      {!r && (
                        <div className="text-xs text-slate-400">
                          Not taken yet
                        </div>
                      )}
                    </div>

                    {/* ACTIONS */}
                    {!r && (
                      <button
                        onClick={() => startAssessmentLocal(a)}
                        className="inline-flex items-center gap-2
                               px-4 py-2 text-xs font-semibold
                               bg-blue-600 text-white rounded-md"
                      >
                        <PlayCircle className="w-4 h-4" />
                        Take Test
                      </button>
                    )}

                    {r && r.passed && (
                      <button
                        onClick={() => reviewAssessmentLocal(a)}
                        className="inline-flex items-center gap-2
                               px-4 py-2 text-xs font-semibold
                               border border-blue-600 text-blue-600 rounded-md"
                      >
                        <EyeIcon className="w-4 h-4" />
                        Review
                      </button>
                    )}

                    {r && !r.passed && (
                      <button
                        onClick={() => startAssessmentLocal(a)}
                        className="inline-flex items-center gap-2
                               px-4 py-2 text-xs font-semibold
                               bg-orange-500 text-white rounded-md"
                      >
                        <RotateCcw className="w-4 h-4" />
                        Retake
                      </button>
                    )}
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

              {/* QUESTIONS */}
              <div className="space-y-4 max-h-[60vh] overflow-auto pr-2">
                {(activeAssessmentLocal.questions || []).map((q, qi) => {
                  const options = JSON.parse(q.options || "[]");

                  return (
                    <div key={qi} className="border rounded-lg p-4">
                      <div className="font-medium text-slate-800 mb-2">
                        {qi + 1}. {q.question}
                      </div>

                      <div className="space-y-2">
                        {options.map((opt, oi) => (
                          <label key={oi} className="flex items-center gap-2 text-sm">
                            <input
                              type="radio"
                              checked={answersLocal[qi] === String.fromCharCode(65 + oi)}
                              onChange={() =>
                                setAnswersLocal(prev => ({
                                  ...prev,
                                  [qi]: String.fromCharCode(65 + oi) // A, B, C, D
                                }))
                              }
                            />

                            {opt}
                          </label>
                        ))}
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* ACTIONS */}
              <div className="mt-6 flex justify-end gap-3">
                <button
                  onClick={() => setAssessmentModalOpenLocal(false)}
                  className="px-4 py-2 bg-slate-200 rounded-md"
                >
                  Cancel
                </button>

                <button
                  onClick={submitAssessmentLocal}
                  className="px-4 py-2 bg-blue-600 text-white rounded-md"
                >
                  Submit
                </button>
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
