import React, { useEffect, useState, useRef } from 'react';
import { useParams, Link } from 'react-router-dom';
import api, { ConversationWithMessages } from '../services/api';
import signalRService from '../services/signalr';
import { ThemeToggle } from './ThemeToggle';
import { useAuth } from '../contexts/AuthContext';
import './ConversationDetail.css';

const ConversationDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [conversation, setConversation] = useState<ConversationWithMessages | null>(null);
  const [inputMessage, setInputMessage] = useState('');
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const { user, logout } = useAuth();

  useEffect(() => {
    if (id) {
      loadConversation();
      connectSignalR();
    }

    return () => {
      if (id) {
        signalRService.leaveConversation(id);
      }
    };
  }, [id]);

  useEffect(() => {
    scrollToBottom();
  }, [conversation?.messages]);

  const connectSignalR = async () => {
    try {
      console.log('Setting up SignalR connection...');
      await signalRService.connect();
      
      signalRService.onMessageReceived((conversationId, message) => {
        console.log('Received SignalR message:', { conversationId, message, currentId: id });
        if (conversationId === id) {
          setConversation(prev => {
            if (!prev) return prev;
            // Check if message already exists to avoid duplicates
            const exists = prev.messages.some(m => m.id === message.id);
            if (exists) {
              console.log('Message already exists, skipping:', message.id);
              return prev;
            }
            return {
              ...prev,
              messages: [...prev.messages, message]
            };
          });
        }
      });
      
      if (id) {
        // Small delay to ensure connection is fully established
        setTimeout(async () => {
          await signalRService.joinConversation(id);
        }, 100);
      }
    } catch (error) {
      console.error('SignalR connection error:', error);
    }
  };

  const loadConversation = async () => {
    if (!id) return;
    
    try {
      const data = await api.getConversationMessages(id);
      setConversation(data);
    } catch (error) {
      console.error('Error loading conversation:', error);
    } finally {
      setLoading(false);
    }
  };

  const sendMessage = async () => {
    if (!inputMessage.trim() || !id || sending) return;

    const messageContent = inputMessage.trim();
    setInputMessage('');
    setSending(true);

    // Optimistic update - add the message immediately to the UI
    const tempMessage = {
      id: 'temp-' + Date.now(),
      content: messageContent,
      isFromPatient: true,
      timestamp: new Date().toISOString()
    };

    setConversation(prev => {
      if (!prev) return prev;
      return {
        ...prev,
        messages: [...prev.messages, tempMessage]
      };
    });

    try {
      const response = await api.sendMessage(id, messageContent, user || undefined);
      
      // Replace the temp message with the actual response
      setConversation(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          messages: prev.messages.map(msg => 
            msg.id === tempMessage.id ? response : msg
          )
        };
      });

      // Fallback: If SignalR doesn't deliver AI response within 3 seconds, fetch manually
      setTimeout(async () => {
        try {
          const updatedConversation = await api.getConversationMessages(id);
          setConversation(prev => {
            if (!prev || updatedConversation.messages.length <= prev.messages.length) {
              return prev; // No new messages
            }
            console.log('Fetched new messages as fallback');
            return updatedConversation;
          });
        } catch (error) {
          console.error('Error fetching updated messages:', error);
        }
      }, 3000);

    } catch (error) {
      console.error('Error sending message:', error);
      
      // Remove the temp message on error
      setConversation(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          messages: prev.messages.filter(msg => msg.id !== tempMessage.id)
        };
      });
      
      alert('Failed to send message. Please try again.');
      setInputMessage(messageContent); // Restore the message text
    } finally {
      setSending(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const formatTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  if (loading) {
    return <div className="loading">Loading conversation...</div>;
  }

  if (!conversation) {
    return <div className="error">Conversation not found</div>;
  }

  return (
    <div className="conversation-detail">
      <div className="conversation-header">
        <div className="conversation-header-left">
          <Link to="/" className="back-button">← Back</Link>
          <div className="conversation-title">
            <h2>
              {user?.role === 'Patient' 
                ? 'Conversation with Hernia Surgical LLC' 
                : conversation.patientName
              }
            </h2>
            <span className="logged-in-as">
              {user?.role === 'Patient' 
                ? `Patient: ${user?.name}` 
                : `Logged in as ${user?.name}`
              }
            </span>
          </div>
        </div>
        <div className="conversation-actions">
          <ThemeToggle />
          <button 
            onClick={() => loadConversation()}
            className="refresh-button"
            disabled={loading}
          >
            ↻ Refresh
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
      
      <div className="messages-container">
        {conversation.messages.map((message) => {
          let messageClass: string;
          let showSenderName = false;
          
          if (user?.role === 'Patient') {
            // Patient view: their messages on right, all clinic messages on left
            messageClass = message.senderUserId === user.id ? 'patient' : 'concierge';
            showSenderName = message.senderUserId !== user.id;
          } else {
            // Staff view: patient messages on left, all clinic messages (staff/AI) on right
            const isPatientMessage = message.senderUserRole === 'Patient';
            messageClass = isPatientMessage ? 'concierge' : 'patient';
            showSenderName = isPatientMessage;
          }
          
          return (
            <div
              key={message.id}
              className={`message ${messageClass}`}
            >
              <div className="message-bubble">
                <div className="message-content">{message.content}</div>
                <div className="message-time">{formatTime(message.timestamp)}</div>
                {showSenderName && (
                  <div className="message-sender">{message.senderUserName}</div>
                )}
              </div>
            </div>
          );
        })}
        <div ref={messagesEndRef} />
      </div>

      <div className="message-input-container">
        <input
          type="text"
          value={inputMessage}
          onChange={(e) => setInputMessage(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Type a message..."
          disabled={sending}
          className="message-input"
        />
        <button
          onClick={sendMessage}
          disabled={sending || !inputMessage.trim()}
          className="send-button"
        >
          Send
        </button>
      </div>
    </div>
  );
};

export default ConversationDetail;