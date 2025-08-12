import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api, { Conversation } from '../services/api';
import { ThemeToggle } from './ThemeToggle';
import { useAuth } from '../contexts/AuthContext';
import './ConversationsList.css';

const ConversationsList: React.FC = () => {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [patientName, setPatientName] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  useEffect(() => {
    loadConversations();
  }, []);

  const loadConversations = async () => {
    try {
      const data = await api.getConversations();
      setConversations(data);
    } catch (error) {
      console.error('Error loading conversations:', error);
    } finally {
      setLoading(false);
    }
  };

  const createConversation = async () => {
    // For patients, use their name; for staff, require patient name input
    const conversationName = user?.role === 'Patient' 
      ? (patientName.trim() || 'General Inquiry') 
      : patientName.trim();

    if (user?.role !== 'Patient' && !conversationName) return;
    if (creating) return;

    setCreating(true);
    try {
      const newConversation = await api.createConversation(
        user?.role === 'Patient' 
          ? `${user.name} - ${conversationName}`
          : conversationName,
        user || undefined
      );
      setConversations([newConversation, ...conversations]);
      setPatientName('');
      setShowCreateForm(false);
      navigate(`/conversation/${newConversation.id}`);
    } catch (error) {
      console.error('Error creating conversation:', error);
      alert('Failed to create conversation. Please try again.');
    } finally {
      setCreating(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      createConversation();
    } else if (e.key === 'Escape') {
      setShowCreateForm(false);
      setPatientName('');
    }
  };

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString();
  };

  if (loading) {
    return <div className="loading">Loading conversations...</div>;
  }

  return (
    <div className="conversations-list">
      <div className="header">
        <div className="header-left">
          <h1>
            {user?.role === 'Patient' 
              ? 'Hernia Surgical LLC - My Messages' 
              : 'Hernia Surgical LLC - Conversations'
            }
          </h1>
          <div className="user-info">
            <span className="welcome-text">Welcome, {user?.name}</span>
            <span className="user-role">{user?.role}</span>
          </div>
        </div>
        <div className="header-actions">
          <ThemeToggle />
          <button 
            className="new-conversation-btn"
            onClick={() => setShowCreateForm(true)}
            disabled={showCreateForm}
          >
            {user?.role === 'Patient' ? '+ New Message' : '+ New Conversation'}
          </button>
          <button 
            className="logout-btn"
            onClick={logout}
            title="Logout"
          >
            Logout
          </button>
        </div>
      </div>

      {showCreateForm && (
        <div className="create-form">
          <div className="create-form-content">
            <h3>
              {user?.role === 'Patient' ? 'Start New Message' : 'Start New Conversation'}
            </h3>
            {user?.role === 'Patient' ? (
              <div className="patient-form">
                <p className="form-description">
                  Start a new conversation with our medical team. We'll respond as soon as possible.
                </p>
                <input
                  type="text"
                  value={patientName}
                  onChange={(e) => setPatientName(e.target.value)}
                  onKeyDown={handleKeyPress}
                  placeholder="Subject (optional)"
                  disabled={creating}
                  autoFocus
                  className="patient-name-input"
                />
              </div>
            ) : (
              <input
                type="text"
                value={patientName}
                onChange={(e) => setPatientName(e.target.value)}
                onKeyDown={handleKeyPress}
                placeholder="Enter patient name..."
                disabled={creating}
                autoFocus
                className="patient-name-input"
              />
            )}
            <div className="create-form-buttons">
              <button
                onClick={createConversation}
                disabled={creating || (user?.role !== 'Patient' && !patientName.trim())}
                className="create-btn"
              >
                {creating ? 'Creating...' : (user?.role === 'Patient' ? 'Start Message' : 'Create')}
              </button>
              <button
                onClick={() => {
                  setShowCreateForm(false);
                  setPatientName('');
                }}
                disabled={creating}
                className="cancel-btn"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="conversations-container">
        {conversations.length === 0 ? (
          <p className="no-conversations">
            {user?.role === 'Patient' 
              ? 'No messages yet. Click "New Message" to start a conversation with our medical team.' 
              : 'No conversations yet'
            }
          </p>
        ) : (
          conversations.map((conversation) => (
            <Link
              key={conversation.id}
              to={`/conversation/${conversation.id}`}
              className="conversation-item"
            >
              <div className="conversation-header">
                <span className="patient-name">{conversation.patientName}</span>
                <span className="message-count">{conversation.messageCount} messages</span>
              </div>
              <div className="conversation-preview">
                {conversation.lastMessage || 'No messages yet'}
              </div>
              <div className="conversation-time">
                Last message: {formatTime(conversation.lastMessageAt)}
              </div>
            </Link>
          ))
        )}
      </div>
    </div>
  );
};

export default ConversationsList;