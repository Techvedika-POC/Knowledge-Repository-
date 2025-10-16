import React, { useState, useEffect } from "react";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";
import axios from "axios";
import api from "../api";

export default function FreshPicksSection({
  freshPicks,
  handleLike,
  handleFavourite,
  handleComment,
  userId: propUserId, 
}) {
  // Get userId from localStorage if not passed as prop
  const userId = propUserId || localStorage.getItem("userId");

  const [selectedItem, setSelectedItem] = useState(null);
  const [engagement, setEngagement] = useState({
    likedItems: [],
    favouritedItems: [],
  });
  const [showFreshPicks, setShowFreshPicks] = useState(true); 

  // Fetch user engagements on mount
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
    if (!userId) {
      console.warn("Cannot like — user not logged in.");
      return;
    }

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
      handleLike(item);
    } catch (error) {
      console.error("Failed to update like:", error);
      updateEngagement(engagement); 
    }
  };

  const handleFavouriteClick = async (item) => {
    if (!userId) {
      console.warn("Cannot favourite — user not logged in.");
      return;
    }

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
      handleFavourite(item);
    } catch (error) {
      console.error("Failed to update favourite:", error);
      updateEngagement(engagement); // revert UI
    }
  };

  const handleCommentClick = async (itemId, commentText) => {
    if (!userId) {
      console.warn("Cannot comment — user not logged in.");
      return;
    }
    if (!commentText.trim()) return;

    try {
      await api.post(
        `/engagement/comment/${itemId}?userId=${userId}`,
        { commentText: commentText.trim() },
        { headers: { "Content-Type": "application/json" } }
      );
      handleComment(itemId, commentText);
    } catch (error) {
      console.error("Failed to post comment:", error);
    }
  };
     const handleReset = () => {
    setShowFreshPicks(false);
    setSelectedItem(null);
  };

  return (
    <>
      <KnowledgeCardsDisplay
        items={freshPicks}
        title="Fresh Picks"
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
