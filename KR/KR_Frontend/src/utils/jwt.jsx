// parse JWT payload (no verification, client-side only)
export function parseJwt(token) {
  if (!token) return null;
  try {
    const raw = token.split(" ").pop(); // allow "Bearer ..." or raw token
    const payload = raw.split(".")[1];
    if (!payload) return null;
    const decoded = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    const json = decodeURIComponent(
      decoded
        .split("")
        .map((c) => `%${("00" + c.charCodeAt(0).toString(16)).slice(-2)}`)
        .join("")
    );
    return JSON.parse(json);
  } catch (e) {
    console.warn("Invalid JWT", e);
    return null;
  }
}

/**
 * Return array of event IDs from claim names:
 * "event_jury", "eventJury", "eventJuries"
 */
export function getEventJuryIdsFromToken(token) {
  const claims = parseJwt(token);
  if (!claims) return [];
  const raw = claims["event_jury"] ?? claims["eventJury"] ?? claims["eventJuries"] ?? null;
  if (!raw) return [];
  return Array.isArray(raw) ? raw.map(String) : [String(raw)];
}
