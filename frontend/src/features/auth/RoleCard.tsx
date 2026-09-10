import { cn } from '@/shared/lib/cn';
import { Card } from '@/shared/ui';
import { Link } from 'react-router-dom';

interface RoleCardProps {
    to: string;
    icon: string;
    title: string;
    description: string;
    className?: string;
}

/** One selectable business role on the Tri-Sign-Up landing (T-SDD Epica 1). */
export function RoleCard({ to, icon, title, description, className }: RoleCardProps): JSX.Element {
    return (
        <Link to={to} className="block rounded-lg focus:outline-none focus-visible:ring-2 focus-visible:ring-fleet-blue/40">
            <Card interactive className={cn('flex h-full flex-col gap-3 p-6', className)}>
                <span className="material-symbols-outlined text-3xl text-fleet-blue" aria-hidden>
                    {icon}
                </span>
                <h2 className="text-headline-sm text-on-surface">{title}</h2>
                <p className="text-body-sm text-steel-gray">{description}</p>
                <span className="mt-auto text-xs font-semibold uppercase tracking-wider text-fleet-blue">
                    Get started →
                </span>
            </Card>
        </Link>
    );
}
