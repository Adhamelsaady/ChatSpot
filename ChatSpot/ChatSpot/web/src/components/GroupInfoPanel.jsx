import React, { useEffect, useState, useCallback } from 'react';
import { groupApi, userApi } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useChat } from '../context/ChatContext';
import { useMediaQuery } from '../hooks/useMediaQuery';
import Avatar from './Avatar';
import styles from './GroupInfoPanel.module.css';

const roleMap = { 0: 'member', 1: 'admin', 2: 'owner' };
const roleBadge = { owner: '👑', admin: '🛡️', member: '' };

export default function GroupInfoPanel({ groupId, onClose }) {
    const { user } = useAuth();
    const { loadGroups, openChat } = useChat();
    const isNarrow = useMediaQuery('(max-width: 767px)');
    const [members, setMembers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [myRole, setMyRole] = useState(0);

    // Add members state
    const [showAddMembers, setShowAddMembers] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const [searchResults, setSearchResults] = useState([]);
    const [selectedUsers, setSelectedUsers] = useState([]);
    const [searching, setSearching] = useState(false);
    const [adding, setAdding] = useState(false);

    const load = async () => {
        setLoading(true);
        try {
            const { data } = await groupApi.getMembers(groupId, { PageNumber: 1, PageSize: 100 });
            const list = data.items || data.data || (Array.isArray(data) ? data : []);
            setMembers(list);
            const me = list.find(m => m.userId === user?.id);
            if (me) setMyRole(me.role ?? 0);
        } catch (e) {
            console.error(e);
            setMembers([]);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { load(); }, [groupId]);

    // Search users
    const searchUsers = useCallback(async (q) => {
        if (!q.trim()) { setSearchResults([]); return; }
        setSearching(true);
        try {
            const { data } = await userApi.search({ SearchQuery: q, PageNumber: 1, PageSize: 20 });
            const results = Array.isArray(data) ? data : data.items || data.data || [];
            // Filter out already existing members
            const memberIds = members.map(m => m.userId);
            setSearchResults(results.filter(u => !memberIds.includes(u.id)));
        } catch { setSearchResults([]); }
        finally { setSearching(false); }
    }, [members]);

    useEffect(() => {
        const t = setTimeout(() => searchUsers(searchQuery), 300);
        return () => clearTimeout(t);
    }, [searchQuery, searchUsers]);

    const toggleUser = (u) => {
        setSelectedUsers(prev =>
            prev.find(x => x.id === u.id) ? prev.filter(x => x.id !== u.id) : [...prev, u]
        );
    };

    const handleAddMembers = async () => {
        if (selectedUsers.length === 0) return;
        setAdding(true);
        try {
            await groupApi.addMembers(groupId, selectedUsers.map(u => u.id));
            setShowAddMembers(false);
            setSelectedUsers([]);
            setSearchQuery('');
            await load(); // refresh member list
        } catch (e) {
            console.error(e);
        } finally {
            setAdding(false);
        }
    };

    const handleRemove = async (userId) => {
        try {
            await groupApi.removeMember(groupId, userId);
            setMembers(prev => prev.filter(m => m.userId !== userId));
        } catch (e) { console.error(e); }
    };
    const handleToggleRole = async (userId) => {
        try {
            await groupApi.changeRole(groupId, userId);
            await load(); 
        } catch (e) {
            console.error(e);
        }
    };

    const handleLeave = async () => {
        if (!confirm('Leave this group?')) return;
        try {
            await groupApi.leaveGroup(groupId);
            await loadGroups();
            openChat(null);
            onClose();
        } catch (e) { console.error(e); }
    };

    const canManage = myRole === 2 || myRole === 1;

    const panelClass = isNarrow ? `${styles.panel} ${styles.panelSheet}` : styles.panel;

    return (
        <>
            {isNarrow && (
                <button
                    type="button"
                    className={styles.backdrop}
                    onClick={onClose}
                    aria-label="Close members panel"
                />
            )}
            <div className={panelClass}>
            <div className={styles.panelHeader}>
                <span className={styles.panelTitle}>Members</span>
                <span className={styles.memberCount}>{members.length}</span>
                <span className={styles.headerSpacer} aria-hidden />
                {isNarrow && (
                    <button
                        type="button"
                        className={styles.sheetClose}
                        onClick={onClose}
                        aria-label="Close"
                    >
                        <svg width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                        </svg>
                    </button>
                )}
                {canManage && !showAddMembers && (
                    <button className={styles.addBtn} onClick={() => setShowAddMembers(true)} title="Add members">
                        <svg width="13" height="13" fill="none" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24">
                            <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
                        </svg>
                    </button>
                )}
            </div>

            {/* Add members panel */}
            {showAddMembers && (
                <div className={styles.addMembersSection}>
                    <div className={styles.addMembersHeader}>
                        <span className={styles.addMembersTitle}>Add Members</span>
                        <button className={styles.cancelBtn} onClick={() => { setShowAddMembers(false); setSelectedUsers([]); setSearchQuery(''); }}>
                            Cancel
                        </button>
                    </div>

                    {selectedUsers.length > 0 && (
                        <div className={styles.chips}>
                            {selectedUsers.map(u => (
                                <div key={u.id} className={styles.chip}>
                                    {u.userName || u.username}
                                    <button onClick={() => toggleUser(u)}>×</button>
                                </div>
                            ))}
                        </div>
                    )}

                    <div className={styles.searchWrap}>
                        <svg className={styles.searchIcon} width="12" height="12" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                            <circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/>
                        </svg>
                        <input
                            className={styles.searchInput}
                            placeholder="Search users…"
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            autoFocus
                        />
                    </div>

                    <div className={styles.searchResults}>
                        {searching && <div className={styles.loading}>Searching…</div>}
                        {!searching && searchQuery && searchResults.length === 0 && (
                            <div className={styles.loading}>No users found</div>
                        )}
                        {searchResults.map(u => {
                            const isSelected = !!selectedUsers.find(x => x.id === u.id);
                            return (
                                <button
                                    key={u.id}
                                    className={`${styles.userItem} ${isSelected ? styles.selectedUser : ''}`}
                                    onClick={() => toggleUser(u)}
                                >
                                    <div className={styles.checkBox}>
                                        {isSelected && <svg width="9" height="9" fill="none" stroke="currentColor" strokeWidth="3" viewBox="0 0 24 24"><polyline points="20 6 9 17 4 12"/></svg>}
                                    </div>
                                    <Avatar name={u.userName || u.username} size={26} />
                                    <span className={styles.userItemName}>{u.userName || u.username}</span>
                                </button>
                            );
                        })}
                    </div>

                    {selectedUsers.length > 0 && (
                        <button className={styles.confirmAddBtn} onClick={handleAddMembers} disabled={adding}>
                            {adding ? 'Adding…' : `Add ${selectedUsers.length} member${selectedUsers.length > 1 ? 's' : ''}`}
                        </button>
                    )}
                </div>
            )}

            {/* Members list */}
            {!showAddMembers && (
                <div className={styles.list}>
                    {loading && <div className={styles.loading}>Loading…</div>}
                    {!loading && members.length === 0 && <div className={styles.loading}>No members found</div>}
                    {members.map((m) => {
                        const isMe = m.userId === user?.id;
                        const roleNum = m.role ?? 0;
                        const roleName = roleMap[roleNum] || 'member';
                        return (
                            <div key={m.userId} className={styles.member}>
                                <Avatar name={m.userName || m.userId} size={32} />
                                <div className={styles.memberInfo}>
                  <span className={styles.memberName}>
                    {m.userName || m.userId}
                      {isMe && <span className={styles.you}> you</span>}
                  </span>
                                    <span className={styles.memberRole}>
                    {roleBadge[roleName]} {roleName}
                  </span>
                                </div>
                                {canManage && !isMe && roleNum !== 2 && (
                                    <div className={styles.memberActions}>
                                        <button
                                            className={styles.roleBtn}
                                            onClick={() => handleToggleRole(m.userId)}
                                            title={roleNum === 1 ? 'Demote to member' : 'Promote to admin'}
                                        >
                                            {roleNum === 1 ? '🛡️→' : '→🛡️'}
                                        </button>
                                        <button className={styles.removeBtn} onClick={() => handleRemove(m.userId)} title="Remove">
                                            <svg width="12" height="12" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                                                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                                            </svg>
                                        </button>
                                    </div>
                                )}
                            </div>
                        );
                    })}
                </div>
            )}

            {myRole !== 2 && !showAddMembers && (
                <button className={styles.leaveBtn} onClick={handleLeave}>
                    <svg width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                        <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
                        <polyline points="16 17 21 12 16 7"/>
                        <line x1="21" y1="12" x2="9" y2="12"/>
                    </svg>
                    Leave Group
                </button>
            )}
        </div>
        </>
    );
}