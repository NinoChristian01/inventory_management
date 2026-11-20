import { NavLink, Outlet, useLocation } from "react-router-dom"
import {
  LayoutDashboard,
  Ticket,
  PlusCircle,
  Users,
  BarChart3,
  Settings,
  Menu,
  Bell,
  Search,
  LogOut,
  LifeBuoy,
} from "lucide-react"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { useState } from "react"

const nav = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard },
  { to: "/tickets", label: "Tickets", icon: Ticket },
  { to: "/tickets/new", label: "New ticket", icon: PlusCircle },
  { to: "/teams", label: "Teams", icon: Users },
  { to: "/reports", label: "Reports", icon: BarChart3 },
  { to: "/settings", label: "Settings", icon: Settings },
]

function titleFor(pathname: string) {
  if (pathname === "/") return "Dashboard"
  if (pathname === "/tickets") return "Tickets"
  if (pathname === "/tickets/new") return "New ticket"
  if (pathname.startsWith("/tickets/")) return "Ticket details"
  if (pathname === "/teams") return "Teams"
  if (pathname === "/reports") return "Reports"
  if (pathname === "/settings") return "Settings"
  return "ServiceDesk"
}

export function AppLayout() {
  const [open, setOpen] = useState(false)
  const location = useLocation()
  const title = titleFor(location.pathname)

  return (
    <div className="min-h-screen bg-background">
      {open && (
        <div className="fixed inset-0 z-30 bg-black/40 lg:hidden" onClick={() => setOpen(false)} aria-hidden />
      )}
      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-40 flex w-64 flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground transition-transform lg:translate-x-0",
          open ? "translate-x-0" : "-translate-x-full"
        )}
      >
        <div className="flex h-16 items-center gap-3 border-b border-sidebar-border px-5">
          <div className="grid h-9 w-9 place-items-center rounded-lg bg-teal-600 text-white">
            <LifeBuoy className="h-5 w-5" />
          </div>
          <div className="leading-tight">
            <div className="font-semibold text-white">ServiceDesk</div>
            <div className="text-xs text-sidebar-foreground/70">MIS Office</div>
          </div>
        </div>
        <nav className="flex-1 space-y-1 p-3">
          <div className="px-3 pb-2 pt-2 text-xs font-semibold uppercase tracking-wider text-sidebar-foreground/50">
            Workspace
          </div>
          {nav.map((item) => {
            const Icon = item.icon
            return (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === "/"}
                onClick={() => setOpen(false)}
                className={({ isActive }) =>
                  cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                    isActive
                      ? "bg-sidebar-accent text-teal-200"
                      : "text-sidebar-foreground hover:bg-white/5 hover:text-white"
                  )
                }
              >
                <Icon className="h-4 w-4" />
                {item.label}
              </NavLink>
            )
          })}
        </nav>
        <div className="border-t border-sidebar-border p-3">
          <div className="flex items-center gap-3 rounded-lg px-3 py-2">
            <div className="grid h-8 w-8 place-items-center rounded-full bg-teal-700 text-xs font-semibold text-white">
              JD
            </div>
            <div className="min-w-0 flex-1">
              <div className="truncate text-sm font-medium text-white">Juan Dela Cruz</div>
              <div className="truncate text-xs text-sidebar-foreground/60">Agent · MIS</div>
            </div>
          </div>
          <NavLink
            to="/login"
            className="mt-1 flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-sidebar-foreground hover:bg-white/5 hover:text-white"
          >
            <LogOut className="h-4 w-4" /> Sign out
          </NavLink>
        </div>
      </aside>

      <div className="lg:pl-64">
        <header className="sticky top-0 z-20 flex h-16 items-center gap-3 border-b bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/80 sm:px-6">
          <Button variant="ghost" size="icon" className="lg:hidden" onClick={() => setOpen(true)} aria-label="Open menu">
            <Menu className="h-5 w-5" />
          </Button>
          <div className="flex-1">
            <h1 className="text-lg font-semibold tracking-tight">{title}</h1>
          </div>
          <div className="hidden w-full max-w-xs items-center md:flex">
            <div className="relative w-full">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input className="h-9 pl-9" placeholder="Search tickets..." />
            </div>
          </div>
          <Button variant="ghost" size="icon" aria-label="Notifications" className="relative">
            <Bell className="h-5 w-5" />
            <span className="absolute right-2 top-2 h-2 w-2 rounded-full bg-destructive" />
          </Button>
        </header>
        <main className="p-4 sm:p-6 lg:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
