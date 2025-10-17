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
  const [mappedTrending, setMappedTrending] = useState([]);


  // Load userId from localStorage on mount
  useEffect(() => {
    const storedUserId = localStorage.getItem("userId");
    if (storedUserId) setUserId(storedUserId);
  }, []);

  // Normalize contributor/owner name
  useEffect(() => {
    const mapped = trending.map(item => ({
      ...item,
      ownerName: item.ownerName || item.contributorName || "Unknown Contributor",
    }));
    setMappedTrending(mapped);
  }, [trending]);

  return (
    <>
      <KnowledgeCardsDisplay
        items={mappedTrending}
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
          userId={userId}
        />
      )}
    </>
  );
}
