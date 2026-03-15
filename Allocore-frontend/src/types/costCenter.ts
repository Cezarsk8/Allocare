export interface CostCenterDto {
  id: string;
  companyId: string;
  code: string;
  name: string;
  description: string | null;
  isActive: boolean;
  employeeCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface CostCenterListItemDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  employeeCount: number;
}

export interface CreateCostCenterRequest {
  code: string;
  name: string;
  description?: string | null;
}

export interface UpdateCostCenterRequest {
  code: string;
  name: string;
  description?: string | null;
}
