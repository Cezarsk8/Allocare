'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Building2, Landmark, Users } from 'lucide-react';

interface CompanyNavProps {
  companyId: string;
}

export function CompanyNav({ companyId }: CompanyNavProps) {
  const pathname = usePathname();

  const tabs = [
    {
      label: 'Geral',
      href: `/companies/${companyId}`,
      icon: Building2,
      isActive: pathname === `/companies/${companyId}`,
    },
    {
      label: 'Centros de Custo',
      href: `/companies/${companyId}/cost-centers`,
      icon: Landmark,
      isActive: pathname.startsWith(`/companies/${companyId}/cost-centers`),
    },
    {
      label: 'Colaboradores',
      href: `/companies/${companyId}/employees`,
      icon: Users,
      isActive: pathname.startsWith(`/companies/${companyId}/employees`),
    },
  ];

  return (
    <nav className="mb-6 overflow-x-auto border-b border-gray-200">
      <div className="flex gap-0">
        {tabs.map((tab) => (
          <Link
            key={tab.href}
            href={tab.href}
            className={`inline-flex items-center gap-2 whitespace-nowrap border-b-2 px-4 py-3 text-sm font-medium transition-colors ${
              tab.isActive
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
            }`}
          >
            <tab.icon className="h-4 w-4" />
            {tab.label}
          </Link>
        ))}
      </div>
    </nav>
  );
}
