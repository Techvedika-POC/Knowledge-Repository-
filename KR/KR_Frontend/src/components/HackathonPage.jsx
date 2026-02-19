import React, { useEffect, useState, useRef } from "react";
import api from "../api";
import {
  RadarChart, PolarGrid, PolarAngleAxis, Radar,
  ResponsiveContainer
} from "recharts";
import {
  LayoutDashboard,
  Lightbulb,
  Bot,
  Trophy,
  BarChart3,
  Brain,
  Send
} from "lucide-react";
import {
  TrendingUp, Star, TrendingDown
} from "lucide-react";
import { Rocket, Users, Calendar, ArrowRight } from "lucide-react";
import { Flag, Activity, Clock, Plus, Trash2 } from "lucide-react";
import { toast } from "react-hot-toast";
import { CheckCircle, Target, Zap, AlertTriangle } from "lucide-react";

export default function HackathonPlatform() {

  const [view, setView] = useState("home");

  const [events, setEvents] = useState([]);
  const [selectedEvent, setSelectedEvent] = useState(null);

  const [teamName, setTeamName] = useState("");
  const [emails, setEmails] = useState("");

  const [idea, setIdea] = useState("");

  const [chat, setChat] = useState([]);
  const [message, setMessage] = useState("");
  const [skillUpdating, setSkillUpdating] = useState(false);
  const lastSkillSignalRef = useRef(0);

  const [radar, setRadar] = useState([]);
  const [rankings, setRankings] = useState([]);
  const [skills, setSkills] = useState([]);
  const [eventInsights, setEventInsights] = useState(null);
  const [myTeam, setMyTeam] = useState(null);
  const [totalTeams, setTotalTeams] = useState(0);
  const userId = localStorage.getItem("userId");
  const [ideaTitle, setIdeaTitle] = useState("");
  const [ideaDescription, setIdeaDescription] = useState("");
  const [repoUrl, setRepoUrl] = useState("");
  const [demoUrl, setDemoUrl] = useState("");
  const [isIdeaSubmitted, setIsIdeaSubmitted] = useState(false);
  const [submittedIdea, setSubmittedIdea] = useState(null);
  const [summary, setSummary] = useState({
    learningVelocity: "-",
    strengthArea: "-",
    growthArea: "-"
  });

  useEffect(() => {
    loadHackathons();
    loadTotalTeams();
  }, []);

  useEffect(() => {
    if (!selectedEvent) return;

    if (view === "analytics") loadRadar();
    if (view === "rankings") loadRankings(selectedEvent.eventId);

    if (view === "skills") {
      Promise.all([loadSkills(), loadSkillSummary()]);
    }

  }, [view, selectedEvent]);
  const [showAdd, setShowAdd] = useState(false);
  const [inviteEmail, setInviteEmail] = useState("");
  const loadSkillSummary = async () => {
    const res = await fetch(`/api/skills/summary/${userId}`);
    const data = await res.json();
    setSummary(data);
  };

  const addMember = async () => {
    await api.post(
      `/team/${myTeam.teamId}/add-member`,
      null,
      {
        params: {
          creatorId: userId,
          email: inviteEmail
        }
      }
    );

    toast.success("Member added");
    setShowAdd(false);
    loadMyTeam(selectedEvent.eventId);
  };
  const removeMember = async (memberId) => {
    await api.delete(
      `/team/${myTeam.teamId}/remove-member/${memberId}`,
      {
        params: { creatorId: userId }
      }
    );

    toast.success("Member removed");
    loadMyTeam(selectedEvent.eventId);
  };

  const loadHackathons = async () => {
    const res = await api.get("/Events/type/Hackathon");
    setEvents(res.data || []);
  };
  const today = new Date();

  const activeHackathons = events.filter(e => {
    const start = new Date(e.startDate);
    const end = new Date(e.endDate);
    return start <= today && end >= today;
  });

  const openEvents = events.filter(e => {
    if (!e.registrationCloseDate) return true;
    return new Date(e.registrationCloseDate) >= today;
  });

  const thisMonth = today.getMonth();
  const thisYear = today.getFullYear();

  const monthStart = new Date(thisYear, thisMonth, 1);
  const monthEnd = new Date(thisYear, thisMonth + 1, 0);

  const monthlyEvents = events.filter(e => {
    const start = new Date(e.startDate);
    const end = new Date(e.endDate);
    return start <= monthEnd && end >= monthStart;
  });
  const loadMyTeam = async (eventId) => {
    const userId = localStorage.getItem("userId");

    try {
      const res = await api.get(
        `/team/my/${eventId}/${userId}`
      );

      setMyTeam(res.data);
    } catch {
      setMyTeam(null);
    }
  };
  const loadMyIdea = async () => {
    try {
      const res = await api.get(
        `/hackathon/idea/by-team/${myTeam.teamId}`
      );

      if (res.data) {
        setSubmittedIdea(res.data);
        setIdeaTitle(res.data.ideaTitle);
        setIdeaDescription(res.data.ideaDescription);
        setRepoUrl(res.data.repoUrl || "");
        setDemoUrl(res.data.demoUrl || "");
        setIsIdeaSubmitted(true);
      }
    } catch {
      setIsIdeaSubmitted(false);
    }
  };


  const loadTotalTeams = async () => {
    const res = await api.get("/Events/hackathons/teams/count");
    setTotalTeams(res.data.totalTeams);
  };
  useEffect(() => {
    if (myTeam) loadMyIdea();
  }, [myTeam]);

  const loadSkills = async () => {
    try {
      const res = await api.get(`/skills/${userId}`);
      setSkills(
        (res.data || []).map(s => ({
          skillName: s.skillName,
          proficiency: Number(s.proficiency || 0)
        }))
      );
    } catch {
      setSkills([]);
    }
  };

  const loadRadar = async () => {
    if (!myTeam) return;

    const res = await api.get(`/jury/radar/${myTeam.teamId}`);
    setRadar(res.data || []);
  };
  const loadRankings = async (eventId) => {
    const res = await api.get(`/jury/rankings/${eventId}`);
    setRankings(res.data || []);
  };
  const loadEventInsights = async (eventId) => {
    const res = await api.get(`/Events/${eventId}/insights`);
    setEventInsights(res.data.data);
  };
  const registerForEvent = async (event) => {
    const res = await api.get(
      `/EventRegistration/is-registered/${event.eventId}`
    );

    setSelectedEvent(event);

    await loadEventInsights(event.eventId);
    await loadMyTeam(event.eventId);   

    if (res.data.isRegistered) {
      setView("overview");
    } else {
      setView("register");
    }
  };


  const createTeam = async () => {
    const res = await api.post("/EventRegistration/register-team", {
      eventId: selectedEvent.eventId,
      teamName,
      teamMemberEmails: emails.split(",")
    });

    setMyTeam(res.data); 
    setView("overview");

  };
  const submitIdea = async () => {
    if (!ideaTitle || !ideaDescription)
      return toast.error("Title and description are required");

    try {
      await api.post("/hackathon/idea/submit", {
        eventId: selectedEvent.eventId,
        teamId: myTeam.teamId,
        title: ideaTitle,
        description: ideaDescription,
        repoUrl,
        demoUrl
      });

      toast.success("Idea submitted successfully");
      await loadSkills();
      await loadSkillSummary();
      await loadMyIdea();
      setIsIdeaSubmitted(true);

    } catch (err) {
      toast.error(err.response?.data || "Idea already submitted");
    }
  };

  const sendChat = async () => {
    setSkillUpdating(true);

    const res = await api.post("/ai/chat", {
      userId,
      eventId: selectedEvent.eventId,
      message
    });

    setChat(res.data);
    setMessage("");
    await Promise.all([
      loadSkills(),
      loadSkillSummary()
    ]);

    setSkillUpdating(false);
  };
  if (view === "home") {
    return (
      <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-slate-50 to-white px-10 py-4">

        {/* HERO */}
        <div className="max-w-6xl mx-auto mb-2 text-center">
          <div className="inline-flex items-center gap-2 bg-indigo-100 text-indigo-700 px-4 py-1 rounded-full text-sm mb-1">
            <Rocket size={16} />
            AI-Powered Innovation Platform
          </div>

          <h1 className="text-4xl font-bold text-slate-700 mb-2">
            Hackathon Innovation Hub
          </h1>

          <p className="text-slate-600 max-w-2xl mx-auto text-lg">
            Build real products, get AI mentorship, collaborate in teams,
            and compete in enterprise-grade hackathons.
          </p>
        </div>

        {/* KPI DASHBOARD */}
        <div className="max-w-7xl mx-auto mb-2">

          <h2 className="text-2xl font-bold text-slate-700 mb-2">
            Platform Overview
          </h2>

          {/* KPI DASHBOARD */}
          <div className="grid md:grid-cols-4 gap-6 mb-6">

            {/* Active Hackathons */}
            <div className="relative bg-white rounded-2xl shadow p-5 overflow-hidden">
              <div className="absolute left-0 top-0 h-full w-1 bg-indigo-600" />
              <div className="flex items-center gap-3">
                <div className="bg-indigo-100 p-2 rounded-lg">
                  <Activity className="text-indigo-600" />
                </div>
                <div>
                  <p className="text-xs text-slate-500">Active Hackathons</p>
                  <p className="text-2xl font-bold text-slate-800">
                    {activeHackathons.length}
                  </p>
                </div>
              </div>
              <p className="text-xs text-slate-400 mt-2">
                Currently running events
              </p>
            </div>

            {/* Open for Registration */}
            <div className="relative bg-white rounded-2xl shadow p-5 overflow-hidden">
              <div className="absolute left-0 top-0 h-full w-1 bg-green-600" />
              <div className="flex items-center gap-3">
                <div className="bg-green-100 p-2 rounded-lg">
                  <Calendar className="text-green-600" />
                </div>
                <div>
                  <p className="text-xs text-slate-500">Open for Registration</p>
                  <p className="text-2xl font-bold text-slate-800">
                    {openEvents.length}
                  </p>
                </div>
              </div>
              <p className="text-xs text-slate-400 mt-2">
                Accepting new teams
              </p>
            </div>

            {/* This Month */}
            <div className="relative bg-white rounded-2xl shadow p-5 overflow-hidden">
              <div className="absolute left-0 top-0 h-full w-1 bg-yellow-500" />
              <div className="flex items-center gap-3">
                <div className="bg-yellow-100 p-2 rounded-lg">
                  <Flag className="text-yellow-600" />
                </div>
                <div>
                  <p className="text-xs text-slate-500">This Month</p>
                  <p className="text-2xl font-bold text-slate-800">
                    {monthlyEvents.length}
                  </p>
                </div>
              </div>
              <p className="text-xs text-slate-400 mt-2">
                Ongoing hackathons
              </p>
            </div>

            {/* Total Teams */}
            <div className="relative bg-white rounded-2xl shadow p-5 overflow-hidden">
              <div className="absolute left-0 top-0 h-full w-1 bg-purple-600" />
              <div className="flex items-center gap-3">
                <div className="bg-purple-100 p-1 rounded-lg">
                  <Users className="text-purple-600" />
                </div>
                <div>
                  <p className="text-xs text-slate-500">Total Teams</p>
                  <p className="text-2xl font-bold text-slate-800">
                    {totalTeams}
                  </p>
                </div>
              </div>
              <p className="text-xs text-slate-400 mt-2">
                Across all hackathons
              </p>
            </div>

          </div>
        </div>

        {/* EVENT GRID */}
        <div className="max-w-7xl mx-auto">

          <div className="flex items-center justify-between mb-4">
            <h2 className="text-2xl font-bold text-slate-600">
              Available Hackathons
            </h2>
            <p className="text-sm text-slate-500">
              Select an event to register and start building
            </p>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {events.map(e => {
              const today = new Date();
              const start = new Date(e.startDate);
              const end = new Date(e.endDate);
              const regClose = e.registrationCloseDate
                ? new Date(e.registrationCloseDate)
                : null;

              const isOngoing = start <= today && end >= today;
              const isOpen = !regClose || regClose >= today;

              return (
                <div
                  key={e.eventId}
                  className="group bg-white rounded-2xl shadow-lg p-6 border border-slate-100 hover:shadow-2xl hover:-translate-y-1 transition-all duration-300"
                >
                  {/* Card Header */}
                  <div className="flex items-start justify-between mb-3">
                    <h3 className="text-xl font-bold text-indigo-700">
                      {e.title}
                    </h3>

                    <span className={`text-xs px-2 py-1 rounded-full ${isOngoing
                      ? "bg-green-100 text-green-700"
                      : isOpen
                        ? "bg-yellow-100 text-yellow-700"
                        : "bg-slate-200 text-slate-600"
                      }`}>
                      {isOngoing ? "Ongoing" : isOpen ? "Open" : "Closed"}
                    </span>
                  </div>

                  <p className="text-slate-600 text-sm mb-4 line-clamp-3">
                    {e.description}
                  </p>

                  {/* Meta */}
                  <div className="flex justify-between text-xs text-slate-500 mb-4">
                    <span className="flex items-center gap-1">
                      <Users size={14} /> Team Based
                    </span>
                    <span className="flex items-center gap-1">
                      <Clock size={14} />
                      {isOngoing
                        ? "In Progress"
                        : isOpen
                          ? "Upcoming"
                          : "Ended"}
                    </span>
                  </div>

                  {/* CTA */}
                  <button
                    disabled={!isOpen}
                    onClick={() => registerForEvent(e)}
                    className={`w-full flex items-center justify-center gap-2 py-2 rounded-lg font-semibold transition ${isOpen
                      ? "bg-indigo-600 text-white hover:bg-indigo-700"
                      : "bg-slate-300 text-slate-500 cursor-not-allowed"
                      }`}
                  >
                    {isOpen ? "Enter Hackathon" : "Registration Closed"}
                    <ArrowRight size={16} />
                  </button>
                </div>
              );
            })}
          </div>
        </div>

      </div>
    );

  }

  if (view === "register") {
    return (
      <div className="min-h-screen bg-slate-100 flex items-center justify-center">
        <div className="bg-white p-8 rounded-xl shadow-lg w-full max-w-md">
          <h2 className="text-2xl font-bold mb-4">
            Create Team for {selectedEvent.title}
          </h2>

          <input
            placeholder="Team Name"
            className="border p-3 w-full mb-3 rounded"
            value={teamName}
            onChange={e => setTeamName(e.target.value)}
          />

          <textarea
            placeholder="Member emails (comma separated)"
            className="border p-3 w-full mb-3 rounded"
            value={emails}
            onChange={e => setEmails(e.target.value)}
          />

          <button
            onClick={createTeam}
            className="w-full bg-indigo-600 text-white py-2 rounded-lg">
            Register Team
          </button>
        </div>
      </div>
    );
  }
  return (
    <div className="min-h-screen bg-slate-100 px-7 py-2">
      <div className="flex flex-wrap gap-3 mb-3">
        {[
          { key: "overview", label: "Overview", icon: LayoutDashboard },
          { key: "idea", label: "Idea", icon: Lightbulb },
          { key: "mentor", label: "Mentor", icon: Bot },
          { key: "analytics", label: "Analytics", icon: BarChart3 },
          { key: "rankings", label: "Rankings", icon: Trophy },
          { key: "skills", label: "Skills", icon: Brain }
        ].map(({ key, label, icon: Icon }) => (
          <button
            key={key}
            onClick={() => setView(key)}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg shadow-sm ${view === key
              ? "bg-indigo-600 text-white"
              : "bg-white text-slate-700"
              }`}
          >
            <Icon size={16} />
            {label}
          </button>
        ))}
      </div>
      {view === "overview" && (
        <div className="space-y-8">
          <div className="bg-white p-4 rounded-2xl shadow flex flex-col md:flex-row justify-between items-start md:items-center">
            <div>
              <h2 className="text-2xl font-bold text-indigo-700">
                {selectedEvent.title}
              </h2>
              <p className="text-slate-600 mt-2 max-w-2xl">
                {selectedEvent.description}
              </p>
            </div>

            <div className="mt-4 md:mt-0 flex gap-4 text-sm">
              <span className="bg-green-100 text-green-700 px-3 py-1 rounded-full">
                Active
              </span>
              <span className="bg-indigo-100 text-indigo-700 px-3 py-1 rounded-full">
                Team Based
              </span>
              <span className="bg-purple-100 text-purple-700 px-3 py-1 rounded-full">
                AI Enabled
              </span>
            </div>
          </div>
          <div className="grid md:grid-cols-3 gap-6">

            {/* Submit Idea */}
            <div className="bg-gradient-to-br from-indigo-50 to-white p-6 rounded-2xl shadow flex flex-col justify-between hover:shadow-xl transition">
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="bg-indigo-100 p-2 rounded-lg">
                    <Lightbulb className="text-indigo-600" />
                  </div>
                  <h3 className="font-bold text-lg">Submit Idea</h3>
                </div>

                <p className="text-slate-600 text-sm">
                  Validate your solution with AI feedback on feasibility,
                  innovation, and impact.
                </p>
              </div>

              <button
                onClick={() => setView("idea")}
                className="mt-6 w-full bg-indigo-600 hover:bg-indigo-700 text-white py-2 rounded-lg font-semibold"
              >
                Submit Idea
              </button>
            </div>

            {/* AI Mentor */}
            <div className="bg-gradient-to-br from-green-50 to-white p-6 rounded-2xl shadow flex flex-col justify-between hover:shadow-xl transition">
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="bg-green-100 p-2 rounded-lg">
                    <Bot className="text-green-600" />
                  </div>
                  <h3 className="font-bold text-lg">AI Mentor</h3>
                </div>

                <p className="text-slate-600 text-sm">
                  Get guidance on architecture, UX, pitching, and scaling.
                </p>
              </div>

              <button
                onClick={() => setView("mentor")}
                className="mt-6 w-full bg-green-600 hover:bg-green-700 text-white py-2 rounded-lg font-semibold"
              >
                Ask Mentor
              </button>
            </div>

            {/* Rankings */}
            <div className="bg-gradient-to-br from-yellow-50 to-white p-6 rounded-2xl shadow flex flex-col justify-between hover:shadow-xl transition">
              <div>
                <div className="flex items-center gap-3 mb-3">
                  <div className="bg-yellow-100 p-2 rounded-lg">
                    <Trophy className="text-yellow-600" />
                  </div>
                  <h3 className="font-bold text-lg">Rankings</h3>
                </div>

                <p className="text-slate-600 text-sm">
                  Track your team's performance against others in real time.
                </p>
              </div>

              <button
                onClick={() => setView("rankings")}
                className="mt-6 w-full bg-yellow-500 hover:bg-yellow-600 text-white py-2 rounded-lg font-semibold"
              >
                View Rankings
              </button>
            </div>

          </div>
          {myTeam && (
            <div className="bg-white rounded-2xl shadow-lg border border-slate-200 overflow-hidden">
              <div className="px-6 py-2 border-b flex justify-between items-center bg-slate-50">
                <div>
                  <h3 className="text-xl font-bold text-slate-800 flex items-center gap-2">
                    <Users size={18} /> Team Workspace
                  </h3>
                  <p className="text-sm text-slate-500">
                    Manage your hackathon team and members
                  </p>
                </div>

                <span className="px-3 py-1 rounded-full text-xs font-semibold bg-green-100 text-green-700">
                  Registered
                </span>
              </div>

              <div className="grid md:grid-cols-3 gap-6 p-6">
                <div className="space-y-4">
                  <div>
                    <p className="text-xs text-slate-500 uppercase tracking-wide">
                      Team Name
                    </p>
                    <p className="text-2xl font-semibold text-indigo-700">
                      {myTeam.teamName}
                    </p>
                  </div>

                  <div>
                    <p className="text-xs text-slate-500 uppercase tracking-wide">
                      Created On
                    </p>
                    <p className="text-slate-700">
                      {new Date(myTeam.createdOn).toLocaleString()}
                    </p>
                  </div>
                  <div className="bg-amber-50 border border-amber-200 text-amber-800 p-4 rounded-lg text-sm">
                    <b>Team Management Rules</b>
                    <br />
                    You can add or remove team members only until the registration
                    deadline. After that, team composition will be locked.
                  </div>
                </div>
                <div className="md:col-span-2 space-y-4">

                  <div className="flex justify-between items-center">
                    <p className="font-semibold text-slate-700 flex items-center gap-2">
                      <Users size={16} /> Team Members
                    </p>

                    {myTeam.createdBy === userId && (
                      <button
                        onClick={() => setShowAdd(true)}
                        className="flex items-center gap-1 text-sm bg-indigo-600 text-white px-3 py-1.5 rounded hover:bg-indigo-700"
                      >
                        <Plus size={14} /> Add Member
                      </button>
                    )}
                  </div>
                  <div className="space-y-3">
                    {Array.isArray(myTeam?.members) && myTeam.members.map(m => (

                      <div
                        key={m.userId}
                        className="flex justify-between items-center bg-slate-50 border border-slate-200 px-4 py-3 rounded-lg hover:shadow transition"
                      >
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-full bg-indigo-600 text-white flex items-center justify-center font-bold">
                            {m.name[0]}
                          </div>

                          <div>
                            <p className="font-semibold text-slate-800">
                              {m.name}
                            </p>
                            <p className="text-xs text-slate-500">
                              {m.email}
                            </p>
                          </div>
                        </div>
                        {myTeam.createdBy === userId &&
                          m.userId !== userId && (
                            <button
                              onClick={() => removeMember(m.userId)}
                              className="p-2 rounded-full hover:bg-red-100 text-red-600"
                              title="Remove member"
                            >
                              <Trash2 size={16} />
                            </button>
                          )}
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          )}
          {showAdd && (
            <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
              <div className="bg-white p-6 rounded-xl w-full max-w-md space-y-4 shadow-xl">

                <h3 className="text-lg font-bold flex items-center gap-2">
                  <Plus /> Add Team Member
                </h3>

                <p className="text-sm text-slate-500">
                  Enter the registered email of the user you want to invite.
                </p>

                <input
                  className="w-full border p-3 rounded"
                  placeholder="user@company.com"
                  value={inviteEmail}
                  onChange={e => setInviteEmail(e.target.value)}
                />

                <div className="bg-amber-50 border border-amber-200 text-amber-800 px-3 py-2 rounded text-xs">
                  You can modify team members only until the registration deadline.
                </div>

                <div className="flex justify-end gap-2">
                  <button
                    onClick={() => setShowAdd(false)}
                    className="px-4 py-2 text-slate-600"
                  >
                    Cancel
                  </button>

                  <button
                    onClick={addMember}
                    className="bg-indigo-600 text-white px-4 py-2 rounded"
                  >
                    Invite
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
      {view === "idea" && (
        <div className="bg-white p-8 rounded-xl shadow space-y-6 max-w-5xl">

          <h2 className="text-2xl font-bold flex items-center gap-2">
            <Lightbulb /> Idea Submission
          </h2>

          {/* STATUS BANNER */}
          {isIdeaSubmitted && (
            <div className="bg-green-50 border border-green-200 text-green-700 p-3 rounded text-sm">
              You have already submitted your idea for this hackathon. Submissions are locked.
            </div>
          )}

          <div className="grid grid-cols-2 gap-4">

            <div className="col-span-2">
              <label className="text-sm font-medium text-slate-600">
                Idea Title *
              </label>
              <input
                disabled={isIdeaSubmitted}
                className="w-full border p-3 rounded mt-1 disabled:bg-slate-100"
                placeholder="Cloud Health Monitor"
                value={ideaTitle}
                onChange={e => setIdeaTitle(e.target.value)}
              />
            </div>

            <div className="col-span-2">
              <label className="text-sm font-medium text-slate-600">
                Problem & Solution *
              </label>
              <textarea
                disabled={isIdeaSubmitted}
                rows={6}
                className="w-full border p-3 rounded mt-1 disabled:bg-slate-100"
                placeholder="Describe the problem, users, solution, differentiation..."
                value={ideaDescription}
                onChange={e => setIdeaDescription(e.target.value)}
              />
            </div>

            <div>
              <label className="text-sm font-medium text-slate-600">
                GitHub Repo (optional)
              </label>
              <input
                disabled={isIdeaSubmitted}
                className="w-full border p-3 rounded mt-1 disabled:bg-slate-100"
                placeholder="https://github.com/..."
                value={repoUrl}
                onChange={e => setRepoUrl(e.target.value)}
              />
            </div>

            <div>
              <label className="text-sm font-medium text-slate-600">
                Live Demo (optional)
              </label>
              <input
                disabled={isIdeaSubmitted}
                className="w-full border p-3 rounded mt-1 disabled:bg-slate-100"
                placeholder="https://demo.vercel.app"
                value={demoUrl}
                onChange={e => setDemoUrl(e.target.value)}
              />
            </div>

          </div>

          {/* VALIDATION */}
          {!isIdeaSubmitted && (!ideaTitle || !ideaDescription) && (
            <div className="bg-red-50 border border-red-200 text-red-700 p-3 rounded text-sm">
              Title and description are mandatory.
            </div>
          )}

          <button
            disabled={isIdeaSubmitted || !ideaTitle || !ideaDescription}
            onClick={submitIdea}
            className={`w-full py-3 rounded-lg font-semibold transition ${isIdeaSubmitted
              ? "bg-green-600 text-white cursor-not-allowed"
              : ideaTitle && ideaDescription
                ? "bg-indigo-600 text-white hover:bg-indigo-700"
                : "bg-slate-300 text-slate-500 cursor-not-allowed"
              }`}
          >
            {isIdeaSubmitted ? "Idea Submitted" : "Submit Idea to Jury"}
          </button>

        </div>
      )}
      {view === "mentor" && (
        <div className="bg-white rounded-xl shadow flex flex-col h-[500px]">

          <div className="border-b p-4 font-bold text-lg text-indigo-600 flex items-center gap-2">
            <Bot /> AI Hackathon Mentor
          </div>

          <div className="flex-1 overflow-y-auto p-4 space-y-3">
            {chat.length === 0 && (
              <p className="text-slate-400 text-center mt-20">
                Ask about product, architecture, or pitch strategy.
              </p>
            )}

            {chat.map((c, i) => (
              <div
                key={i}
                className={`max-w-[70%] px-4 py-2 rounded-lg text-sm ${c.role === "user"
                  ? "ml-auto bg-indigo-600 text-white"
                  : "mr-auto bg-slate-200 text-slate-800"
                  }`}
              >
                {c.message}
              </div>
            ))}
          </div>

          <div className="border-t p-4 flex gap-2">
            <input
              className="flex-1 border p-3 rounded"
              value={message}
              onChange={e => setMessage(e.target.value)}
              placeholder="Ask your AI mentor..."
            />
            <button
              onClick={sendChat}
              className="bg-indigo-600 text-white px-4 rounded flex items-center gap-1">
              <Send size={16} /> Send
            </button>
          </div>
        </div>
      )}
      {view === "analytics" && (
        <div className="space-y-6">
          <div className="bg-white p-6 rounded-xl shadow flex items-center gap-3">
            <BarChart3 className="text-indigo-600" />
            <div>
              <h2 className="font-bold text-xl">Team Performance Analytics</h2>
              <p className="text-slate-500 text-sm">
                Jury evaluation & AI-powered insights.
              </p>
            </div>
          </div>
          <div className="grid md:grid-cols-4 gap-4">
            <KpiCard
              title="Status"
              value={radar.length ? "Evaluated" : "Pending"}
              icon={CheckCircle}
              gradient="bg-gradient-to-br from-indigo-200 to-indigo-300"
            />
            <KpiCard
              title="Total Score"
              value={radar.reduce((a, b) => a + b.score, 0) || 0}
              icon={Target}
              gradient="bg-gradient-to-br from-green-200 to-green-300"
            />
            <KpiCard
              title="Strongest Area"
              value={radar.length
                ? [...radar].sort((a, b) => b.score - a.score)[0]?.criteriaName
                : "N/A"}
              icon={Zap}
              gradient="bg-gradient-to-br from-blue-200 to-blue-300"
            />
            <KpiCard
              title="Weakest Area"
              value={radar.length
                ? [...radar].sort((a, b) => a.score - b.score)[0]?.criteriaName
                : "N/A"}
              icon={AlertTriangle}
              gradient="bg-gradient-to-br from-red-200 to-red-300"
            />
          </div>
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">

            <div className="lg:col-span-2 bg-white p-8 rounded-2xl shadow">
              <h3 className="font-semibold mb-2 flex items-center gap-2">
                <BarChart3 size={18} className="text-indigo-600" />
                Capability Map
              </h3>

              {radar.length === 0 ? (
                <div className="text-center text-slate-400 py-14">
                  <Bot size={40} className="mx-auto mb-2 opacity-40" />
                  Jury has not evaluated your team yet
                </div>
              ) : (
                <ResponsiveContainer width="100%" height={320}>
                  <RadarChart data={radar}>
                    <PolarGrid />
                    <PolarAngleAxis dataKey="criteriaName" />
                    <Radar dataKey="score" fill="#6366f1" fillOpacity={0.7} />
                  </RadarChart>
                </ResponsiveContainer>
              )}
              <div className="bg-white/80 backdrop-blur border border-indigo-100 p-6 rounded-2xl shadow">
                <h3 className="font-semibold mb-2 flex items-center gap-2">
                  <Brain size={18} className="text-indigo-600" />
                  AI Strategic Insight
                </h3>

                {radar.length ? (
                  <ul className="list-disc ml-5 text-sm text-slate-700 space-y-1">
                    <li>
                      Your strongest capability is{" "}
                      <b className="text-green-700">
                        {[...radar].sort((a, b) => b.score - a.score)[0]?.criteriaName}
                      </b>
                    </li>
                    <li>
                      You should prioritize improving{" "}
                      <b className="text-red-700">
                        {[...radar].sort((a, b) => a.score - b.score)[0]?.criteriaName}
                      </b>
                    </li>
                    <li>
                      Focusing on differentiation will significantly improve ranking.
                    </li>
                  </ul>
                ) : (
                  <p className="text-slate-400">
                    AI insights will appear once jury evaluation is complete.
                  </p>
                )}
              </div>

            </div>
            <div className="lg:col-span-1 bg-white p-3 rounded-2xl shadow space-y-4">
              <h3 className="font-semibold flex items-center gap-2">
                <TrendingUp size={18} className="text-green-600" />
                Performance Breakdown
              </h3>

              {radar.map(r => {
                const percent = (r.score / 25) * 100;

                return (
                  <div key={r.criteriaName}>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="font-medium">{r.criteriaName}</span>
                      <span className="font-bold">{r.score}/25</span>
                    </div>

                    <div className="w-full bg-slate-200 rounded-full h-2">
                      <div
                        className={`h-2 rounded-full transition-all ${percent >= 70
                          ? "bg-green-400"
                          : percent >= 50
                            ? "bg-yellow-400"
                            : "bg-red-400"
                          }`}
                        style={{ width: `${percent}%` }}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}
      {view === "rankings" && (
        <div className="space-y-6">
          {/* Header */}
          <div className=" bg-white p-6 rounded-xl shadow flex items-center gap-3 text-black">
            <Trophy className="text-black" size={28} />
            <div>
              <h2 className="font-bold text-2xl">Global Leaderboard</h2>
              <p className="text-black/80 text-sm">
                Live rankings across all teams in this hackathon.
              </p>
            </div>
          </div>

          {/* Table */}
          <div className="bg-white rounded-xl shadow overflow-hidden">

            {/* Column Labels */}
            <div className="grid grid-cols-[80px_1fr_200px] px-6 py-3 text-xs uppercase tracking-wider text-slate-500 bg-slate-50 border-b sticky top-0 z-10">
              <span>Rank</span>
              <span>Team</span>
              <span className="text-right">Score</span>
            </div>

            {/* Rows */}
            {rankings.length === 0 ? (
              <div className="text-center text-slate-400 py-10">
                <Clock className="mx-auto mb-2" />
                Waiting for jury scores...
              </div>
            ) : (
              rankings.map((r, i) => {
                const medals = ["🥇", "🥈", "🥉"];

                return (
                  <div
                    key={i}
                    className={`
              grid grid-cols-[80px_1fr_200px] items-center px-6 py-4
              hover:bg-slate-50 transition
              ${i === 0 && "bg-yellow-50"}
            `}
                  >
                    {/* Rank */}
                    <div className="text-xl">
                      {i < 3 ? medals[i] : `#${i + 1}`}
                    </div>

                    {/* Team */}
                    <div className="flex items-center gap-3">
                      <div className="w-9 h-9 rounded-full bg-slate-200 flex items-center justify-center font-bold">
                        {r.teamName[0]}
                      </div>
                      <span className="font-medium">{r.teamName}</span>
                    </div>

                    {/* Score */}
                    <div className="flex items-center gap-3 justify-end">
                      <div className="w-full bg-slate-200 rounded-full h-2 overflow-hidden">
                        <div
                          className="bg-green-500 h-full"
                          style={{ width: `${r.totalScore}%` }}
                        />
                      </div>
                      <span className="font-bold flex items-center gap-1">
                        <TrendingUp size={14} />
                        {r.totalScore}
                      </span>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      )}
      {view === "skills" && (
        <div className="space-y-6">
          <div className="bg-white p-6 rounded-xl shadow flex items-center gap-3 text-black">
            <Brain size={28} />
            <div>
              <h2 className="font-bold text-2xl">Personal Skill Intelligence</h2>
              <p className="text-black/80 text-sm">
                AI-derived assessment based on your real hackathon behavior.
              </p>
            </div>
          </div>
          {skillUpdating && (
            <div className="text-xs text-slate-500 italic flex items-center gap-2">
              <Brain size={14} className="animate-pulse" />
              AI is learning from your activity…
            </div>
          )}
          <div className="grid md:grid-cols-3 gap-4">

            <div className="bg-green-50 p-4 rounded-xl flex gap-3 items-center">
              <Rocket className="text-green-600" />
              <div>
                <p className="text-xs text-slate-600">Learning Velocity</p>
                <p className="font-bold text-green-700">
                  {summary?.learningVelocity || "Analyzing..."}
                </p>
                <p className="text-xs text-slate-500">
                  Based on recent activity & AI analysis
                </p>
              </div>
            </div>

            <div className="bg-blue-50 p-4 rounded-xl flex gap-3 items-center">
              <Star className="text-blue-600" />
              <div>
                <p className="text-xs text-slate-600">Strength Area</p>
                <p className="font-bold">
                  {summary?.strengthArea || "-"}
                </p>
              </div>
            </div>

            <div className="bg-orange-50 p-4 rounded-xl flex gap-3 items-center">
              <TrendingDown className="text-orange-600" />
              <div>
                <p className="text-xs text-slate-600">Growth Area</p>
                <p className="font-bold">
                  {summary?.growthArea || "-"}
                </p>
              </div>
            </div>
          </div>
          <div className="bg-white p-6 rounded-xl shadow">
            {skills.length === 0 ? (
              <div className="text-center text-slate-400 py-10 space-y-2">
                <p className="font-medium">No skill intelligence yet</p>
                <p className="text-sm">
                  Your profile is built automatically as you:
                </p>
                <ul className="text-sm">
                  <li>• Submit ideas</li>
                  <li>• Collaborate in teams</li>
                  <li>• Interact with the AI mentor</li>
                  <li>• Get evaluated by jury</li>
                </ul>
              </div>
            ) : (
              <>
                <ResponsiveContainer width="100%" height={360}>
                  <RadarChart data={skills}>
                    <PolarGrid />
                    <PolarAngleAxis dataKey="skillName" />
                    <Radar
                      dataKey="proficiency"
                      stroke="#22c55e"
                      fill="#22c55e"
                      fillOpacity={0.55}
                      isAnimationActive
                    />
                  </RadarChart>
                </ResponsiveContainer>
                {summary?.aiInsight && (
                  <div className="mt-4 text-sm text-slate-600 italic">
                    🤖 AI Insight: {summary.aiInsight}
                  </div>
                )}
              </>
            )}
          </div>
          <div className="bg-white rounded-xl shadow overflow-hidden">
            <div className="px-6 py-3 text-xs uppercase tracking-wider text-slate-500 bg-slate-50 border-b">
              Skill Breakdown
            </div>

            {skills.length === 0 ? (
              <div className="text-center text-slate-400 py-6">
                No skills recorded yet.
              </div>
            ) : (
              skills.map((s, i) => (
                <div
                  key={i}
                  className="flex justify-between items-center px-6 py-3 border-b hover:bg-slate-50"
                >
                  <span className="font-medium">{s.skillName}</span>
                  <div className="flex items-center gap-3 w-40">
                    <div className="w-full bg-slate-200 rounded-full h-2">
                      <div
                        className="bg-green-500 h-2 rounded-full transition-all"
                        style={{ width: `${s.proficiency}%` }}
                      />
                    </div>
                    <span className="text-sm font-bold">
                      {Math.round(s.proficiency)}
                    </span>
                  </div>
                </div>
              ))
            )}
          </div>

        </div>
      )}
    </div>
  );
  function KpiCard({ title, value, icon: Icon, gradient }) {
    return (
      <div className={`p-4 rounded-xl shadow-sm text-black ${gradient}`}>
        <div className="flex items-center justify-between">
          <div>
            <p className="text-xs opacity-80">{title}</p>
            <p className="text-2xl font-bold">{value}</p>
          </div>
          <div className="bg-white/20 p-2 rounded-lg">
            <Icon size={20} />
          </div>
        </div>
      </div>
    );
  }


}
