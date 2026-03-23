import React, { useEffect, useState } from 'react';
import { groupApi } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useChat } from '../context/ChatContext';
import styles from './GroupInfoPanel.module.css';

const Avatar = ({ name, size = 32 }) => {
    const initials = name?.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase() || '?';
    const colors = ['#7c6af0','#6a8af0','#f06a8a','#f0a06a','#6af0a8'];
    const color = colors[(name?.charCodeAt(0) || 0) % colors.length];
    return (
        <div style={{
            width: size, height: size, borderRadius: '50%', background: color,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 11, fontWeight: 700, color: 'white', fontFamily: 'var(--font-display)', flexShrink: 0,
        }}>{initials}</div>
    );
};

// Role is a number: 0 = owner, 1 = admin, 2 = member
const roleMap = { 0: 'member', 1: 'admin', 2: 'owner' };
const roleBadge = { member: '' , admin: '🛡️', owner: '👑' };


export default function GroupInfoPanel({ groupId, onClose }) {
    const { user } = useAuth();
    const { loadGroups, openChat } = useChat();
    const [members, setMembers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [myRole, setMyRole] = useState(2);

    const load = async () => {
        setLoading(true);
        try {
            const { data } = await groupApi.getMembers(groupId, { PageNumber: 1, PageSize: 100 });
            const list = data.items || data.data || (Array.isArray(data) ? data : []);
            setMembers(list);
            const me = list.find(m => m.userId === user?.id);
            if (me) setMyRole(me.role ?? 2);
        } catch (e) {
            console.error(e);
            setMembers([]);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { load(); }, [groupId]);

    const handleRemove = async (userId) => {
        try {
            await groupApi.removeMember(groupId, userId);
            setMembers(prev => prev.filter(m => m.userId !== userId));
        } catch (e) { console.error(e); }
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

    const canManage = myRole === 2 || myRole === 1; // owner or admin

    return (
        <div className={styles.panel}>
            <div className={styles.panelHeader}>
                <span className={styles.panelTitle}>Members</span>
                <span className={styles.memberCount}>{members.length}</span>
            </div>

            <div className={styles.list}>
                {loading && <div className={styles.loading}>Loading…</div>}
                {!loading && members.length === 0 && <div className={styles.loading}>No members found</div>}
                {members.map((m) => {
                    const isMe = m.userId === user?.id;
                    const roleNum = m.role ?? 2;
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
                                <button className={styles.removeBtn} onClick={() => handleRemove(m.userId)} title="Remove">
                                    <svg width="12" height="12" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                                        <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                                    </svg>
                                </button>
                            )}
                        </div>
                    );
                })}
            </div>

            {myRole !== 2 && (
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
    );
}