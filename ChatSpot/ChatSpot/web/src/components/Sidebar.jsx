import React, { useMemo, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useChat } from '../context/ChatContext';
import { formatDistanceToNow } from 'date-fns';
import Avatar from './Avatar';
import BrandMark from './BrandMark';
import styles from './Sidebar.module.css';

export default function Sidebar({ onNewChat, onNewGroup }) {
  const { user, logout } = useAuth();
  const { conversations, groups, activeChat, openChat } = useChat();
  const [tab, setTab] = useState('dms');
  const [search, setSearch] = useState('');

  const filtered = useMemo(() => {
    const q = search.toLowerCase();
    if (tab === 'dms') {
      return conversations
          .filter((c) => {
            const name = c.user?.userName || c.user?.email || '';
            return name.toLowerCase().includes(q) ||
                c.lastMessage?.toLowerCase().includes(q);
          })
          .sort((a, b) => {
            const ta = a.lastMessageDate ? new Date(a.lastMessageDate).getTime() : 0;
            const tb = b.lastMessageDate ? new Date(b.lastMessageDate).getTime() : 0;
            return tb - ta;
          });
    }
    return groups
        .filter((g) => g.name?.toLowerCase().includes(q))
        .sort((a, b) => {
          const ta = new Date(a.lastUpdateTime || a.createdAt || 0).getTime();
          const tb = new Date(b.lastUpdateTime || b.createdAt || 0).getTime();
          return tb - ta;
        });
  }, [tab, conversations, groups, search]);

  return (
      <aside className={styles.sidebar}>
        <div className={styles.header}>
          <div className={styles.logo}>
            <BrandMark size={22} className={styles.logoMark} />
            <span className={styles.logoText}>ChatSpot</span>
          </div>
          <div className={styles.headerActions}>
            <button className={styles.iconBtn} onClick={onNewChat} title="New message">
              <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
              </svg>
            </button>
            <button className={styles.iconBtn} onClick={onNewGroup} title="New group">
              <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
                <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
                <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
              </svg>
            </button>
          </div>
        </div>

        <div className={styles.searchWrap}>
          <svg className={styles.searchIcon} width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
            <circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/>
          </svg>
          <input
              className={styles.search}
              placeholder="Search conversations…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
          />
        </div>

        <div className={styles.tabs}>
          <button
              className={`${styles.tab} ${tab === 'dms' ? styles.activeTab : ''}`}
              onClick={() => setTab('dms')}
          >
            Messages
            {conversations.length > 0 && <span className={styles.count}>{conversations.length}</span>}
          </button>
          <button
              className={`${styles.tab} ${tab === 'groups' ? styles.activeTab : ''}`}
              onClick={() => setTab('groups')}
          >
            Groups
            {groups.length > 0 && <span className={styles.count}>{groups.length}</span>}
          </button>
        </div>

        <div className={styles.list}>
          {filtered.length === 0 && (
              <div className={styles.empty}>
                {search ? 'No results found' : tab === 'dms' ? 'No conversations yet' : 'No groups yet'}
              </div>
          )}

          {tab === 'dms' && filtered.map((conv) => {
            const otherName = conv.user?.userName || conv.user?.email || 'Unknown';
            const isOnline = conv.user?.status === 'Online';
            const isActive = activeChat?.id === conv.id;
            const unread = conv.unreadMessagesCount || 0;
            return (
                <button
                    key={conv.id}
                    className={`${styles.item} ${isActive ? styles.activeItem : ''}`}
                    onClick={() => openChat({ type: 'dm', id: conv.id, data: conv })}
                >
                  <Avatar name={otherName} online={isOnline} />
                  <div className={styles.itemContent}>
                    <div className={styles.itemTop}>
                      <span className={styles.itemName}>{otherName}</span>
                      {conv.lastMessageDate && (
                          <span className={styles.itemTime}>
                      {formatDistanceToNow(new Date(conv.lastMessageDate), { addSuffix: false })}
                    </span>
                      )}
                    </div>
                    <div className={styles.itemBottom}>
                      <span className={styles.itemPreview}>{conv.lastMessage || 'No messages yet'}</span>
                      {unread > 0 && <span className={styles.badge}>{unread}</span>}
                    </div>
                  </div>
                </button>
            );
          })}

          {tab === 'groups' && filtered.map((group) => {
            const isActive = activeChat?.id === group.groupId;
            const unread = group.unreadCount ?? group.unreadMessagesCount ?? 0;
            return (
                <button
                    key={group.groupId}
                    className={`${styles.item} ${isActive ? styles.activeItem : ''}`}
                    onClick={() => openChat({ type: 'group', id: group.groupId, data: group })}
                >
                  <Avatar name={group.name} src={group.avatarUrl} size={38} />
                  <div className={styles.itemContent}>
                    <div className={styles.itemTop}>
                      <span className={styles.itemName}>{group.name}</span>
                      {(group.lastUpdateTime || group.createdAt) && (
                          <span className={styles.itemTime}>
                      {formatDistanceToNow(new Date(group.lastUpdateTime || group.createdAt), { addSuffix: false })}
                    </span>
                      )}
                    </div>
                    <div className={styles.itemBottom}>
                      <span className={styles.itemPreview}>{group.lastMessage || 'No messages yet'}</span>
                      {unread > 0 && <span className={styles.badge}>{unread}</span>}
                    </div>
                  </div>
                </button>
            );
          })}
        </div>

        <div className={styles.userBar}>
          <Avatar name={user?.username} size={34} online={true} />
          <div className={styles.userInfo}>
            <span className={styles.userName}>{user?.username}</span>
            <span className={styles.userStatus}>Online</span>
          </div>
          <button className={styles.iconBtn} onClick={logout} title="Sign out">
            <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
              <polyline points="16 17 21 12 16 7"/>
              <line x1="21" y1="12" x2="9" y2="12"/>
            </svg>
          </button>
        </div>
      </aside>
  );
}