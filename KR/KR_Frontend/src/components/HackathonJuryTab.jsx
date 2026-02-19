import React, { useEffect, useState } from "react";
import api from "../api";
import {
    RadarChart, PolarGrid, PolarAngleAxis, Radar,
    ResponsiveContainer
} from "recharts";
import { toast } from "react-hot-toast";
import {
    Trophy,
    Award, Star, ChevronRight, X
} from "lucide-react";
import { Tooltip } from "recharts";

export default function HackathonJuryTab() {
    const [events, setEvents] = useState([]);
    const [eventId, setEventId] = useState("");
    const [teams, setTeams] = useState([]);
    const [selectedTeam, setSelectedTeam] = useState(null);
    const [radar, setRadar] = useState([]);
    const [rankings, setRankings] = useState([]);
    const [scores, setScores] = useState([
        { criteriaName: "Innovation", score: 0 },
        { criteriaName: "Technical Feasibility", score: 0 },
        { criteriaName: "Business Value", score: 0 },
        { criteriaName: "User Experience", score: 0 }
    ]);
    const [activeAiTab, setActiveAiTab] = useState("Strengths");

    const [activeTab, setActiveTab] = useState("evaluation");
    const [showIdeaModal, setShowIdeaModal] = useState(false);
    const [showAIModal, setShowAIModal] = useState(false);

    useEffect(() => {
        api.get("/Events/type/Hackathon")
            .then(res => setEvents(res.data || []));
    }, []);

    const loadEventData = async (id) => {
        const teamsRes = await api.get(`/hackathon/jury/teams/${id}`);
        const unique = Object.values(
            teamsRes.data.reduce((acc, t) => {
                acc[t.teamId] = t;
                return acc;
            }, {})
        );
        setTeams(unique);
        setSelectedTeam(null);
        setRadar([]);

        const rankRes = await api.get(`/jury/rankings/${id}`);
        setRankings(rankRes.data || []);
    };

    const loadRadar = async (teamId) => {
        const res = await api.get(`/jury/radar/${teamId}`);
        const radarData = res.data || [];

        setRadar(radarData);
        if (radarData.length) {
            setScores(prev =>
                prev.map(s => {
                    const match = radarData.find(r => r.criteriaName === s.criteriaName);
                    return match ? { ...s, score: match.score } : s;
                })
            );
        }
    };
    const generateAI = async (team) => {
        await api.post(`/hackathon/jury/ai-evaluate/${team.teamId}?eventId=${eventId}`);
        toast.success("AI evaluation generated");
        loadRadar(team.teamId);
    };

    const submitScore = async () => {
        await api.post("/jury/score", {
            eventId,
            teamId: selectedTeam.teamId,
            juryId: localStorage.getItem("userId"),
            scores
        });
        toast.success("Evaluation submitted");
        loadRadar(selectedTeam.teamId);
        loadEventData(eventId);
    };
    const parseAIFeedback = (text) => {
        const sections = {};
        const regex = /\*\*\d+\.\s(.*?)\:\*\*\n([\s\S]*?)(?=\n\*\*\d+\.|$)/g;
        let match;

        while ((match = regex.exec(text)) !== null) {
            sections[match[1].trim()] = match[2].trim();
        }

        return sections;
    };


    return (
        <div className="min-h-screen bg-slate-50 px-6 space-y-4">
            <div className="flex justify-between items-center">
                <div>
                    <h2 className="text-2xl font-semibold text-gray-900">
                        Hackathon Jury Console
                    </h2>
                    <p className="text-sm text-gray-500">
                        Professional evaluation platform
                    </p>
                </div>

                <select
                    className="border border-gray-300 px-4 py-2 rounded-lg text-sm text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    value={eventId}
                    onChange={e => {
                        setEventId(e.target.value);
                        loadEventData(e.target.value);
                    }}
                >
                    <option value="">Select Hackathon</option>
                    {events.map(e => (
                        <option key={e.eventId} value={e.eventId}>
                            {e.title}
                        </option>
                    ))}
                </select>
            </div>
            {eventId && (
                <div className="flex gap-2">
                    {["Evaluation", "Analytics", "Leaderboard"].map(tab => (
                        <button
                            key={tab}
                            onClick={() => setActiveTab(tab)}
                            className={`px-5 py-1 rounded-lg font-medium
                ${activeTab === tab
                                    ? "bg-blue-600 text-white"
                                    : "bg-white border hover:bg-slate-100"
                                }`}
                        >
                            {tab}
                        </button>
                    ))}
                </div>
            )}
            {activeTab === "Evaluation" && eventId && (
                <div className="space-y-4">
                    <div className="flex gap-2 pt-1 overflow-x-auto pb-1">
                        {teams.map(t => (
                            <button
                                key={t.teamId}
                                onClick={() => {
                                    setSelectedTeam(t);
                                    loadRadar(t.teamId);
                                }}
                                className={`px-4 py-1 rounded-full border text-md
            ${selectedTeam?.teamId === t.teamId
                                        ? "bg-yellow-400 text-black"
                                        : "bg-white hover:bg-slate-100"
                                    }`}
                            >
                                {t.teamName}
                            </button>
                        ))}
                    </div>
                    <div className="grid grid-cols-3 gap-6">
                        <div className="col-span-2 bg-white rounded-xl shadow p-8 space-y-4">
                            {!selectedTeam ? (
                                <p className="text-slate-400">Select a team to begin evaluation</p>
                            ) : (
                                <>
                                    <div>
                                        <h3 className="text-2xl font-bold text-indigo-700">
                                            {selectedTeam.teamName}
                                        </h3>
                                    </div>
                                    <div className="grid grid-cols-3 gap-4">
                                        <div className="bg-slate-50 p-4 rounded-lg">
                                            <p className="text-xs text-slate-500">Submission</p>
                                            <p className="font-bold text-slate-800">
                                                {selectedTeam.ideaText ? "Available" : "Not submitted"}
                                            </p>
                                        </div>

                                        <div className="bg-slate-50 p-4 rounded-lg">
                                            <p className="text-xs text-slate-500">AI Evaluation</p>
                                            <p className={`font-bold ${selectedTeam.aiFeedback ? "text-green-600" : "text-amber-600"
                                                }`}>
                                                {selectedTeam.aiFeedback ? "Completed" : "Pending"}
                                            </p>
                                        </div>

                                        <div className="bg-slate-50 p-4 rounded-lg">
                                            <p className="text-xs text-slate-500">Your Total Score</p>
                                            <p className="font-bold text-indigo-700">
                                                {scores.reduce((a, b) => a + b.score, 0)}/100
                                            </p>
                                        </div>
                                    </div>
                                    <div className="flex gap-3">
                                        <button
                                            onClick={() => setShowIdeaModal(true)}
                                            className="bg-blue-500 text-white px-5 py-2 rounded-lg"
                                        >
                                            View Submission
                                        </button>

                                        {selectedTeam.aiFeedback ? (
                                            <button
                                                onClick={() => setShowAIModal(true)}
                                                className="bg-indigo-600 text-white px-5 py-2 rounded-lg"
                                            >
                                                View AI Evaluation
                                            </button>
                                        ) : (
                                            <button
                                                onClick={() => generateAI(selectedTeam)}
                                                className="bg-green-600 text-white px-5 py-2 rounded-lg"
                                            >
                                                Generate AI Evaluation
                                            </button>
                                        )}
                                    </div>
                                    {radar.length > 0 && (
                                        <div className="bg-green-50 border border-green-200 p-3 rounded-lg text-sm text-green-700">
                                            ✔ This team has already been evaluated by you.
                                        </div>
                                    )}
                                </>
                            )}
                        </div>
                        <div className="bg-white rounded-xl shadow p-6 space-y-4">
                            <h3 className="font-semibold text-slate-700">
                                Jury Scoring
                            </h3>

                            {!selectedTeam ? (
                                <p className="text-slate-400">Select a team</p>
                            ) : (
                                <>
                                    {scores.map((s, i) => (
                                        <div key={i}>
                                            <div className="flex justify-between text-sm mb-1">
                                                <span>{s.criteriaName}</span>
                                                <span className="font-bold text-indigo-700">
                                                    {s.score}/25
                                                </span>
                                            </div>
                                            <input
                                                type="range"
                                                min="0"
                                                max="25"
                                                value={s.score}
                                                onChange={e => {
                                                    const copy = [...scores];
                                                    copy[i].score = Number(e.target.value);
                                                    setScores(copy);
                                                }}
                                                className="w-full accent-indigo-600"
                                            />
                                        </div>
                                    ))}

                                    <button
                                        onClick={submitScore}
                                        className="mt-4 bg-green-600 text-white w-full py-2 rounded-lg"
                                    >
                                        Submit Evaluation
                                    </button>

                                    {radar.length > 0 && (
                                        <p className="text-xs text-green-600 text-center mt-2">
                                            Evaluation already submitted. You can resubmit if needed.
                                        </p>
                                    )}
                                </>
                            )}
                        </div>
                    </div>
                </div>
            )}
            {activeTab === "Analytics" && (
                <div className="space-y-6">

                    {!selectedTeam ? (
                        <div className="bg-white rounded-xl shadow p-8 text-slate-400">
                            Select a team to view analytics
                        </div>
                    ) : radar.length === 0 ? (
                        <div className="bg-white rounded-xl shadow p-8 text-slate-400">
                            No evaluations yet for this team
                        </div>
                    ) : (() => {

                        const sorted = [...radar].sort((a, b) => b.score - a.score);
                        const best = sorted[0];
                        const worst = sorted[sorted.length - 1];
                        const avg = radar.reduce((a, b) => a + b.score, 0) / radar.length;

                        return (
                            <>
                                <div className="grid grid-cols-4 gap-4">

                                    <div className="bg-indigo-50 p-4 rounded-xl">
                                        <p className="text-s text-black-500">Overall Avg</p>
                                        <p className="text-1xl font-semibold text-indigo-700">
                                            {avg.toFixed(1)}/25
                                        </p>
                                    </div>

                                    <div className="bg-green-50 p-4 rounded-xl">
                                        <p className="text-s text-black-500">Strongest Area</p>
                                        <p className="font-bold text-green-700">
                                            {best.criteriaName}
                                        </p>
                                    </div>

                                    <div className="bg-red-50 p-4 rounded-xl">
                                        <p className="text-s text-black-500">Weakest Area</p>
                                        <p className="font-bold text-red-600">
                                            {worst.criteriaName}
                                        </p>
                                    </div>

                                    <div className="bg-slate-100 p-4 rounded-xl">
                                        <p className="text-s text-black-500">Risk Level</p>
                                        <p className={`font-bold ${avg >= 18 ? "text-green-600" :
                                            avg >= 12 ? "text-amber-600" :
                                                "text-red-600"
                                            }`}>
                                            {avg >= 18 ? "Low" :
                                                avg >= 12 ? "Medium" :
                                                    "High"}
                                        </p>
                                    </div>
                                </div>
                                <div className="bg-white rounded-xl shadow p-8 grid grid-cols-2 gap-8">
                                    <div>
                                        <h3 className="font-semibold mb-4 text-slate-700">
                                            Capability Map
                                        </h3>

                                        <ResponsiveContainer width="100%" height={320}>
                                            <RadarChart data={radar}>
                                                <PolarGrid stroke="#e5e7eb" />
                                                <PolarAngleAxis dataKey="criteriaName" tick={{ fill: "#475569", fontSize: 12 }} />

                                                <Tooltip
                                                    contentStyle={{
                                                        backgroundColor: "#0f172a",
                                                        borderRadius: "10px",
                                                        border: "none",
                                                        color: "white",
                                                        padding: "10px 14px"
                                                    }}
                                                    formatter={(value, name) => [
                                                        `${value}/25`,
                                                        "Score"
                                                    ]}
                                                    labelStyle={{ color: "#c7d2fe", fontWeight: 600 }}
                                                />

                                                <Radar
                                                    dataKey="score"
                                                    stroke="#6366f1"
                                                    fill="#6366f1"
                                                    fillOpacity={0.45}
                                                    dot={{ r: 4, fill: "#6366f1" }}
                                                    activeDot={{ r: 7, fill: "#22c55e" }}
                                                />
                                            </RadarChart>
                                        </ResponsiveContainer>

                                        <p className="text-xs text-slate-400 mt-2">
                                            Hover over each dimension to view precise score and strength.
                                        </p>
                                        <div className="mt-3 text-sm text-slate-600">
                                            Strongest area:{" "}
                                            <span className="font-semibold text-green-600">
                                                {[...radar].sort((a, b) => b.score - a.score)[0]?.criteriaName}
                                            </span>
                                        </div>
                                    </div>
                                    <div className="space-y-4">
                                        <h3 className="font-semibold text-slate-700">
                                            Performance Breakdown
                                        </h3>

                                        {sorted.map(r => (
                                            <div key={r.criteriaName} className="space-y-1">

                                                <div className="flex justify-between text-sm">
                                                    <span>{r.criteriaName}</span>
                                                    <span className="font-bold">{r.score}/25</span>
                                                </div>

                                                <div className="h-2 rounded-full bg-slate-100">
                                                    <div
                                                        className={`h-full rounded-full ${r.score >= 18 ? "bg-green-500" :
                                                            r.score >= 12 ? "bg-amber-500" :
                                                                "bg-red-500"
                                                            }`}
                                                        style={{ width: `${(r.score / 25) * 100}%` }}
                                                    />
                                                </div>

                                            </div>
                                        ))}

                                        <div className="mt-4 p-4 rounded-xl bg-indigo-50 border border-indigo-100">
                                            <p className="text-sm text-indigo-700">
                                                <strong>Executive Verdict:</strong>{" "}
                                                {avg >= 18
                                                    ? "This team demonstrates strong overall capability and is a top contender."
                                                    : avg >= 12
                                                        ? "This team shows average potential but requires improvement in key areas."
                                                        : "This team carries high execution risk and needs significant refinement."
                                                }
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </>
                        );
                    })()}
                </div>
            )}
            {activeTab === "Leaderboard" && (
                <div className="bg-white rounded-2xl shadow border">

                    {/* HEADER */}
                    <div className="flex items-center justify-between px-6 py-4 border-b">
                        <div className="flex items-center gap-2">
                            <Trophy className="text-indigo-600" />
                            <h3 className="font-semibold text-lg">Final Leaderboard</h3>
                        </div>
                        <span className="text-sm text-slate-500">
                            Ranked by total jury score
                        </span>
                    </div>
                    <div className="grid grid-cols-3 gap-4 p-6">
                        {rankings.slice(0, 3).map((r, i) => {
                            const colors = [
                                "bg-yellow-100 text-yellow-700",
                                "bg-slate-100 text-slate-700",
                                "bg-amber-100 text-amber-700"
                            ];

                            const medals = ["", "", ""];

                            return (
                                <div key={r.teamId}
                                    className={`rounded-xl p-4 text-center ${colors[i]}`}>
                                    <div className="text-3xl mb-2">{medals[i]}</div>
                                    <p className="font-semibold">{r.teamName}</p>
                                    <p className="text-xl font-bold">{r.totalScore}</p>
                                    <p className="text-xs mt-1 opacity-70">points</p>
                                </div>
                            );
                        })}
                    </div>
                    <div className="divide-y">
                        {rankings.map((r, i) => {
                            const icon =
                                i === 0 ? <Trophy className="text-yellow-500" /> :
                                    i === 1 ? <Award className="text-slate-500" /> :
                                        i === 2 ? <Star className="text-amber-500" /> :
                                            <ChevronRight className="text-slate-400" />;

                            return (
                                <div key={r.teamId}
                                    className="flex items-center justify-between px-6 py-3 hover:bg-slate-50 transition">
                                    <div className="flex items-center gap-4">
                                        <div className="w-8 h-8 rounded-lg bg-slate-100 flex items-center justify-center">
                                            {icon}
                                        </div>
                                        <div>
                                            <p className="font-medium">{r.teamName}</p>
                                            <p className="text-xs text-slate-500">Rank #{i + 1}</p>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <span className="text-lg font-semibold text-indigo-700">
                                            {r.totalScore}
                                        </span>
                                        <span className="text-xs text-slate-500">pts</span>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>
            )}
            {showIdeaModal && selectedTeam && (
                <Modal title="Team Submission" onClose={() => setShowIdeaModal(false)}>

                    <div className="grid grid-cols-3 gap-4 mb-6">
                        <div className="bg-indigo-50 p-4 rounded-lg">
                            <p className="text-xs text-slate-500">Team</p>
                            <p className="font-bold text-indigo-700">
                                {selectedTeam.teamName}
                            </p>
                        </div>

                        <div className="bg-green-50 p-4 rounded-lg">
                            <p className="text-xs text-slate-500">Submission Status</p>
                            <p className="font-bold text-green-700">
                                Submitted
                            </p>
                        </div>

                        <div className="bg-slate-50 p-4 rounded-lg">
                            <p className="text-xs text-slate-500">Content Type</p>
                            <p className="font-bold text-slate-700">
                                Idea Proposal
                            </p>
                        </div>
                    </div>

                    <div className="bg-white border rounded-lg p-5 space-y-4">

                        <h4 className="font-semibold text-indigo-700">
                            Full Idea Description
                        </h4>

                        <div className="text-sm text-slate-700 leading-relaxed whitespace-pre-line">
                            {selectedTeam.ideaText}
                        </div>

                    </div>

                </Modal>
            )}

            {showAIModal && selectedTeam && (() => {
                const sections = parseAIFeedback(selectedTeam.aiFeedback);
                const tabs = Object.keys(sections);

                return (
                    <Modal title="AI Expert Evaluation" onClose={() => setShowAIModal(false)}>
                        <div className="grid grid-cols-3 gap-4 mb-6">
                            <div className="bg-indigo-50 p-4 rounded-lg">
                                <p className="text-xs text-slate-500">AI Score</p>
                                <p className=" font-semibold text-indigo-700">
                                    {selectedTeam.aiScore || "N/A"} / 100
                                </p>
                            </div>

                            <div className="bg-green-50 p-4 rounded-lg">
                                <p className="text-xs text-slate-500">Verdict</p>
                                <p className="font-bold text-green-700">
                                    {selectedTeam.aiScore >= 70
                                        ? "Strong Candidate"
                                        : selectedTeam.aiScore >= 40
                                            ? "Average Potential"
                                            : "High Risk"}
                                </p>
                            </div>

                            <div className="bg-amber-50 p-4 rounded-lg">
                                <p className="text-xs text-slate-500">Risk Level</p>
                                <p className="font-bold text-amber-700">
                                    {selectedTeam.aiScore >= 70 ? "Low" : "Medium"}
                                </p>
                            </div>
                        </div>
                        <div className="flex gap-2 mb-4">
                            {tabs.map(t => (
                                <button
                                    key={t}
                                    onClick={() => setActiveAiTab(t)}
                                    className={`px-4 py-1 rounded-full text-sm font-medium
              ${activeAiTab === t
                                            ? "bg-indigo-600 text-white"
                                            : "bg-slate-100 text-slate-700"}
            `}
                                >
                                    {t}
                                </button>
                            ))}
                        </div>
                        <div className="bg-slate-50 rounded-xl p-5">
                            <h4 className="font-semibold text-indigo-700 mb-2">
                                {activeAiTab}
                            </h4>

                            <p className="text-sm text-slate-700 leading-relaxed whitespace-pre-line">
                                {sections[activeAiTab]}
                            </p>
                        </div>

                    </Modal>
                );
            })()}
        </div>
    );
}

function Modal({ title, children, onClose }) {
    return (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
            <div className="bg-white w-full max-w-5xl rounded-xl p-6
                      max-h-[85vh] overflow-y-auto shadow-xl">

                <div className="flex justify-between items-center mb-4 sticky top-0 bg-white z-10">
                    <h3 className="text-lg font-bold">{title}</h3>
                    <button onClick={onClose} className="p-1 hover:bg-slate-100 rounded">
                        <X />
                    </button>
                </div>

                {children}
            </div>
        </div>
    );
}

