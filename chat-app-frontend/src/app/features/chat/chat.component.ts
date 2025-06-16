import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { SignalRService } from '../../core/services/signalr.service';
import { AuthService } from '../../core/services/auth.service';
import { Subscription } from 'rxjs';

interface ChatMessage {
  id: number;
  username: string;
  message: string;
  timestamp: string;
  sentiment: string;
  sentimentColor: string;
  isEdited: boolean;
  lastEditedAt?: string;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex flex-col h-screen bg-gray-100">
      <!-- Header -->
      <header class="bg-white shadow">
        <div class="max-w-7xl mx-auto py-4 px-4 sm:px-6 lg:px-8 flex justify-between items-center">
          <h1 class="text-2xl font-bold text-gray-900">Chat App</h1>
          <div class="flex items-center space-x-4">
            <span class="text-gray-600">Welcome, {{ username }}</span>
            <button
              (click)="logout()"
              class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-md transition-colors"
            >
              Logout
            </button>
          </div>
        </div>
      </header>

      <!-- Main Content -->
      <main class="flex-1 overflow-hidden">
        <div class="max-w-7xl mx-auto h-full flex">
          <!-- Messages -->
          <div class="flex-1 flex flex-col">
            <!-- Messages List -->
            <div class="flex-1 overflow-y-auto p-4 space-y-4" #messagesContainer>
              <div
                *ngFor="let message of messages"
                class="bg-white rounded-lg shadow p-4"
                [ngStyle]="{ 'border-left': '4px solid ' + message.sentimentColor }"
              >
                <div class="flex justify-between items-start">
                  <div class="flex items-center space-x-2">
                    <span class="font-semibold text-gray-900">{{ message.username }}</span>
                    <span class="text-sm text-gray-500">{{ message.timestamp | date:'short' }}</span>
                  </div>
                  <div class="flex items-center space-x-2" *ngIf="message.username === username">
                    <button
                      (click)="editMessage(message)"
                      class="text-blue-500 hover:text-blue-600"
                    >
                      Edit
                    </button>
                    <button
                      (click)="deleteMessage(message.id)"
                      class="text-red-500 hover:text-red-600"
                    >
                      Delete
                    </button>
                  </div>
                </div>
                <p class="mt-2 text-gray-700">{{ message.message }}</p>
                <div class="mt-2 flex items-center space-x-2 text-sm text-gray-500">
                  <span *ngIf="message.isEdited">(edited)</span>
                  <span *ngIf="message.lastEditedAt">Last edited: {{ message.lastEditedAt | date:'short' }}</span>
                </div>
              </div>
            </div>

            <!-- Message Input -->
            <div class="border-t bg-white p-4">
              <form (ngSubmit)="sendMessage()" class="flex space-x-4">
                <input
                  type="text"
                  [(ngModel)]="newMessage"
                  name="message"
                  placeholder="Type your message..."
                  class="flex-1 rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  (keyup)="onTyping()"
                />
                <button
                  type="submit"
                  class="bg-blue-500 hover:bg-blue-600 text-white px-6 py-2 rounded-md transition-colors"
                  [disabled]="!newMessage.trim()"
                >
                  Send
                </button>
              </form>
            </div>
          </div>

          <!-- Online Users -->
          <div class="w-64 bg-white border-l p-4">
            <h2 class="text-lg font-semibold text-gray-900 mb-4">Online Users</h2>
            <ul class="space-y-2">
              <li
                *ngFor="let user of onlineUsers"
                class="flex items-center space-x-2 text-gray-700"
              >
                <span class="w-2 h-2 bg-green-500 rounded-full"></span>
                <span>{{ user }}</span>
              </li>
            </ul>
          </div>
        </div>
      </main>
    </div>
  `,
  styles: []
})
export class ChatComponent implements OnInit, OnDestroy {
  messages: ChatMessage[] = [];
  newMessage = '';
  username: string | null = null;
  onlineUsers: string[] = [];
  private subscriptions: Subscription[] = [];

  constructor(
    private apiService: ApiService,
    private signalRService: SignalRService,
    private authService: AuthService
  ) {
    this.username = this.authService.getUser();
  }

  ngOnInit(): void {
    this.loadMessages();
    this.setupSignalR();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.signalRService.stopConnection();
  }

  private loadMessages(): void {
    this.apiService.getRecentMessages().subscribe({
      next: (messages) => {
        this.messages = messages;
      },
      error: (error) => {
        console.error('Error loading messages:', error);
      }
    });
  }

  private setupSignalR(): void {
    this.signalRService.startConnection();

    this.subscriptions.push(
      this.signalRService.getMessageObservable().subscribe(message => {
        if (message) {
          this.messages.push(message);
        }
      }),

      this.signalRService.getUserJoinedObservable().subscribe(user => {
        if (user && !this.onlineUsers.includes(user)) {
          this.onlineUsers.push(user);
        }
      }),

      this.signalRService.getUserLeftObservable().subscribe(user => {
        if (user) {
          this.onlineUsers = this.onlineUsers.filter(u => u !== user);
        }
      })
    );
  }

  sendMessage(): void {
    if (this.newMessage.trim()) {
      this.signalRService.sendMessage(this.newMessage);
      this.newMessage = '';
    }
  }

  onTyping(): void {
    this.signalRService.sendTyping();
  }

  editMessage(message: ChatMessage): void {
    const newMessage = prompt('Edit message:', message.message);
    if (newMessage && newMessage !== message.message) {
      this.apiService.updateMessage(message.id, newMessage).subscribe({
        next: (updatedMessage) => {
          const index = this.messages.findIndex(m => m.id === message.id);
          if (index !== -1) {
            this.messages[index] = updatedMessage;
          }
        },
        error: (error) => {
          console.error('Error updating message:', error);
        }
      });
    }
  }

  deleteMessage(messageId: number): void {
    if (confirm('Are you sure you want to delete this message?')) {
      this.apiService.deleteMessage(messageId).subscribe({
        next: () => {
          this.messages = this.messages.filter(m => m.id !== messageId);
        },
        error: (error) => {
          console.error('Error deleting message:', error);
        }
      });
    }
  }

  logout(): void {
    this.authService.logout();
  }
} 