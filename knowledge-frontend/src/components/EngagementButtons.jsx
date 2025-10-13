import React, { useState, useEffect } from "react";
import { Heart, Star, MessageCircle, Eye } from "lucide-react";

export default function EngagementButtons({
  item,
  onLike,
  onFavourite,
  onComment: handleCommentProp, 
  onPreview,
  isLiked,
  isFav,
  likeCount,
  comments
}) {
  const [showCommentModal, setShowCommentModal] = useState(false);
  const [commentText, setCommentText] = useState("");

  // Local state to toggle colors instantly
  const [liked, setLiked] = useState(isLiked);
  const [favoured, setFavoured] = useState(isFav);

  // Sync with props when they change
  useEffect(() => {
    setLiked(isLiked);
  }, [isLiked]);

  useEffect(() => {
    setFavoured(isFav);
  }, [isFav]);

  const handleLike = (e) => {
    e.stopPropagation();
    setLiked(!liked); // instant UI change
    onLike(item); // call parent function for backend
  };

  const handleFavourite = (e) => {
    e.stopPropagation();
    setFavoured(!favoured); // instant UI change
    onFavourite(item); // call parent function for backend
  };

  const handleCommentSend = (e) => {
    e.stopPropagation();
    if (!commentText.trim()) return;

    handleCommentProp(item, commentText.trim()); // send comment text to parent
    setCommentText("");
    setShowCommentModal(false);
  };

  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-center gap-4">
        {/* Preview Button */}
        <button
          onClick={(e) => { e.stopPropagation(); onPreview(item); }}
          className="flex items-center p-2 rounded-full bg-purple-50 hover:bg-purple-100"
          title="Preview"
        >
          <Eye size={20} className="text-purple-500" />
        </button>

        {/* Like Button */}
        <button
          onClick={handleLike}
          className={`flex items-center p-2 rounded-full ${liked ? "bg-red-100" : "bg-red-50 hover:bg-red-100"}`}
          title="Like"
        >
          <Heart size={20} className={liked ? "text-red-500" : "text-red-300"} />
          <span className="ml-1 text-sm">{likeCount}</span>
        </button>

        {/* Favourite Button */}
        <button
          onClick={handleFavourite}
          className={`flex items-center p-2 rounded-full ${favoured ? "bg-yellow-200" : "bg-yellow-50 hover:bg-yellow-100"}`}
          title="Favourite"
        >
          <Star size={20} className={favoured ? "text-yellow-500" : "text-yellow-300"} />
        </button>

        {/* Comment Button */}
        <button
          onClick={(e) => { e.stopPropagation(); setShowCommentModal(true); }}
          className="flex items-center p-2 rounded-full bg-blue-50 hover:bg-blue-100"
          title="Comments"
        >
          <MessageCircle size={20} className="text-blue-500" />
          <span className="ml-1 text-sm">{comments?.length || 0}</span>
        </button>
      </div>

      {/* Comments Modal */}
      {showCommentModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="relative bg-white p-6 rounded-xl shadow-xl max-w-lg w-full mx-4">

            {/* Close Button */}
            <button
              className="absolute top-3 right-3 text-gray-600 hover:text-gray-900 text-xl font-bold"
              onClick={() => setShowCommentModal(false)}
              title="Close"
            >
              ✕
            </button>

            <h3 className="text-xl font-bold text-purple-700 mb-4">
              Comments
            </h3>

            <div className="max-h-80 overflow-y-auto space-y-4 mb-6">
              {comments.length > 0 ? (
                comments.map((c, idx) => (
                  <div key={idx} className="p-3 bg-gray-50 border border-gray-200 rounded-lg shadow-sm">
                    <div className="flex justify-between items-center mb-1">
                      <span className="font-medium text-gray-800">
                        {c.userName || "Anonymous"}
                      </span>
                      <span className="text-xs text-gray-500">
                        {new Date(c.timestamp || Date.now()).toLocaleString()}
                      </span>
                    </div>
                    <p className="text-gray-700">{c.text || c.commentText}</p>
                  </div>
                ))
              ) : (
                <p className="text-sm text-gray-500">No comments yet.</p>
              )}
            </div>

            {/* Add Comment Input */}
            <div className="flex gap-2">
              <input
                type="text"
                className="flex-1 p-2 border rounded focus:outline-none focus:ring-2 focus:ring-purple-400"
                placeholder="Add a comment..."
                value={commentText}
                onChange={(e) => setCommentText(e.target.value)}
              />
              <button
                onClick={handleCommentSend}
                className="px-4 py-2 bg-purple-600 text-white rounded hover:bg-purple-700 shadow"
              >
                Send
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
