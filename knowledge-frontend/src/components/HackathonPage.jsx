import React from "react";
import { Link } from "react-router-dom";

export default function HackathonPage() {
  return (
    <div className="bg-gray-50">
      {/* Hero Section */}
    <section
  className="relative bg-cover bg-center h-screen"
  style={{
    backgroundImage: "url('https://images.unsplash.com/photo-1518770660439-4636190af475')",
  }}
>

       <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className="relative z-10 flex items-center justify-center h-full text-center text-white">
          <div>
            <h1 className="text-5xl font-bold mb-4">Hackathon 2025</h1>
            <p className="text-xl mb-8">Innovate, Collaborate, Create</p>
            <Link to="/app/register" className="bg-blue-600 hover:bg-blue-700 text-white py-2 px-6 rounded-full text-lg">Register Now</Link>
          </div>
        </div>
      </section>

      {/* About Section */}
      <section className="py-16 px-4 text-center">
        <h2 className="text-3xl font-semibold mb-4">About the Hackathon</h2>
        <p className="text-lg max-w-3xl mx-auto">
          Join us for an exciting 48-hour hackathon where developers, designers, and innovators come together to solve real-world problems.
        </p>
      </section>

      {/* Timeline Section */}
      <section className="bg-gray-100 py-16 px-4">
        <h2 className="text-3xl font-semibold text-center mb-8">Event Timeline</h2>
        <div className="grid md:grid-cols-3 gap-8">
          <div className="text-center">
            <h3 className="text-xl font-semibold mb-2">Day 1</h3>
            <p>Kickoff & Team Formation</p>
          </div>
          <div className="text-center">
            <h3 className="text-xl font-semibold mb-2">Day 2</h3>
            <p>Development & Mentorship</p>
          </div>
          <div className="text-center">
            <h3 className="text-xl font-semibold mb-2">Day 3</h3>
            <p>Final Presentations & Judging</p>
          </div>
        </div>
      </section>
      {/* Footer */}
      <footer className="bg-gray-800 text-white py-8 text-center">
        <p>&copy; 2025 Hackathon. All rights reserved.</p>
        <div className="mt-4">
          <a href="#" className="text-blue-400 mx-2">Facebook</a>
          <a href="#" className="text-blue-400 mx-2">Twitter</a>
          <a href="#" className="text-blue-400 mx-2">LinkedIn</a>
        </div>
      </footer>
    </div>
  );
}
