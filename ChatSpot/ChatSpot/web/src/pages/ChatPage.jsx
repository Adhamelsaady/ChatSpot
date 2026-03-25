import Sidebar from '../components/Sidebar';
import ChatWindow from '../components/ChatWindow';
import EmptyState from '../components/EmptyState';
import CreateGroupModal from '../components/CreateGroupModal';
import NewChatModal from '../components/NewChatModal';
import { useChat } from '../context/ChatContext';
import styles from './ChatPage.module.css';
import React, { useState } from 'react';
export default function ChatPage() {
  const { activeChat } = useChat();
  const [showCreateGroup, setShowCreateGroup] = useState(false);
  const [showNewChat, setShowNewChat] = useState(false);

  return (
    <div className={styles.layout}>
      <Sidebar
        onNewChat={() => setShowNewChat(true)}
        onNewGroup={() => setShowCreateGroup(true)}
      />
      <main className={styles.main}>
        {activeChat ? <ChatWindow /> : <EmptyState />}
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
