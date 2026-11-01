NPOI Core 2.8.1, Apache-2.0 source, self-compiled without the official NuGet distribution.
Source: https://github.com/nissl-lab/npoi/tree/ab1aa0f7d9aaea06159eeb0311b7b74e752d819a

Rebuild using .NET 10: clone this commit, sparse-checkout main, then run:
```
dotnet build main/NPOI.Core.csproj -c Debug -p:TargetFrameworks=net10.0 -p:TreatWarningsAsErrors=false -p:EnableNETAnalyzers=false
```
Package contains unmodified solution/Debug/net10.0/NPOI.Core.dll and LICENSE. Runtime dependencies are declared in the package metadata. No official NPOI NuGet binaries or OSMF build targets are included.
