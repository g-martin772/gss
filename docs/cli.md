# gss cli

Install: `dotnet build /t:Install`

## commands
- `gss check` — tools + versions
- `gss new root <name>` — repo root config
- `gss new project <path> -t <type>` — project config
- `gss list types` — project types

## types
Library · Application · Service

## example
```sh
gss new root Acme
gss new project src/Acme.Api -t Service
```

## rules
- one root per repo
- project needs active root
