import React, { useState, useEffect, useRef } from 'react';
import { userApi } from '../api/client';
import { useAuth } from '../context/AuthContext';
import styles from './ProfileModal.module.css';
import modalStyles from './Modal.module.css';

export default function ProfileModal({ onClose }) {
  const { user, setUser } = useAuth();

  // ─── Profile tab state ───────────────────────────────────────────────────
  const [firstName, setFirstName]   = useState('');
  const [lastName,  setLastName]    = useState('');
  const [bio,       setBio]         = useState('');
  const [avatarFile,  setAvatarFile]  = useState(null);
  const [avatarPreview, setAvatarPreview] = useState('');
  const [profileLoading, setProfileLoading] = useState(false);
  const [profileMsg,     setProfileMsg]     = useState(null); // { ok, text }

  // ─── Security tab state ──────────────────────────────────────────────────
  const [currentPw, setCurrentPw] = useState('');
  const [newPw,     setNewPw]     = useState('');
  const [confirmPw, setConfirmPw] = useState('');
  const [pwLoading, setPwLoading] = useState(false);
  const [pwMsg,     setPwMsg]     = useState(null);

  const [tab, setTab] = useState('profile');
  const fileRef = useRef();

  // ─── Fetch current profile ───────────────────────────────────────────────
  useEffect(() => {
    (async () => {
      try {
        const { data } = await userApi.getMe();
        setFirstName(data.firstName || '');
        setLastName(data.lastName  || '');
        setBio(data.bio            || '');
        setAvatarPreview(data.profilePicture || '');
      } catch {/* silently ignore */}
    })();
  }, []);

  // ─── Avatar picker ───────────────────────────────────────────────────────
  const handleAvatarChange = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setAvatarFile(file);
    setAvatarPreview(URL.createObjectURL(file));
  };

  // ─── Save profile ────────────────────────────────────────────────────────
  const handleSaveProfile = async () => {
    setProfileLoading(true);
    setProfileMsg(null);
    try {
      const fd = new FormData();
      fd.append('FirstName', firstName.trim());
      fd.append('LastName',  lastName.trim());
      fd.append('Bio',       bio.trim());
      if (avatarFile) fd.append('ProfilePicture', avatarFile);

      const { data } = await userApi.updateMe(fd);
      setProfileMsg({ ok: true, text: data.message || 'Profile updated!' });

      // Patch AuthContext so sidebar/avatar reflect new values immediately
      setUser((prev) => ({
        ...prev,
        profilePicture: data.data || prev.profilePicture,
      }));
      if (data.data) localStorage.setItem('profilePicture', data.data);
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to update profile.';
      setProfileMsg({ ok: false, text: msg });
    } finally {
      setProfileLoading(false);
    }
  };

  // ─── Change password ─────────────────────────────────────────────────────
  const handleChangePassword = async () => {
    if (newPw !== confirmPw) {
      setPwMsg({ ok: false, text: 'New passwords do not match.' });
      return;
    }
    if (newPw.length < 6) {
      setPwMsg({ ok: false, text: 'New password must be at least 6 characters.' });
      return;
    }
    setPwLoading(true);
    setPwMsg(null);
    try {
      const { data } = await userApi.changePassword({
        currentPassword: currentPw,
        newPassword: newPw,
      });
      setPwMsg({ ok: true, text: data.message || 'Password changed!' });
      setCurrentPw(''); setNewPw(''); setConfirmPw('');
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to change password.';
      setPwMsg({ ok: false, text: msg });
    } finally {
      setPwLoading(false);
    }
  };

  // ─── Initials fallback ───────────────────────────────────────────────────
  const initials = (user?.username || '?').slice(0, 2).toUpperCase();

  return (
    <div className={modalStyles.overlay} onClick={onClose}>
      <div className={styles.panel} onClick={(e) => e.stopPropagation()}>

        {/* Header */}
        <div className={styles.header}>
          <h2 className={styles.title}>My Profile</h2>
          <button className={modalStyles.closeBtn} onClick={onClose} aria-label="Close">
            <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        {/* Tabs */}
        <div className={styles.tabs}>
          <button
            className={`${styles.tab} ${tab === 'profile' ? styles.activeTab : ''}`}
            onClick={() => { setTab('profile'); setProfileMsg(null); }}
          >
            <svg width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>
            </svg>
            Profile
          </button>
          <button
            className={`${styles.tab} ${tab === 'security' ? styles.activeTab : ''}`}
            onClick={() => { setTab('security'); setPwMsg(null); }}
          >
            <svg width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>
            </svg>
            Security
          </button>
        </div>

        {/* ── Profile Tab ─────────────────────────────────────────────────── */}
        {tab === 'profile' && (
          <div className={styles.body}>
            {/* Avatar picker */}
            <div className={styles.avatarSection}>
              <div className={styles.avatarWrap} onClick={() => fileRef.current?.click()}>
                {avatarPreview
                  ? <img src={avatarPreview} alt="avatar" className={styles.avatarImg} />
                  : <div className={styles.avatarFallback}>{initials}</div>
                }
                <div className={styles.avatarOverlay}>
                  <svg width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                    <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/>
                    <circle cx="12" cy="13" r="4"/>
                  </svg>
                </div>
              </div>
              <input
                ref={fileRef}
                type="file"
                accept=".jpg,.jpeg,.png,.webp"
                style={{ display: 'none' }}
                onChange={handleAvatarChange}
              />
              <div className={styles.avatarHint}>
                <span className={styles.avatarUsername}>@{user?.username}</span>
                <span className={styles.avatarTip}>Click avatar to change</span>
              </div>
            </div>

            {/* Name row */}
            <div className={styles.row}>
              <div className={modalStyles.field}>
                <label className={modalStyles.label}>First Name</label>
                <input
                  className={modalStyles.input}
                  placeholder="First name"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                />
              </div>
              <div className={modalStyles.field}>
                <label className={modalStyles.label}>Last Name</label>
                <input
                  className={modalStyles.input}
                  placeholder="Last name"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                />
              </div>
            </div>

            {/* Bio */}
            <div className={modalStyles.field}>
              <label className={modalStyles.label}>
                Bio <span style={{ color: 'var(--text-muted)', textTransform: 'none', fontWeight: 400 }}>optional</span>
              </label>
              <textarea
                className={styles.textarea}
                placeholder="Tell people a little about yourself…"
                value={bio}
                maxLength={300}
                onChange={(e) => setBio(e.target.value)}
                rows={3}
              />
              <span className={styles.charCount}>{bio.length}/300</span>
            </div>

            {profileMsg && (
              <div className={profileMsg.ok ? styles.success : modalStyles.error}>
                {profileMsg.text}
              </div>
            )}

            <button
              className={modalStyles.btn}
              onClick={handleSaveProfile}
              disabled={profileLoading}
            >
              {profileLoading ? 'Saving…' : 'Save Profile'}
            </button>
          </div>
        )}

        {/* ── Security Tab ─────────────────────────────────────────────────── */}
        {tab === 'security' && (
          <div className={styles.body}>
            <div className={modalStyles.field}>
              <label className={modalStyles.label}>Current Password</label>
              <input
                className={modalStyles.input}
                type="password"
                placeholder="Enter current password"
                value={currentPw}
                onChange={(e) => setCurrentPw(e.target.value)}
                autoComplete="current-password"
              />
            </div>
            <div className={styles.divider} />
            <div className={modalStyles.field}>
              <label className={modalStyles.label}>New Password</label>
              <input
                className={modalStyles.input}
                type="password"
                placeholder="Min. 6 characters"
                value={newPw}
                onChange={(e) => setNewPw(e.target.value)}
                autoComplete="new-password"
              />
            </div>
            <div className={modalStyles.field}>
              <label className={modalStyles.label}>Confirm New Password</label>
              <input
                className={`${modalStyles.input} ${confirmPw && confirmPw !== newPw ? styles.inputError : ''}`}
                type="password"
                placeholder="Repeat new password"
                value={confirmPw}
                onChange={(e) => setConfirmPw(e.target.value)}
                autoComplete="new-password"
              />
              {confirmPw && confirmPw !== newPw && (
                <span className={styles.fieldError}>Passwords don't match</span>
              )}
            </div>

            {pwMsg && (
              <div className={pwMsg.ok ? styles.success : modalStyles.error}>
                {pwMsg.text}
              </div>
            )}

            <button
              className={modalStyles.btn}
              onClick={handleChangePassword}
              disabled={pwLoading || !currentPw || !newPw || !confirmPw}
            >
              {pwLoading ? 'Changing…' : 'Change Password'}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
