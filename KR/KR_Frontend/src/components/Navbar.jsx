import React, { useState, useEffect, useRef } from "react";
import api from "../api";
import { FaSearch, FaLayerGroup, FaFolderOpen, FaFilter } from "react-icons/fa";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";

export default function Navbar() {
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
  const [userId, setUserId] = useState(null);
  const [isBrowseAllActive, setIsBrowseAllActive] = useState(false);


  const [engagement, setEngagement] = useState({
    likedItems: [],
    favouritedItems: [],
  });

  const domainDropdownRef = useRef(null);
  const categoryDropdownRef = useRef(null);

  useEffect(() => {
    const storedUserId = localStorage.getItem("userId");
    if (storedUserId) setUserId(storedUserId);
  }, []);

  useEffect(() => {
    if (!userId) return;

    const fetchUserEngagements = async () => {
      try {
        const res = await api.get(`/engagement/user-engagements/${userId}`);
        const likedItems = res.data
          .filter((e) => e.engagementType === "Like")
          .map((e) => e.itemId);
        const favouritedItems = res.data
          .filter((e) => e.engagementType === "Favourite")
          .map((e) => e.itemId);

        setEngagement({ likedItems, favouritedItems });
      } catch (error) {
        console.error("Failed to load user engagements:", error);
      }
    };

    fetchUserEngagements();
  }, [userId]);

  const updateEngagement = (newEngagement) => {
    setEngagement(newEngagement);
    localStorage.setItem("engagement", JSON.stringify(newEngagement));
  };
  const handleLikeClick = async (item) => {
    if (!userId) return;

    const itemId = item.itemId || item.id;
    const alreadyLiked = engagement.likedItems.includes(itemId);
    const likedItems = alreadyLiked
      ? engagement.likedItems.filter((id) => id !== itemId)
      : [...engagement.likedItems, itemId];

    updateEngagement({ ...engagement, likedItems });

    try {
      if (alreadyLiked) {
        await api.delete(`/engagement/like/${itemId}?userId=${userId}`);
      } else {
        await api.post(`/engagement/like/${itemId}?userId=${userId}`);
      }

      setSearchResults((prev) =>
        prev.map((i) =>
          i.id === itemId
            ? { ...i, likesCount: (i.likesCount || 0) + (alreadyLiked ? -1 : 1) }
            : i
        )
      );
    } catch (error) {
      updateEngagement(engagement);
    }
  };
  const handleFavouriteClick = async (item) => {
    if (!userId) return;

    const itemId = item.itemId || item.id;
    const alreadyFavourited = engagement.favouritedItems.includes(itemId);
    const favouritedItems = alreadyFavourited
      ? engagement.favouritedItems.filter((id) => id !== itemId)
      : [...engagement.favouritedItems, itemId];

    updateEngagement({ ...engagement, favouritedItems });

    try {
      if (alreadyFavourited) {
        await api.delete(`/engagement/favourite/${itemId}?userId=${userId}`);
      } else {
        await api.post(`/engagement/favourite/${itemId}?userId=${userId}`);
      }
    } catch (error) {
      updateEngagement(engagement);
    }
  };

  const handleCommentClick = async (item, commentText) => {
    if (!userId || !commentText.trim()) return;

    const itemId = item.itemId || item.id;
    try {
      const res = await api.post(
        `/engagement/comment/${itemId}?userId=${userId}`,
        { commentText: commentText.trim() },
        { headers: { "Content-Type": "application/json" } }
      );

      const newComment = {
        text: commentText.trim(),
        userName: "You",
        timestamp: Date.now(),
      };
      setSearchResults((prev) =>
        prev.map((i) =>
          i.id === itemId
            ? { ...i, comments: [...(i.comments || []), newComment] }
            : i
        )
      );
    } catch (error) {
      console.error("Failed to post comment:", error);
    }
  };

  useEffect(() => {
    const fetchFilters = async () => {
      try {
        const [domainRes, categoryRes] = await Promise.all([
          api.get("/Domains"),
          api.get("/Categories"),
        ]);
        setDomains(domainRes.data || []);
        setCategories(categoryRes.data || []);
      } catch (err) {
        console.error("Error fetching filters:", err);
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
      const items = (res.data || []).map((i) => ({
        ...i,
        id: i.itemId || i.id,
      }));
      setSearchResults(items);
    } catch (err) {
      setError("Error fetching items");
    } finally {
      setIsSearching(false);
    }
  };

  const handleDomainSelection = (id) => {
    setSelectedDomain(id);
    setSelectedCategory("");
    setShowDomainDropdown(false);
    fetchItems(`/KnowledgeItem/ByDomain/${id}`);
  };

  const handleCategorySelection = (id) => {
    setSelectedCategory(id);
    setSelectedDomain("");
    setShowCategoryDropdown(false);
    fetchItems(`/KnowledgeItem/ByCategory/${id}`);
  };

  const handleBrowseAll = () => {
    setSelectedDomain("");
    setSelectedCategory("");
    setIsBrowseAllActive(true);
    fetchItems("/KnowledgeItem/All");
  };
  const handleSearch = (e) => {
    e.preventDefault();
    if (!keyword.trim()) return;
    fetchItems(`/GlobalSearch?keyword=${encodeURIComponent(keyword)}`);
  };

  const handleReset = () => {
    setSelectedDomain("");
    setSelectedCategory("");
    setKeyword("");
    setSearchResults([]);
    setError("");
    setEngagement({ likedItems: [], favouritedItems: [] });
  };

  return (
    <>
      <div className="sticky top-0 bg-white shadow-md border-b z-10 px-4 py-2 mb-0">
        <div className="flex justify-between items-center gap-3">
          <div className="flex gap-4 items-center">
            <div className="relative" ref={domainDropdownRef}>
              <button
                onClick={() => setShowDomainDropdown(!showDomainDropdown)}
                className={`px-5 py-2 rounded-full flex items-center gap-2
                ${selectedDomain
                    ? "bg-indigo-300 text-indigo-900"
                    : "bg-cyan-100 text-cyan-700"}
                hover:bg-cyan-200
              `}
              >
                <FaLayerGroup /> Domain
              </button>
              {showDomainDropdown && (
                <div className="absolute top-full left-0 mt-1 bg-white shadow-lg rounded-lg p-2 z-50 w-56">
                  {domains.map((d) => (
                    <button
                      key={d.domainId}
                      onClick={() => handleDomainSelection(d.domainId)}
                      className="w-full text-left px-3 py-2 hover:bg-gray-100"
                    >
                      {d.domainName}
                    </button>
                  ))}
                </div>
              )}
            </div>

            <div className="relative" ref={categoryDropdownRef}>
              <button
                onClick={() => setShowCategoryDropdown(!showCategoryDropdown)}
                className={`px-5 py-2 rounded-full flex items-center gap-2
                ${selectedCategory
                    ? "bg-indigo-300 text-indigo-900"
                    : "bg-blue-100 text-blue-700"}
                hover:bg-blue-200
              `}
              >
                <FaFolderOpen /> Category
              </button>
              {showCategoryDropdown && (
                <div className="absolute top-full left-0 mt-1 bg-white shadow-lg rounded-lg p-2 z-50 w-56">
                  {categories.map((c) => (
                    <button
                      key={c.categoryId}
                      onClick={() => handleCategorySelection(c.categoryId)}
                      className="w-full text-left px-3 py-2 hover:bg-gray-100"
                    >
                      {c.categoryName}
                    </button>
                  ))}
                </div>
              )}
            </div>
            <button
              className={`px-5 py-2 rounded-full flex items-center gap-2
    ${isBrowseAllActive
                  ? "bg-amber-200 text-amber-900"   
                  : "bg-amber-100 text-amber-800"}
  `}
              onClick={handleBrowseAll}
            >
              Browse All
            </button>
          </div>
          <form
            onSubmit={handleSearch}
            className="relative w-full sm:w-80 ml-4 flex items-center gap-2"
          >
            <FaSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500" />
            <input
              type="text"
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              placeholder="Search knowledge..."
              className="w-full border border-gray-300 rounded-full pl-10 pr-8 py-2 focus:ring-2 focus:ring-blue-400"
            />

            {searchResults.length > 0 && (
              <button
                type="button"
                onClick={handleReset}
                className="text-gray-400 hover:text-red-500"
                title="Reset"
              >
                ✕
              </button>
            )}
          </form>
        </div>
      </div>

      <div className="px-6 w-full">
        {isSearching && <p className="text-center text-gray-500">Searching...</p>}
        {error && <p className="text-center text-red-500">{error}</p>}

        {searchResults.length > 0 && (
          <KnowledgeCardsDisplay
            items={searchResults.map((item) => ({
              ...item,
              isLiked: engagement.likedItems.includes(item.id),
              isFav: engagement.favouritedItems.includes(item.id),
              likeCount: item.likesCount || 0,
              comments: item.comments || [],
            }))}
            title="Filtered Knowledge Articles"
            userId={userId}
            onPreview={(item) => setSelectedItem(item)}
            onLike={handleLikeClick}
            onFavourite={handleFavouriteClick}
            onComment={handleCommentClick}
          />
        )}

        {selectedItem && (
          <PreviewModal
            item={selectedItem}
            onClose={() => setSelectedItem(null)}
          />
        )}
      </div>
    </>
  );

}
