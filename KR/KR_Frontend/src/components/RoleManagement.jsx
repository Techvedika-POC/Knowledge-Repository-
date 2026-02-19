import React, { useEffect, useState, useMemo } from "react";
import { toast } from "react-hot-toast";
import api from "../api";
import debounce from "lodash.debounce";

import {
  PencilSquareIcon,
  TrashIcon,
  PlusIcon,
  XMarkIcon,
  DocumentCheckIcon,
  Cog6ToothIcon,
  ArrowLeftIcon,
  ArrowRightIcon,
} from "@heroicons/react/24/outline";

export default function RoleManagement() {
  const [roles, setRoles] = useState([]);
  const [users, setUsers] = useState([]);
  const [searchUser, setSearchUser] = useState("");
  const [filteredUsers, setFilteredUsers] = useState([]);
  const [selectedUser, setSelectedUser] = useState("");
  const [userRoles, setUserRoles] = useState([]);
  const [form, setForm] = useState({ id: "", roleName: "", description: "" });
  const [loading, setLoading] = useState(false);
  const [auditLog, setAuditLog] = useState([]);
  const emptyForm = { id: "", roleName: "", description: "" };
  const [showRoleForm, setShowRoleForm] = useState(false);
  const usersPerPage = 6;
  const [page, setPage] = useState(1);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [roleRes, userRes] = await Promise.all([
        api.get("/Roles"),
        api.get("/Users"),
      ]);
      setRoles(roleRes.data);
      setUsers(userRes.data);
      setFilteredUsers(userRes.data);
      setPage(1);

    } catch {
      toast.error("Failed to load roles or users.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const fetchUserRoles = async (userId) => {
    if (!userId) return;
    try {
      const res = await api.get(`/UserRole/user/${userId}`);
      setUserRoles(res.data);
    } catch {
      toast.error("Failed to fetch user roles.");
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.roleName.trim()) return toast.error("Role name is required.");
    try {
      if (form.id) {
        await api.put(`/Roles/${form.id}`, {
          roleName: form.roleName.trim(),
          description: form.description,
        });
        toast.success("Role updated.");
      } else {
        await api.post("/Roles", {
          roleName: form.roleName.trim(),
          description: form.description,
        });
        toast.success("Role created.");
      }
      setForm(emptyForm);
      fetchData();
    } catch {
      toast.error("Failed to save role.");
    }
  };

  const handleEdit = (role) => {
    setForm({
      id: role.roleId,
      roleName: role.roleName,
      description: role.description || "",
    });
    setShowRoleForm(true); 
  };

  const handleDelete = async (roleId) => {
    if (!window.confirm("Delete this role?")) return;
    try {
      await api.delete(`/Roles/${roleId}`);
      toast.success("Role deleted.");
      fetchData();
    } catch {
      toast.error("Failed to delete role.");
    }
  };

  const toggleUserRole = async (roleId, assigned) => {
    if (!selectedUser) return toast.error("Select a user first.");
    try {
      const role = roles.find((r) => r.roleId === roleId);
      const user = users.find((u) => u.userId === selectedUser);

      if (assigned) {
        await api.delete(`/UserRole/remove`, {
          data: { userId: selectedUser, roleId },
        });
        addAuditLog(`${user.name} lost role "${role.roleName}".`);
      } else {
        await api.post(`/UserRole/assign`, { userId: selectedUser, roleId });
        addAuditLog(`${user.name} gained role "${role.roleName}".`);
      }

      fetchUserRoles(selectedUser);
    } catch {
      toast.error("Action failed.");
    }
  };

  const addAuditLog = (msg) =>
    setAuditLog((prev) => [
      { id: Date.now(), message: msg, timestamp: new Date().toLocaleString() },
      ...prev.slice(0, 19),
    ]);

  const handleSearch = useMemo(
    () =>
      debounce((q) => {
        if (!q.trim()) return setFilteredUsers(users);
        const s = q.toLowerCase();
        setFilteredUsers(
          users.filter(
            (u) =>
              u.name.toLowerCase().includes(s) ||
              u.email.toLowerCase().includes(s)
          )
        );
      }, 250),
    [users]
  );
  useEffect(() => {
    if (!searchUser.trim()) return; 
    handleSearch(searchUser);
    setPage(1);
  }, [searchUser, handleSearch]);


  const totalPages = Math.max(
    1,
    Math.ceil(filteredUsers.length / usersPerPage)
  );

  const paginatedUsers = filteredUsers.slice(
    (page - 1) * usersPerPage,
    page * usersPerPage
  );

  return (
    <div className="max-w-4xl mx-auto py-6 space-y-10">
      <section className="space-y-4">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-semibold text-gray-900 tracking-tight">
            Role Management
          </h1>
          <button
            onClick={() => {
              setForm(emptyForm);       
              setShowRoleForm(true);    
            }}
            className="flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium
                 bg-green-600 text-white hover:bg-green-700"
          >
            <PlusIcon className="w-4 h-4" />
            Add Role
          </button>
        </div>
        {showRoleForm && (
          <form
            onSubmit={handleSubmit}
            className="flex items-center gap-4 flex-wrap border rounded-md p-4 bg-gray-50"
          >
            <input
              placeholder="Role name"
              value={form.roleName}
              onChange={(e) =>
                setForm({ ...form, roleName: e.target.value })
              }
              className="border rounded-md p-2 w-64"
              required
            />

            <input
              placeholder="Description"
              value={form.description}
              onChange={(e) =>
                setForm({ ...form, description: e.target.value })
              }
              className="border rounded-md p-2 w-64"
            />
            <button type="submit" title="Save">
              <DocumentCheckIcon className="w-6 h-6 text-green-600 hover:text-green-700" />
            </button>
            <button
              type="button"
              title="Close"
              onClick={() => {
                setForm(emptyForm);
                setShowRoleForm(false);  
              }}
            >
              <XMarkIcon className="w-6 h-6 text-gray-600 hover:text-gray-800" />
            </button>
          </form>
        )}
        {roles.map((role) => (
          <div
            key={role.roleId}
            className="flex justify-between items-center py-2 border-b border-gray-200"
          >
            <div>
              <p className="font-medium text-gray-900">{role.roleName}</p>
              <p className="text-sm text-gray-600">
                {role.description || "No description"}
              </p>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => handleEdit(role)} 
                title="Edit"
              >
                <PencilSquareIcon className="w-5 h-5 text-gray-700 hover:text-black" />
              </button>
              <button
                onClick={() => handleDelete(role.roleId)}
                title="Delete"
              >
                <TrashIcon className="w-5 h-5 text-red-600 hover:text-red-800" />
              </button>
            </div>
          </div>
        ))}
      </section>
      <section className="space-y-3">
        <h2 className="text-xl font-semibold text-gray-800">Users</h2>

        <div className="flex gap-3 items-center">
          <input
            placeholder="Search users..."
            value={searchUser}
            onChange={(e) => setSearchUser(e.target.value)}
            className="border rounded-md p-2 w-64"
          />
          <button title="Clear" onClick={() => setSearchUser("")}>
            <XMarkIcon className="w-5 h-5 text-gray-600 hover:text-gray-900" />
          </button>
        </div>

        <div className="space-y-2">
          {paginatedUsers.map((user) => (
            <div
              key={user.userId}
              className="flex justify-between items-center py-2 border-b border-gray-200"
            >
              <span>
                {user.name} <span className="text-gray-500">({user.email})</span>
              </span>

              <button
                onClick={() => {
                  setSelectedUser(user.userId);
                  fetchUserRoles(user.userId);
                }}
                title="Manage roles"
              >
                <Cog6ToothIcon className="w-5 h-5 text-blue-600 hover:text-blue-800" />
              </button>
            </div>
          ))}
        </div>

        {/* Pagination */}
        <div className="flex gap-4 items-center mt-3">
          <button
            disabled={page === 1}
            onClick={() => setPage(page - 1)}
            title="Previous"
          >
            <ArrowLeftIcon
              className={`w-5 h-5 ${page === 1 ? "text-gray-300" : "text-gray-700 hover:text-black"
                }`}
            />
          </button>

          <span className="text-gray-700">
            Page {page} / {totalPages}
          </span>

          <button
            disabled={page === totalPages}
            onClick={() => setPage(page + 1)}
            title="Next"
          >
            <ArrowRightIcon
              className={`w-5 h-5 ${page === totalPages
                  ? "text-gray-300"
                  : "text-gray-700 hover:text-black"
                }`}
            />
          </button>
        </div>
      </section>
      {selectedUser && (
        <>
          <hr className="border-gray-300" />
          <section className="space-y-4">
            <h2 className="text-xl font-semibold text-gray-800">
              Manage Roles for{" "}
              {users.find((u) => u.userId === selectedUser)?.name}
            </h2>

            <div className="space-y-2">
              {roles.map((role) => {
                const assigned = userRoles.some(
                  (ur) => ur.roleId === role.roleId
                );
                return (
                  <div
                    key={role.roleId}
                    className="flex justify-between items-center py-2 border-b border-gray-200"
                  >
                    <span className={assigned ? "font-medium text-green-700" : ""}>
                      {role.roleName}
                    </span>

                    <button
                      title={assigned ? "Remove role" : "Assign role"}
                      onClick={() => toggleUserRole(role.roleId, assigned)}
                    >
                      {assigned ? (
                        <XMarkIcon className="w-5 h-5 text-red-600 hover:text-red-800" />
                      ) : (
                        <PlusIcon className="w-5 h-5 text-blue-600 hover:text-blue-800" />
                      )}
                    </button>
                  </div>
                );
              })}
            </div>
          </section>
        </>
      )}
      <hr className="border-gray-300" />

      <section>
        <h2 className="text-xl font-semibold text-gray-800">Audit Log</h2>
        <ul className="text-gray-700 text-sm mt-3 space-y-1 max-h-48 overflow-y-auto">
          {auditLog.length === 0 ? (
            <li>No recent actions.</li>
          ) : (
            auditLog.map((log) => (
              <li key={log.id}>
                {log.timestamp} — {log.message}
              </li>
            ))
          )}
        </ul>
      </section>
    </div>
  );
}
