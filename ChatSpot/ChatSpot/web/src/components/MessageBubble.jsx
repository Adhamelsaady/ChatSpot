import React, { useState } from 'react';
import { format } from 'date-fns';
import { enUS } from 'date-fns/locale/en-US';
import Avatar from './Avatar';
import styles from './MessageBubble.module.css';

export default function MessageBubble({ message, isMine, showAvatar, avatarSrc, onDelete, onReply }) {

  const [lightbox, setLightbox] = useState(false);

  const time = message.timestamp
    ? format(new Date(message.timestamp), 'h:mm a', { locale: enUS })
    : '';

  const first = message.senderFirstName?.trim();
  const last  = message.senderLastName?.trim();
  const full  = [first, last].filter(Boolean).join(' ');
  const displayName = full || message.senderName || message.senderId;

  const hasMedia = !!message.mediaUrl;
  const isImage = hasMedia && message.mediaType?.startsWith('image');
  const isVideo = hasMedia && message.mediaType?.startsWith('video');

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
    <>
      <div className={`${styles.row} ${isMine ? styles.mine : styles.theirs}`}>
        {!isMine && (
          <div className={styles.avatarSlot}>
            {showAvatar ? (
              <Avatar name={displayName} src={avatarSrc} size={28} />
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

          {/* Media attachment */}
          {hasMedia && (
            <div className={`${styles.mediaContainer} ${isMine ? styles.mediaMine : styles.mediaTheirs}`}>
              {isImage && (
                <img
                  src={message.mediaUrl}
                  alt="Shared image"
                  className={styles.mediaImage}
                  loading="lazy"
                  onClick={() => setLightbox(true)}
                />
              )}
              {isVideo && (
                <video
                  src={message.mediaUrl}
                  className={styles.mediaVideo}
                  controls
                  preload="metadata"
                />
              )}
              {!isImage && !isVideo && (
                <a
                  href={message.mediaUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className={styles.mediaFileLink}
                >
                  <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                    <polyline points="14 2 14 8 20 8"/>
                    <line x1="16" y1="13" x2="8" y2="13"/>
                    <line x1="16" y1="17" x2="8" y2="17"/>
                  </svg>
                  View file
                </a>
              )}
            </div>
          )}

          {/* Text bubble — show only if there's actual text content */}
          {message.content && message.content.trim() && (
            <div
              className={`${styles.bubble} ${isMine ? styles.bubbleMine : styles.bubbleTheirs} ${hasMedia ? styles.bubbleAfterMedia : ''}`}
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
          )}

          {/* If media-only (no text), show just the timestamp */}
          {hasMedia && (!message.content || !message.content.trim()) && (
            <div className={`${styles.mediaTimestamp} ${isMine ? styles.mediaTimeMine : ''}`} dir="ltr">
              {time}
              {isMine && (
                <span
                  className={styles.readReceipt}
                  title={message.isRead ? 'Read' : 'Delivered'}
                >
                  {message.isRead ? ' ✓✓' : ' ✓'}
                </span>
              )}
            </div>
          )}
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

      {/* Lightbox overlay for full-size image */}
      {lightbox && isImage && (
        <div className={styles.lightboxOverlay} onClick={() => setLightbox(false)}>
          <button className={styles.lightboxClose} onClick={() => setLightbox(false)} aria-label="Close">
            <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
          <img
            src={message.mediaUrl}
            alt="Full size"
            className={styles.lightboxImage}
            onClick={(e) => e.stopPropagation()}
          />
        </div>
      )}
    </>
  );
}
