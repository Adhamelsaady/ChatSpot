import React , { useState, useEffect, useCallback } from 'react';
import { userApi, groupApi } from '../api/client';
import { useChat } from '../context/ChatContext';
import styles from './Modal.module.css';

export default function CreateGroupModal({ onClose }) {
  const { loadGroups, openChat } = useChat();
  const [step, setStep] = useState(1);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  const [selected, setSelected] = useState([]);
  const [loading, setLoading] = useState(false);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState('');

  const search = useCallback(async (q) => {
    if (!q.trim()) { setResults([]); return; }
    setLoading(true);
    try {
      const { data } = await userApi.search({ SearchQuery: q, PageNumber: 1, PageSize: 20 });
      setResults(Array.isArray(data) ? data : data.items || data.data || []);
    } catch { setResults([]); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => {
    const t = setTimeout(() => search(query), 300);
    return () => clearTimeout(t);
  }, [query, search]);

  const toggleUser = (u) => {
    setSelected(prev =>
      prev.find(x => x.id === u.id) ? prev.filter(x => x.id !== u.id) : [...prev, u]
    );
  };

  const handleCreate = async () => {
    if (!name.trim()) { setError('Group name is required'); return; }
    setCreating(true);
    setError('');
    try {
      const { data } = await groupApi.createGroup({
        name: name.trim(),
        description: description.trim() || null,
        members: selected.map(u => u.id),
      });
      await loadGroups();
      openChat({ type: 'group', id: data.groupId || data.id, data });
      onClose();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create group');
    } finally {
      setCreating(false);
    }
  };

  return (
    <div className={styles.overlay} onClick={onClose}>
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <h2 className={styles.title}>Create Group</h2>
          <button className={styles.closeBtn} onClick={onClose}>
            <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        {/* Step 1: Group info */}
        {step === 1 && (
          <div className={styles.body}>
            <div className={styles.field}>
              <label className={styles.label}>Group Name *</label>
              <input
                className={styles.input}
                placeholder="e.g. Team Alpha"
                value={name}
                onChange={(e) => setName(e.target.value)}
                autoFocus
              />
            </div>
            <div className={styles.field}>
              <label className={styles.label}>Description <span style={{color:'var(--text-muted)'}}>optional</span></label>
              <input
                className={styles.input}
                placeholder="What's this group about?"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </div>
            {error && <p className={styles.error}>{error}</p>}
            <button className={styles.btn} onClick={() => { if (!name.trim()) { setError('Name required'); return; } setError(''); setStep(2); }}>
              Next: Add Members →
            </button>
          </div>
        )}

        {/* Step 2: Add members */}
        {step === 2 && (
          <div className={styles.body}>
            {selected.length > 0 && (
              <div className={styles.chips}>
                {selected.map(u => (
                  <div key={u.id} className={styles.chip}>
                    {u.userName || u.username}
                    <button onClick={() => toggleUser(u)}>×</button>
                  </div>
                ))}
              </div>
            )}

            <div className={styles.searchWrap}>
              <svg className={styles.searchIcon} width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                <circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/>
              </svg>
              <input
                className={styles.searchInput}
                placeholder="Search users to add…"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                autoFocus
              />
            </div>

            <div className={styles.list}>
              {loading && <div className={styles.empty}>Searching…</div>}
              {!loading && !query && <div className={styles.empty}>Search for users to add</div>}
              {results.map((u) => {
                const isSelected = !!selected.find(x => x.id === u.id);
                return (
                  <button
                    key={u.id}
                    className={`${styles.userItem} ${isSelected ? styles.selectedUser : ''}`}
                    onClick={() => toggleUser(u)}
                  >
                    <div className={styles.checkBox}>
                      {isSelected && <svg width="10" height="10" fill="none" stroke="currentColor" strokeWidth="3" viewBox="0 0 24 24"><polyline points="20 6 9 17 4 12"/></svg>}
                    </div>
                    <div className={styles.userInfo}>
                      <span className={styles.userName}>{u.userName || u.username}</span>
                      <span className={styles.userEmail}>{u.email}</span>
                    </div>
                  </button>
                );
              })}
            </div>

            {error && <p className={styles.error}>{error}</p>}
            <div style={{ display: 'flex', gap: 10 }}>
              <button className={styles.btnSecondary} onClick={() => setStep(1)}>← Back</button>
              <button className={styles.btn} onClick={handleCreate} disabled={creating} style={{ flex: 1 }}>
                {creating ? 'Creating…' : `Create Group${selected.length > 0 ? ` (${selected.length} members)` : ''}`}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
