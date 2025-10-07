import React from "react";
import { Search, Calendar } from "lucide-react";

export default function MyContributions() {
  const contributions = [
    { title: "Zero Trust Security Playbook", category: "Security", date: "2025-09-02", status: "Approved" },
    { title: "GraphQL Gateway Patterns", category: "APIs", date: "2025-08-28", status: "Pending" },
    { title: "Data Contracts 101", category: "Data", date: "2025-08-11", status: "Rejected" },
    { title: "Observability Primer", category: "SRE", date: "2025-07-30", status: "Approved" },
  ];

  const metrics = {
    knowledgeItems: 42,
    pendingReviews: 6,
    approved: 31,
    rejected: 5,
  };

  const submissions = [
    { domain: "Security", value: 72 },
    { domain: "APIs", value: 54 },
    { domain: "Data", value: 36 },
    { domain: "SRE", value: 28 },
  ];

  const getStatusStyle = (status) => {
    switch (status) {
      case "Approved":
        return "bg-green-100 text-green-600";
      case "Pending":
        return "bg-yellow-100 text-yellow-600";
      case "Rejected":
        return "bg-red-100 text-red-600";
      default:
        return "bg-gray-100 text-gray-600";
    }
  };

  return (
    <div className="p-6 bg-gray-50 min-h-screen font-inter">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-[22px] font-semibold">My Contributions</h1>
        <button className="bg-[#22c55e] text-white px-4 py-2 rounded-full hover:bg-[#16a34a] transition">
          New Upload
        </button>
      </div>

      {/* Info bar */}
      <div className="bg-[#fde68a] text-gray-900 p-3 rounded-lg flex justify-between items-center mb-4">
        <span>All uploads made by you. Use quick filters for Category, Title, and Date.</span>
      </div>

      {/* Filters */}
      <div className="flex gap-3 mb-6">
        <div className="flex items-center gap-2 border border-gray-300 rounded-full px-3 py-2 bg-white">
          <Search size={16} className="text-gray-500" />
          <input type="text" placeholder="Search by Title" className="outline-none text-sm" />
        </div>
        <div className="flex items-center gap-2 border border-gray-300 rounded-full px-3 py-2 bg-white">
          <Search size={16} className="text-gray-500" />
          <input type="text" placeholder="Search by Category" className="outline-none text-sm" />
        </div>
        <div className="flex items-center gap-2 border border-gray-300 rounded-full px-3 py-2 bg-white">
          <Calendar size={16} className="text-gray-500" />
          <input type="date" className="outline-none text-sm" />
        </div>
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl shadow-md overflow-hidden mb-8">
        <table className="w-full text-sm">
          <thead className="bg-[#fde68a] text-gray-700 text-left">
            <tr>
              <th className="px-4 py-3">Title</th>
              <th className="px-4 py-3">Category</th>
              <th className="px-4 py-3">Date</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3">Actions</th>
            </tr>
          </thead>
          <tbody>
            {contributions.map((c, i) => (
              <tr key={i} className="border-t">
                <td className="px-4 py-3">{c.title}</td>
                <td className="px-4 py-3">{c.category}</td>
                <td className="px-4 py-3">{c.date}</td>
                <td className="px-4 py-3">
                  <span className={`px-3 py-1 text-xs font-medium rounded-full ${getStatusStyle(c.status)}`}>
                    {c.status}
                  </span>
                </td>
                <td className="px-4 py-3 flex gap-2">
                  <button className="px-3 py-1 text-sm bg-gray-200 rounded-full hover:bg-gray-300">View</button>
                  <button className="px-3 py-1 text-sm bg-yellow-300 rounded-full hover:bg-yellow-400">Edit</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Metrics Heading */}
      <h2 className="text-lg font-semibold mb-4">Your Contribution Metrics</h2>

      {/* Metrics */}
      <div className="grid grid-cols-4 gap-4">
        <div className="bg-[#fef3c7] rounded-xl p-4 text-center">
          <p className="text-gray-500 text-sm">Knowledge Items</p>
          <p className="text-2xl font-semibold">{metrics.knowledgeItems}</p>
        </div>
        <div className="bg-[#ffe4e6] rounded-xl p-4 text-center">
          <p className="text-gray-500 text-sm">Pending Reviews</p>
          <p className="text-2xl font-semibold">{metrics.pendingReviews}</p>
        </div>
        <div className="bg-[#dcfce7] rounded-xl p-4 text-center">
          <p className="text-gray-500 text-sm">Approved</p>
          <p className="text-2xl font-semibold">{metrics.approved}</p>
        </div>
        <div className="bg-[#fef9c3] rounded-xl p-4 text-center">
          <p className="text-gray-500 text-sm">Rejected</p>
          <p className="text-2xl font-semibold">{metrics.rejected}</p>
        </div>
      </div>

      {/* Submissions by Domain */}
      <div className="mt-8">
        <h2 className="text-lg font-semibold mb-4">Submissions by Domain</h2>
        <div className="space-y-4">
          {submissions.map((s, i) => (
            <div key={i}>
              <div className="flex justify-between text-sm mb-1">
                <span>{s.domain}</span>
                <span>{s.value}</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="bg-yellow-500 h-2 rounded-full" style={{ width: `${s.value}%` }}></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
