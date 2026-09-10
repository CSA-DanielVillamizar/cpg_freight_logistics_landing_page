import { formatEnum } from '@/shared/lib/formatEnum';
import type { BadgeTone } from '@/shared/ui';
import { Badge } from '@/shared/ui';
import type { DisbursementStatus } from './payoutsApi';

const STATUS_TONE: Record<DisbursementStatus, BadgeTone> = {
    Pending: 'dispatched',
    Processing: 'transit',
    Completed: 'delivered',
    Failed: 'rejected',
};

interface PayoutStatusBadgeProps {
    status: DisbursementStatus;
}

/** Visual status chip for a payout's lifecycle (T-SDD Epica 2B). */
export function PayoutStatusBadge({ status }: PayoutStatusBadgeProps): JSX.Element {
    return <Badge tone={STATUS_TONE[status]}>{formatEnum(status)}</Badge>;
}
