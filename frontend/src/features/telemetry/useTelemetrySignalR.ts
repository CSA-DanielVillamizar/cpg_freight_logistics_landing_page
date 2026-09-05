import { useEffect, useRef, useState } from 'react';
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from '@microsoft/signalr';
import { currentAccessToken } from '@/shared/api/client';
import { resolveApiOrigin } from '@/shared/config/runtime';
import type { TelemetryReading } from './types';

export type TelemetryConnectionState = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

interface UseTelemetrySignalROptions {
  enabled: boolean;
  onReading: (reading: TelemetryReading) => void;
}

const RECEIVE_EVENT = 'ReceiveTelemetryUpdate';

/**
 * Opens a JWT-authenticated SignalR connection to `/hubs/telemetry`, keeps it alive with
 * automatic reconnection and forwards every `ReceiveTelemetryUpdate` payload to `onReading`.
 * The connection is torn down on unmount / when `enabled` goes false.
 */
export function useTelemetrySignalR({
  enabled,
  onReading,
}: UseTelemetrySignalROptions): TelemetryConnectionState {
  const [state, setState] = useState<TelemetryConnectionState>('disconnected');
  const onReadingRef = useRef(onReading);
  onReadingRef.current = onReading;

  useEffect(() => {
    if (!enabled) {
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
      onReadingRef.current(reading);
    });

    connection.onreconnecting(() => setState('reconnecting'));
    connection.onreconnected(() => setState('connected'));
    connection.onclose(() => {
      if (!disposed) {
        setState('disconnected');
      }
    });

    setState('connecting');
    connection
      .start()
      .then(() => {
        if (!disposed) {
          setState('connected');
        }
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
        void connection.stop();
      }
    };
  }, [enabled]);

  return state;
}
