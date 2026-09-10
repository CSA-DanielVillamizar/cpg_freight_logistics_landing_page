import { loadsApi } from '@/features/load-board/api/loadsApi';
import type { CreateLoadInput, LoadServiceType } from '@/features/load-board/types';
import { LOAD_SERVICE_TYPES } from '@/features/load-board/types';
import { ApiError } from '@/shared/api/client';
import { Button, Card, Input } from '@/shared/ui';
import type { FormEvent } from 'react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { useAgentClients } from './useAgentClients';

interface FormState {
    clientUserId: string;
    shipperName: string;
    serviceType: LoadServiceType;
    equipmentType: string;
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
}

const INITIAL: FormState = {
    clientUserId: '',
    shipperName: '',
    serviceType: 'Flatbed',
    equipmentType: '',
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
};

/**
 * /agent/loads/new — an Agent publishes a load on behalf of one of their accepted clients
 * (T-SDD Epica 4). Reuses the same `POST /api/loads` contract as the Shipper's Post-Load form
 * (see `shipper-portal/PostLoadPage.tsx`), adding the client-selection step Agents need.
 */
export function PublishAgentLoadForm(): JSX.Element {
    const navigate = useNavigate();
    const { clients, status: clientsStatus } = useAgentClients();
    const acceptedClients = clients.filter((c) => c.status === 'Accepted' && c.acceptedByUserId);

    const [form, setForm] = useState<FormState>(INITIAL);
    const [submitting, setSubmitting] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    function update<K extends keyof FormState>(key: K, value: FormState[K]): void {
        setForm((previous) => ({ ...previous, [key]: value }));
    }

    async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
        event.preventDefault();
        setErrorMessage(null);

        if (!form.clientUserId) {
            setErrorMessage('Select which client this load is for.');
            return;
        }

        const input: CreateLoadInput = {
            serviceType: form.serviceType,
            equipmentType: form.equipmentType.trim(),
            shipperName: form.shipperName.trim(),
            shipperUserId: form.clientUserId,
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

        setSubmitting(true);
        try {
            const load = await loadsApi.create(input);
            toast.success(`Load ${load.reference} published.`);
            navigate('/agent');
        } catch (caught) {
            setErrorMessage(
                caught instanceof ApiError ? caught.problem?.detail ?? caught.message : 'Could not publish the load.',
            );
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8">
            <header className="flex flex-col gap-1">
                <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                    Independent Agent Workspace
                </span>
                <h1 className="text-headline-lg">Publish a load</h1>
            </header>

            <Card raised className="p-6">
                <form className="flex flex-col gap-4" onSubmit={(event) => void handleSubmit(event)}>
                    <div className="flex flex-col gap-1">
                        <label htmlFor="client" className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                            Client (Shipper)
                        </label>
                        <select
                            id="client"
                            className="h-12 rounded border border-outline-strong bg-surface-card px-3 text-[16px] text-on-surface outline-none focus:border-fleet-blue focus:ring-2 focus:ring-fleet-blue/25"
                            value={form.clientUserId}
                            onChange={(event) => update('clientUserId', event.target.value)}
                            required
                        >
                            <option value="" disabled>
                                {clientsStatus === 'loading' ? 'Loading clients…' : 'Select a client'}
                            </option>
                            {acceptedClients.map((client) => (
                                <option key={client.invitationId} value={client.acceptedByUserId ?? ''}>
                                    {client.invitedEmail}
                                </option>
                            ))}
                        </select>
                        {acceptedClients.length === 0 && clientsStatus === 'ready' ? (
                            <p className="text-body-sm text-steel-gray">
                                No accepted clients yet — invite one from the Clients page first.
                            </p>
                        ) : null}
                    </div>

                    <Input
                        label="Shipper company name"
                        value={form.shipperName}
                        onChange={(event) => update('shipperName', event.target.value)}
                        required
                    />

                    <div className="flex flex-col gap-1">
                        <label htmlFor="serviceType" className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                            Service type
                        </label>
                        <select
                            id="serviceType"
                            className="h-12 rounded border border-outline-strong bg-surface-card px-3 text-[16px] text-on-surface outline-none focus:border-fleet-blue focus:ring-2 focus:ring-fleet-blue/25"
                            value={form.serviceType}
                            onChange={(event) => update('serviceType', event.target.value as LoadServiceType)}
                        >
                            {LOAD_SERVICE_TYPES.map((option) => (
                                <option key={option.value} value={option.value}>
                                    {option.label}
                                </option>
                            ))}
                        </select>
                    </div>

                    <Input
                        label="Equipment type"
                        value={form.equipmentType}
                        onChange={(event) => update('equipmentType', event.target.value)}
                        required
                    />

                    <div className="grid grid-cols-3 gap-3">
                        <Input
                            label="Origin city"
                            value={form.originCity}
                            onChange={(event) => update('originCity', event.target.value)}
                            required
                        />
                        <Input
                            label="State"
                            value={form.originState}
                            onChange={(event) => update('originState', event.target.value)}
                            maxLength={2}
                            required
                        />
                        <Input
                            label="ZIP"
                            value={form.originZip}
                            onChange={(event) => update('originZip', event.target.value)}
                            required
                        />
                    </div>

                    <div className="grid grid-cols-3 gap-3">
                        <Input
                            label="Destination city"
                            value={form.destinationCity}
                            onChange={(event) => update('destinationCity', event.target.value)}
                            required
                        />
                        <Input
                            label="State"
                            value={form.destinationState}
                            onChange={(event) => update('destinationState', event.target.value)}
                            maxLength={2}
                            required
                        />
                        <Input
                            label="ZIP"
                            value={form.destinationZip}
                            onChange={(event) => update('destinationZip', event.target.value)}
                            required
                        />
                    </div>

                    <div className="grid grid-cols-3 gap-3">
                        <Input
                            label="Distance (mi)"
                            type="number"
                            value={form.distanceMiles}
                            onChange={(event) => update('distanceMiles', event.target.value)}
                            required
                        />
                        <Input
                            label="Weight (lb)"
                            type="number"
                            value={form.weightLbs}
                            onChange={(event) => update('weightLbs', event.target.value)}
                            required
                        />
                        <Input
                            label="Rate (USD)"
                            type="number"
                            value={form.rateUsd}
                            onChange={(event) => update('rateUsd', event.target.value)}
                            required
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-3">
                        <Input
                            label="Pickup"
                            type="datetime-local"
                            value={form.pickupAt}
                            onChange={(event) => update('pickupAt', event.target.value)}
                            required
                        />
                        <Input
                            label="Delivery"
                            type="datetime-local"
                            value={form.deliveryAt}
                            onChange={(event) => update('deliveryAt', event.target.value)}
                            required
                        />
                    </div>

                    {errorMessage ? <p className="text-body-sm text-error">{errorMessage}</p> : null}

                    <Button type="submit" disabled={submitting}>
                        {submitting ? 'Publishing…' : 'Publish load'}
                    </Button>
                </form>
            </Card>
        </div>
    );
}
