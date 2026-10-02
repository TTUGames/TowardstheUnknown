---
name: polish-audit
description: "Audits the whole game for polish ideas and builds an HTML page of them to pick from: six read-only agents comb the code, data, string tables, rooms, sounds and editions by area, screenshots are taken in Play mode, the duplicates are merged, and the page (D:/Downloads/TTU_Polish) shows each idea as a card with a checkbox, filters, notes and a button copying the checked ones; the user pastes them back to add to docs/todo.md. Use when the user says « fais un audit polish », « analyse tout le jeu », « plein d'idées de polish », « refais le document des idées », « relance l'audit », « polish audit »."
---

# polish-audit

A big menu of ideas the user picks from, not a plan: many concrete ideas (about 270 on 2026-10-02), each grounded in a file, an asset or a screenshot, each with its Classic answer. Gameplay ideas stay few and respect the original design. The user would rather discard ideas than lack them.

## Steps

1. **Work folder**: `W=<scratchpad>/polish` (outside the repo). The page goes to `D:/Downloads/TTU_Polish` unless the user names another place.
2. **Agents**: launch the six agents of [prompts.md](prompts.md) in one message, in the background. They are read-only and keep off the editor. While they run, take the screenshots.
3. **Screenshots** (the editor: `EDITOR?` to the peer sessions first, see the shared editor protocol): `bash .claude/skills/polish-audit/scripts/shots.sh $W` (about 3 minutes) fills `$W/shots` and `$W/shots.json`. Look at each screenshot and write your own observations in `$W/ideas/00_observations.json`, with area `"Observé en jeu"` and a `"shot"` field (`combat_tooltip.jpg`) that puts the thumbnail on the card. Check that an observation is not already handled in the code before writing it (e.g. the exit portals already have a treasure look). To film something else, add a `shot` line to `shots.sh`.
4. **Ideas**: as each agent finishes, `python .claude/skills/polish-audit/scripts/extract.py $W <agentId>` copies its JSON from the transcript into `$W/ideas`, without passing it through the context.
5. **Duplicates**: `python .../build.py $W list` lists every idea with its key (`a1c7:12`). The agents overlap a lot (statuses, resolution, remapping, results screen, first launch, English texts). Keep the most precise idea of each group and write the others' keys to `$W/drop.json` (a JSON array). Fix a wrong claim in the idea's JSON instead of dropping it. To give an agent idea a thumbnail, add `"shot"` to it.
6. **Page**: `python .../build.py $W D:/Downloads/TTU_Polish --hero BossRoom01.jpg` writes `TTU_Polish.html` and `captures/`. Preview through `python -m http.server` in the folder (Playwright blocks `file:`), then stop the server. The page saves its checks in the browser under a key dated by the audit, so a new audit starts empty.
7. **Hand over**: the path, the number of ideas per area, a few notable bugs. Commit nothing.

When the user pastes the selection back (`# Polish Towards the Unknown : N idées retenues…`), add each idea to `docs/todo.md` under its area, with its identifier and the user's `NOTE`, in the backlog's style.

## The page

`scripts/template.html` is one self-contained page (Google Fonts, no library): a hero with the counts, the screenshots in a gallery (click to enlarge, a badge counts the linked ideas), a sticky toolbar (areas, search, "Filtres" for type, effort and edition, "Cochées seulement"), the cards by area (click to check; "Sources" shows the evidence, "Note" a comment that also checks the card; "Tout cocher" per area) and a bar at the bottom (count, "Cocher l'affiché", "Tout décocher", "Copier la sélection"). The copied text is Markdown grouped by area. `build.py` fills `__IDEAS__`, `__SHOTS__`, `__AREAS__`, `__DATE__`, `__STAMP__`, `__HERO__`, `__BRANCH__`. The areas and their identifier prefixes (`CBT-07`) are in `build.py`.
