#!/usr/bin/env bash
# прогон всех текстов из tests/samples (нужны curl и jq)
BASE=http://localhost:5080
for f in "$(dirname "$0")"/samples/*.txt; do
  echo "=== $(basename "$f") ==="
  jq -Rs '{text: .}' "$f" | curl -s -o /dev/null -w "HTTP %{http_code}\n" \
       -H "Content-Type: application/json" --data-binary @- "$BASE/api/extract"
  curl -s "$BASE/api/glossary" | jq -r '.[] | "  \(.Name // .name) — \(.Definition // .definition)"'
done
