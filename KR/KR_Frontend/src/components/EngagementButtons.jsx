import React, { useState, useEffect } from "react";
import { Heart, Star, MessageCircle, Eye } from "lucide-react";

export default function EngagementButtons({
  item,
  onLike,
  onFavourite,
  onCommentOpen,
  onPreview,
  isLiked,
  isFav,
  likeCount,
  comments = [],
  themeColor = "indigo",
}) {
  const [liked, setLiked] = useState(isLiked);
  const [favoured, setFavoured] = useState(isFav);

  useEffect(() => setLiked(isLiked), [isLiked]);
  useEffect(() => setFavoured(isFav), [isFav]);

  const buttonClass = "flex items-center gap-1 p-2 rounded-full hover:bg-gray-200 transition cursor-pointer";

  return (
    <div className="flex items-center justify-center gap-3">
      <button onClick={(e) => { e.stopPropagation(); onPreview(item); }} className={buttonClass} title="Preview">
        <Eye size={20} className={`text-${themeColor}-500`} />
      </button>
      <button onClick={(e) => { e.stopPropagation(); setLiked(!liked); onLike(item); }} className={buttonClass} title="Like">
        <Heart size={20} className={liked ? "text-red-600" : "text-gray-400"} fill={liked ? "currentColor" : "none"} />
        <span className="text-sm">{likeCount || 0}</span>
      </button>
      <button onClick={(e) => { e.stopPropagation(); setFavoured(!favoured); onFavourite(item); }} className={buttonClass} title="Favourite">
        <Star size={20} className={favoured ? "text-yellow-500" : "text-gray-400"} fill={favoured ? "currentColor" : "none"} />
      </button>
      <button onClick={(e) => { e.stopPropagation(); onCommentOpen(item); }} className={buttonClass} title="Comments">
        <MessageCircle size={20} className={`text-${themeColor}-500`} />
        <span className="text-sm">{comments.length || 0}</span>
      </button>
    </div>
  );
}
