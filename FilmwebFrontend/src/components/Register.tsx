import React, { useState } from 'react';
import AuthService from '../services/authService';
import './Register.css';

const Register: React.FC = () => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [successful, setSuccessful] = useState(false);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage('');
    setLoading(true);
    
    try {
      await AuthService.register(username, email, password);
      setSuccessful(true);
      setMessage('Registration successful! You can now login.');
    } catch (error: any) {
      setSuccessful(false);
      setMessage((error.response && error.response.data) || 
                 error.message || 
                 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="register-container">
      <h2>Register</h2>
      {message && (
        <div className={`alert ${successful ? 'alert-success' : 'alert-danger'}`}>
          {message}
        </div>
      )}
      {!successful && (
        <form onSubmit={handleRegister}>
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              type="text"
              className="form-control"
              id="username"
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              type="email"
              className="form-control"
              id="email"
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              type="password"
              className="form-control"
              id="password"
              required
            />
          </div>
          <div className="form-group">
            <button className="btn btn-primary" disabled={loading}>
              {loading ? 'Loading...' : 'Register'}
            </button>
          </div>
        </form>
      )}
    </div>
  );
};

export default Register;