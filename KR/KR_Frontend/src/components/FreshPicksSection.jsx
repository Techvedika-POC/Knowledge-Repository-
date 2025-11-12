import React, { useState } from "react";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";

export default function FreshPicksSection({ freshPicks, userId }) {
  const [selectedItem, setSelectedItem] = useState(null);

  return (
    <>
      <KnowledgeCardsDisplay
        items={freshPicks}
        title="Fresh Picks"
        userId={userId}
        onPreview={(item) => setSelectedItem(item)}
      />

      {selectedItem && (
        <PreviewModal
          item={selectedItem}
          onClose={() => setSelectedItem(null)}
        />
      )}
    </>
  );
}