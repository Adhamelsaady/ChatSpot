import React , { useState } from 'react';
import { format } from 'date-fns';
import { useAuth } from '../context/AuthContext';
import styles from './MessageBubble.module.css';

const Avatar = ({ name, size = 28 }) => {
  const initials = name?.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase() || '?';
  const colors = ['#7c6af0','#6a8af0','#f06a8a','#f0a06a','#6af0a8'];
  const color = colors[(name?.charCodeAt(0) || 0) % colors.length];
  return (
    <div style={{
      width: size, height: size, borderRadius: '50%',
      background: color, display: 'flex', alignItems: 'center', justifyContent: 'center',
      fontSize: 10, fontWeight: 700, color: 'white', fontFamily: 'var(--font-display)', flexShrink: 0,
    }}>{initials}</div>
  );
};

export default function MessageBubble({ message, isMine, showAvatar, onDelete, onReply }) {
  const { user } = useAuth();
  const [hovered, setHovered] = useState(false);

  const time = message.timestamp
    ? format(new Date(message.timestamp), 'HH:mm')
    : '';

  if (message.isDeleted) {
    return (
      <div className={`${styles.row} ${isMine ? styles.mine : styles.theirs}`}>
        <div className={styles.deleted}>
          <svg width="12" height="12" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
            <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
          </svg>
          Message deleted
        </div>
      </div>
    );
  }

  return (
    <div
      className={`${styles.row} ${isMine ? styles.mine : styles.theirs}`}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
    >
      {!isMine && (
        <div className={styles.avatarSlot}>
          {showAvatar ? <Avatar name={message.senderName || message.senderId} /> : <div style={{ width: 28 }} />}
        </div>
      )}

      <div className={styles.bubbleWrap}>
        {!isMine && showAvatar && (
          <span className={styles.senderName}>{message.senderName || message.senderId}</span>
        )}

        {/* Reply preview */}
        {message.replyToPreview && (
          <div className={`${styles.replyQuote} ${isMine ? styles.replyMine : ''}`}>
            <span className={styles.replyBar} />
            <span className={styles.replyQuoteText}>{message.replyToPreview}</span>
          </div>
        )}

        <div className={`${styles.bubble} ${isMine ? styles.bubbleMine : styles.bubbleTheirs}`}>
          <span className={styles.content}>{message.content}</span>
            <span className={styles.time}>
  {time}
                {message.isEdited && <span className={styles.edited}> · edited</span>}
                {isMine && (
                    <span className={styles.readReceipt} title={message.isRead ? 'Read' : 'Delivered'}>
      {message.isRead ? ' ✓✓' : ' ✓'}
    </span>
                )}
</span>
        </div>
      </div>

      {/* Actions */}
      {hovered && (
        <div className={`${styles.actions} ${isMine ? styles.actionsMine : styles.actionsTheirs}`}>
          <button className={styles.actionBtn} onClick={onReply} title="Reply">
            <svg width="13" height="13" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <polyline points="9 17 4 12 9 7"/><path d="M20 18v-2a4 4 0 0 0-4-4H4"/>
            </svg>
          </button>
          {isMine && (
            <button className={`${styles.actionBtn} ${styles.deleteBtn}`} onClick={onDelete} title="Delete">
              <svg width="13" height="13" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
              </svg>
            </button>
          )}
        </div>
      )}
    </div>
  );
}
