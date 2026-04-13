# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**ReqNot** is a home automation monitoring system for a heat recovery ventilator (rekuperator). It monitors device status and sends Firebase push notifications when the device stops working.

## Architecture

Three components:

- **ReqNot.WebApi** — ASP.NET Core 10 minimal API hosted on nginx/Raspberry Pi. Polls the device every 30 minutes at `GET /API/RunFunction?name=CurrentWorkParameters`, sends Firebase push notifications when `Values[0] == 0` (device off). Exposes endpoints for current device status.
- **ReqNot.FakeDevice** — ASP.NET Core 10 minimal API that simulates the real device for local development. Exposes `GET /API/RunFunction?name=CurrentWorkParameters` and a `PUT` toggle endpoint to flip the device state.
- **reqNot** — React Native / Expo mobile app (TypeScript). Shows device status (green on / red off icon), has a "Sprawdź" button to poll the API, and handles Firebase push notifications.

## Device API Contract

Real device is at `http://192.168.8.122`. Response shape:

```json
{"CurrentWorkParametersResult": true, "Values": [1, 300, 22.2, ...]}
```

`Values[0]`: 1 = device on, 0 = device off  
`Values[1]`: max airflow in m³/h  
`Values[2]`: current temperature in °C

## Configuration

- `appsettings.Development.json` in WebApi has `"DeviceAddress": "https://localhost:7057"` (points to FakeDevice)
- `appsettings.Production.json` must be created manually and is gitignored — it should contain the real device address `http://192.168.8.122`
- Both production appsettings files are gitignored

## Commands

### .NET projects (run from project directory or with `-p` flag)

```bash
# Run WebApi
dotnet run --project ReqNot.WebApi

# Run FakeDevice (run alongside WebApi for local dev)
dotnet run --project ReqNot.FakeDevice

# Build all
dotnet build

# Build specific project
dotnet build ReqNot.WebApi
```

FakeDevice runs at `https://localhost:7057` (https) / `http://localhost:5268` (http).  
WebApi runs at `https://localhost:7194` / `http://localhost:5076`.

### Mobile app (from `reqNot/` directory)

```bash
cd reqNot
npm install
npm start           # Expo dev server
npm run android     # Android
npm run ios         # iOS
npm run lint        # ESLint
```

## Local Development Setup

1. Start FakeDevice first: `dotnet run --project ReqNot.FakeDevice`
2. Start WebApi: `dotnet run --project ReqNot.WebApi` — it will use `appsettings.Development.json` which points to FakeDevice
3. Use the FakeDevice PUT toggle endpoint to switch device state for testing

## Production Deployment (Raspberry Pi / nginx)

- Target: nginx reverse proxy on Raspberry Pi, local network
- Must create `appsettings.Production.json` with real device address (gitignored)
- Set `ASPNETCORE_ENVIRONMENT=Production` in the systemd service or nginx config
