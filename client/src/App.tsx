import './App.css';
import Layout from './components/Layout';
import WelcomeDashboard from './components/Dashboard/WelcomeDashboard';
import HeroMessage from './components/HeroMessage';
import SearchBar from './components/SearchBar';
import Gallery from './components/Gallery';

const App: React.FC = () => {
  return (
    <Layout>
      <WelcomeDashboard />
      <HeroMessage />
      <SearchBar />
      <Gallery />
    </Layout>
  );
};

export default App;
