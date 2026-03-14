import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '@/stores/auth'

class SignalRService {
  constructor() {
    this.connection = null
    this.reconnectAttempts = 0
    this.maxReconnectAttempts = 5
  }

  async start() {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return
    }

    const authStore = useAuthStore()
    const hubUrl = import.meta.env.VITE_SIGNALR_URL || '/hubs/quiz'

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => authStore.token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build()

    this.connection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error)
    })

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId)
      this.reconnectAttempts = 0
    })

    this.connection.onclose((error) => {
      console.log('SignalR connection closed:', error)
    })

    try {
      await this.connection.start()
      console.log('SignalR connected')
    } catch (error) {
      console.error('SignalR connection error:', error)
      throw error
    }
  }

  async stop() {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
    }
  }

  // Quiz room methods
  async joinQuizRoom(baiThiId) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinQuizRoom', baiThiId)
    }
  }

  async leaveQuizRoom(baiThiId) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveQuizRoom', baiThiId)
    }
  }

  // Event handlers
  onCountdown(callback) {
    this.connection?.on('Countdown', callback)
  }

  onTimeWarning(callback) {
    this.connection?.on('TimeWarning', callback)
  }

  onAutoSubmit(callback) {
    this.connection?.on('AutoSubmit', callback)
  }

  onNewParticipant(callback) {
    this.connection?.on('NewParticipant', callback)
  }

  onQuizCompleted(callback) {
    this.connection?.on('QuizCompleted', callback)
  }

  onPong(callback) {
    this.connection?.on('Pong', callback)
  }

  // Remove handlers
  off(eventName) {
    this.connection?.off(eventName)
  }

  // Ping to keep connection alive
  async ping() {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('Ping')
    }
  }
}

export default new SignalRService()
