import React, { useState, useEffect } from "react";
import axios from "axios";
import EngagementButtons from "./EngagementButtons";
import VersionFilesModal from "./VersionFilesModal";
import {
  FileText,
  Tag,
  User,
  Type,
  MessageCircle,
  Send,
  X,
  Pencil,
  Trash2,
  CornerDownRight,
  ChevronDown,
  ChevronUp,
} from "lucide-react";

export default function KnowledgeCardsDisplay({ items = [], onPreview, userId, onReset }) {
  const [showAll, setShowAll] = useState(false);
  const [engagementData, setEngagementData] = useState({});
  const [loading, setLoading] = useState(false);

  const [commentModalItem, setCommentModalItem] = useState(null);
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState("");
  const [replyTo, setReplyTo] = useState(null);
  const [editingComment, setEditingComment] = useState(null);
  const [editText, setEditText] = useState("");
  const [expandedComments, setExpandedComments] = useState({});
  const [filesItemId, setFilesItemId] = useState(null);

  const themeColor = "indigo";
  const cardHighlightColor = "blue";

  // Helper for Enter key behavior
  const handleKeyDown = (e, action) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      action();
    }
  };

  const fetchEngagementData = async () => {
    if (!userId) return;
    setLoading(true);
    const data = {};
    try {
      await Promise.all(
        items.map(async (item) => {
          const id = item.itemId || item.id;
          if (!id) return;
          const res = await axios.get(`/api/Engagement/summary/${id}?userId=${userId}`);
          data[id] = res.data;
        })
      );
      setEngagementData(data);
    } catch (err) {
      console.error("Error fetching engagement data", err);
    }
    setLoading(false);
  };

  useEffect(() => {
    if (items.length) fetchEngagementData();
  }, [items, userId]);

  // -----------------------------
  // Like / Favourite
  // -----------------------------
  const handleLike = async (item) => {
    const id = item.itemId || item.id;
    const isLiked = engagementData[id]?.userEngagementTypes?.includes("Like");
    try {
      if (isLiked) await axios.delete(`/api/Engagement/like/${id}`, { params: { userId } });
      else await axios.post(`/api/Engagement/like/${id}`, null, { params: { userId } });
      fetchEngagementData();
    } catch (err) {
      console.error("Error liking/unliking item", err);
    }
  };

  const handleFavourite = async (item) => {
    const id = item.itemId || item.id;
    const isFav = engagementData[id]?.userEngagementTypes?.includes("Favourite");
    try {
      if (isFav) await axios.delete(`/api/Engagement/favourite/${id}`, { params: { userId } });
      else await axios.post(`/api/Engagement/favourite/${id}`, null, { params: { userId } });
      fetchEngagementData();
    } catch (err) {
      console.error("Error favouriting/unfavouriting item", err);
    }
  };

  // -----------------------------
  // Comments Logic
  // -----------------------------
  const openCommentsModal = async (item) => {
    setCommentModalItem(item);
    const id = item.itemId || item.id;
    const res = await axios.get(`/api/Engagement/comments/${id}`);
    setComments(res.data || []);
  };

  const refreshComments = async () => {
    if (!commentModalItem) return;
    const id = commentModalItem.itemId || commentModalItem.id;
    const res = await axios.get(`/api/Engagement/comments/${id}`);
    setComments(res.data || []);
  };

  const handleAddComment = async (text, parentCommentId = null) => {
    if (!text.trim() || !commentModalItem) return;
    const id = commentModalItem.itemId || commentModalItem.id;

    await axios.post(
      `/api/Engagement/comment/${id}`,
      { CommentText: text, ParentCommentId: parentCommentId },
      { params: { userId } }
    );

    await refreshComments();
    setNewComment("");
    setReplyTo(null);
  };

  const handleEditComment = async (commentId, newText) => {
    if (!newText.trim()) return;
    try {
      await axios.put(`/api/Engagement/comment/${commentId}`, {
        CommentText: newText,
      });
      await refreshComments();
      setEditingComment(null);
      setEditText("");
    } catch (err) {
      console.error("Error updating comment", err);
    }
  };

  const handleDeleteComment = async (commentId) => {
    if (!window.confirm("Are you sure you want to delete this comment?")) return;
    await axios.delete(`/api/Engagement/comment/${commentId}`);
    await refreshComments();
  };

  const toggleReplies = (commentId) => {
    setExpandedComments((prev) => ({
      ...prev,
      [commentId]: !prev[commentId],
    }));
  };

  // Recursive rendering
  const renderComments = (list, level = 0) =>
    list.map((c) => (
      <div
        key={c.engagementId}
        className={`p-3 border rounded-xl bg-gray-50 mb-2 transition-all ${level > 0 ? "ml-6" : ""}`}
      >
        <div className="flex justify-between items-center">
          <span className="text-sm font-semibold text-gray-800">
            {c.userName || "Anonymous"}
          </span>
          <span className="text-xs text-gray-500">
            {new Date(c.createdOn).toLocaleString()}
          </span>
        </div>

        {editingComment === c.engagementId ? (
          <div className="flex gap-2 mt-2">
            <textarea
              className="border rounded px-2 py-1 flex-1 text-sm resize-none focus:ring-2 focus:ring-indigo-300 outline-none"
              value={editText}
              rows={1}
              onChange={(e) => setEditText(e.target.value)}
              onKeyDown={(e) =>
                handleKeyDown(e, () => handleEditComment(c.engagementId, editText))
              }
            />
            <button
              disabled={!editText.trim()}
              onClick={() => handleEditComment(c.engagementId, editText)}
              className={`px-3 py-1 text-xs text-white bg-${themeColor}-500 rounded hover:bg-${themeColor}-600 disabled:opacity-50`}
            >
              Save
            </button>
          </div>
        ) : (
          <p className="text-sm mt-1 text-gray-700 whitespace-pre-line">
            {c.commentText}
          </p>
        )}

        <div className="flex flex-wrap gap-3 mt-2 text-xs text-gray-500">
          <button
            onClick={() =>
              setReplyTo(c.engagementId === replyTo ? null : c.engagementId)
            }
            className="flex items-center gap-1 hover:text-indigo-600"
          >
            <CornerDownRight size={12} />{" "}
            {replyTo === c.engagementId ? "Cancel" : "Reply"}
          </button>

          {c.replies?.length > 0 && (
            <button
              onClick={() => toggleReplies(c.engagementId)}
              className="flex items-center gap-1 hover:text-indigo-600"
            >
              {expandedComments[c.engagementId] ? (
                <>
                  <ChevronUp size={12} /> Hide Replies
                </>
              ) : (
                <>
                  <ChevronDown size={12} /> View Replies ({c.replies.length})
                </>
              )}
            </button>
          )}

          {c.userId === userId && (
            <>
              <button
                onClick={() => {
                  setEditingComment(c.engagementId);
                  setEditText(c.commentText);
                }}
                className="flex items-center gap-1 hover:text-indigo-600"
              >
                <Pencil size={12} /> Edit
              </button>
              <button
                onClick={() => handleDeleteComment(c.engagementId)}
                className="flex items-center gap-1 hover:text-red-500"
              >
                <Trash2 size={12} /> Delete
              </button>
            </>
          )}
        </div>

        {replyTo === c.engagementId && (
          <div className="flex gap-2 mt-2 ml-2">
            <textarea
              className="border rounded px-2 py-1 flex-1 text-sm resize-none focus:ring-2 focus:ring-indigo-300 outline-none"
              placeholder="Write a reply..."
              value={newComment}
              rows={1}
              onChange={(e) => setNewComment(e.target.value)}
              onKeyDown={(e) =>
                handleKeyDown(e, () => handleAddComment(newComment, c.engagementId))
              }
            />
            <button
              disabled={!newComment.trim()}
              onClick={() => handleAddComment(newComment, c.engagementId)}
              className={`px-3 py-1 bg-${themeColor}-500 text-white text-xs rounded hover:bg-${themeColor}-600 disabled:opacity-50`}
            >
              <Send size={14} /> Send
            </button>
          </div>
        )}

        {expandedComments[c.engagementId] && c.replies?.length > 0 && (
          <div className="ml-4 mt-2 border-l pl-3">
            {renderComments(c.replies, level + 1)}
          </div>
        )}
      </div>
    ));

  return (
    <div className="pb-16 p-6">
      {loading && (
        <p className="text-center text-gray-500">Loading engagements...</p>
      )}

      {/* Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {items.slice(0, showAll ? items.length : 3).map((item, idx) => {
          const id = item.itemId || item.id;
          const data = engagementData[id] || {};
          return (
            <div
              key={id || idx}
              className="bg-white rounded-2xl shadow-md border border-gray-200 hover:shadow-lg transition flex flex-col relative"
            >
              {/* Highlight strip */}
              <div
                className={`h-2 w-full rounded-t-2xl bg-${cardHighlightColor}-100`}
              ></div>

              {/* File icon - top-right */}
              <div className="absolute top-3 right-3 z-10">
                <button
                  onClick={() => setFilesItemId(id)}
                  className="p-2 rounded-full hover:bg-gray-100 transition"
                  aria-label="Open version files"
                >
                  <FileText className="w-5 h-5 text-indigo-600" />
                </button>
              </div>

              <div className="p-6 flex flex-col flex-grow">
                <div className="mb-4">
                  <div className="flex items-center gap-2 mb-2">
                    <Type className="w-4 h-4 text-purple-500" />
                    <span className="font-semibold text-sm text-gray-600">
                      Title:
                    </span>
                  </div>
                  <span className="font-medium text-gray-900">{item.title}</span>
                </div>

                <div className="mb-4">
                  <div className="flex items-center gap-2 mb-1">
                    <FileText className="w-4 h-4 text-indigo-500" />
                    <span className="font-semibold text-sm text-gray-600">
                      Description:
                    </span>
                  </div>
                  <p className="text-gray-700 text-sm truncate">
                    {item.description || "No description available."}
                  </p>
                </div>

                {item.tags?.length > 0 && (
                  <div className="flex flex-wrap gap-2 mb-4">
                    {item.tags.map((tag, tIdx) => (
                      <span
                        key={tIdx}
                        className={`px-2 py-1 text-xs rounded-full bg-${themeColor}-100 text-${themeColor}-800 font-medium flex items-center gap-1`}
                      >
                        <Tag className="w-3 h-3" /> {tag}
                      </span>
                    ))}
                  </div>
                )}

                <div className="flex items-center gap-2 mb-4">
                  <User className={`w-5 h-5 text-${themeColor}-500`} />
                  <span className="px-2 py-1 text-xs font-medium text-${themeColor}-800 rounded-full hover:bg-blue-100 transition cursor-pointer">
                    {item.ownerName || item.submittedBy || "Unknown Contributor"}
                  </span>
                </div>

                <div className="border-t border-gray-300 mb-2"></div>

                <div className="mt-auto flex justify-center">
                  <EngagementButtons
                    item={item}
                    onPreview={onPreview}
                    onLike={handleLike}
                    onFavourite={handleFavourite}
                    onCommentOpen={openCommentsModal}
                    isLiked={data.userEngagementTypes?.includes("Like")}
                    isFav={data.userEngagementTypes?.includes("Favourite")}
                    likeCount={data.likesCount || 0}
                    comments={data.comments || []}
                    themeColor={themeColor}
                  />
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* View More & Reset Buttons */}
      <div className="flex justify-center mt-8 space-x-4">
        <button
          onClick={() => setShowAll(!showAll)}
          className={`px-6 py-2 text-sm font-medium text-black bg-blue-300 rounded-full shadow hover:bg-${themeColor}-600 transition`}
        >
          {showAll ? "View Less" : "View More"}
        </button>
        <button
          onClick={() => {
            setShowAll(false);
            fetchEngagementData();
            if (onReset) onReset();
          }}
          className={`px-6 py-2 text-sm font-medium text-black bg-blue-300 rounded-full shadow hover:bg-${themeColor}-600 transition`}
        >
          Reset
        </button>
      </div>

      {/* Comments Modal */}
      {commentModalItem && (
        <div className="fixed inset-0 bg-black bg-opacity-40 flex justify-center items-center z-50">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg p-6 relative">
            <button
              className="absolute top-4 right-4 text-gray-600 hover:text-gray-900"
              onClick={() => setCommentModalItem(null)}
            >
              <X size={20} />
            </button>
            <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <MessageCircle size={18} /> Comments
            </h2>

            <div className="max-h-80 overflow-y-auto mb-3 space-y-3">
              {comments.length > 0 ? (
                renderComments(comments)
              ) : (
                <p className="text-sm text-gray-500">No comments yet.</p>
              )}
            </div>

            {/* Add New Comment */}
            <div className="flex gap-2 mt-4">
              <textarea
                className="flex-1 border rounded-full px-4 py-2 text-sm resize-none focus:ring-2 focus:ring-indigo-300 outline-none"
                placeholder="Add a comment..."
                value={newComment}
                rows={1}
                onChange={(e) => setNewComment(e.target.value)}
                onKeyDown={(e) => handleKeyDown(e, () => handleAddComment(newComment))}
              />
              <button
                disabled={!newComment.trim()}
                onClick={() => handleAddComment(newComment)}
                className={`px-4 py-2 bg-${themeColor}-500 text-white rounded-full hover:bg-${themeColor}-600 flex items-center gap-1 disabled:opacity-50`}
              >
                <Send size={16} /> Send
              </button>
            </div>
          </div>
        </div>
      )}
      {filesItemId && <VersionFilesModal itemId={filesItemId} onClose={() => setFilesItemId(null)} />}
    </div>
  );
}