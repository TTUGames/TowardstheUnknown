"""Builds the polish audit page from the ideas of <work>/ideas/*.json.
   python build.py <work> list                     every idea with its key (file prefix:index), to pick the duplicates
   python build.py <work> <out> [--hero <file>]    writes <out>/TTU_Polish.html and <out>/captures/*.jpg
<work>/drop.json lists the keys left out (duplicates), <work>/shots.json the captions of <work>/shots/*.png.
The files are read in name order: 00_observations.json (the session's own) comes first."""
import datetime, glob, html, json, os, subprocess, sys

HERE = os.path.dirname(os.path.abspath(__file__))
AREAS = ["Observé en jeu", "Combat", "Entités", "Monde", "Interface", "Inventaire", "Run & progression",
         "Plateformes", "Audio", "Éditions", "Technique"]
PREFIX = {"Observé en jeu": "OBS", "Combat": "CBT", "Entités": "ENT", "Monde": "MON", "Interface": "UI",
          "Inventaire": "INV", "Run & progression": "RUN", "Plateformes": "PLT", "Audio": "AUD",
          "Éditions": "EDI", "Technique": "TEC"}
MONTHS = "janvier février mars avril mai juin juillet août septembre octobre novembre décembre".split()

work = sys.argv[1]
drop_path = os.path.join(work, "drop.json")
drop = set(json.load(open(drop_path, encoding="utf-8"))) if os.path.exists(drop_path) else set()
raw, ideas = [], []
for path in sorted(glob.glob(os.path.join(work, "ideas", "*.json"))):
    name = os.path.splitext(os.path.basename(path))[0]
    for k, idea in enumerate(json.load(open(path, encoding="utf-8"))):
        idea = {a: html.unescape(b) if isinstance(b, str) else b for a, b in idea.items()}
        key = f"{name[:4]}:{k}"
        raw.append((key, idea))
        if key not in drop:
            ideas.append(idea)

if sys.argv[2] == "list":
    sys.stdout.reconfigure(encoding="utf-8")
    for key, idea in raw:
        print(key, "|", idea["area"][:5], "|", idea["title"])
    sys.exit()

count = {}
for idea in ideas:
    if idea["area"] not in PREFIX:
        sys.exit(f"unknown area {idea['area']!r} in {idea['title']!r}")
    count[idea["area"]] = count.get(idea["area"], 0) + 1
    idea["id"] = "%s-%02d" % (PREFIX[idea["area"]], count[idea["area"]])

out = sys.argv[2]
os.makedirs(os.path.join(out, "captures"), exist_ok=True)
from PIL import Image
for png in glob.glob(os.path.join(work, "shots", "*.png")):
    jpg = os.path.splitext(os.path.basename(png))[0] + ".jpg"
    Image.open(png).convert("RGB").save(os.path.join(out, "captures", jpg), quality=88)
shots_path = os.path.join(work, "shots.json")
shots = json.load(open(shots_path, encoding="utf-8")) if os.path.exists(shots_path) else []
hero = sys.argv[sys.argv.index("--hero") + 1] if "--hero" in sys.argv else (shots[0]["file"] if shots else "")

today = datetime.date.today()
date = f"{today.day} {MONTHS[today.month - 1]} {today.year}"
branch = subprocess.run(["git", "branch", "--show-current"], capture_output=True, text=True, cwd=HERE).stdout.strip()
js = lambda x: json.dumps(x, ensure_ascii=False).replace("</", "<" + chr(92) + "/")
page = open(os.path.join(HERE, "template.html"), encoding="utf-8").read()
page = (page.replace("/*__IDEAS__*/[]", js(ideas)).replace("/*__SHOTS__*/[]", js(shots))
        .replace("/*__AREAS__*/[]", js([a for a in AREAS if count.get(a)]))
        .replace("__DATE__", date).replace("__STAMP__", today.isoformat())
        .replace("__HERO__", hero).replace("__BRANCH__", branch))
with open(os.path.join(out, "TTU_Polish.html"), "w", encoding="utf-8") as f:
    f.write(page)
sys.stdout.reconfigure(encoding="utf-8")
print(len(ideas), "ideas", {a: count[a] for a in AREAS if a in count})
