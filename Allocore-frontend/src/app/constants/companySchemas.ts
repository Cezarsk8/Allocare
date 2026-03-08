import { z } from 'zod/v4';

export const createCompanySchema = z.object({
  name: z.string().min(1, 'Nome da empresa é obrigatório').max(200, 'Máximo 200 caracteres'),
  legalName: z.string().max(300, 'Máximo 300 caracteres').optional().or(z.literal('')),
  taxId: z.string().max(50, 'Máximo 50 caracteres').optional().or(z.literal('')),
});

export const updateCompanySchema = createCompanySchema;

export const addUserToCompanySchema = z.object({
  userId: z.string().min(1, 'ID do usuário é obrigatório'),
  roleInCompany: z.enum(['Viewer', 'Manager', 'Owner'], { message: 'Selecione um papel válido' }),
});

export type CreateCompanyFormData = z.infer<typeof createCompanySchema>;
export type UpdateCompanyFormData = z.infer<typeof updateCompanySchema>;
export type AddUserToCompanyFormData = z.infer<typeof addUserToCompanySchema>;
