"""Sets an ability's timing values in its asset file (Data/Artifacts or Data/EnemyPatterns), with a small diff.

    python timing.py <Ability> [field=value ...]
    python timing.py SlashAttack duration=1 impactDelay=0.45 timing.enabled=1 timing.swingStart=0.31 timing.swingSpeed=1.8

Top-level fields (duration, vfxDuration, impactDelay, animationSpeed, legs) and the fields of timing (enabled, swingStart,
windupSpeed, windupHold, strike, swingSpeed, strikeHold, recoverySpeed). Without a value, prints the current ones.
Reimport the asset afterwards (probe.sh Reload) for the editor to read it.
"""
import os
import re
import sys

ROOT = os.path.join(os.path.dirname(__file__), "..", "..", "..", "..")
TIMING = ["enabled", "swingStart", "windupSpeed", "windupHold", "strike", "swingSpeed", "strikeHold", "recoverySpeed"]
DEFAULTS = {"enabled": "0", "swingStart": "0", "windupSpeed": "1", "windupHold": "0", "strike": "0", "swingSpeed": "1.6",
            "strikeHold": "0.1", "recoverySpeed": "1"}


def path_of(name):
    for folder in ("Artifacts", "EnemyPatterns"):
        path = os.path.join(ROOT, "Assets", "Data", folder, name + ".asset")
        if os.path.exists(path):
            return path
    sys.exit("no ability " + name)


def main():
    name, pairs = sys.argv[1], sys.argv[2:]
    path = path_of(name)
    text = open(path, encoding="utf-8", newline="").read()
    nl = "\r\n" if "\r\n" in text else "\n"
    if not pairs:
        for line in text.splitlines():
            if re.match(r"  (duration|vfxDuration|impactDelay|animationSpeed|legs):", line) or re.match(r"    (" + "|".join(TIMING) + "):", line):
                print(line)
        return
    # The timing block, after impactDelay (written first if missing, 0.5 by default)
    if not re.search(r"\n  impactDelay: ", text):
        text = re.sub(r"(\n  vfxDuration: [^\r\n]*)", lambda m: m.group(1) + nl + "  impactDelay: 0.5", text, count=1)
    if not re.search(r"\n  timing:", text):
        block = nl + "  timing:" + "".join(f"{nl}    {k}: {DEFAULTS[k]}" for k in TIMING)
        text = re.sub(r"(\n  impactDelay: [^\r\n]*)", lambda m: m.group(1) + block, text, count=1)
    for pair in pairs:
        key, value = pair.split("=")
        if key.startswith("timing."):
            field = key[len("timing."):]
            assert field in TIMING, field
            start = text.index("\n  timing:")
            end = start + len("\n  timing:")
            match = re.compile(r"\n    " + field + r": [^\r\n]*").search(text, end)
            text = text[:match.start()] + f"\n    {field}: {value}" + text[match.end():]
        else:
            new, count = re.subn(r"\n  " + key + r": [^\r\n]*", f"\n  {key}: {value}", text, count=1)
            if count == 0:
                # A field the asset has not written yet (legs): after the timing block, as in AbilityData
                new, count = re.subn(r"(\n    recoverySpeed: [^\r\n]*)", lambda m: m.group(1) + f"{nl}  {key}: {value}", text, count=1)
            assert count == 1, key
            text = new
    open(path, "w", encoding="utf-8", newline="").write(text)
    print(name, " ".join(pairs))


main()
