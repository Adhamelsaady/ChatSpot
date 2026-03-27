import React, {
  useCallback,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from 'react';
import { useAuth } from '../context/AuthContext';
import { useChat } from '../context/ChatContext';
import Avatar from './Avatar';
import MessageBubble from './MessageBubble';
import MessageInput from './MessageInput';
import GroupInfoPanel from './GroupInfoPanel';
import styles from './ChatWindow.module.css';

const SCROLL_LOAD_THRESHOLD = 120;
const STICK_BOTTOM_PX = 140;

function scrollToBottom(el) {
  if (!el) return;
  el.scrollTop = Math.max(0, el.scrollHeight - el.clientHeight);
}

export default function ChatWindow({ showBack, onBack }) {
  const { user } = useAuth();
  const {
    activeChat,
    messages,
    loadingMessages,
    deleteMessage,
    replyTo,
    setReplyTo,
    loadOlderMessages,
    loadingOlder,
    hasMoreOlder,
  } = useChat();
  const messagesContainerRef = useRef(null);
  const messagesInnerRef = useRef(null);
  const [showInfo, setShowInfo] = useState(false);

  const stickToBottomRef = useRef(true);
  const prependAnchorRef = useRef(null);
  const prevChatIdRef = useRef(undefined);
  const prevLoadingMessagesRef = useRef(false);

  const onScroll = useCallback(() => {
    const el = messagesContainerRef.current;
    if (!el || el.clientHeight < 40) return;
    const distBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
    stickToBottomRef.current = distBottom < STICK_BOTTOM_PX;

    if (
      el.scrollTop < SCROLL_LOAD_THRESHOLD &&
      hasMoreOlder &&
      !loadingOlder &&
      !loadingMessages &&
      messages.length > 0
    ) {
      prependAnchorRef.current = { sh: el.scrollHeight, st: el.scrollTop };
      loadOlderMessages();
    }
  }, [
    hasMoreOlder,
    loadingOlder,
    loadingMessages,
    messages.length,
    loadOlderMessages,
  ]);

  useLayoutEffect(() => {
    const el = messagesContainerRef.current;
    if (!el) return;

    const pending = prependAnchorRef.current;
    if (pending) {
      el.scrollTop = Math.max(0, el.scrollHeight - pending.sh + pending.st);
      prependAnchorRef.current = null;
      return;
    }

    if (loadingMessages) return;
    if (messages.length === 0) return;

    const chatChanged = prevChatIdRef.current !== activeChat?.id;
    prevChatIdRef.current = activeChat?.id;

    if (chatChanged) {
      scrollToBottom(el);
      stickToBottomRef.current = true;
      return;
    }

    if (stickToBottomRef.current) {
      scrollToBottom(el);
    }
  }, [messages, loadingMessages, activeChat?.id]);

  // After fonts/layout (and when list height changes), keep the viewport on the latest messages.
  useEffect(() => {
    const scrollEl = messagesContainerRef.current;
    if (!scrollEl || loadingMessages || messages.length === 0) return;

    const maybeStick = () => {
      if (prependAnchorRef.current) return;
      const el = messagesContainerRef.current;
      if (!el) return;
      if (stickToBottomRef.current) {
        scrollToBottom(el);
        return;
      }
      const distBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
      if (distBottom < STICK_BOTTOM_PX + 60) {
        scrollToBottom(el);
      }
    };

    const ro = new ResizeObserver(() => {
      requestAnimationFrame(maybeStick);
    });
    ro.observe(scrollEl);
    const inner = messagesInnerRef.current;
    if (inner) ro.observe(inner);

    maybeStick();
    const t1 = requestAnimationFrame(() => maybeStick());
    const t2 = setTimeout(maybeStick, 80);

    return () => {
      cancelAnimationFrame(t1);
      clearTimeout(t2);
      ro.disconnect();
    };
  }, [activeChat?.id, loadingMessages, messages.length]);

  useLayoutEffect(() => {
    const el = messagesContainerRef.current;
    const wasLoading = prevLoadingMessagesRef.current;
    prevLoadingMessagesRef.current = loadingMessages;
    if (wasLoading && !loadingMessages && el && messages.length > 0) {
      scrollToBottom(el);
      stickToBottomRef.current = true;
    }
  }, [loadingMessages, messages.length]);

  const dmUser = activeChat?.data?.user;
  const chatName =
    activeChat?.type === 'group'
      ? activeChat.data?.name
      : (() => {
          const first = dmUser?.firstName?.trim();
          const last  = dmUser?.lastName?.trim();
          const full  = [first, last].filter(Boolean).join(' ');
          return full || dmUser?.userName || '';
        })();

  const isOnline = activeChat?.data?.user?.status === 'Online';
  const lastSeen = activeChat?.data?.user?.lastSeen;

  const chatSub =
    activeChat?.type === 'group'
      ? `${activeChat.data?.memberCount || ''} members`
      : isOnline
        ? 'Online'
        : lastSeen
          ? `Last seen ${new Date(lastSeen).toLocaleString()}`
          : 'Offline';

  return (
    <div className={styles.window}>
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          {showBack && onBack && (
            <button
              type="button"
              className={styles.backBtn}
              onClick={onBack}
              aria-label="Back to conversations"
            >
              <svg
                width="20"
                height="20"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                viewBox="0 0 24 24"
                aria-hidden
              >
                <path d="M19 12H5M12 19l-7-7 7-7" />
              </svg>
            </button>
          )}
          <Avatar
            name={chatName}
            src={activeChat?.data?.avatarUrl || activeChat?.data?.otherUserAvatar}
            size={36}
          />
          <div className={styles.headerInfo}>
            <span className={styles.chatName}>{chatName}</span>
            <span
              className={`${styles.chatSub} ${activeChat?.data?.isOnline ? styles.online : ''}`}
            >
              {chatSub}
            </span>
          </div>
        </div>
        <div className={styles.headerActions}>
          {activeChat?.type === 'group' && (
            <button
              type="button"
              className={`${styles.actionBtn} ${showInfo ? styles.activeAction : ''}`}
              onClick={() => setShowInfo((v) => !v)}
              title="Group info"
            >
              <svg
                width="16"
                height="16"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                viewBox="0 0 24 24"
              >
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
                <circle cx="9" cy="7" r="4" />
                <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
                <path d="M16 3.13a4 4 0 0 1 0 7.75" />
              </svg>
            </button>
          )}
        </div>
      </div>

      <div className={styles.body}>
        <div
          ref={messagesContainerRef}
          className={styles.messagesScroll}
          onScroll={onScroll}
        >
          {loadingOlder && (
            <div className={styles.loadOlderHint} aria-live="polite">
              Loading older messages…
            </div>
          )}

          {loadingMessages && (
            <div className={styles.loadingWrap}>
              <div className={styles.loadingDots}>
                <span />
                <span />
                <span />
              </div>
            </div>
          )}

          {!loadingMessages && messages.length === 0 && (
            <div className={styles.startMsg}>
              <div className={styles.startIcon} aria-hidden>
                <svg
                  width="48"
                  height="48"
                  viewBox="0 0 48 48"
                  fill="none"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    d="M14 22h20M14 28h12"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    opacity="0.35"
                  />
                  <path
                    d="M24 8C14.06 8 6 15.16 6 24c0 3.22 1.02 6.2 2.76 8.7L6 40l7.62-2.48C16.38 39.26 20.06 40 24 40c9.94 0 18-7.16 18-16S33.94 8 24 8Z"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinejoin="round"
                  />
                </svg>
              </div>
              <p>No messages yet. Say hello!</p>
            </div>
          )}

          {!loadingMessages && messages.length > 0 && (
            <div ref={messagesInnerRef} className={styles.messagesInner}>
              {messages.map((msg, i) => {
                const isMine = msg.senderId === user?.id;
                const prevMsg = messages[i - 1];
                const showAvatar =
                  !isMine && (!prevMsg || prevMsg.senderId !== msg.senderId);
                return (
                  <MessageBubble
                    key={msg.id || i}
                    message={msg}
                    isMine={isMine}
                    showAvatar={showAvatar}
                    onDelete={() => deleteMessage(msg.id)}
                    onReply={() => setReplyTo(msg)}
                  />
                );
              })}
            </div>
          )}
        </div>

        {showInfo && activeChat?.type === 'group' && (
          <GroupInfoPanel
            groupId={activeChat.id}
            onClose={() => setShowInfo(false)}
          />
        )}
      </div>

      <div className={styles.composer}>
        {replyTo && (
          <div className={styles.replyPreview}>
            <div className={styles.replyBar} />
            <div className={styles.replyContent}>
              <span className={styles.replyLabel}>Replying to</span>
              <span className={styles.replyText}>{replyTo.content}</span>
            </div>
            <button
              type="button"
              className={styles.replyClose}
              onClick={() => setReplyTo(null)}
            >
              <svg
                width="14"
                height="14"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                viewBox="0 0 24 24"
              >
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
          </div>
        )}
        <MessageInput />
      </div>
    </div>
  );
}
