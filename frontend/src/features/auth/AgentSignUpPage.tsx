import { SignUpFormShell } from './SignUpFormShell';

/** /signup/agent (T-SDD Epica 1). */
export function AgentSignUpPage(): JSX.Element {
    return (
        <SignUpFormShell
            role="Agent"
            eyebrow="Independent Agent Sign-Up"
            title="Run your own brokerage under CPG"
            subtitle="Invite clients, publish exclusive loads and earn commission — CPG's DOT/MC has you covered."
            companyFieldLabel="Agency name"
            companyRequired
        />
    );
}
