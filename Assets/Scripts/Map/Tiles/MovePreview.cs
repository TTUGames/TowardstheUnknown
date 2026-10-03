using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// While the player aims an artifact that pushes, pulls or dashes, marks the tile each moved entity would stop on
/// (<see cref="Tile.IsMovePreview"/>), as <see cref="MoveTowardsAction.Path"/> moves them: a wall or an entity in the way shows
/// where it stops. The effects in order: a dash moves the player first, so that the push after it starts from where it lands.
/// One for the game, on <c>Gameplay</c> (<see cref="EditionProfile.movePreview"/>)
/// </summary>
public class MovePreview : MonoBehaviour
{
    private readonly List<Tile> marked = new();
    private PlayerAttack attack;

    private void Start()
    {
        attack = GameScene.Player.playerAttack;
        attack.TargetsPreviewed += Show;
    }

    private void OnDestroy()
    {
        if (attack != null) attack.TargetsPreviewed -= Show;
        Hide();
    }

    private void Show(Artifact artifact, IReadOnlyList<EntityStats> targets)
    {
        Hide();
        if (artifact == null || !Edition.Profile.movePreview || targets.Count == 0) return;
        Tile casterTile = attack.Stats.Tile;
        foreach (EntityStats target in targets)
        {
            Tile from = casterTile, targetTile = target.Tile;
            foreach (CombatEffect effect in artifact.Data.effects)
            {
                if (effect is not MoveEffect move || move.distance == 0) continue;
                bool casterMoves = move.moved == EffectTarget.Caster;
                List<Tile> path = MoveTowardsAction.Path(casterMoves ? from : targetTile, casterMoves ? targetTile : from, move.distance);
                Tile landing = path[path.Count - 1];
                if (casterMoves) from = landing;
                else targetTile = landing;
                Mark(landing);
            }
        }
    }

    private void Mark(Tile tile)
    {
        if (tile == null || marked.Contains(tile)) return;
        marked.Add(tile);
        tile.IsMovePreview = true;
    }

    private void Hide()
    {
        foreach (Tile tile in marked)
            if (tile != null) tile.IsMovePreview = false;
        marked.Clear();
    }
}
