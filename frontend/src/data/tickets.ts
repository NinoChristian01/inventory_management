export type TicketStatus =
  | "New"
  | "Open"
  | "In Progress"
  | "Pending Requester"
  | "Pending Third Party"
  | "On Hold"
  | "Reopened"
  | "Resolved"
  | "Closed"
  | "Canceled"

export type Priority = "Low" | "Medium" | "High" | "Critical"
export type SlaState = "On track" | "At risk" | "Breached" | "Paused" | "Met"

export interface Ticket {
  id: number
  number: string
  subject: string
  description: string
  requester: string
  department: string
  team: string
  assignee: string | null
  status: TicketStatus
  priority: Priority
  category: string
  createdAt: string
  updatedAt: string
  dueAt: string | null
  sla: SlaState
}

export interface TimelineEvent {
  id: number
  at: string
  actor: string
  kind: "status" | "comment" | "assignment" | "system"
  title: string
  body?: string
  visibility?: "public" | "internal"
}

export const tickets: Ticket[] = [
  {
    id: 1124,
    number: "MIS-26K013",
    subject: "Cannot access payroll portal from office workstation",
    description:
      "User reports repeated sign-in failures on the payroll portal from workstation FIN-WS-044. Browser cache cleared and password reset attempted; issue persists since 08:30. Needs verification of account lock status and network access.",
    requester: "Maria Santos",
    department: "Finance",
    team: "MIS",
    assignee: "Juan Dela Cruz",
    status: "In Progress",
    priority: "High",
    category: "Account & Access",
    createdAt: "2026-11-20T00:42:00Z",
    updatedAt: "2026-11-20T02:10:00Z",
    dueAt: "2026-11-20T08:42:00Z",
    sla: "At risk",
  },
  {
    id: 1123,
    number: "MIS-26K012",
    subject: "Printer queue stuck on 3rd floor",
    description: "Shared printer PRN-3F-02 is not releasing jobs. Multiple users affected.",
    requester: "Carlo Reyes",
    department: "HR",
    team: "MIS",
    assignee: "Ana Lim",
    status: "Resolved",
    priority: "Medium",
    category: "Hardware",
    createdAt: "2026-11-19T06:15:00Z",
    updatedAt: "2026-11-19T09:40:00Z",
    dueAt: "2026-11-21T06:15:00Z",
    sla: "Met",
  },
  {
    id: 1122,
    number: "MIS-26K011",
    subject: "Request for new email distribution list",
    description: "Please create a distribution list for the Q4 audit committee with 8 members.",
    requester: "Liza Gomez",
    department: "Audit",
    team: "MIS",
    assignee: null,
    status: "New",
    priority: "Low",
    category: "Service Request",
    createdAt: "2026-11-19T03:05:00Z",
    updatedAt: "2026-11-19T03:05:00Z",
    dueAt: "2026-11-26T03:05:00Z",
    sla: "On track",
  },
  {
    id: 1121,
    number: "MIS-26K010",
    subject: "VPN disconnects every 10 minutes (remote)",
    description: "Remote user experiencing frequent VPN drops while connected from home network.",
    requester: "Paolo Tan",
    department: "Planning",
    team: "MIS",
    assignee: "Juan Dela Cruz",
    status: "Pending Requester",
    priority: "Medium",
    category: "Network",
    createdAt: "2026-11-18T01:20:00Z",
    updatedAt: "2026-11-19T02:00:00Z",
    dueAt: "2026-11-20T01:20:00Z",
    sla: "Paused",
  },
  {
    id: 1120,
    number: "MIS-26K009",
    subject: "Core switch overheating alert",
    description: "Monitoring reported repeated temperature warnings on core switch SW-CORE-01.",
    requester: "System Monitor",
    department: "MIS",
    team: "MIS",
    assignee: "Ana Lim",
    status: "Open",
    priority: "Critical",
    category: "Infrastructure",
    createdAt: "2026-11-20T01:55:00Z",
    updatedAt: "2026-11-20T02:05:00Z",
    dueAt: "2026-11-20T02:25:00Z",
    sla: "Breached",
  },
  {
    id: 1119,
    number: "MIS-26K008",
    subject: "Install approved design software",
    description: "Install licensed design suite on workstation for new hire.",
    requester: "Nina Cruz",
    department: "Communications",
    team: "MIS",
    assignee: "Mark Villanueva",
    status: "Closed",
    priority: "Low",
    category: "Software",
    createdAt: "2026-11-17T04:00:00Z",
    updatedAt: "2026-11-18T08:00:00Z",
    dueAt: "2026-11-24T04:00:00Z",
    sla: "Met",
  },
]

export const ticketTimeline: Record<number, TimelineEvent[]> = {
  1124: [
    { id: 1, at: "2026-11-20T00:42:00Z", actor: "Maria Santos", kind: "system", title: "Ticket created" },
    { id: 2, at: "2026-11-20T00:50:00Z", actor: "Dispatch", kind: "assignment", title: "Assigned to Juan Dela Cruz (MIS)" },
    { id: 3, at: "2026-11-20T01:05:00Z", actor: "Juan Dela Cruz", kind: "status", title: "Status changed to In Progress" },
    {
      id: 4,
      at: "2026-11-20T01:20:00Z",
      actor: "Juan Dela Cruz",
      kind: "comment",
      visibility: "public",
      title: "Replied to requester",
      body: "Hi Maria, I’m checking your account status and recent sign-in logs now. I’ll update you shortly.",
    },
    {
      id: 5,
      at: "2026-11-20T02:10:00Z",
      actor: "Juan Dela Cruz",
      kind: "comment",
      visibility: "internal",
      title: "Internal note",
      body: "No AD lockout found. Suspect conditional access policy on FIN subnet; coordinating with network.",
    },
  ],
}

export function getTicketById(id: number) {
  return tickets.find((t) => t.id === id)
}

export function getTicketByNumber(num: string) {
  return tickets.find((t) => t.number === num)
}
