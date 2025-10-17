import React, { useState, useEffect, useRef } from "react";
import api from "../api";
import { FaSearch, FaLayerGroup, FaFolderOpen, FaFilter } from "react-icons/fa";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";

export default function Navbar({
  onLike,
  onFavourite,
  onComment,
  engagement
}) {
  const [domains, setDomains] = useState([]);
  const [categories, setCategories] = useState([]);
  const [searchResults, setSearchResults] = useState([]);
  const [isSearching, setIsSearching] = useState(false);
  const [error, setError] = useState("");
  const [selectedDomain, setSelectedDomain] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("");
  const [keyword, setKeyword] = useState("");
  const [showDomainDropdown, setShowDomainDropdown] = useState(false);
  const [showCategoryDropdown, setShowCategoryDropdown] = useState(false);
  const [selectedItem, setSelectedItem] = useState(null);

  const userId = localStorage.getItem("userId");
  const domainDropdownRef = useRef(null);
  const categoryDropdownRef = useRef(null);

  useEffect(() => {
    const fetchFilters = async () => {
      try {
        const [domainRes, categoryRes] = await Promise.all([
          api.get("/Domains"),
          api.get("/Categories")
        ]);
        setDomains(domainRes.data);
        setCategories(categoryRes.data);
      } catch (err) {
        console.error("Error fetching filters", err);
      }
    };
    fetchFilters();
  }, []);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (domainDropdownRef.current && !domainDropdownRef.current.contains(event.target))
        setShowDomainDropdown(false);
      if (categoryDropdownRef.current && !categoryDropdownRef.current.contains(event.target))
        setShowCategoryDropdown(false);
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const fetchItems = async (url) => {
    setIsSearching(true);
    setError("");
    try {
      const res = await api.get(url);
      setSearchResults(res.data || []);
    } catch {
      setError("Error fetching items.");
    } finally {
      setIsSearching(false);
    }
  };

  const handleDomainSelection = (domainId) => {
    setSelectedDomain(domainId);
    setSelectedCategory("");
    setKeyword("");
    setShowDomainDropdown(false);
    fetchItems(`/KnowledgeItem/ByDomain/${domainId}`);
  };

  const handleCategorySelection = (categoryId) => {
    setSelectedCategory(categoryId);
    setSelectedDomain("");
    setKeyword("");
    setIsSearching(true);
    setShowCategoryDropdown(false);
    fetchItems(`/KnowledgeItem/ByCategory/${categoryId}`);
  };

  const handleBrowseAll = () => {
    setSelectedDomain("");
    setSelectedCategory("");
    setKeyword("");
    fetchItems("/KnowledgeItem/All");
  };

  const handleSearch = (e) => {
    e.preventDefault();
    if (!keyword.trim()) return;
    setSelectedDomain("");
    setSelectedCategory("");
    fetchItems(`/GlobalSearch?keyword=${encodeURIComponent(keyword)}`);
  };

  const handleReset = () => {
    setSelectedDomain("");
    setSelectedCategory("");
    setKeyword("");
    setSearchResults([]);
    setError("");
    setShowAll(false);
  };

  const chunkedResults = [];
  for (let i = 0; i < searchResults.length; i += 3) {
    chunkedResults.push(searchResults.slice(i, i + 3));
  }
  const rowsToShow = showAll ? chunkedResults.length : 1;

  return (
    <>
      {/* Navbar */}
      <div className="sticky top-0 left-0 w-full bg-white shadow-md border-b z-10 px-4 py-2 mb-0">
      

          <div className="flex justify-between items-center gap-3 w-full">
            {/* Filters */}
          <div className="flex gap-4 items-center">
            {/* Domain */}
              <div className="relative" ref={domainDropdownRef}>
                <button
                  onClick={() => setShowDomainDropdown(!showDomainDropdown)}
                className={`px-5 py-2 rounded-full text-base font-medium flex items-center gap-2 transition ${
                  selectedDomain ? "bg-cyan-700 text-white" : "bg-cyan-100 text-cyan-700 hover:bg-cyan-200"
                  }`}
                >
                  <FaLayerGroup /> Domain
                </button>
                {showDomainDropdown && (
                <div className="absolute top-full left-0 mt-1 bg-white shadow-lg rounded-lg p-2 z-50 w-56 max-h-56 overflow-auto border border-gray-200 text-base">
                    {domains.map((d) => (
                      <button
                        key={d.domainId}
                      className={`w-full text-left px-3 py-2 rounded ${
                        selectedDomain === d.domainId ? "bg-cyan-700 text-white" : "hover:bg-gray-100"
                        }`}
                        onClick={() => handleDomainSelection(d.domainId)}
                      >
                        {d.domainName}
                      </button>
                    ))}
                  </div>
                )}
              </div>

            {/* Category */}
              <div className="relative" ref={categoryDropdownRef}>
                <button
                  onClick={() => setShowCategoryDropdown(!showCategoryDropdown)}
                className={`px-5 py-2 rounded-full text-base font-medium flex items-center gap-2 transition ${
                  selectedCategory ? "bg-blue-700 text-white" : "bg-blue-100 text-blue-700 hover:bg-blue-200"
                  }`}
                >
                  <FaFolderOpen /> Category
                </button>
                {showCategoryDropdown && (
                <div className="absolute top-full left-0 mt-1 bg-white shadow-lg rounded-lg p-2 z-50 w-56 max-h-56 overflow-auto border border-gray-200 text-base">
                    {categories.map((c) => (
                      <button
                        key={c.categoryId}
                      className={`w-full text-left px-3 py-2 rounded ${
                        selectedCategory === c.categoryId ? "bg-blue-700 text-white" : "hover:bg-gray-100"
                        }`}
                        onClick={() => handleCategorySelection(c.categoryId)}
                      >
                        {c.categoryName}
                      </button>
                    ))}
                  </div>
                )}
              </div>

              {/* Browse All */}
              <button
              className="px-5 py-2 rounded-full bg-indigo-700 text-white text-base font-medium flex items-center gap-2 hover:bg-indigo-800 transition"
                onClick={handleBrowseAll}
              >
                <FaFilter /> Browse All
              </button>
            </div>

          {/* Search */}
          <form onSubmit={handleSearch} className="relative flex-none w-full sm:w-80 ml-4">
            <FaSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500 text-lg" />
              <input
                type="text"
                placeholder="Search knowledge..."
                value={keyword}
                onChange={(e) => setKeyword(e.target.value)}
              className="w-full border border-gray-300 rounded-full pl-10 pr-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400 text-gray-800 text-base bg-gray-50"
              />
            </form>
          </div>
        </div>
      </div>

      {/* Main Section */}
<div className="pt-0 px-6 w-full">
  {isSearching && <p className="text-gray-500 text-lg mt-3 text-center">🔍 Searching...</p>}
  {error && <p className="text-red-500 text-lg mt-3 text-center">{error}</p>}

  {/* Knowledge Cards */}
  {searchResults.length > 0 && (
    <KnowledgeCardsDisplay
      items={searchResults}
      title="Knowledge Articles"
      userId={userId}
      onPreview={(item) => setSelectedItem(item)}
      onLike={onLike}
      onFavourite={onFavourite}
      onComment={onComment}
      engagement={engagement}
      onReset={handleReset}
    />
  )}
</main>


      {/* Preview Modal */}
      {previewItem && (
        <div className="fixed inset-0 bg-black bg-opacity-40 flex justify-center items-center z-[999]">
          <div className="bg-white rounded-xl shadow-2xl p-6 max-w-lg w-full relative border border-gray-200">
            <button
              className="absolute top-3 right-3 text-gray-500 hover:text-gray-800 font-bold text-lg"
              onClick={() => setPreviewItem(null)}
            >
              ✖
            </button>
            <h2 className="text-2xl font-bold text-indigo-700 mb-4 border-b pb-2">
              {previewItem.title || previewItem.name}
            </h2>
            <div className="flex flex-wrap gap-2 mb-4">
              <span className="px-3 py-1 rounded border border-gray-300 bg-indigo-50 text-indigo-700 text-sm">
                Domain: {previewItem.domainName}
              </span>
              <span className="px-3 py-1 rounded border border-gray-300 bg-indigo-50 text-indigo-700 text-sm">
                Category: {previewItem.categoryName}
              </span>
            </div>
            <div className="mb-4">
              <h3 className="font-semibold text-gray-800 mb-1">Description</h3>
              <p className="text-sm text-gray-700 leading-relaxed">
                {previewItem.description || "No description available."}
              </p>
            </div>
            <div className="mb-6">
              <h3 className="font-semibold text-gray-800 mb-1">Snippet</h3>
              <p className="text-sm text-gray-700 leading-relaxed">
                {previewItem.snippet || "No snippet available."}
              </p>
            </div>

            <div className="flex justify-end">
              <button
                className="px-4 py-1 text-sm bg-indigo-100 text-indigo-700 rounded hover:bg-indigo-200 transition"
                onClick={() => setPreviewItem(null)}
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
