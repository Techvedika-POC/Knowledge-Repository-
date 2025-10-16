import React, { useState, useEffect } from "react";
import { Swiper, SwiperSlide } from "swiper/react";
import { Navigation, Pagination, Autoplay } from "swiper/modules";
import "swiper/css";
import "swiper/css/navigation";
import "swiper/css/pagination";
import KnowledgeCardsDisplay from "./KnowledgeCardsDisplay";
import PreviewModal from "./PreviewModal";
import axios from "axios";

const topicImages = {
  "Artificial Intelligence": "/assets/Ai.png",
  DevOps: "/assets/Devops.png",
  "Governance/Policies":
    "https://images.unsplash.com/photo-1573164574572-cb89e39749b4?auto=format&fit=crop&w=800&q=60",
  "Cloud Computing":
    "https://images.unsplash.com/photo-1498050108023-c5249f4df085?auto=format&fit=crop&w=800&q=60",
  Backend: "/assets/backend.png",
  Frontend: "/assets/frontend.png",
};

export default function TopicsSection({ topics, userId }) {
  const [selectedTopic, setSelectedTopic] = useState(null);
  const [domainKnowledgeItems, setDomainKnowledgeItems] = useState([]);
  const [selectedItem, setSelectedItem] = useState(null);
  const [mappedItems, setMappedItems] = useState([]);

  const handleTopicClick = async (topicName) => {
    setSelectedTopic(topicName);
    try {
      const res = await axios.get(
        `/api/TopicHighlight/knowledge?domain=${encodeURIComponent(topicName)}&top=10`
      );
      const items = res.data || [];
      // Normalize contributor/owner name
      const normalized = items.map(item => ({
        ...item,
        ownerName: item.ownerName || item.contributorName || "Unknown Contributor",
      }));
      setMappedItems(normalized);
      setDomainKnowledgeItems(normalized);
    } catch (err) {
      console.error("Failed to fetch domain knowledge items:", err);
      setDomainKnowledgeItems([]);
    }
  };

  if (!topics || topics.length === 0)
    return <div className="p-4 text-gray-500">No topics available</div>;

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

      {selectedTopic && mappedItems.length > 0 && (
        <div className="mt-8">
          <h3 className="text-xl font-semibold text-purple-600 mb-4">
            {selectedTopic} Knowledge Items
          </h3>
          <KnowledgeCardsDisplay
            items={mappedItems}
            title={selectedTopic}
            userId={userId}
            onPreview={(item) => setSelectedItem(item)}
          />
        </div>
      )}

      {selectedItem && (
        <PreviewModal
          item={selectedItem}
          onClose={() => setSelectedItem(null)}
          userId={userId}
        />
      )}
    </div>
  );
}
