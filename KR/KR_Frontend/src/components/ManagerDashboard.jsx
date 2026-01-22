import React, { useEffect, useState } from "react";
import api from "../api";

export default function ManagerDashboard() {
  const [plans, setPlans] = useState([]);
  const [users, setUsers] = useState([]);
  const [selectedPlan, setSelectedPlan] = useState("");
  const [progressData, setProgressData] = useState([]);

  const [assignOpen, setAssignOpen] = useState(false);
  const [assignPlanId, setAssignPlanId] = useState("");
  const [selectedUsers, setSelectedUsers] = useState([]);

  // ---------------- LOAD PLANS ----------------
  const fetchPlans = async () => {
    const res = await api.get("/LearningPlan");
    const data = res.data || [];
    setPlans(data);

    // Auto select first plan
    if (data.length > 0) {
      setSelectedPlan(data[0].planId);
    }
  };

  // ---------------- LOAD USERS ----------------
  const fetchUsers = async () => {
    const res = await api.get("/users");
    setUsers(res.data || []);
  };

  // ---------------- LOAD PROGRESS ----------------
  const fetchProgress = async (planId) => {
    if (!planId) return;
    const res = await api.get(`/manager/plan/${planId}/progress`);
    setProgressData(res.data || []);
  };

  useEffect(() => {
    fetchPlans();
    fetchUsers();
  }, []);

  useEffect(() => {
    if (selectedPlan) fetchProgress(selectedPlan);
  }, [selectedPlan]);

  // ---------------- ASSIGN ----------------
  const toggleUser = (id) => {
    setSelectedUsers(prev =>
      prev.includes(id)
        ? prev.filter(x => x !== id)
        : [...prev, id]
    );
  };

  const assignPlan = async () => {
    await api.post("/manager/assign-plan", {
      planId: assignPlanId,
      userIds: selectedUsers
    });

    alert("Plan assigned successfully");
    setAssignOpen(false);
    setSelectedUsers([]);
    fetchProgress(selectedPlan);
  };

  // ---------------- KPI METRICS ----------------
  const avgProgress =
    progressData.length === 0
      ? 0
      : Math.round(
          progressData.reduce((a, b) => a + b.progressPercent, 0) /
            progressData.length
        );

  const completedCount = progressData.filter(
    p => p.planStatus === "Completed"
  ).length;

  return (
    <div className="max-w-7xl mx-auto p-6 space-y-6">

      <h1 className="text-2xl font-bold">
        Manager Learning Dashboard
      </h1>

      {/* PLAN SELECT */}
      <div className="flex items-center justify-between bg-white p-4 rounded shadow">
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
          onClick={() => {
            setAssignPlanId(selectedPlan);
            setAssignOpen(true);
          }}
          className="bg-indigo-600 text-white px-4 py-2 rounded"
        >
          Assign Plan
        </button>
      </div>

      {/* KPI CARDS */}
      <div className="grid grid-cols-3 gap-4">
        <MetricCard label="Assigned Users" value={progressData.length} />
        <MetricCard label="Completed" value={completedCount} />
        <MetricCard label="Avg Progress" value={`${avgProgress}%`} />
      </div>

      {/* PROGRESS TABLE */}
      <div className="bg-white shadow rounded p-4">
        <h2 className="text-lg font-semibold mb-3">
          Team Progress Tracking
        </h2>

        <table className="w-full text-sm border">
          <thead className="bg-gray-100">
            <tr>
              <th className="p-2 text-left">User</th>
              <th>Status</th>
              <th>Progress</th>
              <th>Modules</th>
              <th>Assessment</th>
              <th>Passed</th>
              <th>Started</th>
              <th>Completed</th>
            </tr>
          </thead>
          <tbody>
            {progressData.map(p => (
              <tr key={p.userId} className="border-t text-center">
                <td className="p-2 text-left font-medium">
                  {p.userName}
                </td>
                <td>{p.planStatus}</td>
                <td>
                  <ProgressBar value={p.progressPercent} />
                </td>
                <td>
                  {p.completedModules}/{p.totalModules}
                </td>
                <td>{p.latestAssessmentScore ?? "-"}</td>
                <td>
                  {p.passed == null ? "-" : p.passed ? "yes" : "No"}
                </td>
                <td>
                  {p.startedOn
                    ? new Date(p.startedOn).toLocaleDateString()
                    : "-"}
                </td>
                <td>
                  {p.completedOn
                    ? new Date(p.completedOn).toLocaleDateString()
                    : "-"}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* ASSIGN MODAL */}
      {assignOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center">
          <div className="bg-white rounded-xl p-6 w-96 space-y-4">

            <h2 className="font-bold text-lg">
              Assign Learning Plan
            </h2>

            <div className="text-sm text-gray-600">
              Select users to assign:
            </div>

            <div className="border p-2 max-h-48 overflow-y-auto">
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
              <button
                onClick={() => setAssignOpen(false)}
                className="px-3 py-1"
              >
                Cancel
              </button>

              <button
                onClick={assignPlan}
                className="bg-indigo-600 text-white px-4 py-1 rounded"
                disabled={selectedUsers.length === 0}
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

/* ---------------- SMALL COMPONENTS ---------------- */

function MetricCard({ label, value }) {
  return (
    <div className="bg-white p-4 rounded shadow text-center">
      <div className="text-sm text-gray-500">{label}</div>
      <div className="text-2xl font-bold">{value}</div>
    </div>
  );
}

function ProgressBar({ value }) {
  return (
    <div className="w-full bg-gray-200 rounded h-3">
      <div
        className="bg-green-500 h-3 rounded"
        style={{ width: `${value}%` }}
      />
    </div>
  );
}
