import { Button } from '@/shared/ui';
import { useState } from 'react';
import { toast } from 'sonner';
import { useRequestQuickPay } from './useRequestQuickPay';

interface QuickPayToggleProps {
    invoiceId: string;
    quickPayFeeRatePercent?: number;
    onRequested?: () => void;
}

/**
 * Sits on an invoice's payout row — explains the Quick Pay trade-off and fires
 * `POST /api/invoices/{id}/quick-pay` (T-SDD Epica 2B). Once requested it can't be undone
 * from here, so the button becomes a static "Requested" state after a successful call.
 */
export function QuickPayToggle({
    invoiceId,
    quickPayFeeRatePercent = 3,
    onRequested,
}: QuickPayToggleProps): JSX.Element {
    const { requestQuickPay, submittingInvoiceId } = useRequestQuickPay();
    const [requested, setRequested] = useState(false);
    const submitting = submittingInvoiceId === invoiceId;

    async function handleClick(): Promise<void> {
        const ok = await requestQuickPay(invoiceId);
        if (ok) {
            setRequested(true);
            toast.success('Quick Pay requested — funds are on the way.');
            onRequested?.();
        } else {
            toast.error('Could not request Quick Pay — please retry.');
        }
    }

    if (requested) {
        return (
            <span className="text-xs font-semibold uppercase tracking-wider text-success">
                Quick Pay requested
            </span>
        );
    }

    return (
        <div className="flex flex-col items-end gap-1">
            <Button variant="outline" disabled={submitting} onClick={() => void handleClick()}>
                {submitting ? 'Requesting…' : 'Get paid in 24h'}
            </Button>
            <span className="text-right text-xs text-steel-gray">
                {quickPayFeeRatePercent}% fee for immediate settlement
            </span>
        </div>
    );
}
