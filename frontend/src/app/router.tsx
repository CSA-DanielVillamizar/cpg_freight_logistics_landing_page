import { App } from '@/App';
import { AdminDashboardPage } from '@/features/admin/AdminDashboardPage';
import { AuditLogsPage } from '@/features/admin/AuditLogsPage';
import { AgentPortalPage } from '@/features/agent-portal/AgentPortalPage';
import { AgentSignUpPage } from '@/features/auth/AgentSignUpPage';
import { CarrierSignUpPage } from '@/features/auth/CarrierSignUpPage';
import { LoginPage } from '@/features/auth/LoginPage';
import { RequireRole } from '@/features/auth/RequireRole';
import { RoleSelectionPage } from '@/features/auth/RoleSelectionPage';
import { ShipperSignUpPage } from '@/features/auth/ShipperSignUpPage';
import { CarrierPortalPage } from '@/features/carrier-portal/CarrierPortalPage';
import { LandingPage } from '@/features/landing/LandingPage';
import { VerticalLandingPage } from '@/features/landing/VerticalLandingPage';
import { LoadBoardPage } from '@/features/load-board/LoadBoardPage';
import { RateCalculatorPage } from '@/features/rates/RateCalculatorPage';
import { LoadTrackingDetailPage } from '@/features/shipper-portal/LoadTrackingDetailPage';
import { PostLoadPage } from '@/features/shipper-portal/PostLoadPage';
import { ShipperBillingPage } from '@/features/shipper-portal/ShipperBillingPage';
import { ShipperDashboardPage } from '@/features/shipper-portal/ShipperDashboardPage';
import { LiveTrackingPage } from '@/features/telemetry/LiveTrackingPage';
import { createBrowserRouter } from 'react-router-dom';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      { index: true, element: <LandingPage /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'signup', element: <RoleSelectionPage /> },
      { path: 'signup/shipper', element: <ShipperSignUpPage /> },
      { path: 'signup/carrier', element: <CarrierSignUpPage /> },
      { path: 'signup/agent', element: <AgentSignUpPage /> },
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
        path: 'agent',
        element: (
          <RequireRole role="Agent">
            <AgentPortalPage />
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
        path: 'shipper/loads/new',
        element: (
          <RequireRole role="Shipper">
            <PostLoadPage />
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
      {
        path: 'shipper/loads/:loadId/tracking',
        element: (
          <RequireRole role="Shipper">
            <LoadTrackingDetailPage />
          </RequireRole>
        ),
      },
    ],
  },
]);
