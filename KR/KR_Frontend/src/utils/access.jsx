// read stored event-jury ids from localStorage
export function readEventJuryIdsFromStorage() {
  try {
    const raw =
      localStorage.getItem("eventJuryIds") ||
      localStorage.getItem("eventJuries") ||
      localStorage.getItem("event_juries") ||
      null;
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    if (!parsed) return [];
    return Array.isArray(parsed) ? parsed.map(String) : [String(parsed)];
  } catch {
    return [];
  }
}

// check if user has jury access for an event
export function userHasEventAccess(eventId) {
  if (!eventId) return false;
  const arr = readEventJuryIdsFromStorage();
  return arr.map(String).includes(String(eventId));
}
