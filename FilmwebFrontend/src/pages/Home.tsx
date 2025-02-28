import React from 'react';
import { Link } from 'react-router-dom';
import './Home.css';

const Home: React.FC = () => {
  return (
    <div className="home">
      <h1>Welcome to Filmweb</h1>
      <p>Your ultimate movie database companion</p>
      
      <div className="features">
        <div className="feature">
          <h2>Browse Movies</h2>
          <p>Search and discover thousands of movies from TMDB</p>
        </div>
        
        <div className="feature">
          <h2>Create an Account</h2>
          <p>Register to save your favorite movies and create watchlists</p>
          <Link to="/register" className="btn">Sign Up</Link>
        </div>
        
        <div className="feature">
          <h2>Already a Member?</h2>
          <p>Log in to access your profile and saved content</p>
          <Link to="/login" className="btn">Log In</Link>
        </div>
      </div>
    </div>
  );
};

export default Home;