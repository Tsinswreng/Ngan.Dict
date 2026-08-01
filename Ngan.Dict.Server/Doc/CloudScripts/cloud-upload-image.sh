#!/bin/sh
set -eu

SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
. "$SCRIPT_DIR/common.sh"

IMAGE_TAR="${1:-/tmp/ngan.dict-server-aot-slim.tar.gz}"

mk_askpass
trap cleanup_askpass EXIT

run_ssh "mkdir -p /opt/ngan.dict-server/upload"
run_scp "$IMAGE_TAR" "$NGAN.DICT_USER@$NGAN.DICT_HOST:/opt/ngan.dict-server/upload/"
