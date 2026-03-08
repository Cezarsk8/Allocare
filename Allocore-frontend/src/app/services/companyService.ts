import apiClient from './apiClient';
import type {
  CompanyDto,
  CreateCompanyRequest,
  UpdateCompanyRequest,
  AddUserToCompanyRequest,
  UserCompanyDto,
} from '@/types/company';

export const companyService = {
  getMyCompanies: () =>
    apiClient.get<CompanyDto[]>('/my/companies').then((r) => r.data),

  getCompanyById: (id: string) =>
    apiClient.get<CompanyDto>(`/companies/${id}`).then((r) => r.data),

  createCompany: (data: CreateCompanyRequest) =>
    apiClient.post<CompanyDto>('/companies', data).then((r) => r.data),

  updateCompany: (id: string, data: UpdateCompanyRequest) =>
    apiClient.put<CompanyDto>(`/companies/${id}`, data).then((r) => r.data),

  getCompanyUsers: (companyId: string) =>
    apiClient.get<UserCompanyDto[]>(`/companies/${companyId}/users`).then((r) => r.data),

  addUserToCompany: (companyId: string, data: AddUserToCompanyRequest) =>
    apiClient.post<UserCompanyDto>(`/companies/${companyId}/users`, data).then((r) => r.data),

  removeUserFromCompany: (companyId: string, userId: string) =>
    apiClient.delete(`/companies/${companyId}/users/${userId}`),
};
