import { currentAccessToken } from '@/shared/api/client';
import { resolveApiOrigin } from '@/shared/config/runtime';
import {
    HubConnectionBuilder,
    HubConnectionState,
    LogLevel,
    type HubConnection,
} from '@microsoft/signalr';
import { useEffect, useRef, useState } from 'react';
import type { TelemetryReading } from './types';

export type SignalRConnectionState = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

interface UseSignalRConnectionOptions {
    enabled: boolean;
    onReading: (reading: TelemetryReading) => void;
}

const RECEIVE_EVENT = 'ReceiveTelemetryUpdate';
const JOIN_METHOD = 'JoinLoadGroupAsync';
const LEAVE_METHOD = 'LeaveLoadGroupAsync';

/**
 * Opens a JWT-authenticated SignalR connection to `/hubs/telemetry` and joins the
 * `load-{loadId}` group — the server only allows the load's Shipper or an Admin to join
 * (T-SDD Epica 2A). Every `ReceiveTelemetryUpdate` for this load is forwarded to `onReading`;
 * readings for other loads (fleet-wide simulator broadcasts) are ignored. Rejoins the group
 * automatically after a reconnect.
 */
export function useSignalRConnection(
    loadId: string | undefined,
    { enabled, onReading }: UseSignalRConnectionOptions,
): SignalRConnectionState {
    const [state, setState] = useState<SignalRConnectionState>('disconnected');
    const onReadingRef = useRef(onReading);
    onReadingRef.current = onReading;

    useEffect(() => {
        if (!enabled || !loadId) {
            return undefined;
        }

        const connection: HubConnection = new HubConnectionBuilder()
            .withUrl(`${resolveApiOrigin()}/hubs/telemetry`, {
                accessTokenFactory: () => currentAccessToken() ?? '',
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 15000])
            .configureLogging(LogLevel.Warning)
            .build();

        let disposed = false;

        connection.on(RECEIVE_EVENT, (reading: TelemetryReading) => {
            if (reading.loadId === loadId) {
                onReadingRef.current(reading);
            }
        });

        const joinGroup = (): void => {
            connection.invoke(JOIN_METHOD, loadId).catch(() => {
                /* the hub rejects unauthorized joins; the UI just stays on its last known position */
            });
        };

        connection.onreconnecting(() => setState('reconnecting'));
        connection.onreconnected(() => {
            setState('connected');
            joinGroup();
        });
        connection.onclose(() => {
            if (!disposed) {
                setState('disconnected');
            }
        });

        setState('connecting');
        connection
            .start()
            .then(() => {
                if (disposed) {
                    return;
                }
                setState('connected');
                joinGroup();
            })
            .catch(() => {
                if (!disposed) {
                    setState('disconnected');
                }
            });

        return () => {
            disposed = true;
            connection.off(RECEIVE_EVENT);
            if (connection.state !== HubConnectionState.Disconnected) {
                connection.invoke(LEAVE_METHOD, loadId).catch(() => undefined);
                void connection.stop();
            }
        };
    }, [enabled, loadId]);

    return state;
}
