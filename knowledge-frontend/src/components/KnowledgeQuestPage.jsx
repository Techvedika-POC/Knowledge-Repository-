import React from "react";
import { Swiper, SwiperSlide } from "swiper/react";
import { Pagination, Autoplay } from "swiper/modules";
import "swiper/css";
import "swiper/css/pagination";

const quests = [
  { title: "AI Knowledge Quest", img: "/assets/ux.png", points: "500 XP" },
  { title: "DevOps Mastery", img: "/assets/Capture.jpg", points: "350 XP" },
  { title: "Frontend Wizard", img: "/assets/Ai.png", points: "400 XP" },
];

export default function KnowledgeQuestPage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-green-50 to-white py-10 px-6">
      <h1 className="text-4xl font-bold text-green-700 mb-6 text-center"> Knowledge Quest</h1>
      <p className="text-center text-gray-600 mb-10 max-w-2xl mx-auto">
        Embark on a journey of learning. Complete quests, earn XP, and become a master in your field!
      </p>

      <Swiper
        modules={[Pagination, Autoplay]}
        spaceBetween={20}
        slidesPerView={2}
        autoplay={{ delay: 3000 }}
        pagination={{ clickable: true }}
        className="max-w-5xl mx-auto"
      >
        {quests.map((q, i) => (
          <SwiperSlide key={i}>
            <div className="rounded-2xl overflow-hidden shadow-lg bg-white border border-green-100">
              <img src={q.img} alt={q.title} className="w-full h-56 object-cover" />
              <div className="p-6">
                <h3 className="text-xl font-semibold text-green-700">{q.title}</h3>
                <p className="text-gray-500 mt-2">Reward: {q.points}</p>
              </div>
            </div>
          </SwiperSlide>
        ))}
      </Swiper>
    </div>
  );
}
