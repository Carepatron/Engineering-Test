import axios from 'axios';

const API_BASE_URL = 'http://localhost:5025/api';

export interface Conversation {
  id: string;
  patientName: string;
  startedAt: string;
  lastMessageAt: string;
  lastMessage?: string;
  messageCount: number;
  createdByUserId?: string;
  createdByUserName?: string;
  createdByUserRole?: string;
}

export interface Message {
  id: string;
  content: string;
  isFromPatient: boolean;
  timestamp: string;
  senderUserId?: string;
  senderUserName?: string;
  senderUserRole?: string;
}

export interface ConversationWithMessages {
  id: string;
  patientName: string;
  messages: Message[];
}

const api = {
  getConversations: async (): Promise<Conversation[]> => {
    const response = await axios.get(`${API_BASE_URL}/conversations`);
    return response.data;
  },

  createConversation: async (patientName: string, user?: { id: string; name: string; role: string }): Promise<Conversation> => {
    const response = await axios.post(`${API_BASE_URL}/conversations`, {
      patientName,
      createdByUserId: user?.id,
      createdByUserName: user?.name,
      createdByUserRole: user?.role
    });
    return response.data;
  },

  getConversationMessages: async (id: string): Promise<ConversationWithMessages> => {
    const response = await axios.get(`${API_BASE_URL}/conversations/${id}/messages`);
    return response.data;
  },

  sendMessage: async (conversationId: string, content: string, user?: { id: string; name: string; role: string }): Promise<Message> => {
    const response = await axios.post(`${API_BASE_URL}/conversations/${conversationId}/messages`, {
      content,
      senderUserId: user?.id,
      senderUserName: user?.name,
      senderUserRole: user?.role
    });
    return response.data;
  }
};

export default api;