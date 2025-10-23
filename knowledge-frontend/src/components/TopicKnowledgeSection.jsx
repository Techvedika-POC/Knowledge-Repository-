import React, { useState } from "react";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";

export default function TopicKnowledgeSection({
  selectedTopic,
  knowledgeItems,
  userId,
  engagement,
  handleLike,
  handleFavourite,
  handleComment,
}) {
  const [selectedItem, setSelectedItem] = useState(null);

  if (!selectedTopic || knowledgeItems.length === 0) return null;

  return (
    <div className="mt-6">
      <h2 className="text-2xl font-bold mb-4 text-purple-700">{selectedTopic}</h2>

      <KnowledgeCardsDisplay
        items={knowledgeItems}
        title={selectedTopic}
        userId={userId}
        engagement={engagement}
        onPreview={setSelectedItem}
        onLike={handleLike}
        onFavourite={handleFavourite}
        onComment={handleComment}
      />

      {selectedItem && (
        <PreviewModal
          item={selectedItem}
          onClose={() => setSelectedItem(null)}
          onLike={handleLike}
          onFavourite={handleFavourite}
          onComment={handleComment}
          engagement={engagement}
        />
      )}
    </div>
  );
}
