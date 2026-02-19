import React, { useState, useEffect } from "react";
import api from "../api";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";
import toast from "react-hot-toast";

export default function FavouritesPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedItem, setSelectedItem] = useState(null);
  const handlePreview = (item) => setSelectedItem(item);
  const handleClosePreview = () => setSelectedItem(null);
  const userId = localStorage.getItem("userId");
  const fetchFavourites = async () => {
    setLoading(true);
    try {
      const res = await api.get("/Contributions/my/favourites");
      setItems(res.data);
    } catch (err) {
      console.error("Error fetching favourites", err);
      toast.error("Failed to fetch favourite items");
    }
    setLoading(false);
  };

  useEffect(() => {
    fetchFavourites();
  }, [userId]);

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="mb-6 text-center">
        <h1 className="text-3xl font-bold text-gray-800 mb-1">
          My Favourite Knowledge Items
        </h1>
        <p className="text-gray-600">
          Preview and manage the items you've saved as favourites.
        </p>
      </div>
      <div className="bg-white shadow-md rounded-xl p-6 max-w-7xl mx-auto">
        {loading && (
          <p className="text-center text-gray-500 py-10 animate-pulse">
            Loading favourites...
          </p>
        )}

        {!loading && items.length === 0 && (
          <p className="text-center text-gray-400 py-10">
            You have no favourite items yet.
          </p>
        )}

        {items.length > 0 && (
          <KnowledgeCardsDisplay
            items={items}
            title="Favourites"
            userId={userId}
            onPreview={handlePreview}
            onReset={fetchFavourites}
          />
        )}
      </div>
      {selectedItem && (
        <PreviewModal item={selectedItem} onClose={handleClosePreview} />
      )}
    </div>
  );
}
