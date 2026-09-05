import { createBrowserRouter } from 'react-router-dom';
import { App } from '@/App';
import { AuditLogsPage } from '@/features/admin/AuditLogsPage';
import { LoginPage } from '@/features/auth/LoginPage';
import { RequireRole } from '@/features/auth/RequireRole';
import { CarrierPortalPage } from '@/features/carrier-portal/CarrierPortalPage';
import { LandingPage } from '@/features/landing/LandingPage';
import { LoadBoardPage } from '@/features/load-board/LoadBoardPage';
import { VerticalLandingPage } from '@/features/landing/VerticalLandingPage';
import { RateCalculatorPage } from '@/features/rates/RateCalculatorPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      { index: true, element: <LandingPage /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'rates', element: <RateCalculatorPage /> },
      { path: 'load-board', element: <LoadBoardPage /> },
      { path: 'verticals/:slug', element: <VerticalLandingPage /> },
      {
        path: 'admin/audit-logs',
        element: (
          <RequireRole role="Admin">
            <AuditLogsPage />
          </RequireRole>
        ),
      },
      {
        path: 'carrier',
        element: (
          <RequireRole role="Carrier">
            <CarrierPortalPage />
          </RequireRole>
        ),
      },
    ],
  },
]);
