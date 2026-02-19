import React, { useState, useEffect } from "react";
import axios from "axios";
import EngagementButtons from "./EngagementButtons";
import VersionFilesModal from "./VersionFilesModal"; // 
import {
  FileText,
  BookOpen,
  User,
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
  <div className="pb-2 pt-3 px-7 bg-slate-50 ">
    {loading && (
      <p className="text-center text-gray-500">Loading engagements...</p>
    )}
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
      {items.slice(0, showAll ? items.length : 3).map((item, idx) => {
        const id = item.itemId || item.id;
        const data = engagementData[id] || {};

        return (
          <div
            key={id || idx}
            className="
              bg-white 
              border border-slate-200 
              rounded-xl 
              shadow-sm 
              hover:shadow-md 
              transition 
              flex flex-col 
              h-[320px]
            "
          >
            {/* Header */}
            <div className="px-5 py-4 border-b border-slate-100 flex justify-between items-start">
              <div className="flex gap-3">
                <div className="p-2">
                  <BookOpen className="w-4 h-4 text-indigo-600" />
                </div>
                <div>
                  <h3 className="font-semibold text-slate-900 leading-tight line-clamp-2">
                    {item.title}
                  </h3>
                  <p className="text-xs text-slate-500 mt-0.5">
                    {item.domainName || "Domain"} •{" "}
                    {item.categoryName || "Category"}
                  </p>
                </div>
              </div>

              <button
                onClick={() => setFilesItemId(id)}
                className="p-2 rounded-lg hover:bg-slate-100 transition"
                title="View versions & files"
              >
                <FileText className="w-4 h-4 text-slate-500" />
              </button>
            </div>

            {/* Body */}
            <div className="px-5 py-3 flex flex-col flex-grow gap-3">
              {/* Description */}
              <p className="text-sm text-slate-700 line-clamp-3">
                {item.description || "No description available."}
              </p>

              {/* Tags (max 2 rows) */}
              <div className="flex flex-wrap gap-2 max-h-[48px] overflow-hidden">
                {item.tags?.slice(0, 6).map((tag, tIdx) => (
                  <span
                    key={tIdx}
                    className="
                      px-2 py-0.5 
                      text-xs 
                      rounded-md 
                      bg-slate-100 
                      text-slate-700
                      border border-slate-200
                    "
                  >
                    {tag}
                  </span>
                ))}
              </div>

              {/* Owner */}
              <div className="flex items-center gap-2 text-xs text-slate-500 mt-auto">
                <User size={12} />
                <span>
                  {item.ownerName || item.submittedBy || "Unknown"}
                </span>
              </div>
            </div>

            {/* Footer (fixed height) */}
            <div className="border-t border-slate-100 px-4 py-2 bg-slate-50">
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
                themeColor="slate"
              />
            </div>
          </div>
        );
      })}
    </div>
{items.length > 3 && (
  <div className="flex justify-center mt-4">
    <button
      onClick={() => setShowAll(!showAll)}
      className="
        px-6 py-2 
        text-sm font-medium 
        rounded-lg 
        border border-slate-300 
        bg-white 
        hover:bg-slate-100
      "
    >
      {showAll ? "View Less" : "View More"}
    </button>
  </div>
)}
    {commentModalItem && (
      <div className="fixed inset-0 bg-black bg-opacity-40 flex justify-center items-center z-50">
        <div className="bg-white rounded-xl shadow-xl w-full max-w-lg p-6 relative">
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

          <div className="flex gap-2 mt-4">
            <textarea
              className="flex-1 border rounded-lg px-3 py-2 text-sm resize-none"
              placeholder="Add a comment..."
              value={newComment}
              rows={1}
              onChange={(e) => setNewComment(e.target.value)}
              onKeyDown={(e) =>
                handleKeyDown(e, () => handleAddComment(newComment))
              }
            />
            <button
              disabled={!newComment.trim()}
              onClick={() => handleAddComment(newComment)}
              className="px-4 py-2 bg-slate-800 text-white rounded-lg disabled:opacity-50"
            >
              <Send size={16} />
            </button>
          </div>
        </div>
      </div>
    )}

    {filesItemId && (
      <VersionFilesModal
        itemId={filesItemId}
        onClose={() => setFilesItemId(null)}
      />
    )}
  </div>
);

}