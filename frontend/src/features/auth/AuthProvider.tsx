import { useCallback, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { registerAuthBridge } from '@/shared/api/client';
import type { AuthResponse, AuthenticatedUser } from '@/shared/api/types';
import { AuthContext } from './authContext';
import type { AuthContextValue } from './authContext';
import { authApi } from './authApi';

const STORAGE_KEY = 'cpg.auth.session';

interface Session {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: AuthenticatedUser;
}

function readStoredSession(): Session | null {
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as Session) : null;
  } catch {
    return null;
  }
}

function persistSession(session: Session | null): void {
  try {
    if (session) {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } else {
      window.localStorage.removeItem(STORAGE_KEY);
    }
  } catch {
    /* storage unavailable - session stays in memory only */
  }
}

export function AuthProvider({ children }: { children: ReactNode }): JSX.Element {
  const [session, setSession] = useState<Session | null>(() => readStoredSession());
  const sessionRef = useRef<Session | null>(session);
  sessionRef.current = session;
  const refreshInFlight = useRef<Promise<boolean> | null>(null);

  const applySession = useCallback((response: AuthResponse) => {
    const next: Session = {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresAtUtc: response.expiresAtUtc,
      user: response.user,
    };
    // Update the ref synchronously so a 401 retry mid-refresh reads the new token
    // immediately, before React has re-rendered.
    sessionRef.current = next;
    setSession(next);
    persistSession(next);
  }, []);

  const logout = useCallback(() => {
    sessionRef.current = null;
    setSession(null);
    persistSession(null);
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await authApi.login({ email, password });
      applySession(response);
      return response.user;
    },
    [applySession],
  );

  const bridge = useMemo(
    () => ({
      getAccessToken: () => sessionRef.current?.accessToken ?? null,
      refresh: (): Promise<boolean> => {
        // Single-flight: concurrent 401s must not each rotate the refresh token
        // (the second rotation would fail and force a spurious logout).
        if (refreshInFlight.current) {
          return refreshInFlight.current;
        }

        const current = sessionRef.current;
        if (!current) {
          return Promise.resolve(false);
        }

        const attempt = authApi
          .refresh({ refreshToken: current.refreshToken })
          .then((response) => {
            applySession(response);
            return true;
          })
          .catch(() => false)
          .finally(() => {
            refreshInFlight.current = null;
          });

        refreshInFlight.current = attempt;
        return attempt;
      },
      onAuthLost: () => logout(),
    }),
    [applySession, logout],
  );

  // Register during render (not in an effect): child effects run before parent
  // effects, so the first authenticated request on a protected page would otherwise
  // fire before the bridge exists, 401, and clear the session. No cleanup — the
  // provider is a root singleton for the app's lifetime, and StrictMode's simulated
  // unmount/remount would otherwise leave the bridge null.
  registerAuthBridge(bridge);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      isAuthenticated: session !== null,
      hasRole: (role) => session?.user.role === role,
      login,
      logout,
    }),
    [session, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
