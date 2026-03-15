'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  createCostCenterSchema,
  type CreateCostCenterFormData,
} from '@/app/constants/costCenterSchemas';
import { useCreateCostCenter, useUpdateCostCenter } from '@/app/hooks/useCostCenters';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';

interface CostCenterFormProps {
  companyId: string;
  costCenterId?: string;
  defaultValues?: CreateCostCenterFormData;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function CostCenterForm({
  companyId,
  costCenterId,
  defaultValues,
  onSuccess,
  onCancel,
}: CostCenterFormProps) {
  const isEdit = !!costCenterId;
  const createCostCenter = useCreateCostCenter();
  const updateCostCenter = useUpdateCostCenter();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateCostCenterFormData>({
    resolver: zodResolver(createCostCenterSchema),
    defaultValues: defaultValues ?? { code: '', name: '', description: '' },
  });

  const onSubmit = async (data: CreateCostCenterFormData) => {
    try {
      const payload = {
        ...data,
        description: data.description || null,
      };

      if (isEdit) {
        await updateCostCenter.mutateAsync({
          companyId,
          id: costCenterId,
          data: payload,
        });
        toast.success('Centro de custo atualizado com sucesso.');
      } else {
        await createCostCenter.mutateAsync({ companyId, data: payload });
        toast.success('Centro de custo criado com sucesso.');
      }
      onSuccess?.();
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'response' in err
          ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
          : undefined;
      toast.error(message ?? 'Erro ao salvar centro de custo.');
    }
  };

  const isPending =
    isSubmitting || createCostCenter.isPending || updateCostCenter.isPending;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid gap-4 sm:grid-cols-2">
        <div>
          <label htmlFor="code" className="block text-sm font-medium text-gray-700">
            Código *
          </label>
          <input
            id="code"
            type="text"
            {...register('code')}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
            placeholder="Ex: TI, RH, FIN"
          />
          {errors.code && (
            <p className="mt-1 text-sm text-red-600">{errors.code.message}</p>
          )}
        </div>
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-gray-700">
            Nome *
          </label>
          <input
            id="name"
            type="text"
            {...register('name')}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
            placeholder="Ex: Tecnologia da Informação"
          />
          {errors.name && (
            <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
          )}
        </div>
      </div>

      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700">
          Descrição
        </label>
        <textarea
          id="description"
          rows={3}
          {...register('description')}
          className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          placeholder="Descrição opcional do centro de custo"
        />
        {errors.description && (
          <p className="mt-1 text-sm text-red-600">{errors.description.message}</p>
        )}
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
          {isEdit ? 'Salvar' : 'Criar Centro de Custo'}
        </button>
      </div>
    </form>
  );
}
