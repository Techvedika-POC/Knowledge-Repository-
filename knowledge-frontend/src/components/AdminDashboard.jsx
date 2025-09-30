// AdminDashboard.jsx
import React from "react";

export default function AdminDashboard() {
  return (
    <div className="p-6 min-h-screen font-inter">
      <h1 className="text-2xl font-semibold mb-6">Admin Dashboard</h1>

      <div className="grid grid-cols-2 gap-6">
        {/* Role Management */}
        <div className="p-4 bg-white rounded-xl shadow">
          <h2 className="font-semibold mb-2">Roles</h2>
          <button className="px-3 py-1 rounded-full bg-blue-500 text-white hover:bg-blue-600">
            Add Role
          </button>
        </div>

        {/* Department Management */}
        <div className="p-4 bg-white rounded-xl shadow">
          <h2 className="font-semibold mb-2">Departments</h2>
          <button className="px-3 py-1 rounded-full bg-blue-500 text-white hover:bg-blue-600">
            Add Department
          </button>
        </div>

        {/* Events */}
        <div className="p-4 bg-white rounded-xl shadow">
          <h2 className="font-semibold mb-2">Events</h2>
          <button className="px-3 py-1 rounded-full bg-blue-500 text-white hover:bg-blue-600">
            Add Event
          </button>
        </div>

        {/* Domains & Categories */}
        <div className="p-4 bg-white rounded-xl shadow">
          <h2 className="font-semibold mb-2">Domains & Categories</h2>
          <button className="px-3 py-1 rounded-full bg-blue-500 text-white hover:bg-blue-600">
            Add Domain/Category
          </button>
        </div>

        {/* Mentor Assignment (for Ideathon events) */}
        <div className="p-4 bg-white rounded-xl shadow col-span-2">
          <h2 className="font-semibold mb-2">Mentor Assignment</h2>
          <button className="px-3 py-1 rounded-full bg-blue-500 text-white hover:bg-blue-600">
            Assign Mentor
          </button>
        </div>
      </div>
    </div>
  );
}
