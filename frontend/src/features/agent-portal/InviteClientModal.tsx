import { Button, Card, Input } from '@/shared/ui';
import type { FormEvent } from 'react';
import { useState } from 'react';
import { toast } from 'sonner';
import { useInviteClient } from './useInviteClient';

interface InviteClientModalProps {
    open: boolean;
    onClose: () => void;
    onInvited: () => void;
}

/** Invite-a-client dialog on the Agent portal (T-SDD Epica 4). */
export function InviteClientModal({ open, onClose, onInvited }: InviteClientModalProps): JSX.Element | null {
    const { inviteClient, submitting, errorMessage } = useInviteClient();
    const [email, setEmail] = useState('');

    if (!open) {
        return null;
    }

    async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void> {
        event.preventDefault();
        const ok = await inviteClient(email);
        if (ok) {
            toast.success(`Invitation sent to ${email}`);
            setEmail('');
            onInvited();
            onClose();
        }
    }

    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-primary/40 px-4"
            role="dialog"
            aria-modal="true"
            aria-label="Invite a client"
        >
            <Card raised className="w-full max-w-sm p-6">
                <form className="flex flex-col gap-4" onSubmit={(event) => void handleSubmit(event)}>
                    <header className="flex flex-col gap-1">
                        <h2 className="text-headline-sm">Invite a client</h2>
                        <p className="text-body-sm text-steel-gray">
                            We'll email them a link to join CPG under your agency.
                        </p>
                    </header>
                    <Input
                        label="Client email"
                        type="email"
                        autoComplete="email"
                        value={email}
                        onChange={(event) => setEmail(event.target.value)}
                        required
                        {...(errorMessage ? { error: errorMessage } : {})}
                    />
                    <div className="flex justify-end gap-2">
                        <Button type="button" variant="outline" onClick={onClose} disabled={submitting}>
                            Cancel
                        </Button>
                        <Button type="submit" disabled={submitting}>
                            {submitting ? 'Sending…' : 'Send invitation'}
                        </Button>
                    </div>
                </form>
            </Card>
        </div>
    );
}
