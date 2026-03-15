import apiClient from './apiClient';
import type {
  EmployeeDto,
  EmployeeListItemDto,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
  TerminateEmployeeRequest,
} from '@/types/employee';
import type { PagedResult } from '@/types/common';

interface EmployeeListParams {
  page?: number;
  pageSize?: number;
  costCenterId?: string | null;
  isActive?: boolean | null;
  search?: string;
}

export const employeeService = {
  getByCompany: (companyId: string, params: EmployeeListParams = {}) =>
    apiClient
      .get<PagedResult<EmployeeListItemDto>>(
        `/companies/${companyId}/employees`,
        { params },
      )
      .then((r) => r.data),

  getById: (companyId: string, id: string) =>
    apiClient
      .get<EmployeeDto>(`/companies/${companyId}/employees/${id}`)
      .then((r) => r.data),

  create: (companyId: string, data: CreateEmployeeRequest) =>
    apiClient
      .post<EmployeeDto>(`/companies/${companyId}/employees`, data)
      .then((r) => r.data),

  update: (companyId: string, id: string, data: UpdateEmployeeRequest) =>
    apiClient
      .put<EmployeeDto>(`/companies/${companyId}/employees/${id}`, data)
      .then((r) => r.data),

  terminate: (companyId: string, id: string, data: TerminateEmployeeRequest) =>
    apiClient.patch(`/companies/${companyId}/employees/${id}/terminate`, data),

  reactivate: (companyId: string, id: string) =>
    apiClient.patch(`/companies/${companyId}/employees/${id}/reactivate`),

  deactivate: (companyId: string, id: string) =>
    apiClient.patch(`/companies/${companyId}/employees/${id}/deactivate`),

  activate: (companyId: string, id: string) =>
    apiClient.patch(`/companies/${companyId}/employees/${id}/activate`),
};
