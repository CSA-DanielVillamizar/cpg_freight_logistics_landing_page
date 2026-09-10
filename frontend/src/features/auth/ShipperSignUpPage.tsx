import { SignUpFormShell } from './SignUpFormShell';

/** /signup/shipper (T-SDD Epica 1). */
export function ShipperSignUpPage(): JSX.Element {
  return (
    <SignUpFormShell
      role="Shipper"
      eyebrow="Shipper Sign-Up"
      title="Post your first load in minutes"
      subtitle="Get instant access to the rate calculator, live tracking and the Load Board."
      companyFieldLabel="Company name (optional)"
      companyRequired={false}
    />
  );
}
