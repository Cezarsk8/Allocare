'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  createEmployeeSchema,
  type CreateEmployeeFormData,
} from '@/app/constants/employeeSchemas';
import { useCreateEmployee, useUpdateEmployee } from '@/app/hooks/useEmployees';
import { useCostCenters } from '@/app/hooks/useCostCenters';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';

interface EmployeeFormProps {
  companyId: string;
  employeeId?: string;
  defaultValues?: CreateEmployeeFormData;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function EmployeeForm({
  companyId,
  employeeId,
  defaultValues,
  onSuccess,
  onCancel,
}: EmployeeFormProps) {
  const isEdit = !!employeeId;
  const createEmployee = useCreateEmployee();
  const updateEmployee = useUpdateEmployee();

  const { data: costCentersData } = useCostCenters(companyId, {
    pageSize: 100,
    isActive: true,
  });

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateEmployeeFormData>({
    resolver: zodResolver(createEmployeeSchema),
    defaultValues: defaultValues ?? {
      name: '',
      email: '',
      costCenterId: '',
      jobTitle: '',
      hireDate: '',
    },
  });

  const onSubmit = async (data: CreateEmployeeFormData) => {
    try {
      const payload = {
        name: data.name,
        email: data.email,
        costCenterId: data.costCenterId || null,
        jobTitle: data.jobTitle || null,
        hireDate: data.hireDate || null,
      };

      if (isEdit) {
        await updateEmployee.mutateAsync({
          companyId,
          id: employeeId,
          data: payload,
        });
        toast.success('Colaborador atualizado com sucesso.');
      } else {
        await createEmployee.mutateAsync({ companyId, data: payload });
        toast.success('Colaborador criado com sucesso.');
      }
      onSuccess?.();
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'response' in err
          ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
          : undefined;
      toast.error(message ?? 'Erro ao salvar colaborador.');
    }
  };

  const isPending =
    isSubmitting || createEmployee.isPending || updateEmployee.isPending;

  const costCenters = costCentersData?.items ?? [];

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-gray-700">
            Nome *
          </label>
          <input
            id="name"
            type="text"
            {...register('name')}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
            placeholder="Nome completo"
          />
          {errors.name && (
            <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
          )}
        </div>
        <div>
          <label htmlFor="email" className="block text-sm font-medium text-gray-700">
            E-mail *
          </label>
          <input
            id="email"
            type="email"
            {...register('email')}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
            placeholder="colaborador@empresa.com"
          />
          {errors.email && (
            <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
          )}
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        <div>
          <label htmlFor="jobTitle" className="block text-sm font-medium text-gray-700">
            Cargo
          </label>
          <input
            id="jobTitle"
            type="text"
            {...register('jobTitle')}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
            placeholder="Ex: Analista de TI"
          />
          {errors.jobTitle && (
            <p className="mt-1 text-sm text-red-600">{errors.jobTitle.message}</p>
          )}
        </div>
        <div>
          <label htmlFor="costCenterId" className="block text-sm font-medium text-gray-700">
            Centro de Custo
          </label>
          <select
            id="costCenterId"
            {...register('costCenterId')}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          >
            <option value="">Nenhum</option>
            {costCenters.map((cc) => (
              <option key={cc.id} value={cc.id}>
                {cc.code} — {cc.name}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label htmlFor="hireDate" className="block text-sm font-medium text-gray-700">
            Data de Admissão
          </label>
          <input
            id="hireDate"
            type="date"
            {...register('hireDate')}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          />
          {errors.hireDate && (
            <p className="mt-1 text-sm text-red-600">{errors.hireDate.message}</p>
          )}
        </div>
      </div>

      <div className="flex flex-col gap-3 pt-2 sm:flex-row sm:justify-end">
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Cancelar
          </button>
        )}
        <button
          type="submit"
          disabled={isPending}
          className="inline-flex items-center justify-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
        >
          {isPending && <Loader2 className="h-4 w-4 animate-spin" />}
          {isEdit ? 'Salvar' : 'Criar Colaborador'}
        </button>
      </div>
    </form>
  );
}
