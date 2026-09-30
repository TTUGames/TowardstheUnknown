"""Calls the Wwise Authoring API of the open Wwise (HTTP, port 8090; enabled in its user preferences).

    python waapi.py <uri> ['<args json>' ['<options json>']]
    python waapi.py get '<waql>' [field ...]

    python waapi.py ak.wwise.core.getProjectInfo
    python waapi.py get '$ from type Event where name : "RockFall"' id name path
    python waapi.py ak.wwise.core.object.setProperty '{"object": "{GUID}", "property": "TrimBegin", "value": 0.5}'

Importable too: call(uri, args, options) and get(waql, fields). Wrap several edits between ak.wwise.core.undo.beginGroup
and endGroup, save with ak.wwise.core.project.save, generate the banks with ak.wwise.core.soundbank.generate.
"""
import json
import sys
import urllib.error
import urllib.request

URL = "http://127.0.0.1:8090/waapi"


def call(uri, args=None, options=None):
    body = json.dumps({"uri": uri, "args": args or {}, "options": options or {}}).encode()
    request = urllib.request.Request(URL, data=body, headers={"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(request, timeout=300) as response:
            return json.loads(response.read().decode() or "{}")
    except urllib.error.HTTPError as error:
        raise SystemExit(f"{uri}: {error.read().decode()}")
    except urllib.error.URLError as error:
        raise SystemExit(f"Wwise is not reachable on {URL} (open the project in Wwise, WAAPI enabled): {error.reason}")


def get(waql, fields=("id", "name", "type", "path")):
    return call("ak.wwise.core.object.get", {"waql": waql}, {"return": list(fields)})["return"]


if __name__ == "__main__":
    if len(sys.argv) < 2:
        raise SystemExit(__doc__)
    if sys.argv[1] == "get":
        result = get(sys.argv[2], sys.argv[3:] or ("id", "name", "type", "path"))
    else:
        result = call(sys.argv[1], *(json.loads(argument) for argument in sys.argv[2:4]))
    print(json.dumps(result, indent=2, ensure_ascii=False))
