import { useState } from 'react';
import type { FormEvent } from 'react';
import { toast } from 'sonner';
import { ApiError, apiClient } from '@/shared/api/client';
import type { CreateLeadRequest, ServiceType } from '@/shared/api/types';
import { Button, Input } from '@/shared/ui';

interface CreateLeadResponse {
  id: string;
  status: string;
}

interface LeadCaptureFormProps {
  verticalSlug: string;
  serviceType: ServiceType;
  cargoPlaceholder: string;
}

type Status = 'idle' | 'submitting' | 'submitted' | 'error';

type FieldKey = 'CompanyName' | 'ContactName' | 'ContactEmail' | 'Phone' | 'CargoDetails';

export function LeadCaptureForm({
  verticalSlug,
  serviceType,
  cargoPlaceholder,
}: LeadCaptureFormProps): JSX.Element {
  const [companyName, setCompanyName] = useState('');
  const [contactName, setContactName] = useState('');
  const [contactEmail, setContactEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [cargoDetails, setCargoDetails] = useState('');
  const [status, setStatus] = useState<Status>('idle');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  const errorProp = (key: FieldKey): { error: string } | Record<string, never> => {
    const message = fieldErrors[key]?.[0];
    return message === undefined ? {} : { error: message };
  };

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setStatus('submitting');
    setFieldErrors({});

    const request: CreateLeadRequest = {
      companyName,
      contactName,
      contactEmail,
      phone,
      verticalSlug,
      serviceType,
      cargoDetails,
    };

    try {
      await apiClient.post<CreateLeadResponse>('/leads', request, { anonymous: true });
      setStatus('submitted');
      toast.success('Inquiry received — our commercial team will follow up shortly.');
    } catch (caught) {
      if (caught instanceof ApiError) {
        setFieldErrors(caught.problem?.errors ?? {});
        toast.error(caught.problem?.detail ?? 'Please review the highlighted fields.');
      } else {
        toast.error('Unable to submit right now — please try again.');
      }
      setStatus('error');
    }
  }

  if (status === 'submitted') {
    return (
      <div className="rounded-lg border border-success bg-success-container p-5 text-body-sm text-success">
        <p className="font-heading text-label-md uppercase tracking-wide">Thank you</p>
        <p className="mt-1">
          Our commercial team has your inquiry and a dispatcher will reach out within one business
          hour.
        </p>
      </div>
    );
  }

  return (
    <form className="flex flex-col gap-4" onSubmit={(event) => void handleSubmit(event)}>
      <Input
        label="Company name"
        value={companyName}
        onChange={(event) => setCompanyName(event.target.value)}
        required
        {...errorProp('CompanyName')}
      />
      <div className="grid gap-4 sm:grid-cols-2">
        <Input
          label="Your name"
          value={contactName}
          onChange={(event) => setContactName(event.target.value)}
          autoComplete="name"
          required
          {...errorProp('ContactName')}
        />
        <Input
          label="Work phone"
          type="tel"
          value={phone}
          onChange={(event) => setPhone(event.target.value)}
          autoComplete="tel"
          required
          {...errorProp('Phone')}
        />
      </div>
      <Input
        label="Work email"
        type="email"
        value={contactEmail}
        onChange={(event) => setContactEmail(event.target.value)}
        autoComplete="email"
        required
        {...errorProp('ContactEmail')}
      />
      <div className="flex flex-col gap-1">
        <label
          htmlFor="cargo-details"
          className="font-mono text-label-sm uppercase tracking-wide text-steel-gray"
        >
          Cargo &amp; service details
        </label>
        <textarea
          id="cargo-details"
          className="min-h-24 rounded border border-outline bg-surface-card p-3 text-[16px] outline-none focus:border-primary focus:ring-2 focus:ring-safety-amber/60"
          value={cargoDetails}
          onChange={(event) => setCargoDetails(event.target.value)}
          placeholder={cargoPlaceholder}
          required
        />
        {fieldErrors.CargoDetails?.[0] ? (
          <p className="text-body-sm text-error">{fieldErrors.CargoDetails[0]}</p>
        ) : null}
      </div>
      <Button type="submit" disabled={status === 'submitting'}>
        {status === 'submitting' ? 'Sending…' : 'Request enterprise quote'}
      </Button>
      <p className="font-mono text-label-sm text-steel-gray">
        100% free spec · zero obligation · DOT verified
      </p>
    </form>
  );
}
