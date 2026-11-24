# SyncWave

**English** · [Українська](README.uk.md)

[![CI](https://github.com/y-teitelman/SyncWave/actions/workflows/ci.yml/badge.svg)](https://github.com/y-teitelman/SyncWave/actions/workflows/ci.yml)

**SyncWave** is a self-hosted watch-party service for watching and listening to media together in real time. You share a room link with friends (for example, in Discord), they open it in a regular browser, enter a nickname, and everyone sees and hears the same thing at the same moment: one host controls playback (play, pause, seek), and the room shares a common queue. No installs for participants, and the whole setup runs on free tools.

> **Status:** early development. The domain core and its unit tests are in place; the API, real-time layer and UI are next.

## Tech stack

- **C# / .NET 10** (LTS)
- **xUnit** for unit tests
- **GitHub Actions** for CI (build + tests on every push and pull request)
- Planned: **ASP.NET Core** (file upload and streaming via HTTP Range Requests), **SignalR** (real-time sync), a plain browser client, **Cloudflare Quick Tunnel** / **ngrok** for free public access
- Later, optional: **SQLite / PostgreSQL** + **EF Core**, **Redis**, **Docker**

## What works now

The `SyncWave.Core` library contains the domain model and room logic:

- **Room**: room code, list of participants, current media, playback position and state
- **Host logic**: the first participant becomes the host; when the host leaves, the role passes to the next participant
- **Empty room reset**: when the last participant leaves, playback stops and the current media, position and queue are cleared
- **Shared queue**: the first enqueued item starts playing at once, the rest wait in the queue; `PlayNext` moves through it and stops playback when it is empty
- **Playback control**: `Pause`, `Resume`, and `SeekTo` with bounds checking against the media duration
- **Media types**: audio, video and YouTube

Not yet implemented (see [docs/NEXT_STEPS.md](docs/NEXT_STEPS.md)):

- Managing many rooms and finding a room **by its code** (`IRoomManager`; the code is stored, but there is no room manager yet)
- Restricting playback control to the **host** (right now any caller can pause or seek)

## Project structure

```
SyncWave.sln
├── README.md / README.uk.md  # Project description in English and Ukrainian
├── global.json               # Pins the .NET SDK version (10.0.x)
├── .editorconfig             # Shared code style
├── .github/workflows/ci.yml  # CI: build and test
├── docs/NEXT_STEPS.md        # Learning roadmap with hints
├── SyncWave.Core/            # Domain model and business logic (no web or DB dependencies)
│   ├── Entities/             # Room, Participant, MediaItem
│   └── Enums/                # PlaybackState, MediaType
└── SyncWave.Core.Tests/      # xUnit tests for room logic
```

Planned layers: `SyncWave.Infrastructure` (file storage, external services) and `SyncWave.Web` (ASP.NET Core API, streaming, SignalR hub, web page).

## Getting started

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). Any editor works; the repository recommends VS Code with the C# Dev Kit extension.

```bash
git clone https://github.com/y-teitelman/SyncWave.git
cd SyncWave
dotnet build
dotnet test
```

## Roadmap

**Stage 1: Domain core**
- [x] Domain model: rooms, participants, media items
- [x] Host transfer and shared queue logic
- [x] Unit tests for room behaviour (participants, queue, playback control)
- [x] CI with GitHub Actions
- [ ] `IRoomManager`: create, find, delete and list rooms (in-memory)
- [ ] Host-only playback control

**Stage 2: File streaming**
- [ ] `SyncWave.Infrastructure` and `SyncWave.Web` projects
- [ ] Upload MP3/MP4 files with size validation
- [ ] Stream files in chunks with HTTP Range Requests

**Stage 3: Real-time sync**
- [ ] SignalR hub: play, pause, seek, add track for everyone in the room
- [ ] Simple web page with audio/video player, participant list and queue

**Stage 4: YouTube and public access**
- [ ] YouTube playback by link
- [ ] Free public access via Cloudflare Quick Tunnel or ngrok

**Later, optional**
- [ ] Persistence with SQLite/PostgreSQL and EF Core
- [ ] Docker Compose setup

## Author

**Yehor Teitelman** · [GitHub](https://github.com/y-teitelman)
