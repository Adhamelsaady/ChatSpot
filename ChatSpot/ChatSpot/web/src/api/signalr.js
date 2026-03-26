import * as signalR from '@microsoft/signalr';

function trimBase(v) {
  return typeof v === 'string' ? v.replace(/\/+$/, '') : '';
}

// Same rules as client.js: unset in dev → '/chatHub' via Vite proxy to local ASP.NET.
// Set VITE_API_URL (and optionally VITE_SIGNALR_URL) to use a hosted API from dev or prod.
const hubBase =
  trimBase(import.meta.env.VITE_SIGNALR_URL) ||
  trimBase(import.meta.env.VITE_API_URL) ||
  '';

const hubUrl = hubBase ? `${hubBase}/chatHub` : '/chatHub';

let connection = null;

export const getHubConnection = () => {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => localStorage.getItem('accessToken'),
        skipNegotiation: false,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();
  }
  return connection;
};

export const startConnection = async () => {
  const conn = getHubConnection();
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    try {
      await conn.start();
      console.log('[SignalR] Connected');
    } catch (err) {
      console.error('[SignalR] Connection failed:', err);
    }
  }
  return conn;
};

export const stopConnection = async () => {
  if (connection && connection.state !== signalR.HubConnectionState.Disconnected) {
    await connection.stop();
    connection = null;
  }
};
