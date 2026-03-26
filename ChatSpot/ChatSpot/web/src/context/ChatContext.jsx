import React, { createContext, useContext, useState, useEffect, useCallback, useRef } from 'react';
import { chatApi, groupApi } from '../api/client';
import { startConnection, getHubConnection } from '../api/signalr';
import { useAuth } from './AuthContext';

const ChatContext = createContext(null);

const MESSAGE_PAGE_SIZE = 50;

export const ChatProvider = ({ children }) => {
  const { user } = useAuth();
  const [conversations, setConversations] = useState([]);
  const [groups, setGroups] = useState([]);
  const [activeChat, setActiveChat] = useState(null);
  const [messages, setMessages] = useState([]);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [messagesPage, setMessagesPage] = useState(1);
  const [hasMoreOlder, setHasMoreOlder] = useState(false);
  const [loadingOlder, setLoadingOlder] = useState(false);
  const messagesPageRef = useRef(1);
  const hasMoreOlderRef = useRef(false);
  const loadingOlderRef = useRef(false);

  useEffect(() => {
    messagesPageRef.current = messagesPage;
  }, [messagesPage]);
  useEffect(() => {
    hasMoreOlderRef.current = hasMoreOlder;
  }, [hasMoreOlder]);
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
    if (!chat) {
      setMessages([]);
      setTypingUsers([]);
      setLoadingMessages(false);
      setMessagesPage(1);
      messagesPageRef.current = 1;
      setHasMoreOlder(false);
      return;
    }
    setLoadingMessages(true);
    setMessages([]);
    setTypingUsers([]);
    setMessagesPage(1);
    messagesPageRef.current = 1;
    setHasMoreOlder(false);
    try {
      let data;
      if (chat.type === 'dm') {
        ({ data } = await chatApi.getMessages(chat.id, { PageNumber: 1, PageSize: MESSAGE_PAGE_SIZE }));
      } else {
        ({ data } = await groupApi.getGroupMessages(chat.id, { PageNumber: 1, PageSize: MESSAGE_PAGE_SIZE }));
      }
      const msgs = Array.isArray(data) ? data : data.items || data.data || data.messages || [];
      setMessages([...msgs].reverse());
      setHasMoreOlder(msgs.length >= MESSAGE_PAGE_SIZE);
    } catch (err) {
      console.error('Failed to load messages:', err);
    } finally {
      setLoadingMessages(false);
      // Refresh sidebar to clear unread badge
      if (chat.type === 'dm') loadConversations();
      else loadGroups();
    }
  }, [loadConversations, loadGroups]);

  const loadOlderMessages = useCallback(async () => {
    const chat = activeChatRef.current;
    if (!chat || loadingOlderRef.current || !hasMoreOlderRef.current) return;
    loadingOlderRef.current = true;
    setLoadingOlder(true);
    const nextPage = messagesPageRef.current + 1;
    try {
      let data;
      if (chat.type === 'dm') {
        ({ data } = await chatApi.getMessages(chat.id, { PageNumber: nextPage, PageSize: MESSAGE_PAGE_SIZE }));
      } else {
        ({ data } = await groupApi.getGroupMessages(chat.id, { PageNumber: nextPage, PageSize: MESSAGE_PAGE_SIZE }));
      }
      const msgs = Array.isArray(data) ? data : data.items || data.data || data.messages || [];
      if (msgs.length === 0) {
        setHasMoreOlder(false);
        return;
      }
      const older = [...msgs].reverse();
      setMessages((prev) => [...older, ...prev]);
      setMessagesPage(nextPage);
      messagesPageRef.current = nextPage;
      setHasMoreOlder(msgs.length >= MESSAGE_PAGE_SIZE);
    } catch (err) {
      console.error('Failed to load older messages:', err);
    } finally {
      loadingOlderRef.current = false;
      setLoadingOlder(false);
    }
  }, []);

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
      hub.on('MemberRemoved', (groupId, userId) => {
        const current = activeChatRef.current;
        if (current?.type === 'group' && current?.id === groupId) {
          setMessages(prev => [...prev, {
            id: `sys-${Date.now()}`,
            content: `A member was removed from the group`,
            isSystem: true,
            timestamp: new Date().toISOString(),
          }]);
        }
        loadGroups();
      });

      hub.on('MemberLeft', (groupId, userId) => {
        const current = activeChatRef.current;
        if (current?.type === 'group' && current?.id === groupId) {
          setMessages(prev => [...prev, {
            id: `sys-${Date.now()}`,
            content: `A member left the group`,
            isSystem: true,
            timestamp: new Date().toISOString(),
          }]);
        }
        loadGroups();
      });

      hub.on('MembersAdded', (groupId, userIds) => {
        const current = activeChatRef.current;
        if (current?.type === 'group' && current?.id === groupId) {
          setMessages(prev => [...prev, {
            id: `sys-${Date.now()}`,
            content: `${userIds.length} member${userIds.length > 1 ? 's' : ''} added to the group`,
            isSystem: true,
            timestamp: new Date().toISOString(),
          }]);
        }
        loadGroups();
      });

      hub.on('MemberRoleChanged', (groupId, userId, newRole) => {
        const current = activeChatRef.current;
        if (current?.type === 'group' && current?.id === groupId) {
          setMessages(prev => [...prev, {
            id: `sys-${Date.now()}`,
            content: `A member's role was changed to ${newRole}`,
            isSystem: true,
            timestamp: new Date().toISOString(),
          }]);
        }
      });

      hub.on('RemovedFromGroup', (groupId) => {
        loadGroups();
        const current = activeChatRef.current;
        if (current?.type === 'group' && current?.id === groupId) {
          openChat(null);
        }
      });

      hub.on('AddedToGroup', (group) => {
        loadGroups();
      });

      hub.on('RoleChanged', (groupId, newRole) => {
        // Current user's role changed - reload groups
        loadGroups();
      });
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
          'UserTypingInGroup','UserStoppedTypingInGroup','UserOnline','UserOffline',
          'MemberRemoved','MemberLeft','MembersAdded','MemberRoleChanged',
          'RemovedFromGroup','AddedToGroup','RoleChanged']
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
    if (!chat) return;

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
        loadOlderMessages,
        loadingOlder,
        hasMoreOlder,
      }}>
        {children}
      </ChatContext.Provider>
  );
};

export const useChat = () => useContext(ChatContext);