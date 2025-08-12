import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider } from './contexts/ThemeContext';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import ConversationsList from './components/ConversationsList';
import ConversationDetail from './components/ConversationDetail';
import './App.css';

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <Router>
          <div className="App">
            <ProtectedRoute>
              <Routes>
                <Route path="/" element={<ConversationsList />} />
                <Route path="/conversation/:id" element={<ConversationDetail />} />
              </Routes>
            </ProtectedRoute>
          </div>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
