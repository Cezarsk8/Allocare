import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { costCenterService } from '@/app/services/costCenterService';
import type { CreateCostCenterRequest, UpdateCostCenterRequest } from '@/types/costCenter';

interface CostCenterListParams {
  page?: number;
  pageSize?: number;
  isActive?: boolean | null;
  search?: string;
}

export function useCostCenters(companyId: string, params: CostCenterListParams = {}) {
  return useQuery({
    queryKey: ['cost-centers', companyId, params],
    queryFn: () => costCenterService.getByCompany(companyId, params),
    enabled: !!companyId,
  });
}

export function useCostCenter(companyId: string, id: string) {
  return useQuery({
    queryKey: ['cost-center', companyId, id],
    queryFn: () => costCenterService.getById(companyId, id),
    enabled: !!companyId && !!id,
  });
}

export function useCreateCostCenter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, data }: { companyId: string; data: CreateCostCenterRequest }) =>
      costCenterService.create(companyId, data),
    onSuccess: (_, { companyId }) => {
      queryClient.invalidateQueries({ queryKey: ['cost-centers', companyId] });
    },
  });
}

export function useUpdateCostCenter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, id, data }: { companyId: string; id: string; data: UpdateCostCenterRequest }) =>
      costCenterService.update(companyId, id, data),
    onSuccess: (_, { companyId, id }) => {
      queryClient.invalidateQueries({ queryKey: ['cost-centers', companyId] });
      queryClient.invalidateQueries({ queryKey: ['cost-center', companyId, id] });
    },
  });
}

export function useDeactivateCostCenter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, id }: { companyId: string; id: string }) =>
      costCenterService.deactivate(companyId, id),
    onSuccess: (_, { companyId, id }) => {
      queryClient.invalidateQueries({ queryKey: ['cost-centers', companyId] });
      queryClient.invalidateQueries({ queryKey: ['cost-center', companyId, id] });
    },
  });
}

export function useActivateCostCenter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, id }: { companyId: string; id: string }) =>
      costCenterService.activate(companyId, id),
    onSuccess: (_, { companyId, id }) => {
      queryClient.invalidateQueries({ queryKey: ['cost-centers', companyId] });
      queryClient.invalidateQueries({ queryKey: ['cost-center', companyId, id] });
    },
  });
}
