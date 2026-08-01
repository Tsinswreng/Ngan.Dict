#!/usr/bin/env sh
set -eu

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CFG_PATH="${1:-$SCRIPT_DIR/Ngan.Dict.Server.test.jsonc}"
DOTNET_CMD="${DOTNET_CMD:-dotnet}"
RID="${RID:-win-x64}"

cd "$SCRIPT_DIR"
"$DOTNET_CMD" publish "Ngan.Dict.Server.Test.csproj" -c Release -r "$RID" -p:RunAotTest=true
"./bin/Release/net10.0/$RID/publish/Ngan.Dict.Server.Test.exe" "$CFG_PATH"
