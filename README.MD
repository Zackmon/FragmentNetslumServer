# .hack//frägment Server Emulator

[![Build Status](https://travis-ci.com/Zackmon/FragmentServer_netCore.svg?branch=master)](https://travis-ci.com/Zackmon/FragmentServer_netCore)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Zackmon_FragmentServer_netCore&metric=alert_status)](https://sonarcloud.io/dashboard?id=Zackmon_FragmentServer_netCore)

A private Lobby Server Emulator for the PS2 game .hack//frägment

# Credits
Big thanks to Warranty Voider for the original port of the C/C++ [Lobby Server](https://github.com/Lord-Ptolemy/lobbyemu).
Lobby Server in action: https://www.youtube.com/watch?v=VErjuUM4AQM (Credit to Warranty Voider for the video)

# Setup
The Lobby Server requires a MySQL/MariaDB backend. The file `settings.ini.example` located in the root of this repository should be copied and renamed to `settings.ini`, then modified with the connection details for the database.

## Database Setup
Once your configuration file has been adjusted, you will need to run `FragmentServerWV_Core/SQL/DB.SQL` to lay out the correct schema objects.

# Web Interface
`FragmentServerWV_Console` includes a reference to `FragmentServerWV_WebAPI` and fires it up, attempting to expose all internal services to the WebAPI. For the time being, there is no proper UI but rather API calls. In the future, I hope to include a barebones Web UI that would allow for an Administrator to remotely control the system.

# Notes
There are plenty of things I'm considering doing with the code base. These are listed in no particular order:
- Some additional refactoring / abstractions to properly handle Clients versus Area Servers
- Consider a fully functional Web UI (complete with security abstractions)
- Provide a consistent, fully documented, read only API
- Figure out why Guild Chat doesn't work