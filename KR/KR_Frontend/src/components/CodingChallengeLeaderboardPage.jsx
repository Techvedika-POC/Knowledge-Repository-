import React, { useEffect, useState } from "react";
import api from "../api";
import {
    PieChart, Pie, Cell, Tooltip,
    BarChart, Bar, XAxis, YAxis, ReferenceLine,ResponsiveContainer, LabelList
} from "recharts";
import {
  Trophy,
  Target,
  Code,
  Upload
} from "lucide-react";
import { FaTrophy } from "react-icons/fa";
export default function CodingChallengeLeaderboardPage() {
    const [data, setData] = useState([]);

    useEffect(() => {
        loadLeaderboard();
    }, []);

    const loadLeaderboard = async () => {
        const res = await api.get("/coding/leaderboard");
        setData(res.data);
    };

    const loggedUser = {
        userId: localStorage.getItem("userId"),
        userName: localStorage.getItem("userName")
    };

    if (!loggedUser.userId) {
        return <div className="p-20 text-center">Please login again</div>;
    }

    if (!data.length) {
        return <div className="p-20 text-center">Loading leaderboard...</div>;
    }

    const currentUser =
        data.find(u => u.userId === loggedUser.userId) || data[0];

    const pieData = [
        { name: "Code", value: currentUser.bestCodeScore },
        { name: "Interview", value: currentUser.bestInterviewScore }
    ];

    const top5 = [...data]
        .sort((a, b) => b.overallScore - a.overallScore)
        .slice(0, 5);

    return (
        <div className="min-h-screen bg-slate-100 px-8 space-y-5">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-3xl font-bold text-slate-800">
                        Coding Challenge Dashboard
                    </h1>
                    <p className="text-slate-500 text-sm">
                        Track your performance and compare with top performers
                    </p>
                </div>
                <FaTrophy className="text-yellow-500 text-4xl" />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">

                <KpiCard
                    title="My Rank"
                    value={`#${currentUser.rank}`}
                    icon={Trophy}
                    gradient="from-indigo-200 via-indigo-300 to-indigo-400"
                />

                <KpiCard
                    title="Overall Score"
                    value={currentUser.overallScore}
                    icon={Target}
                    gradient="from-emerald-200 via-green-300 to-green-400"
                />

                <KpiCard
                    title="Code Score"
                    value={currentUser.bestCodeScore}
                    icon={Code}
                    gradient="from-sky-200 via-blue-300 to-blue-400"
                />

                <KpiCard
                    title="Submissions"
                    value={currentUser.submissions}
                    icon={Upload}
                    gradient="from-violet-200 via-purple-300 to-purple-400"
                />

            </div>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                <div className="bg-white rounded-xl shadow p-6 flex flex-col items-center">
                    <h3 className="font-semibold mb-1 text-slate-700">
                        Skill Distribution
                    </h3>
                    <p className="text-xs text-slate-500 mb-2">
                        Breakdown of your performance areas
                    </p>

                    <ResponsiveContainer width="100%" height={260}>
                        <PieChart>
                            <Pie
                                data={pieData}
                                dataKey="value"
                                innerRadius={65}
                                outerRadius={95}
                                paddingAngle={3}
                                label={({ name, percent }) =>
                                    `${name}: ${(percent * 100).toFixed(0)}%`
                                }
                            >
                                <Cell fill="#3b82f6" />
                                <Cell fill="#22c55e" />
                            </Pie>
                            <text
                                x="50%"
                                y="50%"
                                textAnchor="middle"
                                dominantBaseline="middle"
                                className="text-xl font-bold fill-slate-700"
                            >
                                {currentUser.overallScore}
                            </text>

                            <Tooltip />
                        </PieChart>
                    </ResponsiveContainer>
                    <div className="flex gap-6 mt-2 text-sm">
                        <div className="flex items-center gap-2">
                            <span className="w-3 h-3 bg-blue-500 rounded-full"></span>
                            Code: {currentUser.bestCodeScore}
                        </div>
                        <div className="flex items-center gap-2">
                            <span className="w-3 h-3 bg-green-500 rounded-full"></span>
                            Interview: {currentUser.bestInterviewScore}
                        </div>
                    </div>
                    <p className="text-xs text-slate-500 mt-1 text-center">
                        {currentUser.bestInterviewScore > currentUser.bestCodeScore
                            ? "You perform better in interviews than coding challenges."
                            : "Your coding performance is stronger than interviews."}
                    </p>
                </div>
                <div className="bg-white rounded-xl shadow p-6">
                    <h3 className="font-semibold text-slate-700">
                        Top 5 Performers
                    </h3>
                    <p className="text-xs text-slate-500 mb-4">
                        Highest overall scores across all participants
                    </p>

                    <ResponsiveContainer width="100%" height={280}>
                        <BarChart data={top5}>
                            <XAxis
                                dataKey="userName"
                                interval={0}
                                angle={-30}
                                textAnchor="end"
                                height={70}
                            />
                            <YAxis />
                            <Tooltip />
                            <ReferenceLine
                                y={currentUser.overallScore}
                                stroke="#3b82f6"
                                strokeDasharray="4 4"
                                label="You"
                            />

                            <Bar dataKey="overallScore" radius={[6, 6, 0, 0]}>
                                {top5.map((entry, index) => (
                                    <Cell
                                        key={index}
                                        fill={
                                            entry.userId === loggedUser.userId
                                                ? "#3b82f6"
                                                : "#f59e0b"
                                        }
                                    />
                                ))}
                                <LabelList dataKey="overallScore" position="top" />
                            </Bar>
                        </BarChart>
                    </ResponsiveContainer>

                    {/* Insight */}
                    <div className="mt-3 text-xs text-slate-500">
                        You are ranked <b>#{currentUser.rank}</b> out of {data.length} participants.
                    </div>
                </div>

            </div>
            <div className="bg-white rounded-xl shadow p-6">
                <div className="flex justify-between items-center mb-4">
                    <h3 className="font-semibold text-slate-700">
                        Global Leaderboard
                    </h3>
                    <span className="text-xs text-slate-500">
                        Total participants: {data.length}
                    </span>
                </div>
                <div className="overflow-y-auto max-h-[420px] no-scrollbar">
                    <table className="w-full text-sm">
                        <thead className="bg-slate-100 text-slate-600 sticky top-0">
                            <tr>
                                <th className="p-3 text-left">Rank</th>
                                <th className="p-3 text-left">User</th>
                                <th className="p-3 text-center">Overall</th>
                                <th className="p-3 text-center">Interview</th>
                                <th className="p-3 text-center">Submissions</th>
                                <th className="p-3 text-center">Last Active</th>
                            </tr>
                        </thead>

                        <tbody>
                            {data.map(u => {
                                const isMe = u.userId === loggedUser.userId;

                                return (
                                    <tr
                                        key={u.rank}
                                        className={`border-b transition ${isMe
                                            ? "bg-indigo-50 font-semibold"
                                            : "hover:bg-slate-50"
                                            }`}
                                    >
                                        <td className="p-3">
                                            {u.rank === 1 && "🥇"}
                                            {u.rank === 2 && "🥈"}
                                            {u.rank === 3 && "🥉"}
                                            {u.rank > 3 && `#${u.rank}`}
                                        </td>
                                        <td className="p-3">
                                            {u.userName}
                                            {isMe && (
                                                <span className="ml-2 text-xs text-indigo-600">(You)</span>
                                            )}
                                        </td>
                                        <td className="p-3 text-center">
                                            <div className="flex items-center gap-2 justify-center">
                                                <span>{u.overallScore}</span>
                                                <div className="w-20 h-2 bg-slate-200 rounded">
                                                    <div
                                                        className="h-2 rounded bg-green-500"
                                                        style={{ width: `${u.overallScore}%` }}
                                                    />
                                                </div>
                                            </div>
                                        </td>
                                        <td className="p-3 text-center">
                                            {u.bestInterviewScore}
                                        </td>
                                        <td className="p-3 text-center">
                                            {u.submissions}
                                        </td>
                                        <td className="p-3 text-center text-xs text-slate-500">
                                            {u.lastActive
                                                ? new Date(u.lastActive).toLocaleDateString()
                                                : "-"}
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
}

function KpiCard({ title, value, icon: Icon, gradient }) {
  return (
    <div
      className={`
        relative
        overflow-hidden
        rounded-2xl
        bg-gradient-to-br ${gradient}
        text-black
        shadow-lg
        p-6
        transition-all duration-300
        hover:scale-[1.02]
        hover:shadow-xl
      `}
    >
      <div className="
        absolute inset-0
        bg-white/10
        opacity-0
        hover:opacity-100
        transition
      " />
      <div className="
        absolute right-4 top-4
        bg-white/20
        backdrop-blur
        p-3
        rounded-xl
      ">
        <Icon size={20} />
      </div>
      <div className="relative z-10">

        <p className="text-sm opacity-90 font-medium">
          {title}
        </p>

        <p className="
          text-3xl
          font-bold
          mt-1
          tracking-tight
        ">
          {value}
        </p>

      </div>
    </div>
  );
}


