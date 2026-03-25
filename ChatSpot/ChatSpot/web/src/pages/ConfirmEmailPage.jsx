import { Link, useNavigate, useLocation } from 'react-router-dom';
import { authApi } from '../api/client';
import BrandMark from '../components/BrandMark';
import styles from './Auth.module.css';
import React, { useState } from 'react';
export default function ConfirmEmailPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState(location.state?.email || '');
  const [otp, setOtp] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await authApi.confirmEmail({ email, otp });
      setSuccess(true);
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      setError(err.response?.data?.message || err.response?.data || 'Invalid OTP. Please try again.');
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
        <h1 className={styles.title}>Verify your email</h1>
        <p className={styles.subtitle}>Enter the OTP code sent to your email</p>

        {success ? (
          <div className={styles.successBox}>
            ✓ Email verified! Redirecting to login…
          </div>
        ) : (
          <form onSubmit={handleSubmit} className={styles.form}>
            <div className={styles.field}>
              <label className={styles.label}>Email</label>
              <input
                type="email"
                className={styles.input}
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div className={styles.field}>
              <label className={styles.label}>OTP Code</label>
              <input
                className={`${styles.input} ${styles.otpInput}`}
                placeholder="Enter your code"
                value={otp}
                onChange={(e) => setOtp(e.target.value)}
                required
              />
            </div>
            {error && <p className={styles.error}>{error}</p>}
            <button type="submit" className={styles.btn} disabled={loading}>
              {loading ? <span className={styles.spinner} /> : 'Verify Email'}
            </button>
          </form>
        )}

        <p className={styles.footer}>
          <Link to="/login" className={styles.link}>← Back to login</Link>
        </p>
      </div>
    </div>
  );
}
