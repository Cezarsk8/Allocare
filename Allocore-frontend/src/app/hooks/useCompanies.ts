import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { companyService } from '@/app/services/companyService';
import type { CreateCompanyRequest, UpdateCompanyRequest, AddUserToCompanyRequest } from '@/types/company';

export function useMyCompanies() {
  return useQuery({
    queryKey: ['my-companies'],
    queryFn: companyService.getMyCompanies,
  });
}

export function useCompany(id: string) {
  return useQuery({
    queryKey: ['company', id],
    queryFn: () => companyService.getCompanyById(id),
    enabled: !!id,
  });
}

export function useCompanyUsers(companyId: string) {
  return useQuery({
    queryKey: ['company-users', companyId],
    queryFn: () => companyService.getCompanyUsers(companyId),
    enabled: !!companyId,
  });
}

export function useCreateCompany() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCompanyRequest) => companyService.createCompany(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['my-companies'] }),
  });
}

export function useUpdateCompany() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCompanyRequest }) =>
      companyService.updateCompany(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['my-companies'] });
      queryClient.invalidateQueries({ queryKey: ['company', id] });
    },
  });
}

export function useAddUserToCompany() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, data }: { companyId: string; data: AddUserToCompanyRequest }) =>
      companyService.addUserToCompany(companyId, data),
    onSuccess: (_, { companyId }) =>
      queryClient.invalidateQueries({ queryKey: ['company-users', companyId] }),
  });
}

export function useRemoveUserFromCompany() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, userId }: { companyId: string; userId: string }) =>
      companyService.removeUserFromCompany(companyId, userId),
    onSuccess: (_, { companyId }) =>
      queryClient.invalidateQueries({ queryKey: ['company-users', companyId] }),
  });
}
