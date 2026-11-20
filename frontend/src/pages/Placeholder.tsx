import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"

export function Placeholder({ title, description }: { title: string; description: string }) {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold tracking-tight">{title}</h2>
        <p className="text-sm text-muted-foreground">{description}</p>
      </div>
      <Card>
        <CardHeader>
          <CardTitle>Coming soon</CardTitle>
          <CardDescription>This section is planned in later ServiceDesk phases.</CardDescription>
        </CardHeader>
        <CardContent className="text-sm text-muted-foreground">
          The frontend shell and shadcn/ui components are ready to be wired to backend APIs as they become available (teams, reports, settings, roles, SLA configuration, etc.).
        </CardContent>
      </Card>
    </div>
  )
}
