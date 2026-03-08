export interface CompanyDto {
  id: string;
  name: string;
  legalName: string | null;
  taxId: string | null;
  isActive: boolean;
  createdAt: string;
  userRole: string | null;
}

export interface CreateCompanyRequest {
  name: string;
  legalName?: string | null;
  taxId?: string | null;
}

export interface UpdateCompanyRequest {
  name: string;
  legalName?: string | null;
  taxId?: string | null;
}

export interface AddUserToCompanyRequest {
  userId: string;
  roleInCompany: string;
}

export interface UserCompanyDto {
  userId: string;
  userEmail: string;
  userFullName: string;
  companyId: string;
  companyName: string;
  roleInCompany: string;
  joinedAt: string;
}
