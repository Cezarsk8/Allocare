'use client';

import { useParams } from 'next/navigation';
import { useCompany } from '@/app/hooks/useCompanies';
import { CompanyNav } from '@/app/components/companies/CompanyNav';
import { ArrowLeft, Loader2 } from 'lucide-react';
import Link from 'next/link';

export default function CompanyLayout({ children }: { children: React.ReactNode }) {
  const params = useParams();
  const companyId = params.id as string;
  const { data: company, isLoading } = useCompany(companyId);

  if (isLoading) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (!company) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8">
        <div className="rounded-lg border border-gray-200 bg-white p-6 text-center">
          <p className="text-gray-500">Empresa não encontrada.</p>
          <Link
            href="/companies"
            className="mt-3 inline-block text-sm text-blue-600 hover:underline"
          >
            Voltar para empresas
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <Link
        href="/companies"
        className="mb-4 inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700"
      >
        <ArrowLeft className="h-4 w-4" />
        Voltar
      </Link>

      <div className="mb-6">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-gray-900">{company.name}</h1>
          <span
            className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${
              company.isActive
                ? 'bg-green-100 text-green-700'
                : 'bg-gray-100 text-gray-600'
            }`}
          >
            {company.isActive ? 'Ativa' : 'Inativa'}
          </span>
        </div>
        {company.legalName && (
          <p className="mt-1 text-gray-500">{company.legalName}</p>
        )}
      </div>

      <CompanyNav companyId={companyId} />

      {children}
    </div>
  );
}
