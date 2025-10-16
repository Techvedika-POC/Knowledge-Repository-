import React, { useEffect, useState } from "react";
import api from "../api";
import { Check, X, Eye, ChevronLeft, ChevronRight } from "lucide-react";

export default function ApproverPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(null);
  const [previewItem, setPreviewItem] = useState(null);
  const [error, setError] = useState("");

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [totalItems, setTotalItems] = useState(0);
  const totalPages = Math.ceil(totalItems / pageSize);

  const fetchPendingItems = async () => {
    setLoading(true);
    setError("");
    try {
      const token = localStorage.getItem("token");
      const params = new URLSearchParams({ pageNumber, pageSize }).toString();

      const res = await api.get(`/approver/pending/paged?${params}`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setItems(res.data.items || []);
      setTotalItems(res.data.totalCount || 0);
    } catch (err) {
      console.error(err);
      setError("Failed to fetch pending items.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPendingItems();
  }, [pageNumber]);

  const handleAction = async (itemId, action) => {
    setActionLoading(itemId);
    setError("");
    try {
      const token = localStorage.getItem("token");
      await api.post(
        `/approver/${action}/${itemId}`,
        {},
        { headers: { Authorization: `Bearer ${token}` } }
      );
      fetchPendingItems();
    } catch (err) {
      console.error(err);
      setError(err.response?.data || "Action failed.");
    } finally {
      setActionLoading(null);
    }
  };

  const handlePrevPage = () => {
    if (pageNumber > 1) setPageNumber(pageNumber - 1);
  };

  const handleNextPage = () => {
    if (pageNumber < totalPages) setPageNumber(pageNumber + 1);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-indigo-50 p-8">
      <div className="max-w-7xl mx-auto">
        <h2 className="text-3xl font-bold text-indigo-700 mb-6 text-center">
          Pending Knowledge Approvals
        </h2>

        {error && <p className="text-sm text-red-500 text-center mb-4">{error}</p>}

        {loading ? (
          <p className="text-sm text-gray-500 text-center">Loading...</p>
        ) : items.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-6 text-center text-gray-600">
            No pending items 
          </div>
        ) : (
          <>
            <div className="bg-white rounded-lg shadow overflow-x-auto border border-gray-200">
              <table className="min-w-full text-sm">
                <thead className="bg-indigo-100 text-indigo-800">
                  <tr>
                    <th className="py-3 px-4 font-medium">Title</th>
                    <th className="py-3 px-4 font-medium">Submitted By</th>
                    <th className="py-3 px-4 font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {items.map((item) => (
                    <tr key={item.itemId} className="hover:bg-indigo-50 transition-all">
                      <td className="py-3 px-4 font-medium text-gray-800">{item.title}</td>
                      <td className="py-3 px-4 text-gray-700">{item.createdByName}</td>
                      <td className="py-3 px-4 flex gap-2 flex-wrap">
                        <button
                          onClick={() => handleAction(item.itemId, "approve")}
                          disabled={actionLoading === item.itemId}
                          className="flex items-center gap-1 px-2 py-1 text-xs rounded-full bg-green-100 text-green-800 hover:bg-green-200 transition"
                        >
                          <Check size={14} /> Approve
                        </button>
                        <button
                          onClick={() => handleAction(item.itemId, "reject")}
                          disabled={actionLoading === item.itemId}
                          className="flex items-center gap-1 px-2 py-1 text-xs rounded-full bg-rose-100 text-rose-800 hover:bg-rose-200 transition"
                        >
                          <X size={14} /> Reject
                        </button>
                        <button
                          onClick={() => setPreviewItem(item)}
                          className="flex items-center gap-1 px-2 py-1 text-xs rounded-full bg-indigo-200 text-indigo-800 hover:bg-indigo-300 transition"
                        >
                          <Eye size={14} /> Preview
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            <div className="flex justify-end items-center gap-2 mt-3">
              <button
                onClick={handlePrevPage}
                disabled={pageNumber === 1}
                className="flex items-center px-2 py-1 border rounded hover:bg-gray-100 disabled:opacity-50"
              >
                <ChevronLeft size={16} /> Prev
              </button>
              <span>
                Page {pageNumber} of {totalPages}
              </span>
              <button
                onClick={handleNextPage}
                disabled={pageNumber === totalPages}
                className="flex items-center px-2 py-1 border rounded hover:bg-gray-100 disabled:opacity-50"
              >
                Next <ChevronRight size={16} />
              </button>
            </div>
          </>
        )}

        {/* Preview Modal */}
        {previewItem && (
          <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
            <div className="bg-white rounded-2xl shadow-2xl max-w-lg w-full p-6 border border-gray-200 relative animate-fade-in">
              <button
                className="absolute top-4 right-4 text-gray-500 hover:text-gray-900"
                onClick={() => setPreviewItem(null)}
              >
                <X size={20} />
              </button>
              <h3 className="text-2xl font-bold text-indigo-700 mb-4 border-b pb-2">
                {previewItem.title}
              </h3>
              <div className="space-y-3 text-sm text-gray-700">
                <p>
                  <strong>Description:</strong> {previewItem.description}
                </p>
                <p>
                  <strong>Submitted By:</strong> {previewItem.createdByName}
                </p>
                <div className="grid grid-cols-2 gap-3 border-t pt-3 mt-3">
                  <div>
                    <strong>Domain:</strong>{" "}
                    <span className="text-blue-600">{previewItem.domainName || "N/A"}</span>
                  </div>
                  <div>
                    <strong>Category:</strong>{" "}
                    <span className="text-purple-600">{previewItem.categoryName || "N/A"}</span>
                  </div>
                  <div>
                    <strong>Framework:</strong>{" "}
                    <span className="text-cyan-600">{previewItem.framework || "N/A"}</span>
                  </div>
                  <div>
                    <strong>Language:</strong>{" "}
                    <span className="text-pink-600">{previewItem.language || "N/A"}</span>
                  </div>
                </div>
              </div>

              <div className="mt-6 flex justify-end">
                <button
                  onClick={() => setPreviewItem(null)}
                  className="px-4 py-2 rounded-full bg-indigo-600 text-white text-sm hover:bg-indigo-700 transition"
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
