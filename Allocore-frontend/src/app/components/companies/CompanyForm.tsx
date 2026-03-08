'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createCompanySchema, type CreateCompanyFormData } from '@/app/constants/companySchemas';
import { useCreateCompany, useUpdateCompany } from '@/app/hooks/useCompanies';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';

interface CompanyFormProps {
  companyId?: string;
  defaultValues?: CreateCompanyFormData;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function CompanyForm({ companyId, defaultValues, onSuccess, onCancel }: CompanyFormProps) {
  const isEdit = !!companyId;
  const createCompany = useCreateCompany();
  const updateCompany = useUpdateCompany();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateCompanyFormData>({
    resolver: zodResolver(createCompanySchema),
    defaultValues: defaultValues ?? { name: '', legalName: '', taxId: '' },
  });

  const onSubmit = async (data: CreateCompanyFormData) => {
    try {
      const payload = {
        ...data,
        legalName: data.legalName || null,
        taxId: data.taxId || null,
      };

      if (isEdit) {
        await updateCompany.mutateAsync({ id: companyId, data: payload });
        toast.success('Empresa atualizada com sucesso.');
      } else {
        await createCompany.mutateAsync(payload);
        toast.success('Empresa criada com sucesso.');
      }
      onSuccess?.();
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'response' in err
          ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
          : undefined;
      toast.error(message ?? 'Erro ao salvar empresa.');
    }
  };

  const isPending = isSubmitting || createCompany.isPending || updateCompany.isPending;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700">
          Nome da Empresa *
        </label>
        <input
          id="name"
          type="text"
          {...register('name')}
          className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          placeholder="Ex: Acme Corp"
        />
        {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>}
      </div>

      <div>
        <label htmlFor="legalName" className="block text-sm font-medium text-gray-700">
          Razão Social
        </label>
        <input
          id="legalName"
          type="text"
          {...register('legalName')}
          className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          placeholder="Ex: Acme Corporation LLC"
        />
        {errors.legalName && <p className="mt-1 text-sm text-red-600">{errors.legalName.message}</p>}
      </div>

      <div>
        <label htmlFor="taxId" className="block text-sm font-medium text-gray-700">
          CNPJ / Tax ID
        </label>
        <input
          id="taxId"
          type="text"
          {...register('taxId')}
          className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          placeholder="Ex: 12.345.678/0001-90"
        />
        {errors.taxId && <p className="mt-1 text-sm text-red-600">{errors.taxId.message}</p>}
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
          {isEdit ? 'Salvar' : 'Criar Empresa'}
        </button>
      </div>
    </form>
  );
}
