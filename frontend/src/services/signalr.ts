import * as signalR from '@microsoft/signalr';

class SignalRService {
  private connection: signalR.HubConnection | null = null;

  async connect() {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }

    console.log('Connecting to SignalR...');
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5025/conversationHub')
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
    });

    this.connection.onreconnected(() => {
      console.log('SignalR reconnected');
    });

    this.connection.onclose((error) => {
      console.log('SignalR connection closed:', error);
    });

    try {
      await this.connection.start();
      console.log('SignalR connected successfully');
    } catch (err) {
      console.error('Error connecting to SignalR:', err);
      throw err;
    }
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('SignalR disconnected');
    }
  }

  onMessageReceived(callback: (conversationId: string, message: any) => void) {
    if (this.connection) {
      // Remove existing listeners to avoid duplicates
      this.connection.off('ReceiveMessage');
      this.connection.on('ReceiveMessage', callback);
    }
  }

  async joinConversation(conversationId: string) {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      console.log('Joining conversation:', conversationId);
      try {
        await this.connection.invoke('JoinConversation', conversationId);
        console.log('Successfully joined conversation:', conversationId);
      } catch (err) {
        console.error('Error joining conversation:', err);
      }
    } else {
      console.warn('Cannot join conversation - SignalR not connected');
    }
  }

  async leaveConversation(conversationId: string) {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('LeaveConversation', conversationId);
        console.log('Successfully left conversation:', conversationId);
      } catch (err) {
        console.error('Error leaving conversation:', err);
      }
    }
  }
}

export default new SignalRService();