# Localization

Texts use the Unity Localization package: string tables `Artifacts`, `UI` and `Entities` in `Assets/Localization/Tables`, read through the static `Localization` class (`Artifact(id, field, arguments)`, `UI(key)`, `Entity(id)`).

## Keys

| Table | Keys |
|---|---|
| `Artifacts` | `<ID>.Title`, `.Description`, `.Effects`, `.Range`, `.Cooldown`; the ID is the `ArtifactData` asset name |
| `Entities` | The `EntityData` asset names (`Player`, `Kameiko`, `GreatKameiko`...) |
| `UI` | Free keys, read by the code and by the `key` attribute of `LocalizedLabel` and the buttons in UXML; some are built from an enum (`Edition<GameEdition>`, `Status<asset name>`) |

Renaming an artifact or an entity asset requires renaming its keys.

## Arguments

- `Effects`, `Range` and `Cooldown` are smart strings with named values. Each `CombatEffect` names its own through `DescriptionArguments` (`{minDamage}`, `{maxDamage}`, `{minSelfDamage}`, `{armor}`, `{heal}`, `{distance}`...); a status effect names its duration after its asset (`AttackUp` gives `{attackUpTurns}`). `Range` gets `{minRange}`, `{maxRange}`, `{minArea}`, `{maxArea}` and `Cooldown` gets `{value}`.
- `<D>` (damage) and `<B>` (block) tags stay in the texts `Localization` returns; the UI showing one colors them with its `--highlight-color` through `RichText.Highlight` (see [UI components](../features/ui.md#components)).
- UI texts keep positional `{0}` placeholders, formatted by the callers.

## Language

The startup language is the one chosen in the options (`SavedLocaleSelector`, saved by `Localization.SelectLanguage`), otherwise the system language, otherwise English. The options list a button per available locale. The UXML texts and `LocalizedLabel` keys follow by themselves; the HUD rewrites its timeline on `LocalizationSettings.SelectedLocaleChanged`. An `Artifact` reads its texts (`Title`, `Description`, `EffectDescription`, `RangeDescription`, `CooldownDescription`) at each call, so a tooltip or the inventory shows them in the current language.
