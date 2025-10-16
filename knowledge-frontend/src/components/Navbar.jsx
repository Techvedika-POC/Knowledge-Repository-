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
  };

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
</div>

    </>
  );
}
