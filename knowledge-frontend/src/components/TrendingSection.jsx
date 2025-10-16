import React, { useState, useEffect } from "react";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";

export default function TrendingSection({ trending = [] }) {
  const [selectedItem, setSelectedItem] = useState(null);
  const [userId, setUserId] = useState(null);
  const [mappedTrending, setMappedTrending] = useState([]);

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
