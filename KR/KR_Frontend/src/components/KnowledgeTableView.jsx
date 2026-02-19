
import { FileText } from "lucide-react";

export default function KnowledgeTableView({ items, onSelect }) {
  return (
    <div className="bg-white border rounded-lg shadow-sm overflow-hidden">
      <table className="w-full text-sm">
        <thead className="bg-gray-100 text-gray-700">
          <tr>
            <th className="px-4 py-2 text-left">Title</th>
            <th className="px-4 py-2">Domain</th>
            <th className="px-4 py-2">Category</th>
            <th className="px-4 py-2">Owner</th>
            <th className="px-4 py-2">Engagement</th>
            <th className="px-4 py-2">Files</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr
              key={item.itemId}
              className="border-t hover:bg-indigo-50 cursor-pointer"
              onClick={() => onSelect(item)}
            >
              <td className="px-4 py-2 font-medium text-indigo-700">
                {item.title}
              </td>
              <td className="px-4 py-2">{item.domainName}</td>
              <td className="px-4 py-2">{item.categoryName}</td>
              <td className="px-4 py-2">{item.ownerName}</td>
              <td className="px-4 py-2 text-center">
                {item.engagementScore || 0}
              </td>
              <td className="px-4 py-2 text-center">
                <FileText size={16} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
