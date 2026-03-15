'use client';

import type { EmployeeListItemDto } from '@/types/employee';
import { Power, PowerOff, UserX, UserCheck } from 'lucide-react';

interface EmployeeTableProps {
  employees: EmployeeListItemDto[];
  onEdit: (id: string) => void;
  onToggleStatus: (id: string, isActive: boolean) => void;
  onTerminate: (id: string, name: string) => void;
  onReactivate: (id: string) => void;
  isPending?: boolean;
}

export function EmployeeTable({
  employees,
  onEdit,
  onToggleStatus,
  onTerminate,
  onReactivate,
  isPending,
}: EmployeeTableProps) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-gray-200 text-left text-gray-500">
            <th className="pb-3 font-medium">Nome</th>
            <th className="pb-3 font-medium">E-mail</th>
            <th className="pb-3 font-medium">Centro de Custo</th>
            <th className="pb-3 font-medium">Cargo</th>
            <th className="pb-3 font-medium">Status</th>
            <th className="pb-3 font-medium">Ações</th>
          </tr>
        </thead>
        <tbody>
          {employees.map((emp) => (
            <tr key={emp.id} className="border-b border-gray-100">
              <td className="py-3">
                <button
                  onClick={() => onEdit(emp.id)}
                  className="font-medium text-gray-900 hover:text-blue-600"
                >
                  {emp.name}
                </button>
              </td>
              <td className="py-3 text-gray-600">{emp.email}</td>
              <td className="py-3 text-gray-600">
                {emp.costCenterName ?? (
                  <span className="text-gray-400">—</span>
                )}
              </td>
              <td className="py-3 text-gray-600">
                {emp.jobTitle ?? (
                  <span className="text-gray-400">—</span>
                )}
              </td>
              <td className="py-3">
                <span
                  className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${
                    emp.isActive
                      ? 'bg-green-100 text-green-700'
                      : 'bg-gray-100 text-gray-600'
                  }`}
                >
                  {emp.isActive ? 'Ativo' : 'Inativo'}
                </span>
              </td>
              <td className="py-3">
                <div className="flex gap-1">
                  {emp.isActive ? (
                    <>
                      <button
                        onClick={() => onTerminate(emp.id, emp.name)}
                        disabled={isPending}
                        className="rounded p-1 text-amber-500 hover:bg-amber-50 disabled:opacity-50"
                        title="Desligar"
                      >
                        <UserX className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => onToggleStatus(emp.id, emp.isActive)}
                        disabled={isPending}
                        className="rounded p-1 text-gray-400 hover:bg-gray-100 hover:text-gray-600 disabled:opacity-50"
                        title="Desativar"
                      >
                        <PowerOff className="h-4 w-4" />
                      </button>
                    </>
                  ) : (
                    <>
                      <button
                        onClick={() => onReactivate(emp.id)}
                        disabled={isPending}
                        className="rounded p-1 text-blue-500 hover:bg-blue-50 disabled:opacity-50"
                        title="Reativar"
                      >
                        <UserCheck className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => onToggleStatus(emp.id, emp.isActive)}
                        disabled={isPending}
                        className="rounded p-1 text-green-500 hover:bg-green-50 disabled:opacity-50"
                        title="Ativar"
                      >
                        <Power className="h-4 w-4" />
                      </button>
                    </>
                  )}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
