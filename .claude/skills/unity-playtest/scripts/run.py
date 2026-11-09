"""Runs a static method of a C# script in the open editor (unity command script run) and prints its result.
   python3 run.py <file.cs> <Class.Method>                  no arguments
   python3 run.py <file.cs> <Class.Method> '<json array>'   one argument starting with [ : the method's arguments as they are
   python3 run.py <file.cs> <Class.Method> <json> <json>... each argument one JSON value ("\"text\"", 0, true)
On failure prints ERROR: with the error, its details and the compile errors, and exits 1. Every tool script of .claude runs
its probes through it (playtest.sh, room.sh, record_shaders.sh, the attack-inspect and sound-brief scripts)."""
import json, subprocess, sys

if len(sys.argv) < 3:
    sys.exit(__doc__)
cmd = ['unity', '--json', 'command', 'script', 'run', '--file', sys.argv[1], '--entry', sys.argv[2]]
values = sys.argv[3:]
if len(values) == 1 and values[0].lstrip().startswith('['):
    cmd += ['--args', values[0]]
elif values:
    cmd += ['--args', json.dumps([json.loads(v) for v in values])]
out = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8').stdout
try:
    d = json.loads(out)
except ValueError:
    print(out)
    sys.exit(1)
failed = False
r = (d.get('data') or {}).get('result')
if isinstance(r, dict):
    if r.get('success') is False:
        failed = True
        print('ERROR:', r.get('error'), (r.get('errorDetails') or '')[:400],
              [x['message'] for x in r.get('diagnostics', []) if x.get('severity') == 'error'])
    else:
        print(r.get('result'))
elif r is not None:
    print(r)
for e in d.get('errors', []) or []:
    failed = True
    print('ERROR:', e.get('message', e) if isinstance(e, dict) else e)
sys.exit(1 if failed else 0)
