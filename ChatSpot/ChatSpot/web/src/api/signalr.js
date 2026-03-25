import * as signalR from '@microsoft/signalr';

// Dev: same origin so Vite proxies /chatHub → ASP.NET. Prod: set VITE_SIGNALR_URL or use default host.
const hubBase =
  import.meta.env.VITE_SIGNALR_URL ||
  (import.meta.env.DEV ? '' : 'https://chatspot-production-640b.up.railway.app');
const hubUrl = hubBase ? `${hubBase.replace(/\/+$/, '')}/chatHub` : '/chatHub';

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
