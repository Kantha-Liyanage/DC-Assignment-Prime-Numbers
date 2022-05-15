# Start Consul

```
consul agent -dev -enable-script-checks
```

## Consul UI URL

http://localhost:8500/ui/

# Application logger service

```
dotnet DC-Assignment-Prime-Numbers.dll logger
```

## Logger UI URL

http://localhost:8181/Index

# numbers file service

```
dotnet DC-Assignment-Prime-Numbers.dll nfs
```

# AppNodes

```
dotnet DC-Assignment-Prime-Numbers.dll appnode "127.0.0.1" 5051
dotnet DC-Assignment-Prime-Numbers.dll appnode "127.0.0.1" 5052
dotnet DC-Assignment-Prime-Numbers.dll appnode "127.0.0.1" 5053
dotnet DC-Assignment-Prime-Numbers.dll appnode "127.0.0.1" 5054
dotnet DC-Assignment-Prime-Numbers.dll appnode "127.0.0.1" 5055
dotnet DC-Assignment-Prime-Numbers.dll appnode "127.0.0.1" 5056
```
