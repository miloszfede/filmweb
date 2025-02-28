import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import AuthService from '../services/authService';
import './Profile.css';

interface User {
  userId?: number;
  username?: string;
  email?: string;
}

const Profile: React.FC = () => {
  const [currentUser, setCurrentUser] = useState<User>({});
  const navigate = useNavigate();

  useEffect(() => {
    const user = AuthService.getCurrentUser();
    if (!user) {
      navigate('/login');
    } else {
      setCurrentUser(user);
    }
  }, [navigate]);

  return (
    <div className="profile-container">
      <header className="jumbotron">
        <h3>
          <strong>{currentUser.username}</strong> Profile
        </h3>
      </header>
      <p>
        <strong>Id:</strong>
        {currentUser.userId}
      </p>
      <p>
        <strong>Email:</strong>
        {currentUser.email}
      </p>
    </div>
  );
};

export default Profile;