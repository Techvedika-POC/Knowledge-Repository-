import React, { useState, useEffect } from "react";
import { toast } from "react-hot-toast";
import api from "../api";

import {
  PencilSquareIcon,
  TrashIcon,
  PlusIcon,
  XMarkIcon,
  CheckIcon,
  ArrowDownOnSquareIcon,
  DocumentCheckIcon
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
    <div className="max-w-3xl mx-auto py-6">
      {/* PAGE TITLE */}
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">
        Domain & Category Settings
      </h1>

      {/* TABS */}
      <div className="flex items-center gap-6 mb-6">
        {["domains", "categories"].map((tab) => (
          <button
            key={tab}
            onClick={() => {
              setActiveSection(tab);
              setForm(emptyForm);
              setShowForm(false);
            }}
            className={`text-lg pb-1 transition ${activeSection === tab
                ? "text-blue-600 font-semibold border-b-2 border-blue-600"
                : "text-gray-600 hover:text-gray-800"
              }`}
          >
            {tab === "domains" ? "Domains" : "Categories"}
          </button>
        ))}

        {/* ADD ICON */}
        <button
          onClick={() => {
            setForm(emptyForm);
            setShowForm(true);
          }}
          className="ml-auto p-2 rounded-full hover:bg-gray-100 transition"
          title="Add"
        >
          <PlusIcon className="w-6 h-6 text-blue-600" />
        </button>
      </div>

      {/* ADD/EDIT INLINE FORM */}
      {showForm && (
        <form
          onSubmit={handleSubmit}
          className="mb-8 flex items-center gap-4 py-2"
        >
          {activeSection === "categories" && (
            <select
              className="border-b border-gray-300 px-2 py-1 focus:border-blue-500 outline-none"
              value={form.domainId}
              onChange={(e) =>
                setForm({ ...form, domainId: e.target.value })
              }
            >
              <option value="">Domain</option>
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
            className="border-b border-gray-300 px-2 py-1 focus:border-blue-500 outline-none w-64"
          />

          {/* SAVE ICON */}
          <button
            type="submit"
            className="p-2 rounded-full hover:bg-green-50"
            title="Save"
          >
            <DocumentCheckIcon className="w-6 h-6 text-green-600" />
          </button>
          {/* CANCEL ICON */}
          <button
            type="button"
            onClick={() => {
              setShowForm(false);
              setForm(emptyForm);
            }}
            className="p-2 rounded-full hover:bg-red-50"
            title="Cancel"
          >
            <XMarkIcon className="w-6 h-6 text-red-600" />
          </button>
        </form>
      )}

      {/* ITEM LIST */}
      <div className="space-y-1">
        {list.map((item) => (
          <div
            key={
              activeSection === "domains"
                ? item.domainId
                : item.categoryId
            }
            className="flex justify-between items-center py-2 px-1 border-b border-gray-100 hover:bg-gray-50 transition rounded-md"
          >
            <div>
              <p className="text-base text-gray-900 font-medium">
                {activeSection === "domains"
                  ? item.domainName
                  : item.categoryName}
              </p>

              {activeSection === "categories" && (
                <p className="text-sm text-gray-500">
                  {
                    domains.find(
                      (d) => d.domainId === item.domainId
                    )?.domainName
                  }
                </p>
              )}
            </div>

            <div className="flex items-center gap-3">
              <button
                onClick={() => handleEdit(item)}
                className="p-2 rounded-full hover:bg-gray-100 transition"
                title="Edit"
              >
                <PencilSquareIcon className="w-5 h-5 text-gray-700" />
              </button>

              <button
                onClick={() =>
                  handleDelete(
                    activeSection === "domains"
                      ? item.domainId
                      : item.categoryId
                  )
                }
                className="p-2 rounded-full hover:bg-red-50 transition"
                title="Delete"
              >
                <TrashIcon className="w-5 h-5 text-red-600" />
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
