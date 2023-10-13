/* eslint-disable prettier/prettier */
import { Message, Session } from '@/js/types';
declare const API_URL: string;

export default {
  async getSessions() {
    return await $fetch(`${API_URL}/sessions`) as Array<Session>;
  },

  async addSession() {
    return await $fetch(`${API_URL}/sessions`, { method: 'POST' }) as Session;
  },

  async deleteSession(sessionId: string) {
    return await $fetch(`${API_URL}/sessions/${sessionId}`, { method: 'DELETE' }) as Session;
  },

  async getMessages(sessionId: string): Promise<Array<Message>> {
    return await $fetch(`${API_URL}/sessions/${sessionId}/messages`) as Array<Message>;
  },

  async rateMessage(message: Message, rating: Message['rating']) {
    message.rating === rating
      ? (message.rating = null)
      : (message.rating = rating);

    return await $fetch(
      `${API_URL}/sessions/${message.sessionId}/message/${message.id}/rate${message.rating !== null ? '?rating=' + message.rating : ''}`, {
        method: 'POST',
      }
    ) as Message;
  },

  async sendMessage(sessionId: string, text: string) {
    return (await $fetch(`${API_URL}/sessions/${sessionId}/completion`, {
      method: 'POST',
      body: JSON.stringify(text),
    })) as string;
  },
};
