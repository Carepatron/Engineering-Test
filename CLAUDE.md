# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Hernia Surgical LLC - AI Concierge Application. A comprehensive healthcare communication platform that solves the problem of inefficient patient-clinic communication through automated 24/7 support, real-time messaging, appointment management, and staff scheduling systems.

## Current Implementation Status

### ✅ Completed Features

#### Authentication System
- Role-based login (Staff vs Patient)
- Demo credentials:
  - Staff: `sarah@gmail.com` / `password1` 
  - Patient: `john.smith@email.com` / `patient123`
- Persistent sessions using localStorage
- Protected routes requiring authentication
- User context passed in all API calls

#### Messaging System  
- Real-time messaging using SignalR
- User attribution (all messages track sender info)
- **AI auto-responses ONLY for patient messages** (staff messages don't trigger AI)
- Role-based conversation filtering:
  - Patients see only their conversations
  - Staff see all conversations
- Message alignment based on viewer perspective:
  - Staff view: Patient messages left, clinic messages right
  - Patient view: Their messages right, clinic messages left
- Fixed duplicate message issue (removed optimistic updates)

#### Client & Appointment Management
- Full CRUD operations for client records
- Appointment scheduling with start/end DateTimeUtc fields
- Staff assignment to appointments (StaffUserId)
- Client-appointment relationships with cascade operations

#### Staff Scheduling System
- Schedule model with weekly time slots
- ScheduleSlot model for individual time periods (9am-5pm Mon-Fri)
- Schedule visualization endpoint combining working hours with appointments
- Utilization metrics and daily summaries

#### Database
- SQLite with Entity Framework Core
- Comprehensive data model with proper relationships:
  - Users, Clients, Appointments, Conversations, Messages
  - Schedules, ScheduleSlots for staff working hours
- Data seeding with realistic test data
- Foreign key constraints and cascade operations

## Tech Stack

- **Backend**: .NET 8.0 Minimal API with Entity Framework Core, SQLite, and SignalR
- **Frontend**: React 18+ with TypeScript
- **Authentication**: React Context API with fake credentials
- **Real-time**: SignalR for live message updates

## Common Commands

### Backend Commands
```bash
# Navigate to backend
cd backend/HerniaSurgical.API

# Restore packages
dotnet restore

# Run the API (starts on http://localhost:5025)
dotnet run

# Build the project
dotnet build

# Run with hot reload
dotnet watch run
```

### Frontend Commands
```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Start development server (starts on http://localhost:3000)
npm start

# Build for production
npm run build

# Run tests
npm test
```

### Database Commands
```bash
# View all tables
sqlite3 herniasurgical.db ".tables"

# View all users
sqlite3 herniasurgical.db "SELECT * FROM Users;"

# View all conversations
sqlite3 herniasurgical.db "SELECT * FROM Conversations;"

# View all messages
sqlite3 herniasurgical.db "SELECT * FROM Messages;"

# View all appointments
sqlite3 herniasurgical.db "SELECT * FROM Appointments;"

# View all clients
sqlite3 herniasurgical.db "SELECT * FROM Clients;"

# View all schedules
sqlite3 herniasurgical.db "SELECT * FROM Schedules;"

# Clear all data (CAUTION: keeps only structure)
sqlite3 herniasurgical.db "DELETE FROM Messages; DELETE FROM Conversations; DELETE FROM Appointments; DELETE FROM Clients; DELETE FROM ScheduleSlots; DELETE FROM Schedules; DELETE FROM Users;"

# Restart with fresh data (will trigger data seeding)
rm herniasurgical.db && dotnet run
```

## Repository Structure

```
.
├── backend/
│   └── HerniaSurgical.API/
│       ├── Data/           # DbContext configuration
│       ├── Hubs/           # SignalR hubs
│       ├── Models/         # Entity models (User, Conversation, Message, etc.)
│       └── Program.cs      # API endpoints and configuration
├── frontend/
│   └── src/
│       ├── components/     # React components
│       ├── contexts/       # Authentication context
│       ├── services/       # API and SignalR services
│       └── App.tsx         # Main app with routing
└── README.md              # Project documentation
```

## API Endpoints

### Conversations
- `GET /api/conversations?userId={id}&userRole={role}` - List conversations (filtered for patients)
- `POST /api/conversations` - Create conversation with user tracking
  ```json
  {
    "patientName": "string",
    "createdByUserId": "string",
    "createdByUserName": "string", 
    "createdByUserRole": "string"
  }
  ```

### Messages
- `GET /api/conversations/{id}/messages` - Get messages with sender info
- `POST /api/conversations/{id}/messages` - Send message with user attribution
  ```json
  {
    "content": "string",
    "senderUserId": "string",
    "senderUserName": "string",
    "senderUserRole": "string"
  }
  ```

### Client Endpoints
- `GET /api/clients` - List all clients with appointment/conversation counts
- `GET /api/clients/{id}` - Get specific client with full details
- `POST /api/clients` - Create new client record
- `PUT /api/clients/{id}` - Update existing client

### Appointment Endpoints
- `GET /api/appointments` - List all appointments with client/staff info
- `GET /api/appointments/{id}` - Get specific appointment
- `GET /api/clients/{clientId}/appointments` - Get appointments for specific client
- `POST /api/appointments` - Create new appointment
- `PUT /api/appointments/{id}` - Update existing appointment

### Schedule Endpoints
- `GET /api/schedules` - List all staff schedules
- `GET /api/schedules/staff/{staffUserId}` - Get schedules for specific staff member
- `POST /api/schedules` - Create new schedule with time slots
- `GET /api/schedules/staff/{staffUserId}/visualization` - Schedule visualization with appointments

## SignalR Events

- Hub URL: `http://localhost:5025/conversationHub`
- Events: 
  - `ReceiveMessage` - Broadcasts new messages with sender info
  - `JoinConversation` - Join conversation for live updates
  - `LeaveConversation` - Leave conversation room

## Key Models

### User
- Id (string), Name, Email, Role, CreatedAt
- Relationships: CreatedConversations, SentMessages, StaffAppointments, Schedules

### Conversation  
- Id (Guid), PatientName, StartedAt, LastMessageAt
- CreatedByUserId, CreatedByUserName, CreatedByUserRole, ClientId (optional)
- Relationships: Messages, Client, CreatedByUser

### Message
- Id (Guid), Content, IsFromPatient, Timestamp, ConversationId
- SenderUserId, SenderUserName, SenderUserRole
- Relationships: Conversation, SenderUser

### Client
- Id (Guid), FirstName, LastName, Email (unique), Phone, DateOfBirth
- InsuranceProvider, InsurancePolicyNumber, CreatedAt, UpdatedAt
- FullName computed property
- Relationships: Appointments, Conversations

### Appointment
- Id (Guid), ClientId, StaffUserId (optional)
- StartDateUtc, EndDateUtc (DateTime fields)
- AppointmentType, Status, Provider, Notes
- CreatedAt, UpdatedAt
- Relationships: Client, StaffUser

### Schedule
- Id (Guid), StaffUserId, Name, IsActive
- CreatedAt, UpdatedAt
- Relationships: StaffUser, Slots

### ScheduleSlot
- Id (Guid), ScheduleId, DayOfWeek (enum)
- StartTime, EndTime (TimeSpan)
- IsAvailable, Notes
- Relationship: Schedule

## Important Implementation Details

### AI Response Logic
- **CRITICAL**: AI only responds to messages from patients (`SenderUserRole == "Patient"`)
- Staff messages do NOT trigger AI responses
- AI responses attributed to "ai-system" user with "System" role
- Keyword-based responses for common inquiries (scheduling, insurance, hernia info)

### Message Alignment Logic
- **Staff View**: 
  - Patient messages appear on LEFT
  - All clinic messages (staff/AI) appear on RIGHT
- **Patient View**:
  - Their messages appear on RIGHT
  - All clinic responses appear on LEFT

### Conversation Filtering
- Patients can only see conversations where:
  - They created the conversation OR
  - They sent at least one message
- Staff can see all conversations (no filtering)

### SignalR Real-time Updates
- Fixed duplicate message issue by removing optimistic updates
- Messages only appear once when broadcasted via SignalR
- Fallback mechanism to fetch messages if SignalR fails

### Database Relationships
- All foreign keys properly configured with appropriate cascade behaviors
- Unique constraints on User.Email and Client.Email
- Indexes on frequently queried fields for performance

### Schedule Visualization
- Combines staff working hours with actual appointments
- Shows utilization rates and booking summaries
- Handles weekends and non-working days properly
- Date range queries for flexible reporting

## Testing the Application

1. **Login as Patient** (john.smith@email.com / patient123)
   - Can only see their own conversations
   - Can start new messages with "New Message" button
   - Messages appear on right, clinic responses (AI/staff) on left
   - **AI will respond** to patient messages automatically

2. **Login as Staff** (sarah@gmail.com / password1)
   - Can see all conversations from all patients
   - Can create conversations for any patient
   - Patient messages on left, staff messages on right
   - **AI will NOT respond** to staff messages

3. **Real-time Updates**
   - Open two browser windows with different users
   - Send messages and see live updates via SignalR
   - Test AI responses by sending patient messages with keywords like "appointment", "hernia", "insurance"

4. **API Testing**
   - Test schedule visualization: `GET /api/schedules/staff/1/visualization`
   - Create appointments: `POST /api/appointments`
   - Manage clients: CRUD operations on `/api/clients`

## Development Notes

**CRITICAL Rules:**
1. **AI Response Logic**: Only patients trigger AI responses (`SenderUserRole == "Patient"`)
2. **User Context**: Always pass user info (userId, userName, userRole) in API calls
3. **Role-based Access**: Maintain filtering (patients see only their data)
4. **Message Alignment**: Follow viewer perspective rules strictly
5. **SignalR**: Don't add optimistic updates (causes duplicates)

**Data Integrity:**
1. Keep Users table data (required for authentication)
2. Foreign key relationships are enforced
3. Use data seeder for consistent test data
4. Handle cascade deletes appropriately

**Testing Strategy:**
1. Always test with both patient and staff accounts
2. Verify AI responses only for patient messages
3. Check real-time updates work correctly
4. Test role-based conversation filtering
5. Verify appointment and schedule functionality