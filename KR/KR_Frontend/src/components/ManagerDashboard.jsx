import React, { useEffect, useState } from "react";
import api from "../api";
import {
  Users,
  CheckCircle2,
  TrendingUp,
  Activity,
  Calendar
} from "lucide-react";

export default function ManagerDashboard() {
  const [plans, setPlans] = useState([]);
  const [users, setUsers] = useState([]);
  const [selectedPlan, setSelectedPlan] = useState("");
  const [progressData, setProgressData] = useState([]);
  const [assignOpen, setAssignOpen] = useState(false);
  const [selectedUsers, setSelectedUsers] = useState([]);

  useEffect(() => {
    loadPlans();
    loadUsers();
  }, []);

  useEffect(() => {
    if (selectedPlan) loadProgress(selectedPlan);
  }, [selectedPlan]);

  const loadPlans = async () => {
    const res = await api.get("/LearningPlan");
    const data = res.data || [];
    setPlans(data);
    if (data.length) setSelectedPlan(data[0].planId);
  };

  const loadUsers = async () => {
    const res = await api.get("/users");
    setUsers(res.data || []);
  };

  const loadProgress = async (planId) => {
    const res = await api.get(`/manager/plan/${planId}/progress`);
    setProgressData(res.data || []);
  };

  const toggleUser = (id) => {
    setSelectedUsers(prev =>
      prev.includes(id)
        ? prev.filter(x => x !== id)
        : [...prev, id]
    );
  };

  const assignPlan = async () => {
    await api.post("/manager/assign-plan", {
      planId: selectedPlan,
      userIds: selectedUsers
    });
    setAssignOpen(false);
    setSelectedUsers([]);
    loadProgress(selectedPlan);
  };


  const assignedUsers = progressData.length;

  const completedUsers = progressData.filter(
    p => p.planStatus === "Completed"
  ).length;

  const activeLearners = progressData.filter(
    p => p.planStatus !== "Completed"
  ).length;

  const avgProgress =
    progressData.length === 0
      ? 0
      : Math.round(
          progressData.reduce(
            (sum, p) => sum + Number(p.progressPercent || 0),
            0
          ) / progressData.length
        );


  return (
    <div className="max-w-7xl mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Learning Analytics Dashboard</h1>
        <p className="text-sm text-gray-500">
          Monitor team learning performance
        </p>
      </div>
      <div className="flex justify-between bg-white p-4 rounded-xl shadow">
        <div>
          <label className="text-sm font-medium text-gray-600">
            Learning Plan
          </label>
          <select
            className="border p-2 rounded w-72 ml-2"
            value={selectedPlan}
            onChange={(e) => setSelectedPlan(e.target.value)}
          >
            {plans.map(p => (
              <option key={p.planId} value={p.planId}>
                {p.title}
              </option>
            ))}
          </select>
        </div>

        <button
          onClick={() => setAssignOpen(true)}
          className="bg-indigo-600 text-white px-4 py-2 rounded-lg"
        >
          Assign Plan
        </button>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <KpiCard icon={<Users />} label="Assigned Users" value={assignedUsers} color="blue" />
        <KpiCard icon={<CheckCircle2 />} label="Completed" value={completedUsers} color="green" />
        <KpiCard icon={<TrendingUp />} label="Avg Progress" value={`${avgProgress}%`} color="indigo" />
        <KpiCard icon={<Activity />} label="Active Learners" value={activeLearners} color="orange" />
      </div>
      <div className="bg-white shadow rounded-xl p-4">
        <div className="flex justify-between mb-3">
          <h2 className="text-lg font-semibold">Team Progress</h2>
          <div className="text-sm text-gray-500 flex gap-2">
            <Calendar size={14} />
            {new Date().toLocaleString()}
          </div>
        </div>

        <table className="w-full text-sm border">
          <thead className="bg-gray-100">
            <tr>
              <th className="p-2 text-left">User</th>
              <th>Status</th>
              <th>Progress</th>
              <th>Modules</th>
              <th>Assessment</th>
              <th>Passed</th>
            </tr>
          </thead>

          <tbody>
            {progressData.map(p => (
              <tr key={p.userId} className="border-t text-center">
                <td className="p-2 text-left font-medium">{p.userName}</td>
                <td><StatusBadge status={p.planStatus} /></td>
                <td><ProgressBar value={p.progressPercent} /></td>
                <td>{p.completedModules}/{p.totalModules}</td>
                <td>{p.latestAssessmentScore ?? "-"}</td>
                <td>{p.passed == null ? "-" : p.passed ? "Yes" : "No"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {assignOpen && (
        <div className="fixed inset-0 bg-black/40 flex justify-center items-center">
          <div className="bg-white rounded-xl p-6 w-96">
            <h2 className="font-bold mb-3">Assign Learning Plan</h2>

            <div className="max-h-48 overflow-y-auto border p-2 mb-3">
              {users.map(u => (
                <label key={u.userId} className="flex gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={selectedUsers.includes(u.userId)}
                    onChange={() => toggleUser(u.userId)}
                  />
                  {u.name}
                </label>
              ))}
            </div>

            <div className="flex justify-end gap-2">
              <button onClick={() => setAssignOpen(false)}>Cancel</button>
              <button
                disabled={!selectedUsers.length}
                onClick={assignPlan}
                className="bg-indigo-600 text-white px-4 py-1 rounded"
              >
                Assign
              </button>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}

function KpiCard({ icon, label, value, color }) {
  const colors = {
    blue: "bg-blue-50 text-blue-600",
    green: "bg-green-50 text-green-600",
    indigo: "bg-indigo-50 text-indigo-600",
    orange: "bg-orange-50 text-orange-600"
  };

  return (
    <div className="bg-white rounded-xl shadow p-4 flex justify-between">
      <div>
        <div className="text-sm text-gray-500">{label}</div>
        <div className="text-2xl font-bold">{value}</div>
      </div>
      <div className={`p-3 rounded-xl ${colors[color]}`}>
        {icon}
      </div>
    </div>
  );
}

function ProgressBar({ value }) {
  return (
    <div className="w-full bg-gray-200 rounded-full h-2">
      <div
        className="h-2 rounded-full bg-gradient-to-r from-blue-500 to-indigo-500"
        style={{ width: `${Number(value || 0)}%` }}
      />
    </div>
  );
}

function StatusBadge({ status }) {
  const styles = {
    Assigned: "bg-gray-100 text-gray-600",
    InProgress: "bg-blue-100 text-blue-700",
    Completed: "bg-green-100 text-green-700"
  };

  return (
    <span className={`px-2 py-1 rounded-full text-xs ${styles[status]}`}>
      {status}
    </span>
  );
}
