# Hernia Surgical LLC - AI Concierge Application

A full-featured patient messaging system with authentication, real-time communication, and automated AI responses for Hernia Surgical LLC.

## Project Structure

```
.
├── backend/                 # .NET Minimal API backend
│   └── HerniaSurgical.API/ # Main API project
├── frontend/               # React TypeScript frontend
└── CHALLENGE.md           # Instructions for candidates
```

## Prerequisites

- .NET SDK 8.0 or later
- Node.js 18+ and npm
- Git

## Setup Instructions

### Backend Setup

1. Navigate to the backend directory:
```bash
cd backend/HerniaSurgical.API
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Run the backend:
```bash
dotnet run
```

The API will start on `http://localhost:5025`

### Frontend Setup

1. In a new terminal, navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm start
```

The frontend will open in your browser at `http://localhost:3000`

## Features

### Authentication System
- **Role-based Login**: Staff and Patient access with different UI experiences
- **Demo Credentials**:
  - Staff: `sarah@gmail.com` / `password1`
  - Patient: `john.smith@email.com` / `patient123`
- **Persistent Sessions**: Auto-login on return visits
- **User Context**: All actions are tracked to specific authenticated users

### Messaging System
- **Conversations List**: Shows all conversations with role-based filtering
  - Patients see "My Messages" with their conversations only
  - Staff see full "Conversations" management interface
- **Real-time Messaging**: Facebook Messenger-style chat with live updates
- **User Attribution**: All messages track sender information (name, role, user ID)
- **AI Responses**: Automated responses based on keyword matching for common inquiries

### Data Management
- **SQLite Database**: Entity Framework with proper relationships
- **User Tracking**: Conversations and messages linked to authenticated users
- **Clean Database**: No seed data - fresh start for production use

### API Endpoints

#### Conversations
- `GET /api/conversations` - Get all conversations with message counts
- `POST /api/conversations` - Create new conversation with user context
- `GET /api/conversations/{id}/messages` - Get conversation messages with sender details

#### Messages  
- `POST /api/conversations/{id}/messages` - Send message with user attribution
- Real-time delivery via SignalR with automatic AI responses

#### Additional Endpoints
- `GET /api/clients` - Patient/client management
- `GET /api/appointments` - Appointment scheduling system

### SignalR Hub

- **Hub endpoint**: `/conversationHub`
- **Real-time Events**:
  - `ReceiveMessage` - Broadcasts new messages to conversation participants
  - `JoinConversation` - Join a conversation room for live updates
  - `LeaveConversation` - Leave a conversation room

## How It Works

1. **Login**: Users authenticate with role-based credentials (Staff or Patient)
2. **Create Conversations**: 
   - Patients can start new messages with the medical team
   - Staff can initiate conversations for specific patients
3. **Real-time Messaging**: Send messages with instant delivery and AI responses
4. **User Tracking**: All conversations and messages are attributed to authenticated users
5. **Role-based UI**: Different experiences for patients vs medical staff

## License

MIT License - See LICENSE file for details