import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"
import { AppLayout } from "@/components/layout/AppLayout"
import { Dashboard } from "@/pages/Dashboard"
import { Tickets } from "@/pages/Tickets"
import { TicketDetail } from "@/pages/TicketDetail"
import { NewTicket } from "@/pages/NewTicket"
import { Login } from "@/pages/Login"
import { Placeholder } from "@/pages/Placeholder"

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route element={<AppLayout />}>
          <Route index element={<Dashboard />} />
          <Route path="tickets" element={<Tickets />} />
          <Route path="tickets/new" element={<NewTicket />} />
          <Route path="tickets/:id" element={<TicketDetail />} />
          <Route path="teams" element={<Placeholder title="Teams" description="Manage service teams, membership, and assignment queues." />} />
          <Route path="reports" element={<Placeholder title="Reports" description="Operational metrics, SLA compliance, and exportable summaries." />} />
          <Route path="settings" element={<Placeholder title="Settings" description="System configuration, categories, SLA policies, and preferences." />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
