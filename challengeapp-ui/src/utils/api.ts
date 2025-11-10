import axios from "axios";

const token = typeof window !== "undefined" ? localStorage.getItem("token") : null;

export const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: {
    Authorization: token ? `Bearer ${token}` : ""
  }
});
