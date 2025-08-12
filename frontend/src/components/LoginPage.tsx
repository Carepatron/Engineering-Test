import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { ThemeToggle } from './ThemeToggle';
import './LoginPage.css';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login, isLoading } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    
    if (!email || !password) {
      setError('Please enter both email and password');
      return;
    }

    const success = await login(email, password);
    if (!success) {
      setError('Invalid email or password');
    }
  };

  const handleDemoLogin = (userType: 'staff' | 'patient') => {
    if (userType === 'staff') {
      setEmail('sarah@gmail.com');
      setPassword('password1');
    } else {
      setEmail('john.smith@email.com');
      setPassword('patient123');
    }
  };

  return (
    <div className="login-page">
      <div className="login-header">
        <h1>Hernia Surgical LLC</h1>
        <div className="login-theme-toggle">
          <ThemeToggle />
        </div>
      </div>
      
      <div className="login-container">
        <div className="login-card">
          <div className="login-title">
            <h2>Login</h2>
            <p>Access the patient conversation system</p>
          </div>

          <form onSubmit={handleSubmit} className="login-form">
            <div className="form-group">
              <label htmlFor="email">Email</label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Enter your email"
                disabled={isLoading}
                className="login-input"
                autoComplete="email"
              />
            </div>

            <div className="form-group">
              <label htmlFor="password">Password</label>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Enter your password"
                disabled={isLoading}
                className="login-input"
                autoComplete="current-password"
              />
            </div>

            {error && <div className="login-error">{error}</div>}

            <button
              type="submit"
              disabled={isLoading}
              className="login-button"
            >
              {isLoading ? 'Signing In...' : 'Sign In'}
            </button>
          </form>

          <div className="demo-section">
            <div className="demo-divider">
              <span>Demo Credentials</span>
            </div>
            <div className="demo-buttons">
              <button
                type="button"
                onClick={() => handleDemoLogin('staff')}
                disabled={isLoading}
                className="demo-button staff-demo"
              >
                Staff Login
              </button>
              <button
                type="button"
                onClick={() => handleDemoLogin('patient')}
                disabled={isLoading}
                className="demo-button patient-demo"
              >
                Patient Login
              </button>
            </div>
            <div className="demo-info">
              <div className="demo-credentials">
                <div className="credential-group">
                  <strong>Staff:</strong><br />
                  <small>sarah@gmail.com / password1</small>
                </div>
                <div className="credential-group">
                  <strong>Patient:</strong><br />
                  <small>john.smith@email.com / patient123</small>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};