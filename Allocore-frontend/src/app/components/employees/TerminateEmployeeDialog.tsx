'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  terminateEmployeeSchema,
  type TerminateEmployeeFormData,
} from '@/app/constants/employeeSchemas';
import { useTerminateEmployee } from '@/app/hooks/useEmployees';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';

interface TerminateEmployeeDialogProps {
  companyId: string;
  employeeId: string;
  employeeName: string;
  onSuccess: () => void;
  onCancel: () => void;
}

export function TerminateEmployeeDialog({
  companyId,
  employeeId,
  employeeName,
  onSuccess,
  onCancel,
}: TerminateEmployeeDialogProps) {
  const terminateEmployee = useTerminateEmployee();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<TerminateEmployeeFormData>({
    resolver: zodResolver(terminateEmployeeSchema),
    defaultValues: { terminationDate: '' },
  });

  const onSubmit = async (data: TerminateEmployeeFormData) => {
    try {
      await terminateEmployee.mutateAsync({
        companyId,
        id: employeeId,
        data: { terminationDate: data.terminationDate },
      });
      toast.success(`${employeeName} foi desligado(a).`);
      onSuccess();
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'response' in err
          ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
          : undefined;
      toast.error(message ?? 'Erro ao desligar colaborador.');
    }
  };

  const isPending = isSubmitting || terminateEmployee.isPending;

  return (
    <div className="rounded-lg border border-amber-200 bg-amber-50 p-4">
      <p className="mb-3 text-sm text-amber-800">
        Confirme o desligamento de <strong>{employeeName}</strong>. Informe a data
        de desligamento:
      </p>
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <div className="flex-1">
          <label
            htmlFor="terminationDate"
            className="block text-sm font-medium text-gray-700"
          >
            Data de Desligamento *
          </label>
          <input
            id="terminationDate"
            type="date"
            {...register('terminationDate')}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          />
          {errors.terminationDate && (
            <p className="mt-1 text-sm text-red-600">
              {errors.terminationDate.message}
            </p>
          )}
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={onCancel}
            className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isPending}
            className="inline-flex items-center gap-2 rounded-md bg-amber-600 px-4 py-2 text-sm font-medium text-white hover:bg-amber-700 disabled:opacity-50"
          >
            {isPending && <Loader2 className="h-4 w-4 animate-spin" />}
            Confirmar
          </button>
        </div>
      </form>
    </div>
  );
}
