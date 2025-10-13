import React, { useState, useEffect } from "react";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";
import axios from "axios";

export default function TrendingSection({ trending = [] }) {
  const [selectedItem, setSelectedItem] = useState(null);
  const [engagement, setEngagement] = useState({
    likedItems: [],
    favouritedItems: [],
  });
  const [userId, setUserId] = useState(null);
  useEffect(() => {
    const storedUserId = localStorage.getItem("userId");
    if (storedUserId) {
      setUserId(storedUserId);
      console.log("Loaded userId from localStorage:", storedUserId);
    } else {
      console.warn("No userId in localStorage.");
    }
  }, []);

  useEffect(() => {
    const fetchUserEngagements = async () => {
      console.log("Fetching engagements for userId:", userId);

      // Load from localStorage first
      const storedEngagement = localStorage.getItem("engagement");
      if (storedEngagement) {
        console.log("Loading engagement from localStorage");
        setEngagement(JSON.parse(storedEngagement));
      }

      if (!userId) {
        console.warn("No userId yet — skipping API call.");
        return;
      }

      try {
        const res = await axios.get(
          `/api/engagement/user-engagements/${userId}`
        );
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
        console.error("Failed to load user engagements:", error);
      }
    };

    if (userId) {
      fetchUserEngagements();
    }
  }, [userId]);

  const updateLocalStorage = (newEngagement) => {
    setEngagement(newEngagement);
    localStorage.setItem("engagement", JSON.stringify(newEngagement));
  };

  const handleLikeClick = async (item) => {
    const itemId = item.itemId || item.id;
    const alreadyLiked = engagement.likedItems.includes(itemId);

    const likedItems = alreadyLiked
      ? engagement.likedItems.filter((id) => id !== itemId)
      : [...engagement.likedItems, itemId];

    updateLocalStorage({ ...engagement, likedItems });

    if (!userId) {
      console.warn("No user ID — like stored locally only");
      return;
    }

    try {
      if (alreadyLiked) {
        await axios.delete(`/api/engagement/like/${itemId}?userId=${userId}`);
      } else {
        await axios.post(`/api/engagement/like/${itemId}?userId=${userId}`);
      }
    } catch (error) {
      console.error("Failed to update like:", error);
    }
  };

  const handleFavouriteClick = async (item) => {
    const itemId = item.itemId || item.id;
    const alreadyFavourited = engagement.favouritedItems.includes(itemId);

    const favouritedItems = alreadyFavourited
      ? engagement.favouritedItems.filter((id) => id !== itemId)
      : [...engagement.favouritedItems, itemId];

    updateLocalStorage({ ...engagement, favouritedItems });

    if (!userId) {
      console.warn("No user ID — favourite stored locally only");
      return;
    }

    try {
      if (alreadyFavourited) {
        await axios.delete(
          `/api/engagement/favourite/${itemId}?userId=${userId}`
        );
      } else {
        await axios.post(
          `/api/engagement/favourite/${itemId}?userId=${userId}`
        );
      }
    } catch (error) {
      console.error("Failed to update favourite:", error);
    }
  };

  const handleCommentClick = async (itemId, commentText) => {
    if (!userId) {
      console.warn("Cannot post comment — user not logged in.");
      return;
    }

    try {
      await axios.post(
        `/api/engagement/comment/${itemId}?userId=${userId}`,
        { commentText },
        { headers: { "Content-Type": "application/json" } }
      );
    } catch (error) {
      console.error("Failed to post comment:", error);
    }
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
