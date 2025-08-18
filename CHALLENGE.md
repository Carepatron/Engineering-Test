# Engineering Challenge: AI Concierge for Hernia Surgical LLC

## The Problem: Healthcare Communication Gaps

Healthcare practices face persistent challenges in patient communication that directly impact both operational efficiency and patient satisfaction. Traditional phone-based systems create bottlenecks, leading to busy signals during peak hours, long hold times, and missed calls that result in lost appointments and frustrated patients.

**Key Pain Points:**
- **For Clinics**: Staff spend 40-60% of their time on repetitive administrative tasks like appointment scheduling, answering basic questions, and providing routine information
- **For Patients**: Limited availability to contact the clinic during business hours, long wait times for simple questions, and difficulty accessing basic information like appointment times or preparation instructions
- **Missed Opportunities**: After-hours inquiries go unanswered, leading to potential patient loss to competitors who offer more accessible communication

## The AI Concierge Solution

An AI Concierge system addresses these challenges by providing **24/7 intelligent patient support** that can handle routine inquiries instantly while escalating complex cases to human staff when needed.

**Benefits for the Clinic:**
- **Operational Efficiency**: Reduces staff workload by automating 70-80% of routine patient inquiries
- **Cost Reduction**: Decreases administrative overhead and reduces the need for additional front desk staff
- **Improved Patient Flow**: Streamlines appointment scheduling and reduces no-shows through automated reminders and confirmations
- **Data Insights**: Captures patient interaction patterns to identify common concerns and optimize services

**Benefits for Patients:**
- **24/7 Accessibility**: Get answers to questions and schedule appointments anytime, even outside business hours
- **Instant Responses**: No more waiting on hold for simple questions about appointment times, preparation instructions, or clinic policies
- **Consistent Information**: Receive accurate, standardized responses every time
- **Convenient Experience**: Interact through familiar messaging interfaces rather than navigating phone menus

## Overview

You have been provided with a skeleton codebase for an AI Concierge system that handles patient communications for Hernia Surgical LLC. The current implementation is functional but lacks best practice architecture and does implement any AI Concierge.

## Current System

The skeleton application consists of:
- **Backend**: .NET Minimal API with Entity Framework and SignalR
- **Frontend**: React TypeScript application with two pages
  1. Conversations List - Shows all patient conversations with ability to create new ones
  2. Conversation Detail - Shows messages in a conversation with real-time messaging

### Fully Functional Features (For Your Use)
- ✅ Create new conversations (working API endpoint)
- ✅ Send and receive messages in real-time
- ✅ Basic AI responses (keyword matching)
- ✅ Facebook Messenger-style UI
- ✅ SignalR real-time communication

## Requirements

The AI Concierge must be able to handle the following conversational flows.

#### Retrieve Existing Appointment Information

User Intent: A patient asks when their next appointment is.
- Example Queries: "When is my next appointment?", "Do I have anything scheduled for next week?", "check my appointments"

Expected AI Response:
- If an appointment exists: "Your next appointment is on Tuesday, November 15th at 10:30 AM with Dr. Evans. See you then!"
- If no appointments exist: "It looks like you don't have any upcoming appointments scheduled."

#### Check for Available Appointments
User Intent: A patient asks for open appointment slots.
Example Queries: "Do you have any availability next week?", "I need to book an appointment", "any openings on Friday?"

Expected AI Response:
- If slots are available: "Yes, we have an opening on Friday, November 18th at 2:00 PM. Would you like to book it?"
- If no slots are available: "I'm sorry, we are fully booked for that period. The next available opening is on Monday, November 28th."

#### Book a New Appointment
User Intent: A patient confirms they want to book a suggested slot. This is a multi-turn conversation.

Expected AI Flow:
- AI: "We have an opening on Friday, November 18th at 2:00 PM. Would you like to book it?"
- Patient: "Yes, please."
- AI: "Great. To confirm, you want to book an appointment for Friday, November 18th at 2:00 PM. Is that correct?"
- Patient: "That's correct."
- AI: "Excellent, you're all set! Your appointment is confirmed. We've sent a confirmation to your email."

#### Answer questions about notes taken during an appointment
User Intent: A patient wants to know the notes that were taken during their appointment

Expected AI Response:
- The notes taken, that include some helpful information.

#### Do anything else
You come up with another capability of it!

## Deliverables

1. **Refactored codebase** following best practice design principles
2. **AI agent integration** with proper LLM implementation
3. **Tests and evals** covering critical functionality
4. **Brief loom video** explaining:
   - AI implementation approach
   - Architecture decisions
   - Future improvements

## Technical Requirements

- Use .NET 8.0 or later (feel free to use another backend language if you need)
- Use React 18+ with TypeScript
- Provide clear instructions for API keys and configuration

## How to submit your work
- Create a new **private** GitHub repository (don't fork this!)
  - Invite @davidpene and @Julian-Robinson 
- Email us a link to the GitHub repository
- Include any notes you think are relevant
  - If time wasn't a constraint what else would you have done?
  - How was this test overall? I.e too hard, too easy, how long it took, etc


## ✨✨ REMEMBER ✨✨

SHOW OFF YOUR SKILLS AND KNOWLEDGE 🧠