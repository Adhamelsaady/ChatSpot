import React, { useState, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { GoogleLogin } from '@react-oauth/google';
import { authApi } from '../api/client';
import { useAuth } from '../context/AuthContext';
import BrandMark from '../components/BrandMark';
import styles from './Auth.module.css';

export default function RegisterPage() {
  const { googleLogin } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', password: '', userName: '', bio: '' });
  const [profilePicture, setProfilePicture] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const fileInputRef = useRef(null);

  const handleFileChange = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setProfilePicture(file);
    const reader = new FileReader();
    reader.onloadend = () => setPreviewUrl(reader.result);
    reader.readAsDataURL(file);
  };

  const handleGoogleSuccess = async (credentialResponse) => {
    setError('');
    setLoading(true);
    try {
      await googleLogin(credentialResponse.credential);
      navigate('/');
    } catch (err) {
      const msg = err.response?.data;
      if (typeof msg === 'string') setError(msg);
      else if (Array.isArray(msg)) setError(msg.map(e => e.description || e).join(', '));
      else setError('Google sign up failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const formData = new FormData();
      formData.append('firstName', form.firstName);
      formData.append('lastName', form.lastName);
      formData.append('userName', form.userName);
      formData.append('email', form.email);
      formData.append('password', form.password);
      if (form.bio) formData.append('bio', form.bio);
      if (profilePicture) formData.append('profilePicture', profilePicture);

      await authApi.register(formData);
      navigate('/confirm-email', { state: { email: form.email } });
    } catch (err) {
      const msg = err.response?.data;
      if (typeof msg === 'string') setError(msg);
      else if (Array.isArray(msg)) setError(msg.map(e => e.description || e).join(', '));
      else setError('Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const initials = [form.firstName, form.lastName]
    .map(n => n?.[0] || '')
    .join('')
    .toUpperCase() || '?';

  return (
    <div className={styles.page}>
      <div className={styles.bg} aria-hidden />
      <div className={styles.card}>
        <div className={styles.logo}>
          <BrandMark size={28} className={styles.logoMark} />
          <span className={styles.logoText}>ChatSpot</span>
        </div>
        <h1 className={styles.title}>Create account</h1>
        <p className={styles.subtitle}>Join ChatSpot and start connecting</p>

        <form onSubmit={handleSubmit} className={styles.form}>
          {/* Avatar Picker */}
          <div className={styles.avatarPickerWrap}>
            <button
              type="button"
              className={styles.avatarPicker}
              onClick={() => fileInputRef.current?.click()}
              title="Upload profile picture"
            >
              {previewUrl ? (
                <img src={previewUrl} alt="Preview" className={styles.avatarPickerPreview} />
              ) : (
                <span className={styles.avatarPickerInitials}>{initials}</span>
              )}
              <span className={styles.avatarPickerOverlay}>
                <svg width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" aria-hidden>
                  <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/>
                  <circle cx="12" cy="13" r="4"/>
                </svg>
              </span>
            </button>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              onChange={handleFileChange}
              style={{ display: 'none' }}
            />
            <span className={styles.avatarPickerHint}>Upload photo</span>
          </div>

          <div className={styles.fieldRow}>
            <div className={styles.field}>
              <label className={styles.label}>First Name</label>
              <input
                className={styles.input}
                placeholder="John"
                value={form.firstName}
                onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                required
              />
            </div>
            <div className={styles.field}>
              <label className={styles.label}>Last Name</label>
              <input
                className={styles.input}
                placeholder="Doe"
                value={form.lastName}
                onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                required
              />
            </div>
          </div>
          <div className={styles.field}>
            <label className={styles.label}>Username</label>
            <input
              className={styles.input}
              placeholder="johndoe"
              value={form.userName}
              onChange={(e) => setForm({ ...form, userName: e.target.value })}
              minLength={2}
              required
            />
          </div>
          <div className={styles.field}>
            <label className={styles.label}>Email</label>
            <input
              type="email"
              className={styles.input}
              placeholder="you@example.com"
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              required
            />
          </div>
          <div className={styles.field}>
            <label className={styles.label}>Password</label>
            <input
              type="password"
              className={styles.input}
              placeholder="Min. 6 characters"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              minLength={6}
              required
            />
          </div>
          <div className={styles.field}>
            <label className={styles.label}>Bio <span style={{color:'var(--text-muted)'}}>optional</span></label>
            <input
              className={styles.input}
              placeholder="Tell us about yourself"
              value={form.bio}
              onChange={(e) => setForm({ ...form, bio: e.target.value })}
            />
          </div>
          {error && <p className={styles.error}>{error}</p>}
          <button type="submit" className={styles.btn} disabled={loading}>
            {loading ? <span className={styles.spinner} /> : 'Create Account'}
          </button>
        </form>

        <div className={styles.divider}>or sign up with</div>
        <div className={styles.googleLoginWrapper}>
          <GoogleLogin
            onSuccess={handleGoogleSuccess}
            onError={() => setError('Google Sign-Up failed')}
            locale="en"
          />
        </div>

        <p className={styles.footer}>
          Already have an account? <Link to="/login" className={styles.link}>Sign in</Link>
        </p>
      </div>
    </div>
  );
}
