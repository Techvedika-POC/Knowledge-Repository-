import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import api from "../api";
import CodingWorkspace from "./CodingWorkSpace";
import { motion } from "framer-motion";
import { FaFlagCheckered, FaListUl, FaClock, FaSignal } from "react-icons/fa";
export default function CodingChallengePage() {
    const { eventId } = useParams();
    const [problems, setProblems] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (eventId) loadProblems();
    }, [eventId]);

    const loadProblems = async () => {
        try {
            const res = await api.get(`/coding/challenges/by-event/${eventId}`);
            setProblems(res.data || []);
        } catch (err) {
            console.error(err);
            alert("Failed to load coding problems");
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-slate-900 flex items-center justify-center">
                <motion.div
                    className="text-white text-xl"
                    animate={{ opacity: [0.3, 1, 0.3] }}
                    transition={{ repeat: Infinity, duration: 1.5 }}
                >
                    Initializing Coding Arena...
                </motion.div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-slate-100">

            <div className="relative bg-blue  text-black overflow-hidden">
                <div className="absolute -top-20 -left-20 w-64 h-64 bg-white/10 rounded-full blur-3xl" />
                <div className="absolute top-10 right-10 w-64 h-64 bg-white/10 rounded-full blur-3xl" />

                <div className="relative max-w-7xl mx-auto px-10 py-2">
                    <h2 className="text-2xl font-bold tracking-tight mb-1">
                        AI Coding Challenge
                    </h2>
                    <p className="text-black  max-w-xl">
                        Solve real-world engineering problems under live contest conditions.
                    </p>

                    {/* STATS BAR */}
                    <div className="mt-2 grid grid-cols-1 sm:grid-cols-3 gap-6 max-w-2xl text-black">
                        <GlassStat icon={<FaListUl className="text-black" />} label="Problems" value={problems.length} />
                        <GlassStat icon={<FaSignal className="text-black" />} label="Difficulty" value="Mixed" />
                        <GlassStat icon={<FaClock className="text-black" />} label="Time Limit" value="90 min" />
                    </div>

                </div>
            </div>

            <div className="bg-white border-b">
                <div className="max-w-7xl mx-auto px-10 py-2 flex justify-between text-sm text-black">
                    <span>Contest Status: Active</span>
                    <span>Progress: 0 / {problems.length} solved</span>
                </div>
            </div>
            <div className="max-w-7xl mx-auto px-8 ">
                {problems.length > 0 ? (
                    <motion.div
                        className="bg-white rounded-xl shadow-lg border overflow-hidden"
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                    >
                        <CodingWorkspace problems={problems} />
                    </motion.div>
                ) : (
                    <div className="bg-white p-12 rounded-xl text-center text-black shadow">
                        No coding problems found for this event
                    </div>
                )}
            </div>
        </div>
    );
}

function GlassStat({ icon, label, value }) {
    return (
        <motion.div
            whileHover={{ y: -4 }}
            className="
        bg-white/20 backdrop-blur-lg
        rounded-xl p-5
        border border-white/30
        shadow-lg
        flex items-center gap-4
      "
        >
            <div className="text-2xl text-white">
                {icon}
            </div>
            <div>
                <div className="text-xs text-black font-bold">
                    {label}
                </div>
                <div className="text-x ">
                    {value}
                </div>
            </div>
        </motion.div>
    );
}

