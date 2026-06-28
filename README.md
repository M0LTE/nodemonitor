# nodemonitor
## Deployment

Hosted on a Proxmox LXC (`ham-apps`) as a systemd service.

- **Build:** `dotnet publish nodemonitor.csproj -c Release -r linux-x64 --self-contained -o /opt/nodemonitor`
- **Run:** systemd unit `nodemonitor.service` → `/opt/nodemonitor/nodemonitor --urls http://0.0.0.0:5227`
- **Data:** subscribes to the `mqtt` broker (MQTT, port 1883); no database.
- **Public:** `gb7rdg-live.ukpacketradio.network`, via a Cloudflare tunnel → `localhost:5227`.
