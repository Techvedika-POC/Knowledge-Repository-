// VersionFilesModal.jsx
import React, { useEffect, useState } from "react";
import api from "../api"; // adjust if needed
import {
  X,
  FileText,
  FileImage,
  FileArchive,
  FileCode,
  FileSpreadsheet,
  FileType,
  ChevronDown,
  ExternalLink,
  // optional: Download icon from lucide-react if you want
} from "lucide-react";

export default function VersionFilesModal({ itemId, onClose }) {
  const [versions, setVersions] = useState([]);
  const [openVersion, setOpenVersion] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!itemId) return;
    setLoading(true);
    api
      .get(`/KnowledgeItem/${itemId}/versions`)
      .then((res) => setVersions(res.data || []))
      .catch((err) => {
        console.error("Failed to load versions:", err);
        setVersions([]);
      })
      .finally(() => setLoading(false));
  }, [itemId]);

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
    const i = Math.floor(Math.log(bytes) / Math.log(1024)) || 0;
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

  const toggle = (v) => setOpenVersion(openVersion === v ? null : v);

  const extFromName = (name = "") => (name.split(".").pop() || "").toLowerCase();

  const isPreviewableByBrowser = (mimeOrName = "", fileName = "") => {
    const lc = (mimeOrName || "").toLowerCase();
    const ext = extFromName(fileName);
    return (
      lc.includes("pdf") ||
      lc.startsWith("image/") ||
      lc.startsWith("text/") ||
      lc.startsWith("video/") ||
      lc.startsWith("audio/") ||
      /\.(pdf|txt|log|json|xml|html|htm|jpg|jpeg|png|gif|mp4|webm|mp3)$/i.test(fileName) ||
      /\.(pdf|txt|json|xml|html|htm)$/i.test(lc)
    );
  };

  const isOfficeFile = (fileName = "") => /\.(docx|xlsx|pptx|doc|ppt)$/i.test(fileName);

 
  const normalizeToAbsolute = (path) => {
    if (!path) return "";
    if (/^https?:\/\//i.test(path)) return path;

    const base = api.defaults.baseURL || (process.env.REACT_APP_API_URL || window.location.origin);
    const backendOrigin = base.replace(/\/api\/?$/i, "");
    if (!path.startsWith("/")) path = "/" + path;
    return backendOrigin + path;
  };


  const previewFile = async (attachment) => {
    try {
      let fileUrl = attachment.fileUrl || attachment.filePath || "";
      if (fileUrl && fileUrl.startsWith("/")) fileUrl = normalizeToAbsolute(fileUrl);


      if (!fileUrl) {
        fileUrl = normalizeToAbsolute(`/api/KnowledgeItem/attachment/${attachment.attachmentId}`);
      }

      const filename = attachment.fileName || "";
      const mime = (attachment.mimeType || "").toLowerCase();

     
      if (isOfficeFile(filename) && /^https?:\/\//i.test(fileUrl)) {
        const officeViewer = `https://view.officeapps.live.com/op/view.aspx?src=${encodeURIComponent(fileUrl)}`;
        const w = window.open("", "_blank", "noopener,noreferrer");
        if (w) {
          try {
            w.document.write(`<html><body style="font-family:system-ui;padding:20px"><h3>Opening ${filename}…</h3><div style="color:#666">Redirecting to Office viewer</div></body></html>`);
          } catch {}
          w.location.href = officeViewer;
          return;
        }
    
      }

      if (isPreviewableByBrowser(mime, filename) && /^https?:\/\//i.test(fileUrl)) {
        const newWin = window.open(fileUrl, "_blank", "noopener,noreferrer");
        if (newWin) return;

      }


      const res = await api.get(`/KnowledgeItem/attachment/${attachment.attachmentId}`, { responseType: "blob" });
      const contentType = (res.headers && (res.headers["content-type"] || res.headers["Content-Type"])) || attachment.mimeType || "application/octet-stream";
      const blob = new Blob([res.data], { type: contentType });
      const blobUrl = window.URL.createObjectURL(blob);


      const newWin = window.open("", "_blank", "noopener,noreferrer");
      if (newWin) {
        try {
          const low = (contentType || "").toLowerCase();
          let embedHtml = "";
          if (low.includes("pdf") || low.startsWith("text/") || low.includes("json") || low.includes("xml")) {
            embedHtml = `<iframe src="${blobUrl}" style="border:0;width:100%;height:100vh"></iframe>`;
          } else if (low.startsWith("image/")) {
            embedHtml = `<div style="display:flex;align-items:center;justify-content:center;height:100vh;margin:0;"><img src="${blobUrl}" alt="${filename}" style="max-width:100%;max-height:100%;"/></div>`;
          } else if (low.startsWith("video/")) {
            embedHtml = `<video src="${blobUrl}" controls style="width:100%;height:100vh;object-fit:contain"></video>`;
          } else if (low.startsWith("audio/")) {
            embedHtml = `<audio src="${blobUrl}" controls style="width:100%"></audio>`;
          } else {
   
            embedHtml = `<iframe src="${blobUrl}" style="border:0;width:100%;height:100vh"></iframe>`;
          }

          const html = `<html><head><title>${filename}</title><meta name="viewport" content="width=device-width,initial-scale=1"></head><body style="margin:0">${embedHtml}</body></html>`;
          newWin.document.open();
          newWin.document.write(html);
          newWin.document.close();

          setTimeout(() => { try { window.URL.revokeObjectURL(blobUrl); } catch {} }, 2 * 60 * 1000);
          return;
        } catch (e) {
   
          try { newWin.location.href = blobUrl; } catch {}
          setTimeout(() => { try { window.URL.revokeObjectURL(blobUrl); } catch {} }, 2 * 60 * 1000);
          return;
        }
      } else {
     
        window.location.href = blobUrl;
      }
    } catch (err) {
      console.error("Preview failed:", err);

      try {
        const fallback = normalizeToAbsolute(`/api/KnowledgeItem/attachment/${attachment.attachmentId}`);
        window.open(fallback, "_blank", "noopener,noreferrer");
      } catch (e) {
        console.error("Fallback open also failed:", e);
      }
    }
  };


  const downloadFile = async (attachment) => {
    try {
      const res = await api.get(`/KnowledgeItem/attachment/${attachment.attachmentId}`, { responseType: "blob" });
      const contentType = (res.headers && (res.headers["content-type"] || res.headers["Content-Type"])) || attachment.mimeType || "application/octet-stream";
      const filename = attachment.fileName || "file";
      const blob = new Blob([res.data], { type: contentType });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      a.remove();
      setTimeout(() => { try { window.URL.revokeObjectURL(url); } catch {} }, 60 * 1000);
    } catch (err) {
      console.error("Download failed:", err);
  
      try {
        const fallback = normalizeToAbsolute(`/api/KnowledgeItem/attachment/${attachment.attachmentId}`);
        window.open(fallback, "_blank", "noopener,noreferrer");
      } catch (e) {
        console.error("Fallback download open failed:", e);
      }
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl w-full max-w-3xl shadow-2xl overflow-hidden max-h-[90vh]">
        <div className="flex items-center justify-between px-6 py-4 bg-gradient-to-r from-indigo-600 to-blue-500 text-white">
          <div className="flex items-center gap-4">
            <div className="bg-white/20 p-2 rounded-lg">
              <FileText size={22} />
            </div>
            <div>
              <h3 className="text-lg font-semibold">Version Files</h3>
              <p className="text-sm opacity-90">Preview or download files</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <button onClick={onClose} aria-label="Close" className="p-2 rounded-md bg-white/20 hover:bg-white/30 transition">
              <X size={18} />
            </button>
          </div>
        </div>

        <div className="p-6 overflow-auto" style={{ maxHeight: "60vh" }}>
          {loading ? (
            <div className="text-center py-10 text-gray-500">Loading versions…</div>
          ) : (!versions || versions.length === 0) ? (
            <div className="text-center py-8 text-gray-500">No versions found.</div>
          ) : (
            <div className="space-y-4">
              {versions.map((v) => (
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
                      <div className={`transform transition-transform ${openVersion === v.versionNumber ? "rotate-180" : "rotate-0"}`}>
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
                          {v.attachments.map((att) => (
                            <li key={att.attachmentId} className="flex items-center justify-between bg-white rounded-lg p-3 shadow-sm border hover:shadow-md transition">
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

                              <div className="flex items-center gap-2">
                                <button
                                  onClick={() => previewFile(att)}
                                  className="inline-flex items-center gap-2 px-3 py-1 rounded-md bg-indigo-600 text-white text-sm font-medium hover:bg-indigo-700 transition"
                                  aria-label={`Preview ${att.fileName}`}
                                >
                                  <ExternalLink size={14} /> Preview
                                </button>

                                <button
                                  onClick={() => downloadFile(att)}
                                  className="inline-flex items-center gap-2 px-3 py-1 rounded-md bg-gray-100 text-gray-800 text-sm font-medium hover:bg-gray-200 transition"
                                  aria-label={`Download ${att.fileName}`}
                                >
                                  Download
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

        <div className="px-6 py-4 border-t text-right">
          <button onClick={onClose} className="px-4 py-2 rounded-md bg-gray-100 text-gray-700 hover:bg-gray-200 transition">
            Close
          </button>
        </div>
      </div>
    </div>
  );
}
