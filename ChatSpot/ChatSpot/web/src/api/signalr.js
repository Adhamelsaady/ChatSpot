import * as signalR from '@microsoft/signalr';

const BASE_URL = 'https://localhost:7184';

let connection = null;

export const getHubConnection = () => {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${BASE_URL}/hubs/chat`, {
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
