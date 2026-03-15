import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { employeeService } from '@/app/services/employeeService';
import type {
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
  TerminateEmployeeRequest,
} from '@/types/employee';

interface EmployeeListParams {
  page?: number;
  pageSize?: number;
  costCenterId?: string | null;
  isActive?: boolean | null;
  search?: string;
}

export function useEmployees(companyId: string, params: EmployeeListParams = {}) {
  return useQuery({
    queryKey: ['employees', companyId, params],
    queryFn: () => employeeService.getByCompany(companyId, params),
    enabled: !!companyId,
  });
}

export function useEmployee(companyId: string, id: string) {
  return useQuery({
    queryKey: ['employee', companyId, id],
    queryFn: () => employeeService.getById(companyId, id),
    enabled: !!companyId && !!id,
  });
}

export function useCreateEmployee() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, data }: { companyId: string; data: CreateEmployeeRequest }) =>
      employeeService.create(companyId, data),
    onSuccess: (_, { companyId }) => {
      queryClient.invalidateQueries({ queryKey: ['employees', companyId] });
    },
  });
}

export function useUpdateEmployee() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, id, data }: { companyId: string; id: string; data: UpdateEmployeeRequest }) =>
      employeeService.update(companyId, id, data),
    onSuccess: (_, { companyId, id }) => {
      queryClient.invalidateQueries({ queryKey: ['employees', companyId] });
      queryClient.invalidateQueries({ queryKey: ['employee', companyId, id] });
    },
  });
}

export function useTerminateEmployee() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, id, data }: { companyId: string; id: string; data: TerminateEmployeeRequest }) =>
      employeeService.terminate(companyId, id, data),
    onSuccess: (_, { companyId, id }) => {
      queryClient.invalidateQueries({ queryKey: ['employees', companyId] });
      queryClient.invalidateQueries({ queryKey: ['employee', companyId, id] });
    },
  });
}

export function useReactivateEmployee() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, id }: { companyId: string; id: string }) =>
      employeeService.reactivate(companyId, id),
    onSuccess: (_, { companyId, id }) => {
      queryClient.invalidateQueries({ queryKey: ['employees', companyId] });
      queryClient.invalidateQueries({ queryKey: ['employee', companyId, id] });
    },
  });
}

export function useDeactivateEmployee() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, id }: { companyId: string; id: string }) =>
      employeeService.deactivate(companyId, id),
    onSuccess: (_, { companyId, id }) => {
      queryClient.invalidateQueries({ queryKey: ['employees', companyId] });
      queryClient.invalidateQueries({ queryKey: ['employee', companyId, id] });
    },
  });
}

export function useActivateEmployee() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ companyId, id }: { companyId: string; id: string }) =>
      employeeService.activate(companyId, id),
    onSuccess: (_, { companyId, id }) => {
      queryClient.invalidateQueries({ queryKey: ['employees', companyId] });
      queryClient.invalidateQueries({ queryKey: ['employee', companyId, id] });
    },
  });
}
