import { useState } from 'react';
import { toast } from 'sonner';
import { ApiError } from '@/shared/api/client';
import { Button, Card } from '@/shared/ui';
import { payoutsApi } from './payoutsApi';

/** /carrier/settings/stripe-connect — hosted Stripe Connect onboarding (T-SDD Epica 2B). */
export function StripeConnectOnboardingPage(): JSX.Element {
  const [submitting, setSubmitting] = useState(false);

  async function handleConnect(): Promise<void> {
    setSubmitting(true);
    try {
      const { accountLinkUrl } = await payoutsApi.connectStripe();
      window.location.href = accountLinkUrl;
    } catch (caught) {
      toast.error(
        caught instanceof ApiError && caught.status === 409
          ? 'This account is already connected to Stripe.'
          : 'Could not start Stripe onboarding — please retry.',
      );
      setSubmitting(false);
    }
  }

  return (
    <div className="mx-auto flex max-w-md flex-col gap-6 px-4 py-16">
      <header className="flex flex-col gap-1">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Carrier portal · Settings
        </span>
        <h1 className="text-headline-lg">Connect your bank account</h1>
        <p className="text-body-sm text-steel-gray">
          CPG uses Stripe Connect to pay you directly — your banking details are hosted by
          Stripe and never touch CPG's servers.
        </p>
      </header>

      <Card raised className="flex flex-col gap-4 p-6">
        <p className="text-body-sm text-steel-gray">
          You'll be redirected to Stripe's secure onboarding flow. It takes a few minutes and
          you can resume anytime if you don't finish in one sitting.
        </p>
        <Button type="button" disabled={submitting} onClick={() => void handleConnect()}>
          {submitting ? 'Redirecting…' : 'Connect with Stripe'}
        </Button>
      </Card>
    </div>
  );
}
