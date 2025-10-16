import React, { useState, useEffect } from "react";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";
import api from "../api";

export default function TrendingSection({ trending = [] }) {
  const [selectedItem, setSelectedItem] = useState(null);
  const [engagement, setEngagement] = useState({
    likedItems: [],
    favouritedItems: [],
  });
  const [userId, setUserId] = useState(null);
  const [showTrending, setShowTrending] = useState(true);


  // Load userId from localStorage on mount
  useEffect(() => {
    const storedUserId = localStorage.getItem("userId");
    if (storedUserId) {
      setUserId(storedUserId);
      console.log("Loaded userId from localStorage:", storedUserId);
    } else {
      console.warn("No userId in localStorage.");
    }
  }, []);

  // Fetch user engagements when userId is available
  useEffect(() => {
    if (!userId) return;

    const fetchUserEngagements = async () => {
      // Load from localStorage first
      const storedEngagement = localStorage.getItem("engagement");
      if (storedEngagement) {
        console.log("Loading engagement from localStorage");
        setEngagement(JSON.parse(storedEngagement));
      }

      try {
        const res = await api.get(`/engagement/user-engagements/${userId}`);
        console.log("Engagement fetched:", res.data);

        const likedItems = res.data
          .filter((e) => e.engagementType === "Like")
          .map((e) => e.itemId);

        const favouritedItems = res.data
          .filter((e) => e.engagementType === "Favourite")
          .map((e) => e.itemId);

        const newEngagement = { likedItems, favouritedItems };
        setEngagement(newEngagement);
        localStorage.setItem("engagement", JSON.stringify(newEngagement));
      } catch (error) {
        console.error(
          "Failed to load user engagements:",
          error.response?.data || error.message
        );
      }
    };

    fetchUserEngagements();
  }, [userId]);

  // Update engagement in state and localStorage
  const updateLocalStorage = (newEngagement) => {
    setEngagement(newEngagement);
    localStorage.setItem("engagement", JSON.stringify(newEngagement));
  };

  // Like/unlike handler
  const handleLikeClick = async (item) => {
    const itemId = item.itemId || item.id;
    const alreadyLiked = engagement.likedItems.includes(itemId);

    const likedItems = alreadyLiked
      ? engagement.likedItems.filter((id) => id !== itemId)
      : [...engagement.likedItems, itemId];

    updateLocalStorage({ ...engagement, likedItems });

    if (!userId) return;

    try {
      if (alreadyLiked) {
        await api.delete(`/engagement/like/${itemId}?userId=${userId}`);
      } else {
        await api.post(`/engagement/like/${itemId}?userId=${userId}`);
      }
    } catch (error) {
      console.error(
        "Failed to update like:",
        error.response?.data || error.message
      );
    }
  };

  // Favourite/unfavourite handler
  const handleFavouriteClick = async (item) => {
    const itemId = item.itemId || item.id;
    const alreadyFavourited = engagement.favouritedItems.includes(itemId);

    const favouritedItems = alreadyFavourited
      ? engagement.favouritedItems.filter((id) => id !== itemId)
      : [...engagement.favouritedItems, itemId];

    updateLocalStorage({ ...engagement, favouritedItems });

    if (!userId) return;

    try {
      if (alreadyFavourited) {
        await api.delete(`/engagement/favourite/${itemId}?userId=${userId}`);
      } else {
        await api.post(`/engagement/favourite/${itemId}?userId=${userId}`);
      }
    } catch (error) {
      console.error(
        "Failed to update favourite:",
        error.response?.data || error.message
      );
    }
  };

  // Comment handler
  const handleCommentClick = async (itemId, commentText) => {
    if (!userId) return;

    try {
      await api.post(`/engagement/comment/${itemId}?userId=${userId}`, {
        commentText,
      });
    } catch (error) {
      console.error(
        "Failed to post comment:",
        error.response?.data || error.message
      );
    }
  };
  const handleReset = () => {
  setShowTrending(false);
  setSelectedItem(null);
};


  return (
    <>
      <KnowledgeCardsDisplay
        items={trending}
        title="Trending"
        userId={userId}
        onPreview={(item) => setSelectedItem(item)}
        onLike={handleLikeClick}
        onFavourite={handleFavouriteClick}
        onComment={handleCommentClick}
        engagement={engagement}
          onReset={handleReset}
     
      />

      {selectedItem && (
        <PreviewModal
          item={selectedItem}
          onClose={() => setSelectedItem(null)}
          onLike={handleLikeClick}
          onFavourite={handleFavouriteClick}
          onComment={handleCommentClick}
          engagement={engagement}
     
        />
      )}
    </>
  );
}
