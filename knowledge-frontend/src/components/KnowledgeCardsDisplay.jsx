import React, { useState, useEffect } from "react";
import axios from "axios";
import api from "../api";
import EngagementButtons from "./EngagementButtons";
import { FileText, Tag, User, Type } from "lucide-react";

export default function KnowledgeCardsDisplay({ items = [], title, onPreview, userId,onReset }) {
  const [showAll, setShowAll] = useState(false);
  const [engagementData, setEngagementData] = useState({});
  const [loading, setLoading] = useState(false);

  const themeColor = "indigo"; 
  const cardHighlightColor = "blue"; 

  // Fetch engagement summary for all items
  const fetchEngagementData = async () => {
    if (!userId) return;
    setLoading(true);
    const data = {};
    try {
      await Promise.all(
        items.map(async (item) => {
          const id = item.itemId || item.id;
          if (!id) return;
          const res = await axios.get(`/api/Engagement/summary/${id}?userId=${userId}`);
          data[id] = res.data;
        })
      );
      setEngagementData(data);
    } catch (err) {
      console.error("Error fetching engagement data", err);
    }
    setLoading(false);
  };

  useEffect(() => {
    if (items.length) fetchEngagementData();
  }, [items, userId]);

  // Engagement handlers (fully internal)
  const handleLike = async (item) => {
    const id = item.itemId || item.id;
    if (!id) return;
    const isLiked = engagementData[id]?.userEngagementTypes?.includes("Like");
    try {
      if (isLiked) {
        await axios.delete(`/api/Engagement/like/${id}`, { params: { userId } });
      } else {
        await axios.post(`/api/Engagement/like/${id}`, null, { params: { userId } });
      }
      fetchEngagementData();
    } catch (err) {
      console.error("Error liking/unliking item", err);
    }
  };

  const handleFavourite = async (item) => {
    const id = item.itemId || item.id;
    if (!id) return;
    const isFav = engagementData[id]?.userEngagementTypes?.includes("Favourite");
    try {
      if (isFav) {
        await axios.delete(`/api/Engagement/favourite/${id}`, { params: { userId } });
      } else {
        await axios.post(`/api/Engagement/favourite/${id}`, null, { params: { userId } });
      }
      fetchEngagementData();
    } catch (err) {
      console.error("Error favouriting/unfavouriting item", err);
    }
  };

  const handleComment = async (item, commentText) => {
    const id = item.itemId || item.id;
    if (!id) return;
    try {
      await axios.post(`/api/Engagement/comment/${id}`, { CommentText: commentText }, { params: { userId } });
      fetchEngagementData();
    } catch (error) {
      console.error("Error adding comment", error);
    }
  };

  return (
    <div className="pb-16 p-6">
      {loading && <p className="text-center text-gray-500">Loading engagements...</p>}

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {items.slice(0, showAll ? items.length : 3).map((item, idx) => {
          const id = item.itemId || item.id;
          const data = engagementData[id] || {};

          return (
            <div key={id || idx} className="bg-white rounded-2xl shadow-md border border-gray-200 hover:shadow-lg transition flex flex-col">
              <div className={`h-2 w-full rounded-t-2xl bg-${cardHighlightColor}-100`}></div>

              <div className="p-6 flex flex-col flex-grow">
                <div className="mb-4 p-4 rounded bg-gray-50 flex flex-col gap-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Type className="w-4 h-4 text-purple-500" />
                      <span className="font-semibold text-sm text-gray-600">Title:</span>
                    </div>
                    <span className="font-medium text-gray-900">{item.title}</span>
                  </div>

                                        {/* Description */}
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <FileText className="w-4 h-4 text-indigo-500" />
                      <span className="font-semibold text-sm text-gray-600">Description:</span>
                    </div>
                    <span className="text-gray-700 text-sm line-clamp-3">
                      {item.description || "No description available."}
                    </span>
                  </div>
                </div>

                                {/* Tags */}
                {item.tags?.length > 0 && (
                  <div className="flex flex-wrap gap-2 mb-4">
                    {item.tags.map((tag, tIdx) => (
                      <span key={tIdx} className={`px-2 py-1 text-xs rounded-full bg-${themeColor}-100 text-${themeColor}-800 font-medium flex items-center gap-1`}>
                        <Tag className="w-3 h-3" /> {tag}
                      </span>
                    ))}
                  </div>
                )}

                                {/* Contributor */}
               <div className="flex items-center gap-2 mb-4">
  <User className={`w-5 h-5 text-${themeColor}-500`} />
  <span className="px-2 py-1 text-xs font-medium text-${themeColor}-800 rounded-full hover:bg-blue-100 transition cursor-pointer">
     {item.ownerName || item.submittedBy || "Unknown Contributor"}
  </span>
</div>


                <div className="border-t border-gray-300 mb-1"></div>

                <div className="mt-auto flex justify-center">
                  <EngagementButtons
                    item={item}
                    onPreview={onPreview}
                    onLike={handleLike}
                    onFavourite={handleFavourite}
                    onComment={handleComment}
                    isLiked={data.userEngagementTypes?.includes("Like")}
                    isFav={data.userEngagementTypes?.includes("Favourite")}
                    likeCount={data.likesCount || 0}
                    comments={data.comments || []}
                    themeColor={themeColor}
                  />
                </div>
              </div>
            </div>
          );
        })}
      </div>

          {/* View More & Reset Buttons */}
      {items.length > 3 && (
        <div className="flex justify-center mt-8">
          <button
            onClick={() => setShowAll(!showAll)}
            className={`px-6 py-2 text-sm font-medium text-black bg-blue-300 rounded-full shadow hover:bg-${themeColor}-600 transition`}
          >
            {showAll ? "View Less" : "View More"}
          </button>

    {/* Reset Button */}
    <button
      onClick={() => {
        setShowAll(false);
        fetchEngagementData();
         if (onReset) onReset();
      }}
      className={`px-6 py-2 text-sm font-medium text-black bg-blue-300 rounded-full shadow hover:bg-${themeColor}-600 transition`}
    >
      Reset
    </button>
        </div>
      )}

    </div>
  );
}

