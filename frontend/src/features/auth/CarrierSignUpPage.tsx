import { SignUpFormShell } from './SignUpFormShell';

/** /signup/carrier (T-SDD Epica 1). */
export function CarrierSignUpPage(): JSX.Element {
  return (
    <SignUpFormShell
      role="Carrier"
      eyebrow="Carrier Sign-Up"
      title="Start booking loads today"
      subtitle="File your compliance packet and accept your first load from the board."
      companyFieldLabel="Company name"
      companyRequired
      showCarrierLicenseFields
    />
  );
}
