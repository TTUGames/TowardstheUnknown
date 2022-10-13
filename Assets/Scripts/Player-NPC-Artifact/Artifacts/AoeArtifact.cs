using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AoeArtifact : Artifact {
    protected int minAreaRange = 0;
    protected int maxAreaRange = 0;

    public override bool CanTarget(Tile tile) {
		return true;
	}

	public override void Launch(PlayerStats source, Tile tile, Animator animator) {
        ApplyCosts(source);

        Vector3 VFXposition = tile.transform.position;
        VFXposition.y += 2;
        animator.SetTrigger("attacking");

        //StartCoroutine(LaunchFXAndAnim(animator, position));
        if (Prefab != null)
            GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity);
        foreach (Tile t in tile.GetTilesWithinDistance(maxAreaRange, minAreaRange)) {
            EntityStats entity = t.GetEntity();
            if (entity == null || !targets.Contains(entity.tag)) continue;
            ApplyEffects(source, t.GetEntity());
		}
    }
}
