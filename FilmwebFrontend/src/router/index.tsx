import React from 'react';
import { 
  Navigate, 
  RouteObject, 
  useLocation
} from 'react-router-dom';
import Home from '../pages/Home';  
import Login from '../components/Login';
import Register from '../components/Register';
import Profile from '../components/Profile';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const location = useLocation();
  const loggedIn = localStorage.getItem('user') !== null;

  if (!loggedIn) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
};

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

export const isPublicPage = (path: string): boolean => {
  const publicPages = ['/', '/home', '/login', '/register'];
  return publicPages.some(page => 
    path === page || (page !== '/' && path.startsWith(page + '/'))
  );
};