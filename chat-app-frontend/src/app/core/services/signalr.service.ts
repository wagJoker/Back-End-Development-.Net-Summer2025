import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection;
  private messageSubject = new BehaviorSubject<any>(null);
  private typingSubject = new BehaviorSubject<any>(null);
  private userJoinedSubject = new BehaviorSubject<any>(null);
  private userLeftSubject = new BehaviorSubject<any>(null);

  constructor(private authService: AuthService) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.signalRUrl}/chathub`, {
        accessTokenFactory: () => this.authService.getToken()
      })
      .withAutomaticReconnect()
      .build();

    this.setupSignalRHandlers();
  }

  private setupSignalRHandlers(): void {
    this.hubConnection.on('ReceiveMessage', (message) => {
      this.messageSubject.next(message);
    });

    this.hubConnection.on('UserTyping', (user) => {
      this.typingSubject.next(user);
    });

    this.hubConnection.on('UserJoined', (user) => {
      this.userJoinedSubject.next(user);
    });

    this.hubConnection.on('UserLeft', (user) => {
      this.userLeftSubject.next(user);
    });
  }

  async startConnection(): Promise<void> {
    try {
      await this.hubConnection.start();
      console.log('SignalR Connected!');
    } catch (err) {
      console.error('Error while starting SignalR connection:', err);
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  async stopConnection(): Promise<void> {
    try {
      await this.hubConnection.stop();
      console.log('SignalR Disconnected!');
    } catch (err) {
      console.error('Error while stopping SignalR connection:', err);
    }
  }

  async sendMessage(message: string): Promise<void> {
    try {
      await this.hubConnection.invoke('SendMessage', message);
    } catch (err) {
      console.error('Error while sending message:', err);
    }
  }

  async sendTyping(): Promise<void> {
    try {
      await this.hubConnection.invoke('SendTyping');
    } catch (err) {
      console.error('Error while sending typing status:', err);
    }
  }

  getMessageObservable(): Observable<any> {
    return this.messageSubject.asObservable();
  }

  getTypingObservable(): Observable<any> {
    return this.typingSubject.asObservable();
  }

  getUserJoinedObservable(): Observable<any> {
    return this.userJoinedSubject.asObservable();
  }

  getUserLeftObservable(): Observable<any> {
    return this.userLeftSubject.asObservable();
  }
} 