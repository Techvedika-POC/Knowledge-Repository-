import React, { useState, useEffect } from "react";
import { toast } from "react-hot-toast";
import api from "../api";

/**
 * Domain & Category Management
 * Admin can create, edit, and delete domains and categories.
 * This uses two sections in one file with simple toggle.
 */

export default function DomainCategoryManagement() {
  const [activeSection, setActiveSection] = useState("domains");

  const [domains, setDomains] = useState([]);
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({ id: "", name: "", domainId: "" });
  const [loading, setLoading] = useState(false);

  const emptyForm = { id: "", name: "", domainId: "" };

  /** Fetch all domains and categories */
  const fetchData = async () => {
    setLoading(true);
    try {
      const domainRes = await api.get("/Domains");
      const categoryRes = await api.get("/Categories");
      setDomains(domainRes.data);
      setCategories(categoryRes.data);
    } catch {
      toast.error("Failed to load data.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  /** Save (add/update) handler */
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.name.trim()) {
      toast.error("Name is required.");
      return;
    }

    try {
      if (activeSection === "domains") {
        const payload = { domainName: form.name.trim() };

        if (form.id) {
          await api.put(`/Domains/${form.id}`, payload);
          toast.success("Domain updated successfully.");
        } else {
          await api.post("/Domains", payload);
          toast.success("Domain added successfully.");
        }
      } else {
        if (!form.domainId) {
          toast.error("Please select a domain for this category.");
          return;
        }

        const payload = {
          categoryName: form.name.trim(),
          domainId: form.domainId,
        };

        if (form.id) {
          await api.put(`/Categories/${form.id}`, payload);
          toast.success("Category updated successfully.");
        } else {
          await api.post("/Categories", payload);
          toast.success("Category added successfully.");
        }
      }

      setForm(emptyForm);
      fetchData();
    } catch {
      toast.error("Failed to save changes.");
    }
  };

  const handleEdit = (item) => {
    setForm({
      id: activeSection === "domains" ? item.domainId : item.categoryId,
      name:
        activeSection === "domains" ? item.domainName : item.categoryName,
      domainId: item.domainId || "",
    });
  };

  const handleDelete = async (id) => {
    try {
      if (activeSection === "domains") {
        await api.delete(`/Domains/${id}`);
        toast.success("Domain deleted.");
      } else {
        await api.delete(`/Categories/${id}`);
        toast.success("Category deleted.");
      }
      fetchData();
    } catch {
      toast.error("Delete failed.");
    }
  };

  return (
    <div className="bg-white p-8 rounded-xl shadow-md border border-gray-200">
      <h2 className="text-xl font-semibold mb-4">
        Domain & Category Management
      </h2>

      {/* Section Toggle */}
      <div className="flex gap-4 mb-6">
        <button
          onClick={() => {
            setActiveSection("domains");
            setForm(emptyForm);
          }}
          className={`px-4 py-2 rounded-md ${
            activeSection === "domains"
              ? "bg-blue-200 text-blue-900"
              : "bg-gray-100 hover:bg-gray-200"
          }`}
        >
          Domains
        </button>
        <button
          onClick={() => {
            setActiveSection("categories");
            setForm(emptyForm);
          }}
          className={`px-4 py-2 rounded-md ${
            activeSection === "categories"
              ? "bg-blue-200 text-blue-900"
              : "bg-gray-100 hover:bg-gray-200"
          }`}
        >
          Categories
        </button>
      </div>

      {/* Form */}
      <form
        onSubmit={handleSubmit}
        className="flex flex-wrap gap-4 items-end mb-6 border p-4 rounded-lg"
      >
        {activeSection === "categories" && (
          <div>
            <label className="block text-sm font-medium mb-1">Domain</label>
            <select
              name="domainId"
              value={form.domainId}
              onChange={handleChange}
              className="border p-2 rounded-md w-64"
            >
              <option value="">Select Domain</option>
              {domains.map((d) => (
                <option key={d.domainId} value={d.domainId}>
                  {d.domainName}
                </option>
              ))}
            </select>
          </div>
        )}

        <div>
          <label className="block text-sm font-medium mb-1">
            {activeSection === "domains" ? "Domain Name" : "Category Name"}
          </label>
          <input
            name="name"
            value={form.name}
            onChange={handleChange}
            placeholder={`Enter ${
              activeSection === "domains" ? "domain" : "category"
            } name`}
            className="border p-2 rounded-md w-64"
          />
        </div>

        <button
          type="submit"
          className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
        >
          {form.id ? "Update" : "Add"}
        </button>
      </form>

      {/* Table */}
      <div>
        {loading ? (
          <p>Loading...</p>
        ) : (
          <table className="w-full border-collapse text-left text-sm">
            <thead>
              <tr className="bg-blue-50 text-blue-900 uppercase">
                <th className="border px-3 py-2">Name</th>
                {activeSection === "categories" && (
                  <th className="border px-3 py-2">Domain</th>
                )}
                <th className="border px-3 py-2 text-center">Actions</th>
              </tr>
            </thead>
            <tbody>
              {(activeSection === "domains" ? domains : categories).map(
                (item) => (
                  <tr
                    key={
                      activeSection === "domains"
                        ? item.domainId
                        : item.categoryId
                    }
                    className="hover:bg-blue-50"
                  >
                    <td className="border px-3 py-2">
                      {activeSection === "domains"
                        ? item.domainName
                        : item.categoryName}
                    </td>

                    {activeSection === "categories" && (
                      <td className="border px-3 py-2">
                        {domains.find(
                          (d) => d.domainId === item.domainId
                        )?.domainName || "—"}
                      </td>
                    )}

                    <td className="border px-3 py-2 text-center space-x-2">
                      <button
                        onClick={() => handleEdit(item)}
                        className="px-3 py-1 bg-yellow-200 text-sm rounded hover:bg-yellow-300"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() =>
                          handleDelete(
                            activeSection === "domains"
                              ? item.domainId
                              : item.categoryId
                          )
                        }
                        className="px-3 py-1 bg-red-200 text-sm rounded hover:bg-red-300"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                )
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
