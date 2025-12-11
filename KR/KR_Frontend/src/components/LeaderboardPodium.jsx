const gradients = [
  "from-yellow-300 to-yellow-500",
  "from-blue-300 to-blue-500",
  "from-orange-300 to-orange-500"
];

const LeaderboardPodium = ({ leaderboardData, loading, error }) => {
  if (loading) return <p className="text-sm ml-6">Loading leaderboard...</p>;
  if (error) return <p className="text-red-500 text-sm ml-6">{error}</p>;
  if (!leaderboardData || leaderboardData.length === 0) return null;

  return (
    <div className="mx-6 mt-10">
      <h2 className="text-lg font-bold text-gray-800 mb-3">🏆 Top Performers</h2>

      <div className="flex justify-center items-end gap-4 mt-6">

        {leaderboardData.slice(0, 3).map((user, index) => {
          const isFirst = index === 0;

          return (
            <div
              key={index}
              className={`
                flex flex-col items-center text-center p-4 rounded-2xl shadow-lg
                bg-gradient-to-br ${gradients[index]}
                transition-all hover:-translate-y-2
                ${isFirst ? "scale-110 z-10" : "scale-95"}
              `}
            >
              {/* Crown for Rank 1 */}
              {isFirst && (
                <img 
                  src="/assets/crown.png"
                  alt="crown"
                  className="w-12 h-12 mb-2 motion-safe:animate-[float_3s_ease-in-out_infinite]"
                />
              )}

              {/* Avatar */}
              <div className="
                w-20 h-20 rounded-full overflow-hidden shadow-md border-2 border-white mb-2 
                motion-safe:animate-[floatSlow_4s_ease-in-out_infinite]
              ">
                <img 
                  src={`/assets/avatars/${user.userName}.png`} 
                  className="w-full h-full object-cover"
                  alt={user.userName}
                />
              </div>

              <span className="text-white font-extrabold text-lg drop-shadow-md">
                #{index + 1}
              </span>

              <h4 className="font-bold text-white text-md">{user.userName}</h4>

              <p className="text-white text-sm opacity-90 mt-1">
                ❤️ {user.totalLikesReceived} Likes
              </p>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default LeaderboardPodium;
