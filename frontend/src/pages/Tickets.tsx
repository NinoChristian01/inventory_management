import { useMemo, useState } from "react"
import { Link } from "react-router-dom"
import { Search, Filter, Plus } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { tickets } from "@/data/tickets"
import { cn } from "@/lib/utils"

function statusVariant(status: string) {
  if (status === "Resolved" || status === "Closed") return "success" as const
  if (status === "New" || status === "Open") return "info" as const
  if (status === "In Progress" || status === "Reopened") return "default" as const
  if (status.startsWith("Pending") || status === "On Hold") return "warning" as const
  if (status === "Canceled") return "muted" as const
  return "secondary" as const
}

function priorityClass(p: string) {
  if (p === "Critical") return "text-red-700 bg-red-50 border-red-200"
  if (p === "High") return "text-orange-700 bg-orange-50 border-orange-200"
  if (p === "Medium") return "text-blue-700 bg-blue-50 border-blue-200"
  return "text-emerald-700 bg-emerald-50 border-emerald-200"
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString("en-PH", { dateStyle: "medium", timeStyle: "short" })
}

const statuses = ["All", "New", "Open", "In Progress", "Pending", "Resolved", "Closed"] as const

export function Tickets() {
  const [q, setQ] = useState("")
  const [status, setStatus] = useState<(typeof statuses)[number]>("All")

  const filtered = useMemo(() => {
    const query = q.trim().toLowerCase()
    return tickets.filter((t) => {
      const matchesQ = !query || [t.number, t.subject, t.requester, t.department, t.category].some((v) => v.toLowerCase().includes(query))
      const matchesStatus =
        status === "All" ||
        (status === "Pending" ? t.status.startsWith("Pending") || t.status === "On Hold" : t.status === status)
      return matchesQ && matchesStatus
    })
  }, [q, status])

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Tickets</h2>
          <p className="text-sm text-muted-foreground">Track, triage, and resolve service requests.</p>
        </div>
        <Button asChild>
          <Link to="/tickets/new"><Plus className="h-4 w-4" /> New ticket</Link>
        </Button>
      </div>

      <Card>
        <CardHeader className="space-y-4">
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <CardTitle>All tickets</CardTitle>
              <CardDescription>{filtered.length} of {tickets.length} tickets shown</CardDescription>
            </div>
            <div className="flex w-full flex-col gap-2 sm:flex-row lg:w-auto">
              <div className="relative w-full sm:w-72">
                <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input value={q} onChange={(e) => setQ(e.target.value)} className="pl-9" placeholder="Search number, subject, requester..." />
              </div>
              <div className="flex items-center gap-2">
                <Filter className="h-4 w-4 text-muted-foreground" />
                <select
                  value={status}
                  onChange={(e) => setStatus(e.target.value as any)}
                  className="h-10 rounded-md border border-input bg-background px-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  {statuses.map((s) => (
                    <option key={s} value={s}>{s === "All" ? "All statuses" : s}</option>
                  ))}
                </select>
              </div>
            </div>
          </div>
        </CardHeader>
        <CardContent className="pt-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Ticket</TableHead>
                <TableHead className="hidden md:table-cell">Requester</TableHead>
                <TableHead className="hidden lg:table-cell">Assignee</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="hidden sm:table-cell">Priority</TableHead>
                <TableHead className="hidden xl:table-cell">Category</TableHead>
                <TableHead className="hidden lg:table-cell">Updated</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.map((t) => (
                <TableRow key={t.id}>
                  <TableCell>
                    <Link to={`/tickets/${t.id}`} className="font-semibold text-primary hover:underline">{t.number}</Link>
                    <div className="mt-0.5 max-w-[32rem] truncate text-sm text-muted-foreground">{t.subject}</div>
                  </TableCell>
                  <TableCell className="hidden md:table-cell">
                    <div className="font-medium">{t.requester}</div>
                    <div className="text-xs text-muted-foreground">{t.department}</div>
                  </TableCell>
                  <TableCell className="hidden lg:table-cell">{t.assignee ?? <span className="text-muted-foreground">Unassigned</span>}</TableCell>
                  <TableCell><Badge variant={statusVariant(t.status)}>{t.status}</Badge></TableCell>
                  <TableCell className="hidden sm:table-cell">
                    <span className={cn("inline-flex rounded-md border px-2 py-0.5 text-xs font-medium", priorityClass(t.priority))}>{t.priority}</span>
                  </TableCell>
                  <TableCell className="hidden xl:table-cell text-sm text-muted-foreground">{t.category}</TableCell>
                  <TableCell className="hidden lg:table-cell text-sm text-muted-foreground">{formatDate(t.updatedAt)}</TableCell>
                </TableRow>
              ))}
              {filtered.length === 0 && (
                <TableRow>
                  <TableCell colSpan={7} className="py-10 text-center text-muted-foreground">
                    No tickets match your filters.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  )
}
