import axios from "axios";

const baseURL = process.env.REACT_APP_API_URL || "https://localhost:7098/api";

const api = axios.create({
  baseURL,
   timeout: 10000,
});
// Add a request interceptor to attach token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("jwtToken");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);


export default api;
