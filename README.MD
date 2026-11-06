# TP Sistemas Operativos - Productor/Consumidor

Sistema de ingesta de telemetría aeronáutica para el TP de productor/consumidor.
Usa `Thread`, `SemaphoreSlim`, `lock` e `Interlocked`, sin tareas ni colecciones
concurrentes.

## Requisitos
- .NET SDK 8.0 (o la versión indicada en `TargetFramework` del `.csproj`)
- Git

## Instalar .NET

### WSL / Ubuntu
```bash
sudo apt update
sudo apt install -y dotnet-sdk-8.0
```
Si apt no encuentra el paquete, seguir la guía de Microsoft para la distribución
instalada: https://learn.microsoft.com/dotnet/core/install/linux

### macOS
Descargar el instalador desde https://dotnet.microsoft.com/download
(elegir el de macOS Arm64 en Macs con chip M1/M2/M3).

### Windows
Descargar el instalador desde https://dotnet.microsoft.com/download

### Verificar
```bash
dotnet --version
```

## Ejecutar
```bash
git clone <URL_DEL_REPO>
cd tp-sistemas-operativos
dotnet run
```

El programa crea un productor por zona y cada productor realiza diez consultas
a la API, esperando cinco segundos entre consultas.

`dotnet run` restaura dependencias, compila y ejecuta.

## Problemas comunes
- **"Couldn't find a project to run"**: no estás dentro de la carpeta del repo
  (tiene que verse el archivo `.csproj` con `ls`).
- **"framework not found"**: la versión del SDK no coincide con el
  `TargetFramework` del `.csproj`.