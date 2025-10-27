import './App.css';
import Layout from './components/Layout';
import WelcomeDashboard from './components/Dashboard/WelcomeDashboard';
import HeroMessage from './components/HeroMessage';
import SearchBar from './components/SearchBar';

const App: React.FC = () => {
  return (
    <Layout>
      <WelcomeDashboard />
      <HeroMessage />
      <SearchBar />
    </Layout>
  );
};

export default App;
