using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleTargetArtifact : Artifact
{
    const int RAYCAST_DISTANCE = 10;


	protected List<string> targets = new List<string>();
	public override bool IsRaycastHitAccepted(RaycastHit hitTerrain) {
        if (Physics.Raycast(hitTerrain.transform.position, Vector3.up, RAYCAST_DISTANCE, LayerMask.GetMask(targets.ToArray())))
            return true;
        else
            return false;
    }

	public override void Launch(PlayerStats source, RaycastHit hitTerrain, Animator animator) {
        SpendEnergy(source);
        RaycastHit hitAbove;
        if (Physics.Raycast(hitTerrain.transform.position, Vector3.up, out hitAbove, RAYCAST_DISTANCE, LayerMask.GetMask(targets.ToArray()))) {
            GameObject enemy = hitAbove.collider.gameObject;

            Vector3 position = hitTerrain.transform.position;
            position.y += 2;
            animator.SetTrigger("attacking");

            //StartCoroutine(LaunchFXAndAnim(animator, position));
            if (Prefab != null)
                GameObject.Instantiate(this.Prefab, position, Quaternion.identity);

            ApplyEffects(source, enemy.GetComponentInParent<EntityStats>());
        }
    }
}
