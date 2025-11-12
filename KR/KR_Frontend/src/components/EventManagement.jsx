import React, { useState, useEffect } from "react";
import { toast, Toaster } from "react-hot-toast";
import api from "../api"; // Axios instance

/**
 * Event Types Dropdown Options
 */
const eventTypes = [
  "Hackathon",
  "Ideathon",
  "Coding Challenge",
  "Workshop",
  "Seminar",
  "Webinar",
];

export default function EventManagement() {
  const [activeTab, setActiveTab] = useState("add");
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(false);

  const emptyForm = {
    eventId: "",
    title: "",
    description: "",
    eventType: "",
    startDate: "",
    endDate: "",
    registrationCloseDate: "",
    mentorCheckpointStart: "",
    mentorCheckpointEnd: "",
    finalSubmissionDeadline: "",
    ideaPresentationStart: "",
    ideaPresentationEnd: "",
    winnersAnnouncementDate: "",
    contactEmail: "",
    notes: "",
  };

  const [form, setForm] = useState(emptyForm);

  /**
   * Fetch All Events
   */
  const fetchEvents = async () => {
    setLoading(true);
    try {
      const res = await api.get("/Events");
      setEvents(res.data);
    } catch {
      toast.error("Failed to load events.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (activeTab === "view") fetchEvents();
  }, [activeTab]);

  /**
   * Handle Form Input Changes
   */
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  /**
   * Submit Event (Add or Update)
   */
  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!form.title || !form.eventType || !form.startDate || !form.endDate) {
      toast.error("Title, Event Type, Start Date, and End Date are required.");
      return;
    }

    const normalizeDate = (date) => (date ? date : null);

    const payload = {
      title: form.title.trim(),
      description: form.description?.trim() || null,
      eventType: form.eventType,
      startDate: normalizeDate(form.startDate),
      endDate: normalizeDate(form.endDate),
      registrationCloseDate: normalizeDate(form.registrationCloseDate),
      mentorCheckpointStart: normalizeDate(form.mentorCheckpointStart),
      mentorCheckpointEnd: normalizeDate(form.mentorCheckpointEnd),
      finalSubmissionDeadline: normalizeDate(form.finalSubmissionDeadline),
      ideaPresentationStart: normalizeDate(form.ideaPresentationStart),
      ideaPresentationEnd: normalizeDate(form.ideaPresentationEnd),
      winnersAnnouncementDate: normalizeDate(form.winnersAnnouncementDate),
      contactEmail: form.contactEmail?.trim() || null,
      notes: form.notes?.trim() || null,
    };

    try {
      if (form.eventId) {
        await api.put(`/Events/${form.eventId}`, {
          ...payload,
          eventId: form.eventId,
        });
        toast.success(`Event "${form.title}" updated successfully.`);
      } else {
        await api.post("/Events", payload);
        toast.success(`Event "${form.title}" added successfully.`);
      }

      setForm(emptyForm);
      fetchEvents();
      setActiveTab("view");
    } catch (err) {
      console.error("Save error:", err);
      const message =
        err.response?.data?.message ||
        err.response?.data?.title ||
        "Failed to save event. Please try again.";
      toast.error(message);
    }
  };

  /**
   * Load Selected Event for Editing
   */
  const handleEdit = (evt) => {
    setForm({
      eventId: evt.eventId,
      title: evt.title || "",
      description: evt.description || "",
      eventType: evt.eventType || "",
      startDate: evt.startDate?.split("T")[0] || "",
      endDate: evt.endDate?.split("T")[0] || "",
      registrationCloseDate: evt.registrationCloseDate?.split("T")[0] || "",
      mentorCheckpointStart: evt.mentorCheckpointStart?.split("T")[0] || "",
      mentorCheckpointEnd: evt.mentorCheckpointEnd?.split("T")[0] || "",
      finalSubmissionDeadline: evt.finalSubmissionDeadline?.split("T")[0] || "",
      ideaPresentationStart: evt.ideaPresentationStart?.split("T")[0] || "",
      ideaPresentationEnd: evt.ideaPresentationEnd?.split("T")[0] || "",
      winnersAnnouncementDate: evt.winnersAnnouncementDate?.split("T")[0] || "",
      contactEmail: evt.contactEmail || "",
      notes: evt.notes || "",
    });
    setActiveTab("add");
  };

  /**
   * Confirm Delete (toast confirmation)
   */
  const handleDelete = (id) => {
    toast(
      (t) => (
        <div className="flex flex-col gap-2">
          <p className="text-sm text-gray-800">
            Are you sure you want to delete this event?
          </p>
          <div className="flex justify-end gap-2">
            <button
              onClick={() => {
                toast.dismiss(t.id);
                performDelete(id);
              }}
              className="bg-red-500 text-white px-3 py-1 rounded-md text-sm hover:bg-red-600"
            >
              Delete
            </button>
            <button
              onClick={() => toast.dismiss(t.id)}
              className="bg-gray-200 px-3 py-1 rounded-md text-sm hover:bg-gray-300"
            >
              Cancel
            </button>
          </div>
        </div>
      ),
      { duration: 4000 }
    );
  };

  const performDelete = async (id) => {
    try {
      await api.delete(`/Events/${id}`);
      toast.success("Event deleted successfully.");
      fetchEvents();
    } catch {
      toast.error("Failed to delete event.");
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 text-gray-800 font-sans flex flex-col">
      <Toaster position="top-right" />

      {/* Sub-navigation / Section Header */}
<div className="bg-white px-6 py-3 rounded-t-xl shadow-sm border-b border-gray-200 flex justify-between items-center">
  <h1 className="text-lg font-semibold text-gray-800">
    Event Management
  </h1>
  <div className="flex gap-3">
    <button
      onClick={() => {
        setForm(emptyForm);
        setActiveTab("add");
      }}
      className={`relative px-4 py-2 text-sm font-medium transition-all ${
        activeTab === "add"
          ? "text-blue-700 after:absolute after:bottom-0 after:left-0 after:w-full after:h-[2px] after:bg-blue-600"
          : "text-gray-600 hover:text-blue-600"
      }`}
    >
      Add Event
    </button>
    <button
      onClick={() => setActiveTab("view")}
      className={`relative px-4 py-2 text-sm font-medium transition-all ${
        activeTab === "view"
          ? "text-blue-700 after:absolute after:bottom-0 after:left-0 after:w-full after:h-[2px] after:bg-blue-600"
          : "text-gray-600 hover:text-blue-600"
      }`}
    >
      Events List
    </button>
  </div>
</div>


      {/* Main Content */}
      <main className="flex-1 p-8">
        {activeTab === "add" && (
          <form
            onSubmit={handleSubmit}
            className="bg-white p-8 rounded-xl shadow-md max-w-4xl mx-auto border border-gray-200 space-y-6"
          >
            <h2 className="text-xl font-semibold mb-2">
              {form.eventId ? "Edit Event" : "Add New Event"}
            </h2>

            {/* Title + Type */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium mb-1">Title</label>
                <input
                  type="text"
                  name="title"
                  value={form.title}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring focus:ring-blue-100"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">
                  Event Type
                </label>
                <select
                  name="eventType"
                  value={form.eventType}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring focus:ring-blue-100"
                  required
                >
                  <option value="">Select Type</option>
                  {eventTypes.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {/* Dates */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium mb-1">
                  Start Date
                </label>
                <input
                  type="date"
                  name="startDate"
                  value={form.startDate}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">
                  End Date
                </label>
                <input
                  type="date"
                  name="endDate"
                  value={form.endDate}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2"
                  required
                />
              </div>
            </div>

            {/* Optional Dates */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {[
                "registrationCloseDate",
                "mentorCheckpointStart",
                "mentorCheckpointEnd",
              ].map((field) => (
                <div key={field}>
                  <label className="block text-sm font-medium mb-1 capitalize">
                    {field.replace(/([A-Z])/g, " $1")}
                  </label>
                  <input
                    type="date"
                    name={field}
                    value={form[field]}
                    onChange={handleChange}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2"
                  />
                </div>
              ))}
            </div>

            {/* Presentation Deadlines */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {[
                "finalSubmissionDeadline",
                "ideaPresentationStart",
                "ideaPresentationEnd",
              ].map((field) => (
                <div key={field}>
                  <label className="block text-sm font-medium mb-1 capitalize">
                    {field.replace(/([A-Z])/g, " $1")}
                  </label>
                  <input
                    type="date"
                    name={field}
                    value={form[field]}
                    onChange={handleChange}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2"
                  />
                </div>
              ))}
            </div>

            {/* Winner Date + Contact */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium mb-1">
                  Winners Announcement
                </label>
                <input
                  type="date"
                  name="winnersAnnouncementDate"
                  value={form.winnersAnnouncementDate}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">
                  Contact Email
                </label>
                <input
                  type="email"
                  name="contactEmail"
                  value={form.contactEmail}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2"
                />
              </div>
            </div>

            {/* Description + Notes */}
            {["description", "notes"].map((field) => (
              <div key={field}>
                <label className="block text-sm font-medium mb-1 capitalize">
                  {field}
                </label>
                <textarea
                  name={field}
                  value={form[field]}
                  onChange={handleChange}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 h-24"
                />
              </div>
            ))}

            {/* Submit */}
            <div className="text-right">
              <button
                type="submit"
                className="bg-blue-200 text-blue-900 px-6 py-2 rounded-lg hover:bg-blue-300 transition"
              >
                {form.eventId ? "Update Event" : "Add Event"}
              </button>
            </div>
          </form>
        )}

        {/* Events List */}
        {activeTab === "view" && (
          <div className="bg-white p-6 rounded-xl shadow-md max-w-6xl mx-auto border border-gray-200">
            <h2 className="text-xl font-semibold mb-4">All Events</h2>

            {loading ? (
              <p className="text-gray-500">Loading events...</p>
            ) : events.length === 0 ? (
              <p className="text-gray-500">No events found.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="bg-blue-50 text-sm uppercase text-blue-900">
                      <th className="border px-3 py-2">Title</th>
                      <th className="border px-3 py-2">Type</th>
                      <th className="border px-3 py-2">Start</th>
                      <th className="border px-3 py-2">End</th>
                      <th className="border px-3 py-2 text-center">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {events.map((evt) => (
                      <tr key={evt.eventId} className="hover:bg-blue-50">
                        <td className="border px-3 py-2">{evt.title}</td>
                        <td className="border px-3 py-2">{evt.eventType}</td>
                        <td className="border px-3 py-2">
                          {evt.startDate?.split("T")[0]}
                        </td>
                        <td className="border px-3 py-2">
                          {evt.endDate?.split("T")[0]}
                        </td>
                        <td className="border px-3 py-2 text-center space-x-2">
                          <button
                            className="px-3 py-1 bg-yellow-200 text-sm rounded hover:bg-yellow-300"
                            onClick={() => handleEdit(evt)}
                          >
                            Edit
                          </button>
                          <button
                            className="px-3 py-1 bg-red-200 text-sm rounded hover:bg-red-300"
                            onClick={() => handleDelete(evt.eventId)}
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </main>
    </div>
  );
}
