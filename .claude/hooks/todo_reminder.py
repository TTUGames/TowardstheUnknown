"""PostToolUse hook on Bash: after a `git commit`, reminds Claude to write into docs/todo.md what the work spotted and left
for later (a path compiled but never played, a bug seen on the way, a follow-up), since no script can tell what was spotted.

Reads the hook input (JSON) on stdin and prints the reminder as additional context; never blocks.
"""
import json
import re
import subprocess
import sys

data = json.load(sys.stdin)
if not re.search(r'\bgit\s+commit\b', data.get('tool_input', {}).get('command', '')):
    sys.exit(0)
changed = subprocess.run(['git', 'show', '--name-only', '--format=', 'HEAD'], capture_output=True, text=True).stdout.split()
touched = 'docs/todo.md' in changed
reminder = ('Commit done. Was anything spotted and left for later in this work: a path compiled but not played, a bug seen '
            'on the way, a follow-up, a value to tune by ear or eye? ' +
            ('The commit edited docs/todo.md: check that it holds them all. ' if touched else 'The commit did not touch docs/todo.md. ') +
            'Add each one missing as a line of docs/todo.md (in French, in its section) and commit it; nothing to add, move on.')
print(json.dumps({'hookSpecificOutput': {'hookEventName': 'PostToolUse', 'additionalContext': reminder}}))
