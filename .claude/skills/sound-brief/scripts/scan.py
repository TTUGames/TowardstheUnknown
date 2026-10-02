"""Scans the Wwise profiler's history for the voices that started during the last recording.
    python scan.py <window seconds> <out.json>
The recording's start and end are the voices posted on the StudioSync game object (Rec.Start / Rec.Stop): the times written
are seconds from the start marker, the video's zero."""
import http.client
import json
import sys

STEP = 25  # ms
conn = http.client.HTTPConnection('127.0.0.1', 8090, timeout=10)


def call(uri, args=None, options=None):
    body = json.dumps({'uri': uri, 'args': args or {}, 'options': options or {}})
    conn.request('POST', '/waapi', body, {'Content-Type': 'application/json'})
    return json.loads(conn.getresponse().read().decode() or '{}')


end = call('ak.wwise.core.profiler.getCursorTime', {'cursor': 'capture'})['return']
start = end - int(float(sys.argv[1]) * 1000)
first = {}
for t in range(start, end, STEP):
    voices = call('ak.wwise.core.profiler.getVoices', {'time': t}, {'return': ['objectName', 'gameObjectName', 'playingID']}).get('return', [])
    for v in voices:
        key = (v['playingID'], v['objectName'], v['gameObjectName'])
        if key not in first:
            first[key] = t
markers = sorted(t for (pid, name, go), t in first.items() if go == 'StudioSync')
if not markers:
    raise SystemExit('no sync marker in the window')
zero, stop = (markers[-2], markers[-1]) if len(markers) > 1 else (markers[0], end)
events = [{'t': round((t - zero) / 1000, 3), 'sound': name, 'go': go, 'playing': pid}
          for (pid, name, go), t in sorted(first.items(), key=lambda kv: kv[1])
          if go != 'StudioSync' and zero - 50 <= t <= stop + 50 and t > start + STEP]
active = sorted({v['objectName'] for v in call('ak.wwise.core.profiler.getVoices', {'time': zero + 200}, {'return': ['objectName', 'gameObjectName']}).get('return', []) if v['gameObjectName'] != 'StudioSync'})
json.dump({'duration': round((stop - zero) / 1000, 3), 'active': active, 'events': events}, open(sys.argv[2], 'w'), indent=1)
print('active at start:', ', '.join(active))
print(f'markers {len(markers)}, recording {(stop - zero) / 1000:.2f} s, {len(events)} voices started')
for e in events:
    print(f"  {e['t']:7.2f}  {e['sound']}  ({e['go']})")
