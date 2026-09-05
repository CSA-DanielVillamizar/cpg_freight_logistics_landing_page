import { createBrowserRouter } from 'react-router-dom';
import { App } from '@/App';
import { AdminDashboardPage } from '@/features/admin/AdminDashboardPage';
import { AuditLogsPage } from '@/features/admin/AuditLogsPage';
import { LoginPage } from '@/features/auth/LoginPage';
import { RequireRole } from '@/features/auth/RequireRole';
import { CarrierPortalPage } from '@/features/carrier-portal/CarrierPortalPage';
import { LandingPage } from '@/features/landing/LandingPage';
import { LoadBoardPage } from '@/features/load-board/LoadBoardPage';
import { VerticalLandingPage } from '@/features/landing/VerticalLandingPage';
import { RateCalculatorPage } from '@/features/rates/RateCalculatorPage';
import { ShipperBillingPage } from '@/features/shipper-portal/ShipperBillingPage';
import { ShipperDashboardPage } from '@/features/shipper-portal/ShipperDashboardPage';
import { LiveTrackingPage } from '@/features/telemetry/LiveTrackingPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      { index: true, element: <LandingPage /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'rates', element: <RateCalculatorPage /> },
      {
        path: 'load-board',
        element: (
          <RequireRole>
            <LoadBoardPage />
          </RequireRole>
        ),
      },
      {
        path: 'tracking',
        element: (
          <RequireRole>
            <LiveTrackingPage />
          </RequireRole>
        ),
      },
      { path: 'verticals/:slug', element: <VerticalLandingPage /> },
      {
        path: 'admin/carriers',
        element: (
          <RequireRole role="Admin">
            <AdminDashboardPage />
          </RequireRole>
        ),
      },
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
      {
        path: 'shipper/dashboard',
        element: (
          <RequireRole role="Shipper">
            <ShipperDashboardPage />
          </RequireRole>
        ),
      },
      {
        path: 'shipper/billing',
        element: (
          <RequireRole role="Shipper">
            <ShipperBillingPage />
          </RequireRole>
        ),
      },
    ],
  },
]);
