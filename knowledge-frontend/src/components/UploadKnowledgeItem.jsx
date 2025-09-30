import React, { useState, useEffect } from "react";
import axios from "axios";
import { API_BASE_URL } from "../config";
import { FaLightbulb } from "react-icons/fa";

export default function UploadKnowledgeItem() {
  const [frameworks, setFrameworks] = useState([
    "C#",
    "Python",
    "Java",
    "React",
    ".NET",
    "Spring Boot",
  ]);
  const [files, setFiles] = useState([]);
  
  const [domains, setDomains] = useState([]);
  const [categories, setCategories] = useState([]);
  const [events, setEvents] = useState([]);

  const [form, setForm] = useState({
    name: "",
    domainId: "",
    categoryId: "",
    eventId: "",
    teamName: "",
    teamMemberEmails: "",
    isEventItem: false,
    languages: [],
    description: "",
  });

  const [activeTab, setActiveTab] = useState("File");

  useEffect(() => {
    axios
      .get(`${API_BASE_URL}/domains`)
      .then((res) => setDomains(res.data))
      .catch((err) => console.error(err));
  }, []);

  useEffect(() => {
    if (form.domainId) {
      axios
        .get(`${API_BASE_URL}/domains/${form.domainId}/categories`)
        .then((res) => setCategories(res.data))
        .catch((err) => console.error(err));
    }
  }, [form.domainId]);

  useEffect(() => {
    axios
      .get(`${API_BASE_URL}/events`)
      .then((res) => setEvents(res.data))
      .catch((err) => console.error(err));
  }, []);

  const handleFileChange = (e) => {
    setFiles([...files, ...Array.from(e.target.files)]);
  };

  const handleFileRemove = (name) => {
    setFiles(files.filter((f) => f.name !== name));
  };

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const handleTabChange = (tab) => {
    setActiveTab(tab);
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const formData = new FormData();
    formData.append("Title", form.name);
    formData.append("DomainId", form.domainId);
    formData.append("CategoryId", form.categoryId);
    formData.append("Language", form.language);
    formData.append("Description", form.description);
    frameworks.forEach((fw) => formData.append("Frameworks", fw));
    files.forEach((file) => formData.append("Files", file));

    if (form.isEventItem) {
      formData.append("IsEventItem", true);
      formData.append("EventId", form.eventId);
      formData.append("TeamName", form.teamName);
      if (form.teamMemberEmails.trim()) {
        form.teamMemberEmails
          .split(",")
          .map((email) => email.trim())
          .forEach((email) => formData.append("TeamMemberEmails", email));
      }
    }

    axios
      .post(`${API_BASE_URL}/knowledgeitem/upload`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      })
      .then(() => alert("Knowledge item uploaded successfully!"))
      .catch((err) => console.error(err));
  };

  return (
    <div className="max-w-[1000px] mx-auto mt-5 p-6 bg-white rounded-[12px] shadow-[0_6px_12px_rgba(0,0,0,0.05)] font-inter text-[#1f2937]">
      {/* Header */}
      <div className="flex flex-wrap justify-between items-center mb-4">
        <h2 className="text-[22px] font-semibold">Upload Knowledge Item</h2>
        <input
          type="text"
          placeholder="Search knowledge..."
          className="px-3 py-2 rounded-[8px] border border-[#d1d5db] text-sm max-w-[300px] flex-1 m-2"
        />
        <div className="flex items-center gap-2">
          <span className="font-medium">Alex Morgan</span>
          <img
            src="https://i.pravatar.cc/40"
            alt="user"
            className="w-10 h-10 rounded-full"
          />
        </div>
      </div>

      {/* Tip Bar */}
      <div className="flex items-center gap-2 bg-[#fef3c7] p-3 rounded-[8px] text-[#92400e] text-sm mb-5">
        <FaLightbulb />
        <span>
          <strong>Tip:</strong> Follow upload guidelines for faster approvals.
        </span>
      </div>

      <form onSubmit={handleSubmit}>
        {/* Knowledge Item Details */}
        <section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
          <h3 className="text-[16px] font-semibold mb-4 text-[#111827]">
            Knowledge Item Details
          </h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-5 mb-4">
            <input
              name="name"
              placeholder="Knowledge Item Name"
              value={form.name}
              onChange={handleInputChange}
              required
              className="w-[96%] px-2 py-2 border border-[#d1d5db] rounded-[8px] focus:outline-none focus:border-[#3b82f6] focus:ring-2 focus:ring-[#3b82f6]/30 text-[12px] text-[#111111]"
            />
            <select
              name="domainId"
              value={form.domainId}
              onChange={handleInputChange}
              required
              className="w-[96%] px-2 py-2 border border-[#d1d5db] rounded-[8px] focus:outline-none focus:border-[#3b82f6] focus:ring-2 focus:ring-[#3b82f6]/30 text-[12px] text-[#111111]"
            >
              <option value="">Select Domain</option>
              {domains.map((d) => (
                <option key={d.domainId} value={d.domainId}>
                  {d.domainName}
                </option>
              ))}
            </select>
            <select
              name="categoryId"
              value={form.categoryId}
              onChange={handleInputChange}
              required
              className="w-[96%] px-2 py-2 border border-[#d1d5db] rounded-[8px] focus:outline-none focus:border-[#3b82f6] focus:ring-2 focus:ring-[#3b82f6]/30 text-[12px] text-[#111111]"
            >
              <option value="">Select Category</option>
              {categories.map((c) => (
                <option key={c.categoryId} value={c.categoryId}>
                  {c.categoryName}
                </option>
              ))}
            </select>
          </div>

          {/* Event Checkbox */}
          <div className="my-2 font-medium">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                name="isEventItem"
                checked={form.isEventItem}
                onChange={handleInputChange}
              />
              Is this submission event-related?
            </label>
          </div>

          {form.isEventItem && (
            <div className="grid gap-2 mb-4">
              <select
                name="eventId"
                value={form.eventId}
                onChange={handleInputChange}
                required
                className="px-2 py-2 border border-[#ccc] rounded-[6px]"
              >
                <option value="">Select Event</option>
                {events.map((e) => (
                  <option key={e.eventId} value={e.eventId}>
                    {e.title}
                  </option>
                ))}
              </select>
              <input
                type="text"
                name="teamName"
                placeholder="Team Name"
                value={form.teamName}
                onChange={handleInputChange}
                required
                className="px-2 py-2 border border-[#ccc] rounded-[6px]"
              />
              <input
                type="text"
                name="teamMemberEmails"
                placeholder="Team Member Emails (comma-separated)"
                value={form.teamMemberEmails}
                onChange={handleInputChange}
                className="px-2 py-2 border border-[#ccc] rounded-[6px]"
              />
            </div>
          )}

          {/* Languages Section */}
          <section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
            <h3 className="text-[16px] font-semibold mb-4 text-[#111827]">
              Programming Languages
            </h3>

            {/* Display added languages as chips */}
            <div className="flex flex-wrap gap-2 mb-2">
              {form.languages.map((lang, index) => (
                <div
                  key={index}
                  className="flex items-center gap-1 px-2 py-1 rounded-[16px] bg-gradient-to-r from-[#a5b4fc] to-[#6366f1] text-white text-[13px] hover:shadow-md transition-shadow"
                >
                  <span>{lang}</span>
                  <button
                    type="button"
                    className="text-[12px] font-bold hover:text-red-200"
                    onClick={() =>
                      setForm((prev) => ({
                        ...prev,
                        languages: prev.languages.filter((l) => l !== lang),
                      }))
                    }
                  >
                    ×
                  </button>
                </div>
              ))}
            </div>

            {/* Pre-populated suggestions */}
          <div className="flex flex-wrap gap-2 mb-2">
            {["JavaScript", "Python", "Java", "C#", "Go", "Ruby"].map(
              (suggestion) => (
                <button
                  key={suggestion}
                  type="button"
                    className="px-2 py-1 rounded-[12px] bg-[#e0e7ff] text-[#3730a3] text-[12px] hover:bg-[#c7d2fe] transition"
                  onClick={() => {
                    if (!form.languages.includes(suggestion)) {
                      setForm((prev) => ({
                        ...prev,
                        languages: [...prev.languages, suggestion],
                      }));
                    }
                  }}
                >
                  {suggestion}
                </button>
              )
            )}
          </div>

            {/* Input to add new language */}
          <div className="flex gap-2 mt-2">
            <input
              type="text"
              placeholder="Add a language..."
              className="flex-1 px-2 py-2 border border-[#d1d5db] rounded-[8px]"
              id="languageInput"
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  const value = e.target.value.trim();
                  if (value && !form.languages.includes(value)) {
                    setForm((prev) => ({
                      ...prev,
                      languages: [...prev.languages, value],
                    }));
                    e.target.value = "";
                  }
                }
              }}
            />
            <button
              type="button"
                className="px-2 py-1 rounded-[18px] bg-[#06b6d4] text-white hover:bg-[#0891b2]"
              onClick={() => {
                const input = document.getElementById("languageInput");
                const value = input.value.trim();
                if (value && !form.languages.includes(value)) {
                  setForm((prev) => ({
                    ...prev,
                    languages: [...prev.languages, value],
                  }));
                  input.value = "";
                }
              }}
            >
              Add
            </button>
          </div>
        </section>
        </section>

        {/* Frameworks */}
        <section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
          <h3 className="text-[16px] font-semibold mb-4 text-[#111827]">
            Frameworks
          </h3>
          <div className="flex flex-wrap gap-2 mb-2">
            {frameworks.map((fw, index) => (
              <div
                key={index}
                className="flex items-center gap-1 px-2 py-1 rounded-[16px] bg-[#fef3c7] text-[13px] text-[#111111]"
              >
                <span>{fw}</span>
                <button
                  type="button"
                  className="text-[12px] font-bold text-red-600 hover:text-red-800"
                  onClick={() =>
                    setFrameworks(frameworks.filter((f) => f !== fw))
                  }
                >
                  ×
                </button>
              </div>
            ))}
          </div>
          <div className="flex gap-2 mt-2">
            <input
              type="text"
              id="frameworkInput"
              placeholder="Add another framework..."
              className="flex-1 px-2 py-2 border border-[#d1d5db] rounded-[8px]"
            />
            <button
              type="button"
              className="px-2 py-0.5 rounded-[18px] bg-[#06b6d4] text-white hover:bg-[#0891b2]"
              onClick={() => {
                const input = document.getElementById("frameworkInput");
                if (input.value.trim()) {
                  setFrameworks([...frameworks, input.value.trim()]);
                  input.value = "";
                }
              }}
            >
              Add
            </button>
          </div>
        </section>


        {/* Description */}
        <section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
          <h3 className="text-[16px] font-semibold mb-4 text-[#111827]">
            Description
          </h3>
          <textarea
            name="description"
            placeholder="Enter description..."
            rows={3}
            value={form.description}
            onChange={handleInputChange}
            required
            className="w-[96%] min-h-[100px] px-2 py-2 border border-[#d1d5db] rounded-[8px] resize-y text-[#111827] text-[14px]"
          />
        </section>

        {/* Upload Options */}
        <section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
          <h3 className="text-[16px] font-semibold mb-3 text-[#111827]">
            Upload Options
          </h3>
          <div className="flex gap-2 mb-3">
            {["File", "Text", "Video", "Audio"].map((tab) => (
              <button
                type="button"
                key={tab}
                className={`px-2 py-1 rounded-[15px] font-medium ${activeTab === tab
                  ? "bg-[#e4a931] text-[#0c0c0c]"
                  : "bg-[#fef3c7]"
                  }`}
                onClick={() => handleTabChange(tab)}
              >
                {tab}
              </button>
            ))}
          </div>

          {activeTab === "File" && (
            <>
              <div
                className="border-2 border-dashed border-[#d1d5db] rounded-[8px] p-5 text-center text-[14px] text-[#6b7280] mb-3 bg-[#f9fafb] cursor-pointer"
                onClick={() => document.getElementById("fileInput").click()}
                onDragOver={(e) => e.preventDefault()}
                onDrop={(e) => {
                  e.preventDefault();
                  handleFileChange({ target: { files: e.dataTransfer.files } });
                }}
              >
                Drag & drop files here (PDF, PPTX, DOCX, MP4 up to 500MB)
                <br />
                <span className="text-[12px]">or click to browse</span>
              </div>

              <input
                type="file"
                id="fileInput"
                multiple
                className="hidden"
                onChange={handleFileChange}
              />

              <div className="flex flex-col gap-2 mt-3">
                {files.map((file) => (
                  <div
                    key={file.name}
                    className="flex items-center gap-3 p-2 border rounded-[15px] bg-[#fcfcf9]"
                  >
                    <span
                      className={`px-2 py-1 rounded-[15px] text-[11px] font-bold text-black bg-[#fcc222]`}
                    >
                      {file.type || "FILE"}
                    </span>
                    <span className="text-[14px]">{file.name}</span>
                    <button
                      type="button"
                      className="ml-auto px-2 py-1 rounded-[10px] bg-[#eda30f] text-[#555] text-[12px] hover:bg-[#fff176]"
                      onClick={() => handleFileRemove(file.name)}
                    >
                      Remove
                    </button>
                  </div>
                ))}
              </div>
            </>
          )}
        </section>

        {/* Footer Buttons */}
        <div className="flex gap-2 justify-end mt-5">
          <button className="px-2 py-1 rounded-[20px] bg-[#f6e1a3] text-[#1f2937]">
            Preview
          </button>
          <button className="px-2 py-1 rounded-[20px] bg-[#3b82f6] text-[#101010]">
            Continue
          </button>
          <button
            type="submit"
            className="px-4 py-2 rounded-[20px] bg-[#f4b107] text-black"
          >
            Submit
          </button>
        </div>
      </form>
    </div>
  );
}
