import axios from 'axios';

const API_URL = "http://localhost:5112/api/auth/"; // Use your correct port

interface AuthResponse {
  id: number;
  username: string;
  email: string;
  token: string;
}

const register = async (username: string, email: string, password: string) => {
  return axios.post(API_URL + "register", {
    username,
    email,
    password
  });
};

const login = async (username: string, password: string) => {
  const response = await axios.post<AuthResponse>(API_URL + "login", {
    username,
    password
  });
  
  if (response.data && response.data.token) {
    localStorage.setItem("user", JSON.stringify(response.data));
  }
  
  return response.data;
};

const logout = () => {
  localStorage.removeItem("user");
};

const getCurrentUser = () => {
  const userStr = localStorage.getItem("user");
  if (userStr) return JSON.parse(userStr);
  return null;
};

const setupAxiosInterceptors = () => {
  axios.interceptors.request.use(
    (config) => {
      const user = getCurrentUser();
      if (user && user.token) {
        config.headers['Authorization'] = 'Bearer ' + user.token;
      }
      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );
};

setupAxiosInterceptors();

const AuthService = {
  register,
  login,
  logout,
  getCurrentUser,
  setupAxiosInterceptors
};

export default AuthService;