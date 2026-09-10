import { useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { ApiError } from '@/shared/api/client';
import type { RegisterRequest, UserRole } from '@/shared/api/types';
import { Button, Card, Input } from '@/shared/ui';
import { useAuth } from './useAuth';
import { ROLE_HOME } from './roleHome';

interface SignUpFormShellProps {
  role: Extract<UserRole, 'Shipper' | 'Carrier' | 'Agent'>;
  eyebrow: string;
  title: string;
  subtitle: string;
  /** Label for the company-name field; Shippers see "Company name (optional)". */
  companyFieldLabel: string;
  companyRequired: boolean;
  /** Carrier-only: collects DOT/MC up-front to seed the compliance profile. */
  showCarrierLicenseFields?: boolean;
}

/**
 * Shared registration form for the three Tri-Sign-Up roles (T-SDD Epica 1). Renders the
 * role-specific fields, posts to `POST /api/auth/register`, then redirects to the
 * matching workspace once the account (and JWT session) is created.
 */
export function SignUpFormShell({
  role,
  eyebrow,
  title,
  subtitle,
  companyFieldLabel,
  companyRequired,
  showCarrierLicenseFields = false,
}: SignUpFormShellProps): JSX.Element {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [companyName, setCompanyName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [dotNumber, setDotNumber] = useState('');
  const [mcNumber, setMcNumber] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setSubmitting(true);
    setErrorMessage(null);
    setFieldErrors({});

    const request: RegisterRequest = {
      email,
      password,
      fullName,
      role,
      ...(companyName.trim() ? { companyName: companyName.trim() } : {}),
      ...(phoneNumber.trim() ? { phoneNumber: phoneNumber.trim() } : {}),
      ...(showCarrierLicenseFields && dotNumber.trim() ? { dotNumber: dotNumber.trim() } : {}),
      ...(showCarrierLicenseFields && mcNumber.trim() ? { mcNumber: mcNumber.trim() } : {}),
    };

    try {
      const user = await register(request);
      navigate(ROLE_HOME[user.role], { replace: true });
    } catch (caught) {
      if (caught instanceof ApiError && caught.status === 409) {
        setErrorMessage('An account with this email already exists. Try signing in instead.');
      } else if (caught instanceof ApiError && caught.status === 400 && caught.problem?.errors) {
        setFieldErrors(caught.problem.errors);
        setErrorMessage('Please fix the highlighted fields.');
      } else {
        setErrorMessage('Unable to create your account right now. Try again shortly.');
      }
    } finally {
      setSubmitting(false);
    }
  }

  const errorProp = (field: string): { error: string } | Record<string, never> => {
    const message = fieldErrors[field]?.[0];
    return message === undefined ? {} : { error: message };
  };

  return (
    <div className="mx-auto flex max-w-md flex-col gap-6 px-4 py-16">
      <header className="flex flex-col gap-1">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          {eyebrow}
        </span>
        <h1 className="text-headline-lg">{title}</h1>
        <p className="text-body-sm text-steel-gray">{subtitle}</p>
      </header>

      <Card raised className="p-6">
        <form className="flex flex-col gap-4" onSubmit={(event) => void handleSubmit(event)}>
          <Input
            label="Full name"
            autoComplete="name"
            value={fullName}
            onChange={(event) => setFullName(event.target.value)}
            required
            {...errorProp('FullName')}
          />
          <Input
            label="Work email"
            type="email"
            autoComplete="username"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
            {...errorProp('Email')}
          />
          <Input
            label="Password"
            type="password"
            autoComplete="new-password"
            hint="At least 12 characters, with upper/lowercase, a digit and a symbol."
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            {...errorProp('Password')}
          />
          <Input
            label={companyFieldLabel}
            autoComplete="organization"
            value={companyName}
            onChange={(event) => setCompanyName(event.target.value)}
            required={companyRequired}
            {...errorProp('CompanyName')}
          />
          <Input
            label="Phone (optional)"
            type="tel"
            autoComplete="tel"
            value={phoneNumber}
            onChange={(event) => setPhoneNumber(event.target.value)}
            {...errorProp('PhoneNumber')}
          />
          {showCarrierLicenseFields ? (
            <div className="grid grid-cols-2 gap-3">
              <Input
                label="DOT number (optional)"
                value={dotNumber}
                onChange={(event) => setDotNumber(event.target.value)}
                {...errorProp('DotNumber')}
              />
              <Input
                label="MC number (optional)"
                value={mcNumber}
                onChange={(event) => setMcNumber(event.target.value)}
                {...errorProp('McNumber')}
              />
            </div>
          ) : null}
          {errorMessage ? <p className="text-body-sm text-error">{errorMessage}</p> : null}
          <Button type="submit" disabled={submitting}>
            {submitting ? 'Creating account…' : 'Create account'}
          </Button>
        </form>
      </Card>
    </div>
  );
}
