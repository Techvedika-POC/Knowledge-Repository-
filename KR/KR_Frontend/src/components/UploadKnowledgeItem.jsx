
import React, { useState, useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { FaLightbulb } from "react-icons/fa";
import api from "../api";
import toast from "react-hot-toast";

export default function UploadKnowledgeItem() {
  const location = useLocation();
  const navigate = useNavigate();
  const editItemId = location.state?.itemId ?? null;

  const [frameworks, setFrameworks] = useState([
    "C#",
    "Python",
    "Java",
    "React",
    ".NET",
    "Spring Boot",
  ]);
  const [files, setFiles] = useState([]); 
  const [existingAttachments, setExistingAttachments] = useState([]); 
  const [keptExistingAttachmentIds, setKeptExistingAttachmentIds] = useState([]); 
  const [replaceAttachments, setReplaceAttachments] = useState(false);

  const [tags, setTags] = useState("");
  const [domains, setDomains] = useState([]);
  const [categories, setCategories] = useState([]);
  const [events, setEvents] = useState([]);
  const [activeTab, setActiveTab] = useState("File");

  
  const [form, setForm] = useState({
    name: "",
    domainId: "",
    categoryId: "",
    description: "",
    languages: [], 
    tags: [],
    isEventItem: false,
    eventId: "",
    teamId: "", // prefilled
    teamMemberEmails: "", // prefilled
  });
const isEventFlow = !!(location.state?.eventId || form.isEventItem);
  // ------------------- Prefill event & team info -------------------
  useEffect(() => {
    if (location.state?.eventId) {
      const userTeam = JSON.parse(localStorage.getItem("userTeam") || "{}");
      setForm((prev) => ({
        ...prev,
        isEventItem: true,
        eventId: location.state.eventId,
        teamId: userTeam?.teamId || "",
        teamMemberEmails: userTeam?.members?.map((m) => m.email).join(",") || "",
      }));
    }
  }, [location.state]);

  // ------------------- Fetch domains -------------------
  useEffect(() => {
    api.get("/domains").then((res) => setDomains(res.data)).catch((err) => console.error(err));
  }, []);

  // ------------------- Fetch categories on domain change -------------------
  useEffect(() => {
    if (form.domainId) {
      api.get(`/domains/${form.domainId}/categories`).then((res) => setCategories(res.data)).catch((err) => console.error(err));
    } else {
      setCategories([]);
    }
  }, [form.domainId]);

  // ------------------- Fetch events  -------------------
  useEffect(() => {
    api.get("/events").then((res) => setEvents(res.data)).catch((err) => console.error(err));
  }, []);

  // ------------------- Prefill when editing -------------------
  useEffect(() => {
    if (!editItemId) return;
    const token = localStorage.getItem("jwtToken");
    if (!token) return;
    api.get(`/KnowledgeItem/${editItemId}/details`, { headers: { Authorization: `Bearer ${token}` } })
      .then((res) => {
        const d = res.data;
        setForm((prev) => ({
          ...prev,
          name: d.title || "",
          domainId: d.domainId ?? "",
          categoryId: d.categoryId ?? "",
          description: d.description || "",
          languages: d.language || "",
          tags: d.tags || [],
          isEventItem: d.isEventItem || false,
          eventId: d.eventId ?? "",
          teamId: prev.teamId || "",
          teamMemberEmails: prev.teamMemberEmails || "",
        }));
        setExistingAttachments(d.attachments || []);
        setKeptExistingAttachmentIds((d.attachments || []).map((a) => a.attachmentId));
      })
      .catch((err) => {
        console.error("Failed to load item details:", err);
        toast.error("Failed to load item for edit.");
      });
  }, [editItemId]);

  // ------------------- File handlers -------------------
  const handleFileChange = (e) => {
    const chosen = Array.from(e.target.files || []);
    setFiles((prev) => [...prev, ...chosen]);
  };

  const handleFileRemove = (name) => {
    setFiles((prev) => prev.filter((f) => f.name !== name));
  };

  const handleRemoveExistingAttachment = (attachmentId) => {
    setKeptExistingAttachmentIds((prev) => prev.filter((id) => id !== attachmentId));
  };

  const handleKeepExistingAttachment = (attachmentId) => {
    setKeptExistingAttachmentIds((prev) => (prev.includes(attachmentId) ? prev : [...prev, attachmentId]));
  };

  // ------------------- Input handler -------------------
  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const addTag = () => {
    const input = document.getElementById("tagInput");
    const value = input?.value?.trim();
    if (value && !form.tags.includes(value)) {
      setForm((prev) => ({ ...prev, tags: [...(prev.tags || []), value] }));
      if (input) input.value = "";
    }
  };


  const handleTabChange = (tab) => setActiveTab(tab);


  const handleSubmit = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem("jwtToken");
    if (!token) {
      toast.error("You must be logged in to upload a knowledge item.");
      return;
    }

    try {
      const formData = new FormData();

      formData.append("Title", form.name);
      formData.append("DomainId", form.domainId ?? "");
      formData.append("CategoryId", form.categoryId ?? "");
      formData.append("Description", form.description ?? "");

   
      const languageList = Array.isArray(form.languages) ? form.languages : ((form.languages || "") + "").split(",").map((l) => l.trim()).filter(Boolean);
      languageList.forEach((lang) => formData.append("Language", lang));

      const frameworkList = Array.isArray(frameworks) ? frameworks : ((frameworks || "") + "").split(",").map((f) => f.trim()).filter(Boolean);
      frameworkList.forEach((fw) => formData.append("Framework", fw));


      (form.tags || []).forEach((tag) => formData.append("Tags", tag));

     
      (files || []).forEach((file) => formData.append("Files", file));


      if (form.isEventItem && !editItemId) {
        formData.append("IsEventItem", "true");
        formData.append("EventId", form.eventId || "");
        formData.append("TeamId", form.teamId || "");
        formData.append("TeamMemberEmails", form.teamMemberEmails || "");
      }


      if (editItemId) {
   
        (keptExistingAttachmentIds || []).forEach((id) => formData.append("ExistingAttachmentIds", id));
        formData.append("ReplaceAttachments", replaceAttachments ? "true" : "false");

 
        formData.append("itemId", editItemId);
      }

      const config = {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      };

      if (editItemId) {
     
        await api.put(`/KnowledgeItem?itemId=${encodeURIComponent(editItemId)}`, formData, config);
        toast.success("Knowledge item updated successfully!");
        navigate("/app/my-contributions"); 
      } else {
     
        await api.post(`/knowledgeitem/upload`, formData, config);
        toast.success("Knowledge item uploaded successfully!");
        navigate("/app/my-contributions");
      }
    } catch (err) {
      console.error("Submit error:", err);
      const msg = err.response?.data || err.message || "Submit failed";
      toast.error(`Submit failed: ${JSON.stringify(msg)}`);
    }
  };


  return (
    <div className="max-w-[1000px] mx-auto mt-5 p-6 bg-white rounded-[12px] shadow-md font-inter text-[#1f2937]">
      <div className="flex justify-between items-center mb-4">
     
        <h2 className="text-[22px] font-semibold">
  {editItemId
    ? "Edit Knowledge Item"
    : isEventFlow
      ? "Event Knowledge Upload"
      : "Upload Knowledge Articles"}
</h2>

      </div>

      <div className="flex items-center gap-2 bg-[#fef3c7] p-3 rounded-[8px] text-[#92400e] text-sm mb-5">
        <FaLightbulb />
        <span>
          <strong>Tip:</strong> Follow upload guidelines for faster approvals.
        </span>
      </div>

      <form onSubmit={handleSubmit}>
        {/* ------------------- Knowledge Item Details ------------------- */}
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
              className="w-full px-2 py-2 border border-[#d1d5db] rounded-[8px]"
            />
            <select
              name="domainId"
              value={form.domainId}
              onChange={handleInputChange}
              required
              className="w-full px-2 py-2 border border-[#d1d5db] rounded-[8px]"
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
              className="w-full px-2 py-2 border border-[#d1d5db] rounded-[8px]"
            >
              <option value="">Select Category</option>
              {categories.map((c) => (
                <option key={c.categoryId} value={c.categoryId}>
                  {c.categoryName}
                </option>
              ))}
            </select>
          </div>

          {/* Event checkbox (optional, just shows info) */}
          {form.isEventItem && (
            <div className="text-sm text-gray-700 mb-2">
              This submission will be linked to the event automatically.
            </div>
          )}
        </section>

        {/* ------------------- Languages ------------------- */}
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
              setForm((prev) => ({ ...prev, languages: e.target.value }))
            }
            className="w-full px-2 py-2 border border-[#d1d5db] rounded-[8px]"
          />
        </section>

        {/* ------------------- Frameworks ------------------- */}
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
            className="w-full px-2 py-2 border border-[#d1d5db] rounded-[8px]"
          />
        </section>

        {/* ------------------- Tags ------------------- */}
        <section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
          <h3 className="text-[16px] font-semibold mb-2 text-[#111827]">Tags</h3>
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
                  addTag();
                }
              }}
            />
            <button
              type="button"
              className="px-2 py-1 rounded-[18px] bg-[#06b6d4] text-white"
              onClick={addTag}
            >
              Add
            </button>
          </div>
        </section>

        {/* ------------------- Description ------------------- */}
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
            className="w-full min-h-[100px] px-2 py-2 border border-[#d1d5db] rounded-[8px]"
          />
        </section>

        {/* ------------------- Upload Options ------------------- */}
        <section className="bg-[#f9fafb] p-4 rounded-[8px] mb-6">
          <h3 className="text-[16px] font-semibold mb-3 text-[#111827]">
            Upload Options
          </h3>
          <div className="flex gap-2 mb-3">
            {["File", "Text", "Video", "Audio"].map((tab) => (
              <button
                type="button"
                key={tab}
                className={`px-2 py-1 rounded-[15px] ${
                  activeTab === tab ? "bg-[#e4a931] text-[#0c0c0c]" : "bg-[#fef3c7]"
                }`}
                onClick={() => handleTabChange(tab)}
              >
                {tab}
              </button>
            ))}
          </div>

          {activeTab === "File" && (
            <>
              {/* existing attachments (prefill) */}
              {existingAttachments.length > 0 && (
                <div className="mb-4">
                  <div className="text-sm mb-2">Existing attachments — keep/remove</div>
                  <div className="flex flex-col gap-2 mb-2">
                    {existingAttachments.map((att) => {
                      const kept = keptExistingAttachmentIds.includes(att.attachmentId);
                      return (
                        <div key={att.attachmentId} className="flex items-center gap-3 p-2 border rounded-[8px] bg-white">
                          <a href={att.fileUrl} target="_blank" rel="noreferrer" className="text-sm text-blue-600 underline">{att.fileName}</a>
                          <span className="text-xs text-gray-500">{att.mimeType}</span>
                          <div className="ml-auto">
                            {kept ? (
                              <button type="button" className="px-2 py-1 rounded bg-red-400 text-white" onClick={() => handleRemoveExistingAttachment(att.attachmentId)}>Remove</button>
                            ) : (
                              <button type="button" className="px-2 py-1 rounded bg-green-400 text-white" onClick={() => handleKeepExistingAttachment(att.attachmentId)}>Keep</button>
                            )}
                          </div>
                        </div>
                      );
                    })}
                  </div>

                  <label className="inline-flex items-center gap-2 text-sm">
                    <input type="checkbox" checked={replaceAttachments} onChange={(e) => setReplaceAttachments(e.target.checked)} />
                    <span>Replace all existing attachments with newly uploaded files</span>
                  </label>
                </div>
              )}

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

        {/* ------------------- Footer Buttons ------------------- */}
        <div className="flex gap-2 justify-end mt-5">
          <button
            type="submit"
            className="px-4 py-2 rounded-[20px] bg-[#eab308] text-white"
          >
            {editItemId ? "Update" : "Upload"}
          </button>
        </div>
      </form>
    </div>
  );
}
