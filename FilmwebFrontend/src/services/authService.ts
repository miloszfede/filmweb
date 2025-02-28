import axios from 'axios';

const API_URL = 'http://localhost:5000/api/auth/';

export interface User {
  userId: number;
  username: string;
  email: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export class AuthService {
  async login(username: string, password: string): Promise<User> {
    const response = await axios.post(API_URL + 'login', {
      username,
      password,
    });
    
    if (response.data) {
      localStorage.setItem('user', JSON.stringify(response.data));
    }
    
    return response.data;
  }

  logout(): void {
    localStorage.removeItem('user');
  }

  async register(username: string, email: string, password: string): Promise<User> {
    const response = await axios.post(API_URL + 'register', {
      username,
      email,
      password,
    });
    
    return response.data;
  }

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      return JSON.parse(userStr);
    }
    
    return null;
  }
}

export default new AuthService();