using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : Artifact
{
    public BlackHole()
    {
        this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

        SetValues(2, 1, 5, 1, 0, new Vector2(2, 2), 0.01f);
    }

    public override void Launch(PlayerStats source, RaycastHit hitTerrain, Animator animator)
    {

        //TODO We must know by the artifact if it's a Tile artefact or an AOE artifact then decide which mask to take

        RaycastHit hitAbove;
        if (Physics.Raycast(hitTerrain.transform.position, Vector3.up, out hitAbove, 1 << LayerMask.GetMask("Player", "Enemy")))
        {
            GameObject enemy = hitAbove.collider.gameObject;

            Vector3 position = hitTerrain.transform.position;
            position.y += 2;
            animator.SetTrigger("attacking");

            //StartCoroutine(LaunchFXAndAnim(animator, position));
            GameObject.Instantiate(this.Prefab, position, Quaternion.identity);

            int damage = 100;
            enemy.GetComponentInParent<Enemy>().LowerHealth(damage);
        }
    }

    /*public IEnumerator LaunchFXAndAnim(Animator animator, Vector3 position)
    {
        while(!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            yield return null;

        Instantiate(this.Prefab, position, Quaternion.identity);
    }*/

    public override bool IsRaycastHitAccepted(RaycastHit hitTerrain)
    {
        if (Physics.Raycast(hitTerrain.transform.position, Vector3.up, 1 << LayerMask.GetMask("Player", "Enemy")))
            return true;
        else 
            return false;
    }
}
