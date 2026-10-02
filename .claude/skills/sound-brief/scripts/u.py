"""python u.py <file.cs> <Class.Method> [json args...]: runs a script in the editor and prints its result"""
import json, subprocess, sys
cmd = ['unity', '--json', 'command', 'run_script', '--file', sys.argv[1], '--entry', sys.argv[2]]
if len(sys.argv) > 3:
    cmd += ['--args', json.dumps([json.loads(a) for a in sys.argv[3:]])]
out = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8').stdout
try:
    d = json.loads(out)
    r = (d.get('data') or {}).get('result')
    if isinstance(r, dict) and r.get('success') is False:
        print('FAILED', r.get('error'), [x['message'] for x in r.get('diagnostics', [])][:5])
    else:
        print(r.get('result') if isinstance(r, dict) else (r if r is not None else d.get('errors')))
except Exception:
    print(out)
