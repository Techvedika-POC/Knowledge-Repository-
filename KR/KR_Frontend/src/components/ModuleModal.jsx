import React, { useEffect, useMemo, useState } from "react";
import toast from "react-hot-toast";
import api from "../api";
import {
  ChevronLeft,
  PlusCircle,
  Trash,
  Loader2,
  Save,
  ArrowLeftCircle,
} from "lucide-react";

export default function ModuleModal({
  moduleData,
  weekId,
  onClose,
  onModuleSaved,
  mode = "edit",
}) {
  const isViewOnly = mode === "view";
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState("info");
  const [moduleName, setModuleName] = useState("");
  const [description, setDescription] = useState("");
  const [orderNo, setOrderNo] = useState(1);
  const [durationDays, setDurationDays] = useState(0);
  const [overview, setOverview] = useState("");
  const [prerequisites, setPrerequisites] = useState("");
  const [localModuleId, setLocalModuleId] = useState(moduleData?.moduleId ?? null);
  const [lessons, setLessons] = useState([]);
  const [selectedLessonIndex, setSelectedLessonIndex] = useState(null);
  const [lessonSubTab, setLessonSubTab] = useState("resources");
  const [savingModule, setSavingModule] = useState(false);
  const [savingLesson, setSavingLesson] = useState(false);

  useEffect(() => {
    setLocalModuleId(moduleData?.moduleId ?? null);

    if (!moduleData?.moduleId) {
      setModuleName("");
      setDescription("");
      setOrderNo(1);
      setDurationDays(0);
      setOverview("");
      setPrerequisites("");
      setLessons([]);
      setSelectedLessonIndex(null);
      setActiveTab("info");
      return;
    }
    const load = async () => {
      setLoading(true);
      try {
        const params = {};
        if (moduleData.userId) params.userId = moduleData.userId;
        const res = await api.get(`/Module/${moduleData.moduleId}`, { params });
        const m = res.data || {};

        setModuleName(m.moduleName || "");
        setDescription(m.description || m.extendedDescription || "");
        setOrderNo(m.orderNo ?? 1);
        setDurationDays(m.durationDays ?? 0);
        setOverview(m.overview ?? "");
        setPrerequisites(m.prerequisites ?? "");
        const normalizedLessons = (m.lessons || []).map((l, idx) => ({
          lessonId: l.lessonId ?? null,
          title: l.title ?? "",
          content: l.content ?? "",
          lessonType: l.lessonType ?? "Text",
          orderIndex: l.orderIndex ?? idx + 1,
          overview: l.overview ?? "",
          prerequisites: l.prerequisites ?? "",
          durationMinutes: l.durationMinutes ?? 0,
          resources: [],
          assessments: [],
        }));
        const lessonsById = {};
        normalizedLessons.forEach((ls) => {
          if (ls.lessonId) lessonsById[ls.lessonId.toString()] = ls;
        });
        (m.resources || []).forEach((r) => {
          const topicKey = r.topicId ? String(r.topicId) : null;
          const targetLesson = topicKey ? lessonsById[topicKey] : null;

          const resourceObject = {
            resourceId: r.resourceId ?? null,
            title: r.title ?? "",
            url: r.url ?? "",
            resourceType: r.resourceType ?? "Link",
            description: r.description ?? "",
            metadata: r.metadata ?? "{}",
            isAiGenerated: r.isAiGenerated ?? false,
            topicId: r.topicId ?? null,
            moduleId: r.moduleId ?? null,
            createdOn: r.createdOn ?? null,
            updatedOn: r.updatedOn ?? null
          };

          if (targetLesson) targetLesson.resources.push(resourceObject);
        });


        (m.assessments || []).forEach((a) => {
          const t = a.topicId ? String(a.topicId) : null;
          const target = t ? lessonsById[t] : null;
          const normQuestions = (a.questions || []).map((q) => ({
            questionId: q.questionId ?? null,
            question: q.question ?? "",
            options: parseOptionsToArray(q.options),
            correctAnswer: q.correctAnswer ?? "",
            explanation: q.explanation ?? "",
            hint: q.hint ?? "",
            questionType: q.questionType ?? "multiple-choice",
            metadata: q.metadata ?? "{}",
            isAiGenerated: q.isAiGenerated ?? false,
          }));
          const ao = {
            assessmentId: a.assessmentId ?? null,
            title: a.title ?? "",
            description: a.description ?? "",
            difficulty: a.difficulty ?? 1,
            metadata: a.metadata ?? "{}",
            learningObjectives: a.learningObjectives ?? "",
            estimatedDurationMinutes: a.estimatedDurationMinutes ?? 0,
            questions: normQuestions,
          };
          if (target) target.assessments.push(ao);
        });

        setLessons(normalizedLessons.length ? normalizedLessons : []);
        setSelectedLessonIndex(normalizedLessons.length ? 0 : null);
      } catch (err) {
        console.error("load module", err);
        toast.error("Failed to load module details");
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [moduleData]);
  function parseOptionsToArray(options) {
    if (!options) return [];
    if (Array.isArray(options)) return options;
    if (typeof options === "string") {
      try {
        const parsed = JSON.parse(options);
        if (Array.isArray(parsed)) return parsed;
      } catch { }
      return options.split(",").map((s) => s.trim()).filter(Boolean);
    }
    return [];
  }

  function createEmptyLesson(index = 1) {
    return {
      lessonId: null,
      title: "",
      content: "",
      lessonType: "Text",
      orderIndex: index,
      overview: "",
      prerequisites: "",
      durationMinutes: 0,
      resources: [],
      assessments: [],
    };
  }
  function detectResourceType(filename) {
    const ext = filename.split(".").pop().toLowerCase();

    if (["mp4", "mov"].includes(ext)) return "video";
    if (ext === "pdf") return "pdf";
    if (ext === "doc" || ext === "docx") return "docx";
    if (ext === "xls" || ext === "xlsx") return "xlsx";
    if (ext === "ppt" || ext === "pptx") return "pptx";
    if (["jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext)) return "image";

    return "file";
  }

  // ---------- Module Info Save ----------
  const saveModuleInfo = async () => {
    if (isViewOnly) return;
    if (!moduleName.trim()) return toast.error("Module name is required");
    setSavingModule(true);
    try {
      let saved;
      if (localModuleId) {
        // update
        await api.put(`/Module/${localModuleId}`, {
          moduleName,
          description,
          orderNo,
          durationDays,
          overview,
          prerequisites,
        });
        saved = { ...(moduleData || {}), moduleId: localModuleId, moduleName, description, orderNo };
        toast.success("Module updated");
      } else {
        // create
        const res = await api.post(`/Module/${weekId}`, {
          moduleName,
          description,
          orderNo,
          durationDays,
          overview,
          prerequisites,
        });
        saved = res.data;
        toast.success("Module created");
        if (saved?.moduleId) setLocalModuleId(saved.moduleId);
      }
      onModuleSaved?.(saved);
      if (saved?.moduleId) setActiveTab("lessons");
    } catch (err) {
      console.error("save module", err);
      toast.error("Failed to save module info");
    } finally {
      setSavingModule(false);
    }
  };

  // ---------- Lesson CRUD ----------
  const addLessonLocal = () => {
    setLessons((prev) => {
      const next = [...prev, createEmptyLesson(prev.length + 1)];
      setSelectedLessonIndex(next.length - 1);
      return next;
    });
  };

  const updateLessonLocal = (index, patch) => {
    setLessons((prev) => {
      const copy = [...prev];
      copy[index] = { ...copy[index], ...patch };
      return copy;
    });
  };

  const removeLessonLocal = async (index) => {
    if (!window.confirm("Delete this lesson?")) return;
    const lesson = lessons[index];

    try {
      if (lesson.lessonId) {
        await api.delete(`/Lesson/${lesson.lessonId}`);
        toast.success("Lesson deleted");
      }
    } catch (err) {
      console.error("delete lesson", err);
      toast.error("Failed to delete lesson from server");
    }

    // Remove locally
    setLessons((prev) => {
      const copy = prev.filter((_, i) => i !== index).map((l, i) => ({ ...l, orderIndex: i + 1 }));
      setSelectedLessonIndex((s) => {
        if (copy.length === 0) return null;
        if (s == null) return 0;
        if (s >= copy.length) return copy.length - 1;
        return s;
      });
      return copy;
    });
  };
  const reorderLessons = (startIndex, endIndex) => {
    const updated = Array.from(lessons);
    const [moved] = updated.splice(startIndex, 1);
    updated.splice(endIndex, 0, moved);
    const normalized = updated.map((l, idx) => ({
      ...l,
      orderIndex: idx + 1
    }));
    setLessons(normalized);
  };
  const persistLesson = async (index) => {
    if (isViewOnly) return;
    const lesson = lessons[index];
    if (!lesson.title?.trim()) return toast.error("Lesson title required");

    setSavingLesson(true);
    try {
      const payload = {
        ...(lesson.lessonId ? { lessonId: lesson.lessonId } : {}),
        moduleId: localModuleId ?? moduleData?.moduleId ?? null,
        title: lesson.title,
        content: lesson.content,
        lessonType: lesson.lessonType || "Text",
        orderIndex: lesson.orderIndex || 1,
        overview: lesson.overview,
        prerequisites: lesson.prerequisites,
        durationMinutes: lesson.durationMinutes,
        isAiGenerated: false,
        metadata: "{}"
      };

      let savedLessonId = lesson.lessonId;

      if (savedLessonId) {
        await api.put(`/Lesson`, payload);
        toast.success("Lesson updated");
      } else {
        const res = await api.post(`/Lesson`, payload);
        savedLessonId = res.data.lessonId;
        updateLessonLocal(index, { lessonId: savedLessonId });
        toast.success("Lesson created");
      }
    } catch (err) {
      console.error("persist lesson", err);
      toast.error("Failed to save lesson");
    } finally {
      setSavingLesson(false);
    }
  };
  const addResourceToLesson = (li) => {
    updateLessonLocal(li, {
      resources: [...(lessons[li].resources || []), { resourceId: null, title: "", url: "", resourceType: "Link", description: "" }],
    });
  };

  const updateResource = (li, ri, patch) => {
    const lesson = lessons[li];
    const resources = [...(lesson.resources || [])];
    resources[ri] = { ...resources[ri], ...patch };
    updateLessonLocal(li, { resources });
  };

  const removeResource = async (li, ri) => {
    if (!window.confirm("Delete this resource?")) return;

    const lesson = lessons[li];
    const resObj = lesson.resources?.[ri];
    const resources = [...(lesson.resources || [])];
    resources.splice(ri, 1);
    updateLessonLocal(li, { resources });

    if (!resObj?.resourceId) return;

    try {
      await api.delete(`/Resource/${resObj.resourceId}`);
      toast.success("Resource deleted");
    } catch (err) {
      console.error("delete resource", err);
      toast.error("Failed to delete resource from server");
    }
  };

  const persistResourcesBatch = async (li) => {
    const lesson = lessons[li];
    const moduleIdToUse = localModuleId ?? moduleData?.moduleId;
    if (!moduleIdToUse) return toast.error("Please save module first");
    if (!lesson.resources?.length) return;

    try {
      const newResources = lesson.resources.filter(r => !r.resourceId);
      const existingResources = lesson.resources.filter(r => r.resourceId);
      const createPayload = newResources.map(r => ({
        title: r.title || "",
        url: r.url || "",
        resourceType: r.resourceType || "Link",
        moduleId: moduleIdToUse,
        topicId: lesson.lessonId || null,
        description: r.description || "",
        metadata: "{}",
        isAiGenerated: false,
      }));

      // Create new resources on server
      const createdResources = createPayload.length
        ? await api.post(`/Resource/batch`, createPayload)
        : [];

      // Update existing resources individually
      const updatedResources = [];
      for (let r of existingResources) {
        const payload = {
          resourceId: r.resourceId,
          title: r.title || "",
          url: r.url || "",
          resourceType: r.resourceType || "Link",
          moduleId: moduleIdToUse,
          topicId: lesson.lessonId || null,
          description: r.description || "",
          metadata: "{}",
          isAiGenerated: false,
        };
        const updated = await api.put(`/Resource/${r.resourceId}`, payload);
        updatedResources.push(updated);
      }
      const mergedResources = [
        ...updatedResources,
        ...(Array.isArray(createdResources) ? createdResources : [])
      ];

      updateLessonLocal(li, { resources: mergedResources });

      toast.success("Resources saved successfully!");
    } catch (err) {
      console.error("save resources", err);
      toast.error("Failed to save resources");
    }
  };
  const addAssessmentToLesson = (li) => {
    updateLessonLocal(li, {
      assessments: [
        ...(lessons[li].assessments || []),
        {
          assessmentId: null,
          title: "",
          description: "",
          difficulty: 1,
          metadata: "{}",
          questions: [],
        },
      ],
    });
  };

  const updateAssessment = (li, ai, patch) => {
    const assessments = lessons[li].assessments.map((a, idx) =>
      idx === ai ? { ...a, ...patch } : a
    );
    updateLessonLocal(li, { assessments });
  };

  const removeAssessment = async (li, ai) => {
    if (!window.confirm("Delete this assessment?")) return;

    const a = lessons[li].assessments?.[ai];
    const updated = [...lessons[li].assessments];
    updated.splice(ai, 1);
    updateLessonLocal(li, { assessments: updated });

    if (!a?.assessmentId) return;

    try {
      await api.delete(`/Assessment/${a.assessmentId}`);
      toast.success("Assessment deleted");
    } catch (e) {
      console.error(e);
      toast.error("Failed deleting assessment");
    }
  };
  const persistAssessmentsBatch = async (li) => {
    const lesson = lessons[li];
    const moduleId = localModuleId ?? moduleData?.moduleId;
    const existing = lesson.assessments.filter(a => a.assessmentId);
    const create = lesson.assessments.filter(a => !a.assessmentId);
    try {
      if (create.length) {
        const payload = create.map(a => ({
          moduleId,
          topicId: lesson.lessonId,
          title: a.title,
          description: a.description,
          difficulty: a.difficulty,
          metadata: a.metadata
        }));

        const res = await api.post("/Assessment/batch", payload);
        const created = res.data;

        let idx = 0;
        const updated = lesson.assessments.map(a =>
          a.assessmentId ? a : { ...a, assessmentId: created[idx++].assessmentId }
        );

        updateLessonLocal(li, { assessments: updated });
      }
      for (const a of existing) {
        await api.put(`/Assessment/${a.assessmentId}`, {
          title: a.title,
          description: a.description,
          difficulty: a.difficulty,
          metadata: a.metadata
        });
      }

      toast.success("Assessment saved!");
    } catch (e) {
      console.error(e);
      toast.error("Failed to save assessment");
    }
  };
  const saveQuestions = async (li, ai) => {
    const lesson = lessons[li];
    const assessment = lesson.assessments[ai];

    if (!assessment.assessmentId) {
      return toast.error("Save assessment first!");
    }

    try {
      for (const q of assessment.questions) {

        if (!q.questionId) {
          await api.post(`/Assessment/${assessment.assessmentId}/questions`, [
            {
              question: q.question,
              options: JSON.stringify(q.options || []),
              correctAnswer: q.correctAnswer,
              explanation: q.explanation,
              hint: q.hint,
              questionType: "multiple-choice"
            }
          ]);
        }
        else {
          await api.put(`/Assessment/question/${q.questionId}`, {
            questionId: q.questionId,
            question: q.question,
            options: JSON.stringify(q.options || []),
            correctAnswer: q.correctAnswer,
            explanation: q.explanation,
            hint: q.hint,
            questionType: "multiple-choice"
          });
        }
      }

      toast.success("Questions saved!");
    } catch (err) {
      console.error(err);
      toast.error("Failed saving questions");
    }
  };

  const addQuestion = (li, ai) => {
    const ass = [...lessons[li].assessments];
    ass[ai] = {
      ...ass[ai],
      questions: [
        ...(ass[ai].questions || []),
        {
          questionId: null,
          question: "",
          options: [],
          correctAnswer: "",
          explanation: "",
          hint: ""
        }
      ]
    };
    updateLessonLocal(li, { assessments: ass });
  };

  const updateQuestion = (li, ai, qi, patch) => {
    const ass = lessons[li].assessments.map((a, idx) =>
      idx !== ai
        ? a
        : { ...a, questions: a.questions.map((q, i) => i === qi ? { ...q, ...patch } : q) }
    );
    updateLessonLocal(li, { assessments: ass });
  };

  const removeQuestion = async (li, ai, qi) => {
    const a = lessons[li].assessments[ai];
    const q = a.questions[qi];
    const updatedQ = [...a.questions];
    updatedQ.splice(qi, 1);

    const updatedA = [...lessons[li].assessments];
    updatedA[ai] = { ...updatedA[ai], questions: updatedQ };
    updateLessonLocal(li, { assessments: updatedA });

    if (!q.questionId) return;

    await api.delete(`/Assessment/question/${q.questionId}`);
  };
  const moduleExists = useMemo(() => !!(localModuleId || (moduleData && moduleData.moduleId)), [localModuleId, moduleData]);

  const selectedLesson = selectedLessonIndex != null ? lessons[selectedLessonIndex] : null;
  const handleSaveAll = async (e) => {
    e?.preventDefault?.();
    await saveModuleInfo();
    for (let i = 0; i < lessons.length; i++) {
      await persistLesson(i);
      await persistResourcesBatch(i);
      await persistAssessmentsBatch(i);
    }
    toast.success("All saved");
    onModuleSaved?.({ moduleId: localModuleId ?? moduleData?.moduleId });
    onClose?.();
  };
  const handleSaveAssessment = async (li) => {
    const lesson = lessons[li];
    if (!lesson) return;
    await persistAssessmentsBatch(li);
    for (let ai = 0; ai < lesson.assessments.length; ai++) {
      await saveQuestions(li, ai);
    }

    toast.success("Assessment + questions saved!");
  };

  function detectResourceType(filename) {
    const ext = filename.split(".").pop().toLowerCase();

    if (["mp4", "mov", "avi", "mkv"].includes(ext)) return "video";
    if (["jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext)) return "image";
    if (ext === "pdf") return "pdf";
    if (ext === "doc" || ext === "docx") return "docx";
    if (ext === "xls" || ext === "xlsx") return "xlsx";
    if (ext === "ppt" || ext === "pptx") return "pptx";
    return "file";
  }
  function extractYouTubeId(url) {
    const match = url.match(/(?:v=|youtu\.be\/|embed\/)([A-Za-z0-9_-]{11})/);
    return match ? match[1] : null;
  }
  return (
    <div className="fixed inset-0 z-50 bg-black/40 flex justify-center items-start overflow-auto">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-7xl mx-4 md:mx-8 mt-4 mb-4">
        <form onSubmit={handleSaveAll} className="p-6">
          {/* HEADER */}
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="text-2xl font-semibold text-[#0A2342]">
                {localModuleId ? (isViewOnly ? "View Module" : "Edit Module") : "Create Module"}
              </h2>
              <p className="text-sm text-[#1C3C5A] mt-1">
                Organize module info, lessons, resources and assessments.
              </p>
            </div>

            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={onClose}
                className="px-3 py-2 rounded-md border bg-white hover:bg-gray-50 text-sm"
              >
                Close
              </button>

              {!isViewOnly && (
                <button
                  type="submit"
                  className="inline-flex items-center gap-2 px-4 py-2 rounded bg-[#0A2342] text-white"
                >
                  <Save size={16} /> Save All
                </button>
              )}
            </div>
          </div>

          {/* TOP TABS */}
          <div className="flex gap-3 mb-6">
            <button
              type="button"
              onClick={() => setActiveTab("info")}
              className={`px-4 py-2 rounded-md ${activeTab === "info" ? "bg-[#F7D57A] text-[#0A2342]" : "bg-white text-[#1C3C5A] border"}`}
            >
              Module Info
            </button>

            <button
              type="button"
              onClick={() => {
                if (!moduleExists) {
                  if (!window.confirm("Module not yet saved. Save now to manage lessons?")) return;
                }
                setActiveTab("lessons");
              }}
              className={`px-4 py-2 rounded-md ${activeTab === "lessons" ? "bg-[#F7D57A] text-[#0A2342]" : "bg-white text-[#1C3C5A] border"}`}
            >
              Lessons
            </button>
          </div>

          {/* CONTENT */}
          {loading ? (
            <div className="py-12 flex justify-center"><Loader2 className="animate-spin" /></div>
          ) : (
            <>
              {activeTab === "info" && (
                <section className="bg-[#F5F5F5] rounded-lg p-5">
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div>
                      <label className="text-sm font-medium text-[#0A2342]">Module Name</label>
                      <input
                        value={moduleName}
                        onChange={(e) => setModuleName(e.target.value)}
                        className="mt-1 w-full p-2 rounded border"
                        disabled={isViewOnly}
                        placeholder="E.g., LangChain & Prompt Engineering"
                      />
                    </div>

                    <div>
                      <label className="text-sm font-medium text-[#0A2342]">Order in Week</label>
                      <input
                        type="number"
                        min={1}
                        value={orderNo}
                        onChange={(e) => setOrderNo(Number(e.target.value))}
                        className="mt-1 w-full p-2 rounded border"
                        disabled={isViewOnly}
                      />
                    </div>

                    <div>
                      <label className="text-sm font-medium text-[#0A2342]">Duration (days)</label>
                      <input
                        type="number"
                        min={0}
                        value={durationDays}
                        onChange={(e) => setDurationDays(Number(e.target.value))}
                        className="mt-1 w-full p-2 rounded border"
                        disabled={isViewOnly}
                      />
                    </div>
                  </div>

                  <div className="mt-4 grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-[#0A2342]">Short Description</label>
                      <input
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        className="mt-1 w-full p-2 rounded border"
                        disabled={isViewOnly}
                        placeholder="Short description for cards"
                      />
                    </div>

                    <div>
                      <label className="text-sm font-medium text-[#0A2342]">Prerequisites</label>
                      <input
                        value={prerequisites}
                        onChange={(e) => setPrerequisites(e.target.value)}
                        className="mt-1 w-full p-2 rounded border"
                        disabled={isViewOnly}
                        placeholder="Prerequisites (comma separated)"
                      />
                    </div>
                  </div>

                  <div className="mt-4">
                    <label className="text-sm font-medium text-[#0A2342]">Overview / Extended description</label>
                    <textarea
                      value={overview}
                      onChange={(e) => setOverview(e.target.value)}
                      className="mt-1 w-full p-3 rounded border h-32"
                      disabled={isViewOnly}
                      placeholder="Longer module overview shown on module detail"
                    />
                  </div>

                  {!isViewOnly && (
                    <div className="mt-4 flex justify-end gap-3">
                      <button
                        type="button"
                        onClick={saveModuleInfo}
                        className="px-4 py-2 rounded bg-[#1C3C5A] text-white"
                        disabled={savingModule}
                      >
                        {savingModule ? "Saving..." : "Save Module Info"}
                      </button>
                    </div>
                  )}
                </section>
              )}

              {activeTab === "lessons" && (
                <section className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                  {/* Left: lesson list */}
                  <aside className="col-span-1 bg-white border rounded-lg p-4 h-full">
                    <div className="flex items-center justify-between mb-3">
                      <h3 className="font-semibold text-[#0A2342]">Lessons</h3>
                      {!isViewOnly && (
                        <button type="button" className="text-sm text-[#1C3C5A] inline-flex items-center gap-1" onClick={addLessonLocal}>
                          <PlusCircle /> Add
                        </button>
                      )}
                    </div>

                    <div className="space-y-2 max-h-[60vh] overflow-auto">
                      {lessons.length === 0 && <div className="text-sm text-gray-500">No lessons yet</div>}
                      {lessons.map((ls, i) => {
                        const key = ls.lessonId ?? `l-${i}`;
                        return (
                          <div
                            key={key}
                            onClick={() => { setSelectedLessonIndex(i); setLessonSubTab("resources"); }}
                            className={`p-3 rounded-lg cursor-pointer border ${selectedLessonIndex === i ? "bg-[#F7D57A] text-[#0A2342]" : "bg-white"}`}
                          >
                            <div className="flex justify-between items-start">
                              <div>
                                <div className="font-medium">{ls.title || `Lesson ${i + 1}`}</div>
                                <div className="text-xs text-[#1C3C5A]">{ls.overview || ls.lessonType}</div>
                              </div>

                              <div className="flex flex-col items-end gap-2">
                                <div className="text-xs text-[#1C3C5A]">Order {ls.orderIndex}</div>
                                {!isViewOnly && (
                                  <button
                                    type="button"
                                    onClick={(e) => { e.stopPropagation(); removeLessonLocal(i); }}
                                    className="text-red-600 text-sm"
                                  >
                                    <Trash />
                                  </button>
                                )}
                              </div>
                            </div>

                            {/* quick actions */}
                            <div className="mt-2 flex gap-2">
                              {!isViewOnly && (
                                <button
                                  type="button"
                                  onClick={(e) => { e.stopPropagation(); persistLesson(i); }}
                                  className="text-sm px-2 py-1 rounded bg-[#0A2342] text-white"
                                >
                                  Save
                                </button>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </aside>
                  <div className="lg:col-span-2 bg-[#F5F5F5] rounded-lg p-4 min-h-[60vh]">
                    {!selectedLesson ? (
                      <div className="flex flex-col items-center justify-center h-full text-center text-[#1C3C5A]">
                        <ArrowLeftCircle size={36} />
                        <p className="mt-2">Select a lesson from the left to edit content, resources and assessments.</p>
                      </div>
                    ) : (
                      <>
                        {/* Lesson editor top */}
                        <div className="flex items-start justify-between mb-3 gap-4">
                          <div className="flex-1">
                            <input
                              value={selectedLesson.title}
                              onChange={(e) => updateLessonLocal(selectedLessonIndex, { title: e.target.value })}
                              className="w-full p-2 rounded border mb-2"
                              placeholder="Lesson title"
                              disabled={isViewOnly}
                            />
                            <input
                              value={selectedLesson.lessonType}
                              onChange={(e) => updateLessonLocal(selectedLessonIndex, { lessonType: e.target.value })}
                              className="w-full p-2 rounded border mb-2"
                              placeholder="Type (Video / Text / Notebook)"
                              disabled={isViewOnly}
                            />
                            <div className="flex gap-2">
                              <input
                                type="number"
                                value={selectedLesson.orderIndex}
                                onChange={(e) => updateLessonLocal(selectedLessonIndex, { orderIndex: Number(e.target.value) })}
                                className="p-2 rounded border w-28"
                                disabled={isViewOnly}
                              />
                              <input
                                type="number"
                                value={selectedLesson.durationMinutes}
                                onChange={(e) => updateLessonLocal(selectedLessonIndex, { durationMinutes: Number(e.target.value) })}
                                className="p-2 rounded border w-36"
                                disabled={isViewOnly}
                                placeholder="Duration (mins)"
                              />
                            </div>
                          </div>

                          <div className="flex items-center gap-2">
                            {!isViewOnly && (
                              <button
                                type="button"
                                onClick={() => persistLesson(selectedLessonIndex)}
                                className="px-3 py-2 rounded bg-[#0A2342] text-white"
                              >
                                Save Lesson
                              </button>
                            )}
                          </div>
                        </div>

                        <textarea
                          value={selectedLesson.content}
                          onChange={(e) => updateLessonLocal(selectedLessonIndex, { content: e.target.value })}
                          className="w-full p-3 rounded border h-28 mb-3"
                          placeholder="Lesson content (notes, embed code, video link etc.)"
                          disabled={isViewOnly}
                        />

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                          <textarea
                            value={selectedLesson.overview}
                            onChange={(e) => updateLessonLocal(selectedLessonIndex, { overview: e.target.value })}
                            className="p-2 rounded border h-24"
                            placeholder="Short overview"
                            disabled={isViewOnly}
                          />
                          <textarea
                            value={selectedLesson.prerequisites}
                            onChange={(e) => updateLessonLocal(selectedLessonIndex, { prerequisites: e.target.value })}
                            className="p-2 rounded border h-24"
                            placeholder="Prerequisites"
                            disabled={isViewOnly}
                          />
                        </div>
                        <div className="mb-3 flex gap-3">
                          <button
                            type="button"
                            onClick={() => setLessonSubTab("resources")}
                            className={`px-3 py-2 rounded-md ${lessonSubTab === "resources" ? "bg-[#F7D57A] text-[#0A2342]" : "bg-white border text-[#1C3C5A]"}`}
                          >
                            Resources
                          </button>

                          <button
                            type="button"
                            onClick={() => setLessonSubTab("assessments")}
                            className={`px-3 py-2 rounded-md ${lessonSubTab === "assessments" ? "bg-[#F7D57A] text-[#0A2342]" : "bg-white border text-[#1C3C5A]"}`}
                          >
                            Assessments
                          </button>
                        </div>
                        {lessonSubTab === "resources" && (
                          <div>
                            <div className="flex items-center justify-between mb-2">
                              <h4 className="font-semibold text-[#0A2342]">
                                Resources for: {selectedLesson.title || `Lesson ${selectedLessonIndex + 1}`}
                              </h4>

                              {!isViewOnly && (
                                <div className="flex items-center gap-2">
                                  <button
                                    type="button"
                                    onClick={() => addResourceToLesson(selectedLessonIndex)}
                                    className="inline-flex items-center gap-2 text-[#1C3C5A]"
                                  >
                                    <PlusCircle /> Add Resource
                                  </button>

                                  <button
                                    type="button"
                                    onClick={() => persistResourcesBatch(selectedLessonIndex)}
                                    className="px-3 py-1 rounded bg-[#0A2342] text-white"
                                  >
                                    Save Resources
                                  </button>
                                </div>
                              )}
                            </div>
                            <div className="space-y-4">
                              {(selectedLesson.resources || []).map((r, ri) => {
                                const key = r.resourceId ?? `r-${ri}`;

                                return (
                                  <div
                                    key={key}
                                    className="bg-white shadow-sm border rounded-lg p-4 space-y-3"
                                  >
                                    {/* TITLE */}
                                    <input
                                      className="p-2 border rounded w-full"
                                      placeholder="Resource title"
                                      value={r.title}
                                      disabled={isViewOnly}
                                      onChange={(e) =>
                                        updateResource(selectedLessonIndex, ri, { title: e.target.value })
                                      }
                                    />

                                    {/* TYPE DROPDOWN */}
                                    <select
                                      className="p-2 border rounded w-full"
                                      disabled={isViewOnly}
                                      value={r.resourceType || "Link"}
                                      onChange={(e) =>
                                        updateResource(selectedLessonIndex, ri, {
                                          resourceType: e.target.value
                                        })
                                      }
                                    >
                                      <option value="Link">Web Link</option>
                                      <option value="youtube">YouTube Video</option>
                                      <option value="video">Video File (.mp4)</option>
                                      <option value="image">Image</option>
                                      <option value="pdf">PDF</option>
                                      <option value="docx">Word Document</option>
                                      <option value="xlsx">Excel File</option>
                                      <option value="pptx">PowerPoint</option>
                                      <option value="file">Other File</option>
                                    </select>

                                    {/* URL */}
                                    <input
                                      className="p-2 border rounded w-full"
                                      placeholder="URL or file path"
                                      value={r.url}
                                      disabled={isViewOnly}
                                      onChange={(e) =>
                                        updateResource(selectedLessonIndex, ri, { url: e.target.value })
                                      }
                                    />

                                    {/* FILE UPLOAD */}
                                    {!isViewOnly && (
                                      <div className="flex items-center gap-4">
                                        <input
                                          type="file"
                                          id={`file-upload-${key}`}
                                          className="hidden"
                                          onChange={async (e) => {
                                            const file = e.target.files?.[0];
                                            if (!file) return;

                                            try {
                                              const form = new FormData();
                                              form.append("file", file);

                                              const res = await api.post("/File/upload", form, {
                                                headers: { "Content-Type": "multipart/form-data" }
                                              });

                                              const uploadedUrl =
                                                res.data?.url || res.data?.path || res.data;

                                              updateResource(selectedLessonIndex, ri, {
                                                url: uploadedUrl,
                                                resourceType: detectResourceType(file.name)
                                              });

                                              toast.success("File uploaded!");
                                            } catch (err) {
                                              console.error(err);
                                              toast.error("Upload failed");
                                            }
                                          }}
                                        />

                                        <label
                                          htmlFor={`file-upload-${key}`}
                                          className="px-3 py-2 bg-[#0A2342] text-white rounded cursor-pointer"
                                        >
                                          Upload File
                                        </label>

                                        <button
                                          type="button"
                                          onClick={() => removeResource(selectedLessonIndex, ri)}
                                          className="text-red-600 ml-auto"
                                        >
                                          <Trash />
                                        </button>
                                      </div>
                                    )}

                                    {/* PREVIEW SECTION */}
                                    {r.url && (
                                      <div className="mt-2">
                                        {/* YOUTUBE */}
                                        {r.resourceType === "youtube" && extractYouTubeId(r.url) && (
                                          <iframe
                                            width="100%"
                                            height="220"
                                            className="rounded"
                                            src={`https://www.youtube.com/embed/${extractYouTubeId(r.url)}`}
                                          />
                                        )}

                                        {/* VIDEO */}
                                        {r.resourceType === "video" && (
                                          <video src={r.url} controls className="w-full rounded" />
                                        )}

                                        {/* IMAGE */}
                                        {r.resourceType === "image" && (
                                          <img src={r.url} alt="resource" className="max-h-48 rounded" />
                                        )}

                                        {/* PDF */}
                                        {r.resourceType === "pdf" && (
                                          <embed
                                            src={r.url}
                                            type="application/pdf"
                                            className="w-full h-64 border rounded"
                                          />
                                        )}

                                        {/* OTHER FILES */}
                                        {["docx", "xlsx", "pptx", "file"].includes(r.resourceType) && (
                                          <a
                                            href={r.url}
                                            target="_blank"
                                            rel="noreferrer"
                                            className="text-blue-600 underline"
                                          >
                                            Download file
                                          </a>
                                        )}
                                      </div>
                                    )}
                                  </div>
                                );
                              })}

                              {!selectedLesson.resources?.length && (
                                <div className="text-sm text-[#1C3C5A]">No resources yet</div>
                              )}
                            </div>
                          </div>
                        )}


                        {/* Assessments view */}
                        {lessonSubTab === "assessments" && (
                          <div className="space-y-4">
                            <div className="flex items-center justify-between mb-3">
                              <h4 className="font-semibold text-[#0A2342]">
                                Assessments for: {selectedLesson.title || `Lesson ${selectedLessonIndex + 1}`}
                              </h4>
                              {!isViewOnly && (
                                <div className="flex gap-2">
                                  <button
                                    type="button"
                                    onClick={() => addAssessmentToLesson(selectedLessonIndex)}
                                    className="inline-flex items-center gap-1 text-[#1C3C5A] hover:text-[#0A2342]"
                                  >
                                    <PlusCircle /> Add Assessment
                                  </button>
                                  <button
                                    type="button"
                                    onClick={() => handleSaveAssessment(selectedLessonIndex)}
                                    className="px-3 py-1 rounded bg-[#0A2342] text-white hover:bg-[#1C3C5A]"
                                  >
                                    Save Assessment
                                  </button>
                                </div>
                              )}
                            </div>

                            {/* Assessments List */}
                            <div className="space-y-3">
                              {(selectedLesson.assessments || []).map((a, ai) => {
                                const key = a.assessmentId ?? `a-${ai}`;
                                return (
                                  <div key={key} className="bg-white rounded border shadow-sm">
                                    <div className="flex justify-between items-start gap-4 p-3 cursor-pointer hover:bg-gray-50">
                                      <div className="flex-1">
                                        <input
                                          placeholder="Assessment title"
                                          value={a.title}
                                          onChange={(e) => updateAssessment(selectedLessonIndex, ai, { title: e.target.value })}
                                          className="w-full p-2 rounded border mb-2"
                                          disabled={isViewOnly}
                                        />
                                        <textarea
                                          placeholder="Description"
                                          value={a.description}
                                          onChange={(e) => updateAssessment(selectedLessonIndex, ai, { description: e.target.value })}
                                          className="w-full p-2 rounded border mb-2"
                                          disabled={isViewOnly}
                                        />
                                        <div className="flex gap-2 items-center">
                                          <label className="text-sm">Difficulty:</label>
                                          <input
                                            type="number"
                                            min={1}
                                            max={5}
                                            value={a.difficulty}
                                            onChange={(e) =>
                                              updateAssessment(selectedLessonIndex, ai, { difficulty: Number(e.target.value) })
                                            }
                                            className="p-2 rounded border w-20"
                                            disabled={isViewOnly}
                                          />
                                        </div>
                                      </div>

                                      <div className="flex flex-col items-end gap-2">
                                        {!isViewOnly && (
                                          <button
                                            type="button"
                                            onClick={() => removeAssessment(selectedLessonIndex, ai)}
                                            className="text-red-600 hover:text-red-800"
                                          >
                                            <Trash />
                                          </button>
                                        )}
                                        <div className="text-xs text-[#1C3C5A]">
                                          {a.questions?.length ?? 0} question{(a.questions?.length ?? 0) !== 1 && "s"}
                                        </div>
                                      </div>
                                    </div>

                                    {/* Questions Section */}
                                    <div className="p-3 border-t space-y-2">
                                      <div className="flex justify-between items-center mb-2">
                                        <div className="text-sm font-medium">Questions</div>
                                        {!isViewOnly && (
                                          <button
                                            type="button"
                                            onClick={() => addQuestion(selectedLessonIndex, ai)}
                                            className="inline-flex items-center gap-1 text-[#1C3C5A] hover:text-[#0A2342]"
                                          >
                                            <PlusCircle /> Add Question
                                          </button>
                                        )}
                                      </div>

                                      {(a.questions || []).map((q, qi) => {
                                        const qkey = q.questionId ?? `q-${qi}`;
                                        return (
                                          <div key={qkey} className="bg-[#FAFAFA] p-2 rounded border space-y-2">
                                            <input
                                              placeholder="Question text"
                                              value={q.question}
                                              onChange={(e) =>
                                                updateQuestion(selectedLessonIndex, ai, qi, { question: e.target.value })
                                              }
                                              className="p-2 rounded border w-full"
                                              disabled={isViewOnly}
                                            />

                                            {/* Options */}
                                            <div className="space-y-1">
                                              {(q.options || []).map((opt, oi) => (
                                                <div key={oi} className="flex gap-2 items-center">
                                                  <input
                                                    value={opt}
                                                    onChange={(e) => {
                                                      const newOptions = [...(q.options || [])];
                                                      newOptions[oi] = e.target.value;
                                                      updateQuestion(selectedLessonIndex, ai, qi, { options: newOptions });
                                                    }}
                                                    placeholder={`Option ${oi + 1}`}
                                                    className="p-2 rounded border flex-1"
                                                    disabled={isViewOnly}
                                                  />
                                                  {!isViewOnly && (
                                                    <button
                                                      type="button"
                                                      onClick={() => {
                                                        const newOptions = [...(q.options || [])];
                                                        newOptions.splice(oi, 1);
                                                        updateQuestion(selectedLessonIndex, ai, qi, { options: newOptions });
                                                      }}
                                                      className="text-red-600 hover:text-red-800"
                                                    >
                                                      <Trash />
                                                    </button>
                                                  )}
                                                </div>
                                              ))}

                                              {!isViewOnly && (
                                                <button
                                                  type="button"
                                                  onClick={() => {
                                                    const newOptions = [...(q.options || []), ""];
                                                    updateQuestion(selectedLessonIndex, ai, qi, { options: newOptions });
                                                  }}
                                                  className="text-sm text-[#1C3C5A] hover:text-[#0A2342]"
                                                >
                                                  + Add Option
                                                </button>
                                              )}
                                            </div>

                                            <input
                                              placeholder="Correct answer"
                                              value={q.correctAnswer}
                                              onChange={(e) =>
                                                updateQuestion(selectedLessonIndex, ai, qi, { correctAnswer: e.target.value })
                                              }
                                              className="p-2 rounded border w-full"
                                              disabled={isViewOnly}
                                            />

                                            {!isViewOnly && (
                                              <button
                                                type="button"
                                                onClick={() => removeQuestion(selectedLessonIndex, ai, qi)}
                                                className="text-red-600 hover:text-red-800"
                                              >
                                                Delete Question
                                              </button>
                                            )}
                                          </div>
                                        );
                                      })}

                                      {!a.questions?.length && (
                                        <div className="text-sm text-[#1C3C5A]">No questions yet</div>
                                      )}
                                    </div>
                                  </div>
                                );
                              })}
                              {!selectedLesson.assessments?.length && (
                                <div className="text-sm text-[#1C3C5A]">No assessments yet</div>
                              )}
                            </div>
                          </div>
                        )}

                      </>
                    )}
                  </div>
                </section>
              )}
            </>
          )}
        </form>
      </div>
    </div>
  );
}
