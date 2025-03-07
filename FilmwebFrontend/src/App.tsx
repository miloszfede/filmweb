import { 
  BrowserRouter as Router, 
  useRoutes
} from 'react-router-dom';
import { routes } from './router';
import './App.css';

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
