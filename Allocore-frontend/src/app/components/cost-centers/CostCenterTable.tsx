'use client';

import type { CostCenterListItemDto } from '@/types/costCenter';
import { Power, PowerOff } from 'lucide-react';

interface CostCenterTableProps {
  costCenters: CostCenterListItemDto[];
  onEdit: (id: string) => void;
  onToggleStatus: (id: string, isActive: boolean) => void;
  isPending?: boolean;
}

export function CostCenterTable({
  costCenters,
  onEdit,
  onToggleStatus,
  isPending,
}: CostCenterTableProps) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-gray-200 text-left text-gray-500">
            <th className="pb-3 font-medium">Código</th>
            <th className="pb-3 font-medium">Nome</th>
            <th className="pb-3 font-medium">Colaboradores</th>
            <th className="pb-3 font-medium">Status</th>
            <th className="pb-3 font-medium">Ações</th>
          </tr>
        </thead>
        <tbody>
          {costCenters.map((cc) => (
            <tr key={cc.id} className="border-b border-gray-100">
              <td className="py-3">
                <span className="rounded bg-gray-100 px-2 py-0.5 font-mono text-xs font-medium text-gray-700">
                  {cc.code}
                </span>
              </td>
              <td className="py-3">
                <button
                  onClick={() => onEdit(cc.id)}
                  className="font-medium text-gray-900 hover:text-blue-600"
                >
                  {cc.name}
                </button>
              </td>
              <td className="py-3 text-gray-600">{cc.employeeCount}</td>
              <td className="py-3">
                <span
                  className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${
                    cc.isActive
                      ? 'bg-green-100 text-green-700'
                      : 'bg-gray-100 text-gray-600'
                  }`}
                >
                  {cc.isActive ? 'Ativo' : 'Inativo'}
                </span>
              </td>
              <td className="py-3">
                <button
                  onClick={() => onToggleStatus(cc.id, cc.isActive)}
                  disabled={isPending}
                  className={`rounded p-1 disabled:opacity-50 ${
                    cc.isActive
                      ? 'text-gray-400 hover:bg-gray-100 hover:text-gray-600'
                      : 'text-green-500 hover:bg-green-50'
                  }`}
                  title={cc.isActive ? 'Desativar' : 'Ativar'}
                >
                  {cc.isActive ? (
                    <PowerOff className="h-4 w-4" />
                  ) : (
                    <Power className="h-4 w-4" />
                  )}
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
