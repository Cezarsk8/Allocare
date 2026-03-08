# Allocore Design System

## Foundation

- **Framework**: Tailwind CSS 4 (CSS-based configuration)
- **Icons**: Lucide React
- **Toasts**: Sonner (top-right, 5s duration)
- **Forms**: React Hook Form + Zod validation

## Colors (Base Light Theme)

| Token | Value | Usage |
|-------|-------|-------|
| Background | `rgb(249 250 251)` / `gray-50` | Page background |
| Foreground | `rgb(17 24 39)` / `gray-900` | Default text |
| Primary | `#2563eb` / `blue-600` | Buttons, links, accents |
| Danger | `red-600` | Error states, destructive actions |
| Success | `green-600` | Success states |
| Warning | `amber-500` | Warning states |

## Typography

- Base font: System font stack (no custom fonts loaded)
- Font smoothing: antialiased
- Headings: `font-semibold` or `font-bold`

## Component Patterns

### Atoms (Primitive)
- Button, Input, Label, Badge, Spinner, Select, Textarea
- Self-contained, no business logic

### Molecules (Composed)
- FormField (Label + Input + Error), Card, Dialog, DataTable row
- Compose atoms, minimal logic

### Organisms (Features)
- LoginForm, ProviderTable, Sidebar, CompanySelector
- Full business features, connect to hooks/services

## Responsive Baseline

- Mobile-first: default styles for mobile
- Breakpoints: `sm:` (640px), `md:` (768px), `lg:` (1024px), `xl:` (1280px)
- Sidebar: hidden on mobile, visible on `lg:`

## Dark Mode

- Not yet implemented. Will use CSS variables when added.
