#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "
set -eu
mkdir -p /opt/ngan.dict-server/deploy /opt/ngan.dict-server/config
if [ -f /opt/ngan.dict-server/upload/docker-compose.aot-slim.yml ]; then
  mv /opt/ngan.dict-server/upload/docker-compose.aot-slim.yml /opt/ngan.dict-server/deploy/
fi
if [ -f /opt/ngan.dict-server/upload/Ngan.Dict.Server.cloud.docker.jsonc ]; then
  mv /opt/ngan.dict-server/upload/Ngan.Dict.Server.cloud.docker.jsonc /opt/ngan.dict-server/config/
fi
docker load -i /opt/ngan.dict-server/upload/ngan.dict-server-aot-slim.tar.gz
cd /opt/ngan.dict-server/deploy
docker compose -f docker-compose.aot-slim.yml up -d postgres redis
docker compose -f docker-compose.aot-slim.yml run --rm web migrate Ngan.Dict.Server.cloud.docker.jsonc
docker compose -f docker-compose.aot-slim.yml up -d web
docker compose -f docker-compose.aot-slim.yml ps
"
