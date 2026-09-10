import { useState } from 'react';
import { agentApi } from './agentApi';

/** Submits a new client invitation, tracking in-flight/error state for the modal. */
export function useInviteClient(): {
    inviteClient: (email: string) => Promise<boolean>;
    submitting: boolean;
    errorMessage: string | null;
} {
    const [submitting, setSubmitting] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    async function inviteClient(email: string): Promise<boolean> {
        setSubmitting(true);
        setErrorMessage(null);
        try {
            await agentApi.inviteClient(email);
            return true;
        } catch {
            setErrorMessage('Could not send the invitation — please retry.');
            return false;
        } finally {
            setSubmitting(false);
        }
    }

    return { inviteClient, submitting, errorMessage };
}
