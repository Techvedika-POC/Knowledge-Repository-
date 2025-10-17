import React, { useEffect, useState } from "react";
import { Search, X, ChevronLeft, ChevronRight } from "lucide-react";
import api from "../api";

export default function MyContributions() {
  const [contributions, setContributions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [hoveredItem, setHoveredItem] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState(null);

  const [metrics, setMetrics] = useState({
    knowledgeItems: 0,
    pendingReviews: 0,
    approved: 0,
    rejected: 0,
  });

  const [searchType, setSearchType] = useState("Title");
  const [keyword, setKeyword] = useState("");
  const [domainList, setDomainList] = useState([]);
  const [categoryList, setCategoryList] = useState([]);
  const [titleList, setTitleList] = useState([]);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10); // items per page
  const [totalPages, setTotalPages] = useState(1);

  const searchTypes = ["Title", "Domain", "Category", "Status", "Date"];

  useEffect(() => {
    fetchContributions();
    fetchFilterOptions();

  const fetchContributions = async (filters = {}) => {
    const token = localStorage.getItem("jwtToken");
    if (!token) {
      setError("Authentication required");
      return;
    }

    try {
      setLoading(true);
      const cleanFilters = Object.fromEntries(
        Object.entries(filters).filter(([_, v]) => v != null && v !== "")
      );
      const params = new URLSearchParams(cleanFilters).toString();
      let url = "/Contributions/my";
      if (params) url = `/Contributions/my/filter?${params}`;

      const params = new URLSearchParams({
        pageNumber,
        pageSize,
        ...cleanFilters
      }).toString();

      const url = `/Contributions/my/paged?${params}`;

      const response = await api.get(url, {
        headers: { Authorization: `Bearer ${token}` },
      });

      const data = response.data;


      setMetrics({
        knowledgeItems: total,
        approved,
        rejected,
        pendingReviews: pending,
      });

    } catch (err) {
      console.error("Fetch contributions error:", err.response?.data || err);
      setError(err.response?.data?.title || "Failed to fetch contributions");
    } finally {
      setLoading(false);
    }
  };

  const fetchFilterOptions = async () => {
    try {
      const [domainsRes, categoriesRes, titlesRes] = await Promise.all([
        api.get("/Contributions/my/domains"),
        api.get("/Contributions/my/categories"),
        api.get("/Contributions/my/titles"),
      ]);

      setDomainList(domainsRes.data || []);
      setCategoryList(categoriesRes.data || []);
      setTitleList(titlesRes.data || []);
    } catch (error) {
      console.error("Failed to load filter options:", error);
    }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    setPageNumber(1); // reset to first page
    const filters = {};
    if (keyword.trim()) {
      switch (searchType) {
        case "Domain":
          filters.domain = keyword.trim();
          break;
        case "Category":
          filters.category = keyword.trim();
          break;
        case "Title":
          filters.title = keyword.trim();
          break;
        case "Status":
          filters.status = keyword.trim();
          break;
        case "Date":
          filters.date = keyword;
          break;
      }
    }
    fetchContributions(filters);
  };

  const openModal = (item) => {
    setSelectedItem(item);
    setModalOpen(true);
  };
  const closeModal = () => {
    setModalOpen(false);
    setSelectedItem(null);
  };

  const handlePrevPage = () => {
    if (pageNumber > 1) setPageNumber(pageNumber - 1);
  };
  const handleNextPage = () => {
    if (pageNumber < totalPages) setPageNumber(pageNumber + 1);
  };

  if (loading) return <p className="p-4">Loading...</p>;
  if (error) return <p className="p-4 text-red-500">{error}</p>;

  return (
    <div className="bg-gray-50 min-h-screen font-sans m-0 p-4">
      {/* HEADER */}
      <div className="mb-2">
        <h1
          className="text-2xl font-bold text-indigo-700 mt-0"
          style={{ fontFamily: "Calibri, sans-serif" }}
        >
          My Contributions
        </h1>
        <p className="text-sm font-bold text-gray-800 px-2 py-1 rounded mt-1 border border-gray-200 bg-white">
          View and filter your uploaded knowledge Articles.
        </p>
      </div>

      {/* SEARCH */}
      <form
        onSubmit={handleSearch}
        className="flex gap-2 p-3 flex-wrap rounded-lg border border-gray-200 bg-white mb-3"
      >
        <select
          value={searchType}
          onChange={(e) => setSearchType(e.target.value)}
          className="border rounded px-2 py-1 bg-white text-sm"
        >
          {searchTypes.map((type) => (
            <option key={type} value={type}>
              {type}
            </option>
          ))}
        </select>

        {/* keyword input/select based on type */}
        {searchType === "Domain" ? (
            <option value="">Select Domain</option>
          </select>
        ) : searchType === "Category" ? (
            <option value="">Select Category</option>
          </select>
        ) : searchType === "Title" ? (
            <option value="">Select Title</option>
          </select>
        ) : searchType === "Date" ? (
        ) : (
        )}

          <Search size={16} /> Search
        </button>
      </form>

      {/* METRICS */}
      <div className="grid grid-cols-4 gap-2 p-2 mb-3 text-center">
        {[
          { label: "Knowledge Items", value: metrics.knowledgeItems, text: "text-blue-700" },
          { label: "Pending", value: metrics.pendingReviews, text: "text-yellow-600" },
          { label: "Approved", value: metrics.approved, text: "text-green-700" },
          { label: "Rejected", value: metrics.rejected, text: "text-red-700" },
        ].map((metric, idx) => (
          <div key={idx} className="p-2 rounded border border-gray-200 bg-white">
            <p className={`text-sm font-semibold ${metric.text}`}>{metric.label}</p>
            <p className="font-bold text-lg">{metric.value}</p>
          </div>
        ))}
      </div>

      {/* TABLE */}
      <div className="bg-white rounded shadow overflow-x-auto p-2">
        <table className="w-full text-sm">
          <thead className="bg-purple-100 text-left text-purple-700">
            <tr>
              <th className="px-2 py-1">Title</th>
              <th className="px-2 py-1">Category</th>
              <th className="px-2 py-1">Date</th>
              <th className="px-2 py-1">Status</th>
              <th className="px-2 py-1">Actions</th>
            </tr>
          </thead>
          <tbody>
            {contributions.map((c) => (
              <tr key={c.itemId} className="border-t hover:bg-gray-50 relative">
                <td
                  className="px-2 py-1 cursor-pointer relative"
                  onMouseEnter={(e) =>
                  }
                  onMouseMove={(e) =>
                  }
                  onMouseLeave={() =>
                  }
                >
                  {c.title}
                </td>
                <td className="px-2 py-1">{c.category}</td>
                <td className="px-2 py-1">{new Date(c.date).toLocaleDateString()}</td>
                <td className="px-2 py-1">
                    {c.status}
                  </span>
                </td>
                <td className="px-2 py-1">
                    Preview
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* PAGINATION */}
        <div className="flex justify-end items-center gap-2 mt-2">
          <button onClick={handlePrevPage} disabled={pageNumber === 1} className="flex items-center px-2 py-1 border rounded hover:bg-gray-100 disabled:opacity-50">
            <ChevronLeft size={16} /> Prev
          </button>
          <span>Page {pageNumber} of {totalPages}</span>
          <button onClick={handleNextPage} disabled={pageNumber === totalPages} className="flex items-center px-2 py-1 border rounded hover:bg-gray-100 disabled:opacity-50">
            Next <ChevronRight size={16} />
          </button>
        </div>
      </div>

      {/* TOOLTIP */}
{hoveredItem && hoveredItem.visible && (
    <h3 className="font-semibold text-indigo-700">{hoveredItem.title}</h3>
    <p className="mt-1 text-gray-700">{hoveredItem.description}</p>
  </div>
)}


      {/* PREVIEW MODAL */}
      {modalOpen && selectedItem && (
        <div className="fixed inset-0 flex justify-center items-center bg-black bg-opacity-40 z-50">
          <div className="bg-white rounded-lg w-2/5 p-4 relative shadow-lg">
              <X size={20} />
            </button>
            <h2 className="text-lg font-bold text-indigo-700">{selectedItem.title}</h2>
            <p className="mt-2 text-sm">{selectedItem.description}</p>
          </div>
        </div>
      )}
    </div>
  );
}
