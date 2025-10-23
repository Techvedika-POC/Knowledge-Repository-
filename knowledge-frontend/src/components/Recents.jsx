// src/components/Recents.jsx
import React, { useState, useEffect } from "react";
import api from "../api";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";

export default function Recents() {
  const [recents, setRecents] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [selectedItem, setSelectedItem] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const userId = localStorage.getItem("userId");

  useEffect(() => {
    const fetchRecents = async () => {
      try {
        setLoading(true);
        setError("");
        const res = await api.get("/FreshPicks?count=12"); // Fetch recent items
        setRecents(res.data);
      } catch (err) {
        console.error("Error fetching recents:", err);
        setError("Failed to load recents.");
      } finally {
        setLoading(false);
      }
    };
    fetchRecents();
  }, []);

  const handlePreview = (item) => {
    setSelectedItem(item);
    setIsModalOpen(true);
  };

  return (
    <div className="px-6 py-4">
      <h2 className="text-xl font-semibold text-black-800 mb-4">
        Recent Knowledge Articles
      </h2>

      {loading && <p>Loading recents...</p>}
      {error && <p className="text-red-500">{error}</p>}
      {!loading && !error && recents.length === 0 && (
        <p className="text-gray-600">No recent items available.</p>
      )}

      {!loading && !error && recents.length > 0 && (
        <KnowledgeCardsDisplay
          items={recents}
          userId={userId}
          onPreview={handlePreview}
        />
      )}

      {selectedItem && (
        <PreviewModal
          item={selectedItem}
          isOpen={isModalOpen}
          onClose={() => setIsModalOpen(false)}
        />
      )}
    </div>
  );
}
