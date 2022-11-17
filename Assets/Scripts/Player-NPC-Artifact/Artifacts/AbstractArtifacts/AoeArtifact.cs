using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AoeArtifact : Artifact {
    protected TileSearch area;

    public override bool CanTarget(Tile tile) {
		return true;
	}

	public override void Launch(PlayerStats source, Tile tile, Animator animator) {
        ApplyCosts(source);

        Vector3 VFXposition = tile.transform.position;
        VFXposition.y += 2;
        if(animStateName == "")
            animator.SetTrigger("attacking");
        else
            animator.Play(animStateName);

        //StartCoroutine(LaunchFXAndAnim(animator, position));
        if (Prefab != null)
            GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity);
        foreach (Tile target in GetTargets(tile)) {
            EntityStats entity = target.GetEntity();
            if (entity == null || !targets.Contains(entity.tag)) continue;
            ApplyEffects(source, target.GetEntity());
		}
    }

	public override List<Tile> GetTargets(Tile targetedTile) {
        if (targetedTile == null || !targetedTile.isSelectable) return new List<Tile>();
        area.SetStartingTile(targetedTile);
        area.Search();
        return area.GetTiles();
    }
}
