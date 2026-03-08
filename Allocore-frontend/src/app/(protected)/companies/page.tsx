'use client';

import { useMyCompanies } from '@/app/hooks/useCompanies';
import { CompanyForm } from '@/app/components/companies/CompanyForm';
import { Building2, Plus, Loader2 } from 'lucide-react';
import { useState } from 'react';
import Link from 'next/link';

export default function CompaniesPage() {
  const { data: companies, isLoading, error, refetch } = useMyCompanies();
  const [showCreateForm, setShowCreateForm] = useState(false);

  if (isLoading) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8">
        <div className="rounded-lg border border-red-200 bg-red-50 p-6 text-center">
          <p className="text-red-700">Erro ao carregar empresas.</p>
          <button
            onClick={() => refetch()}
            className="mt-3 rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700"
          >
            Tentar novamente
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Minhas Empresas</h1>
        <button
          onClick={() => setShowCreateForm(true)}
          className="inline-flex items-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <Plus className="h-4 w-4" />
          Nova Empresa
        </button>
      </div>

      {showCreateForm && (
        <div className="mb-6 rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-lg font-semibold text-gray-900">Criar Empresa</h2>
          <CompanyForm
            onSuccess={() => setShowCreateForm(false)}
            onCancel={() => setShowCreateForm(false)}
          />
        </div>
      )}

      {!companies || companies.length === 0 ? (
        <div className="rounded-lg border border-gray-200 bg-white p-12 text-center">
          <Building2 className="mx-auto h-12 w-12 text-gray-400" />
          <p className="mt-4 text-gray-500">Você não está vinculado a nenhuma empresa.</p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {companies.map((company) => (
            <Link
              key={company.id}
              href={`/companies/${company.id}`}
              className="block rounded-lg border border-gray-200 bg-white p-5 shadow-sm transition-shadow hover:shadow-md"
            >
              <div className="flex items-start justify-between">
                <div>
                  <h3 className="font-semibold text-gray-900">{company.name}</h3>
                  {company.legalName && (
                    <p className="mt-1 text-sm text-gray-500">{company.legalName}</p>
                  )}
                </div>
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
              {company.userRole && (
                <p className="mt-3 text-xs text-gray-400">
                  Seu papel: <span className="font-medium text-gray-600">{company.userRole}</span>
                </p>
              )}
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
