using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleTargetArtifact : Artifact
{
	protected List<string> targets = new List<string>();
	
	public override bool CanTarget(Tile tile) {
        EntityStats target = tile.GetEntity();
        if (target != null) Debug.Log(target.name);
        return target != null && targets.Contains(target.tag);
    }

	public override void Launch(PlayerStats source, Tile tile, Animator animator) {
        if (!CanTarget(tile)) return;
        SpendEnergy(source);
        EntityStats target = tile.GetEntity();

        Vector3 VFXposition = tile.transform.position;
        VFXposition.y += 2;
        animator.SetTrigger("attacking");

        //StartCoroutine(LaunchFXAndAnim(animator, position));
        if (Prefab != null)
            GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity);

        ApplyEffects(source, target.GetComponentInParent<EntityStats>());
    }
}
