import React, { useState, useEffect, useMemo } from "react";
import { toast } from "react-hot-toast";
import api from "../api"; // Axios instance
import debounce from "lodash.debounce";

/**
 * IdeathonAdminDashboard
 * - Select an Ideathon event
 * - Teams tab: shows teams list, expand to show members, lead, assigned mentors; add/remove mentors; schedule presentation (datetime input)
 * - Jury tab: add/remove jury members (uses EventJury backend)
 * - Presentations tab: shows scheduled presentations + remove
 * - Audit tab: simple in-memory audit log
 *
 * Notes:
 * - Backend response shapes: many endpoints return { success, data } — this code uses `.data.data` for those,
 *   and `.data` for endpoints returning raw objects/arrays (Events/type endpoint).
 */

export default function IdeathonAdminDashboard() {
  const [events, setEvents] = useState([]);
  const [selectedEventId, setSelectedEventId] = useState(null);

  const [eventData, setEventData] = useState({});
  const [teams, setTeams] = useState([]);
  const [mentors, setMentors] = useState([]);
  const [juryMembers, setJuryMembers] = useState([]);
  const [juryUsers, setJuryUsers] = useState([]);
  const [presentations, setPresentations] = useState([]);
  const [auditLog, setAuditLog] = useState([]);

  const [activeTab, setActiveTab] = useState("teams");
  const [searchTeam, setSearchTeam] = useState("");
  const [filteredTeams, setFilteredTeams] = useState([]);
  const [page, setPage] = useState(1);
  const teamsPerPage = 5;

  // UI state for expanded team details and per-team scheduling form
  const [expandedTeamId, setExpandedTeamId] = useState(null);
  const [scheduleInputs, setScheduleInputs] = useState({}); // { [teamId]: "2025-11-09T15:00" }
  const [mentorSelect, setMentorSelect] = useState({}); // { [teamId]: selectedMentorUserId }
  const [selectedJuryToAdd, setSelectedJuryToAdd] = useState("");

  // -------------------- Fetch Ideathon-type events --------------------
  const fetchEvents = async () => {
    try {
      // Backend appears to return plain array for type endpoint (based on sample). If it returns { data }, adjust here.
      const res = await api.get("/Events/type/Ideathon");
      setEvents(Array.isArray(res.data) ? res.data : res.data || []);
    } catch (err) {
      console.error(err);
      toast.error("Failed to fetch Ideathon events.");
    }
  };

  // -------------------- Fetch event-specific data --------------------
  const fetchEventData = async (eventId) => {
    if (!eventId) return;
    try {
      const [
        eventRes,
        teamsRes,
        mentorsRes,
        juryRes,
        presentationsRes,
        allJuryUsersRes,
      ] = await Promise.all([
        api.get(`/Events/${eventId}`), // eventRes.data
        api.get(`/Ideathon/${eventId}/teams`), // teamsRes.data.data
        api.get(`/Ideathon/${eventId}/mentors`), // mentorsRes.data.data
        api.get(`/Ideathon/${eventId}/jury`), // juryRes.data.data
        api.get(`/Ideathon/${eventId}/presentations`), // presentationsRes.data.data
        api.get(`/Users?role=jurymember`), // allJuryUsersRes.data
      ]);

      setEventData(eventRes.data || {});
      setTeams((teamsRes.data && teamsRes.data.data) || []);
      setFilteredTeams((teamsRes.data && teamsRes.data.data) || []);
      setMentors((mentorsRes.data && mentorsRes.data.data) || []);
      setJuryMembers((juryRes.data && juryRes.data.data) || []);
      setJuryUsers(allJuryUsersRes.data || []);
      setPresentations((presentationsRes.data && presentationsRes.data.data) || []);

      // Initialize per-team selects/inputs
      const initialScheduleInputs = {};
      const initialMentorSelect = {};
      ((teamsRes.data && teamsRes.data.data) || []).forEach((t) => {
        initialScheduleInputs[t.teamId] = ""; // empty
        initialMentorSelect[t.teamId] = "";
      });
      setScheduleInputs(initialScheduleInputs);
      setMentorSelect(initialMentorSelect);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load event data.");
    }
  };

  useEffect(() => {
    fetchEvents();
  }, []);

  useEffect(() => {
    if (selectedEventId) fetchEventData(selectedEventId);
    else {
      // reset when no event selected
      setEventData({});
      setTeams([]);
      setMentors([]);
      setJuryMembers([]);
      setJuryUsers([]);
      setPresentations([]);
      setFilteredTeams([]);
      setExpandedTeamId(null);
    }
  }, [selectedEventId]);

  // -------------------- Debounced search --------------------
  const handleSearch = useMemo(
    () =>
      debounce((query) => {
        if (!query.trim()) return setFilteredTeams(teams);

        const q = query.toLowerCase();

        setFilteredTeams(
          teams.filter((t) => {
            const teamName = (t.teamName || "").toLowerCase();
            const members = t.teamMembers || [];
            const matchMembers = members.some((m) => (m.name || "").toLowerCase().includes(q));
            return teamName.includes(q) || matchMembers;
          })
        );

        setPage(1);
      }, 300),
    [teams]
  );

  useEffect(() => {
    handleSearch(searchTeam);
  }, [searchTeam, handleSearch]);

  const totalPages = Math.ceil((filteredTeams.length || 0) / teamsPerPage);
  const paginatedTeams = (filteredTeams || []).slice((page - 1) * teamsPerPage, page * teamsPerPage);

  // -------------------- Audit Log --------------------
  const addAuditLog = (message) => {
    setAuditLog((prev) => [{ id: Date.now(), message, timestamp: new Date().toLocaleString() }, ...prev.slice(0, 19)]);
  };

  // -------------------- Mentor Management --------------------
  const assignMentor = async (teamId, mentorId) => {
    try {
      if (!selectedEventId) throw new Error("No event selected");
      // API expects AssignMentorDto -> MentorIds
      await api.post(`/Ideathon/${selectedEventId}/teams/${teamId}/assign-mentor`, {
        MentorIds: [mentorId],
      });
      toast.success("Mentor assigned.");
      addAuditLog(`Assigned mentor ${mentorId} to team ${teamId}.`);
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to assign mentor.");
    }
  };

  const removeMentor = async (teamId, mentorId) => {
    try {
      if (!selectedEventId) throw new Error("No event selected");
      // backend route: /Ideathon/{eventId}/teams/{teamId}/remove-mentor/{mentorId}
      await api.delete(`/Ideathon/${selectedEventId}/teams/${teamId}/remove-mentor/${mentorId}`);
      toast.success("Mentor removed.");
      addAuditLog(`Removed mentor ${mentorId} from team ${teamId}.`);
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to remove mentor.");
    }
  };

  // -------------------- Jury Management --------------------
  const addJuryMember = async () => {
    if (!selectedJuryToAdd) {
      toast.error("Select a jury member first.");
      return;
    }
    try {
      await api.post(`/Ideathon/${selectedEventId}/jury`, { UserIds: [selectedJuryToAdd] });
      toast.success("Jury member added.");
      addAuditLog(`Added jury member ${selectedJuryToAdd}.`);
      setSelectedJuryToAdd("");
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to add jury member.");
    }
  };

  const removeJuryMember = async (userId) => {
    try {
      await api.delete(`/Ideathon/${selectedEventId}/jury/${userId}`);
      toast.success("Jury member removed.");
      addAuditLog(`Removed jury member ${userId}.`);
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to remove jury member.");
    }
  };

  // -------------------- Presentation Management --------------------
  const schedulePresentation = async (teamId) => {
    try {
      const inputValue = scheduleInputs[teamId];
      if (!inputValue) {
        toast.error("Please pick a date & time.");
        return;
      }
      // Convert to ISO string so backend DateTime parses correctly
      const dt = new Date(inputValue);
      if (Number.isNaN(dt.getTime())) {
        toast.error("Invalid date/time format.");
        return;
      }
      await api.post(`/Ideathon/${selectedEventId}/teams/${teamId}/schedule-presentation`, {
        PresentationDate: dt.toISOString(),
      });
      toast.success("Presentation scheduled.");
      addAuditLog(`Scheduled presentation for team ${teamId} at ${dt.toISOString()}.`);
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to schedule presentation.");
    }
  };

  const removePresentation = async (presentationId) => {
    try {
      await api.delete(`/Presentations/${presentationId}`);
      toast.success("Presentation removed.");
      addAuditLog(`Presentation ${presentationId} removed.`);
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to remove presentation.");
    }
  };

  // -------------------- Helpers --------------------
  const inferTeamLead = (team) => {
    // Try common conventions
    const members = team.teamMembers || [];
    if (!members.length) return null;

    const byFlags = members.find((m) => m.isTeamLead || m.isLead || m.isLeader || m.isCaptain);
    if (byFlags) return byFlags;

    // If team has teamLeadId property
    if (team.teamLeadId) {
      const lead = members.find((m) => String(m.userId) === String(team.teamLeadId));
      if (lead) return lead;
    }

    // fallback: first member
    return members[0];
  };

  // -------------------- Render --------------------
  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold mb-6">Ideathon Admin Dashboard</h1>

      {/* Event Selector */}
      <div className="mb-6 flex items-center gap-4">
        <label className="mr-2 font-semibold">Select Ideathon Event:</label>
        <select
          value={selectedEventId || ""}
          onChange={(e) => setSelectedEventId(e.target.value || null)}
          className="border p-2 rounded-md"
        >
          <option value="">-- Select an Event --</option>
          {events.map((ev) => (
            <option key={ev.eventId} value={ev.eventId}>
              {ev.title}
            </option>
          ))}
        </select>

        <div className="ml-auto text-sm text-gray-600">
          {selectedEventId ? `Selected: ${eventData.title || ""}` : "No event selected"}
        </div>
      </div>

      {selectedEventId && (
        <>
          <h2 className="text-2xl font-semibold mb-2">{eventData.title}</h2>
          <p className="mb-6 text-gray-600">{eventData.description}</p>

          {/* Tabs */}
          <div className="flex gap-4 mb-6 border-b">
            {["teams", "jury", "presentations", "audit"].map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`px-4 py-2 rounded-t-lg ${activeTab === tab ? "bg-white border-t border-l border-r" : "bg-gray-100"}`}
              >
                {tab.charAt(0).toUpperCase() + tab.slice(1)}
              </button>
            ))}
          </div>

          {/* TEAMS */}
          {activeTab === "teams" && (
            <div>
              <div className="flex items-center gap-4 mb-4">
                <input
                  type="text"
                  placeholder="Search teams or member..."
                  value={searchTeam}
                  onChange={(e) => setSearchTeam(e.target.value)}
                  className="border p-2 rounded-md w-64"
                />
                <div className="text-sm text-gray-500">{filteredTeams.length} teams</div>
              </div>

              <div className="space-y-3">
                {paginatedTeams.map((team) => {
                  const lead = inferTeamLead(team);
                  const assignedMentors = team.mentors || []; // mentors returned with team
                  return (
                    <div key={team.teamId} className="border rounded-md p-3 bg-gray-50">
                      <div className="flex justify-between items-center">
                        <div>
                          <strong className="text-lg">{team.teamName}</strong>
                          <div className="text-sm text-gray-600">Team ID: {team.teamId}</div>
                        </div>

                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => setExpandedTeamId((id) => (id === team.teamId ? null : team.teamId))}
                            className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300 text-sm"
                          >
                            {expandedTeamId === team.teamId ? "Collapse" : "Details"}
                          </button>
                        </div>
                      </div>

                      {expandedTeamId === team.teamId && (
                        <div className="mt-3 grid grid-cols-1 md:grid-cols-3 gap-4">
                          {/* Members */}
                          <div className="col-span-1 md:col-span-1 border rounded p-3 bg-white">
                            <h4 className="font-semibold mb-2">Members</h4>
                            {team.teamMembers && team.teamMembers.length ? (
                              <ul className="space-y-1 text-sm">
                                {team.teamMembers.map((m) => (
                                  <li key={m.userId} className={`flex justify-between ${lead && String(lead.userId) === String(m.userId) ? "font-semibold" : ""}`}>
                                    <div>
                                      {m.name || m.email || m.userId}
                                      {lead && String(lead.userId) === String(m.userId) && <span className="ml-2 text-xs text-blue-600">(Lead)</span>}
                                    </div>
                                    <div className="text-gray-500 text-xs">{m.email}</div>
                                  </li>
                                ))}
                              </ul>
                            ) : (
                              <div className="text-sm text-gray-500">No members registered.</div>
                            )}
                          </div>

                          {/* Mentors */}
                          <div className="col-span-1 md:col-span-1 border rounded p-3 bg-white">
                            <h4 className="font-semibold mb-2">Mentors</h4>

                            {assignedMentors && assignedMentors.length ? (
                              <ul className="space-y-2 mb-3">
                                {assignedMentors.map((am) => (
                                  <li key={am.mentorId} className="flex justify-between items-center">
                                    <div className="text-sm">
                                      {am.user?.name || am.user?.email || am.userId}
                                    </div>
                                    <div className="flex gap-2">
                                      <button
                                        onClick={() => removeMentor(team.teamId, am.userId)}
                                        className="px-2 py-1 rounded bg-red-200 hover:bg-red-300 text-xs"
                                      >
                                        Remove
                                      </button>
                                    </div>
                                  </li>
                                ))}
                              </ul>
                            ) : (
                              <div className="text-sm text-gray-500 mb-3">No mentors assigned</div>
                            )}

                            {/* Assign mentor */}
                            <div className="mt-2">
                              <select
                                value={mentorSelect[team.teamId] || ""}
                                onChange={(e) => setMentorSelect((s) => ({ ...s, [team.teamId]: e.target.value }))}
                                className="border p-2 rounded-md w-full mb-2 text-sm"
                              >
                                <option value="">Select mentor to assign</option>
                                {mentors
                                  .filter((m) => !assignedMentors.some((am) => String(am.userId) === String(m.userId)))
                                  .map((m) => (
                                    <option key={m.userId} value={m.userId}>
                                      {m.name || m.email}
                                    </option>
                                  ))}
                              </select>
                              <div className="flex gap-2">
                                <button
                                  onClick={() => {
                                    const sel = mentorSelect[team.teamId];
                                    if (!sel) return toast.error("Select a mentor first.");
                                    assignMentor(team.teamId, sel);
                                    // clear selection
                                    setMentorSelect((s) => ({ ...s, [team.teamId]: "" }));
                                  }}
                                  className="px-3 py-1 rounded bg-blue-600 text-white text-sm"
                                >
                                  Assign Mentor
                                </button>
                                <button
                                  onClick={async () => {
                                    // quick open schedule input focus - not necessary but helpful
                                    setScheduleInputs((s) => ({ ...s, [team.teamId]: s[team.teamId] || "" }));
                                    setExpandedTeamId(team.teamId);
                                  }}
                                  className="px-3 py-1 rounded bg-gray-200 text-sm"
                                >
                                  Focus Schedule
                                </button>
                              </div>
                            </div>
                          </div>

                          {/* Presentation scheduling */}
                          <div className="col-span-1 md:col-span-1 border rounded p-3 bg-white">
                            <h4 className="font-semibold mb-2">Presentation</h4>
                            {presentations.some((p) => String(p.teamId) === String(team.teamId)) ? (
                              <div className="text-sm mb-3">
                                {presentations
                                  .filter((p) => String(p.teamId) === String(team.teamId))
                                  .map((p) => (
                                    <div key={p.presentationId} className="flex justify-between items-center mb-2">
                                      <div>
                                        {p.teamName} —{" "}
                                        {p.presentationDate ? new Date(p.presentationDate).toLocaleString() : "Not scheduled"}
                                      </div>
                                      <button
                                        onClick={() => removePresentation(p.presentationId)}
                                        className="px-2 py-1 rounded bg-red-200 hover:bg-red-300 text-xs"
                                      >
                                        Remove
                                      </button>
                                    </div>
                                  ))}
                              </div>
                            ) : (
                              <div className="text-sm text-gray-500 mb-3">No presentation scheduled</div>
                            )}

                            <label className="block text-sm mb-1">Pick date & time</label>
                            <input
                              type="datetime-local"
                              value={scheduleInputs[team.teamId] || ""}
                              onChange={(e) => setScheduleInputs((s) => ({ ...s, [team.teamId]: e.target.value }))}
                              className="border p-2 rounded-md w-full mb-2 text-sm"
                            />
                            <div className="flex gap-2">
                              <button
                                onClick={() => schedulePresentation(team.teamId)}
                                className="px-3 py-1 rounded bg-green-600 text-white text-sm"
                              >
                                Schedule Presentation
                              </button>
                              <button
                                onClick={() => setScheduleInputs((s) => ({ ...s, [team.teamId]: "" }))}
                                className="px-3 py-1 rounded bg-gray-200 text-sm"
                              >
                                Clear
                              </button>
                            </div>
                          </div>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>

              {/* Pagination */}
              <div className="mt-4 flex items-center gap-3">
                <button
                  disabled={page <= 1}
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  className="px-3 py-1 rounded bg-gray-200"
                >
                  Prev
                </button>
                <div className="text-sm">
                  Page {page} / {totalPages || 1}
                </div>
                <button
                  disabled={page >= totalPages}
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  className="px-3 py-1 rounded bg-gray-200"
                >
                  Next
                </button>
              </div>
            </div>
          )}

          {/* JURY */}
          {activeTab === "jury" && (
            <div className="max-w-2xl">
              <div className="flex gap-3 items-center mb-4">
                <select
                  value={selectedJuryToAdd}
                  onChange={(e) => setSelectedJuryToAdd(e.target.value)}
                  className="border p-2 rounded-md w-72"
                >
                  <option value="">Select jury to add</option>
                  {juryUsers
                    .filter((u) => !juryMembers.some((j) => String(j.userId) === String(u.userId)))
                    .map((u) => (
                      <option key={u.userId} value={u.userId}>
                        {u.name} ({u.email})
                      </option>
                    ))}
                </select>
                <button onClick={addJuryMember} className="px-3 py-1 rounded bg-blue-600 text-white">
                  Add Jury
                </button>
                <div className="text-sm text-gray-500 ml-auto">{juryMembers.length} assigned</div>
              </div>

              <div className="border rounded-md p-3 bg-gray-50">
                {juryMembers.length ? (
                  <ul className="space-y-2">
                    {juryMembers.map((member) => (
                      <li key={member.userId} className="flex justify-between items-center border p-2 rounded bg-white">
                        <div>
                          <div className="font-medium">{member.name}</div>
                          <div className="text-sm text-gray-500">{member.email}</div>
                        </div>
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => removeJuryMember(member.userId)}
                            className="px-3 py-1 rounded bg-red-200 hover:bg-red-300 text-sm"
                          >
                            Remove
                          </button>
                        </div>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <div className="text-sm text-gray-500">No jury members assigned yet.</div>
                )}
              </div>
            </div>
          )}

          {/* PRESENTATIONS */}
          {activeTab === "presentations" && (
            <div>
              <div className="space-y-2 mb-4">
                {presentations.length ? (
                  presentations.map((p) => (
                    <div key={p.presentationId} className="flex justify-between items-center border p-3 rounded bg-white">
                      <div>
                        <div className="font-medium">{p.teamName}</div>
                        <div className="text-sm text-gray-600">
                          {p.presentationDate ? new Date(p.presentationDate).toLocaleString() : "Not scheduled"}
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <button onClick={() => removePresentation(p.presentationId)} className="px-3 py-1 rounded bg-red-200">
                          Remove
                        </button>
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="text-sm text-gray-500">No presentations scheduled yet.</div>
                )}
              </div>

              <div className="mt-4">
                <div className="text-sm text-gray-600 mb-2">Schedule a meeting for a team</div>
                <div className="flex flex-wrap gap-2">
                  {teams.map((t) => (
                    <div key={t.teamId} className="flex items-center gap-2">
                      <div className="text-sm">{t.teamName}</div>
                      <input
                        type="datetime-local"
                        value={scheduleInputs[t.teamId] || ""}
                        onChange={(e) => setScheduleInputs((s) => ({ ...s, [t.teamId]: e.target.value }))}
                        className="border p-1 rounded text-sm"
                      />
                      <button
                        onClick={() => schedulePresentation(t.teamId)}
                        className="px-2 py-1 rounded bg-green-600 text-white text-sm"
                      >
                        Schedule
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {/* AUDIT */}
          {activeTab === "audit" && (
            <div className="mt-6 border-t pt-4">
              <h3 className="font-semibold mb-2">Audit Log</h3>
              <ul className="max-h-60 overflow-y-auto text-sm space-y-1">
                {auditLog.length === 0 ? <li>No recent actions.</li> : auditLog.map((log) => <li key={log.id}>{log.timestamp} — {log.message}</li>)}
              </ul>
            </div>
          )}
        </>
      )}
    </div>
  );
}
