---
name: docs-sync
description: "Brings docs/ and CLAUDE.md back in step with the code: maps the changed files to the docs describing them, finds the names and paths the docs mention that no longer exist, and updates those docs from the real code. Use before committing a change to the architecture, a feature, a data asset type, the tooling or the workflow, when the commit hook reports docs to review, or when the user says « mets à jour la doc », « synchronise la doc », « la doc est à jour ? », « update the docs »."
---

# docs-sync

`docs/` describes how the game works (`docs/README.md` is the index) and `CLAUDE.md` holds the rules and points to it. A change to what they describe updates them in the same commit.

## Steps

1. List the docs concerned:
   ```bash
   D=.claude/skills/docs-sync/scripts/doc_check.py
   python $D impacted          # working tree; python $D impacted <base> for commits since <base>; python $D staged for the index
   python $D stale             # names and paths between backticks that exist nowhere in the project
   ```
2. For each doc `TO REVIEW`, read the diff of the listed files (`git diff`) and the doc, then update the doc where it no longer matches: renamed or removed classes, fields and events, new components or assets, changed flow, new rules. Describe the behavior of the code as it is now, never as planned.
3. A new feature or subsystem gets its section, or a new doc listed in `docs/README.md` and in the table of `CLAUDE.md`. A new rule that applies everywhere (a pitfall, a convention) goes in `docs/tech/conventions.md`, and in the Rules of `CLAUDE.md` if breaking it breaks the game.
4. `python $D stale` reports nothing. An external name reported by mistake (a Unity or package API) goes in `.claude/skills/docs-sync/known-names.txt`.
5. Nothing to change in a listed doc (a pure bug fix, a value tweak): say so, and move on.

## Writing rules

- English, in the style of the existing docs: short paragraphs, tables for lists of fields, events or commands, names between backticks, links between docs.
- Check every name, path and value against the code or the assets before writing it.
- Document the non-obvious (why, pitfalls, order of events), not what reading the code gives at once.
- Keep `CLAUDE.md` short: rules and links, the details go to `docs/`.
