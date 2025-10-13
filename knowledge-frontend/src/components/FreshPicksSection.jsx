import React, { useState, useEffect } from "react";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";
import axios from "axios"; // for API calls

export default function FreshPicksSection({
  freshPicks,
  handleLike,
  handleFavourite,
  handleComment,
  userId, // <- we need userId to fetch user engagements
}) {
  const [selectedItem, setSelectedItem] = useState(null);
  const [engagement, setEngagement] = useState({
    likedItems: [],
    favouritedItems: [],
  });

  // Fetch user engagements from backend when component mounts
  useEffect(() => {
    const fetchUserEngagements = async () => {
      if (!userId) {
        console.warn("User ID is missing — skipping engagement fetch.");
        return;
      }

      try {
        const res = await axios.get(`/api/engagement/user-engagements/${userId}`);
        const likedItems = res.data
          .filter(e => e.engagementType === "Like")
          .map(e => e.itemId);
        const favouritedItems = res.data
          .filter(e => e.engagementType === "Favourite")
          .map(e => e.itemId);

        setEngagement({ likedItems, favouritedItems });
      } catch (error) {
        console.error("Failed to load user engagements:", error);
      }
    };

    fetchUserEngagements();
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

    try {
      if (alreadyLiked) {
        await axios.delete(`/api/engagement/like/${itemId}?userId=${userId}`);
      } else {
        await axios.post(`/api/engagement/like/${itemId}?userId=${userId}`);
      }
      handleLike(item); // Callback to update parent state/UI
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

    try {
      if (alreadyFavourited) {
        await axios.delete(`/api/engagement/favourite/${itemId}?userId=${userId}`);
      } else {
        await axios.post(`/api/engagement/favourite/${itemId}?userId=${userId}`);
      }
      handleFavourite(item); // Callback to update parent state/UI
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
        { commentText },  // ✅ JSON body matches backend DTO
        { headers: { "Content-Type": "application/json" } }
      );

      // (Optional) Refetch engagement data for this item to show new comment
      // Or you can optimistically append it to state if you want instant update
    } catch (error) {
      console.error("Failed to post comment:", error);
    }
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
