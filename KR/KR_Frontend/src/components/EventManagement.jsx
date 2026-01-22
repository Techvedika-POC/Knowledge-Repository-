import React, { useState, useEffect } from "react";
import { toast, Toaster } from "react-hot-toast";
import api from "../api";
import { Plus, SquarePen, Trash2, List } from "lucide-react";

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

  const fetchEvents = async () => {
    setLoading(true);
    try {
      const res = await api.get("/Events");
      setEvents(res.data || []);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load events.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (activeTab === "view") fetchEvents();
  }, [activeTab]);

  useEffect(() => {
    fetchEvents();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

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
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        "Failed to save event. Please try again.";
      toast.error(message);
    }
  };

  const handleEdit = (evt) => {
    setForm({
      eventId: evt.eventId,
      title: evt.title || "",
      description: evt.description || "",
      eventType: evt.eventType || "",
      startDate: evt.startDate?.split?.("T")?.[0] || "",
      endDate: evt.endDate?.split?.("T")?.[0] || "",
      registrationCloseDate: evt.registrationCloseDate?.split?.("T")?.[0] || "",
      mentorCheckpointStart: evt.mentorCheckpointStart?.split?.("T")?.[0] || "",
      mentorCheckpointEnd: evt.mentorCheckpointEnd?.split?.("T")?.[0] || "",
      finalSubmissionDeadline: evt.finalSubmissionDeadline?.split?.("T")?.[0] || "",
      ideaPresentationStart: evt.ideaPresentationStart?.split?.("T")?.[0] || "",
      ideaPresentationEnd: evt.ideaPresentationEnd?.split?.("T")?.[0] || "",
      winnersAnnouncementDate: evt.winnersAnnouncementDate?.split?.("T")?.[0] || "",
      contactEmail: evt.contactEmail || "",
      notes: evt.notes || "",
    });
    setActiveTab("add");
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

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
    } catch (err) {
      console.error(err);
      toast.error("Failed to delete event.");
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-white to-slate-50 text-gray-800 font-sans">
      <Toaster position="top-right" />
      {/* Header */}
      <div className="max-w-6xl mx-auto px-6 pt-8">
        <div className="rounded-2xl bg-white shadow-sm border border-gray-100 p-6 flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div>
            <h1 className="text-2xl md:text-3xl font-extrabold text-slate-800">Event Management</h1>
            <p className="mt-1 text-sm text-slate-500 max-w-xl">
              Create and manage platform events. Use the form to add or edit events, and the list to review or delete them.
            </p>
          </div>

          <div className="flex items-center gap-3">
            <button
              onClick={() => {
                setForm(emptyForm);
                setActiveTab("add");
                window.scrollTo({ top: 0, behavior: "smooth" });
              }}
              className={`px-3 py-2 rounded-md text-sm font-medium transition
    flex items-center gap-2 whitespace-nowrap
    ${activeTab === "add"
                  ? "bg-blue-500 text-white shadow-md"
                  : "bg-white text-indigo-700 border border-indigo-100 hover:shadow-sm"
                }`}
            >
              <Plus size={16} />
              Add Event
            </button>

            <button
              onClick={() => setActiveTab("view")}
              className={`px-3 py-2 rounded-md text-sm font-medium transition
    flex items-center gap-2 whitespace-nowrap
    ${activeTab === "view"
                  ? "bg-blue-600 text-white shadow-md"
                  : "bg-white text-indigo-700 border border-indigo-100 hover:shadow-sm"
                }`}
            >
              <List size={16} />
              Events List
            </button>


          </div>
        </div>
      </div>

      <main className="flex-1 p-8">
        <div className="max-w-6xl mx-auto space-y-6">
          {/* Form */}
          {activeTab === "add" && (
            <form
              onSubmit={handleSubmit}
              className="bg-white rounded-2xl shadow-md border border-gray-100 p-6"
            >
              <h2 className="text-xl font-semibold text-slate-800 mb-4">
                {form.eventId ? "Edit Event" : "Add New Event"}
              </h2>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Title</label>
                  <input
                    type="text"
                    name="title"
                    value={form.title}
                    onChange={handleChange}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-100"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Event Type</label>
                  <select
                    name="eventType"
                    value={form.eventType}
                    onChange={handleChange}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 focus:ring-2 focus:ring-indigo-100"
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

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5 mt-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Start Date</label>
                  <input
                    type="date"
                    name="startDate"
                    value={form.startDate}
                    onChange={handleChange}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">End Date</label>
                  <input
                    type="date"
                    name="endDate"
                    value={form.endDate}
                    onChange={handleChange}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-5 mt-4">
                {[
                  "registrationCloseDate",
                  "mentorCheckpointStart",
                  "mentorCheckpointEnd",
                ].map((field) => (
                  <div key={field}>
                    <label className="block text-sm font-medium text-slate-700 mb-1 capitalize">
                      {field.replace(/([A-Z])/g, " $1")}
                    </label>
                    <input
                      type="date"
                      name={field}
                      value={form[field]}
                      onChange={handleChange}
                      className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                    />
                  </div>
                ))}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-5 mt-4">
                {[
                  "finalSubmissionDeadline",
                  "ideaPresentationStart",
                  "ideaPresentationEnd",
                ].map((field) => (
                  <div key={field}>
                    <label className="block text-sm font-medium text-slate-700 mb-1 capitalize">
                      {field.replace(/([A-Z])/g, " $1")}
                    </label>
                    <input
                      type="date"
                      name={field}
                      value={form[field]}
                      onChange={handleChange}
                      className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                    />
                  </div>
                ))}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5 mt-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Winners Announcement</label>
                  <input
                    type="date"
                    name="winnersAnnouncementDate"
                    value={form.winnersAnnouncementDate}
                    onChange={handleChange}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Contact Email</label>
                  <input
                    type="email"
                    name="contactEmail"
                    value={form.contactEmail}
                    onChange={handleChange}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                    placeholder="organizer@example.com"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-5 mt-4">
                {["description", "notes"].map((field) => (
                  <div key={field}>
                    <label className="block text-sm font-medium text-slate-700 mb-1 capitalize">{field}</label>
                    <textarea
                      name={field}
                      value={form[field]}
                      onChange={handleChange}
                      className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 h-28"
                    />
                  </div>
                ))}
              </div>

              <div className="mt-6 flex justify-end gap-3">
                <button
                  type="button"
                  onClick={() => setForm(emptyForm)}
                  className="px-4 py-2 rounded-lg bg-white border border-slate-200 text-slate-700 hover:shadow-sm"
                >
                  Reset
                </button>
                <button
                  type="submit"
                  className="px-4 py-1 rounded-md bg-green-600 text-white font-semibold shadow"
                >
                  {form.eventId ? "Update Event" : "Add Event"}
                </button>
              </div>
            </form>
          )}

          {/* Events List */}
          {activeTab === "view" && (
            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-xl font-semibold text-slate-800">All Events</h2>
                <div className="flex items-center gap-3">
                  <button
                    onClick={() => fetchEvents()}
                    className="px-3 py-2 rounded-lg bg-white border border-slate-200 text-slate-700 hover:shadow-sm text-sm"
                  >
                    Refresh
                  </button>
                </div>
              </div>

              {loading ? (
                <div className="text-slate-500">Loading events...</div>
              ) : events.length === 0 ? (
                <div className="text-slate-500">No events found.</div>
              ) : (
                <div className="overflow-hidden rounded-lg">
                  <table className="w-full text-left">
                    <thead className="bg-indigo-50">
                      <tr>
                        <th className="px-4 py-3 text-sm font-medium text-indigo-700">Title</th>
                        <th className="px-4 py-3 text-sm font-medium text-indigo-700">Type</th>
                        <th className="px-4 py-3 text-sm font-medium text-indigo-700">Start</th>
                        <th className="px-4 py-3 text-sm font-medium text-indigo-700">End</th>
                        <th className="px-4 py-3 text-sm font-medium text-indigo-700 text-center">Actions</th>
                      </tr>
                    </thead>

                    <tbody className="bg-white divide-y">
                      {events.map((evt, i) => (
                        <tr key={evt.eventId} className="hover:bg-slate-50 transition">
                          <td className="px-4 py-4">{evt.title}</td>
                          <td className="px-4 py-4">
                            <span className="text-xs px-2 py-1 rounded-full bg-sky-50 text-sky-700 font-semibold">{evt.eventType}</span>
                          </td>
                          <td className="px-4 py-4 text-sm text-slate-600">{evt.startDate?.split?.("T")?.[0] || "-"}</td>
                          <td className="px-4 py-4 text-sm text-slate-600">{evt.endDate?.split?.("T")?.[0] || "-"}</td>
                          <td className="px-4 py-4 text-center">
                            <div className="inline-flex gap-2">
                              <button
                                className=" text-sm p-1"
                                onClick={() => handleEdit(evt)}
                                aria-label="Edit"
                                title="Edit"
                              >
                                <SquarePen size={16} />
                              </button>

                              <button
                                className="text-red-600 hover:text-red-800 text-sm p-1"
                                onClick={() => handleDelete(evt.eventId)}
                                aria-label="Delete"
                                title="Delete"
                              >
                                <Trash2 size={16} />
                              </button>

                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
