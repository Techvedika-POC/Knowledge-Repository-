 
import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { ChevronLeft, ChevronRight } from "lucide-react";
 
const features = [
  {
    title: "Knowledge Capture",
    desc: "Upload files, paste links, or type notes. AI automatically structures content into knowledge items.",
    image: "/assets/capture.jpg",
  },
  {
    title: "Smart Discovery",
    desc: "AI-powered semantic search finds relevant content across your organization’s knowledge repository.",
    image: "/assets/smart_discovery.jpg",
  },
  {
    title: "Collaboration & Reuse",
    desc: "Comment, rate, link ideas to projects, and reuse organizational knowledge seamlessly.",
    image: "/assets/collaboration.jpg",
  },
  {
    title: "AI Assistant",
    desc: "Ask questions naturally and get instant structured answers from multiple sources.",
    image: "/assets/ai_assistant.png",
  },
];
 
export default function LandingPage() {
  const navigate = useNavigate();
  const [index, setIndex] = useState(0);
 
  useEffect(() => {
    const interval = setInterval(() => {
      setIndex((prev) => (prev === features.length - 1 ? 0 : prev + 1));
    }, 6000);
    return () => clearInterval(interval);
  }, []);
 
  const prevSlide = () =>
    setIndex((i) => (i === 0 ? features.length - 1 : i - 1));
  const nextSlide = () =>
    setIndex((i) => (i === features.length - 1 ? 0 : i + 1));
 
  return (
    <div className="min-h-screen flex flex-col bg-gradient-to-b from-gray-50 to-white">
      {/* Header */}
      <header className="flex justify-between items-center px-8 py-4 bg-white shadow sticky top-0 z-50">
        <h1 className="text-xl font-bold text-gray-800">KnowLedger Synaptix</h1>
        <div className="space-x-4">
          <button
            onClick={() => navigate("/login")}
            className="px-4 py-2 text-sm font-medium text-gray-700 hover:text-gray-900 transition"
          >
            Login
          </button>
          <button
            onClick={() => navigate("/signup")}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg shadow hover:bg-blue-700 transition"
          >
            Sign Up
          </button>
        </div>
      </header>
 
      {/* Hero + Carousel */}
      <section className="flex flex-col md:flex-row h-screen w-full">
        {/* Left: Hero Message */}
        <div className="flex-1 md:flex-[1] flex flex-col justify-center px-6 md:px-12 text-center md:text-left">
          <h1 className="text-3xl md:text-4xl font-extrabold text-gray-900 mb-4">
            Welcome to <span className="text-blue-600">KnowLedger Synaptix</span>
          </h1>
          <p className="text-lg md:text-xl text-gray-700 mb-6">Innovate. Collaborate. Build.</p>
          <p className="text-gray-600 text-base md:text-lg max-w-md mb-8">
            Capture knowledge, discover insights, collaborate seamlessly, and leverage AI-powered assistance to build smarter solutions.
          </p>
          <div className="flex justify-center md:justify-start gap-4">
            <button
              onClick={() => navigate("/signup")}
              className="px-6 py-3 bg-blue-600 text-white rounded-lg shadow hover:bg-blue-700 transition font-semibold"
            >
              Get Started
            </button>
            <button
              onClick={() => navigate("/features")}
              className="px-6 py-3 border border-blue-600 text-blue-600 rounded-lg hover:bg-blue-50 transition font-semibold"
            >
              Explore Features
            </button>
          </div>
        </div>
 
        {/* Right: Carousel */}
        <div className="flex-1 md:flex-[2] relative flex items-center justify-center overflow-hidden">
          {features.map((feature, i) => (
            <div
              key={i}
              className={`absolute inset-0 flex items-center justify-center transition-all duration-1000 ease-in-out transform ${
                i === index
                  ? "opacity-100 z-10 translate-x-0"
                  : "opacity-0 z-0 -translate-x-20"
              }`}
            >
              {/* Image */}
              <div className="w-full h-full md:h-[28rem] rounded-xl overflow-hidden shadow-2xl relative">
                <img
                  src={feature.image}
                  alt={feature.title}
                  className="w-full h-full object-cover"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent rounded-xl"></div>
                <div className="absolute bottom-12 left-1/2 transform -translate-x-1/2 text-center px-4 md:px-0">
                  <h2
                    className={`text-2xl md:text-4xl font-extrabold text-white drop-shadow-xl transition-all duration-700 ${
                      i === index
                        ? "opacity-100 translate-y-0 scale-100"
                        : "opacity-0 translate-y-6 scale-95"
                    }`}
                  >
                    {feature.title}
                  </h2>
                  <p
                    className={`mt-3 text-base md:text-lg text-white drop-shadow-md transition-all duration-700 delay-150 ${
                      i === index
                        ? "opacity-100 translate-y-0 scale-100"
                        : "opacity-0 translate-y-6 scale-95"
                    }`}
                  >
                    {feature.desc}
                  </p>
                </div>
              </div>
            </div>
          ))}
 
          {/* Carousel Controls */}
          <button
            onClick={prevSlide}
            className="absolute left-4 top-1/2 transform -translate-y-1/2 bg-white rounded-full p-2 shadow hover:bg-gray-50 transition"
          >
            <ChevronLeft className="w-6 h-6 text-gray-600" />
          </button>
          <button
            onClick={nextSlide}
            className="absolute right-4 top-1/2 transform -translate-y-1/2 bg-white rounded-full p-2 shadow hover:bg-gray-50 transition"
          >
            <ChevronRight className="w-6 h-6 text-gray-600" />
          </button>
        </div>
      </section>
 
      {/* Footer */}
      <footer className="mt-auto py-6 text-center text-gray-500 text-sm">
        © {new Date().getFullYear()} KnowLedger. All rights reserved.
      </footer>
    </div>
  );
}
 
 