"""PreToolUse hook on Bash: blocks a `git commit` whose code or asset changes concern docs that the commit does not update.

Reads the hook input (JSON) on stdin. Exit 2 blocks the command and shows the report to Claude.
Bypass once the docs are checked and need no change: prefix the command with DOCS_REVIEWED=1.
"""
import json
import os
import re
import subprocess
import sys

command = json.load(sys.stdin).get('tool_input', {}).get('command', '')
if not re.search(r'\bgit\s+commit\b', command) or 'DOCS_REVIEWED=1' in command:
    sys.exit(0)

root = subprocess.check_output(['git', 'rev-parse', '--show-toplevel'], text=True).strip()
check = os.path.join(root, '.claude', 'skills', 'docs-sync', 'scripts', 'doc_check.py')
# A command staging files itself (git add ... && git commit, commit -a) is judged on the working tree
mode = 'impacted' if re.search(r'\bgit\s+add\b|\bcommit\b[^&|;]*\s-(a|am|[a-z]*a[a-z]*)\b', command) else 'staged'
result = subprocess.run([sys.executable, check, mode], cwd=root, capture_output=True, text=True, encoding='utf-8')
if result.returncode == 0:
    sys.exit(0)
print('Docs to review before this commit (docs-sync skill, or the docs-keeper agent):\n' + result.stdout
      + '\nUpdate and stage them with the commit. If they need no change, run the commit again prefixed with DOCS_REVIEWED=1.',
      file=sys.stderr)
sys.exit(2)
