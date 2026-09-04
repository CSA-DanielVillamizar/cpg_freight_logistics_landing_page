import { useState } from 'react';
import type { FormEvent } from 'react';
import { ApiError, apiClient } from '@/shared/api/client';
import type { CreateLeadRequest, CreateLeadResponse, ServiceType } from '@/shared/api/types';
import { Button, Input } from '@/shared/ui';

interface LeadCaptureFormProps {
  verticalSlug: string;
  serviceType: ServiceType;
}

type Status = 'idle' | 'submitting' | 'submitted' | 'error';

export function LeadCaptureForm({ verticalSlug, serviceType }: LeadCaptureFormProps): JSX.Element {
  const [companyName, setCompanyName] = useState('');
  const [contactEmail, setContactEmail] = useState('');
  const [cargoDetails, setCargoDetails] = useState('');
  const [status, setStatus] = useState<Status>('idle');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setStatus('submitting');
    setErrorMessage(null);

    const request: CreateLeadRequest = {
      companyName,
      contactEmail,
      verticalSlug,
      serviceType,
      ...(cargoDetails ? { cargoDetails } : {}),
    };

    try {
      await apiClient.post<CreateLeadResponse>('/leads', request);
      setStatus('submitted');
    } catch (error) {
      setErrorMessage(
        error instanceof ApiError
          ? (error.problem?.detail ?? error.message)
          : 'Unable to submit the inquiry right now.',
      );
      setStatus('error');
    }
  }

  if (status === 'submitted') {
    return (
      <p className="rounded-lg border border-success bg-success-container p-4 text-body-sm text-success">
        Thanks — our commercial team has your inquiry and will follow up shortly.
      </p>
    );
  }

  return (
    <form className="flex flex-col gap-4" onSubmit={(event) => void handleSubmit(event)}>
      <Input
        label="Company name"
        value={companyName}
        onChange={(event) => setCompanyName(event.target.value)}
        required
      />
      <Input
        label="Work email"
        type="email"
        value={contactEmail}
        onChange={(event) => setContactEmail(event.target.value)}
        required
      />
      <Input
        label="Cargo details"
        value={cargoDetails}
        onChange={(event) => setCargoDetails(event.target.value)}
        hint="Dimensions, weight, lanes, timing"
      />
      {status === 'error' && errorMessage ? (
        <p className="text-body-sm text-error">{errorMessage}</p>
      ) : null}
      <Button type="submit" disabled={status === 'submitting'}>
        {status === 'submitting' ? 'Submitting…' : 'Request enterprise quote'}
      </Button>
    </form>
  );
}
