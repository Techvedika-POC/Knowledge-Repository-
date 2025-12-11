import React, { useEffect, useState, useMemo } from "react";
import { toast } from "react-hot-toast";
import api from "../api";
import debounce from "lodash.debounce";

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
  const [roleFilter, setRoleFilter] = useState("");

  const emptyForm = { id: "", roleName: "", description: "" };
  const usersPerPage = 5;
  const [page, setPage] = useState(1);
  const fetchData = async () => {
    setLoading(true);
    try {
      const [roleRes, userRes] = await Promise.all([api.get("/Roles"), api.get("/Users")]);
      setRoles(roleRes.data);
      setUsers(userRes.data);
      setFilteredUsers(userRes.data);
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
        await api.put(`/Roles/${form.id}`, { roleName: form.roleName.trim(), description: form.description });
        toast.success("Role updated successfully.");
      } else {
        await api.post("/Roles", { roleName: form.roleName.trim(), description: form.description });
        toast.success("Role created successfully.");
      }
      setForm(emptyForm);
      fetchData();
    } catch {
      toast.error("Failed to save role.");
    }
  };

  const handleEdit = (role) => setForm({ id: role.roleId, roleName: role.roleName, description: role.description || "" });

  const handleDelete = async (roleId) => {
    if (!window.confirm("Are you sure you want to delete this role?")) return;
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
        await api.delete(`/UserRole/remove`, { data: { userId: selectedUser, roleId } });
        toast.success(`Removed ${role.roleName} from ${user.name}.`);
        addAuditLog(`${user.name} had "${role.roleName}" removed.`);
      } else {
        await api.post(`/UserRole/assign`, { userId: selectedUser, roleId });
        toast.success(`Assigned ${role.roleName} to ${user.name}.`);
        addAuditLog(`${user.name} was assigned "${role.roleName}".`);
      }
      fetchUserRoles(selectedUser);
    } catch {
      toast.error("Action failed.");
    }
  };

  const addAuditLog = (message) => setAuditLog((prev) => [
    { id: Date.now(), message, timestamp: new Date().toLocaleString() },
    ...prev.slice(0, 19),
  ]);
  const handleSearch = useMemo(
    () =>
      debounce((query) => {
        if (!query.trim()) return setFilteredUsers(users);
        const q = query.toLowerCase();
        setFilteredUsers(users.filter((u) => u.name.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)));
      }, 300),
    [users]
  );

  useEffect(() => handleSearch(searchUser), [searchUser, handleSearch]);

  const resetUserSearch = () => {
    setSearchUser("");
    setFilteredUsers(users);
  };

  const resetRoleFilter = () => setRoleFilter("");
  const totalPages = Math.ceil(filteredUsers.length / usersPerPage);
  const paginatedUsers = filteredUsers.slice((page - 1) * usersPerPage, page * usersPerPage);

  const visibleRoles = roleFilter
    ? roles.filter((r) => r.roleName.toLowerCase().includes(roleFilter.toLowerCase()))
    : roles;

  return (
    <div className="bg-white p-8 rounded-xl shadow-md border border-gray-200 space-y-6">
      <h2 className="text-2xl font-bold text-gray-800">Role Management</h2>

      {/* Role Form */}
      <div className="p-4 rounded-lg border bg-gray-50 shadow-sm">
        <h3 className="text-lg font-semibold mb-3">Create / Edit Role</h3>
        <form onSubmit={handleSubmit} className="flex flex-wrap gap-4 items-end">
          <div>
            <input
              placeholder="Role Name"
              value={form.roleName}
              onChange={(e) => setForm({ ...form, roleName: e.target.value })}
              className="border p-2 rounded-md w-64"
            />
          </div>
          <div>
            <input
              placeholder="Description"
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
              className="border p-2 rounded-md w-64"
            />
          </div>
          <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
            {form.id ? "Update" : "Add"}
          </button>
          <button type="button" onClick={() => setForm(emptyForm)} className="px-4 py-2 rounded-md border hover:bg-gray-100">
            Reset
          </button>
        </form>
      </div>

      {/* Role Table with Filter */}
      <div className="p-4 rounded-lg border bg-gray-50 shadow-sm">
        <div className="flex items-center justify-between mb-2">
          <h3 className="text-lg font-semibold">Roles</h3>
          <div className="flex gap-2">
            <input
              placeholder="Search roles..."
              value={roleFilter}
              onChange={(e) => setRoleFilter(e.target.value)}
              className="border p-2 rounded-md"
            />
            <button onClick={resetRoleFilter} className="px-3 py-1 border rounded hover:bg-gray-100">Reset</button>
          </div>
        </div>
        <table className="w-full border-collapse text-sm text-left">
          <thead>
            <tr className="bg-blue-50 text-blue-900 uppercase">
              <th className="border px-3 py-2">Role Name</th>
              <th className="border px-3 py-2">Description</th>
              <th className="border px-3 py-2 text-center">Actions</th>
            </tr>
          </thead>
          <tbody>
            {visibleRoles.map((r) => (
              <tr key={r.roleId} className="hover:bg-blue-50">
                <td className="border px-3 py-2">{r.roleName}</td>
                <td className="border px-3 py-2">{r.description || "—"}</td>
                <td className="border px-3 py-2 text-center space-x-2">
                  <button onClick={() => handleEdit(r)} className="px-3 py-1 bg-yellow-200 rounded hover:bg-yellow-300">Edit</button>
                  <button onClick={() => handleDelete(r.roleId)} className="px-3 py-1 bg-red-200 rounded hover:bg-red-300">Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Assign Roles to Users */}
      <div className="p-4 rounded-lg border bg-gray-50 shadow-sm">
        <h3 className="text-lg font-semibold mb-4">Assign Roles to Users</h3>

        {/* Search Users */}
        <div className="flex flex-wrap gap-2 items-center mb-3">
          <input
            placeholder="Search users..."
            value={searchUser}
            onChange={(e) => setSearchUser(e.target.value)}
            className="border p-2 rounded-md w-64"
          />
          <button
            onClick={resetUserSearch}
            className="px-3 py-1 border rounded hover:bg-gray-100"
          >
            Reset
          </button>
        </div>

        {/* Users List with Pagination */}
        <div className="mb-3">
          {paginatedUsers.length === 0 ? (
            <p className="text-sm text-gray-500">No users found.</p>
          ) : (
            <div className="space-y-1">
              {paginatedUsers.map((u) => (
                <div
                  key={u.userId}
                  className="flex justify-between items-center border rounded p-2 bg-white hover:bg-gray-50"
                >
                  <span>{u.name} ({u.email})</span>
                  <button
                    onClick={() => { setSelectedUser(u.userId); fetchUserRoles(u.userId); }}
                    className="px-3 py-1 bg-blue-200 hover:bg-blue-300 rounded text-sm"
                  >
                    Manage Roles
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Pagination Controls */}
        {totalPages > 1 && (
          <div className="flex gap-2 mb-4">
            {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
              <button
                key={p}
                onClick={() => setPage(p)}
                className={`px-3 py-1 rounded border ${p === page ? "bg-blue-200" : "bg-gray-100 hover:bg-gray-200"}`}
              >
                {p}
              </button>
            ))}
          </div>
        )}

        {/* Assigned Roles Section */}
        {selectedUser && (
          <div className="mt-4 border rounded-lg p-4 bg-white shadow-sm">
            <h4 className="font-medium mb-3 text-gray-800">
              Manage Roles for {users.find(u => u.userId === selectedUser)?.name}
            </h4>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
              {roles.map((role) => {
                const assigned = userRoles.some((ur) => ur.roleId === role.roleId);
                return (
                  <div
                    key={role.roleId}
                    className={`flex justify-between items-center border p-2 rounded-md ${assigned ? "bg-green-50 border-green-300" : "bg-gray-50"}`}
                  >
                    <span>{role.roleName}</span>
                    <button
                      onClick={() => toggleUserRole(role.roleId, assigned)}
                      className={`px-3 py-1 text-sm rounded ${assigned ? "bg-red-200 hover:bg-red-300" : "bg-blue-200 hover:bg-blue-300"}`}
                    >
                      {assigned ? "Remove" : "Assign"}
                    </button>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>


      {/* Audit Log */}
      <div className="p-4 rounded-lg border bg-gray-50 shadow-sm">
        <h3 className="text-lg font-semibold mb-2">Audit Log</h3>
        <ul className="max-h-48 overflow-y-auto text-sm text-gray-600 space-y-1">
          {auditLog.length === 0 ? <li>No recent actions.</li> :
            auditLog.map((log) => <li key={log.id}>{log.timestamp} — {log.message}</li>)}
        </ul>
      </div>
    </div>
  );
}
