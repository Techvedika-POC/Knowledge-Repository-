const AnnouncementsBanner = ({ ideathonEvent }) => {
  if (!ideathonEvent) return null;

  return (
    <div className="mx-6 mt-6">
      <div className="
        bg-gradient-to-r from-pink-500 to-rose-500 
        text-white rounded-2xl shadow-lg p-5
        motion-safe:animate-[slideDown_0.4s_ease-out]
        relative overflow-hidden
      ">
        {/* background decorative glow */}
        <div className="absolute top-0 right-0 w-24 h-24 bg-white/10 rounded-full blur-2xl"></div>

        <div className="flex gap-4 items-center">
          <img 
            src="/assets/megaphone.png"
            className="w-16 h-16 motion-safe:animate-bounce"
            alt="megaphone"
          />

          <div className="flex-1">
            <span className="px-3 py-1 bg-white text-pink-700 text-xs font-bold rounded-full animate-pulse">
              NEW
            </span>

            <h3 className="text-xl font-extrabold mt-1">
              {ideathonEvent.name}
            </h3>

            <p className="text-sm opacity-90 mt-1">
              {ideathonEvent.description.slice(0, 120)}...
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AnnouncementsBanner;
