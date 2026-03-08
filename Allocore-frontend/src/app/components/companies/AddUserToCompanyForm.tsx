'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { addUserToCompanySchema, type AddUserToCompanyFormData } from '@/app/constants/companySchemas';
import { useAddUserToCompany } from '@/app/hooks/useCompanies';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';

interface AddUserToCompanyFormProps {
  companyId: string;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function AddUserToCompanyForm({ companyId, onSuccess, onCancel }: AddUserToCompanyFormProps) {
  const addUser = useAddUserToCompany();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<AddUserToCompanyFormData>({
    resolver: zodResolver(addUserToCompanySchema),
    defaultValues: { userId: '', roleInCompany: 'Viewer' },
  });

  const onSubmit = async (data: AddUserToCompanyFormData) => {
    try {
      await addUser.mutateAsync({ companyId, data });
      toast.success('Usuário adicionado à empresa.');
      onSuccess?.();
    } catch (err: unknown) {
      const message =
        err && typeof err === 'object' && 'response' in err
          ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
          : undefined;
      toast.error(message ?? 'Erro ao adicionar usuário.');
    }
  };

  const isPending = isSubmitting || addUser.isPending;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <label htmlFor="userId" className="block text-sm font-medium text-gray-700">
          ID do Usuário *
        </label>
        <input
          id="userId"
          type="text"
          {...register('userId')}
          className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
          placeholder="UUID do usuário"
        />
        {errors.userId && <p className="mt-1 text-sm text-red-600">{errors.userId.message}</p>}
      </div>

      <div>
        <label htmlFor="roleInCompany" className="block text-sm font-medium text-gray-700">
          Papel na Empresa *
        </label>
        <select
          id="roleInCompany"
          {...register('roleInCompany')}
          className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none"
        >
          <option value="Viewer">Viewer</option>
          <option value="Manager">Manager</option>
          <option value="Owner">Owner</option>
        </select>
        {errors.roleInCompany && (
          <p className="mt-1 text-sm text-red-600">{errors.roleInCompany.message}</p>
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
          Adicionar
        </button>
      </div>
    </form>
  );
}
