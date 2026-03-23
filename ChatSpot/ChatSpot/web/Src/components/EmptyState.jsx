import styles from './EmptyState.module.css';
import React from 'react';
export default function EmptyState() {
  return (
    <div className={styles.wrap}>
      <div className={styles.icon}>◈</div>
      <h2 className={styles.title}>ChatSpot</h2>
      <p className={styles.sub}>Select a conversation or start a new one</p>
      <div className={styles.hints}>
        <div className={styles.hint}>
          <span className={styles.hintKey}>Click</span> a conversation on the left
        </div>
        <div className={styles.hint}>
          <span className={styles.hintKey}>✎</span> icon to start a new message
        </div>
        <div className={styles.hint}>
          <span className={styles.hintKey}>⊞</span> icon to create a group
        </div>
      </div>
    </div>
  );
}
