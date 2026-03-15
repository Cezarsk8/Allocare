import { z } from 'zod/v4';

export const createEmployeeSchema = z.object({
  name: z
    .string()
    .min(1, 'Nome é obrigatório')
    .max(200, 'Máximo 200 caracteres'),
  email: z
    .string()
    .min(1, 'E-mail é obrigatório')
    .email('E-mail inválido')
    .max(300, 'Máximo 300 caracteres'),
  costCenterId: z.string().optional().or(z.literal('')),
  jobTitle: z
    .string()
    .max(200, 'Máximo 200 caracteres')
    .optional()
    .or(z.literal('')),
  hireDate: z.string().optional().or(z.literal('')),
});

export const updateEmployeeSchema = createEmployeeSchema;

export const terminateEmployeeSchema = z.object({
  terminationDate: z.string().min(1, 'Data de desligamento é obrigatória'),
});

export type CreateEmployeeFormData = z.infer<typeof createEmployeeSchema>;
export type UpdateEmployeeFormData = z.infer<typeof updateEmployeeSchema>;
export type TerminateEmployeeFormData = z.infer<typeof terminateEmployeeSchema>;
