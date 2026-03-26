import Sidebar from '../components/Sidebar';
import ChatWindow from '../components/ChatWindow';
import EmptyState from '../components/EmptyState';
import CreateGroupModal from '../components/CreateGroupModal';
import NewChatModal from '../components/NewChatModal';
import { useChat } from '../context/ChatContext';
import { useMediaQuery } from '../hooks/useMediaQuery';
import styles from './ChatPage.module.css';
import React, { useState } from 'react';

export default function ChatPage() {
  const { activeChat, openChat } = useChat();
  const [showCreateGroup, setShowCreateGroup] = useState(false);
  const [showNewChat, setShowNewChat] = useState(false);
  const isNarrow = useMediaQuery('(max-width: 767px)');

  const layoutClass = [
    styles.layout,
    isNarrow && !activeChat && styles.narrowList,
    isNarrow && activeChat && styles.narrowThread,
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <div className={layoutClass}>
      <div className={styles.sidebarShell}>
        <Sidebar
          onNewChat={() => setShowNewChat(true)}
          onNewGroup={() => setShowCreateGroup(true)}
        />
      </div>
      <main className={styles.main}>
        {activeChat ? (
          <ChatWindow
            showBack={isNarrow}
            onBack={() => openChat(null)}
          />
        ) : (
          <EmptyState />
        )}
      </main>

      {showCreateGroup && (
        <CreateGroupModal onClose={() => setShowCreateGroup(false)} />
      )}
      {showNewChat && (
        <NewChatModal onClose={() => setShowNewChat(false)} />
      )}
    </div>
  );
}
