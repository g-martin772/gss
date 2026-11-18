# versions

Policy: **latest stable, always.**

- sdk: `global.json` (11.0.100)
- tfm: `net11.0`
- packages: `Directory.Packages.props` only
- prerelease: only if no stable exists for current major · note why

## check
```sh
dotnet list GSS.slnx package --outdated
dotnet list GSS.slnx package --vulnerable
gss check
```

## update
1. bump in `Directory.Packages.props`
2. `dotnet build`
3. commit: `deps: <name>`
