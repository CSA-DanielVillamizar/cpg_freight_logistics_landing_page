import { useMemo, useState } from 'react';
import type { FormEvent } from 'react';
import { Button, Card, Input } from '@/shared/ui';
import { cn } from '@/shared/lib/cn';
import type { RateCalculationRequest, ServiceType } from '@/shared/api/types';
import { RateBreakdown } from './RateBreakdown';
import { getServiceLine, SERVICE_LINES } from './serviceLines';
import { useRateCalculator } from './useRateCalculator';

const METRICS = [
  { value: '12 min', label: 'Avg dispatch' },
  { value: '< 500 ms', label: 'Quote engine' },
  { value: '48 states', label: 'Corridor permits' },
];

export function RateCalculatorPage(): JSX.Element {
  const [serviceType, setServiceType] = useState<ServiceType>('ColdChain');
  const [originZip, setOriginZip] = useState('33101');
  const [destinationZip, setDestinationZip] = useState('32801');
  const [weightLbs, setWeightLbs] = useState('35000');
  const [targetTemp, setTargetTemp] = useState('-20');

  const { status, result, fieldErrors, errorMessage, calculate } = useRateCalculator();

  const activeLine = useMemo(() => getServiceLine(serviceType), [serviceType]);

  const errorProp = (key: string): { error: string } | Record<string, never> => {
    const message = fieldErrors[key]?.[0];
    return message === undefined ? {} : { error: message };
  };

  function handleSubmit(event: FormEvent<HTMLFormElement>): void {
    event.preventDefault();
    const base: RateCalculationRequest = {
      serviceType,
      originZip: originZip.trim(),
      destinationZip: destinationZip.trim(),
      weightLbs: Number(weightLbs),
    };
    const request: RateCalculationRequest = activeLine.requiresTemperature
      ? { ...base, targetTemperatureCelsius: Number(targetTemp) }
      : base;
    void calculate(request);
  }

  return (
    <div className="mx-auto grid max-w-container gap-8 px-4 py-10 lg:grid-cols-[1.15fr_0.85fr]">
      <div className="flex flex-col gap-6">
        <header className="flex flex-col gap-2">
          <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
            Precision Freight Quoting · SPEC.md US-02
          </span>
          <h1 className="text-headline-lg">Interactive Rate Calculator</h1>
          <p className="text-body-sm text-steel-gray">
            Select the service line and lane. The engine returns a base rate, cold-chain surcharge
            and fuel surcharge in under 500&#160;ms — no external geocoding round trip.
          </p>
          <dl className="mt-2 grid grid-cols-3 gap-2">
            {METRICS.map((metric) => (
              <div key={metric.label} className="rounded border border-outline bg-surface-card p-3">
                <dt className="font-mono text-label-md font-semibold text-fleet-blue">{metric.value}</dt>
                <dd className="font-mono text-label-sm uppercase text-steel-gray">{metric.label}</dd>
              </div>
            ))}
          </dl>
        </header>

        <Card className="p-6">
          <form className="flex flex-col gap-5" onSubmit={handleSubmit}>
            <fieldset className="flex flex-col gap-2">
              <legend className="font-mono text-label-sm uppercase tracking-wide text-steel-gray">
                Service line
              </legend>
              <div className="grid gap-2 sm:grid-cols-2">
                {SERVICE_LINES.map((line) => {
                  const selected = line.value === serviceType;
                  return (
                    <button
                      key={line.value}
                      type="button"
                      aria-pressed={selected}
                      onClick={() => setServiceType(line.value)}
                      className={cn(
                        'flex flex-col items-start gap-1 rounded border p-3 text-left transition-colors',
                        selected
                          ? 'border-primary bg-primary text-white'
                          : 'border-outline bg-surface-card hover:bg-surface-muted',
                      )}
                    >
                      <span className="font-heading text-label-md uppercase tracking-wide">
                        {line.label}
                      </span>
                      <span
                        className={cn(
                          'text-body-sm',
                          selected ? 'text-white/70' : 'text-steel-gray',
                        )}
                      >
                        {line.blurb}
                      </span>
                    </button>
                  );
                })}
              </div>
            </fieldset>

            <div className="grid gap-4 sm:grid-cols-2">
              <Input
                label="Origin ZIP"
                value={originZip}
                onChange={(event) => setOriginZip(event.target.value)}
                inputMode="numeric"
                maxLength={5}
                required
                {...errorProp('OriginZip')}
              />
              <Input
                label="Destination ZIP"
                value={destinationZip}
                onChange={(event) => setDestinationZip(event.target.value)}
                inputMode="numeric"
                maxLength={5}
                required
                {...errorProp('DestinationZip')}
              />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <Input
                label="Gross weight (lbs)"
                type="number"
                min={1}
                max={200000}
                value={weightLbs}
                onChange={(event) => setWeightLbs(event.target.value)}
                required
                {...errorProp('WeightLbs')}
              />
              {activeLine.requiresTemperature ? (
                <Input
                  label="Target temperature (°C)"
                  type="number"
                  value={targetTemp}
                  onChange={(event) => setTargetTemp(event.target.value)}
                  required
                  {...errorProp('TargetTemperatureCelsius')}
                />
              ) : null}
            </div>

            <Button type="submit" disabled={status === 'loading'}>
              {status === 'loading' ? 'Calculating…' : 'Calculate rate'}
            </Button>

            {status === 'error' && errorMessage ? (
              <p className="font-mono text-body-sm text-error">{errorMessage}</p>
            ) : null}
          </form>
        </Card>
      </div>

      <div className="flex flex-col gap-4">
        {status === 'success' && result ? (
          <RateBreakdown result={result} />
        ) : (
          <Card className="flex h-full min-h-48 flex-col items-center justify-center gap-2 p-6 text-center">
            <span className="font-mono text-label-sm uppercase tracking-wider text-steel-gray">
              Quote breakdown
            </span>
            <p className="text-body-sm text-steel-gray">
              Submit a lane to see the base rate, surcharges and all-inclusive total.
            </p>
          </Card>
        )}
      </div>
    </div>
  );
}
