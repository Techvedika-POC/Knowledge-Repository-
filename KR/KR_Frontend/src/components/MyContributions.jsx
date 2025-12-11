import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Search, X, ChevronLeft, ChevronRight } from "lucide-react";
import { FaLightbulb } from "react-icons/fa";
import api from "../api";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";

export default function MyContributions() {
  const navigate = useNavigate();
  const userId = localStorage.getItem("userId");

  const [contributions, setContributions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [hoveredItem, setHoveredItem] = useState(null);

  // Modal states
  const [modalOpen, setModalOpen] = useState(false);       
  const [selectedItem, setSelectedItem] = useState(null);  
  const [fullPreviewItem, setFullPreviewItem] = useState(null); 

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
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);

  const searchTypes = ["Title", "Domain", "Category", "Status", "Date"];

  // ----------------- FETCH METRICS -----------------
  const fetchMetrics = async () => {
    try {
      const token = localStorage.getItem("jwtToken");
      if (!token) return;

      const url = `/Contributions/my/paged?pageNumber=1&pageSize=10000`;
      const response = await api.get(url, {
        headers: { Authorization: `Bearer ${token}` },
      });

      const allItems = response.data.items || [];

      const approvedCount = allItems.filter(i => (i.status || "").toLowerCase() === "approved").length;
      const rejectedCount = allItems.filter(i => (i.status || "").toLowerCase() === "rejected").length;
      const pendingCount = allItems.filter(i => (i.status || "").toLowerCase() === "pending").length;

      setMetrics({
        knowledgeItems: allItems.length,
        approved: approvedCount,
        rejected: rejectedCount,
        pendingReviews: pendingCount,
      });
    } catch (err) {
      console.error("Failed to fetch metrics", err);
    }
  };

  // ----------------- FETCH PAGED CONTRIBUTIONS -----------------
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

      const params = new URLSearchParams({
        pageNumber,
        pageSize,
        ...cleanFilters,
      }).toString();

      const url = `/Contributions/my/paged?${params}`;

      const response = await api.get(url, {
        headers: { Authorization: `Bearer ${token}` },
      });

      const { items } = response.data;

      setContributions(items || []);
      setTotalPages(response.data.totalPages || 1);
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

  useEffect(() => {
    fetchMetrics();
    fetchContributions();
    fetchFilterOptions();
   
  }, []);

  useEffect(() => {
    fetchContributions();
    
  }, [pageNumber]);

  const handleSearch = (e) => {
    e.preventDefault();
    setPageNumber(1);
    const filters = {};
    if (keyword.trim()) {
      switch (searchType) {
        case "Domain": filters.domain = keyword.trim(); break;
        case "Category": filters.category = keyword.trim(); break;
        case "Title": filters.title = keyword.trim(); break;
        case "Status": filters.status = keyword.trim(); break;
        case "Date": filters.date = keyword; break;
      }
    }
    fetchContributions(filters);
  };


  const openCardModal = (item) => {
    setSelectedItem(item);
    setModalOpen(true);
  };

  const closeCardModal = () => {
    setModalOpen(false);
    setSelectedItem(null);
  };

  const handlePrevPage = () => pageNumber > 1 && setPageNumber(pageNumber - 1);
  const handleNextPage = () => pageNumber < totalPages && setPageNumber(pageNumber + 1);

  if (loading) return <p className="p-6 text-center text-gray-500">Loading contributions...</p>;
  if (error) return <p className="p-6 text-center text-red-500 font-semibold">{error}</p>;

  return (
    <div className="max-w-[1000px] mx-auto mt-5 p-6 bg-white rounded-[12px] shadow-[0_6px_12px_rgba(0,0,0,0.05)] font-inter text-[#1f2937]">
      {/* HEADER */}
      <div className="flex flex-col gap-3 mb-5">
        <h2 className="text-[24px] font-semibold">My Contributions</h2>
        <div className="flex items-center gap-2 bg-[#fef3c7] p-3 rounded-[8px] text-[#92400e] text-sm">
          <FaLightbulb />
          <span><strong>Tip:</strong> Manage and review your knowledge articles efficiently.</span>
        </div>
      </div>

      <section className="bg-[#f9fafb] p-6 rounded-[12px] shadow mb-5">
        {/* SEARCH */}
        <form onSubmit={handleSearch} className="flex flex-wrap gap-3 mb-5">
          <select
            value={searchType}
            onChange={(e) => setSearchType(e.target.value)}
            className="border rounded-[12px] px-3 py-2 text-[#1f2937] font-medium flex-shrink-0 w-[160px]"
          >
            {searchTypes.map((type) => (<option key={type} value={type}>{type}</option>))}
          </select>

          <div className="flex-1">
            {searchType === "Domain" ? (
              <select value={keyword} onChange={(e) => setKeyword(e.target.value)}
                className="border rounded-[12px] px-3 py-2 w-full">
                <option value="">Select Domain</option>
                {domainList.map((d) => <option key={d} value={d}>{d}</option>)}
              </select>
            ) : searchType === "Category" ? (
              <select value={keyword} onChange={(e) => setKeyword(e.target.value)}
                className="border rounded-[12px] px-3 py-2 w-full">
                <option value="">Select Category</option>
                {categoryList.map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            ) : searchType === "Title" ? (
              <select value={keyword} onChange={(e) => setKeyword(e.target.value)}
                className="border rounded-[12px] px-3 py-2 w-full">
                <option value="">Select Title</option>
                {titleList.map((t) => <option key={t} value={t}>{t}</option>)}
              </select>
            ) : searchType === "Date" ? (
              <input type="date" value={keyword} onChange={(e) => setKeyword(e.target.value)}
                className="border rounded-[12px] px-3 py-2 w-full" />
            ) : (
              <input type="text" value={keyword} onChange={(e) => setKeyword(e.target.value)}
                placeholder={`Enter ${searchType}`}
                className="border rounded-[12px] px-3 py-2 w-full" />
            )}
          </div>

          <button type="submit"
            className="px-4 py-2 bg-[#fef3c7] text-[#92400e] rounded-[12px] hover:bg-[#fde68a] transition flex justify-center items-center gap-1">
            <Search size={16} /> Search
          </button>
        </form>

        {/* METRICS */}
        <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
          {[
            { label: "Knowledge Items", value: metrics.knowledgeItems, color: "text-[#1f2937]" },
            { label: "Pending", value: metrics.pendingReviews, color: "text-[#92400e]" },
            { label: "Approved", value: metrics.approved, color: "text-[#16a34a]" },
            { label: "Rejected", value: metrics.rejected, color: "text-[#b91c1c]" },
          ].map((metric, idx) => (
            <div key={idx} className="bg-white rounded-[12px] shadow p-4 flex flex-col justify-center items-center hover:shadow-md transition">
              <p className={`text-sm font-semibold ${metric.color}`}>{metric.label}</p>
              <p className="text-2xl font-bold">{metric.value}</p>
            </div>
          ))}
        </div>
      </section>

      {/* TABLE */}
      <div className="bg-[#f9fafb] rounded-[12px] shadow overflow-x-auto">
        <table className="w-full text-sm table-auto">
          <thead className="bg-[#fef3c7] text-[#92400e]">
            <tr>
              <th className="px-4 py-2 text-left">Title</th>
              <th className="px-4 py-2 text-left">Category</th>
              <th className="px-4 py-2 text-left">Date</th>
              <th className="px-4 py-2 text-left">Status</th>
              <th className="px-4 py-2 text-left">Actions</th>
            </tr>
          </thead>
          <tbody>
            {contributions.map((c) => (
              <tr key={c.itemId} className="border-t transition"
                onMouseEnter={() => setHoveredItem(c)} onMouseLeave={() => setHoveredItem(null)}>
                <td className="px-4 py-2">{c.title}</td>
                <td className="px-4 py-2">{c.category}</td>
                <td className="px-4 py-2">{c.date ? new Date(c.date).toLocaleDateString() : "-"}</td>
                <td className="px-4 py-2 font-medium">{c.status}</td>
                <td className="px-4 py-2 flex gap-3 items-center">
                  <button
                    className="text-[#92400e] hover:underline font-semibold"
                    onClick={() => openCardModal(c)}
                    aria-label={`Preview ${c.title}`}
                  >
                    Preview
                  </button>

                  <button
                    className="px-3 py-1 rounded-[10px] bg-[#06b6d4] text-white text-[13px] hover:opacity-90"
                    onClick={() => navigate("/app/upload-knowledge", { state: { itemId: c.itemId } })}
                    aria-label={`Edit ${c.title}`}
                    title="Edit"
                  >
                    Edit
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* PAGINATION */}
        <div className="flex justify-end items-center gap-3 p-3 mt-3">
          <button onClick={handlePrevPage} disabled={pageNumber === 1}
            className="px-4 py-2 bg-[#fef3c7] rounded-[8px] hover:bg-[#e5e7eb] text-center disabled:opacity-50 flex items-center gap-1">
            <ChevronLeft size={16} /> Prev
          </button>
          <span className="px-4 py-2 bg-[#f3f4f6] rounded-[8px] text-center text-sm">
            Page {pageNumber} of {totalPages}
          </span>
          <button onClick={handleNextPage} disabled={pageNumber === totalPages}
            className="px-4 py-2 bg-[#fef3c7] rounded-[8px] hover:bg-[#e5e7eb] text-center disabled:opacity-50 flex items-center gap-1">
            Next <ChevronRight size={16} />
          </button>
        </div>
      </div>

      {/* TOOLTIP */}
      {hoveredItem && (
        <div className="fixed bottom-16 left-1/2 transform -translate-x-1/2 w-80 bg-white border border-gray-200 shadow rounded p-3 text-sm z-50">
          <h3 className="font-semibold text-[#1f2937]">{hoveredItem.title}</h3>
          <p className="mt-1 text-gray-600">{hoveredItem.description}</p>
        </div>
      )}

 
      {modalOpen && selectedItem && (
        <div className="fixed inset-0 flex justify-center items-start bg-black bg-opacity-20 z-50">
          <div className="bg-transparent w-full max-w-5xl mx-4 mt-16 mb-8">
            <div className="relative">
              <button
                className="absolute right-2 top-2 z-50 bg-white rounded-full p-2 shadow"
                onClick={closeCardModal}
                aria-label="Close preview"
              >
                <X size={18} />
              </button>

              <div className="bg-white rounded-2xl p-4 shadow">
                <KnowledgeCardsDisplay
                  items={[{
                    ...selectedItem,
                    itemId: selectedItem.itemId,
                    id: selectedItem.itemId,
                    title: selectedItem.title,
                    description: selectedItem.description,
                    tags: selectedItem.tags || [],
                    ownerName: selectedItem.ownerName || selectedItem.submittedBy,
                  }]}
                  userId={userId}
                  onPreview={(item) => {

                    setFullPreviewItem(item);
                  }}
                />
              </div>
            </div>
          </div>
        </div>
      )}

      {/* FULL PREVIEW MODAL (uses PreviewModal component) */}
      {fullPreviewItem && (
        <PreviewModal
          item={fullPreviewItem}
          onClose={() => setFullPreviewItem(null)}
        />
      )}
    </div>
  );
}
