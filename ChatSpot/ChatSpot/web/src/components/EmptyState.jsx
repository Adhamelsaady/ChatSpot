import BrandMark from './BrandMark';
import styles from './EmptyState.module.css';
import React from 'react';

export default function EmptyState() {
  return (
    <div className={styles.wrap}>
      <BrandMark size={56} className={styles.mark} />
      <h2 className={styles.title}>ChatSpot</h2>
      <p className={styles.sub}>Select a conversation or start a new one</p>
      <div className={styles.hints}>
        <div className={styles.hint}>
          <span className={styles.hintKey}>Tap</span> a conversation in the list
        </div>
        <div className={styles.hint}>
          <span className={styles.hintKey}>New</span> message from the toolbar
        </div>
        <div className={styles.hint}>
          <span className={styles.hintKey}>Group</span> icon to create a group
        </div>
      </div>
    </div>
  );
}
