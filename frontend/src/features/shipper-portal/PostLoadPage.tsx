import { useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { loadsApi } from '@/features/load-board/api/loadsApi';
import type { CreateLoadInput, LoadServiceType } from '@/features/load-board/types';
import { LOAD_SERVICE_TYPES } from '@/features/load-board/types';
import { useAuth } from '@/features/auth/useAuth';
import { ApiError } from '@/shared/api/client';
import { Button, Card, Input } from '@/shared/ui';
import { ShipperNav } from './ShipperNav';

interface FormState {
  reference: string;
  serviceType: LoadServiceType;
  equipmentType: string;
  shipperName: string;
  originCity: string;
  originState: string;
  originZip: string;
  destinationCity: string;
  destinationState: string;
  destinationZip: string;
  distanceMiles: string;
  weightLbs: string;
  rateUsd: string;
  pickupAt: string;
  deliveryAt: string;
  targetTemperatureF: string;
  specialInstructions: string;
}

type Errors = Partial<Record<keyof FormState, string>>;

const ZIP = /^\d{5}$/;
const REFERENCE = /^CPG-[A-Za-z0-9-]{2,30}$/;

function initialState(shipperName: string): FormState {
  return {
    reference: '',
    serviceType: 'Flatbed',
    equipmentType: '',
    shipperName,
    originCity: '',
    originState: '',
    originZip: '',
    destinationCity: '',
    destinationState: '',
    destinationZip: '',
    distanceMiles: '',
    weightLbs: '',
    rateUsd: '',
    pickupAt: '',
    deliveryAt: '',
    targetTemperatureF: '',
    specialInstructions: '',
  };
}

function validate(form: FormState): Errors {
  const errors: Errors = {};
  const required = (key: keyof FormState, label: string): void => {
    if (!form[key].trim()) {
      errors[key] = `${label} is required.`;
    }
  };

  if (form.reference.trim() && !REFERENCE.test(form.reference.trim())) {
    errors.reference = 'Use the form CPG-XXXXX, or leave blank to auto-generate.';
  }
  required('equipmentType', 'Equipment type');
  required('shipperName', 'Shipper name');
  required('originCity', 'Origin city');
  required('destinationCity', 'Destination city');

  for (const key of ['originState', 'destinationState'] as const) {
    if (form[key].trim().length !== 2) {
      errors[key] = 'Two-letter state.';
    }
  }
  for (const key of ['originZip', 'destinationZip'] as const) {
    if (!ZIP.test(form[key].trim())) {
      errors[key] = 'Five-digit ZIP.';
    }
  }

  const miles = Number(form.distanceMiles);
  if (!Number.isFinite(miles) || miles <= 0 || miles > 6000) {
    errors.distanceMiles = 'Between 1 and 6000 miles.';
  }
  const weight = Number(form.weightLbs);
  if (!Number.isFinite(weight) || weight <= 0 || weight > 200_000) {
    errors.weightLbs = 'Between 1 and 200,000 lb.';
  }
  const rate = Number(form.rateUsd);
  if (!Number.isFinite(rate) || rate <= 0 || rate > 1_000_000) {
    errors.rateUsd = 'Between $1 and $1,000,000.';
  }

  if (!form.pickupAt) {
    errors.pickupAt = 'Pickup date/time is required.';
  }
  if (!form.deliveryAt) {
    errors.deliveryAt = 'Delivery date/time is required.';
  } else if (form.pickupAt && new Date(form.deliveryAt) <= new Date(form.pickupAt)) {
    errors.deliveryAt = 'Delivery must be after pickup.';
  }

  if (form.targetTemperatureF.trim()) {
    const temp = Number(form.targetTemperatureF);
    if (!Number.isFinite(temp) || temp < -40 || temp > 120) {
      errors.targetTemperatureF = 'Between -40°F and 120°F.';
    }
  }
  if (form.specialInstructions.length > 1000) {
    errors.specialInstructions = 'Keep it under 1000 characters.';
  }

  return errors;
}

function toPayload(form: FormState): CreateLoadInput {
  const payload: CreateLoadInput = {
    serviceType: form.serviceType,
    equipmentType: form.equipmentType.trim(),
    shipperName: form.shipperName.trim(),
    originCity: form.originCity.trim(),
    originState: form.originState.trim().toUpperCase(),
    originZip: form.originZip.trim(),
    destinationCity: form.destinationCity.trim(),
    destinationState: form.destinationState.trim().toUpperCase(),
    destinationZip: form.destinationZip.trim(),
    distanceMiles: Number(form.distanceMiles),
    weightLbs: Number(form.weightLbs),
    rateUsd: Number(form.rateUsd),
    pickupAtUtc: new Date(form.pickupAt).toISOString(),
    deliveryAtUtc: new Date(form.deliveryAt).toISOString(),
  };
  if (form.reference.trim()) {
    payload.reference = form.reference.trim();
  }
  if (form.targetTemperatureF.trim()) {
    payload.targetTemperatureF = Number(form.targetTemperatureF);
  }
  if (form.specialInstructions.trim()) {
    payload.specialInstructions = form.specialInstructions.trim();
  }
  return payload;
}

export function PostLoadPage(): JSX.Element {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [form, setForm] = useState<FormState>(() => initialState(user?.fullName ?? ''));
  const [errors, setErrors] = useState<Errors>({});
  const [submitting, setSubmitting] = useState(false);

  const set = <K extends keyof FormState>(key: K, value: FormState[K]): void => {
    setForm((prev) => ({ ...prev, [key]: value }));
    setErrors((prev) => {
      if (!prev[key]) {
        return prev;
      }
      const next = { ...prev };
      delete next[key];
      return next;
    });
  };

  const errorProp = (key: keyof FormState): { error: string } | Record<string, never> => {
    const message = errors[key];
    return message === undefined ? {} : { error: message };
  };

  async function handleSubmit(event: FormEvent): Promise<void> {
    event.preventDefault();
    const found = validate(form);
    if (Object.values(found).some(Boolean)) {
      setErrors(found);
      toast.error('Fix the highlighted fields and try again.');
      return;
    }

    setSubmitting(true);
    try {
      const load = await loadsApi.create(toPayload(form));
      toast.success(`Load ${load.reference} posted — it is live on the board.`);
      navigate('/load-board');
    } catch (error) {
      if (error instanceof ApiError && error.status === 400) {
        const fieldErrors = Object.entries(error.problem?.errors ?? {}).reduce<Errors>(
          (acc, [key, messages]) => {
            const camel = key.charAt(0).toLowerCase() + key.slice(1);
            acc[camel as keyof FormState] = messages.join(' ');
            return acc;
          },
          {},
        );
        setErrors(fieldErrors);
        toast.error(error.problem?.detail ?? 'The load was rejected — check the fields.');
      } else if (error instanceof ApiError && error.status === 403) {
        toast.error('Only shipper accounts can post loads.');
      } else {
        toast.error('Could not post the load — please try again.');
      }
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="mx-auto flex max-w-container flex-col gap-6 px-4 py-8">
      <header className="flex flex-col gap-2">
        <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
          Shipper portal
        </span>
        <h1 className="text-headline-lg">Post a Load</h1>
        <p className="max-w-2xl text-body-sm text-steel-gray">
          Your load goes onto the Carrier &amp; Shipper Load Workspace as{' '}
          <span className="font-semibold">Available</span> the moment you post it. A carrier accepts
          it, then it shows on your Shipments board and the invoice is raised on delivery.
        </p>
      </header>

      <ShipperNav />

      <form onSubmit={(event) => void handleSubmit(event)} className="flex flex-col gap-6" noValidate>
        <Card className="flex flex-col gap-4 p-6">
          <h2 className="text-headline-sm">Freight</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="flex flex-col gap-1">
              <label
                htmlFor="serviceType"
                className="text-xs font-semibold uppercase tracking-wider text-steel-gray"
              >
                Service line
              </label>
              <select
                id="serviceType"
                value={form.serviceType}
                onChange={(event) => set('serviceType', event.target.value as LoadServiceType)}
                className="h-12 rounded border border-outline-strong bg-surface-card px-3 text-[16px] text-on-surface outline-none transition-colors focus:border-fleet-blue focus:ring-2 focus:ring-fleet-blue/25"
              >
                {LOAD_SERVICE_TYPES.map((line) => (
                  <option key={line.value} value={line.value}>
                    {line.label}
                  </option>
                ))}
              </select>
            </div>
            <Input
              label="Equipment type"
              placeholder="e.g. 48' Flatbed"
              value={form.equipmentType}
              onChange={(event) => set('equipmentType', event.target.value)}
              {...errorProp('equipmentType')}
            />
            <Input
              label="Shipper name"
              value={form.shipperName}
              onChange={(event) => set('shipperName', event.target.value)}
              {...errorProp('shipperName')}
            />
            <Input
              label="Board reference (optional)"
              placeholder="CPG-XXXXX — leave blank to auto-generate"
              value={form.reference}
              onChange={(event) => set('reference', event.target.value)}
              {...errorProp('reference')}
            />
          </div>
        </Card>

        <Card className="flex flex-col gap-4 p-6">
          <h2 className="text-headline-sm">Lane</h2>
          <div className="grid gap-4 sm:grid-cols-3">
            <Input
              label="Origin city"
              value={form.originCity}
              onChange={(event) => set('originCity', event.target.value)}
              {...errorProp('originCity')}
            />
            <Input
              label="State"
              maxLength={2}
              placeholder="FL"
              value={form.originState}
              onChange={(event) => set('originState', event.target.value)}
              {...errorProp('originState')}
            />
            <Input
              label="ZIP"
              inputMode="numeric"
              maxLength={5}
              placeholder="32801"
              value={form.originZip}
              onChange={(event) => set('originZip', event.target.value)}
              {...errorProp('originZip')}
            />
            <Input
              label="Destination city"
              value={form.destinationCity}
              onChange={(event) => set('destinationCity', event.target.value)}
              {...errorProp('destinationCity')}
            />
            <Input
              label="State"
              maxLength={2}
              placeholder="GA"
              value={form.destinationState}
              onChange={(event) => set('destinationState', event.target.value)}
              {...errorProp('destinationState')}
            />
            <Input
              label="ZIP"
              inputMode="numeric"
              maxLength={5}
              placeholder="30301"
              value={form.destinationZip}
              onChange={(event) => set('destinationZip', event.target.value)}
              {...errorProp('destinationZip')}
            />
          </div>
          <Input
            label="Distance (miles)"
            inputMode="numeric"
            placeholder="438"
            value={form.distanceMiles}
            onChange={(event) => set('distanceMiles', event.target.value)}
            {...errorProp('distanceMiles')}
          />
        </Card>

        <Card className="flex flex-col gap-4 p-6">
          <h2 className="text-headline-sm">Load &amp; schedule</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            <Input
              label="Gross weight (lb)"
              inputMode="numeric"
              placeholder="39200"
              value={form.weightLbs}
              onChange={(event) => set('weightLbs', event.target.value)}
              {...errorProp('weightLbs')}
            />
            <Input
              label="All-in rate (USD)"
              inputMode="decimal"
              placeholder="2180"
              value={form.rateUsd}
              onChange={(event) => set('rateUsd', event.target.value)}
              {...errorProp('rateUsd')}
            />
            <Input
              label="Pickup"
              type="datetime-local"
              value={form.pickupAt}
              onChange={(event) => set('pickupAt', event.target.value)}
              {...errorProp('pickupAt')}
            />
            <Input
              label="Delivery"
              type="datetime-local"
              value={form.deliveryAt}
              onChange={(event) => set('deliveryAt', event.target.value)}
              {...errorProp('deliveryAt')}
            />
            <Input
              label="Target temp (°F, cold chain only)"
              inputMode="numeric"
              placeholder="-10"
              value={form.targetTemperatureF}
              onChange={(event) => set('targetTemperatureF', event.target.value)}
              {...errorProp('targetTemperatureF')}
            />
          </div>
          <div className="flex flex-col gap-1">
            <label
              htmlFor="specialInstructions"
              className="text-xs font-semibold uppercase tracking-wider text-steel-gray"
            >
              Special instructions (optional)
            </label>
            <textarea
              id="specialInstructions"
              rows={3}
              maxLength={1000}
              value={form.specialInstructions}
              onChange={(event) => set('specialInstructions', event.target.value)}
              className="rounded border border-outline-strong bg-surface-card px-3 py-2 text-[16px] text-on-surface outline-none transition-colors focus:border-fleet-blue focus:ring-2 focus:ring-fleet-blue/25"
            />
            {errors.specialInstructions ? (
              <p className="text-body-sm text-error">{errors.specialInstructions}</p>
            ) : null}
          </div>
        </Card>

        <div className="flex flex-wrap gap-3">
          <Button type="submit" variant="primary" disabled={submitting}>
            {submitting ? 'Posting…' : 'Post load to board'}
          </Button>
          <Button type="button" variant="outline" onClick={() => navigate('/shipper/dashboard')}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}
