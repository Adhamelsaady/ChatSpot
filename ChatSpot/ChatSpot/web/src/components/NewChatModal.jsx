import React , { useState, useEffect, useCallback } from 'react';
import { userApi, chatApi } from '../api/client';
import { useChat } from '../context/ChatContext';
import styles from './Modal.module.css';

const Avatar = ({ name, size = 36 }) => {
  const initials = name?.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase() || '?';
  const colors = ['#7c6af0','#6a8af0','#f06a8a','#f0a06a','#6af0a8'];
  const color = colors[(name?.charCodeAt(0) || 0) % colors.length];
  return (
    <div style={{
      width: size, height: size, borderRadius: '50%', background: color,
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      fontSize: 13, fontWeight: 700, color: 'white', fontFamily: 'var(--font-display)', flexShrink: 0,
    }}>{initials}</div>
  );
};

export default function NewChatModal({ onClose }) {
  const { openChat, loadConversations } = useChat();
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);

  const search = useCallback(async (q) => {
    if (!q.trim()) { setResults([]); return; }
    setLoading(true);
    try {
      const { data } = await userApi.search({ SearchQuery: q, PageNumber: 1, PageSize: 20 });
      setResults(Array.isArray(data) ? data : data.items || data.data || []);
    } catch {
      setResults([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const t = setTimeout(() => search(query), 300);
    return () => clearTimeout(t);
  }, [query, search]);

  const startChat = async (user) => {
    try {
      const { data } = await chatApi.createConversation(user.id);
      await loadConversations();
      openChat({ type: 'dm', id: data.id || data.conversationId, data });
      onClose();
    } catch (err) {
      console.error('Failed to create conversation:', err);
    }
  };

  return (
    <div className={styles.overlay} onClick={onClose}>
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <h2 className={styles.title}>New Message</h2>
          <button className={styles.closeBtn} onClick={onClose}>
            <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        <div className={styles.searchWrap}>
          <svg className={styles.searchIcon} width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
            <circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/>
          </svg>
          <input
            className={styles.searchInput}
            placeholder="Search users by name or email…"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            autoFocus
          />
        </div>

        <div className={styles.list}>
          {loading && <div className={styles.empty}>Searching…</div>}
          {!loading && query && results.length === 0 && (
            <div className={styles.empty}>No users found</div>
          )}
          {!loading && !query && (
            <div className={styles.empty}>Type a name to search for users</div>
          )}
          {results.map((u) => {
            const userName = u.userName || u.username || '?';
            const first = u.firstName?.trim();
            const last  = u.lastName?.trim();
            const fullName = [first, last].filter(Boolean).join(' ');
            const subtitle = fullName || u.email || '';
            return (
              <button key={u.id} className={styles.userItem} onClick={() => startChat(u)}>
                <Avatar name={userName} />
                <div className={styles.userInfo}>
                  <span className={styles.userName}>{userName}</span>
                  {subtitle && <span className={styles.userEmail}>{subtitle}</span>}
                </div>
                <svg width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" style={{color:'var(--text-muted)'}}>
                  <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
                </svg>
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}
