using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The pointer on the board, one for the game (on <c>Gameplay</c>): finds the tile and entity under it every frame, raises
/// their hover and the clicks on the tiles of the current room, and lets the UI point the board at an entity (the timeline)
/// </summary>
public class BoardPointer : MonoBehaviour
{
    /// <summary>
    /// Fired when the pointer moves to another tile of the current room, with null when it leaves the tiles
    /// </summary>
    public static event System.Action<Tile> TileHovered;

    /// <summary>
    /// Fired when a selectable tile of the current room is clicked
    /// </summary>
    public static event System.Action<Tile> TileClicked;

    /// <summary>
    /// Fired when a tile of the current room that is not selectable is clicked
    /// </summary>
    public static event System.Action<Tile> UnselectableTileClicked;

    /// <summary>
    /// Fired when the entity on the hovered tile changes (its tile, its model or its timeline item is hovered), with null
    /// when none, and when the same entity goes from being pointed at from the UI to being hovered on the board or back
    /// </summary>
    public static event System.Action<TacticsMove> EntityHovered;

    private static Tile hoveredTile;
    private static TacticsMove hoveredEntity;

    /// <summary>
    /// The tile of the current room under the pointer, null if none (a room left is disabled: its tiles no longer count)
    /// </summary>
    public static Tile HoveredTile => hoveredTile != null && hoveredTile.gameObject.activeInHierarchy ? hoveredTile : null;

    /// <summary>
    /// The entity on <see cref="HoveredTile"/>, null if none; with <see cref="EditionProfile.infoOnModelHover"/>, the entity
    /// whose model is under the pointer first
    /// </summary>
    public static TacticsMove HoveredEntity => hoveredTile != null && !hoveredTile.gameObject.activeInHierarchy ? null : hoveredEntity;

    //The entity pointed at from the UI (the timeline): the board acts as if the pointer were on its tile
    private static TacticsMove pointedEntity;
    //Whether HoveredEntity was pointed at from the UI
    private static bool hoveredFromUI;

    // The pointer hits the tiles and the entities' models: an entity's body or hover box (a trigger child sized to its
    // model) stands for its tile, so that the model hides the tiles behind it as it does on screen
    private static readonly RaycastHit[] pointerHits = new RaycastHit[16];
    private static int pointerMask;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        TileHovered = null;
        TileClicked = null;
        UnselectableTileClicked = null;
        EntityHovered = null;
        hoveredTile = null;
        hoveredEntity = null;
        pointedEntity = null;
        hoveredFromUI = false;
        pointerMask = 0;
    }

    private void OnEnable() => GameInput.Controls.Gameplay.Select.performed += OnSelect;

    private void OnDisable() => GameInput.Controls.Gameplay.Select.performed -= OnSelect;

    /// <summary>
    /// Makes the board act as if the pointer were on the entity's tile, until <see cref="StopPointingAt"/>: hovering and
    /// clicking an entity in the UI works as on its tile
    /// </summary>
    public static void PointAt(TacticsMove entity) => pointedEntity = entity;

    /// <summary>
    /// The board is pointed at an entity from the UI rather than by the pointer: the UI shows the entity's info itself
    /// </summary>
    public static bool IsPointedFromUI => pointedEntity != null;

    public static void StopPointingAt(TacticsMove entity) {
        if (pointedEntity == entity) pointedEntity = null;
    }

    /// <summary>
    /// Updates the hovered tile and entity. Done every frame: the pointer or the entities move, and the hover highlight
    /// must be restored after the tiles are reset
    /// </summary>
    private void Update() {
        //The tile of a room left, disabled, is forgotten without an event, as when its room was disabled
        if (hoveredTile != null && !hoveredTile.gameObject.activeInHierarchy) {
            hoveredTile = null;
            hoveredEntity = null;
        }
        TacticsMove model = null;
        Tile hovered = pointedEntity == null ? FindHovered(out model)
            : GameScene.IsGameplayBlocked ? null : pointedEntity.CurrentTile;
        if (hovered != hoveredTile) {
            //Before the modes paint the tiles for the new one, which may use the old one
            if (hoveredTile != null) hoveredTile.IsTarget = false;
            hoveredTile = hovered;
            TileHovered?.Invoke(hovered);
        }
        //After the modes, which reset the target tiles when the hovered one changes: every paint keeps a target tile's
        if (hovered != null && hovered.isWalkable && !hovered.IsTarget) hovered.IsTarget = true;

        TacticsMove entity = hovered != null ? hovered.GetEntity() : null;
        //The original showed an entity's info on its model, whatever tile is picked behind it
        if (model != null && Edition.Profile.infoOnModelHover) entity = model;
        bool fromUI = IsPointedFromUI;
        if (entity == hoveredEntity && fromUI == hoveredFromUI) return;
        hoveredEntity = entity;
        hoveredFromUI = fromUI;
        EntityHovered?.Invoke(entity);
    }

    private void OnSelect(InputAction.CallbackContext context) {
        Tile tile = HoveredTile;
        if (tile == null) return;
        if (tile.Selection != Tile.SelectionType.NONE) TileClicked?.Invoke(tile);
        else UnselectableTileClicked?.Invoke(tile);
    }

    /// <summary>
    /// Returns the tile under the pointer: the nearest tile it points at, even through an entity's model, so that the
    /// tiles behind a tall model stay easy to pick; the tile of the entity whose model it points at only when no tile is
    /// behind it (not in the original, which picked the tiles only: <see cref="EditionProfile.modelPicking"/>). None over a
    /// UI Toolkit element, or while a menu covers the game, so that nothing reacts to the pointer behind it.
    /// One pick and one raycast give both
    /// </summary>
    /// <param name="model">The living entity whose model is under the pointer, nearer than any tile, whatever tile is
    /// picked: the original showed an enemy's info on its model (<see cref="EditionProfile.infoOnModelHover"/>)</param>
    private static Tile FindHovered(out TacticsMove model) {
        model = null;
        if (GameScene.IsGameplayBlocked || IsPointerOverUI()) return null;
        if (pointerMask == 0) pointerMask = LayerMask.GetMask("Terrain", "Player", "Enemy");
        Ray ray = Camera.main.ScreenPointToRay(GameInput.PointerPosition);
        int count = Physics.RaycastNonAlloc(ray, pointerHits, Mathf.Infinity, pointerMask, QueryTriggerInteraction.Collide);
        System.Array.Sort(pointerHits, 0, count, HitDistance.Instance);
        bool modelPicking = Edition.Profile.modelPicking;
        Tile entityTile = null;
        //A relic's orb picks its tile, as an entity's model does
        float relicDistance = Mathf.Infinity;
        Tile relicTile = modelPicking ? Collectable.FindPointed(ray, out relicDistance) : null;
        for (int i = 0; i < count; i++) {
            if (relicTile != null && pointerHits[i].distance > relicDistance) return entityTile ?? relicTile;
            Collider hit = pointerHits[i].collider;
            if (hit.TryGetComponent(out Tile tile)) return tile;
            TacticsMove entity = hit.GetComponentInParent<TacticsMove>();
            //A dying entity is no longer on its tile: it lets the pointer through to the tiles
            if (entity == null || entity.CurrentTile == null || entity.CurrentTile.GetEntity() != entity) continue;
            model ??= entity;
            if (modelPicking) entityTile ??= entity.CurrentTile;
        }
        return entityTile ?? relicTile;
    }

    /// <summary>
    /// Checks if the pointer is over an element of the UI Toolkit screens, which then gets the click instead of the game
    /// </summary>
    private static bool IsPointerOverUI() =>
        GameScene.UI != null && GameScene.UI.Hud.IsPointerOver(GameInput.PointerPosition);

    private class HitDistance : IComparer<RaycastHit> {
        public static readonly HitDistance Instance = new();
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}
