import { useState } from "react"
import { Link } from "react-router-dom"
import { ArrowLeft } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"

export function NewTicket() {
  const [submitted, setSubmitted] = useState(false)

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <div className="space-y-1">
        <Button variant="ghost" size="sm" asChild className="-ml-3 w-fit text-muted-foreground">
          <Link to="/tickets"><ArrowLeft className="h-4 w-4" /> Back to tickets</Link>
        </Button>
        <h2 className="text-2xl font-bold tracking-tight">Create a new ticket</h2>
        <p className="text-sm text-muted-foreground">Describe the issue or request. MIS will triage and respond based on priority and SLA.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Ticket details</CardTitle>
          <CardDescription>Fields marked with * are required. This demo form uses mock submission.</CardDescription>
        </CardHeader>
        <CardContent>
          {submitted ? (
            <div className="rounded-lg border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-900">
              <p className="font-medium">Ticket submitted (demo)</p>
              <p className="mt-1 text-emerald-800">In a connected environment this would create a ticket via the ServiceDesk API and return a number like MIS-26K014.</p>
              <div className="mt-4 flex gap-2">
                <Button asChild><Link to="/tickets">View tickets</Link></Button>
                <Button variant="outline" onClick={() => setSubmitted(false)}>Create another</Button>
              </div>
            </div>
          ) : (
            <form
              className="space-y-5"
              onSubmit={(e) => {
                e.preventDefault()
                setSubmitted(true)
              }}
            >
              <div className="space-y-2">
                <Label htmlFor="subject">Subject *</Label>
                <Input id="subject" required placeholder="Brief summary of the issue or request" />
              </div>
              <div className="grid gap-5 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="category">Category *</Label>
                  <select id="category" required className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
                    <option value="">Select a category</option>
                    <option>Account & Access</option>
                    <option>Hardware</option>
                    <option>Software</option>
                    <option>Network</option>
                    <option>Infrastructure</option>
                    <option>Service Request</option>
                  </select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="priority">Priority *</Label>
                  <select id="priority" required defaultValue="Medium" className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
                    <option>Low</option>
                    <option>Medium</option>
                    <option>High</option>
                  </select>
                  <p className="text-xs text-muted-foreground">Requesters can select Low–High. Critical is set by agents.</p>
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="description">Description *</Label>
                <Textarea id="description" required placeholder="What happened, when it started, affected system/location, and any steps already tried..." className="min-h-[160px]" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="attachment">Attachment (optional)</Label>
                <Input id="attachment" type="file" className="cursor-pointer" />
                <p className="text-xs text-muted-foreground">Allowed in production: common docs/images up to 10 MB (policy enforced by backend).</p>
              </div>
              <div className="flex flex-wrap gap-2 pt-2">
                <Button type="submit">Submit ticket</Button>
                <Button type="button" variant="outline" asChild>
                  <Link to="/tickets">Cancel</Link>
                </Button>
              </div>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
