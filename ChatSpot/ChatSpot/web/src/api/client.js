import axios from 'axios';

// Backend is ASP.NET. Leave VITE_API_URL unset in dev to use Vite proxy → localhost (see vite.config.js).
// Set VITE_API_URL in .env.local or the Vercel build to point at your deployed API (no trailing slash).
const envBase = import.meta.env.VITE_API_URL;
const BASE_URL = typeof envBase === 'string' ? envBase.replace(/\/+$/, '') : '';

export const api = axios.create({
  baseURL: BASE_URL || undefined,
  headers: { 'Content-Type': 'application/json' },
});

// Attach JWT token to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) prom.reject(error);
    else prom.resolve(token);
  });
  failedQueue = [];
};

// Auto refresh JWT on 401
api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        });
      }
      originalRequest._retry = true;
      isRefreshing = true;
      try {
        const refreshToken = localStorage.getItem('refreshToken');
        const accessToken = localStorage.getItem('accessToken');
        const refreshUrl = BASE_URL
          ? `${BASE_URL}/api/auth/refresh-token`
          : '/api/auth/refresh-token';
        const { data } = await axios.post(refreshUrl, {
          token: accessToken,
          refreshToken,
        });
        localStorage.setItem('accessToken', data.token);
        localStorage.setItem('refreshToken', data.refreshToken);
        processQueue(null, data.token);
        originalRequest.headers.Authorization = `Bearer ${data.token}`;
        return api(originalRequest);
      } catch (err) {
        processQueue(err, null);
        localStorage.clear();
        window.location.href = new URL(
          'login',
          new URL(import.meta.env.BASE_URL || '/', window.location.origin)
        ).href;
        return Promise.reject(err);
      } finally {
        isRefreshing = false;
      }
    }
    return Promise.reject(error);
  }
);

// ─── Auth ────────────────────────────────────────────────────────────────────
export const authApi = {
  register: (data) => {
    const formData = data instanceof FormData ? data : (() => {
      const fd = new FormData();
      Object.entries(data).forEach(([key, val]) => {
        if (val != null) fd.append(key, val);
      });
      return fd;
    })();
    return api.post('/api/auth/register', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  confirmEmail: (data) => api.post('/api/auth/confirm-email', data),
  login: (data) => api.post('/api/auth/login', data),
  logout: (refreshToken) => api.post('/api/auth/logout', { refreshToken }),
  refreshToken: (data) => api.post('/api/auth/refresh-token', data),
};

// ─── Chat ────────────────────────────────────────────────────────────────────
export const chatApi = {
  getConversations: (params) => api.get('/api/chat', { params }),
  getMessages: (conversationId, params) =>
    api.get(`/api/chat/${conversationId}`, { params }),
  createConversation: (otherUserId) =>
    api.post('/api/chat/create-conversation', { otherUserId }),
  sendMessage: (conversationId, content, replyToId = null) =>
    api.post(`/api/chat/${conversationId}/send-message`, { content, replyToId }),
  deleteMessage: (conversationId, messageId) =>
    api.delete(`/api/chat/${conversationId}/delete-message`, {
      data: { messageId },
    }),
};

// ─── Group ───────────────────────────────────────────────────────────────────
export const groupApi = {
  getGroups: (params) => api.get('/api/group', { params }),
  getGroupMessages: (groupId, params) =>
    api.get(`/api/group/${groupId}`, { params }),
  createGroup: (data) => api.post('/api/group/create-group', data),
  sendGroupMessage: (groupId, content, replyToId = null) =>
    api.post(`/api/group/${groupId}/send-message`, { content, replyToId }),
  deleteGroupMessage: (groupId, messageId) =>
    api.delete(`/api/group/${groupId}/delete-message`, { data: { messageId } }),
  getMembers: (groupId, params) =>
    api.get(`/api/group/${groupId}/members`, { params }),
  addMembers: (groupId, userIds) =>
    api.post(`/api/group/${groupId}/members`, { userIds }),
  removeMember: (groupId, targetUserId) =>
    api.delete(`/api/group/${groupId}/remove/${targetUserId}`),
  leaveGroup: (groupId) => api.delete(`/api/group/${groupId}/leave`),
  changeRole: (groupId, userId) =>
    api.post(`/api/group/${groupId}/members/${userId}/change-role`),
};

// ─── User ────────────────────────────────────────────────────────────────────
export const userApi = {
  search: (params) => api.get('/api/user/search', { params }),
};
