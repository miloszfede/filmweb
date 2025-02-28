import React from 'react';
import { 
  Navigate, 
  RouteObject, 
  useLocation
} from 'react-router-dom';
import Home from '../pages/Home';  // Updated import path
import Login from '../components/Login';
import Register from '../components/Register';
import Profile from '../components/Profile';

// Create a wrapper component to handle protected routes
interface ProtectedRouteProps {
  children: React.ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const location = useLocation();
  const loggedIn = localStorage.getItem('user') !== null;

  if (!loggedIn) {
    // Redirect to login if not authenticated, but remember where they were going
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
};

// Define all routes
export const routes: RouteObject[] = [
  {
    path: '/',
    element: <Home />
  },
  {
    path: '/home',
    element: <Home />
  },
  {
    path: '/login',
    element: <Login />
  },
  {
    path: '/register',
    element: <Register />
  },
  {
    path: '/profile',
    element: (
      <ProtectedRoute>
        <Profile />
      </ProtectedRoute>
    )
  },
  {
    path: '*',
    element: <Navigate to="/" replace />
  }
];

// Helper function for route navigation
export const isPublicPage = (path: string): boolean => {
  const publicPages = ['/', '/home', '/login', '/register'];
  return publicPages.some(page => 
    path === page || (page !== '/' && path.startsWith(page + '/'))
  );
};