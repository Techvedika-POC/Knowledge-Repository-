import React, { useState } from "react";

export default function AiChatPanel({ item }) {
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState("");

  const askAi = async () => {
    if (!input.trim()) return;
    const fakeAnswer = `This is a simulated AI answer about: ${item.title}`;

    setMessages((prev) => [
      ...prev,
      { role: "user", text: input },
      { role: "ai", text: fakeAnswer },
    ]);
    setInput("");
  };

  return (
    <div className="border rounded-xl p-4 mt-6 bg-gray-50">
      <h3 className="font-semibold mb-2">🤖 Ask AI about this article</h3>

      <div className="space-y-2 max-h-64 overflow-y-auto mb-3">
        {messages.map((m, i) => (
          <div
            key={i}
            className={`p-2 rounded text-sm ${
              m.role === "ai"
                ? "bg-indigo-100"
                : "bg-white border"
            }`}
          >
            {m.text}
          </div>
        ))}
      </div>

      <div className="flex gap-2">
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          className="flex-1 border rounded px-2 py-1"
          placeholder="Ask something..."
        />
        <button
          onClick={askAi}
          className="bg-indigo-600 text-white px-4 rounded"
        >
          Ask
        </button>
      </div>
    </div>
  );
}
