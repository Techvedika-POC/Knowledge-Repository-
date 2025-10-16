import React, { useState, useEffect } from "react";
import { Swiper, SwiperSlide } from "swiper/react";
import { Navigation, Pagination, Autoplay } from "swiper/modules";
import "swiper/css";
import "swiper/css/navigation";
import "swiper/css/pagination";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";
import axios from "axios";
import api from "../api";

const topicImages = {
  ArtificialIntelligence: "/assets/Ai.png",
  DevOps: "/assets/Devops.png",
  GovernancePolicies:
    "https://images.unsplash.com/photo-1573164574572-cb89e39749b4?auto=format&fit=crop&w=800&q=60",
  CloudComputing:
    "https://images.unsplash.com/photo-1498050108023-c5249f4df085?auto=format&fit=crop&w=800&q=60",
  Backend: "/assets/backend.png",
  Frontend: "/assets/frontend.png",
};

export default function TopicsSection({ topics, userId }) {
  const [selectedTopic, setSelectedTopic] = useState(null);
  const [domainKnowledgeItems, setDomainKnowledgeItems] = useState([]);
  const [selectedItem, setSelectedItem] = useState(null);
  const [engagement, setEngagement] = useState({
    likedItems: [],
    favouritedItems: [],
  });
  useEffect(() => {
    const fetchUserEngagements = async () => {
      if (!userId) return;
      try {
        const res = await api.get(`/engagement/user-engagements/${userId}`);
        const likedItems = res.data
          .filter((e) => e.engagementType === "Like")
          .map((e) => e.itemId);
        const favouritedItems = res.data
          .filter((e) => e.engagementType === "Favourite")
          .map((e) => e.itemId);
        setEngagement({ likedItems, favouritedItems });
      } catch (err) {
        console.error("Error fetching user engagements:", err);
      }
    };
    fetchUserEngagements();
  }, [userId]);
  const handleTopicClick = async (topicName) => {
    setSelectedTopic(topicName);
    try {
      const res = await api.get(
        `/TopicHighlight/knowledge?domain=${encodeURIComponent(topicName)}&top=10`
      );
      setDomainKnowledgeItems(res.data || []);
    } catch (err) {
      console.error("Failed to fetch domain knowledge items:", err);
      setDomainKnowledgeItems([]);
    }
  };
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
      const method = alreadyLiked ? "delete" : "post";
      await axios[method](`/api/engagement/like/${itemId}?userId=${userId}`);
    } catch (error) {
      console.error("Error updating like:", error);
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
      const method = alreadyFavourited ? "delete" : "post";
      await axios[method](`/api/engagement/favourite/${itemId}?userId=${userId}`);
    } catch (error) {
      console.error("Error updating favourite:", error);
    }
  };

  const handleCommentClick = async (itemId, commentText) => {
    if (!userId) return;
    try {
      await axios.post(`/api/engagement/comment/${itemId}?userId=${userId}`, { commentText });
    } catch (error) {
      console.error("Error posting comment:", error);
    }
  };
  if (!topics || topics.length === 0)
    return <div className="p-4 text-gray-500">No topics available</div>;
  const handleReset = () => {
  setSelectedTopic(null); // clears the selected topic
  setDomainKnowledgeItems([]); // clears the displayed cards
};

 

  return (
    <div className="p-4">
      <Swiper
        modules={[Navigation, Pagination, Autoplay]}
        spaceBetween={20}
        slidesPerView={3}
        navigation
        pagination={{ clickable: true }}
        autoplay={{ delay: 3000 }}
        className="mySwiper"
      >
        {topics.map((topic, idx) => (
          <SwiperSlide key={idx}>
            <div
              onClick={() => handleTopicClick(topic.topicName)}
              className="relative rounded-xl overflow-hidden shadow-lg cursor-pointer transform hover:scale-105 transition duration-300"
            >
              <div
                className="absolute inset-0 bg-cover bg-center"
                style={{
                  backgroundImage: `url(${
                    topicImages[topic.topicName] ||
                    "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=800&q=60"
                  })`,
                }}
              />
              <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent"></div>
              <div className="relative p-4 flex flex-col justify-end h-40">
                <h3 className="text-lg font-bold text-white">{topic.topicName}</h3>
                <p className="text-sm text-white">{topic.recentItemCount} items</p>
              </div>
            </div>
          </SwiperSlide>
        ))}
      </Swiper>

      {/* Knowledge Cards (after clicking a topic) */}
      {selectedTopic && domainKnowledgeItems.length > 0 && (
        <div className="mt-8">
          <h3 className="text-xl font-semibold text-purple-600 mb-4">
            {selectedTopic} Knowledge Items
          </h3>
          <KnowledgeCardsDisplay
            items={domainKnowledgeItems}
            title={selectedTopic}
            userId={userId}
            onPreview={(item) => setSelectedItem(item)}
            onLike={handleLikeClick}
            onFavourite={handleFavouriteClick}
            onComment={handleCommentClick}
            engagement={engagement}
              onReset={handleReset}
          />
        </div>
      )}

      {/* Preview Modal */}
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
    </div>
  );
}
