'use client';

import { useParams } from 'next/navigation';
import { useCostCenters, useCostCenter, useDeactivateCostCenter, useActivateCostCenter } from '@/app/hooks/useCostCenters';
import { CostCenterForm } from '@/app/components/cost-centers/CostCenterForm';
import { CostCenterTable } from '@/app/components/cost-centers/CostCenterTable';
import { Pagination } from '@/app/components/ui/molecules/Pagination';
import { Loader2, Plus, Search, Landmark } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

export default function CostCentersPage() {
  const params = useParams();
  const companyId = params.id as string;

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const isActive = statusFilter === '' ? undefined : statusFilter === 'true';
  const { data, isLoading, error, refetch } = useCostCenters(companyId, {
    page,
    pageSize: 10,
    isActive: isActive ?? null,
    search: search || undefined,
  });

  const { data: editingCostCenter } = useCostCenter(
    companyId,
    editingId ?? '',
  );

  const deactivate = useDeactivateCostCenter();
  const activate = useActivateCostCenter();

  const handleToggleStatus = async (id: string, currentlyActive: boolean) => {
    try {
      if (currentlyActive) {
        await deactivate.mutateAsync({ companyId, id });
        toast.success('Centro de custo desativado.');
      } else {
        await activate.mutateAsync({ companyId, id });
        toast.success('Centro de custo ativado.');
      }
    } catch {
      toast.error('Erro ao alterar status.');
    }
  };

  const handleEdit = (id: string) => {
    setEditingId(id);
    setShowCreateForm(false);
  };

  const handleSearchChange = (value: string) => {
    setSearch(value);
    setPage(1);
  };

  const handleFilterChange = (value: string) => {
    setStatusFilter(value);
    setPage(1);
  };

  return (
    <>
      {/* Header + Actions */}
      <div className="mb-4 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h2 className="text-lg font-semibold text-gray-900">Centros de Custo</h2>
        <button
          onClick={() => {
            setShowCreateForm(!showCreateForm);
            setEditingId(null);
          }}
          className="inline-flex items-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <Plus className="h-4 w-4" />
          Novo Centro de Custo
        </button>
      </div>

      {/* Create Form */}
      {showCreateForm && (
        <div className="mb-6 rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-base font-semibold text-gray-900">
            Criar Centro de Custo
          </h3>
          <CostCenterForm
            companyId={companyId}
            onSuccess={() => setShowCreateForm(false)}
            onCancel={() => setShowCreateForm(false)}
          />
        </div>
      )}

      {/* Edit Form */}
      {editingId && editingCostCenter && (
        <div className="mb-6 rounded-lg border border-blue-200 bg-blue-50 p-6 shadow-sm">
          <h3 className="mb-4 text-base font-semibold text-gray-900">
            Editar Centro de Custo
          </h3>
          <CostCenterForm
            companyId={companyId}
            costCenterId={editingId}
            defaultValues={{
              code: editingCostCenter.code,
              name: editingCostCenter.name,
              description: editingCostCenter.description ?? '',
            }}
            onSuccess={() => setEditingId(null)}
            onCancel={() => setEditingId(null)}
          />
        </div>
      )}

      {/* Filters */}
      <div className="mb-4 flex flex-col gap-3 sm:flex-row">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => handleSearchChange(e.target.value)}
            placeholder="Buscar por código ou nome..."
            className="block w-full rounded-md border border-gray-300 py-2 pl-10 pr-3 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) => handleFilterChange(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
        >
          <option value="">Todos</option>
          <option value="true">Ativos</option>
          <option value="false">Inativos</option>
        </select>
      </div>

      {/* Table */}
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        {isLoading ? (
          <div className="flex justify-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          </div>
        ) : error ? (
          <div className="py-12 text-center">
            <p className="text-red-600">Erro ao carregar centros de custo.</p>
            <button
              onClick={() => refetch()}
              className="mt-3 rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700"
            >
              Tentar novamente
            </button>
          </div>
        ) : !data || data.items.length === 0 ? (
          <div className="py-12 text-center">
            <Landmark className="mx-auto h-12 w-12 text-gray-400" />
            <p className="mt-4 text-gray-500">Nenhum centro de custo encontrado.</p>
          </div>
        ) : (
          <>
            <CostCenterTable
              costCenters={data.items}
              onEdit={handleEdit}
              onToggleStatus={handleToggleStatus}
              isPending={deactivate.isPending || activate.isPending}
            />
            <Pagination
              page={data.page}
              totalPages={data.totalPages}
              onPageChange={setPage}
            />
          </>
        )}
      </div>
    </>
  );
}
