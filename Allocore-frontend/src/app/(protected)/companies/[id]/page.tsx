'use client';

import { useParams } from 'next/navigation';
import { useCompany, useCompanyUsers, useRemoveUserFromCompany } from '@/app/hooks/useCompanies';
import { CompanyForm } from '@/app/components/companies/CompanyForm';
import { AddUserToCompanyForm } from '@/app/components/companies/AddUserToCompanyForm';
import { ArrowLeft, Loader2, Pencil, Trash2, UserPlus } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';
import Link from 'next/link';

export default function CompanyDetailPage() {
  const params = useParams();
  const companyId = params.id as string;

  const { data: company, isLoading: loadingCompany } = useCompany(companyId);
  const { data: users, isLoading: loadingUsers } = useCompanyUsers(companyId);
  const removeUser = useRemoveUserFromCompany();

  const [showEditForm, setShowEditForm] = useState(false);
  const [showAddUserForm, setShowAddUserForm] = useState(false);

  const canManage = company?.userRole === 'Owner' || company?.userRole === 'Admin';

  const handleRemoveUser = async (userId: string, userName: string) => {
    if (!confirm(`Remover ${userName} desta empresa?`)) return;

    try {
      await removeUser.mutateAsync({ companyId, userId });
      toast.success('Usuário removido da empresa.');
    } catch {
      toast.error('Erro ao remover usuário.');
    }
  };

  if (loadingCompany) {
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
          <Link href="/companies" className="mt-3 inline-block text-sm text-blue-600 hover:underline">
            Voltar para empresas
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <Link href="/companies" className="mb-4 inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700">
        <ArrowLeft className="h-4 w-4" />
        Voltar
      </Link>

      {/* Company Info */}
      <div className="mb-6 rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">{company.name}</h1>
            {company.legalName && <p className="mt-1 text-gray-500">{company.legalName}</p>}
            {company.taxId && <p className="mt-1 text-sm text-gray-400">CNPJ/Tax ID: {company.taxId}</p>}
            <div className="mt-3 flex items-center gap-3">
              <span
                className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${
                  company.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                }`}
              >
                {company.isActive ? 'Ativa' : 'Inativa'}
              </span>
              {company.userRole && (
                <span className="text-xs text-gray-400">
                  Seu papel: <span className="font-medium text-gray-600">{company.userRole}</span>
                </span>
              )}
            </div>
          </div>
          {canManage && (
            <button
              onClick={() => setShowEditForm(!showEditForm)}
              className="inline-flex items-center gap-2 rounded-md border border-gray-300 px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              <Pencil className="h-4 w-4" />
              Editar
            </button>
          )}
        </div>

        {showEditForm && (
          <div className="mt-6 border-t border-gray-200 pt-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900">Editar Empresa</h2>
            <CompanyForm
              companyId={companyId}
              defaultValues={{
                name: company.name,
                legalName: company.legalName ?? '',
                taxId: company.taxId ?? '',
              }}
              onSuccess={() => setShowEditForm(false)}
              onCancel={() => setShowEditForm(false)}
            />
          </div>
        )}
      </div>

      {/* Company Users */}
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <div className="mb-4 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <h2 className="text-lg font-semibold text-gray-900">Usuários</h2>
          {canManage && (
            <button
              onClick={() => setShowAddUserForm(!showAddUserForm)}
              className="inline-flex items-center gap-2 rounded-md bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-700"
            >
              <UserPlus className="h-4 w-4" />
              Adicionar Usuário
            </button>
          )}
        </div>

        {showAddUserForm && (
          <div className="mb-6 rounded-lg border border-gray-100 bg-gray-50 p-4">
            <AddUserToCompanyForm
              companyId={companyId}
              onSuccess={() => setShowAddUserForm(false)}
              onCancel={() => setShowAddUserForm(false)}
            />
          </div>
        )}

        {loadingUsers ? (
          <div className="flex justify-center py-8">
            <Loader2 className="h-6 w-6 animate-spin text-blue-600" />
          </div>
        ) : !users || users.length === 0 ? (
          <p className="py-8 text-center text-gray-500">Nenhum usuário vinculado.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-200 text-left text-gray-500">
                  <th className="pb-3 font-medium">Nome</th>
                  <th className="pb-3 font-medium">Email</th>
                  <th className="pb-3 font-medium">Papel</th>
                  <th className="pb-3 font-medium">Desde</th>
                  {canManage && <th className="pb-3 font-medium">Ações</th>}
                </tr>
              </thead>
              <tbody>
                {users.map((user) => (
                  <tr key={user.userId} className="border-b border-gray-100">
                    <td className="py-3 font-medium text-gray-900">{user.userFullName}</td>
                    <td className="py-3 text-gray-600">{user.userEmail}</td>
                    <td className="py-3">
                      <span className="rounded-full bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-700">
                        {user.roleInCompany}
                      </span>
                    </td>
                    <td className="py-3 text-gray-500">
                      {new Date(user.joinedAt).toLocaleDateString('pt-BR')}
                    </td>
                    {canManage && (
                      <td className="py-3">
                        <button
                          onClick={() => handleRemoveUser(user.userId, user.userFullName)}
                          disabled={removeUser.isPending}
                          className="rounded p-1 text-red-500 hover:bg-red-50 disabled:opacity-50"
                          title="Remover usuário"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
