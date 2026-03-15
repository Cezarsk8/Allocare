import { z } from 'zod/v4';

export const createCostCenterSchema = z.object({
  code: z
    .string()
    .min(1, 'Código é obrigatório')
    .max(50, 'Máximo 50 caracteres'),
  name: z
    .string()
    .min(1, 'Nome é obrigatório')
    .max(200, 'Máximo 200 caracteres'),
  description: z
    .string()
    .max(2000, 'Máximo 2000 caracteres')
    .optional()
    .or(z.literal('')),
});

export const updateCostCenterSchema = createCostCenterSchema;

export type CreateCostCenterFormData = z.infer<typeof createCostCenterSchema>;
export type UpdateCostCenterFormData = z.infer<typeof updateCostCenterSchema>;
