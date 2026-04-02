import React , { useState, useEffect, useCallback } from 'react';
import { userApi, chatApi } from '../api/client';
import { useChat } from '../context/ChatContext';
import styles from './Modal.module.css';

const Avatar = ({ name, src, size = 36 }) => {
  const initials = name?.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase() || '?';
  const colors = ['#7c6af0','#6a8af0','#f06a8a','#f0a06a','#6af0a8'];
  const color = colors[(name?.charCodeAt(0) || 0) % colors.length];
  if (src) {
    return (
      <div style={{
        width: size, height: size, borderRadius: '50%', overflow: 'hidden', flexShrink: 0,
      }}>
        <img src={src} alt={name || ''} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
      </div>
    );
  }
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
  const [selectedUser, setSelectedUser] = useState(null);
  const [profile, setProfile] = useState(null);
  const [profileLoading, setProfileLoading] = useState(false);
  const [profileError, setProfileError] = useState(null);
  const [startingChat, setStartingChat] = useState(false);

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
    if (selectedUser) return undefined;
    const t = setTimeout(() => search(query), 300);
    return () => clearTimeout(t);
  }, [query, search, selectedUser]);

  const startChat = async (user) => {
    try {
      if (!user?.id) return;
      setStartingChat(true);
      const { data } = await chatApi.createConversation(user.id);
      await loadConversations();
      openChat({
        type: 'dm',
        id: data.id || data.conversationId,
        data: { ...data, user },
      });
      onClose();
    } catch (err) {
      console.error('Failed to create conversation:', err);
    } finally {
      setStartingChat(false);
    }
  };

  const viewProfile = async (u) => {
    if (!u?.id) return;
    setSelectedUser(u);
    setProfile(null);
    setProfileError(null);
    setProfileLoading(true);
    try {
      const { data } = await userApi.getById(u.id);
      setProfile(data);
    } catch {
      // Fall back to the search result if profile fetch fails.
      setProfile(u);
      setProfileError('Could not load profile details. You can still message this user.');
    } finally {
      setProfileLoading(false);
    }
  };

  const backToSearch = () => {
    setSelectedUser(null);
    setProfile(null);
    setProfileLoading(false);
    setProfileError(null);
  };

  const displayUser = profile || selectedUser;
  const displayFirst = displayUser?.firstName?.trim() || '';
  const displayLast = displayUser?.lastName?.trim() || '';
  const displayUserName = displayUser?.userName?.trim() || '';
  const displayName =
    [displayFirst, displayLast].filter(Boolean).join(' ') || displayUserName || 'Unknown';
  const subtitle = displayUser?.email || '';

  let lastSeenText = '';
  if (displayUser?.lastSeen) {
    const d = new Date(displayUser.lastSeen);
    lastSeenText = Number.isNaN(d.getTime()) ? String(displayUser.lastSeen) : d.toLocaleString();
  }

  return (
    <div className={styles.overlay} onClick={onClose}>
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <h2 className={styles.title}>{selectedUser ? 'User Profile' : 'New Message'}</h2>
          <button className={styles.closeBtn} onClick={onClose}>
            <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        {selectedUser && (
          <div className={styles.body}>
            <div style={{ display: 'flex', gap: 14, alignItems: 'center' }}>
              <Avatar name={displayName} src={displayUser?.profilePicture} size={56} />
              <div style={{ minWidth: 0 }}>
                <div className={styles.userName} style={{ fontSize: 15 }}>
                  {displayName}
                </div>
                {subtitle && <div className={styles.userEmail}>{subtitle}</div>}
                {displayUser?.status && (
                  <div className={styles.userEmail} style={{ marginTop: 2 }}>
                    Status: {displayUser.status}
                  </div>
                )}
                {lastSeenText && (
                  <div className={styles.userEmail} style={{ marginTop: 2 }}>
                    Last seen: {lastSeenText}
                  </div>
                )}
              </div>
            </div>

            {profileLoading ? (
              <div className={styles.empty} style={{ paddingTop: 6 }}>
                Loading profile…
              </div>
            ) : (
              <>
                <div style={{ height: 1, background: 'var(--border)', margin: '12px 0' }} />
                <div className={styles.userEmail} style={{ color: 'var(--text-secondary)' }}>
                  Bio
                </div>
                <div className={styles.userEmail} style={{ whiteSpace: 'pre-wrap', lineHeight: 1.35 }}>
                  {displayUser?.bio ? displayUser.bio : 'No bio yet.'}
                </div>

                {profileError && (
                  <div className={styles.error} style={{ marginTop: 10 }}>
                    {profileError}
                  </div>
                )}

                <div style={{ display: 'flex', gap: 10, marginTop: 14 }}>
                  <button
                    className={styles.btnSecondary}
                    onClick={backToSearch}
                    disabled={startingChat}
                    style={{ width: '50%' }}
                  >
                    Back
                  </button>
                  <button
                    className={styles.btn}
                    onClick={() => startChat(displayUser)}
                    disabled={startingChat}
                    style={{ width: '50%' }}
                  >
                    {startingChat ? 'Starting…' : 'Message'}
                  </button>
                </div>
              </>
            )}
          </div>
        )}

        {!selectedUser && (
          <>
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
              <button key={u.id} className={styles.userItem} onClick={() => viewProfile(u)}>
              <Avatar name={userName} src={u.profilePicture} />
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
          </>
        )}
      </div>
    </div>
  );
}
