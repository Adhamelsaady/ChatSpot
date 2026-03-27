import React from 'react';
import { format } from 'date-fns';
import { enUS } from 'date-fns/locale/en-US';
import Avatar from './Avatar';
import styles from './MessageBubble.module.css';

export default function MessageBubble({ message, isMine, showAvatar, onDelete, onReply }) {

  const time = message.timestamp
    ? format(new Date(message.timestamp), 'h:mm a', { locale: enUS })
    : '';

  const first = message.senderFirstName?.trim();
  const last  = message.senderLastName?.trim();
  const full  = [first, last].filter(Boolean).join(' ');
  const displayName = full || message.senderName || message.senderId;

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
    <div className={`${styles.row} ${isMine ? styles.mine : styles.theirs}`}>
      {!isMine && (
        <div className={styles.avatarSlot}>
          {showAvatar ? (
            <Avatar name={displayName} size={28} />
          ) : (
            <div className={styles.avatarSpacer} />
          )}
        </div>
      )}

      <div className={styles.bubbleWrap}>
        {!isMine && showAvatar && (
          <span className={styles.senderName}>{displayName}</span>
        )}

        {/* Reply preview */}
        {message.replyToPreview && (
          <div
            className={`${styles.replyQuote} ${isMine ? styles.replyMine : ''}`}
            dir="auto"
          >
            <span className={styles.replyBar} aria-hidden />
            <span className={styles.replyQuoteText}>{message.replyToPreview}</span>
          </div>
        )}

        <div
          className={`${styles.bubble} ${isMine ? styles.bubbleMine : styles.bubbleTheirs}`}
          dir="auto"
        >
          <span className={styles.content}>{message.content}</span>
            <span className={styles.time} dir="ltr">
              {time}
              {message.isEdited && <span className={styles.edited}> · edited</span>}
              {isMine && (
                <span
                  className={styles.readReceipt}
                  title={message.isRead ? 'Read' : 'Delivered'}
                >
                  {message.isRead ? ' ✓✓' : ' ✓'}
                </span>
              )}
            </span>
        </div>
      </div>

      <div className={`${styles.actions} ${isMine ? styles.actionsMine : styles.actionsTheirs}`}>
        <button type="button" className={styles.actionBtn} onClick={onReply} title="Reply">
          <svg width="13" height="13" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" aria-hidden>
            <polyline points="9 17 4 12 9 7"/><path d="M20 18v-2a4 4 0 0 0-4-4H4"/>
          </svg>
        </button>
        {isMine && (
          <button type="button" className={`${styles.actionBtn} ${styles.deleteBtn}`} onClick={onDelete} title="Delete">
            <svg width="13" height="13" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" aria-hidden>
              <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
            </svg>
          </button>
        )}
      </div>
    </div>
  );
}
