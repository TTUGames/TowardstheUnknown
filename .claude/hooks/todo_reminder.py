"""PostToolUse hook on Bash: after a `git commit`, reminds Claude to open a GitHub issue for what the work spotted and left
for later (a path compiled but never played, a bug seen on the way, a follow-up), since no script can tell what was spotted,
and to close the issues the commit does.

Reads the hook input (JSON) on stdin and prints the reminder as additional context; never blocks.
"""
import json
import re
import sys

data = json.load(sys.stdin)
if not re.search(r'\bgit\s+commit\b', data.get('tool_input', {}).get('command', '')):
    sys.exit(0)
reminder = ('Commit done. Close the GitHub issues it does (`gh issue close <n> --comment` with the commit). Was anything '
            'spotted and left for later in this work: a path compiled but not played, a bug seen on the way, a follow-up, '
            'a value to tune by ear or eye? Open a GitHub issue for each one (`gh issue create`, in French, with its area '
            'label); nothing to add, move on.')
print(json.dumps({'hookSpecificOutput': {'hookEventName': 'PostToolUse', 'additionalContext': reminder}}))
