import { Link } from "react-router-dom"
import { LifeBuoy, ShieldCheck, Clock, Bell } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

export function Login() {
  return (
    <div className="grid min-h-screen lg:grid-cols-[1.05fr_1fr]">
      <div className="hidden flex-col justify-between bg-[hsl(var(--sidebar))] p-10 text-white lg:flex">
        <div className="flex items-center gap-3">
          <div className="grid h-10 w-10 place-items-center rounded-lg bg-teal-600">
            <LifeBuoy className="h-5 w-5" />
          </div>
          <div>
            <div className="text-lg font-semibold">ServiceDesk</div>
            <div className="text-sm text-white/70">Management Information System Office</div>
          </div>
        </div>
        <div>
          <h1 className="max-w-md text-4xl font-bold leading-tight">Track requests, meet SLAs, and keep everyone informed.</h1>
          <ul className="mt-8 space-y-4 text-white/80">
            <li className="flex items-start gap-3"><ShieldCheck className="mt-0.5 h-5 w-5 text-teal-300" /> Secure sign-in with role-based access and audit trails.</li>
            <li className="flex items-start gap-3"><Clock className="mt-0.5 h-5 w-5 text-teal-300" /> Business-hours SLA tracking with breach and risk alerts.</li>
            <li className="flex items-start gap-3"><Bell className="mt-0.5 h-5 w-5 text-teal-300" /> Real-time updates on assignments, replies, and resolutions.</li>
          </ul>
        </div>
        <p className="text-sm text-white/50">Frontend demo built with React, Tailwind CSS, and shadcn/ui components.</p>
      </div>

      <div className="flex items-center justify-center bg-background p-6 sm:p-10">
        <div className="w-full max-w-md space-y-6">
          <div className="flex items-center gap-3 lg:hidden">
            <div className="grid h-10 w-10 place-items-center rounded-lg bg-primary text-primary-foreground"><LifeBuoy className="h-5 w-5" /></div>
            <div>
              <div className="font-semibold">ServiceDesk</div>
              <div className="text-xs text-muted-foreground">MIS Office</div>
            </div>
          </div>
          <Card className="border-0 shadow-lg sm:border sm:shadow-sm">
            <CardHeader className="space-y-1">
              <CardTitle className="text-2xl">Sign in</CardTitle>
              <CardDescription>Use your ServiceDesk account to continue. (UI preview)</CardDescription>
            </CardHeader>
            <CardContent>
              <form className="space-y-4" onSubmit={(e) => e.preventDefault()}>
                <div className="space-y-2">
                  <Label htmlFor="username">Username or email</Label>
                  <Input id="username" autoComplete="username" placeholder="juan.delacruz" />
                </div>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <Label htmlFor="password">Password</Label>
                    <button type="button" className="text-xs font-medium text-primary hover:underline">Forgot password?</button>
                  </div>
                  <Input id="password" type="password" autoComplete="current-password" />
                </div>
                <Button className="w-full" asChild>
                  <Link to="/">Sign in to dashboard</Link>
                </Button>
              </form>
              <p className="mt-4 text-center text-xs text-muted-foreground">
                This screen is a frontend preview. Authentication will be handled by ASP.NET Core Identity in the integrated app.
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
