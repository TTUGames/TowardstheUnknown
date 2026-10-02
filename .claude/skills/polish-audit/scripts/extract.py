"""Saves the JSON array an audit agent returned into <work>/ideas/<agentId>.json.
   python extract.py <work> <agentId> [...]
Reads the agent's transcript (~/.claude/projects/*/*/subagents/agent-<id>.jsonl), its last message holding a
JSON array, so that the ideas never pass through the context. The output file the Agent tool names is empty."""
import glob, json, os, sys

work, ids = sys.argv[1], sys.argv[2:]
os.makedirs(os.path.join(work, "ideas"), exist_ok=True)
for aid in ids:
    paths = glob.glob(os.path.expanduser(f"~/.claude/projects/*/*/subagents/agent-{aid}.jsonl"))
    if not paths:
        sys.exit(f"no transcript for agent {aid}")
    last = None
    for line in open(max(paths, key=os.path.getmtime), encoding="utf-8"):
        try:
            message = json.loads(line).get("message") or {}
        except ValueError:
            continue
        if message.get("role") == "assistant" and isinstance(message.get("content"), list):
            text = "".join(c.get("text", "") for c in message["content"] if c.get("type") == "text")
            if "[" in text and "]" in text:
                last = text
    if last is None:
        sys.exit(f"agent {aid} returned no JSON array")
    ideas = json.loads(last[last.index("["):last.rindex("]") + 1])
    with open(os.path.join(work, "ideas", aid + ".json"), "w", encoding="utf-8") as f:
        json.dump(ideas, f, ensure_ascii=False, indent=0)
    print(aid, len(ideas))
