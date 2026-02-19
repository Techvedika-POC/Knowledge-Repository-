import React, { useEffect, useState } from "react";
import api from "../api";
import { Check, X, Eye, ChevronLeft, ChevronRight } from "lucide-react";
import PreviewModal from "./PreviewModal";

export default function ApproverPage() {
  const [tab, setTab] = useState("normal");
  const [items, setItems] = useState([]);
  const [eventTypes, setEventTypes] = useState([]);
  const [selectedEventType, setSelectedEventType] = useState("");
  const [loading, setLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(null);
  const [fullPreviewItem, setFullPreviewItem] = useState(null);
  const [error, setError] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalItems, setTotalItems] = useState(0);
  const totalPages = Math.ceil(totalItems / pageSize);
  const [rejectItem, setRejectItem] = useState(null);
  const [feedback, setFeedback] = useState("");
  const openFullPreview = async (item) => {
    const itemId = getItemId(item);
    if (!itemId) return;

    try {
      const token = localStorage.getItem("token");

      const res = await api.get(
        `/KnowledgeItem/${itemId}/details`,
        { headers: { Authorization: `Bearer ${token}` } }
      );

      setFullPreviewItem(res.data);
    } catch (err) {
      console.error("Failed to load preview", err.response?.data || err);
      setError("Failed to load preview details.");
    }
  };

  const getItemId = (item) =>
    item.knowledgeItemId ||
    item.eventItemId ||
    item.itemId ||
    item.Id ||
    item.id;
  const loadEventTypes = async () => {
    try {
      const token = localStorage.getItem("token");
      const res = await api.get("/approver/event/types", {
        headers: { Authorization: `Bearer ${token}` },
      });
      setEventTypes(res.data || []);
    } catch (err) {
      console.error("Failed loading event types", err);
    }
  };
  useEffect(() => {
    loadEventTypes();
  }, []);
  const fetchData = async () => {
    setLoading(true);
    setError("");

    try {
      const token = localStorage.getItem("token");
      let params = `?page=${pageNumber}&size=${pageSize}`;
      let endpoint = "";

      if (tab === "normal") endpoint = `/approver/pending/normal${params}`;
      else if (tab === "event") endpoint = `/approver/pending/event${params}`;
      else if (tab === "eventType") {
        if (!selectedEventType) {
          setItems([]);
          return;
        }
        endpoint = `/approver/pending/event/type/${selectedEventType}${params}`;
      }
      else if (tab === "all") {
        const [normal, event] = await Promise.all([
          api.get(`/approver/pending/normal${params}`, {
            headers: { Authorization: `Bearer ${token}` },
          }),
          api.get(`/approver/pending/event${params}`, {
            headers: { Authorization: `Bearer ${token}` },
          }),
        ]);

        const merged = [
          ...(normal.data.items || []),
          ...(event.data.items || []),
        ];

        setItems(merged);
        setTotalItems(merged.length);
        setLoading(false);
        return;
      }

      const res = await api.get(endpoint, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setItems(res.data.items || []);
      setTotalItems(res.data.total || 0);
    } catch (err) {
      console.error(err);
      setError("Failed to load items.");
    } finally {
      setLoading(false);
    }
  };
  useEffect(() => {
    fetchData();
  }, [tab, selectedEventType, pageNumber]);
  const handleAction = async (itemId, action) => {
    if (!itemId) {
      setError("Invalid item ID.");
      return;
    }

    setActionLoading(itemId);

    try {
      const token = localStorage.getItem("token");

      if (action === "reject") {
        await api.post(
          `/approver/reject/${itemId}`,
          { feedback },
          { headers: { Authorization: `Bearer ${token}` } }
        );
      } else {
        await api.post(
          `/approver/approve/${itemId}`,
          {},
          { headers: { Authorization: `Bearer ${token}` } }
        );
      }

      setRejectItem(null);
      setFeedback("");
      fetchData();
    } catch (err) {
      console.error("Action failed", err);
      setError(
        err.response?.data ||
        err.response?.data?.title ||
        "Action failed."
      );
    } finally {
      setActionLoading(null);
    }
  };
  return (
    <div className="p-10 min-h-screen bg-gradient-to-br from-[#F7F9FC] to-[#EDF2FA]">
      <div className="max-w-6xl mx-auto">
        <div className="mb-10 text-center">
          <h1 className="text-3xl font-extrabold text-gray-700 tracking-wide">
            Knowledge Approval Center
          </h1>
          <p className="text-gray-500 mt-2 text-sm">
            Review, approve and manage knowledge submissions efficiently.
          </p>
        </div>

        <div className="flex gap-3 mb-6 justify-center">
          {[
            { key: "normal", label: "Normal Items" },
            { key: "event", label: "Event Items" },
            { key: "eventType", label: "Filter by Event Type" },
            { key: "all", label: "Show All" },
          ].map((t) => (
            <button
              key={t.key}
              className={`px-5 py-2.5 shadow-sm rounded-full text-sm font-medium transition
                ${tab === t.key
                  ? "bg-blue-500 text-white shadow-md"
                  : "bg-white border border-gray-200 text-gray-600 hover:bg-gray-50"
                }
              `}
              onClick={() => {
                setTab(t.key);
                setPageNumber(1);
              }}
            >
              {t.label}
            </button>
          ))}
        </div>
        {tab === "eventType" && (
          <div className="flex justify-center mb-5">
            <select
              className="border rounded-lg px-4 py-2.5 bg-white shadow-sm text-gray-600"
              value={selectedEventType}
              onChange={(e) => setSelectedEventType(e.target.value)}
            >
              <option value="">Select Event Type</option>
              {eventTypes.map((type, idx) => (
                <option key={idx} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
        )}
        {error && (
          <p className="text-red-500 text-center bg-red-50 py-2 rounded-lg mb-4 border border-red-200">
            {error}
          </p>
        )}
        {loading ? (
          <p className="text-center text-gray-500">Loading...</p>
        ) : items.length === 0 ? (
          <p className="text-center bg-white p-6 rounded-lg shadow text-gray-600 border border-gray-100">
            No pending items found.
          </p>
        ) : (
          <>
            <div className="bg-white shadow-lg rounded-xl overflow-hidden border border-gray-100">
              <table className="min-w-full text-sm">
                <thead className="bg-blue-50 text-blue-700">
                  <tr>
                    <th className="px-4 py-3 text-left font-medium">Title</th>
                    <th className="px-4 py-3 text-left font-medium">Submitted By</th>
                    <th className="px-4 py-3 text-center font-medium">Actions</th>
                  </tr>
                </thead>

                <tbody className="divide-y divide-gray-100">
                  {items.map((item) => {
                    const id = getItemId(item);
                    return (
                      <tr key={id} className="hover:bg-blue-50/40 transition">

                        <td className="px-4 py-3 font-medium text-gray-700">
                          {item.title}
                        </td>

                        <td className="px-4 py-3 text-gray-600">
                          {item.createdByName}
                        </td>

                        <td className="px-4 py-3 flex gap-2 justify-center">
                          <button
                            onClick={() => handleAction(id, "approve")}
                            disabled={actionLoading === id}
                            className="px-3 py-1.5 bg-green-100 text-green-700 hover:bg-green-200 rounded-full flex items-center gap-1 text-xs"
                          >
                            <Check size={14} /> Approve
                          </button>
                          <button
                            onClick={() => {
                              setRejectItem(id);
                              setFeedback("");
                            }}
                            disabled={actionLoading === id}
                            className="px-3 py-1.5 bg-red-100 text-red-700 hover:bg-red-200 rounded-full flex items-center gap-1 text-xs"
                          >
                            <X size={14} /> Reject
                          </button>

                          <button
                            onClick={() => openFullPreview(item)}
                            className="px-3 py-1.5 bg-blue-100 text-blue-700 hover:bg-blue-200 rounded-full flex items-center gap-1 text-xs"
                          >
                            <Eye size={14} /> Preview
                          </button>
                          {fullPreviewItem && (
                            <PreviewModal
                              item={fullPreviewItem}
                              onClose={() => setFullPreviewItem(null)}
                            />
                          )}

                        </td>

                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            {rejectItem && (
              <div className="fixed inset-0 bg-black/30 backdrop-blur-sm flex items-center justify-center">
                <div className="bg-white rounded-2xl p-6 shadow-xl w-full max-w-md border border-gray-100">

                  <h2 className="text-lg font-bold text-gray-700 mb-2">
                    Reject Item
                  </h2>

                  <p className="text-sm text-gray-500 mb-3">
                    Please provide feedback for rejection.
                  </p>

                  <textarea
                    className="w-full border rounded-lg p-3 text-sm"
                    rows={4}
                    placeholder="What needs to be changed or improved?"
                    value={feedback}
                    onChange={(e) => setFeedback(e.target.value)}
                  />

                  <div className="flex justify-end gap-3 mt-4">
                    <button
                      onClick={() => setRejectItem(null)}
                      className="px-4 py-2 bg-gray-100 rounded-lg"
                    >
                      Cancel
                    </button>

                    <button
                      disabled={!feedback.trim()}
                      onClick={() => handleAction(rejectItem, "reject")}
                      className="px-4 py-2 bg-red-500 text-white rounded-lg disabled:opacity-50"
                    >
                      Reject with Feedback
                    </button>
                  </div>

                </div>
              </div>
            )}
            {rejectItem && (
              <div className="fixed inset-0 bg-black/30 backdrop-blur-sm flex items-center justify-center">
                <div className="bg-white rounded-2xl p-6 shadow-xl w-full max-w-md border border-gray-100">

                  <h2 className="text-lg font-bold text-gray-700 mb-2">
                    Reject Item
                  </h2>

                  <p className="text-sm text-gray-500 mb-3">
                    Please provide feedback for rejection.
                  </p>

                  <textarea
                    className="w-full border rounded-lg p-3 text-sm"
                    rows={4}
                    placeholder="What needs to be changed or improved?"
                    value={feedback}
                    onChange={(e) => setFeedback(e.target.value)}
                  />

                  <div className="flex justify-end gap-3 mt-4">
                    <button
                      onClick={() => setRejectItem(null)}
                      className="px-4 py-2 bg-gray-100 rounded-lg"
                    >
                      Cancel
                    </button>

                    <button
                      disabled={!feedback.trim()}
                      onClick={() => handleAction(rejectItem, "reject")}
                      className="px-4 py-2 bg-red-500 text-white rounded-lg disabled:opacity-50"
                    >
                      Reject with Feedback
                    </button>
                  </div>

                </div>
              </div>
            )}
            {tab !== "all" && (
              <div className="flex justify-end items-center gap-3 mt-4">
                <button
                  disabled={pageNumber === 1}
                  onClick={() => setPageNumber(pageNumber - 1)}
                  className="px-3 py-1.5 border border-gray-200 bg-white rounded-md shadow-sm hover:bg-gray-50 flex items-center gap-1"
                >
                  <ChevronLeft size={16} /> Prev
                </button>

                <span className="text-gray-600 text-sm">
                  Page {pageNumber} / {totalPages}
                </span>

                <button
                  disabled={pageNumber === totalPages}
                  onClick={() => setPageNumber(pageNumber + 1)}
                  className="px-3 py-1.5 border border-gray-200 bg-white rounded-md shadow-sm hover:bg-gray-50 flex items-center gap-1"
                >
                  Next <ChevronRight size={16} />
                </button>
              </div>
            )}
          </>
        )}

      </div>
    </div>

  );
}
