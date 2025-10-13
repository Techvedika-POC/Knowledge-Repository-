import React, { useState, useEffect, useRef } from "react";
import api from "../api";
import { FaSearch, FaLayerGroup, FaFolderOpen, FaFilter } from "react-icons/fa";

const Navbar = () => {
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
  const [previewItem, setPreviewItem] = useState(null);
  const [showAll, setShowAll] = useState(false);

  const domainDropdownRef = useRef(null);
  const categoryDropdownRef = useRef(null);

  useEffect(() => {
    const fetchFilters = async () => {
      try {
        const [domainRes, categoryRes] = await Promise.all([
          api.get("/Domains"),
          api.get("/Categories"),
        ]);
        setDomains(domainRes.data);
        setCategories(categoryRes.data);
      } catch (err) {
        console.error("Error fetching filter data", err);
      }
    };
    fetchFilters();
  }, []);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (
        domainDropdownRef.current &&
        !domainDropdownRef.current.contains(event.target)
      )
        setShowDomainDropdown(false);
      if (
        categoryDropdownRef.current &&
        !categoryDropdownRef.current.contains(event.target)
      )
        setShowCategoryDropdown(false);
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleDomainSelection = async (domainId) => {
    setSelectedDomain(domainId);
    setSelectedCategory("");
    setKeyword("");
    setIsSearching(true);
    setShowDomainDropdown(false);
    try {
      const res = await api.get(`/KnowledgeItem/ByDomain/${domainId}`);
      setSearchResults(res.data || []);
    } catch {
      setError("Error fetching items by domain.");
    } finally {
      setIsSearching(false);
    }
  };

  const handleCategorySelection = async (categoryId) => {
    setSelectedCategory(categoryId);
    setSelectedDomain("");
    setKeyword("");
    setIsSearching(true);
    setShowCategoryDropdown(false);
    try {
      const res = await api.get(`/KnowledgeItem/ByCategory/${categoryId}`);
      setSearchResults(res.data || []);
    } catch {
      setError("Error fetching items by category.");
    } finally {
      setIsSearching(false);
    }
  };

  const handleBrowseAll = async () => {
    setSelectedDomain("");
    setSelectedCategory("");
    setKeyword("");
    setIsSearching(true);
    try {
      const res = await api.get("/KnowledgeItem/All");
      setSearchResults(res.data || []);
    } catch {
      setError("Error fetching all items.");
    } finally {
      setIsSearching(false);
    }
  };

  const handleSearch = async (e) => {
    e.preventDefault();
    if (!keyword.trim()) return;
    setSelectedDomain("");
    setSelectedCategory("");
    setIsSearching(true);
    try {
      const res = await api.get("/GlobalSearch", { params: { keyword } });
      setSearchResults(res.data || []);
    } catch {
      setError("Search failed.");
    } finally {
      setIsSearching(false);
    }
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
      <div
        className="sticky top-0 left-0 w-full bg-white shadow-md border-b z-10 px-4"
        style={{ height: "50px", minHeight: "40px", borderColor: "#e5e7eb" }}
      >
        <div className="flex justify-between items-center px-0.25 py-0.25 w-full">
          <div className="flex justify-between items-center gap-3 w-full">
            {/* Filters */}
            <div className="flex gap-3 items-center">
              {/* Domain Dropdown */}
              <div className="relative" ref={domainDropdownRef}>
                <button
                  onClick={() => setShowDomainDropdown(!showDomainDropdown)}
                  className={`px-4 py-2 rounded-full text-sm flex items-center gap-1 transition ${
                    selectedDomain
                      ? "bg-cyan-700 text-white"
                      : "bg-cyan-100 text-cyan-700 hover:bg-cyan-200"
                  }`}
                >
                  <FaLayerGroup /> Domain
                </button>
                {showDomainDropdown && (
                  <div className="absolute top-full left-0 mt-1 bg-white shadow-lg rounded-lg p-2 z-50 w-48 max-h-48 overflow-auto border border-gray-200">
                    {domains.map((d) => (
                      <button
                        key={d.domainId}
                        className={`w-full text-left px-3 py-1 rounded text-sm ${
                          selectedDomain === d.domainId
                            ? "bg-cyan-700 text-white"
                            : "hover:bg-gray-100"
                        }`}
                        onClick={() => handleDomainSelection(d.domainId)}
                      >
                        {d.domainName}
                      </button>
                    ))}
                  </div>
                )}
              </div>

              {/* Category Dropdown */}
              <div className="relative" ref={categoryDropdownRef}>
                <button
                  onClick={() => setShowCategoryDropdown(!showCategoryDropdown)}
                  className={`px-3 py-1 rounded-full text-sm flex items-center gap-1 transition ${
                    selectedCategory
                      ? "bg-blue-700 text-white"
                      : "bg-blue-100 text-blue-700 hover:bg-blue-200"
                  }`}
                >
                  <FaFolderOpen /> Category
                </button>
                {showCategoryDropdown && (
                  <div className="absolute top-full left-0 mt-1 bg-white shadow-lg rounded-lg p-2 z-50 w-48 max-h-48 overflow-auto border border-gray-200">
                    {categories.map((c) => (
                      <button
                        key={c.categoryId}
                        className={`w-full text-left px-3 py-1 rounded text-sm ${
                          selectedCategory === c.categoryId
                            ? "bg-blue-700 text-white"
                            : "hover:bg-gray-100"
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
                className="px-3 py-1 rounded-full bg-indigo-700 text-white text-sm flex items-center gap-1 hover:bg-indigo-800 transition"
                onClick={handleBrowseAll}
              >
                <FaFilter /> Browse All
              </button>
            </div>

            {/* Search Box */}
            <form
              onSubmit={handleSearch}
              className="relative flex-none w-full sm:w-64 ml-4"
            >
              <FaSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500 text-sm" />
              <input
                type="text"
                placeholder="Search knowledge..."
                value={keyword}
                onChange={(e) => setKeyword(e.target.value)}
                className="w-full border border-gray-300 rounded-full pl-8 pr-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-400 text-gray-800 text-sm bg-gray-50"
              />
            </form>
          </div>
        </div>
      </div>

     <main className="pt-4 px-4 w-full">
  {/* Heading */}
  {searchResults.length > 0 && (
    <h2 className="text-1xl font-bold text-gray-800 mb-2">
      Knowledge Articles
    </h2>
  )}

  {/* Searching */}
  {isSearching && (
    <p className="text-gray-500 text-sm mt-2 text-center">🔍 Searching...</p>
  )}

  {/* Error */}
  {error && <p className="text-red-500 text-sm mt-2 text-center">{error}</p>}

  {/* Cards */}
  {searchResults.length > 0 && (
    <div className="bg-gray-50 rounded-xl p-3 shadow-sm flex flex-col gap-4 mt-2">
      {chunkedResults.slice(0, rowsToShow).map((row, rowIndex) => (
        <div
          key={rowIndex}
          className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 w-full"
        >
          {row.map((item, idx) => (
            <div
              key={idx}
              className="bg-white rounded-lg shadow-md p-4 flex flex-col transition-transform transform hover:-translate-y-1 hover:scale-102"
            >
              <h4 className="text-lg font-semibold text-gray-800 mb-2 truncate">
                {item.title || item.name}
              </h4>
              <div className="flex flex-col gap-1 text-sm text-gray-700">
                <span className="font-medium">
                  Domain: {item.domainName}
                </span>
                <span className="font-medium">
                  Category: {item.categoryName}
                </span>
              </div>
              <p className="text-sm mt-2 text-gray-600 flex-grow">
                {item.description
                  ? item.description.substring(0, 100) + "..."
                  : "No description available."}
              </p>
              <div className="flex gap-2 mt-3 text-sm">
                <button className="text-blue-600 hover:underline">
                  👍 Like
                </button>
                <button className="text-pink-600 hover:underline">
                  💬 Comment
                </button>
                <button
                  className="text-purple-600 hover:underline"
                  onClick={() => setPreviewItem(item)}
                >
                  👁 Preview
                </button>
              </div>
            </div>
          ))}
        </div>
      ))}

      {/* Controls: View More / Reset */}
      <div className="flex justify-center gap-2 mt-3">
        {searchResults.length > 3 && (
          <button
            className="px-3 py-1 bg-purple-500 text-white rounded text-sm hover:bg-purple-600 transition"
            onClick={() => setShowAll(!showAll)}
          >
            {showAll ? "View Less" : "View More"}
          </button>
        )}
        <button
          className="px-3 py-1 bg-gray-100 text-gray-800 rounded text-sm hover:bg-gray-200 transition"
          onClick={handleReset}
        >
          Reset
        </button>
      </div>
    </div>
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
};

export default Navbar;
