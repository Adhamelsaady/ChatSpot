import React, { createContext, useContext, useState, useEffect, useCallback, useRef } from 'react';
import { chatApi, groupApi } from '../api/client';
import { startConnection, getHubConnection } from '../api/signalr';
import { useAuth } from './AuthContext';

const ChatContext = createContext(null);

export const ChatProvider = ({ children }) => {
  const { user } = useAuth();
  const [conversations, setConversations] = useState([]);
  const [groups, setGroups] = useState([]);
  const [activeChat, setActiveChat] = useState(null);
  const [messages, setMessages] = useState([]);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [replyTo, setReplyTo] = useState(null);
  const [typingUsers, setTypingUsers] = useState([]);
  const activeChatRef = useRef(null);
  const hubRef = useRef(null);

  useEffect(() => {
    activeChatRef.current = activeChat;
  }, [activeChat]);

  const loadConversations = useCallback(async () => {
    try {
      const { data } = await chatApi.getConversations({ PageNumber: 1, PageSize: 50 });
      setConversations(Array.isArray(data) ? data : data.items || data.data || []);
    } catch (err) {
      console.error('Failed to load conversations:', err);
    }
  }, []);

  const loadGroups = useCallback(async () => {
    try {
      const { data } = await groupApi.getGroups({ PageNumber: 1, PageSize: 50 });
      setGroups(Array.isArray(data) ? data : data.items || data.data || []);
    } catch (err) {
      console.error('Failed to load groups:', err);
    }
  }, []);

  const loadMessages = useCallback(async (chat) => {
    if (!chat) return;
    setLoadingMessages(true);
    setMessages([]);
    setTypingUsers([]);
    try {
      let data;
      if (chat.type === 'dm') {
        ({ data } = await chatApi.getMessages(chat.id, { PageNumber: 1, PageSize: 50 }));
      } else {
        ({ data } = await groupApi.getGroupMessages(chat.id, { PageNumber: 1, PageSize: 50 }));
      }
      const msgs = Array.isArray(data) ? data : data.items || data.data || data.messages || [];
      setMessages([...msgs].reverse());
    } catch (err) {
      console.error('Failed to load messages:', err);
    } finally {
      setLoadingMessages(false);
      // Refresh sidebar to clear unread badge
      if (chat.type === 'dm') loadConversations();
      else loadGroups();
    }
  }, [loadConversations, loadGroups]);

  // SignalR setup
  useEffect(() => {
    if (!user) return;
    let mounted = true;

    const setup = async () => {
      const hub = await startConnection();
      if (!mounted) return;
      hubRef.current = hub;

      hub.on('ReceiveDirectMessage', (conversationId, message) => {
        const current = activeChatRef.current;
        if (current?.type === 'dm' && current?.id === conversationId) {
          setMessages(prev => {
            if (prev.find(m => m.id === message.id)) return prev;
            return [...prev, message];
          });
        }
        loadConversations();
      });

      hub.on('ReceiveGroupMessage', (groupId, message) => {
        const current = activeChatRef.current;
        if (current?.type === 'group' && current?.id === groupId) {
          setMessages(prev => {
            if (prev.find(m => m.id === message.id)) return prev;
            return [...prev, message];
          });
        }
        loadGroups();
      });

      hub.on('DirectMessageDeleted', (conversationId, messageId) => {
        const current = activeChatRef.current;
        if (current?.type === 'dm' && current?.id === conversationId) {
          setMessages(prev =>
              prev.map(m => m.id === messageId ? { ...m, isDeleted: true, content: 'Message deleted' } : m)
          );
        }
      });

      hub.on('GroupMessageDeleted', (groupId, messageId) => {
        const current = activeChatRef.current;
        if (current?.type === 'group' && current?.id === groupId) {
          setMessages(prev =>
              prev.map(m => m.id === messageId ? { ...m, isDeleted: true, content: 'Message deleted' } : m)
          );
        }
      });

      hub.on('UserTyping', (conversationId, userId) => {
        if (activeChatRef.current?.id === conversationId)
          setTypingUsers(prev => prev.includes(userId) ? prev : [...prev, userId]);
      });
      hub.on('UserStoppedTyping', (_, userId) => {
        setTypingUsers(prev => prev.filter(id => id !== userId));
      });
      hub.on('UserTypingInGroup', (groupId, userId) => {
        if (activeChatRef.current?.id === groupId)
          setTypingUsers(prev => prev.includes(userId) ? prev : [...prev, userId]);
      });
      hub.on('UserStoppedTypingInGroup', (_, userId) => {
        setTypingUsers(prev => prev.filter(id => id !== userId));
      });
      hub.on('ConversationRead', (conversationId, readByUserId) => {
        // Update messages in the active conversation to show as read
        const current = activeChatRef.current;
        if (current?.type === 'dm' && current?.id === conversationId) {
          setMessages(prev => prev.map(m => ({ ...m, isRead: true })));
        }
        loadConversations();
      });
      hub.on('UserOnline', () => loadConversations());
      hub.on('UserOffline', () => loadConversations());
    };

    setup();
    loadConversations();
    loadGroups();

    return () => {
      mounted = false;
      const hub = getHubConnection();
      if (hub) {
        ['ReceiveDirectMessage','ReceiveGroupMessage','DirectMessageDeleted',
          'GroupMessageDeleted','UserTyping','UserStoppedTyping',
          'UserTypingInGroup','UserStoppedTypingInGroup','UserOnline','UserOffline']
            .forEach(e => hub.off(e));
      }
    };
  }, [user, loadConversations, loadGroups]);

  // Join/leave SignalR group rooms
  useEffect(() => {
    const hub = hubRef.current;
    if (!hub || !activeChat || activeChat.type !== 'group') return;
    hub.invoke('JoinGroup', activeChat.id).catch(console.error);
    return () => { hub.invoke('LeaveGroup', activeChat.id).catch(() => {}); };
  }, [activeChat]);
  const openChat = useCallback(async (chat) => {
    setActiveChat(chat);
    setReplyTo(null);
    setTypingUsers([]);
    loadMessages(chat);

    const hub = hubRef.current || getHubConnection();
    if (!hub) return;

    try {
      if (chat.type === 'dm') {
        const otherUserId = chat.data?.user?.id;
        if (otherUserId) {
          await hub.invoke('MarkConversationRead', chat.id, otherUserId);
        }
      } else {
        await hub.invoke('MarkGroupRead', chat.id);
      }
    } catch (err) {
      console.error('Mark as read failed:', err);
    }
  }, [loadMessages]);

  const sendMessage = useCallback(async (content) => {
    if (!activeChat || !content.trim()) return;
    const replyToId = replyTo?.id || null;
    setReplyTo(null);
    try {
      if (activeChat.type === 'dm') {
        const { data } = await chatApi.sendMessage(activeChat.id, content, replyToId);
        const hub = hubRef.current;
        if (hub) {
          await hub.invoke('SendDirectMessage', activeChat.id, { content, replyToId }, data)
              .catch(console.error);
        }
      } else {
        const { data } = await groupApi.sendGroupMessage(activeChat.id, content, replyToId);
        const hub = hubRef.current;
        if (hub) {
          await hub.invoke('SendGroupMessage', activeChat.id, { content, replyToId }, data)
              .catch(console.error);
        }
      }
    } catch (err) {
      console.error('Send failed:', err);
    }
  }, [activeChat, replyTo]);

  const deleteMessage = useCallback(async (messageId) => {
    if (!activeChat) return;
    try {
      if (activeChat.type === 'dm') {
        await chatApi.deleteMessage(activeChat.id, messageId);
        const hub = hubRef.current;
        const otherUserId = activeChat.data?.user?.id;
        if (hub && otherUserId) {
          await hub.invoke('DeleteDirectMessage', activeChat.id, messageId, otherUserId).catch(console.error);
        }
      } else {
        await groupApi.deleteGroupMessage(activeChat.id, messageId);
        const hub = hubRef.current;
        if (hub) {
          await hub.invoke('DeleteGroupMessage', activeChat.id, messageId).catch(console.error);
        }
      }
      setMessages(prev =>
          prev.map(m => m.id === messageId ? { ...m, isDeleted: true, content: 'Message deleted' } : m)
      );
    } catch (err) {
      console.error('Delete failed:', err);
    }
  }, [activeChat]);

  return (
      <ChatContext.Provider value={{
        conversations, groups,
        activeChat, openChat,
        messages, loadingMessages,
        sendMessage, deleteMessage,
        replyTo, setReplyTo,
        typingUsers,
        loadConversations, loadGroups,
      }}>
        {children}
      </ChatContext.Provider>
  );
};

export const useChat = () => useContext(ChatContext);