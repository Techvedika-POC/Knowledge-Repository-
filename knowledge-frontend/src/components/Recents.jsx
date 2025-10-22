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
    <div className="min-h-screen bg-gray-100 p-6">
      {/* Page Header */}
      <div className="mb-6 text-center">
        <h1 className="text-3xl font-bold text-gray-800 mb-1">
          Recent Knowledge Articles
        </h1>
        <p className="text-gray-600">
          Check out the latest knowledge items added to the platform.
        </p>
      </div>

      {/* Container Card */}
      <div className="bg-white shadow-md rounded-xl p-6 max-w-7xl mx-auto">
        {/* Loading */}
        {loading && (
          <p className="text-center text-gray-500 py-10 animate-pulse">
            Loading recent items...
          </p>
        )}

        {/* Error */}
        {error && <p className="text-center text-red-500 py-10">{error}</p>}

        {/* Empty State */}
        {!loading && !error && recents.length === 0 && (
          <p className="text-center text-gray-400 py-10">
            No recent items available.
          </p>
        )}

        {/* Knowledge Cards */}
        {!loading && !error && recents.length > 0 && (
          <KnowledgeCardsDisplay
            items={recents}
            userId={userId}
            onPreview={handlePreview}
          />
        )}
      </div>

      {/* Preview Modal */}
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
