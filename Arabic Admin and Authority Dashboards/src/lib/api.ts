// Mock API functions that simulate the backend endpoints

import { mockUsers, mockReports, mockComments, mockInteractions, User, Report, Comment, Interaction } from './mock-data';
import { http, toQuery } from './http';

// Helper function to simulate API delay
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

// Admin APIs
export const adminApi = {
  // User Management
  async getAllUsers(): Promise<User[]> {
    try {
      const res = await http<{ users: User[] }>(`/api/admin/users`);
      return res.users;
    } catch {
      await delay(500);
      return [...mockUsers];
    }
  },

  async getUserById(userId: string): Promise<User | null> {
    try {
      return await http<User>(`/api/admin/users/${userId}`);
    } catch {
      await delay(300);
      return mockUsers.find(user => user.id === userId) || null;
    }
  },

  async updateUser(userId: string, userData: Partial<User>): Promise<User> {
    try {
      await http(`/api/admin/users/${userId}`, { method: 'PUT', body: userData });
      return (await this.getUserById(userId)) as User;
    } catch {
      await delay(500);
      const userIndex = mockUsers.findIndex(user => user.id === userId);
      if (userIndex === -1) throw new Error('User not found');
      
      mockUsers[userIndex] = { ...mockUsers[userIndex], ...userData } as User;
      return mockUsers[userIndex];
    }
  },

  async deleteUser(userId: string): Promise<void> {
    try {
      await http(`/api/admin/users/${userId}`, { method: 'DELETE' });
    } catch {
      await delay(500);
      const userIndex = mockUsers.findIndex(user => user.id === userId);
      if (userIndex === -1) throw new Error('User not found');
      
      mockUsers.splice(userIndex, 1);
    }
  },

  // Reports Management
  async getAllReports(filters?: {
    status?: string;
    category?: string;
    priority?: string;
    dateFrom?: string;
    dateTo?: string;
    authority?: string;
    assignedOfficer?: string;
  }): Promise<Report[]> {
    try {
      const qs = toQuery({
        status: filters?.status,
        category: filters?.category,
        priority: filters?.priority,
      });
      const list = await http<Report[]>(`/api/feed${qs}`);
      return list;
    } catch {
      await delay(500);
      let filteredReports = [...mockReports];
      if (filters) {
        if (filters.status) filteredReports = filteredReports.filter(report => report.status === filters.status);
        if (filters.category) filteredReports = filteredReports.filter(report => report.category === filters.category);
        if (filters.priority) filteredReports = filteredReports.filter(report => report.priority === filters.priority);
        if (filters.authority) filteredReports = filteredReports.filter(report => report.authorityName.includes(filters.authority!));
      }
      return filteredReports;
    }
  },

  async deleteReport(reportId: string): Promise<void> {
    try {
      await http(`/api/reports/${reportId}`, { method: 'DELETE' });
    } catch {
      await delay(500);
      const reportIndex = mockReports.findIndex(report => report.id === reportId);
      if (reportIndex === -1) throw new Error('Report not found');
      
      mockReports.splice(reportIndex, 1);
    }
  }
};

// Authority APIs
export const authorityApi = {
  async getAuthorityReports(userId: string): Promise<Report[]> {
    try {
      return await http<Report[]>(`/api/users/${userId}/reports`);
    } catch {
      await delay(500);
      const user = mockUsers.find(u => u.id === userId);
      if (!user || user.role !== 'authority') return [];
      
      return mockReports.filter(report => report.authorityId === userId);
    }
  }
};

// Shared APIs
export const reportsApi = {
  async getReportById(reportId: string): Promise<Report | null> {
    try {
      return await http<Report>(`/api/reports/${reportId}`);
    } catch {
      await delay(300);
      return mockReports.find(report => report.id === reportId) || null;
    }
  },

  async updateReportStatus(reportId: string, status: string): Promise<Report> {
    try {
      await http(`/api/reports/${reportId}/status`, { method: 'PATCH', body: status });
      return (await this.getReportById(reportId)) as Report;
    } catch {
      await delay(500);
      const reportIndex = mockReports.findIndex(report => report.id === reportId);
      if (reportIndex === -1) throw new Error('Report not found');
      
      mockReports[reportIndex] = {
        ...mockReports[reportIndex],
        status: status as any,
        updatedAt: new Date().toISOString().split('T')[0]
      } as Report;
      
      return mockReports[reportIndex];
    }
  },

  async addComment(reportId: string, userId: string, content: string): Promise<Comment> {
    try {
      return await http<Comment>(`/api/reports/${reportId}/comments`, { method: 'POST', body: { content } });
    } catch {
      await delay(500);
      const user = mockUsers.find(u => u.id === userId);
      if (!user) throw new Error('User not found');
      
      const newComment: Comment = {
        id: (mockComments.length + 1).toString(),
        reportId,
        userId,
        userName: user.name,
        content,
        createdAt: new Date().toISOString()
      };
      
      mockComments.push(newComment);
      return newComment;
    }
  },

  async getComments(reportId: string): Promise<Comment[]> {
    try {
      return await http<Comment[]>(`/api/reports/${reportId}/comments`);
    } catch {
      await delay(300);
      return mockComments.filter(comment => comment.reportId === reportId);
    }
  },

  async getInteractions(reportId: string): Promise<Interaction[]> {
    try {
      return await http<Interaction[]>(`/api/reports/${reportId}/interactions`);
    } catch {
      await delay(300);
      return mockInteractions.filter(interaction => interaction.reportId === reportId);
    }
  },

  async likeReport(reportId: string, userId: string): Promise<void> {
    try {
      await http(`/api/reports/${reportId}/like`, { method: 'POST' });
    } catch {
      await delay(300);
      const user = mockUsers.find(u => u.id === userId);
      if (!user) throw new Error('User not found');
      
      const newInteraction: Interaction = {
        id: (mockInteractions.length + 1).toString(),
        reportId,
        userId,
        userName: user.name,
        type: 'like',
        createdAt: new Date().toISOString()
      };
      
      mockInteractions.push(newInteraction);
      
      // Update likes count in report
      const reportIndex = mockReports.findIndex(report => report.id === reportId);
      if (reportIndex !== -1) {
        mockReports[reportIndex].likes += 1;
      }
    }
  }
};