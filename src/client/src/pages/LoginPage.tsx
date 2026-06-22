import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getUserStatus } from '../api/client';
import { useAuth } from '../auth/AuthContext';

export function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const status = await getUserStatus(email.trim());

      if (!status.exists) {
        setError('No account found for that email. Create a user first.');
        return;
      }

      if (status.status !== 'active') {
        setError('This account is inactive.');
        return;
      }

      if (!status.id) {
        setError('Unable to resolve user id for this account.');
        return;
      }

      login({ id: status.id, email: email.trim().toLowerCase() });
      navigate('/tasks');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="page">
      <div className="card">
        <h1>Sign in</h1>
        <p className="subtitle">Enter your email to access your tasks.</p>

        <form onSubmit={handleSubmit}>
          <div className="field">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="you@example.com"
              required
              autoFocus
            />
          </div>

          {error && <p className="error">{error}</p>}

          <button type="submit" disabled={loading}>
            {loading ? 'Checking...' : 'Continue'}
          </button>
        </form>

        <p className="footer-link">
          New here? <Link to="/create-user">Create User</Link>
        </p>
      </div>
    </div>
  );
}
