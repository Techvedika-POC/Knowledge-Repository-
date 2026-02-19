export default function SectionTabs({ activeSection, onSectionChange }) {
  const sections = [
    {
      key: "freshPicks",
      label: "Fresh Picks",
      color: "bg-green-100 text-green-800 hover:bg-green-200",
    },
    {
      key: "trending",
      label: "Trending",
      color: "bg-pink-100 text-pink-800 hover:bg-pink-200",
    },
    {
      key: "topics",
      label: "Topics",
      color: "bg-yellow-100 text-yellow-800 hover:bg-yellow-200",
    },
  ];

  return (
    <div className="flex gap-4 px-10 overflow-x-auto mb-1 mt-2">
      {sections.map((section) => (
        <button
          key={section.key}
          onClick={() => onSectionChange(section.key)}
          className={`px-4 py-2 rounded-full text-sm font-medium transition ${
            activeSection === section.key
              ? "bg-indigo-600 text-white"
              : section.color
          }`}
        >
          {section.label}
        </button>
      ))}
    </div>
  );
}
