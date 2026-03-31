import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '../api/client';
import BrandMark from '../components/BrandMark';
import styles from './Auth.module.css';

const STEP_EMAIL = 'email';
const STEP_RESET = 'reset';
const STEP_DONE  = 'done';

function extractError(err) {
  const d = err.response?.data;
  if (typeof d === 'string' && d.trim()) return d;
  if (d?.message) return d.message;
  if (!err.response && (err.code === 'ERR_NETWORK' || err.message === 'Network Error'))
    return 'Cannot reach the server. Make sure the API is running.';
  return 'Something went wrong. Please try again.';
}

export default function ForgotPasswordPage() {
  const navigate = useNavigate();
  const [step, setStep]       = useState(STEP_EMAIL);
  const [email, setEmail]     = useState('');
  const [otp, setOtp]         = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError]     = useState('');
  const [loading, setLoading] = useState(false);

  /* ── Step 1: send OTP ─────────────────────────────────────────── */
  const handleSendOtp = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await authApi.forgotPassword(email);
      setStep(STEP_RESET);
    } catch (err) {
      setError(extractError(err));
    } finally {
      setLoading(false);
    }
  };

  /* ── Step 2: reset password ───────────────────────────────────── */
  const handleReset = async (e) => {
    e.preventDefault();
    setError('');
    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }
    setLoading(true);
    try {
      await authApi.resetPassword({ email, otp, newPassword });
      setStep(STEP_DONE);
    } catch (err) {
      setError(extractError(err));
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

        {/* ── STEP: email ── */}
        {step === STEP_EMAIL && (
          <>
            <h1 className={styles.title}>Forgot password?</h1>
            <p className={styles.subtitle}>
              Enter your email and we'll send you a reset code.
            </p>
            <form onSubmit={handleSendOtp} className={styles.form}>
              <div className={styles.field}>
                <label className={styles.label}>Email address</label>
                <input
                  id="fp-email"
                  type="email"
                  className={styles.input}
                  placeholder="john@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  autoFocus
                />
              </div>
              {error && <p className={styles.error}>{error}</p>}
              <button type="submit" className={styles.btn} disabled={loading}>
                {loading ? <span className={styles.spinner} /> : 'Send Reset Code'}
              </button>
            </form>
          </>
        )}

        {/* ── STEP: reset ── */}
        {step === STEP_RESET && (
          <>
            <h1 className={styles.title}>Reset password</h1>
            <p className={styles.subtitle}>
              Enter the 6-character code sent to <strong>{email}</strong> and choose a new password.
            </p>
            <form onSubmit={handleReset} className={styles.form}>
              <div className={styles.field}>
                <label className={styles.label}>Verification code</label>
                <input
                  id="fp-otp"
                  type="text"
                  className={`${styles.input} ${styles.otpInput}`}
                  placeholder="A1b2C3"
                  maxLength={6}
                  value={otp}
                  onChange={(e) => setOtp(e.target.value)}
                  required
                  autoFocus
                />
              </div>
              <div className={styles.field}>
                <label className={styles.label}>New password</label>
                <input
                  id="fp-new-password"
                  type="password"
                  className={styles.input}
                  placeholder="••••••••"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  required
                />
              </div>
              <div className={styles.field}>
                <label className={styles.label}>Confirm new password</label>
                <input
                  id="fp-confirm-password"
                  type="password"
                  className={styles.input}
                  placeholder="••••••••"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  required
                />
              </div>
              {error && <p className={styles.error}>{error}</p>}
              <button type="submit" className={styles.btn} disabled={loading}>
                {loading ? <span className={styles.spinner} /> : 'Reset Password'}
              </button>
              <button
                type="button"
                className={styles.btn}
                style={{ background: 'var(--bg-elevated)', color: 'var(--text-secondary)', marginTop: 0 }}
                onClick={() => { setStep(STEP_EMAIL); setError(''); setOtp(''); }}
              >
                ← Back
              </button>
            </form>
          </>
        )}

        {/* ── STEP: done ── */}
        {step === STEP_DONE && (
          <>
            <h1 className={styles.title}>All done! 🎉</h1>
            <div className={styles.successBox}>
              Your password has been reset successfully.
              <br />You can now sign in with your new password.
            </div>
            <button
              className={styles.btn}
              onClick={() => navigate('/login')}
              style={{ marginTop: 8 }}
            >
              Go to Sign In
            </button>
          </>
        )}

        {step !== STEP_DONE && (
          <p className={styles.footer}>
            Remember it? <Link to="/login" className={styles.link}>Sign in</Link>
          </p>
        )}
      </div>
    </div>
  );
}
