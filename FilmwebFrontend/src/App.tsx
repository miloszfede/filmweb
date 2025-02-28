import React from 'react';
import { 
  BrowserRouter as Router, 
  Routes, 
  Route,
  useRoutes
} from 'react-router-dom';
import { routes } from './router';
import './App.css';

// AppRoutes component to use the useRoutes hook
const AppRoutes = () => {
  const routeElements = useRoutes(routes);
  return routeElements;
};

function App() {
  return (
    <Router>
      <AppRoutes />
    </Router>
  );
}

export default App;
