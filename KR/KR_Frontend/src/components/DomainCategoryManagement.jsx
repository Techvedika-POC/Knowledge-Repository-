import React, { useState, useEffect } from "react";
import { toast } from "react-hot-toast";
import api from "../api";

import {
  PencilSquareIcon,
  TrashIcon,
  PlusIcon,
  CheckIcon,
} from "@heroicons/react/24/outline";

export default function DomainCategoryManagement() {
  const [activeSection, setActiveSection] = useState("domains");
  const [domains, setDomains] = useState([]);
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({ id: "", name: "", domainId: "" });
  const [showForm, setShowForm] = useState(false);

  const emptyForm = { id: "", name: "", domainId: "" };

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const d = await api.get("/Domains");
      const c = await api.get("/Categories");
      setDomains(d.data);
      setCategories(c.data);
    } catch {
      toast.error("Failed to load data");
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.name.trim()) return toast.error("Name required");

    try {
      if (activeSection === "domains") {
        const payload = { domainName: form.name.trim() };
        form.id
          ? await api.put(`/Domains/${form.id}`, payload)
          : await api.post("/Domains", payload);
      } else {
        if (!form.domainId) return toast.error("Select domain");

        const payload = {
          categoryName: form.name.trim(),
          domainId: form.domainId,
        };

        form.id
          ? await api.put(`/Categories/${form.id}`, payload)
          : await api.post("/Categories", payload);
      }

      toast.success(form.id ? "Updated" : "Added");
      setForm(emptyForm);
      setShowForm(false);
      loadData();
    } catch {
      toast.error("Save failed");
    }
  };

  const handleEdit = (item) => {
    setForm({
      id: activeSection === "domains" ? item.domainId : item.categoryId,
      name:
        activeSection === "domains"
          ? item.domainName
          : item.categoryName,
      domainId: item.domainId || "",
    });
    setShowForm(true);
  };

  const handleDelete = async (id) => {
    try {
      activeSection === "domains"
        ? await api.delete(`/Domains/${id}`)
        : await api.delete(`/Categories/${id}`);

      toast.success("Deleted");
      loadData();
    } catch {
      toast.error("Delete failed");
    }
  };

  const list = activeSection === "domains" ? domains : categories;

return (
  <div className="max-w-4xl mx-auto py-8">
    <div className="flex items-center justify-between mb-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">
          Domain & Category Management
        </h1>
        <p className="text-sm text-gray-500 mt-1">
          Manage knowledge classification structure
        </p>
      </div>

      <button
        onClick={() => {
          setForm(emptyForm);
          setShowForm(true);
        }}
        className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-md shadow hover:bg-blue-700 transition"
      >
        <PlusIcon className="w-5 h-5" />
        Add {activeSection === "domains" ? "Domain" : "Category"}
      </button>
    </div>
    <div className="border-b border-gray-200 mb-6">
      <nav className="flex gap-8">
        {["domains", "categories"].map((tab) => (
          <button
            key={tab}
            onClick={() => {
              setActiveSection(tab);
              setForm(emptyForm);
              setShowForm(false);
            }}
            className={`pb-3 text-sm font-medium transition
              ${activeSection === tab
                ? "text-blue-600 border-b-2 border-blue-600"
                : "text-gray-500 hover:text-gray-700"}
            `}
          >
            {tab === "domains" ? "Domains" : "Categories"}
          </button>
        ))}
      </nav>
    </div>

    {showForm && (
      <div className="mb-6 bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
        <form onSubmit={handleSubmit} className="flex items-center gap-4">

          {activeSection === "categories" && (
            <select
              className="border border-gray-300 rounded-md px-3 py-2 text-sm"
              value={form.domainId}
              onChange={(e) =>
                setForm({ ...form, domainId: e.target.value })
              }
            >
              <option value="">Select domain</option>
              {domains.map((d) => (
                <option key={d.domainId} value={d.domainId}>
                  {d.domainName}
                </option>
              ))}
            </select>
          )}

          <input
            type="text"
            placeholder={
              activeSection === "domains"
                ? "Domain name"
                : "Category name"
            }
            value={form.name}
            onChange={(e) =>
              setForm({ ...form, name: e.target.value })
            }
            className="flex-1 border border-gray-300 rounded-md px-3 py-2 text-sm"
          />

          <button
            type="submit"
            className="inline-flex items-center gap-1 px-3 py-2 bg-green-600 text-white rounded-md hover:bg-green-700"
          >
            <CheckIcon className="w-4 h-4" />
            Save
          </button>

          <button
            type="button"
            onClick={() => {
              setShowForm(false);
              setForm(emptyForm);
            }}
            className="px-3 py-2 border border-gray-300 rounded-md text-gray-600 hover:bg-gray-50"
          >
            Cancel
          </button>
        </form>
      </div>
    )}

    <div className="bg-white border border-gray-200 rounded-lg shadow-sm">

      {list.length === 0 ? (
        <p className="p-6 text-center text-gray-500 text-sm">
          No {activeSection} found.
        </p>
      ) : (
        <ul className="divide-y divide-gray-100">
          {list.map((item) => {
            const id =
              activeSection === "domains"
                ? item.domainId
                : item.categoryId;

            return (
              <li
                key={id}
                className="flex items-center justify-between px-6 py-4 hover:bg-gray-50 transition"
              >
                <div>
                  <p className="text-sm font-medium text-gray-900">
                    {activeSection === "domains"
                      ? item.domainName
                      : item.categoryName}
                  </p>

                  {activeSection === "categories" && (
                    <span className="inline-block mt-1 text-xs px-2 py-0.5 rounded-full bg-gray-100 text-gray-600">
                      {
                        domains.find(
                          (d) => d.domainId === item.domainId
                        )?.domainName
                      }
                    </span>
                  )}
                </div>

                <div className="flex items-center gap-2">
                  <button
                    onClick={() => handleEdit(item)}
                    className="p-2 rounded-md hover:bg-gray-100"
                    title="Edit"
                  >
                    <PencilSquareIcon className="w-5 h-5 text-gray-600" />
                  </button>

                  <button
                    onClick={() => handleDelete(id)}
                    className="p-2 rounded-md hover:bg-red-50"
                    title="Delete"
                  >
                    <TrashIcon className="w-5 h-5 text-red-600" />
                  </button>
                </div>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  </div>
);


}
