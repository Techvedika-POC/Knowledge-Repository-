import React, { useState, useEffect } from "react";
import { useLocation } from "react-router-dom";
import axios from "axios";
import { FaLightbulb } from "react-icons/fa";
import api from "../api";


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
  const [tags, setTags] = useState("");
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
    tags: [],
    description: "",
  });
  const [activeTab, setActiveTab] = useState("File");
  const location = useLocation();
  const { eventId, eventTitle, eventType } = location.state || {};

  useEffect(() => {
    // If coming from an event (via state)
    if (location.state?.eventId) {
      setForm((prev) => ({
        ...prev,
        isEventItem: true,
        eventId: location.state.eventId,
      }));
    }
  }, [location.state]);
  useEffect(() => {
    if (eventId) {
      console.log("Prefilled Event:", { eventId, eventTitle, eventType });
      // set form data here if needed
    }
  }, [eventId]);

  useEffect(() => {
    api.
      get(`/domains`)
      .then((res) => setDomains(res.data))
      .catch((err) => console.error(err));
  }, []);

  useEffect(() => {
    if (form.domainId) {
      api
        .get(`/domains/${form.domainId}/categories`)
        .then((res) => setCategories(res.data))
        .catch((err) => console.error(err));
    }
  }, [form.domainId]);

  useEffect(() => {
    api
      .get(`/events`)
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

  const handleTabChange = (tab) => setActiveTab(tab);

const handleSubmit = async (e) => {
  e.preventDefault();
  const token = localStorage.getItem("jwtToken");
  if (!token) {
    alert("You must be logged in to upload a knowledge item.");
    return;
  }

  try {
    const formData = new FormData();

    // Basic Info
    formData.append("Title", form.name);
    formData.append("DomainId", form.domainId);
    formData.append("CategoryId", form.categoryId);
    formData.append("Description", form.description);

    // Languages: handle string or array safely
      // Languages:
    const languageList = Array.isArray(form.languages)
      ? form.languages
      : (form.languages || "").split(",").map((l) => l.trim()).filter(Boolean);

    languageList.forEach((lang) => formData.append("Language", lang));

    // Frameworks: handle string or array safely
      // Frameworks
    const frameworkList = Array.isArray(frameworks)
      ? frameworks
      : (frameworks || "").split(",").map((f) => f.trim()).filter(Boolean);

    frameworkList.forEach((fw) => formData.append("Framework", fw));

      const tagInput = document.getElementById("tagInput").value.trim();
      const allTags = [...form.tags];
      if (tagInput && !allTags.includes(tagInput)) allTags.push(tagInput);

      allTags.forEach((tag) => formData.append("Tags", tag));

    (files || []).forEach((file) => formData.append("Files", file));


    // Event-specific fields
    if (form.isEventItem) {
      formData.append("IsEventItem", true);
      formData.append("EventId", form.eventId);
      formData.append("TeamName", form.teamName);

      let emails = (form.teamMemberEmails || "")
        .split(",")
        .map((e) => e.trim())
        .filter(Boolean);

      const userEmail = localStorage.getItem("userEmail");
      if (userEmail && !emails.includes(userEmail)) emails.push(userEmail);

      emails.forEach((email) => formData.append("TeamMemberEmails", email));
    }
      console.log("FormData entries:");
      for (let pair of formData.entries()) {
        console.log(pair[0], ":", pair[1]);
      }
      // Submit the form
      const response = await api.post(
        `/knowledgeitem/upload`,
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
            Authorization: `Bearer ${token}`,
          },
        }
      );

    alert("Knowledge item uploaded successfully!");
    console.log("Upload response:", response.data);
  } catch (err) {
    if (err.response) {
      console.error("Error response:", err.response.data);
      alert(`Upload failed: ${JSON.stringify(err.response.data)}`);
    } else {
      console.error("Error:", err.message);
      alert(`Upload failed: ${err.message}`);
    }
  }
};
  return (
    <div className="max-w-[1000px] mx-auto mt-5 p-6 bg-white rounded-[12px] shadow-[0_6px_12px_rgba(0,0,0,0.05)] font-inter text-[#1f2937]">
      <div className="flex flex-wrap justify-between items-center mb-4 relative">
        <h2 className="text-[22px] font-semibold">Upload Knowledge Articles</h2>
      </div>

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
              className="w-[96%] px-2 py-2 border border-[#d1d5db] rounded-[8px] text-[12px]"
            />
            <select
              name="domainId"
              value={form.domainId}
              onChange={handleInputChange}
              required
              className="w-[96%] px-2 py-2 border border-[#d1d5db] rounded-[8px] text-[12px]"
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
              className="w-[96%] px-2 py-2 border border-[#d1d5db] rounded-[8px] text-[12px]"
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
        </section>

      
{/* Languages Section */}

<section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
  <h3 className="text-[16px] font-semibold mb-2 text-[#111827]">
    Languages (comma-separated)
  </h3>
  <input
    type="text"
    name="languages"
    placeholder="e.g. C#, Python, Java"
    value={form.languages}
    onChange={(e) =>
      setForm((prev) => ({
        ...prev,
        languages: e.target.value,
      }))
    }
    className="w-[96%] px-2 py-2 border border-[#d1d5db] rounded-[8px]"
  />
</section>

{/* Frameworks Section */}
<section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
  <h3 className="text-[16px] font-semibold mb-2 text-[#111827]">
    Frameworks (comma-separated)
  </h3>
  <input
    type="text"
    name="frameworks"
    placeholder="e.g. React, .NET, Spring Boot"
    value={frameworks}
    onChange={(e) => setFrameworks(e.target.value)}
    className="w-[96%] px-2 py-2 border border-[#d1d5db] rounded-[8px]"
  />
</section>

        {/* Tags Section */}
        <section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
          <h3 className="text-[16px] font-semibold mb-2 text-[#111827]">
            Tags
          </h3>
          <div className="flex flex-wrap gap-2 mb-2">
            {form.tags.map((tag, index) => (
              <div
                key={index}
                className="flex items-center gap-1 px-2 py-1 rounded-[16px] bg-[#d1fae5] text-[#065f46] text-[13px]"
              >
                <span>{tag}</span>
                <button
                  type="button"
                  onClick={() =>
                    setForm((prev) => ({
                      ...prev,
                      tags: prev.tags.filter((t) => t !== tag),
                    }))
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
              id="tagInput"
              placeholder="Add a tag..."
              className="flex-1 px-2 py-2 border border-[#d1d5db] rounded-[8px]"
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  const value = e.target.value.trim();
                  if (value && !form.tags.includes(value)) {
                    setForm((prev) => ({ ...prev, tags: [...prev.tags, value] }));
                    e.target.value = "";
                  }
                }
              }}
            />
            <button
              type="button"
              className="px-2 py-1 rounded-[18px] bg-[#06b6d4] text-white"
              onClick={() => {
                const input = document.getElementById("tagInput");
                const value = input.value.trim();
                if (value && !form.tags.includes(value)) {
                  setForm((prev) => ({ ...prev, tags: [...prev.tags, value] }));
                  input.value = "";
                }
              }}
            >
              Add
            </button>
          </div>
        </section>

        {/* Description Section */}
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
            className="w-[96%] min-h-[100px] px-2 py-2 border border-[#d1d5db] rounded-[8px]"
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
                className={`px-2 py-1 rounded-[15px] ${activeTab === tab ? "bg-[#e4a931] text-[#0c0c0c]" : "bg-[#fef3c7]"
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
                    <span className="px-2 py-1 rounded-[15px] text-[11px] font-bold text-black bg-[#fcc222]">
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
          <button
            type="button"
            className="px-2 py-1 rounded-[20px] bg-[#f6e1a3] text-[#1f2937]"
          >
            Preview
          </button>
          <button
            type="submit"
            className="px-2 py-1 rounded-[20px] bg-[#eab308] text-white"
          >
            Upload
          </button>
        </div>
      </form>
    </div>
  );
}