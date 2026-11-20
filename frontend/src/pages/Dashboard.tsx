import { Link } from "react-router-dom"
import { AlertTriangle, CheckCircle2, Clock3, Inbox, ArrowRight, TrendingUp } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
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

function slaClass(s: string) {
  if (s === "Breached") return "text-red-700"
  if (s === "At risk") return "text-amber-700"
  if (s === "Paused") return "text-slate-500"
  if (s === "Met") return "text-emerald-700"
  return "text-teal-700"
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString("en-PH", { dateStyle: "medium", timeStyle: "short" })
}

export function Dashboard() {
  const openCount = tickets.filter((t) => ["New", "Open", "In Progress", "Pending Requester", "Pending Third Party", "On Hold", "Reopened"].includes(t.status)).length
  const breached = tickets.filter((t) => t.sla === "Breached").length
  const atRisk = tickets.filter((t) => t.sla === "At risk").length
  const resolvedToday = tickets.filter((t) => t.status === "Resolved" || t.status === "Closed").length
  const recent = [...tickets].sort((a, b) => +new Date(b.updatedAt) - +new Date(a.updatedAt)).slice(0, 6)

  const stats = [
    { label: "Open tickets", value: openCount, icon: Inbox, tone: "text-teal-700 bg-teal-50", note: "Across your queues" },
    { label: "At risk", value: atRisk, icon: Clock3, tone: "text-amber-700 bg-amber-50", note: "Approaching SLA target" },
    { label: "Breached", value: breached, icon: AlertTriangle, tone: "text-red-700 bg-red-50", note: "Needs immediate action" },
    { label: "Resolved / closed", value: resolvedToday, icon: CheckCircle2, tone: "text-emerald-700 bg-emerald-50", note: "Recent completions" },
  ]

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-sm text-muted-foreground">Friday, 20 November 2026 · Asia/Manila</p>
          <h2 className="mt-1 text-2xl font-bold tracking-tight">Good morning, Juan</h2>
          <p className="mt-1 text-sm text-muted-foreground">Here’s what needs attention in the MIS service desk today.</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" asChild>
            <Link to="/tickets">View queue</Link>
          </Button>
          <Button asChild>
            <Link to="/tickets/new">New ticket</Link>
          </Button>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {stats.map((s) => {
          const Icon = s.icon
          return (
            <Card key={s.label}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">{s.label}</CardTitle>
                <div className={cn("rounded-md p-2", s.tone)}>
                  <Icon className="h-4 w-4" />
                </div>
              </CardHeader>
              <CardContent>
                <div className="text-3xl font-bold">{s.value}</div>
                <p className="mt-1 text-xs text-muted-foreground">{s.note}</p>
              </CardContent>
            </Card>
          )
        })}
      </div>

      <div className="grid gap-6 xl:grid-cols-3">
        <Card className="xl:col-span-2">
          <CardHeader className="flex flex-row items-center justify-between">
            <div>
              <CardTitle>Recent tickets</CardTitle>
              <CardDescription>Latest updates across your team</CardDescription>
            </div>
            <Button variant="ghost" size="sm" asChild>
              <Link to="/tickets" className="gap-1">All tickets <ArrowRight className="h-4 w-4" /></Link>
            </Button>
          </CardHeader>
          <CardContent className="pt-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Ticket</TableHead>
                  <TableHead className="hidden md:table-cell">Requester</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="hidden sm:table-cell">Priority</TableHead>
                  <TableHead className="hidden lg:table-cell">SLA</TableHead>
                  <TableHead className="hidden lg:table-cell">Updated</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {recent.map((t) => (
                  <TableRow key={t.id}>
                    <TableCell>
                      <Link to={`/tickets/${t.id}`} className="font-medium text-primary hover:underline">
                        {t.number}
                      </Link>
                      <div className="mt-0.5 max-w-[28rem] truncate text-sm text-muted-foreground">{t.subject}</div>
                      <div className="mt-1 text-xs text-muted-foreground md:hidden">{t.requester}</div>
                    </TableCell>
                    <TableCell className="hidden md:table-cell">
                      <div className="font-medium">{t.requester}</div>
                      <div className="text-xs text-muted-foreground">{t.department}</div>
                    </TableCell>
                    <TableCell>
                      <Badge variant={statusVariant(t.status)}>{t.status}</Badge>
                    </TableCell>
                    <TableCell className="hidden sm:table-cell">
                      <span className={cn("inline-flex rounded-md border px-2 py-0.5 text-xs font-medium", priorityClass(t.priority))}>
                        {t.priority}
                      </span>
                    </TableCell>
                    <TableCell className={cn("hidden text-sm font-medium lg:table-cell", slaClass(t.sla))}>{t.sla}</TableCell>
                    <TableCell className="hidden text-sm text-muted-foreground lg:table-cell">{formatDate(t.updatedAt)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><TrendingUp className="h-4 w-4 text-primary" /> SLA overview</CardTitle>
            <CardDescription>Current response and resolution health</CardDescription>
          </CardHeader>
          <CardContent className="space-y-5">
            {[
              { label: "On track", value: tickets.filter((t) => t.sla === "On track").length, color: "bg-teal-600", total: tickets.length },
              { label: "At risk", value: atRisk, color: "bg-amber-500", total: tickets.length },
              { label: "Breached", value: breached, color: "bg-red-600", total: tickets.length },
              { label: "Paused", value: tickets.filter((t) => t.sla === "Paused").length, color: "bg-slate-400", total: tickets.length },
              { label: "Met", value: tickets.filter((t) => t.sla === "Met").length, color: "bg-emerald-600", total: tickets.length },
            ].map((row) => (
              <div key={row.label} className="space-y-1.5">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-muted-foreground">{row.label}</span>
                  <span className="font-medium">{row.value}</span>
                </div>
                <div className="h-2 w-full overflow-hidden rounded-full bg-muted">
                  <div className={cn("h-full rounded-full", row.color)} style={{ width: `${row.total ? (row.value / row.total) * 100 : 0}%` }} />
                </div>
              </div>
            ))}
            <div className="rounded-lg border bg-muted/30 p-3 text-sm text-muted-foreground">
              Business-hours SLA: Mon–Fri 08:00–17:00 (Asia/Manila). Critical tickets run 24×7.
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
