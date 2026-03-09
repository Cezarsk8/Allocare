import { z } from 'zod';

export const loginSchema = z.object({
  email: z.string().min(1, 'E-mail é obrigatório').email('E-mail inválido'),
  password: z.string().min(1, 'Senha é obrigatória'),
});

export type LoginFormData = z.infer<typeof loginSchema>;

export const registerSchema = z.object({
  firstName: z.string().min(1, 'Nome é obrigatório').max(100, 'Máximo 100 caracteres'),
  lastName: z.string().min(1, 'Sobrenome é obrigatório').max(100, 'Máximo 100 caracteres'),
  email: z.string().min(1, 'E-mail é obrigatório').email('E-mail inválido').max(256, 'Máximo 256 caracteres'),
  password: z
    .string()
    .min(8, 'Mínimo 8 caracteres')
    .regex(/[A-Z]/, 'Deve conter pelo menos uma letra maiúscula')
    .regex(/[a-z]/, 'Deve conter pelo menos uma letra minúscula')
    .regex(/[0-9]/, 'Deve conter pelo menos um número')
    .regex(/[^a-zA-Z0-9]/, 'Deve conter pelo menos um caractere especial'),
});

export type RegisterFormData = z.infer<typeof registerSchema>;
