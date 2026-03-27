import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import BrandMark from '../components/BrandMark';
import styles from './Auth.module.css';

function loginErrorMessage(err) {
  const d = err.response?.data;
  if (typeof d === 'string' && d.trim()) return d;
  if (d?.message) return d.message;
  if (d?.detail) return d.detail;
  if (d?.title && d?.errors) {
    const first = Object.values(d.errors).flat()[0];
    if (first) return Array.isArray(first) ? first[0] : String(first);
  }
  if (!err.response && (err.code === 'ERR_NETWORK' || err.message === 'Network Error')) {
    return 'Cannot reach the API. Start the ASP.NET project (Vite proxies to http://localhost:5215).';
  }
  return 'Invalid email/username or password';
}

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ emailOrUserName: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(form.emailOrUserName, form.password);
      navigate('/');
    } catch (err) {
      setError(loginErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={styles.page}>
      <div className={styles.bg} aria-hidden />
      <div className={styles.card}>
        <div className={styles.logo}>
          <BrandMark size={28} className={styles.logoMark} />
          <span className={styles.logoText}>ChatSpot</span>
        </div>
        <h1 className={styles.title}>Welcome back</h1>
        <p className={styles.subtitle}>Sign in to continue your conversations</p>

        <form onSubmit={handleSubmit} className={styles.form}>
          <div className={styles.field}>
            <label className={styles.label}>Email or Username</label>
            <input
              type="text"
              className={styles.input}
              placeholder="johndoe or john@example.com"
              value={form.emailOrUserName}
              onChange={(e) => setForm({ ...form, emailOrUserName: e.target.value })}
              required
            />
          </div>
          <div className={styles.field}>
            <label className={styles.label}>Password</label>
            <input
              type="password"
              className={styles.input}
              placeholder="••••••••"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              required
            />
          </div>
          {error && <p className={styles.error}>{error}</p>}
          <button type="submit" className={styles.btn} disabled={loading}>
            {loading ? <span className={styles.spinner} /> : 'Sign In'}
          </button>
        </form>

        <p className={styles.footer}>
          Don't have an account? <Link to="/register" className={styles.link}>Create one</Link>
        </p>
      </div>
    </div>
  );
}
