import React , { useState, useRef } from 'react';
import { useChat } from '../context/ChatContext';
import styles from './MessageInput.module.css';

export default function MessageInput() {
  const { sendMessage } = useChat();
  const [text, setText] = useState('');
  const [sending, setSending] = useState(false);
  const textareaRef = useRef(null);

  const handleSend = async () => {
    const trimmed = text.trim();
    if (!trimmed || sending) return;
    setSending(true);
    setText('');
    try {
      await sendMessage(trimmed);
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
    // Auto-resize
    const el = textareaRef.current;
    if (el) {
      el.style.height = 'auto';
      el.style.height = Math.min(el.scrollHeight, 120) + 'px';
    }
  };

  return (
    <div className={styles.wrap}>
      <div className={styles.inputRow}>
        <textarea
          ref={textareaRef}
          className={styles.textarea}
          dir="auto"
          placeholder="Write a message… (Enter to send, Shift+Enter for newline)"
          value={text}
          onChange={handleInput}
          onKeyDown={handleKeyDown}
          rows={1}
        />
        <button
          className={`${styles.sendBtn} ${text.trim() ? styles.active : ''}`}
          onClick={handleSend}
          disabled={!text.trim() || sending}
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
