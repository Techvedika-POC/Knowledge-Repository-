const dev = process.env.NODE_ENV !== "production";

export const API_BASE_URL = dev
  ? "https://localhost:7098/api"  // local dev
  : "https://your-production-url.com/api"; // prod


