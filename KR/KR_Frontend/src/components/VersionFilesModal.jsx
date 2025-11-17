import React, { useEffect, useState } from "react";
import axios from "axios";
import {
  X,
  FileText,
  FileImage,
  FileArchive,
  FileCode,
  FileSpreadsheet,
  FileType,
  ChevronDown,
  ChevronUp,
  ExternalLink,
} from "lucide-react";

export default function VersionFilesModal({ itemId, onClose }) {
  const [versions, setVersions] = useState([]);
  const [openVersion, setOpenVersion] = useState(null);
  const [loading, setLoading] = useState(false);

  const token = typeof window !== "undefined" ? localStorage.getItem("jwtToken") : null;

  const openFile = async (attachment) => {
    try {
      const url = attachment.fileUrl || attachment.filePath;
      console.log("Opening file URL:", url);

      if (!url) {
        console.warn("No URL available for attachment", attachment);
        return;
      }

      if (url.startsWith("http://") || url.startsWith("https://")) {
        const requiresAuth = false;
        if (!requiresAuth) {
          window.open(url, "_blank");
          return;
        }
      }

      const headers = token ? { Authorization: `Bearer ${token}` } : {};
      const res = await axios.get(url, { headers, responseType: "blob" });

      const contentType = (res.headers && (res.headers["content-type"] || res.headers["Content-Type"])) || attachment.mimeType || "application/octet-stream";
      const filename = attachment.fileName || "download";

      const blob = new Blob([res.data], { type: contentType });
      const blobUrl = window.URL.createObjectURL(blob);

      const newWindow = window.open(blobUrl, "_blank");
      if (newWindow) {
        setTimeout(() => window.URL.revokeObjectURL(blobUrl), 60 * 1000);
      } else {
        const a = document.createElement("a");
        a.href = blobUrl;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        a.remove();
        setTimeout(() => window.URL.revokeObjectURL(blobUrl), 60 * 1000);
      }
    } catch (err) {
      console.error("Failed to open file:", err);
    }
  };

  useEffect(() => {
    if (!itemId) return;
    setLoading(true);
    const headers = token ? { Authorization: `Bearer ${token}` } : {};
    axios
      .get(`/api/KnowledgeItem/${itemId}/versions`, { headers })
      .then((res) => setVersions(res.data || []))
      .catch((err) => {
        console.error("Failed to load versions:", err);
        setVersions([]);
      })
      .finally(() => setLoading(false));
  }, [itemId, token]);

  const toggle = (v) => {
    setOpenVersion(openVersion === v ? null : v);
  };

  const formatDate = (iso) => {
    if (!iso) return "";
    try {
      return new Date(iso).toLocaleString();
    } catch {
      return iso;
    }
  };

  const formatBytes = (bytes) => {
    if (!bytes && bytes !== 0) return "";
    const sizes = ["B", "KB", "MB", "GB", "TB"];
    if (bytes === 0) return "0 B";
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${(bytes / Math.pow(1024, i)).toFixed(i ? 1 : 0)} ${sizes[i]}`;
  };

  const getFileIcon = (fileName) => {
    if (!fileName) return <FileType size={18} className="text-gray-400" />;

    const ext = fileName.split(".").pop().toLowerCase();
    const baseProps = { size: 18 };

    switch (ext) {
      case "pdf":
        return <FileText {...baseProps} className="text-red-500" />;
      case "jpg":
      case "jpeg":
      case "png":
      case "gif":
        return <FileImage {...baseProps} className="text-blue-500" />;
      case "zip":
      case "rar":
        return <FileArchive {...baseProps} className="text-yellow-600" />;
      case "xlsx":
      case "csv":
        return <FileSpreadsheet {...baseProps} className="text-green-600" />;
      case "ppt":
      case "pptx":
      case "doc":
      case "docx":
        return <FileText {...baseProps} className="text-indigo-600" />;
      case "js":
      case "html":
      case "css":
        return <FileCode {...baseProps} className="text-purple-600" />;
      default:
        return <FileType {...baseProps} className="text-gray-400" />;
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl w-full max-w-3xl shadow-2xl overflow-hidden max-h-[90vh]">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 bg-gradient-to-r from-indigo-600 to-blue-500 text-white">
          <div className="flex items-center gap-4">
            <div className="bg-white/20 p-2 rounded-lg">
              <FileText size={22} />
            </div>
            <div>
              <h3 className="text-lg font-semibold">Version Files</h3>
              <p className="text-sm opacity-90">All uploaded files for this item — preview or download</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <button
              onClick={onClose}
              aria-label="Close"
              className="p-2 rounded-md bg-white/20 hover:bg-white/30 transition"
            >
              <X size={18} />
            </button>
          </div>
        </div>

        {/* Body */}
        <div className="p-6">
          {loading ? (
            <div className="text-center py-10 text-gray-500">Loading versions…</div>
          ) : (!versions || versions.length === 0) ? (
            <div className="text-center py-8 text-gray-500">No versions found.</div>
          ) : (
            <div className="space-y-4">
              {(versions || []).map((v) => (
                <div key={v.versionId} className="border rounded-xl bg-gray-50 overflow-hidden shadow-sm">
                  <button
                    onClick={() => toggle(v.versionNumber)}
                    className="w-full flex items-center justify-between px-4 py-3 hover:bg-gray-100 focus:outline-none"
                    aria-expanded={openVersion === v.versionNumber}
                  >
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-md bg-white flex items-center justify-center shadow-sm">
                        <span className="text-sm font-medium">v{v.versionNumber}</span>
                      </div>
                      <div>
                        <div className="text-sm font-semibold text-gray-800">Version {v.versionNumber}</div>
                        <div className="text-xs text-gray-500">{formatDate(v.createdOn)}</div>
                      </div>
                    </div>

                    <div className="flex items-center gap-4">
                      <div className="text-xs text-gray-500 mr-2">{v.attachments?.length || 0} file(s)</div>
                      <div
                        className={`transform transition-transform ${openVersion === v.versionNumber ? "rotate-180" : "rotate-0"}`}
                      >
                        <ChevronDown size={18} />
                      </div>
                    </div>
                  </button>

                  {openVersion === v.versionNumber && (
                    <div className="px-4 pb-4 pt-2">
                      {(!v.attachments || v.attachments.length === 0) ? (
                        <div className="text-sm text-gray-500 p-3">No files uploaded for this version.</div>
                      ) : (
                        <ul className="space-y-2">
                          {(v.attachments || []).map((att) => (
                            <li
                              key={att.attachmentId}
                              className="flex items-center justify-between bg-white rounded-lg p-3 shadow-sm border hover:shadow-md transition"
                            >
                              <div className="flex items-center gap-3 min-w-0">
                                <div className="flex-shrink-0">{getFileIcon(att.fileName)}</div>
                                <div className="min-w-0">
                                  <div className="text-sm font-medium text-gray-800 truncate">{att.fileName}</div>
                                  <div className="text-xs text-gray-500 flex items-center gap-2">
                                    {att.mimeType && <span>{att.mimeType}</span>}
                                    {att.fileSize != null && <span>• {formatBytes(att.fileSize)}</span>}
                                  </div>
                                </div>
                              </div>

                              <div className="flex items-center gap-3">
                                <button
                                  onClick={() => openFile(att)}
                                  className="inline-flex items-center gap-2 px-3 py-1 rounded-md bg-indigo-600 text-white text-sm font-medium hover:bg-indigo-700 transition"
                                  aria-label={`Open ${att.fileName}`}
                                >
                                  <ExternalLink size={14} /> Open
                                </button>
                              </div>
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t text-right">
          <button
            onClick={onClose}
            className="px-4 py-2 rounded-md bg-gray-100 text-gray-700 hover:bg-gray-200 transition"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
}
