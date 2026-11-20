import { Link, useParams } from "react-router-dom"
import { ArrowLeft, Clock, MessageSquare, User, Building2, Tag, ShieldAlert } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"
import { Separator } from "@/components/ui/separator"
import { getTicketById, ticketTimeline } from "@/data/tickets"
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

function formatDate(iso: string | null) {
  if (!iso) return "—"
  return new Date(iso).toLocaleString("en-PH", { dateStyle: "medium", timeStyle: "short" })
}

export function TicketDetail() {
  const { id } = useParams()
  const ticket = getTicketById(Number(id))
  const timeline = ticket ? ticketTimeline[ticket.id] ?? [] : []

  if (!ticket) {
    return (
      <Card>
        <CardContent className="py-10 text-center">
          <p className="text-muted-foreground">Ticket not found.</p>
          <Button asChild variant="outline" className="mt-4">
            <Link to="/tickets">Back to tickets</Link>
          </Button>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="space-y-1">
          <Button variant="ghost" size="sm" asChild className="-ml-3 w-fit text-muted-foreground">
            <Link to="/tickets"><ArrowLeft className="h-4 w-4" /> Back to tickets</Link>
          </Button>
          <div className="flex flex-wrap items-center gap-2">
            <span className="font-mono text-sm font-semibold text-primary">{ticket.number}</span>
            <Badge variant={statusVariant(ticket.status)}>{ticket.status}</Badge>
            <span className={cn("inline-flex rounded-md border px-2 py-0.5 text-xs font-medium", priorityClass(ticket.priority))}>
              {ticket.priority}
            </span>
          </div>
          <h2 className="text-2xl font-bold tracking-tight text-balance">{ticket.subject}</h2>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button variant="outline">Add note</Button>
          <Button variant="outline">Reassign</Button>
          <Button>Update status</Button>
        </div>
      </div>

      <div className="grid gap-6 xl:grid-cols-3">
        <div className="space-y-6 xl:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Description</CardTitle>
              <CardDescription>Submitted by {ticket.requester} · {formatDate(ticket.createdAt)}</CardDescription>
            </CardHeader>
            <CardContent>
              <p className="whitespace-pre-wrap text-sm leading-6 text-foreground/90">{ticket.description}</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2"><MessageSquare className="h-4 w-4" /> Activity & conversation</CardTitle>
              <CardDescription>Public replies and internal notes (mock data)</CardDescription>
            </CardHeader>
            <CardContent className="space-y-5">
              {timeline.length === 0 && <p className="text-sm text-muted-foreground">No activity yet.</p>}
              {timeline.map((ev, idx) => (
                <div key={ev.id} className="relative pl-6">
                  {idx !== timeline.length - 1 && <span className="absolute left-[7px] top-6 h-[calc(100%-0.5rem)] w-px bg-border" />}
                  <span className="absolute left-0 top-1.5 h-4 w-4 rounded-full border-2 border-primary bg-background" />
                  <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
                    <span className="text-sm font-medium">{ev.actor}</span>
                    <span className="text-xs text-muted-foreground">{formatDate(ev.at)}</span>
                    {ev.visibility === "internal" && <Badge variant="warning">Internal</Badge>}
                  </div>
                  <p className="mt-0.5 text-sm font-medium">{ev.title}</p>
                  {ev.body && <p className="mt-1 rounded-md border bg-muted/30 p-3 text-sm text-muted-foreground">{ev.body}</p>}
                </div>
              ))}
              <Separator />
              <div className="space-y-3">
                <p className="text-sm font-medium">Add a reply</p>
                <Textarea placeholder="Write a public reply or internal note..." />
                <div className="flex flex-wrap gap-2">
                  <Button>Post public reply</Button>
                  <Button variant="outline">Save internal note</Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-sm">
              <div className="flex items-start gap-3">
                <User className="mt-0.5 h-4 w-4 text-muted-foreground" />
                <div>
                  <div className="text-muted-foreground">Requester</div>
                  <div className="font-medium">{ticket.requester}</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <Building2 className="mt-0.5 h-4 w-4 text-muted-foreground" />
                <div>
                  <div className="text-muted-foreground">Department / Team</div>
                  <div className="font-medium">{ticket.department} · {ticket.team}</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <User className="mt-0.5 h-4 w-4 text-muted-foreground" />
                <div>
                  <div className="text-muted-foreground">Assignee</div>
                  <div className="font-medium">{ticket.assignee ?? "Unassigned"}</div>
                </div>
              </div>
              <div className="flex items-start gap-3">
                <Tag className="mt-0.5 h-4 w-4 text-muted-foreground" />
                <div>
                  <div className="text-muted-foreground">Category</div>
                  <div className="font-medium">{ticket.category}</div>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2"><Clock className="h-4 w-4" /> SLA</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">State</span>
                <span className={cn("font-semibold", slaClass(ticket.sla))}>{ticket.sla}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Target due</span>
                <span className="font-medium">{formatDate(ticket.dueAt)}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Last updated</span>
                <span className="font-medium">{formatDate(ticket.updatedAt)}</span>
              </div>
              {ticket.sla === "Breached" && (
                <div className="mt-2 flex items-start gap-2 rounded-md border border-red-200 bg-red-50 p-3 text-red-800">
                  <ShieldAlert className="mt-0.5 h-4 w-4 shrink-0" />
                  <p className="text-xs">This ticket has breached its SLA target. Escalate or respond immediately.</p>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
