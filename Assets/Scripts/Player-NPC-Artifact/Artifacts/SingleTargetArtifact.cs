using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleTargetArtifact : Artifact
{	
	public override bool CanTarget(Tile tile) {
        EntityStats target = tile.GetEntity();
        return target != null && targets.Contains(target.tag);
    }

	public override void Launch(PlayerStats source, Tile tile, Animator animator) {
        if (!CanTarget(tile)) return;
        ApplyCosts(source);
        EntityStats target = tile.GetEntity();

        Vector3 VFXposition = tile.transform.position;
        VFXposition.y += 2;
        animator.SetTrigger("attacking");

        //StartCoroutine(LaunchFXAndAnim(animator, position));
        if (Prefab != null)
            GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity);

        ApplyEffects(source, target.GetComponentInParent<EntityStats>());
    }

	public override List<Tile> GetTargets() {
        Tile targetedTile = Tile.GetHoveredTile();
        if (targetedTile == null || !targetedTile.isSelectable || targetedTile.GetEntity() == null || !targets.Contains(targetedTile.GetEntity().tag)) return new List<Tile>();
        List<Tile> targetedTiles = new List<Tile>();
        targetedTiles.Add(targetedTile);
        return targetedTiles;
	}
}