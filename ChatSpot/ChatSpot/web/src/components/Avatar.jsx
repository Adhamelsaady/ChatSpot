import React from 'react';
import styles from './Avatar.module.css';

const AVATAR_VARS = [
  'var(--avatar-1)',
  'var(--avatar-2)',
  'var(--avatar-3)',
  'var(--avatar-4)',
  'var(--avatar-5)',
];

export default function Avatar({ name, src, size = 38, online }) {
  const initials =
    name?.split(' ').map((n) => n[0]).join('').slice(0, 2).toUpperCase() || '?';
  const bg = AVATAR_VARS[(name?.charCodeAt(0) || 0) % AVATAR_VARS.length];
  const fontSize = Math.round(size * 0.34);

  return (
    <div
      className={styles.avatar}
      style={{
        width: size,
        height: size,
        background: src ? 'transparent' : bg,
        fontSize,
      }}
    >
      {src ? <img src={src} alt={name || ''} /> : <span>{initials}</span>}
      {online !== undefined && (
        <span
          className={`${styles.dot} ${online ? styles.online : styles.offline}`}
        />
      )}
    </div>
  );
}
