# ServiceDesk Frontend

A modern frontend for the **ServiceDesk** IT ticketing/help desk project, built with:

- **React 18 + TypeScript**
- **Vite**
- **Tailwind CSS**
- **shadcn/ui-style components** (local components using Tailwind, `class-variance-authority`, `tailwind-merge`, and `clsx`)
- **React Router**
- **lucide-react** icons

## What's included

- App shell with sidebar navigation, top bar, search, and notifications affordance
- **Dashboard**: KPI cards, recent tickets, and SLA overview (mock data)
- **Tickets**: searchable/filterable ticket list
- **Ticket details**: request info, SLA panel, description, and activity timeline
- **New ticket**: create-ticket form (demo submission)
- **Login**: sign-in screen preview aligned with the ServiceDesk brand
- Placeholder pages for **Teams**, **Reports**, and **Settings** (planned in later backend phases)

The UI reflects the ServiceDesk domain (MIS ticket numbers like `MIS-26K013`, statuses, priorities, SLA states) and is ready to be connected to future backend APIs (e.g. `/api/v1/...`).

## Prerequisites

- Node.js 18+ and npm

## Run locally

```bash
cd frontend
npm install
npm run dev
```

Then open the URL shown by Vite (typically http://localhost:5173).

## Build

```bash
npm run build
npm run preview
```

## API proxy (optional)

`vite.config.ts` includes a dev proxy for `/api` to `https://localhost:7080` (the ASP.NET Core app). Once backend endpoints are available, replace mock data in `src/data/` with API calls.

## Notes

- This is a standalone frontend prototype/mock. The main ServiceDesk application is the ASP.NET Core MVC solution in the repository root (`src/ServiceDesk.Web`).
- Components in `src/components/ui/` follow shadcn/ui conventions and can be extended or replaced with generated shadcn components as needed.
