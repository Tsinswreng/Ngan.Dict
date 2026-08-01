#!/bin/sh
set -eu

: "${NGAN.DICT_HOST:?NGAN.DICT_HOST is required}"
: "${NGAN.DICT_USER:?NGAN.DICT_USER is required}"
: "${NGAN.DICT_SSH_PASS:?NGAN.DICT_SSH_PASS is required}"

mk_askpass() {
  PASS_SCRIPT="$(mktemp)"
  cat > "$PASS_SCRIPT" <<EOF
#!/bin/sh
printf '%s\n' '$NGAN.DICT_SSH_PASS'
EOF
  chmod 700 "$PASS_SCRIPT"
  export SSH_ASKPASS="$PASS_SCRIPT"
  export SSH_ASKPASS_REQUIRE=force
  export DISPLAY=:0
}

cleanup_askpass() {
  rm -f "${PASS_SCRIPT:-}"
}

run_ssh() {
  ssh -o StrictHostKeyChecking=no "$NGAN.DICT_USER@$NGAN.DICT_HOST" "$@"
}

run_scp() {
  scp -o StrictHostKeyChecking=no "$@"
}
