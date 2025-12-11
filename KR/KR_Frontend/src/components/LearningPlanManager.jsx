import React, { useEffect, useState } from "react";
import toast from "react-hot-toast";
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

    const [openedModuleDetail, setOpenedModuleDetail] = useState(null);

    const [showAIModal, setShowAIModal] = useState(false);
    const [aiInstructions, setAIInstructions] = useState("");
    const [aiLoading, setAILoading] = useState(false);
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
        setSelectedPlan(plan);
        setSelectedWeek(null);
        setModules([]);
        setOpenedModuleDetail(null);

        try {
            const res = await api.get(`/Week/plan/${plan.planId}`);
            setWeeks(res.data || []);
        } catch {
            toast.error("Failed to load weeks");
        }
    };

    const deletePlan = async (planId) => {
        if (!window.confirm("Delete this learning plan?")) return;
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
        setSelectedWeek(week);
        setOpenedModuleDetail(null);

        try {
            const res = await api.get(`/Module/week/${week.weekId}`);
            setModules(res.data || []);
        } catch {
            toast.error("Failed to load modules");
        }
    };

    const deleteWeek = async (weekId) => {
        if (!window.confirm("Delete week?")) return;

        try {
            await api.delete(`/Week/${weekId}`);
            if (selectedPlan) managePlan(selectedPlan);
            toast.success("Week deleted");
        } catch {
            toast.error("Failed to delete week");
        }
    };

    const handleWeekCreated = () => {
        if (selectedPlan) managePlan(selectedPlan);
    };

    const deleteModule = async (moduleId) => {
        if (!window.confirm("Delete module?")) return;

        try {
            await api.delete(`/Module/${moduleId}`);
            if (selectedWeek) manageWeek(selectedWeek);
        } catch {
            toast.error("Failed to delete module");
        }
    };

    const openModuleDetail = async (module) => {
        try {
            const res = await api.get(`/Module/${module.moduleId}`, {
                params: { userId: "00000000-0000-0000-0000-000000000001" }
            });
            setOpenedModuleDetail(res.data);
        } catch {
            toast.error("Failed to load module details");
        }
    };

    const generateAIContent = async () => {
        if (!aiInstructions.trim()) {
            toast.error("Please provide instructions for AI");
            return;
        }

        setAILoading(true);

        try {
            await api.post("/AI/generate/week", {
                planId: selectedPlan.planId,
                instructions: aiInstructions
            });

            toast.success("AI generated content added!");
            managePlan(selectedPlan);
            setShowAIModal(false);
            setAIInstructions("");
        } catch {
            toast.error("AI generation failed");
        } finally {
            setAILoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-gradient-to-b from-white to-gray-100 flex flex-col font-inter">

            {/* HEADER */}
            <header className="w-full bg-white/70 backdrop-blur-md border-b shadow-sm">
                <div className="max-w-6xl mx-auto px-8 py-6 flex justify-between items-center">
                    <div>
                        <h1 className="text-3xl font-bold text-gray-900 tracking-tight">Learning Platform Manager</h1>
                        <p className="text-sm text-gray-600">Organize learning plans, weeks, modules, lessons, resources</p>
                    </div>

                    <button
                        className="px-6 py-2.5 rounded-lg text-sm bg-blue-600 text-white shadow-md hover:bg-blue-700 transition"
                        onClick={() => {
                            setSelectedPlan(null);
                            setShowPlanModal(true);
                        }}
                    >
                        + Create Plan
                    </button>
                </div>
            </header>

            {/* MAIN CONTENT */}
            <main className="flex-1 max-w-6xl mx-auto px-8 py-12 space-y-16">

                {/* PLANS */}
                <section>
                    <h2 className="text-2xl font-semibold text-gray-800 mb-6">Learning Plans</h2>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                        {plans.map(plan => (
                            <div
                                key={plan.planId}
                                className="bg-white border rounded-2xl p-6 shadow hover:shadow-lg transition-all"
                            >
                                <h3 className="text-lg font-semibold text-gray-900">{plan.title}</h3>
                                <p className="text-sm text-gray-600 mt-2">{plan.overview || plan.description}</p>

                                <p className="text-xs mt-4 text-gray-700">
                                    📅 {plan.durationWeeks || 0} Weeks
                                </p>

                                <div className="flex gap-2 mt-4">
                                    <button
                                        className="px-3 py-1 rounded bg-blue-100 text-blue-700 text-sm hover:bg-blue-200"
                                        onClick={() => {
                                            setSelectedPlan(plan);
                                            setShowPlanModal(true);
                                        }}
                                    >
                                        Edit
                                    </button>

                                    <button
                                        className="px-3 py-1 rounded bg-red-100 text-red-700 text-sm hover:bg-red-200"
                                        onClick={() => deletePlan(plan.planId)}
                                    >
                                        Delete
                                    </button>

                                    <button
                                        className="px-3 py-1 rounded bg-green-100 text-green-700 text-sm hover:bg-green-200"
                                        onClick={() => managePlan(plan)}
                                    >
                                        Manage
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                </section>

                {/* WEEKS */}
                {selectedPlan && (
                    <section>
                        <div className="flex justify-between items-center mb-6">
                            <h2 className="text-2xl font-semibold text-gray-800">Weeks in “{selectedPlan.title}”</h2>

                            <button
                                className="px-4 py-2 rounded bg-blue-600 text-white shadow hover:bg-blue-700"
                                onClick={() => {
                                    setSelectedWeek(null);
                                    setShowWeekModal(true);
                                }}
                            >
                                + Add Week
                            </button>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                            {weeks.map(week => (
                                <div
                                    key={week.weekId}
                                    className="bg-white border rounded-2xl p-6 shadow hover:shadow-lg transition"
                                >
                                    <h4 className="text-lg font-semibold text-gray-800">{week.title}</h4>
                                    <p className="text-sm text-gray-600 mt-1">
                                        Contains {selectedWeek?.weekId === week.weekId ? modules.length : "—"} modules
                                    </p>

                                    <div className="flex gap-2 mt-4">
                                        <button
                                            className="px-3 py-1 bg-blue-100 text-blue-700 rounded hover:bg-blue-200"
                                            onClick={() => {
                                                setSelectedWeek(week);
                                                setShowWeekModal(true);
                                            }}
                                        >
                                            Edit
                                        </button>

                                        <button
                                            className="px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200"
                                            onClick={() => deleteWeek(week.weekId)}
                                        >
                                            Delete
                                        </button>

                                        <button
                                            className="px-3 py-1 bg-green-100 text-green-700 rounded hover:bg-green-200"
                                            onClick={() => manageWeek(week)}
                                        >
                                            Modules
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </section>
                )}

                {/* MODULES */}
                {selectedWeek && (
                    <section>
                        <div className="flex justify-between items-center mb-6">
                            <h2 className="text-2xl font-semibold text-gray-800">Modules in “{selectedWeek.title}”</h2>

                            <button
                                className="px-4 py-2 rounded bg-blue-600 text-white shadow hover:bg-blue-700"
                                onClick={() => {
                                    setSelectedModule(null);
                                    setShowModuleModal(true);
                                }}
                            >
                                + Add Module
                            </button>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                            {modules.map(mod => (
                                <div
                                    key={mod.moduleId}
                                    className="bg-white border rounded-2xl p-6 shadow hover:shadow-lg transition"
                                >
                                    <h4 className="text-lg font-semibold text-gray-900">{mod.moduleName}</h4>
                                    <p className="text-sm text-gray-600 mt-2">
                                        {mod.overview || mod.description}
                                    </p>

                                    <div className="flex gap-2 mt-4">
                                        <button
                                            className="px-3 py-1 bg-blue-100 text-blue-700 rounded hover:bg-blue-200"
                                            onClick={() => {
                                                setSelectedModule(mod);
                                                setShowModuleModal(true);
                                            }}
                                        >
                                            Edit
                                        </button>

                                        <button
                                            className="px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200"
                                            onClick={() => deleteModule(mod.moduleId)}
                                        >
                                            Delete
                                        </button>

                                        <button
                                            className="px-3 py-1 bg-green-100 text-green-700 rounded hover:bg-green-200"
                                            onClick={() => openModuleDetail(mod)}
                                        >
                                            Details
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </section>
                )}

                {/* MODULE DETAILS */}
                {openedModuleDetail && (
                    <section className="bg-white rounded-2xl border shadow p-8">
                        <h2 className="text-2xl font-semibold text-gray-800">{openedModuleDetail.moduleName}</h2>
                        <p className="text-gray-600 mt-2 mb-6">{openedModuleDetail.description}</p>

                        <div className="space-y-4">
                            {openedModuleDetail.lessons?.map(lesson => (
                                <div key={lesson.lessonId} className="bg-gray-50 border rounded-xl p-5 shadow-sm">
                                    <h4 className="font-semibold text-gray-800">{lesson.title}</h4>
                                    <p className="text-sm text-gray-600 mt-2">{lesson.content}</p>
                                </div>
                            ))}
                        </div>
                    </section>
                )}
            </main>

            {/* MODALS */}
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
                    onWeekCreated={handleWeekCreated}
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
