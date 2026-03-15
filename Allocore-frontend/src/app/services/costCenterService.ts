import apiClient from './apiClient';
import type {
  CostCenterDto,
  CostCenterListItemDto,
  CreateCostCenterRequest,
  UpdateCostCenterRequest,
} from '@/types/costCenter';
import type { EmployeeListItemDto } from '@/types/employee';
import type { PagedResult } from '@/types/common';

interface CostCenterListParams {
  page?: number;
  pageSize?: number;
  isActive?: boolean | null;
  search?: string;
}

interface CostCenterEmployeesParams {
  page?: number;
  pageSize?: number;
}

export const costCenterService = {
  getByCompany: (companyId: string, params: CostCenterListParams = {}) =>
    apiClient
      .get<PagedResult<CostCenterListItemDto>>(
        `/companies/${companyId}/cost-centers`,
        { params },
      )
      .then((r) => r.data),

  getById: (companyId: string, id: string) =>
    apiClient
      .get<CostCenterDto>(`/companies/${companyId}/cost-centers/${id}`)
      .then((r) => r.data),

  create: (companyId: string, data: CreateCostCenterRequest) =>
    apiClient
      .post<CostCenterDto>(`/companies/${companyId}/cost-centers`, data)
      .then((r) => r.data),

  update: (companyId: string, id: string, data: UpdateCostCenterRequest) =>
    apiClient
      .put<CostCenterDto>(`/companies/${companyId}/cost-centers/${id}`, data)
      .then((r) => r.data),

  deactivate: (companyId: string, id: string) =>
    apiClient.patch(`/companies/${companyId}/cost-centers/${id}/deactivate`),

  activate: (companyId: string, id: string) =>
    apiClient.patch(`/companies/${companyId}/cost-centers/${id}/activate`),

  getEmployees: (companyId: string, id: string, params: CostCenterEmployeesParams = {}) =>
    apiClient
      .get<PagedResult<EmployeeListItemDto>>(
        `/companies/${companyId}/cost-centers/${id}/employees`,
        { params },
      )
      .then((r) => r.data),
};
