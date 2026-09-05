import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
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

  const applySession = useCallback((response: AuthResponse) => {
    const next: Session = {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresAtUtc: response.expiresAtUtc,
      user: response.user,
    };
    setSession(next);
    persistSession(next);
  }, []);

  const logout = useCallback(() => {
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

  useEffect(() => {
    registerAuthBridge({
      getAccessToken: () => sessionRef.current?.accessToken ?? null,
      refresh: async () => {
        const current = sessionRef.current;
        if (!current) {
          return false;
        }
        try {
          applySession(await authApi.refresh({ refreshToken: current.refreshToken }));
          return true;
        } catch {
          return false;
        }
      },
      onAuthLost: logout,
    });

    return () => registerAuthBridge(null);
  }, [applySession, logout]);

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
