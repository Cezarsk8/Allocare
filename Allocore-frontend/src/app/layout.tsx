import type { Metadata, Viewport } from 'next';
import { Providers } from './providers';
import './globals.css';

export const metadata: Metadata = {
  title: 'Allocare — Gestão de Provedores & Controle de Custos',
  description: 'Plataforma centralizada para gestão de provedores, contratos, serviços e controle de custos corporativos.',
  keywords: ['gestão de provedores', 'controle de custos', 'contratos', 'procurement', 'cost allocation'],
  authors: [{ name: 'Allocare Team' }],
};

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
  themeColor: '#2563eb',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="pt-BR">
      <body>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
