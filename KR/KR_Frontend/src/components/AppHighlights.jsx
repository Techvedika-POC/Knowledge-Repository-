import React from "react";
import { Swiper, SwiperSlide } from "swiper/react";
import { Navigation, Autoplay, Pagination } from "swiper/modules";
import "swiper/css";
import "swiper/css/navigation";
import "swiper/css/pagination";
import { Sparkles, Users, Lightbulb, Rocket } from "lucide-react";

const features = [
  {
    title: "Knowledge Repository",
    desc: "Access, share, and manage valuable knowledge across your organization.",
    image: "/assets/knowledge_repo.png",
  },
  {
    title: "Fresh Picks",
    desc: "Explore handpicked trending knowledge cards curated by admins.",
    image: "/assets/fresh_picks.png",
  },
  {
    title: "Engagement Analytics",
    desc: "Track views, likes, favorites, and comments with insightful dashboards.",
    image: "/assets/analytics.png",
  },
  {
    title: "Admin Dashboard",
    desc: "Manage users, departments, roles, and performance analytics with ease.",
    image: "/assets/admin_dashboard.png",
  },
  {
    title: "AI Assistant",
    desc: "Get instant answers and recommendations from an AI-driven chatbot.",
    image: "/assets/AI_Assistant.png",
  },
  {
    title: "Event Management",
    desc: "Organize, track, and showcase technical events and coding challenges.",
    image: "/assets/events.png",
  },
  {
    title: "User Profiles",
    desc: "Personalized user dashboards showing contributions, badges, and activity.",
    image: "/assets/profile.png",
  },
  {
    title: "Search & Discovery",
    desc: "Quickly find topics, documents, and experts with intelligent search.",
    image: "/assets/search.png",
  },
];

export default function AppHighlights() {
  return (
    <div className="bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50 font-sans pt-0 mt-0">
      <div className="max-w-7xl mx-auto px-4 pt-5">

        {/* ---------------- FEATURES SECTION (Moved to Top) ---------------- */}
        <section className="mb-12">
          <h2 className="text-[22px] font-bold text-center text-black mb-2">
            KnowLedger Synaptix Features
          </h2>
          <p className="text-center text-gray-600 text-base max-w-2xl mx-auto mt-1 mb-4">
            Explore the full suite of smart modules built to enhance productivity,
            collaboration, and innovation within your teams.
          </p>

          <Swiper
            modules={[Navigation, Autoplay, Pagination]}
            navigation={{
              nextEl: ".swiper-button-next",
              prevEl: ".swiper-button-prev",
            }}
            pagination={{
              clickable: true,
              el: ".custom-swiper-pagination",
            }}
            autoplay={{ delay: 3000, disableOnInteraction: false }}
            loop
            spaceBetween={15} 
            slidesPerView={3}
            breakpoints={{
              320: { slidesPerView: 1 },
              768: { slidesPerView: 2 },
              1024: { slidesPerView: 3 },
            }}
            className="relative max-w-6xl mx-auto py-4"
          >
            {features.map((feature, i) => (
              <SwiperSlide key={i}>
                <div className="bg-white rounded-xl shadow-md overflow-hidden flex flex-col hover:shadow-xl hover:-translate-y-1 transition duration-300 h-full">
                  <img
                    src={feature.image}
                    alt={feature.title}
                    className="w-full h-44 object-cover"
                  />
                  <div className="p-4 flex flex-col flex-grow">
                    <h3 className="text-lg font-semibold text-blue-700 mb-1">
                      {feature.title}
                    </h3>
                    <p className="text-gray-600 text-sm leading-relaxed">
                      {feature.desc}
                    </p>
                  </div>
                </div>
              </SwiperSlide>
            ))}

            <div className="swiper-button-prev !w-6 !h-6 !text-blue-600 hover:!text-blue-800"></div>
            <div className="swiper-button-next !w-6 !h-6 !text-blue-600 hover:!text-blue-800"></div>
          </Swiper>
        </section>

        {/* ---------------- ABOUT SECTION ---------------- */}
        <section className="text-center mb-14">
          <h2 className="text-3xl font-bold text-indigo-800 mb-4">
            About KnowLedger Synaptix
          </h2>
          <p className="text-gray-700 text-lg leading-relaxed max-w-4xl mx-auto">
            We’re redefining how knowledge flows within organizations — combining
            <span className="text-indigo-600 font-semibold"> AI intelligence</span>,
            <span className="text-indigo-600 font-semibold"> data-driven insights</span>,
            and
            <span className="text-indigo-600 font-semibold"> seamless collaboration</span>
            into one unified experience. Whether you’re learning, managing projects, or sharing discoveries,
            KnowLedger Synaptix is your trusted digital workspace for innovation.
          </p>
        </section>

        {/* ---------------- MISSION & VISION ---------------- */}
        <div className="grid md:grid-cols-2 gap-8 mb-16 text-center">
          <div className="bg-white p-8 rounded-2xl shadow-md hover:shadow-xl transition duration-300 transform hover:-translate-y-1">
            <Rocket className="w-10 h-10 text-indigo-700 mx-auto mb-3" />
            <h3 className="text-xl font-semibold text-indigo-800 mb-2">
              Our Mission
            </h3>
            <p className="text-gray-600 text-sm leading-relaxed">
              To empower organizations with smarter tools that help capture,
              share, and preserve valuable insights — making knowledge truly
              accessible for everyone.
            </p>
          </div>

          <div className="bg-white p-8 rounded-2xl shadow-md hover:shadow-xl transition duration-300 transform hover:-translate-y-1">
            <Lightbulb className="w-10 h-10 text-purple-700 mx-auto mb-3" />
            <h3 className="text-xl font-semibold text-purple-800 mb-2">
              Our Vision
            </h3>
            <p className="text-gray-600 text-sm leading-relaxed">
              To create an intelligent ecosystem where collaboration drives
              creativity, and learning becomes a continuous, self-sustaining
              cycle of innovation.
            </p>
          </div>
        </div>

        {/* ---------------- CORE VALUES ---------------- */}
        <section className="mb-16">
          <h2 className="text-2xl font-bold text-center text-indigo-900 mb-8">
            Our Core Values
          </h2>
          <div className="grid md:grid-cols-3 gap-8 text-center">
            <div className="bg-white rounded-2xl p-6 shadow-md hover:shadow-lg transition duration-300">
              <Sparkles className="w-8 h-8 text-indigo-600 mx-auto mb-2" />
              <h4 className="text-lg font-semibold text-indigo-700">Innovation</h4>
              <p className="text-gray-600 text-sm mt-2">
                We continuously evolve, integrating AI and analytics to drive
                smarter, faster decisions.
              </p>
            </div>

            <div className="bg-white rounded-2xl p-6 shadow-md hover:shadow-lg transition duration-300">
              <Users className="w-8 h-8 text-purple-600 mx-auto mb-2" />
              <h4 className="text-lg font-semibold text-purple-700">Collaboration</h4>
              <p className="text-gray-600 text-sm mt-2">
                Teamwork fuels everything we build — empowering each voice to
                contribute and grow together.
              </p>
            </div>

            <div className="bg-white rounded-2xl p-6 shadow-md hover:shadow-lg transition duration-300">
              <Rocket className="w-8 h-8 text-blue-600 mx-auto mb-2" />
              <h4 className="text-lg font-semibold text-blue-700">Growth</h4>
              <p className="text-gray-600 text-sm mt-2">
                We inspire progress — both for individuals and organizations —
                through continuous learning and knowledge exchange.
              </p>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}
