import React, { useState, useRef, useCallback } from 'react';
import { useChat } from '../context/ChatContext';
import styles from './MessageInput.module.css';

const ACCEPTED_TYPES = 'image/*,video/*';
const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB

export default function MessageInput() {
  const { sendMessage } = useChat();
  const [text, setText] = useState('');
  const [sending, setSending] = useState(false);
  const [mediaFile, setMediaFile] = useState(null);
  const [mediaPreview, setMediaPreview] = useState(null);
  const textareaRef = useRef(null);
  const fileInputRef = useRef(null);

  const clearMedia = useCallback(() => {
    setMediaFile(null);
    if (mediaPreview) URL.revokeObjectURL(mediaPreview);
    setMediaPreview(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  }, [mediaPreview]);

  const handleFileSelect = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > MAX_FILE_SIZE) {
      alert('File is too large. Maximum size is 10 MB.');
      e.target.value = '';
      return;
    }
    if (mediaPreview) URL.revokeObjectURL(mediaPreview);
    setMediaFile(file);
    setMediaPreview(URL.createObjectURL(file));
  };

  const handleSend = async () => {
    const trimmed = text.trim();
    if ((!trimmed && !mediaFile) || sending) return;
    setSending(true);
    const currentMedia = mediaFile;
    setText('');
    clearMedia();
    try {
      await sendMessage(trimmed || ' ', currentMedia);
    } finally {
      setSending(false);
      textareaRef.current?.focus();
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const handleInput = (e) => {
    setText(e.target.value);
    const el = textareaRef.current;
    if (el) {
      el.style.height = 'auto';
      el.style.height = Math.min(el.scrollHeight, 120) + 'px';
    }
  };

  const isImage = mediaFile?.type?.startsWith('image/');
  const isVideo = mediaFile?.type?.startsWith('video/');
  const canSend = text.trim() || mediaFile;

  return (
    <div className={styles.wrap}>
      {/* Media preview strip */}
      {mediaFile && (
        <div className={styles.mediaPreview}>
          <div className={styles.mediaThumbnailWrap}>
            {isImage && (
              <img
                src={mediaPreview}
                alt="Attachment preview"
                className={styles.mediaThumbnail}
              />
            )}
            {isVideo && (
              <video
                src={mediaPreview}
                className={styles.mediaThumbnail}
                muted
              />
            )}
            <button
              type="button"
              className={styles.mediaRemoveBtn}
              onClick={clearMedia}
              aria-label="Remove attachment"
            >
              <svg width="10" height="10" fill="none" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
          </div>
          <span className={styles.mediaFileName}>{mediaFile.name}</span>
        </div>
      )}

      <div className={styles.inputRow}>
        {/* Hidden file input */}
        <input
          ref={fileInputRef}
          type="file"
          accept={ACCEPTED_TYPES}
          onChange={handleFileSelect}
          style={{ display: 'none' }}
          id="media-file-input"
        />

        {/* Attachment button */}
        <button
          type="button"
          className={styles.attachBtn}
          onClick={() => fileInputRef.current?.click()}
          title="Attach image or video"
          aria-label="Attach image or video"
        >
          <svg width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
            <path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48" />
          </svg>
        </button>

        <textarea
          ref={textareaRef}
          className={styles.textarea}
          dir="auto"
          placeholder="Write a message…"
          value={text}
          onChange={handleInput}
          onKeyDown={handleKeyDown}
          rows={1}
        />
        <button
          className={`${styles.sendBtn} ${canSend ? styles.active : ''}`}
          onClick={handleSend}
          disabled={!canSend || sending}
        >
          {sending ? (
            <span className={styles.spinner} />
          ) : (
            <svg width="16" height="16" fill="none" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24">
              <line x1="22" y1="2" x2="11" y2="13"/>
              <polygon points="22 2 15 22 11 13 2 9 22 2"/>
            </svg>
          )}
        </button>
      </div>
    </div>
  );
}
