'use client';

import { useParams } from 'next/navigation';
import {
  useEmployees,
  useEmployee,
  useDeactivateEmployee,
  useActivateEmployee,
  useReactivateEmployee,
} from '@/app/hooks/useEmployees';
import { useCostCenters } from '@/app/hooks/useCostCenters';
import { EmployeeForm } from '@/app/components/employees/EmployeeForm';
import { EmployeeTable } from '@/app/components/employees/EmployeeTable';
import { TerminateEmployeeDialog } from '@/app/components/employees/TerminateEmployeeDialog';
import { Pagination } from '@/app/components/ui/molecules/Pagination';
import { Loader2, Plus, Search, Users } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';

export default function EmployeesPage() {
  const params = useParams();
  const companyId = params.id as string;

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [costCenterFilter, setCostCenterFilter] = useState<string>('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [terminatingEmployee, setTerminatingEmployee] = useState<{
    id: string;
    name: string;
  } | null>(null);

  const isActive = statusFilter === '' ? undefined : statusFilter === 'true';
  const { data, isLoading, error, refetch } = useEmployees(companyId, {
    page,
    pageSize: 10,
    costCenterId: costCenterFilter || null,
    isActive: isActive ?? null,
    search: search || undefined,
  });

  const { data: editingEmployee } = useEmployee(
    companyId,
    editingId ?? '',
  );

  const { data: costCentersData } = useCostCenters(companyId, {
    pageSize: 100,
    isActive: true,
  });

  const deactivate = useDeactivateEmployee();
  const activate = useActivateEmployee();
  const reactivate = useReactivateEmployee();

  const handleToggleStatus = async (id: string, currentlyActive: boolean) => {
    try {
      if (currentlyActive) {
        await deactivate.mutateAsync({ companyId, id });
        toast.success('Colaborador desativado.');
      } else {
        await activate.mutateAsync({ companyId, id });
        toast.success('Colaborador ativado.');
      }
    } catch {
      toast.error('Erro ao alterar status.');
    }
  };

  const handleReactivate = async (id: string) => {
    try {
      await reactivate.mutateAsync({ companyId, id });
      toast.success('Colaborador reativado.');
    } catch {
      toast.error('Erro ao reativar colaborador.');
    }
  };

  const handleEdit = (id: string) => {
    setEditingId(id);
    setShowCreateForm(false);
    setTerminatingEmployee(null);
  };

  const handleSearchChange = (value: string) => {
    setSearch(value);
    setPage(1);
  };

  const handleFilterChange = (value: string) => {
    setStatusFilter(value);
    setPage(1);
  };

  const handleCostCenterFilterChange = (value: string) => {
    setCostCenterFilter(value);
    setPage(1);
  };

  const costCenters = costCentersData?.items ?? [];

  return (
    <>
      {/* Header + Actions */}
      <div className="mb-4 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h2 className="text-lg font-semibold text-gray-900">Colaboradores</h2>
        <button
          onClick={() => {
            setShowCreateForm(!showCreateForm);
            setEditingId(null);
            setTerminatingEmployee(null);
          }}
          className="inline-flex items-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          <Plus className="h-4 w-4" />
          Novo Colaborador
        </button>
      </div>

      {/* Create Form */}
      {showCreateForm && (
        <div className="mb-6 rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-base font-semibold text-gray-900">
            Criar Colaborador
          </h3>
          <EmployeeForm
            companyId={companyId}
            onSuccess={() => setShowCreateForm(false)}
            onCancel={() => setShowCreateForm(false)}
          />
        </div>
      )}

      {/* Edit Form */}
      {editingId && editingEmployee && (
        <div className="mb-6 rounded-lg border border-blue-200 bg-blue-50 p-6 shadow-sm">
          <h3 className="mb-4 text-base font-semibold text-gray-900">
            Editar Colaborador
          </h3>
          <EmployeeForm
            companyId={companyId}
            employeeId={editingId}
            defaultValues={{
              name: editingEmployee.name,
              email: editingEmployee.email,
              costCenterId: editingEmployee.costCenterId ?? '',
              jobTitle: editingEmployee.jobTitle ?? '',
              hireDate: editingEmployee.hireDate
                ? editingEmployee.hireDate.split('T')[0]
                : '',
            }}
            onSuccess={() => setEditingId(null)}
            onCancel={() => setEditingId(null)}
          />
        </div>
      )}

      {/* Terminate Dialog */}
      {terminatingEmployee && (
        <div className="mb-6">
          <TerminateEmployeeDialog
            companyId={companyId}
            employeeId={terminatingEmployee.id}
            employeeName={terminatingEmployee.name}
            onSuccess={() => setTerminatingEmployee(null)}
            onCancel={() => setTerminatingEmployee(null)}
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
            placeholder="Buscar por nome ou e-mail..."
            className="block w-full rounded-md border border-gray-300 py-2 pl-10 pr-3 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          />
        </div>
        <select
          value={costCenterFilter}
          onChange={(e) => handleCostCenterFilterChange(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
        >
          <option value="">Todos os Centros</option>
          {costCenters.map((cc) => (
            <option key={cc.id} value={cc.id}>
              {cc.code} — {cc.name}
            </option>
          ))}
        </select>
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
            <p className="text-red-600">Erro ao carregar colaboradores.</p>
            <button
              onClick={() => refetch()}
              className="mt-3 rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700"
            >
              Tentar novamente
            </button>
          </div>
        ) : !data || data.items.length === 0 ? (
          <div className="py-12 text-center">
            <Users className="mx-auto h-12 w-12 text-gray-400" />
            <p className="mt-4 text-gray-500">Nenhum colaborador encontrado.</p>
          </div>
        ) : (
          <>
            <EmployeeTable
              employees={data.items}
              onEdit={handleEdit}
              onToggleStatus={handleToggleStatus}
              onTerminate={(id, name) => {
                setTerminatingEmployee({ id, name });
                setEditingId(null);
              }}
              onReactivate={handleReactivate}
              isPending={
                deactivate.isPending || activate.isPending || reactivate.isPending
              }
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
