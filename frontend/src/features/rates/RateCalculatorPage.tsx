import { useState } from 'react';
import type { FormEvent } from 'react';
import { Button, Card, Input } from '@/shared/ui';
import type { RateCalculationRequest, ServiceType } from '@/shared/api/types';
import { useRateCalculator } from './useRateCalculator';

const SERVICE_TYPES: readonly { value: ServiceType; label: string }[] = [
  { value: 'ColdChain', label: 'Cold Chain (reefer)' },
  { value: 'HeavyHaul', label: 'Heavy Haul / Superload' },
  { value: 'Flatbed', label: 'Flatbed / Step-Deck' },
  { value: 'FdotConcrete', label: 'FDOT Concrete Barricades' },
];

const CURRENCY = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });

export function RateCalculatorPage(): JSX.Element {
  const [serviceType, setServiceType] = useState<ServiceType>('ColdChain');
  const [originZip, setOriginZip] = useState('33101');
  const [destinationZip, setDestinationZip] = useState('32801');
  const [weightLbs, setWeightLbs] = useState('35000');
  const [targetTemp, setTargetTemp] = useState('-20');

  const { status, result, errorMessage, calculate } = useRateCalculator();

  function handleSubmit(event: FormEvent<HTMLFormElement>): void {
    event.preventDefault();
    const request: RateCalculationRequest = {
      serviceType,
      originZip,
      destinationZip,
      weightLbs: Number(weightLbs),
      ...(serviceType === 'ColdChain' ? { targetTemperatureCelsius: Number(targetTemp) } : {}),
    };
    void calculate(request);
  }

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6 px-4 py-10">
      <header className="flex flex-col gap-1">
        <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
          Precision Freight Quoting
        </span>
        <h1 className="text-headline-lg">Interactive Rate Calculator</h1>
        <p className="text-body-sm text-steel-gray">
          Contract: <code>POST /api/rates/calculate</code> — base rate, cold-chain surcharge and fuel
          surcharge, in under 500&#160;ms (SPEC.md US-02).
        </p>
      </header>

      <Card className="p-6">
        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <div className="flex flex-col gap-1">
            <label
              htmlFor="service-type"
              className="font-mono text-label-sm uppercase tracking-wide text-steel-gray"
            >
              Service line
            </label>
            <select
              id="service-type"
              className="h-12 rounded border border-outline bg-surface-card px-3 text-[16px]"
              value={serviceType}
              onChange={(event) => setServiceType(event.target.value as ServiceType)}
            >
              {SERVICE_TYPES.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <Input
              label="Origin ZIP"
              value={originZip}
              onChange={(event) => setOriginZip(event.target.value)}
              inputMode="numeric"
              required
            />
            <Input
              label="Destination ZIP"
              value={destinationZip}
              onChange={(event) => setDestinationZip(event.target.value)}
              inputMode="numeric"
              required
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <Input
              label="Gross weight (lbs)"
              type="number"
              value={weightLbs}
              onChange={(event) => setWeightLbs(event.target.value)}
              required
            />
            {serviceType === 'ColdChain' ? (
              <Input
                label="Target temperature (°C)"
                type="number"
                value={targetTemp}
                onChange={(event) => setTargetTemp(event.target.value)}
              />
            ) : null}
          </div>

          <Button type="submit" disabled={status === 'loading'}>
            {status === 'loading' ? 'Calculating…' : 'Calculate rate'}
          </Button>
        </form>
      </Card>

      {status === 'error' ? (
        <Card className="border-error bg-error-container p-4 text-body-sm text-error">
          {errorMessage}
        </Card>
      ) : null}

      {status === 'success' && result ? (
        <Card anchored className="flex flex-col gap-2 p-6">
          <h2 className="text-headline-sm">Lane estimate</h2>
          <dl className="grid grid-cols-2 gap-2 font-mono text-label-md">
            <dt className="text-steel-gray">Base rate</dt>
            <dd className="text-right">{CURRENCY.format(result.baseRate)}</dd>
            <dt className="text-steel-gray">Cold-chain surcharge</dt>
            <dd className="text-right">{CURRENCY.format(result.coldChainSurcharge)}</dd>
            <dt className="text-steel-gray">Fuel surcharge</dt>
            <dd className="text-right">{CURRENCY.format(result.fuelSurcharge)}</dd>
            <dt className="text-on-surface">Total estimated</dt>
            <dd className="text-right font-semibold text-hazard-orange">
              {CURRENCY.format(result.totalEstimated)}
            </dd>
          </dl>
        </Card>
      ) : null}
    </div>
  );
}
