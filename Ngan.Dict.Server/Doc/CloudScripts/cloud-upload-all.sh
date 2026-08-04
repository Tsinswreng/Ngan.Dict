#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

IMAGE_TAR="${1:-/tmp/ngan.dict-server-aot-slim.tar.gz}"
COMPOSE_FILE="${2:-/mnt/e/_code/Ngan/Ngan.Dict/Ngan.Dict.Server/Deploy/Cloud/docker-compose.aot-slim.yml}"
CFG_FILE="${3:-/mnt/e/_code/Ngan/Ngan.Dict/Ngan.Dict.Server/ExternalRsrc/Ngan.Dict.Server.cloud.docker.jsonc}"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "mkdir -p /opt/ngan.dict-server/upload"
run_scp \
  "$IMAGE_TAR" \
  "$COMPOSE_FILE" \
  "$CFG_FILE" \
  "$NGAN.DICT_USER@$NGAN.DICT_HOST:/opt/ngan.dict-server/upload/"
