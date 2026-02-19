import React, { useState, useEffect, useMemo } from "react";
import { toast } from "react-hot-toast";
import api from "../api";
import debounce from "lodash.debounce";
import {
  ChevronDownIcon,
  ChevronUpIcon, TrashIcon, PlusIcon
} from "@heroicons/react/24/outline";

export default function IdeathonAdminDashboard() {
  const [events, setEvents] = useState([]);
  const [eventsByType, setEventsByType] = useState([]);
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
  const [expandedTeamId, setExpandedTeamId] = useState(null);
  const [scheduleInputs, setScheduleInputs] = useState({});
  const [mentorSelect, setMentorSelect] = useState({});
  const [selectedJuryToAdd, setSelectedJuryToAdd] = useState("");
  const [presentationTeamToSchedule, setPresentationTeamToSchedule] = useState("");
  const [presentationDatetime, setPresentationDatetime] = useState("");
  const safeVal = (v, fallback = "") => (v === null || v === undefined ? fallback : v);
  const formatDateLabel = (d) => {
    if (!d) return "";
    try {
      const dt = new Date(d);
      if (Number.isNaN(dt.getTime())) return d;
      return dt.toLocaleDateString();
    } catch {
      return d;
    }
  };

  const fetchGroupedEvents = async () => {
    try {
      const res = await api.get("/Events/grouped-by-type-month");
      setEventsByType(Array.isArray(res.data) ? res.data : res.data || []);
    } catch (err) {
      console.error(err);
      toast.error("Failed to fetch grouped events.");
      setEventsByType([]);
    }
  };
  const fetchEvents = async () => {
    try {
      const res = await api.get("/Events/type/Ideathon");
      setEvents(Array.isArray(res.data) ? res.data : res.data || []);
    } catch (err) {
      console.error(err);
      toast.error("Failed to fetch Ideathon events.");
    }
  };
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
        api.get(`/Events/${eventId}`),
        api.get(`/Ideathon/${eventId}/teams`),
        api.get(`/Ideathon/${eventId}/mentors`),
        api.get(`/Ideathon/${eventId}/jury`),
        api.get(`/Ideathon/${eventId}/presentations`),
        api.get(`/Users?role=jurymember`),
      ]);

      setEventData(eventRes.data || {});
      setTeams((teamsRes.data && teamsRes.data.data) || []);
      setFilteredTeams((teamsRes.data && teamsRes.data.data) || []);
      setMentors((mentorsRes.data && mentorsRes.data.data) || []);
      setJuryMembers((juryRes.data && juryRes.data.data) || []);
      setJuryUsers(allJuryUsersRes.data || []);
      setPresentations((presentationsRes.data && presentationsRes.data.data) || []);

      const initialScheduleInputs = {};
      const initialMentorSelect = {};
      ((teamsRes.data && teamsRes.data.data) || []).forEach((t) => {
        initialScheduleInputs[t.teamId] = "";
        initialMentorSelect[t.teamId] = "";
      });
      setScheduleInputs(initialScheduleInputs);
      setMentorSelect(initialMentorSelect);

      setPresentationTeamToSchedule(((teamsRes.data && teamsRes.data.data) || [])[0]?.teamId || "");
      setPresentationDatetime("");
    } catch (err) {
      console.error(err);
      toast.error("Failed to load event data.");
    }
  };
  useEffect(() => {
    fetchGroupedEvents();
    fetchEvents();
  }, []);
  useEffect(() => {
    if (selectedEventId) fetchEventData(selectedEventId);
    else {
      setEventData({});
      setTeams([]);
      setMentors([]);
      setJuryMembers([]);
      setJuryUsers([]);
      setPresentations([]);
      setFilteredTeams([]);
      setExpandedTeamId(null);
      setActiveTab("teams");
    }
  }, [selectedEventId]);
  const handleSearch = useMemo(
    () =>
      debounce((query) => {
        if (!query.trim()) return setFilteredTeams(teams);

        const q = query.toLowerCase();
        setFilteredTeams(
          teams.filter((t) => {
            const teamName = (t.teamName || "").toLowerCase();
            const members = t.teamMembers || [];
            const matchMembers = members.some((m) => {
              const name = (m.user?.name || "").toLowerCase();
              const email = (m.user?.email || "").toLowerCase();
              return name.includes(q) || email.includes(q);
            });
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
  const addAuditLog = (message) => {
    setAuditLog((prev) => [{ id: Date.now(), message, timestamp: new Date().toLocaleString() }, ...prev.slice(0, 19)]);
  };

  const assignMentor = async (teamId, mentorId) => {
    try {
      if (!selectedEventId) throw new Error("No event selected");
      await api.post(`/Ideathon/${selectedEventId}/teams/${teamId}/assign-mentor`, { MentorIds: [mentorId] });
      toast.success("Mentor assigned.");
      addAuditLog(`Assigned mentor ${mentorId} to team ${teamId}.`);
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to assign mentor.");
    }
  };

  const removeMentor = async (teamId, mentorRecordId) => {
    try {
      if (!selectedEventId) throw new Error("No event selected");
      await api.delete(`/Ideathon/${selectedEventId}/teams/${teamId}/remove-mentor/${mentorRecordId}`);
      toast.success("Mentor removed.");
      addAuditLog(`Removed mentor record ${mentorRecordId} from team ${teamId}.`);
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to remove mentor.");
    }
  };

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

  const schedulePresentation = async (teamId) => {
    try {
      const inputValue = scheduleInputs[teamId];
      if (!inputValue) { toast.error("Please pick a date & time."); return; }
      const dt = new Date(inputValue);
      if (Number.isNaN(dt.getTime())) { toast.error("Invalid date/time format."); return; }
      await api.post(`/Ideathon/${selectedEventId}/teams/${teamId}/schedule-presentation`, { PresentationDate: dt.toISOString() });
      toast.success("Presentation scheduled.");
      addAuditLog(`Scheduled presentation for team ${teamId} at ${dt.toISOString()}.`);
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to schedule presentation.");
    }
  };

  const schedulePresentationCompact = async () => {
    try {
      if (!presentationTeamToSchedule) { toast.error("Select a team to schedule."); return; }
      if (!presentationDatetime) { toast.error("Pick date & time."); return; }
      const dt = new Date(presentationDatetime);
      if (Number.isNaN(dt.getTime())) { toast.error("Invalid date/time format."); return; }
      await api.post(`/Ideathon/${selectedEventId}/teams/${presentationTeamToSchedule}/schedule-presentation`, { PresentationDate: dt.toISOString() });
      toast.success("Presentation scheduled.");
      addAuditLog(`Scheduled presentation for team ${presentationTeamToSchedule} at ${dt.toISOString()}.`);
      setPresentationDatetime("");
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to schedule presentation.");
    }
  };

  const removePresentation = async (presentationId) => {
    try {
      await api.delete(`/Ideathon/Presentations/${presentationId}`);
      toast.success("Presentation removed.");
      addAuditLog(`Presentation ${presentationId} removed.`);
      await fetchEventData(selectedEventId);
    } catch (err) {
      console.error(err);
      toast.error("Failed to remove presentation.");
    }
  };
  const inferTeamLead = (team) => {
    const members = team.teamMembers || [];
    if (!members.length) return null;
    const normalize = (m) => ({ userId: m.userId, role: m.role, name: m.user?.name || "", email: m.user?.email || "" });
    const normalized = members.map(normalize);
    const leader = normalized.find((m) => (m.role || "").toLowerCase() === "leader");
    if (leader) return leader;
    if (team.teamLeadId) {
      const match = normalized.find((m) => String(m.userId) === String(team.teamLeadId));
      if (match) return match;
    }
    return normalized[0];
  };
  const ideathonBlock = (eventsByType || []).find((b) => String(b.eventType || "").toLowerCase() === "ideathon");

  return (
    <div className="p-6 max-w-6xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">Ideathon Hub</h1>
        <p className="text-gray-600">Browse all Ideathon knowledge cards month-wise. Select an event to manage its teams, jury, presentations and audit.</p>
      </div>
      <div className="mb-6">
        {(!eventsByType || eventsByType.length === 0) ? (

          <div className="flex items-center gap-4">
            <select value={selectedEventId || ""} onChange={(e) => setSelectedEventId(e.target.value || null)} className="border p-2 rounded-md">
              <option value="">-- Select an Event --</option>
              {events.map((ev) => <option key={ev.eventId} value={ev.eventId}>{ev.title}</option>)}
            </select>
            <div className="text-sm text-gray-600">{selectedEventId ? `Selected: ${eventData.title || ""}` : "No event selected"}</div>
          </div>
        ) : !ideathonBlock ? (
          <div className="text-sm text-gray-500">No Ideathon events found.</div>
        ) : (

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-3">
            {ideathonBlock.months && ideathonBlock.months.length ? (
              ideathonBlock.months.map((m) => (
                <div key={`${m.year}-${m.month}`} className="border rounded-lg p-4 bg-gray-50">
                  <div className="flex justify-between items-center mb-3">
                    <div>
                      <div className="text-md font-semibold">{m.monthLabel}</div>
                      <div className="text-sm text-gray-600">{(m.events?.length || 0)} event(s)</div>
                    </div>
                  </div>

                  <div className="space-y-3">
                    {m.events && m.events.length ? (
                      m.events.map((ev) => (
                        <div key={ev.eventId} className="bg-white border rounded p-3 flex justify-between items-start shadow-sm">
                          <div className="flex-1 pr-3">
                            <div className="font-semibold text-sm">{ev.title}</div>
                            <div className="text-xs text-gray-600 mt-1 line-clamp-3">{ev.description}</div>
                            <div className="text-xs text-gray-500 mt-2">
                              {ev.startDate ? `From ${formatDateLabel(ev.startDate)}` : ""}
                              {ev.endDate ? ` — To ${formatDateLabel(ev.endDate)}` : ""}
                            </div>
                          </div>

                          <div className="flex flex-col items-end gap-2">
                            <button
                              onClick={() => {
                                setSelectedEventId(ev.eventId);
                                fetchEventData(ev.eventId);
                                window.scrollTo({ top: 0, behavior: "smooth" });
                              }}
                              className="px-3 py-1 rounded bg-blue-600 text-white text-sm"
                            >
                              Select
                            </button>
                          </div>
                        </div>
                      ))
                    ) : (
                      <div className="text-sm text-gray-500">No events for this month.</div>
                    )}
                  </div>
                </div>
              ))
            ) : (
              <div className="text-sm text-gray-500">No months available.</div>
            )}
          </div>
        )}
      </div>
      {!selectedEventId && (
        <div className="p-6 bg-white border rounded-lg text-center text-gray-600 mb-8">
          <div className="text-lg font-medium mb-2">No event selected</div>
          <div className="text-sm mb-3">Pick an event above to manage its teams, jury, presentations and audit.</div>
        </div>
      )}
      {selectedEventId && eventData && (
        <>
          <div className="mb-6">
            <h2 className="text-2xl font-semibold mb-2 text-blue-700 bg-gray-100 p-3 rounded">
              {eventData.title}
            </h2>

            <p className="mb-6 text-gray-700 bg-white p-3 rounded shadow-sm">{eventData.description}</p>
          </div>
          <div className="flex gap-4 mb-6 border-b">
            {["teams", "jury", "presentations", "audit"].map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`px-4 py-2 rounded-t-lg transition-all
                  ${activeTab === tab
                    ? "bg-indigo-100 text-indigo-700 font-semibold border-t border-l border-r border-indigo-700"
                    : "bg-slate-200 text-slate-700 hover:bg-slate-300 hover:text-indigo-700"
                  }
                `}
              >
                {tab.charAt(0).toUpperCase() + tab.slice(1)}
              </button>
            ))}
          </div>
          {activeTab === "teams" && (
            <div className="mb-8">
              <div className="flex items-center gap-4 mb-4">
                <input type="text" placeholder="Search teams or member..." value={searchTeam} onChange={(e) => setSearchTeam(e.target.value)} className="border p-2 rounded-md w-64" />
                <div className="text-sm text-gray-500">{filteredTeams.length} teams</div>
                <div className="ml-auto text-sm text-gray-400">Event: <strong>{eventData.title}</strong></div>
              </div>

              <div className="space-y-6">
                {paginatedTeams.map((team) => {
                  const lead = inferTeamLead(team);
                  const assignedMentors = team.mentors || [];
                  return (
                    <div key={team.teamId} className="border rounded-md p-4 bg-white shadow-sm">
                      <div className="flex justify-between items-center">
                        <div>
                          <strong className="text-lg">{team.teamName}</strong>
                          <div className="text-sm text-gray-600">Lead: {lead?.fullName || lead?.name || lead?.email || "—"}</div>
                        </div>

                        <div className="flex items-center gap-3">
                          <button
                            onClick={() =>
                              setExpandedTeamId((id) =>
                                id === team.teamId ? null : team.teamId
                              )
                            }
                            title={expandedTeamId === team.teamId ? "Collapse" : "Details"}
                            className="p-2 rounded-md bg-gray-100 hover:bg-gray-200"
                          >
                            {expandedTeamId === team.teamId ? (
                              <ChevronUpIcon className="w-4 h-4 text-gray-700" />
                            ) : (
                              <ChevronDownIcon className="w-4 h-4 text-gray-700" />
                            )}
                          </button>

                        </div>
                      </div>

                      {expandedTeamId === team.teamId && (
                        <div className="mt-4 grid grid-cols-1 md:grid-cols-3 gap-4">
                          <div className="col-span-1 border rounded p-3 bg-gray-50">
                            <h4 className="font-semibold mb-2">Members</h4>
                            {team.teamMembers && team.teamMembers.length ? (
                              <ul className="space-y-2 text-sm">
                                {team.teamMembers.map((m) => {
                                  const user = m.user || {};
                                  return (
                                    <li key={m.teamMemberId || m.userId} className="flex justify-between">
                                      <div>{user.name || "Unknown User"} {lead && String(lead.userId) === String(m.userId) && <span className="text-xs text-blue-600 ml-2">(Lead)</span>}</div>
                                      <div className="text-gray-500 text-xs">{user.email}</div>
                                    </li>
                                  );
                                })}
                              </ul>
                            ) : (
                              <div className="text-sm text-gray-500">No members registered.</div>
                            )}
                          </div>

                          <div className="col-span-1 border rounded p-3 bg-gray-50">
                            <h4 className="font-semibold mb-2">Mentors</h4>
                            {assignedMentors && assignedMentors.length ? (
                              <ul className="space-y-2 mb-3">
                                {assignedMentors.map((am) => (
                                  <li key={am.mentorId} className="flex justify-between items-center">
                                    <div className="text-sm">{am.user?.fullName || am.user?.name || am.user?.email || am.userId}</div>
                                    <div className="flex gap-2">
                                      <button
                                        onClick={() => removeMentor(team.teamId, am.mentorId)}
                                        title="Remove mentor"
                                        className="p-1"
                                      >
                                        <TrashIcon className="w-4 h-4 text-red-600 hover:text-red-800" />
                                      </button>
                                    </div>

                                  </li>
                                ))}
                              </ul>
                            ) : <div className="text-sm text-gray-500 mb-3">No mentors assigned</div>}

                            <select value={mentorSelect[team.teamId] || ""} onChange={(e) => setMentorSelect((s) => ({ ...s, [team.teamId]: e.target.value }))} className="border p-2 rounded-md w-full mb-2 text-sm">
                              <option value="">Select mentor to assign</option>
                              {mentors.filter((m) => !assignedMentors.some((am) => String(am.userId) === String(m.userId))).map((m) => (
                                <option key={m.userId} value={m.userId}>{m.fullName || m.name || m.email}</option>
                              ))}
                            </select>

                            <div className="flex gap-2">
                              <button onClick={() => { const sel = mentorSelect[team.teamId]; if (!sel) return toast.error("Select a mentor first."); assignMentor(team.teamId, sel); setMentorSelect((s) => ({ ...s, [team.teamId]: "" })); }} className="px-3 py-1 rounded bg-blue-600 text-white text-sm">Assign Mentor</button>
                              <button onClick={() => setExpandedTeamId(team.teamId)} className="px-3 py-1 rounded bg-gray-200 text-sm">Focus Schedule</button>
                            </div>
                          </div>

                          <div className="col-span-1 border rounded p-3 bg-gray-50">
                            <h4 className="font-semibold mb-2">Presentation</h4>
                            {presentations.some((p) => String(p.teamId) === String(team.teamId)) ? (
                              <div className="text-sm mb-3">
                                {presentations.filter((p) => String(p.teamId) === String(team.teamId)).map((p) => (
                                  <div key={p.presentationId} className="flex justify-between items-center mb-2">
                                    <div>{p.teamName} — {p.presentationDate ? new Date(p.presentationDate).toLocaleString() : "Not scheduled"}</div>
                                    <button onClick={() => removePresentation(p.presentationId)} className="px-2 py-1 rounded bg-red-200 text-xs">Remove</button>
                                  </div>
                                ))}
                              </div>
                            ) : <div className="text-sm text-gray-500 mb-3">No presentation scheduled</div>}

                            <label className="block text-sm mb-1">Pick date & time</label>
                            <input type="datetime-local" value={scheduleInputs[team.teamId] || ""} onChange={(e) => setScheduleInputs((s) => ({ ...s, [team.teamId]: e.target.value }))} className="border p-2 rounded-md w-full mb-2 text-sm" />
                            <div className="flex gap-2">
                              <button onClick={() => schedulePresentation(team.teamId)} className="px-3 py-1 rounded bg-green-600 text-white text-sm">Schedule Presentation</button>
                              <button onClick={() => setScheduleInputs((s) => ({ ...s, [team.teamId]: "" }))} className="px-3 py-1 rounded bg-gray-200 text-sm">Clear</button>
                            </div>
                          </div>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>

              <div className="mt-6 flex items-center gap-3">
                <button disabled={page <= 1} onClick={() => setPage((p) => Math.max(1, p - 1))} className="px-3 py-1 rounded bg-gray-200">Prev</button>
                <div className="text-sm">Page {page} / {totalPages || 1}</div>
                <button disabled={page >= totalPages} onClick={() => setPage((p) => Math.min(totalPages, p + 1))} className="px-3 py-1 rounded bg-gray-200">Next</button>
              </div>
            </div>
          )}
          {activeTab === "jury" && (
            <div className="max-w-2xl mb-8">
              <div className="flex gap-3 items-center mb-4">
                <select value={selectedJuryToAdd} onChange={(e) => setSelectedJuryToAdd(e.target.value)} className="border p-2 rounded-md w-72">
                  <option value="">Select jury to add</option>
                  {juryUsers.filter((u) => !juryMembers.some((j) => String(j.userId) === String(u.userId))).map((u) => (
                    <option key={u.userId} value={u.userId}>{u.name} ({u.email})</option>
                  ))}
                </select>
                <button
                  onClick={addJuryMember}
                  className="flex items-center gap-1 px-3 py-1 rounded bg-blue-600 text-white hover:bg-blue-700"
                >
                  <PlusIcon className="w-4 h-4" />
                  Add
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
                            title="Remove jury member"
                            className="p-1"
                          >
                            <TrashIcon className="w-4 h-4 text-red-600 hover:text-red-800" />
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
          {activeTab === "presentations" && (
            <div className="mb-8">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                {presentations.length ? (
                  presentations.map((p) => (
                    <div key={p.presentationId} className="bg-white border rounded-lg p-4 shadow-sm flex justify-between items-start">
                      <div>
                        <div className="font-semibold text-lg">{p.teamName}</div>
                        <div className="text-sm text-gray-600 mt-1">{p.presentationDate ? new Date(p.presentationDate).toLocaleString() : "Not scheduled"}</div>
                        {p.presenterName && <div className="text-sm text-gray-500 mt-1">Presenter: {p.presenterName}</div>}
                        {p.location && <div className="text-sm text-gray-500 mt-1">Location: {p.location}</div>}
                      </div>
                      <div className="flex flex-col items-end gap-2">
                        <button onClick={() => removePresentation(p.presentationId)} className="px-3 py-1 rounded bg-red-200 text-sm">Remove</button>
                      </div>
                    </div>
                  ))
                ) : <div className="text-sm text-gray-500">No presentations scheduled yet.</div>}
              </div>

              <div className="bg-white border rounded-lg p-4 shadow-sm max-w-2xl">
                <h4 className="font-semibold mb-3">Schedule Presentation</h4>
                <div className="flex flex-col md:flex-row gap-3 items-start">
                  <select value={presentationTeamToSchedule || ""} onChange={(e) => setPresentationTeamToSchedule(e.target.value)} className="border p-2 rounded-md w-full md:w-1/2">
                    <option value="">Select a team</option>
                    {teams.map((t) => <option key={t.teamId} value={t.teamId}>{t.teamName}</option>)}
                  </select>

                  <input type="datetime-local" value={presentationDatetime} onChange={(e) => setPresentationDatetime(e.target.value)} className="border p-2 rounded-md w-full md:w-1/3" />

                  <div className="flex gap-2">
                    <button onClick={schedulePresentationCompact} className="px-4 py-1 rounded bg-green-600 text-white">Schedule</button>
                    <button onClick={() => { setPresentationTeamToSchedule(""); setPresentationDatetime(""); }} className="px-4 py-1 rounded bg-gray-200">Clear</button>
                  </div>
                </div>
                <p className="text-xs text-gray-500 mt-2">Tip: scheduled items appear above as cards.</p>
              </div>
            </div>
          )}
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

