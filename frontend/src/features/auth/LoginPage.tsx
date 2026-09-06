import { useState } from 'react';
import type { FormEvent } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { ApiError } from '@/shared/api/client';
import type { UserRole } from '@/shared/api/types';
import { Button, Card, Input } from '@/shared/ui';
import { useAuth } from './useAuth';

interface LocationState {
  from?: { pathname: string };
}

/** Default landing route per role when the user wasn't intercepted en route to a specific page. */
const ROLE_HOME: Record<UserRole, string> = {
  Admin: '/admin/carriers',
  Carrier: '/carrier',
  Shipper: '/shipper/dashboard',
};

const SEED_ACCOUNTS = [
  { role: 'Admin', email: 'admin@cpgorlando.com' },
  { role: 'Carrier', email: 'carrier@cpgorlando.com' },
  { role: 'Shipper', email: 'shipper@cpgorlando.com' },
];

export function LoginPage(): JSX.Element {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const explicitFrom = (location.state as LocationState | null)?.from?.pathname;

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      const user = await login(email, password);
      navigate(explicitFrom ?? ROLE_HOME[user.role], { replace: true });
    } catch (caught) {
      setError(
        caught instanceof ApiError && caught.status === 401
          ? 'Invalid email or password.'
          : 'Unable to sign in right now. Try again shortly.',
      );
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="mx-auto flex max-w-md flex-col gap-6 px-4 py-16">
      <header className="flex flex-col gap-1">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Secure Access
        </span>
        <h1 className="text-headline-lg">Sign in to CPG Enterprises</h1>
      </header>

      <Card raised className="p-6">
        <form className="flex flex-col gap-4" onSubmit={(event) => void handleSubmit(event)}>
          <Input
            label="Email"
            type="email"
            autoComplete="username"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
          />
          <Input
            label="Password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
          />
          {error ? <p className="text-body-sm text-error">{error}</p> : null}
          <Button type="submit" disabled={submitting}>
            {submitting ? 'Signing in…' : 'Sign in'}
          </Button>
        </form>
      </Card>

      <Card className="p-4">
        <p className="mb-2 text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Development seed accounts — password <code>Passw0rd!</code>
        </p>
        <ul className="flex flex-col gap-1 font-mono text-body-sm text-on-surface-variant">
          {SEED_ACCOUNTS.map((account) => (
            <li key={account.email}>
              <button
                type="button"
                className="text-fleet-blue hover:underline"
                onClick={() => {
                  setEmail(account.email);
                  setPassword('Passw0rd!');
                }}
              >
                {account.role}: {account.email}
              </button>
            </li>
          ))}
        </ul>
      </Card>
    </div>
  );
}
