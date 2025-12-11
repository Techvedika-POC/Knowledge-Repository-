import React, { useEffect, useState, useRef } from "react";
import api from "../api"; 
import toast from "react-hot-toast";
import { motion } from "framer-motion";
import {
  Lock,
  UnlockKeyhole,
  CheckCircle2,
  ExternalLink,
  Play,
  X,
} from "lucide-react";

export default function KnowledgeQuestDashboard() {
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
  const [incompleteAssessments, setIncompleteAssessments] = useState([]);
  const [showIncompleteAssessmentsModal, setShowIncompleteAssessmentsModal] =
    useState(false);
  const [fullScreenResource, setFullScreenResource] = useState(null);
  const weeksPanelRef = useRef(null);
  const modulesPanelRef = useRef(null);
  useEffect(() => {
    loadPlans();
    const onResize = () => equalizeHeights();
    window.addEventListener("resize", onResize);
    return () => window.removeEventListener("resize", onResize);
  }, []);

  useEffect(() => {
    setTimeout(equalizeHeights, 120);
  }, [weeks, modules, moduleDetail, weekDetails, planDetails]);

  // ---------- helpers ----------
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

  const parseOptions = (options) => {
    if (!options) return [];
    if (Array.isArray(options)) return options;
    try {
      return JSON.parse(options);
    } catch {
      if (typeof options === "string") {
        return options
          .split(",")
          .map((s) => s.trim())
          .filter(Boolean);
      }
      return [];
    }
  };

  // ---------- progress helpers ----------
  const loadUserProgressForPlan = async (planId) => {
    if (!userId || !planId) {
      setUserProgressModules([]);
      return [];
    }
    try {
      setLoading(true);
      const res = await api.get(`/UserProgress/${userId}/plan/${planId}`);
      const modulesProgress = res.data?.modules || res.data || [];
      const arr = Array.isArray(modulesProgress) ? modulesProgress : [];
      setUserProgressModules(arr);
      return arr;
    } catch (err) {
      console.error("Failed to load user progress", err);
      toast.error("Failed to load user progress");
      setUserProgressModules([]);
      return [];
    } finally {
      setLoading(false);
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

  const updateLocalProgress = (moduleId, updates = {}) => {
    setUserProgressModules((prev) => {
      const key = String(moduleId).toLowerCase();
      const idx = prev.findIndex((it) => {
        const idCandidate = it.moduleId ?? it.ModuleId ?? it.ModuleID;
        return idCandidate && String(idCandidate).toLowerCase() === key;
      });
      if (idx >= 0) {
        const copy = [...prev];
        copy[idx] = { ...copy[idx], ...updates };
        return copy;
      } else {
        const newItem = {
          moduleId,
          isUnlocked: updates.isUnlocked ?? false,
          isCompleted: updates.isCompleted ?? false,
          testStatus: updates.testStatus ?? updates.TestStatus ?? "NotTaken",
          weekId: updates.weekId ?? updates.WeekId ?? null,
        };
        return [...prev, newItem];
      }
    });
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
  const decorateModuleDetailWithProgress = (detail, moduleId) => {
    if (!detail || !moduleId) return detail;
    const pm = buildProgressMap(userProgressModules);
    const key = String(moduleId).toLowerCase();
    const prog = pm.get(key);
    const testStatus = prog?.testStatus || "NotTaken";
    const isCompleted = !!prog?.isCompleted;
    const percentCompleted = testStatus === "Passed" ? 100 : 0;
    const assessmentCompleted = testStatus === "Passed";
    const assessments = (detail.assessments || []).map((a) => ({
      ...a,
      isCompleted: assessmentCompleted,
    }));
    const lessons = (detail.lessons || []).map((ls) => ({
      ...ls,
      assessments: (ls.assessments || []).map((a) => ({
        ...a,
        isCompleted: assessmentCompleted,
      })),
    }));

    return {
      ...detail,
      isCompleted,
      progress: {
        ...(detail.progress || {}),
        testStatus,
        percentCompleted,
      },
      assessments,
      lessons,
    };
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
    setPlanDetails(null);
    setPlanDetailsVisible(false);
    setWeeks([]);
    setModules([]);
    setSelectedModule(null);
    setModuleDetail(null);
    setActiveLesson(null);
    setActiveAssessment(null);
    setUserProgressModules([]);

    try {
      setLoading(true);
      const res = await api.get(`/Week/full/plan/${p.planId}`, {
        params: { userId },
      });
      const weeksData = Array.isArray(res.data) ? res.data : [];
      const progressList = await loadUserProgressForPlan(p.planId);
      const progressMap = buildProgressMap(progressList);
      const weeksWithUnlocks = await applyWeekUnlocks(
        weeksData,
        progressMap,
        p.planId
      );
      setWeeks(weeksWithUnlocks);
      await fetchPlanDetails(p);
      setPlanDetailsVisible(false);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load weeks for plan");
    } finally {
      setLoading(false);
      setTimeout(equalizeHeights, 100);
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
    setSelectedWeek(w);
    setWeekDetails(null);
    setWeekDetailsVisible(false);
    setModules([]);
    setSelectedModule(null);
    setModuleDetail(null);
    setActiveLesson(null);
    setActiveAssessment(null);

    if (w.isUnlocked === false) {
      toast.error("This week is locked. Complete previous week to unlock.");
      return;
    }

    try {
      setLoading(true);
      const res = await api.get(`/Module/week/${w.weekId}`);
      const modulesData = Array.isArray(res.data) ? res.data : [];
      const progressList =
        userProgressModules.length
          ? userProgressModules
          : await loadUserProgressForPlan(selectedPlan?.planId);
      const progressMap = buildProgressMap(progressList);
      let merged = mergeModuleProgressToModules(modulesData, progressMap);

      if (w.isUnlocked && merged.length > 0) {
        const idxFirst = merged.findIndex(
          (mm) => (mm.orderNo ?? mm.OrderNo ?? mm.orderIndex) === 1
        );
        const firstIndex = idxFirst >= 0 ? idxFirst : 0;
        if (merged[firstIndex]) {
          merged[firstIndex] = {
            ...merged[firstIndex],
            isUnlocked: true,
            isLocked: false,
          };
        }
      }

      setModules(merged);
      await fetchWeekDetails(w);
      setWeekDetailsVisible(false);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load modules for week");
    } finally {
      setLoading(false);
      setTimeout(equalizeHeights, 100);
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

    // preserve original lesson order
    const resultLessons = (detail.lessons || []).map((ls) =>
      lessonsMap.get(String(ls.lessonId).toLowerCase())
    );
    return { ...detail, lessons: resultLessons };
  }

  const openModule = async (m) => {
    if (m.isLocked || m.isUnlocked === false) {
      toast.error("This module is locked.");
      return;
    }

    setSelectedModule(m);
    setModuleDetail(null);
    setActiveLesson(null);
    setActiveAssessment(null);
    setAnswers({});
    setTestTaken(false);
    setTestScore(null);
    setTestPassed(null);

    try {
      setLoading(true);
      const res = await api.get(`/Module/${m.moduleId}`, {
        params: { userId },
      });
      const grouped = attachResourcesAndAssessments(res.data || {});
      const decorated = decorateModuleDetailWithProgress(grouped, m.moduleId);
      setModuleDetail(decorated);

      updateLocalProgress(m.moduleId, {
        isUnlocked: true,
        weekId: m.weekId,
      });

      // scroll to module detail area
      setTimeout(() => {
        const el = document.getElementById("module-detail-inline");
        if (el) el.scrollIntoView({ behavior: "smooth", block: "start" });
      }, 120);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load module details");
    } finally {
      setLoading(false);
      setTimeout(equalizeHeights, 120);
    }
  };

  // ---------- actions ----------
  const playLesson = (lesson) => {
    if (lesson.isLocked) {
      toast.error("Lesson locked.");
      return;
    }
    setActiveLesson(lesson);
    setLessonModalOpen(true);
  };

  const markLessonCompleted = async (lesson) => {
    if (!userId) return toast.error("User not identified");
    try {
      await api.post(`/Lesson/${lesson.lessonId}/complete/${userId}`);
      toast.success("Lesson marked completed");
      await refreshProgressAndUI();
    } catch (err) {
      console.error(err);
      toast.error("Failed to mark lesson complete");
    }
  };

  const openResource = async (resource) => {
    if (!userId) return toast.error("User not identified");
    try {
      await api.post(`/Resource/${resource.resourceId}/access/${userId}`);
    } catch {}
    if (resource.url) window.open(resource.url, "_blank");
    else if (resource.content) {
      setActiveLesson({
        title: resource.title || "Resource",
        content: resource.content,
      });
      setLessonModalOpen(true);
    } else toast("No resource available");
    if (selectedModule) openModule(selectedModule);
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

  const changeAnswer = (qIndex, answer) => {
    setAnswers((prev) => ({ ...prev, [qIndex]: answer }));
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

  const handleMarkModuleComplete = async () => {
    if (!userId || !selectedModule) return toast.error("Missing context");
    if (!selectedWeek) return toast.error("Please select a week first.");
    if (!moduleDetail) return toast.error("Module details not loaded yet.");

    const allAssessments = [
      ...(moduleDetail.assessments || []),
      ...((moduleDetail.lessons || []).flatMap((ls) => ls.assessments || [])),
    ];

    const seen = new Set();
    const uniqueAssessments = allAssessments.filter((a) => {
      const id = a.assessmentId ?? a.AssessmentId;
      if (!id) return true; 
      if (seen.has(id)) return false;
      seen.add(id);
      return true;
    });

    const notCompleted = uniqueAssessments.filter((a) => !a.isCompleted);
    if (notCompleted.length > 0) {
      setIncompleteAssessments(notCompleted);
      setShowIncompleteAssessmentsModal(true);
      return;
    }
    try {
      await api.post(
        `/UserProgress/${userId}/week/${selectedWeek.weekId}/module/${selectedModule.moduleId}/complete`
      );

      toast.success("Module marked complete");

      updateLocalProgress(selectedModule.moduleId, { isCompleted: true });

      await refreshProgressAndUI();
    } catch (err) {
      console.error("Failed to mark module complete", err);
      toast.error("Failed to mark module complete");
    }
  };

  const refreshProgressAndUI = async () => {
    try {
      if (!selectedPlan) {
        await loadUserProgressForPlan(null);
        return;
      }
      const progressList = await loadUserProgressForPlan(selectedPlan.planId);
      const progressMap = buildProgressMap(progressList);
      const weeksWithUnlocks = await applyWeekUnlocks(weeks, progressMap, selectedPlan.planId);
      setWeeks(weeksWithUnlocks);

      if (selectedWeek) {
        const res = await api.get(`/Module/week/${selectedWeek.weekId}`);
        const modulesData = Array.isArray(res.data) ? res.data : [];
        let merged = mergeModuleProgressToModules(modulesData, progressMap);
        const w = weeksWithUnlocks.find((x) => x.weekId === selectedWeek.weekId);
        if (w?.isUnlocked && merged.length > 0) {
          const idxFirst = merged.findIndex(
            (mm) => (mm.orderNo ?? mm.OrderNo ?? mm.orderIndex) === 1
          );
          const firstIndex = idxFirst >= 0 ? idxFirst : 0;
          if (merged[firstIndex])
            merged[firstIndex] = {
              ...merged[firstIndex],
              isUnlocked: true,
              isLocked: false,
            };
        }
        setModules(merged);
      }

      if (selectedModule) {
        try {
          const resMod = await api.get(`/Module/${selectedModule.moduleId}`, { params: { userId } });
          const grouped = attachResourcesAndAssessments(resMod.data || {});
          const decorated = decorateModuleDetailWithProgress(grouped, selectedModule.moduleId);
          setModuleDetail(decorated);
        } catch (e) {
        }
      }
    } catch (err) {
      console.error("Failed to refresh progress/UI", err);
    } finally {
      setTimeout(equalizeHeights, 80);
    }
  };

  // ---------- small UI helpers ----------
  const SectionCard = ({ title, color, children }) => (
    <div className={`bg-gradient-to-br ${color} p-5 rounded-lg border shadow-sm`}>
      <div className="font-semibold text-sm mb-2 flex items-center gap-2">{title}</div>
      <div className="text-sm text-gray-700 whitespace-pre-wrap">{children}</div>
    </div>
  );

  const InfoCard = ({ label, value }) => (
    <div className="bg-white border rounded-lg p-6 text-center shadow">
      <div className="text-sm text-gray-500 font-semibold">{label}</div>
      <div className="text-2xl font-bold text-[#0A2342] mt-1">{value}</div>
    </div>
  );

  const InfoField = ({ label, value }) => (
    <div>
      <div className="text-sm text-gray-500 font-semibold">{label}</div>
      <div className="text-sm text-gray-700 mt-1 whitespace-pre-wrap">{value || "—"}</div>
    </div>
  );

  // --------------------- UI COMPONENTS ---------------------
  const PlansBar = ({ plans, onSelect, selected }) => {
    return (
      <section className="rounded-xl overflow-hidden shadow-lg">
        <div className="bg-gradient-to-r from-[#0A2342] to-[#123A63] px-6 py-6 text-white flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold tracking-tight">Learning Plans</h2>
            <p className="text-sm opacity-90 mt-1">Choose a plan to begin your learning journey</p>
          </div>
          <button
            onClick={() => loadPlans()}
            className="bg-[#F7D57A] hover:bg-[#ffeaa7] text-[#0A2342] px-4 py-2 rounded-lg font-semibold shadow"
          >
            Refresh
          </button>
        </div>

        <div className="bg-white p-4 grid grid-cols-1 sm:grid-cols-3 gap-3">
          {plans.map((p) => (
            <motion.div
              key={p.planId}
              whileHover={{ scale: 1.01 }}
              className={`group p-4 rounded-lg flex flex-col justify-between text-left shadow-sm transition ${
                selected?.planId === p.planId ? "ring-4 ring-[#F7D57A]/60 bg-[#FFF7E4]" : "bg-white"
              }`}
            >
              <div>
                <div className="text-sm text-gray-500">{p.category || "Program"}</div>
                <div className="mt-2 font-semibold text-lg text-[#0A23442]">{p.title}</div>
                <div className="text-xs text-gray-500 mt-1 line-clamp-2">{p.description}</div>
              </div>

              <div className="mt-4 flex items-center justify-between">
                <button onClick={() => onSelect(p)} className="text-sm font-semibold text-[#123A63]">
                  Open →
                </button>

                <button
                  onClick={async () => {
                    if (selected?.planId !== p.planId) {
                      await onSelect(p);
                      setPlanDetailsVisible(true);
                    } else {
                      setPlanDetailsVisible((v) => !v);
                    }
                  }}
                  className="ml-2 px-2 py-1 text-sm rounded-md bg-[#0A2342] text-white"
                >
                  Details
                </button>
              </div>
            </motion.div>
          ))}
          {!plans.length && <div className="text-sm text-gray-400 col-span-full">No plans available</div>}
        </div>

        {planDetailsVisible && planDetails && (
          <div className="mt-6 animate-fadeIn">
            <PlanDetailsPanel details={planDetails} onClose={() => setPlanDetailsVisible(false)} />
          </div>
        )}
      </section>
    );
  };

  const PlanDetailsPanel = ({ details, onClose }) => {
    return (
      <div className="bg-white border shadow-lg rounded-xl p-8 space-y-8">
        <div className="flex justify-between items-start border-b pb-4">
          <div>
            <h3 className="text-2xl font-extrabold text-[#0A2342] flex items-center gap-2">📘 {details.title}</h3>
            <p className="text-sm text-gray-600 mt-1">{details.description}</p>
          </div>
          <button onClick={onClose} className="px-4 py-2 rounded-lg bg-gray-100 hover:bg-gray-200 text-sm font-medium">
            ✖ Close
          </button>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 auto-rows-max">
          <SectionCard title="📄 Overview" color="from-blue-50 to-blue-100">
            {details.overview || "—"}
          </SectionCard>
          <SectionCard title="🎯 Objectives" color="from-yellow-50 to-yellow-100">
            {details.objectives || "—"}
          </SectionCard>
          <SectionCard title="🧩 Prerequisites" color="from-purple-50 to-purple-100">
            {details.prerequisites || "—"}
          </SectionCard>
          <SectionCard title="🛠 Technical Requirements" color="from-green-50 to-green-100">
            {details.technicalRequirements || "—"}
          </SectionCard>

          <div className="col-span-full">
            <SectionCard title="📝 Self-Assessment Checklist" color="from-pink-50 to-pink-100">
              {details.selfAssessmentChecklist || "—"}
            </SectionCard>
          </div>

          <InfoCard label="⏳ Duration" value={`${details.durationWeeks ?? "—"} Weeks`} />
          <InfoCard label="📅 Total Days" value={`${details.totalDays ?? "—"} Days`} />
        </div>

        {Array.isArray(details.weeks) && details.weeks.length > 0 && (
          <div>
            <div className="text-lg font-bold text-gray-800 flex items-center gap-2 mb-3">📚 Weeks Overview</div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 auto-rows-max">
              {details.weeks.map((w) => (
                <div key={w.weekId} className="p-4 rounded-lg border bg-white shadow-sm">
                  <div className="font-bold text-[#0A2342] flex items-center gap-2">🗂 {w.title}</div>
                  <div className="text-xs text-gray-500">Week {w.weekNumber}</div>
                  <div className="text-sm text-gray-700 mt-2">{w.overview}</div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    );
  };

  // Weeks panel - left
  const WeeksPanel = ({ weeks, onSelect, selected }) => {
    return (
      <section ref={weeksPanelRef} className="bg-white rounded-xl shadow-md p-4 h-full flex flex-col">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h3 className="text-lg font-bold text-[#1E40AF]">Weeks</h3>
            <p className="text-sm text-gray-500">Pick a week inside the selected plan</p>
          </div>
        </div>

        <div className="flex-1 overflow-hidden">
          <div className="overflow-auto scrollbar-none space-y-2 pr-2 max-h-full">
            {weeks.map((w) => (
              <div
                key={w.weekId}
                onClick={() => {
                  if (!w.isUnlocked) {
                    toast.error("This week is locked. Complete previous week to unlock.");
                    return;
                  }
                  onSelect(w);
                }}
                className={`cursor-pointer p-3 rounded-md transition flex justify-between items-center border ${
                  selected?.weekId === w.weekId ? "bg-[#EFF6FF] border-[#A9C1E8]" : "bg-white border-gray-100"
                }`}
              >
                <div>
                  <div className="font-medium text-[#1E40AF] flex items-center gap-2">
                    {w.isUnlocked ? <UnlockKeyhole className="text-green-600" size={16} /> : <Lock className="text-gray-400" size={16} />}
                    {w.title}
                  </div>
                  <div className="text-xs text-gray-500">Week {w.weekNumber || "—"}</div>
                </div>

                <div className="flex items-center gap-2">
                  <div className="text-xs text-gray-400">{w.modulesCount ? `${w.modulesCount} modules` : "-"}</div>

                  <button
                    onClick={async (e) => {
                      e.stopPropagation();
                      if (!w.isUnlocked) {
                        toast.error("This week is locked. Complete previous week to unlock.");
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
                    className="ml-2 px-2 py-1 text-sm rounded-md bg-[#0A2342] text-white"
                  >
                    View
                  </button>
                </div>
              </div>
            ))}
            {!weeks.length && <div className="text-sm text-gray-400">No weeks</div>}
          </div>
        </div>
      </section>
    );
  };
  const ModulesGrid = ({ modules, onOpen }) => {
    return (
      <section ref={modulesPanelRef} className="rounded-xl p-0 h-full flex flex-col">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-bold text-[#AC4F00]">Modules</h3>
          <div className="text-sm text-gray-500">Explore modules in this week</div>
        </div>

        <div className="overflow-auto scrollbar-none" style={{ maxHeight: "calc(100% - 64px)" }}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {modules.map((m, idx) => (
              <div key={m.moduleId} className="bg-gradient-to-br from-[#FFF1D6] to-[#FFEBD6] rounded-xl p-4 shadow-sm border border-[#FFDFC2]">
                <div className="flex justify-between items-start">
                  <div>
                    <div className="text-sm text-[#AC4F00] font-semibold">Module {idx + 1}</div>
                    <div className="mt-1 font-bold text-xl text-[#6B2F00]">{m.moduleName}</div>
                    <div className="text-sm text-gray-600 mt-2 line-clamp-2">{m.description}</div>
                    {m._progress && (
                      <div className="mt-2 text-xs text-gray-600">
                        Progress: <span className="font-semibold">{m._progress.percentCompleted ?? 0}%</span>
                      </div>
                    )}
                  </div>

                  <div className="flex flex-col items-end gap-2">
                    {m.isLocked || m.isUnlocked === false ? (
                      <div className="px-2 py-1 rounded-md bg-white/60 text-gray-600 text-xs flex items-center gap-1">
                        <Lock size={14} /> Locked
                      </div>
                    ) : (
                      <>
                        <button onClick={() => onOpen(m)} className="px-3 py-2 rounded-md bg-[#6B2F00] text-white font-semibold shadow">
                          Open
                        </button>
                        <button onClick={() => onOpen(m)} className="text-xs underline text-[#6B2F00]">
                          Details
                        </button>
                        {m.isCompleted && (
                          <div className="px-2 py-1 rounded-md bg-green-100 text-green-700 text-xs flex items-center gap-1">
                            <CheckCircle2 size={14} /> Completed
                          </div>
                        )}
                      </>
                    )}
                    <div className="text-xs text-gray-500 mt-1">{m.duration ? `${m.duration} mins` : ""}</div>
                  </div>
                </div>
              </div>
            ))}
            {!modules.length && <div className="text-sm text-gray-400 col-span-full">No modules in this week</div>}
          </div>
        </div>
      </section>
    );
  };

  const ModuleDetailInline = ({ detail, userId }) => {
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

    if (!detail) return null;

    // ------------------ FIXED PROGRESS CALCULATION ------------------
    const allLessons = detail.lessons || [];
    const allAssessments = allLessons.flatMap((x) => x.assessments || []);

    const completedLessons = allLessons.filter((l) => l.isCompleted).length;
    const completedAssessments = allAssessments.filter((a) => a.latestResult?.passed).length;

    const totalLessons = allLessons.length;
    const totalAssessments = allAssessments.length;

    const percent = Math.round(
      ((completedLessons + completedAssessments) / (totalLessons + totalAssessments || 1)) * 100
    );

    // ============================= HELPERS =============================
    const ensureLatestResultLoaded = async (assessment) => {
      if (assessment.latestResult) return assessment.latestResult;

      const fetched = await getAssessmentResultApi(assessment.assessmentId, userId);

      if (fetched) assessment.latestResult = fetched;

      return fetched;
    };

    // ---------------- START TEST ----------------
    const startAssessmentLocal = async (assessment) => {
      // Fetch latest attempt (if exists)
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

    // ---------------- REVIEW MODE ----------------
    const reviewAssessmentLocal = async (assessment) => {
      const result = await ensureLatestResultLoaded(assessment);

      if (!result) {
        startAssessmentLocal(assessment);
        return;
      }

      setAssessmentReviewMode(true);
      setActiveAssessmentLocal(assessment);
      setAssessmentModalOpenLocal(true);

      setAssessmentResult(result);
      setTestTakenLocal(true);
      setTestScoreLocal(result.scorePercentage ?? null);
      setTestPassedLocal(!!result.passed);
      try {
        const parsed = JSON.parse(result.userAnswers || "{}");
        const answerMap = {};
        (assessment.questions || []).forEach((q, qi) => {
          answerMap[qi] = parsed[q.questionId] || "";
        });
        setAnswersLocal(answerMap);
      } catch {
        setAnswersLocal({});
      }
    };

    // ---------------- SUBMIT TEST ----------------
    const submitAssessmentLocal = async () => {
      if (!activeAssessmentLocal) return;

      const userAnswersObj = {};
      (activeAssessmentLocal.questions || []).forEach((q, qi) => {
        if (answersLocal[qi]) userAnswersObj[q.questionId] = answersLocal[qi];
      });

      const payload = {
        userId,
        assessmentId: activeAssessmentLocal.assessmentId,
        weekId: detail.weekId,
        userAnswers: JSON.stringify(userAnswersObj),
      };

      const result = await submitAssessmentApi(payload);
      if (!result) {
        setAssessmentResult(null);
        setTestTakenLocal(true);
        setTestScoreLocal(null);
        setTestPassedLocal(false);
        toast.error("Failed to submit test or no response from server");
      } else {
        activeAssessmentLocal.latestResult = result;

        setAssessmentResult(result);
        setTestTakenLocal(true);
        setTestScoreLocal(result.scorePercentage ?? null);
        setTestPassedLocal(!!result.passed);
        if (detail.lessons) {
          detail.lessons = detail.lessons.map((ls) => ({
            ...ls,
            assessments: ls.assessments.map((a) =>
              a.assessmentId === activeAssessmentLocal.assessmentId
                ? { ...a, latestResult: result, isCompleted: !!result.passed }
                : a
            ),
          }));
        }
      }
    };

    // ---------------- MODULE COMPLETION FIXED ----------------
    const handleMarkModuleCompleteLocal = async () => {
      const updatedAssessments = [];
      for (const a of allAssessments) {
        const res = await ensureLatestResultLoaded(a);
        if (!res?.passed) updatedAssessments.push(a);
      }

      if (updatedAssessments.length > 0) {
        setIncompleteAssessmentsLocal(updatedAssessments);
        setShowIncompleteAssessmentsModalLocal(true);
        return;
      }

      try {
        await fetch(
          `/api/userprogress/${userId}/week/${detail.weekId}/module/${detail.moduleId}/complete`,
          { method: "POST" }
        );
        detail.isCompleted = true;
        toast.success("Module marked complete");
      } catch (err) {
        console.error("Failed to mark module complete (inline):", err);
        toast.error("Failed to mark module complete");
      }
    };

    // ---------------- RENDER ----------------
    return (
      <div className="bg-white rounded-xl shadow-md p-6 border border-gray-100 mt-6" id="module-detail-inline">
        {/* ---------------- HEADER ---------------- */}
        <div className="flex justify-between items-start">
          <div>
            <h3 className="text-2xl font-bold text-[#3A3A3A]">{detail.moduleName}</h3>
            <p className="text-sm text-gray-600">{detail.description}</p>

            {/* Progress UI */}
            <div className="mt-4 text-sm space-y-2">
              <div className="flex justify-between">
                <span>Lessons: {completedLessons}/{totalLessons}</span>
                <span>Tests: {completedAssessments}/{totalAssessments}</span>
                <span className="font-bold text-blue-700">{percent}%</span>
              </div>

              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="h-2 rounded-full bg-gradient-to-r from-blue-500 to-green-500" style={{ width: `${percent}%` }} />
              </div>
            </div>
          </div>

          {!detail.isCompleted ? (
            <button onClick={handleMarkModuleCompleteLocal} className="px-4 py-2 bg-[#0A2342] text-white rounded-md">
              Mark Complete
            </button>
          ) : (
            <div className="text-green-600 font-semibold">Completed ✔</div>
          )}
        </div>

        {/* ----------------- LESSONS LIST ----------------- */}
        {!activeLessonLocal && (
          <div className="mt-6 space-y-4">
            {detail.lessons.map((ls, idx) => (
              <div key={ls.lessonId} className="p-4 border rounded bg-gray-50 flex justify-between">
                <div>
                  <h4 className="font-bold">{idx + 1}. {ls.title}</h4>
                  <p className="text-xs text-gray-500">{ls.overview}</p>
                </div>
                <button onClick={() => setActiveLessonLocal(ls)} className="px-3 py-1 bg-blue-700 text-white rounded-md">
                  View
                </button>
              </div>
            ))}
          </div>
        )}

        {/* ----------------- LESSON DETAIL ----------------- */}
        {activeLessonLocal && (
          <div className="mt-6">
            <button onClick={() => setActiveLessonLocal(null)} className="mb-4 px-4 py-1 bg-gray-200 rounded-md">← Back</button>

            <h3 className="text-xl font-bold">{activeLessonLocal.title}</h3>
            <p className="text-sm text-gray-600">{activeLessonLocal.overview}</p>

            {/* CONTENT */}
            <div className="mt-4">
              {activeLessonLocal.content?.includes("youtube") ? (
                <iframe
                  className="w-full h-96 rounded"
                  src={`https://www.youtube.com/embed/${(activeLessonLocal.content.match(/(?:v=|\/)([0-9A-Za-z_-]{11})/) || [])[1]}`}
                  allowFullScreen
                />
              ) : (
                <div className="prose max-w-full" dangerouslySetInnerHTML={{ __html: activeLessonLocal.content ?? "<i>No content</i>" }} />
              )}
            </div>

            {/* RESOURCES */}
            <div className="mt-8">
              <h4 className="font-semibold">Resources</h4>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-3">
                {(activeLessonLocal.resources || []).map((r) => <div key={r.resourceId}>{renderResourcePreview(r)}</div>)}
              </div>
            </div>

            {/* ASSESSMENTS */}
            <div className="mt-10">
              <h4 className="font-semibold">Assessments</h4>

              {(activeLessonLocal.assessments || []).map((a) => {
                const r = a.latestResult;
                return (
                  <div key={a.assessmentId} className="p-4 border rounded bg-white flex justify-between items-center mt-4">
                    <div>
                      <h5 className="font-semibold flex gap-2 items-center">
                        {a.title}
                        {r && (
                          <span className={`px-2 text-xs rounded-full ${r.passed ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}`}>
                            {r.passed ? "Passed" : "Failed"} • {((r.scorePercentage ?? 0)).toFixed(0)}%
                          </span>
                        )}
                      </h5>
                      {!r && <p className="text-xs text-gray-500">Not taken</p>}
                    </div>

                    {/* BUTTONS */}
                    {!r && (
                      <button onClick={() => startAssessmentLocal(a)} className="px-3 py-1 bg-blue-600 text-white rounded-md">
                        Take Test
                      </button>
                    )}

                    {r && r.passed && (
                      <button onClick={() => reviewAssessmentLocal(a)} className="px-3 py-1 border border-blue-600 text-blue-600 rounded-md">
                        Review
                      </button>
                    )}

                    {r && !r.passed && (
                      <button onClick={() => startAssessmentLocal(a)} className="px-3 py-1 bg-orange-500 text-white rounded-md">
                        Retake
                      </button>
                    )}
                  </div>
                );
              })}
            </div>
          </div>
        )}

        {/* ---------------- INCOMPLETE ASSESSMENTS MODAL ---------------- */}
        {showIncompleteAssessmentsModalLocal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50" onClick={() => setShowIncompleteAssessmentsModalLocal(false)} />
            <div className="relative bg-white rounded-xl shadow-2xl p-6 max-w-lg w-full">
              <h3 className="text-xl font-bold text-red-600">Complete Required Tests</h3>
              <p className="text-sm mt-2">You must pass all assessments before marking the module as completed.</p>

              <div className="mt-4 space-y-3 max-h-[50vh] overflow-auto">
                {incompleteAssessmentsLocal.map((a) => (
                  <div key={a.assessmentId} className="p-3 border rounded bg-red-50 flex justify-between">
                    <div>
                      <div className="font-semibold">{a.title}</div>
                    </div>
                    <button
                      onClick={() => {
                        setShowIncompleteAssessmentsModalLocal(false);
                        startAssessmentLocal(a);
                      }}
                      className="px-3 py-1 bg-blue-600 text-white rounded-md"
                    >
                      Take Test
                    </button>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}

        {/* ---------------- ASSESSMENT MODAL (inline) ---------------- */}
        {assessmentModalOpenLocal && activeAssessmentLocal && (
          <div className="fixed inset-0 z-50 flex justify-center items-center">
            <div className="absolute inset-0 bg-black/50" onClick={() => setAssessmentModalOpenLocal(false)} />

            <div className="relative bg-white rounded-xl shadow-2xl p-6 max-w-3xl w-full">
              <div className="flex justify-between">
                <div>
                  <h3 className="text-lg font-semibold">{activeAssessmentLocal.title}</h3>

                  {assessmentReviewMode && assessmentResult && (
                    <p className="text-xs text-gray-600 mt-1">
                      Score: {(assessmentResult.scorePercentage ?? 0).toFixed(0)}% —{" "}
                      {assessmentResult.passed ? <span className="text-green-600">Passed</span> : <span className="text-red-600">Failed</span>}
                    </p>
                  )}
                </div>

                <button onClick={() => setAssessmentModalOpenLocal(false)} className="px-3 py-1 bg-gray-200 rounded-md">
                  Close
                </button>
              </div>

              <div className="mt-4 max-h-[60vh] overflow-y-auto space-y-4">
                {(activeAssessmentLocal.questions || []).map((q, qi) => {
                  const options = JSON.parse(q.options || "[]");
                  const userAnswer = answersLocal[qi];
                  const correct = q.correctAnswer;

                  return (
                    <div key={qi} className="p-3 border rounded-md">
                      <p className="font-medium">{qi + 1}. {q.question}</p>

                      {options.map((opt, oi) => {
                        const isSelected = userAnswer === opt;
                        const isCorrect = opt === correct;

                        return (
                          <label key={oi} className={`flex items-center gap-2 text-sm ${assessmentReviewMode ? (isCorrect ? "text-green-600 font-semibold" : isSelected && !isCorrect ? "text-red-600" : "") : ""}`}>
                            <input
                              type="radio"
                              checked={isSelected}
                              disabled={assessmentReviewMode}
                              onChange={() => {
                                if (!assessmentReviewMode) {
                                  setAnswersLocal((prev) => ({ ...prev, [qi]: opt }));
                                }
                              }}
                            />
                            {opt}
                          </label>
                        );
                      })}

                      {assessmentReviewMode && (
                        <div className="border-t mt-2 pt-2 text-xs text-gray-700">
                          <p><strong>Correct Answer:</strong> {correct}</p>
                          <p><strong>Your Answer:</strong> {userAnswer || "Not answered"}</p>
                          {q.explanation && <p className="mt-1 text-gray-600"><strong>Explanation:</strong> {q.explanation}</p>}
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>

              {!assessmentReviewMode && (
                <div className="mt-4 flex justify-end gap-3">
                  <button className="px-4 py-2 bg-gray-200 rounded-md" onClick={() => setAssessmentModalOpenLocal(false)}>Cancel</button>
                  <button className="px-4 py-2 bg-blue-600 text-white rounded-md" onClick={submitAssessmentLocal}>Submit</button>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    );
  };

  // ================== RESOURCE PREVIEW (INLINE) ==================
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

  // ================== FULLSCREEN VIEWER ==================
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
      <div className="max-w-7xl mx-auto p-6 space-y-8">
        <motion.div initial={{ opacity: 0, y: -8 }} animate={{ opacity: 1, y: 0 }} className="text-center">
          <h1 className="text-3xl font-extrabold text-[#123A63]">KnowledgeQuest — Learning Platform</h1>
          <p className="mt-2 text-gray-600">A modern learning experience — plans, weeks, modules, lessons and assessments.</p>
        </motion.div>

        {/* PLANS */}
        <PlansBar plans={plans} onSelect={selectPlan} selected={selectedPlan} />

        {/* WEEKS + MODULES GRID */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* LEFT — WEEKS */}
          <div className="lg:col-span-1">
            <WeeksPanel weeks={weeks} onSelect={selectWeek} selected={selectedWeek} />
          </div>

          {/* RIGHT — MODULES */}
          <div className="lg:col-span-2 space-y-6">
            <ModulesGrid modules={modules} onOpen={openModule} />
          </div>
        </div>

        {/* FULL MODULE DETAILS */}
        {selectedModule && moduleDetail && (
          <div className="col-span-full">
            <ModuleDetailInline detail={moduleDetail} userId={userId} />
          </div>
        )}

        {/* INLINE LESSON VIEWER (top-level activeLesson, if you still use it) */}
        {activeLesson && (
          <div className="bg-white rounded-xl shadow-md p-6 border mt-6">
            <div className="flex justify-between items-start">
              <div>
                <h3 className="text-2xl font-bold text-[#123A63]">{activeLesson.title}</h3>
                <p className="text-sm text-gray-600 mt-1">{activeLesson.overview}</p>
              </div>

              <button onClick={() => setActiveLesson(null)} className="px-3 py-1 rounded-md bg-gray-200">Close</button>
            </div>

            {/* LESSON CONTENT */}
            <div className="mt-4 prose max-w-full">
              {activeLesson.content ? (
                activeLesson.content.includes("youtube") || activeLesson.content.includes("youtu.be") ? (
                  <iframe className="w-full h-96 rounded" src={`https://www.youtube.com/embed/${(activeLesson.content.match(/(?:v=|\/)([0-9A-Za-z_-]{11})/) || [])[1] || ""}`} allowFullScreen />
                ) : (
                  <div dangerouslySetInnerHTML={{ __html: activeLesson.content }} />
                )
              ) : (
                <div className="text-sm text-gray-500">No content</div>
              )}
            </div>

            {/* RESOURCES (GRID PREVIEW) */}
            <div className="mt-8">
              <h4 className="font-semibold text-gray-800 mb-2">Resources</h4>

              {!activeLesson.resources?.length && <div className="text-sm text-gray-500">No resources</div>}

              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                {(activeLesson.resources || []).map((r) => (
                  <div key={r.resourceId}>{renderResourcePreview(r)}</div>
                ))}
              </div>
            </div>

            {/* ASSESSMENTS */}
            <div className="mt-8">
              <h4 className="font-semibold text-gray-800 mb-2">Assessments</h4>

              {!activeLesson.assessments?.length && <div className="text-sm text-gray-500">No assessments</div>}

              <div className="space-y-3">
                {(activeLesson.assessments || []).map((a) => (
                  <div key={a.assessmentId} className="p-3 border rounded bg-white flex justify-between">
                    <div>
                      <div className="font-medium">{a.title}</div>
                      <div className="text-xs text-gray-500">{a.description}</div>
                    </div>

                    <button onClick={() => startAssessment(a)} className="px-3 py-1 bg-blue-600 text-white rounded-md">Take Test</button>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>

      {/* FULLSCREEN RESOURCE OVERLAY */}
      {fullScreenResource && (
        <div className="fixed inset-0 z-[100] bg-black/80 flex flex-col">
          <div className="flex justify-between items-center px-4 py-3 bg-black/60 text-white">
            <div className="text-sm truncate">{fullScreenResource.title || fullScreenResource.url}</div>

            <button onClick={() => setFullScreenResource(null)} className="px-3 py-1 bg-white/10 hover:bg-white/20 rounded-md text-xs">Close</button>
          </div>

          <div className="flex-1 flex items-center justify-center p-4">{renderFullScreenResource(fullScreenResource)}</div>
        </div>
      )}

      {/* INCOMPLETE ASSESSMENTS MODAL */}
      {showIncompleteAssessmentsModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <div className="absolute inset-0 bg-black/50" onClick={() => setShowIncompleteAssessmentsModal(false)} />
          <div className="relative max-w-xl w-full mx-4 bg-white rounded-xl shadow-2xl p-6">
            <h3 className="text-xl font-bold text-red-600">Complete Required Tests</h3>
            <p className="text-sm text-gray-600 mt-1">You must complete all assessments before marking this module complete.</p>

            <div className="mt-4 space-y-3 max-h-[50vh] overflow-auto">
              {incompleteAssessments.map((a) => (
                <div key={a.assessmentId} className="p-3 border rounded-md bg-red-50 flex justify-between items-center">
                  <div>
                    <div className="font-semibold">{a.title}</div>
                    <div className="text-xs text-gray-600">{a.description}</div>
                  </div>

                  <button onClick={() => { setShowIncompleteAssessmentsModal(false); startAssessment(a); }} className="px-3 py-1 bg-blue-600 text-white rounded-md text-sm">Take Test</button>
                </div>
              ))}
            </div>

            <div className="mt-4 flex justify-end">
              <button onClick={() => setShowIncompleteAssessmentsModal(false)} className="px-4 py-2 bg-gray-200 rounded-md">Close</button>
            </div>
          </div>
        </div>
      )}

      {/* ASSESSMENT MODAL (parent fallback) */}
      {assessmentModalOpen && activeAssessment && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <div className="absolute inset-0 bg-black/50" onClick={() => setAssessmentModalOpen(false)} />
          <div className="relative max-w-3xl w-full mx-4 bg-white rounded-xl shadow-2xl p-6">
            <div className="flex justify-between items-start">
              <div>
                <h3 className="text-lg font-semibold">{activeAssessment.title}</h3>
                <p className="text-sm text-gray-500 mt-1">{activeAssessment.description}</p>
              </div>

              <button onClick={() => setAssessmentModalOpen(false)} className="px-3 py-1 rounded-md bg-gray-100">Close</button>
            </div>

            <div className="mt-4 space-y-4 max-h-[60vh] overflow-auto pr-2">
              {(activeAssessment.questions || []).map((q, qi) => (
                <div key={qi} className="p-3 border rounded-md">
                  <div className="font-medium">{qi + 1}. {q.question}</div>
                  <div className="mt-2 space-y-2">
                    {parseOptions(q.options).map((opt, oi) => (
                      <label key={oi} className="block cursor-pointer">
                        <input type="radio" name={`q-${qi}`} value={opt} checked={answers[qi] === opt} onChange={() => changeAnswer(qi, opt)} className="mr-2" />
                        {opt}
                      </label>
                    ))}
                  </div>

                  {testTaken && q.explanation && <div className="mt-2 text-sm text-gray-600">Explanation: {q.explanation}</div>}
                </div>
              ))}
            </div>

            <div className="mt-4 flex justify-between items-center">
              <div>
                {testTaken && (
                  <div className="text-sm">Score: <strong>{testScore?.toFixed(0)}%</strong> — {testPassed ? (<span className="text-green-600">Passed ✅</span>) : (<span className="text-red-500">Failed ❌</span>)}</div>
                )}
              </div>

              <div className="flex gap-3">
                <button onClick={() => setAssessmentModalOpen(false)} className="px-4 py-2 bg-gray-200 rounded-md">Cancel</button>
                <button onClick={async () => {
                  if (!activeAssessment) return;
                  const userAnswersObj = {};
                  (activeAssessment.questions || []).forEach((q, qi) => {
                    if (answers[qi]) userAnswersObj[q.questionId] = answers[qi];
                  });
                  const payload = {
                    userId,
                    assessmentId: activeAssessment.assessmentId,
                    weekId: moduleDetail?.weekId,
                    userAnswers: JSON.stringify(userAnswersObj),
                  };
                  const result = await submitAssessmentApi(payload);
                  if (!result) {
                    toast.error("Failed to submit test or no response from server");
                    setTestTaken(true);
                    setTestScore(null);
                    setTestPassed(false);
                  } else {
                    activeAssessment.latestResult = result;
                    setTestTaken(true);
                    setTestScore(result.scorePercentage ?? null);
                    setTestPassed(!!result.passed);
                    if (moduleDetail?.lessons) {
                      moduleDetail.lessons = moduleDetail.lessons.map(ls => ({
                        ...ls,
                        assessments: ls.assessments.map(a => a.assessmentId === activeAssessment.assessmentId ? { ...a, latestResult: result } : a)
                      }));
                    }
                  }
                }} className="px-4 py-2 bg-blue-600 text-white rounded-md">Submit</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
