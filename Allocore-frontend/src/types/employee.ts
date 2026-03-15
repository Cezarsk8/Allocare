export interface EmployeeDto {
  id: string;
  companyId: string;
  name: string;
  email: string;
  costCenterId: string | null;
  costCenterName: string | null;
  costCenterCode: string | null;
  jobTitle: string | null;
  hireDate: string | null;
  terminationDate: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface EmployeeListItemDto {
  id: string;
  name: string;
  email: string;
  costCenterName: string | null;
  jobTitle: string | null;
  isActive: boolean;
}

export interface CreateEmployeeRequest {
  name: string;
  email: string;
  costCenterId?: string | null;
  jobTitle?: string | null;
  hireDate?: string | null;
}

export interface UpdateEmployeeRequest {
  name: string;
  email: string;
  costCenterId?: string | null;
  jobTitle?: string | null;
  hireDate?: string | null;
}

export interface TerminateEmployeeRequest {
  terminationDate: string;
}
