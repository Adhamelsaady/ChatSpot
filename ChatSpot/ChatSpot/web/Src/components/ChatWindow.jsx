import React ,  { useEffect, useRef, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useChat } from '../context/ChatContext';
import MessageBubble from './MessageBubble';
import MessageInput from './MessageInput';
import GroupInfoPanel from './GroupInfoPanel';
import styles from './ChatWindow.module.css';

const Avatar = ({ name, src, size = 36 }) => {
  const initials = name?.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase() || '?';
  const colors = ['#7c6af0','#6a8af0','#f06a8a','#f0a06a','#6af0a8'];
  const color = colors[(name?.charCodeAt(0) || 0) % colors.length];
  return (
    <div style={{
      width: size, height: size, borderRadius: '50%',
      background: src ? 'transparent' : color,
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      fontSize: size * 0.35, fontWeight: 700, color: 'white',
      fontFamily: 'var(--font-display)', flexShrink: 0,
    }}>
      {src ? <img src={src} alt={name} style={{ width: '100%', height: '100%', borderRadius: '50%', objectFit: 'cover' }} /> : initials}
    </div>
  );
};

export default function ChatWindow() {
  const { user } = useAuth();
  const { activeChat, messages, loadingMessages, deleteMessage, replyTo, setReplyTo } = useChat();
  const bottomRef = useRef(null);
  const [showInfo, setShowInfo] = useState(false);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const chatName = activeChat?.type === 'group'
    ? activeChat.data?.name
    : activeChat?.data?.otherUserName || 'Conversation';

  const chatSub = activeChat?.type === 'group'
    ? `${activeChat.data?.memberCount || ''} members`
    : activeChat?.data?.isOnline ? 'Online' : 'Offline';

  return (
    <div className={styles.window}>
      {/* Header */}
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <Avatar
            name={chatName}
            src={activeChat?.data?.avatarUrl || activeChat?.data?.otherUserAvatar}
            size={36}
          />
          <div className={styles.headerInfo}>
            <span className={styles.chatName}>{chatName}</span>
            <span className={`${styles.chatSub} ${activeChat?.data?.isOnline ? styles.online : ''}`}>
              {chatSub}
            </span>
          </div>
        </div>
        <div className={styles.headerActions}>
          {activeChat?.type === 'group' && (
            <button
              className={`${styles.actionBtn} ${showInfo ? styles.activeAction : ''}`}
              onClick={() => setShowInfo(v => !v)}
              title="Group info"
            >
              <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
                <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
                <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
              </svg>
            </button>
          )}
        </div>
      </div>

      <div className={styles.body}>
        {/* Messages */}
        <div className={styles.messages}>
          {loadingMessages && (
            <div className={styles.loadingWrap}>
              <div className={styles.loadingDots}>
                <span /><span /><span />
              </div>
            </div>
          )}

          {!loadingMessages && messages.length === 0 && (
            <div className={styles.startMsg}>
              <div className={styles.startIcon}>💬</div>
              <p>No messages yet. Say hello!</p>
            </div>
          )}

          {messages.map((msg, i) => {
            const isMine = msg.senderId === user?.id;
            const prevMsg = messages[i - 1];
            const showAvatar = !isMine && (!prevMsg || prevMsg.senderId !== msg.senderId);
            return (
              <MessageBubble
                key={msg.id || i}
                message={msg}
                isMine={isMine}
                showAvatar={showAvatar}
                onDelete={() => deleteMessage(msg.id)}
                onReply={() => setReplyTo(msg)}
              />
            );
          })}
          <div ref={bottomRef} />
        </div>

        {/* Group info panel */}
        {showInfo && activeChat?.type === 'group' && (
          <GroupInfoPanel groupId={activeChat.id} onClose={() => setShowInfo(false)} />
        )}
      </div>

      {/* Reply preview */}
      {replyTo && (
        <div className={styles.replyPreview}>
          <div className={styles.replyBar} />
          <div className={styles.replyContent}>
            <span className={styles.replyLabel}>Replying to</span>
            <span className={styles.replyText}>{replyTo.content}</span>
          </div>
          <button className={styles.replyClose} onClick={() => setReplyTo(null)}>
            <svg width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
      )}

      <MessageInput />
    </div>
  );
}
