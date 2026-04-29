
import React, { createContext, useContext, useState, useCallback } from 'react';
import { authApi } from '../api/client';
import { stopConnection } from '../api/signalr';

// Decode JWT payload without a library
const decodeJwt = (token) => {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64));
  } catch {
    return null;
  }
};

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(() => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      const payload = decodeJwt(token);
      console.log(payload.exp * 1000, Date.now(), payload.exp * 1000 > Date.now())
      if (payload && payload.exp * 1000 > Date.now()) {
        return {
          id: payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
          email: payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
          username: payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
          profilePicture: localStorage.getItem('profilePicture') || '',
          token,
        };
      }
    }
    return null;
  });

  const login = useCallback(async (emailOrUserName, password) => {
    const { data } = await authApi.login({
      emailOrUserName: emailOrUserName?.trim?.() ?? emailOrUserName,
      password,
    });
    localStorage.setItem('accessToken', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    if (data.profilePicture) localStorage.setItem('profilePicture', data.profilePicture);
    const payload = decodeJwt(data.token);
    const userData = {
      id: payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
      email: payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
      username: payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
      profilePicture: data.profilePicture || '',
      token: data.token,
    };
    setUser(userData);
    return userData;
  }, []);

  const logout = useCallback(async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) await authApi.logout(refreshToken);
    } catch {}
    await stopConnection();
    localStorage.clear();
    sessionStorage.clear();
    setUser(null);
    window.location.href = new URL(import.meta.env.BASE_URL || '/', window.location.origin).href;
  }, []);

  const googleLogin = useCallback(async (idToken) => {
    const { data } = await authApi.googleLogin(idToken);
    localStorage.setItem('accessToken', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    if (data.profilePicture) localStorage.setItem('profilePicture', data.profilePicture);
    const payload = decodeJwt(data.token);
    const userData = {
      id: payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
      email: payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
      username: payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
      profilePicture: data.profilePicture || '',
      token: data.token,
    };
    setUser(userData);
    return userData;
  }, []);

  return (
    <AuthContext.Provider value={{ user, setUser, login, googleLogin, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
