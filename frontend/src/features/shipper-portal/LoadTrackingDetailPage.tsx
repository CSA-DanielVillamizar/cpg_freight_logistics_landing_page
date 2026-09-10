import { loadsApi } from '@/features/load-board/api/loadsApi';
import type { Load } from '@/features/load-board/types';
import { LiveMap } from '@/features/telemetry/components/LiveMap';
import type { TelemetryReading } from '@/features/telemetry/types';
import type { SignalRConnectionState } from '@/features/telemetry/useSignalRConnection';
import { useSignalRConnection } from '@/features/telemetry/useSignalRConnection';
import { cn } from '@/shared/lib/cn';
import { formatEnum } from '@/shared/lib/formatEnum';
import { Card, EmptyState } from '@/shared/ui';
import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

const MAX_TRAIL = 100;

const CONNECTION_META: Record<SignalRConnectionState, { label: string; dot: string }> = {
    connecting: { label: 'Connecting', dot: 'bg-safety-amber' },
    connected: { label: 'Live', dot: 'bg-success' },
    reconnecting: { label: 'Reconnecting', dot: 'bg-safety-amber animate-pulse' },
    disconnected: { label: 'Offline', dot: 'bg-signal-red' },
};

/** Great-circle initial bearing from `from` to `to`, in degrees (0 = north). */
function bearingDeg(from: [number, number], to: [number, number]): number {
    const toRad = (deg: number): number => (deg * Math.PI) / 180;
    const [lat1, lon1] = [toRad(from[0]), toRad(from[1])];
    const [lat2, lon2] = [toRad(to[0]), toRad(to[1])];
    const dLon = lon2 - lon1;
    const y = Math.sin(dLon) * Math.cos(lat2);
    const x = Math.cos(lat1) * Math.sin(lat2) - Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);
    return ((Math.atan2(y, x) * 180) / Math.PI + 360) % 360;
}

/**
 * /shipper/loads/:loadId/tracking — the Shipper's single-load live map (T-SDD Epica 2A).
 * Seeds the GPS trail from `GET /api/loads/{id}/telemetry-history`, then joins the load's
 * SignalR group for real-time updates as the carrier's ELD reports new positions.
 */
export function LoadTrackingDetailPage(): JSX.Element {
    const { loadId } = useParams<{ loadId: string }>();
    const [load, setLoad] = useState<Load | null>(null);
    const [trail, setTrail] = useState<[number, number][]>([]);
    const [status, setStatus] = useState<'loading' | 'ready' | 'error'>('loading');

    useEffect(() => {
        if (!loadId) {
            return undefined;
        }
        let cancelled = false;

        Promise.all([loadsApi.list(), loadsApi.getTelemetryHistory(loadId, 100)])
            .then(([loads, history]) => {
                if (cancelled) {
                    return;
                }
                setLoad(loads.find((entry) => entry.id === loadId) ?? null);
                // History is newest-first; the map/trail want oldest-first.
                setTrail(history.map((entry) => [entry.latitude, entry.longitude] as [number, number]).reverse());
                setStatus('ready');
            })
            .catch(() => {
                if (!cancelled) {
                    setStatus('error');
                }
            });

        return () => {
            cancelled = true;
        };
    }, [loadId]);

    const onReading = useCallback((reading: TelemetryReading) => {
        setTrail((previous) =>
            [...previous, [reading.latitude, reading.longitude] as [number, number]].slice(-MAX_TRAIL),
        );
    }, []);

    const connectionState = useSignalRConnection(loadId, {
        enabled: status === 'ready',
        onReading,
    });

    const current = trail.at(-1);
    const previous = trail.at(-2);
    const headingDeg = current && previous ? bearingDeg(previous, current) : 0;
    const connection = CONNECTION_META[connectionState];

    return (
        <div className="mx-auto flex max-w-container flex-col gap-5 px-4 py-8">
            <header className="flex flex-wrap items-start justify-between gap-3">
                <div className="flex flex-col gap-2">
                    <span className="text-xs font-semibold uppercase tracking-wider text-steel-gray">
                        Live Tracking
                    </span>
                    <h1 className="text-headline-lg">
                        {load ? `${load.originCity}, ${load.originState} → ${load.destinationCity}, ${load.destinationState}` : 'Load Tracking'}
                    </h1>
                    {load ? (
                        <p className="text-body-sm text-steel-gray">
                            {formatEnum(load.serviceType)} ·{' '}
                            <span className="font-mono normal-case tracking-normal">{load.reference}</span>
                        </p>
                    ) : null}
                </div>
                <span className="inline-flex items-center gap-2 rounded-full border border-slate-200 bg-surface-card px-3 py-1.5 text-xs font-semibold uppercase tracking-wider text-steel-gray">
                    <span className={cn('h-2 w-2 rounded-full', connection.dot)} />
                    {connection.label}
                </span>
            </header>

            {status === 'error' ? (
                <Card className="border-error bg-error-container p-4 text-body-sm text-error">
                    Unable to load tracking data — check that you are signed in.
                </Card>
            ) : status === 'loading' ? (
                <EmptyState icon="progress_activity" title="Loading tracking data…" />
            ) : !current ? (
                <EmptyState
                    icon="satellite_alt"
                    title="No GPS position reported yet"
                    hint="The carrier's ELD hasn't sent a position for this load yet. The map appears here as soon as the first reading arrives."
                />
            ) : (
                <Card raised className="overflow-hidden">
                    <div className="aspect-[4/3] w-full sm:aspect-[16/9]">
                        <LiveMap latitude={current[0]} longitude={current[1]} headingDeg={headingDeg} trail={trail} />
                    </div>
                </Card>
            )}
        </div>
    );
}
