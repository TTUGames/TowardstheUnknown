using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXInfo
{
    public enum Target { GUN, SWORD, LEFTHAND, RIGHTHAND, SOURCETILE, TARGETTILE }

    private GameObject prefab;
    private float delay;
    private Vector3 offset;
    private Target target;
    private float rotationOffset;

    public VFXInfo(string name, Target target, float delay = 0f) : this(name, target, delay, Vector3.zero){
        
	}

    public VFXInfo(string name, Target target, float delay, Vector3 offset, float rotationOffset = 0) {
        this.prefab = Resources.Load<GameObject>("VFX/00-Prefab/" + name);
        this.target = target;
        this.delay = delay;
        this.offset = offset;
        this.rotationOffset = rotationOffset;
	}

    public void Play(WaitForAttackEndAction action, GameObject source, Tile targetTile) {
        source.GetComponent<TacticsAttack>().StartCoroutine(PlayDelayed(action, source, targetTile));
        
	}

    public IEnumerator PlayDelayed(WaitForAttackEndAction action, GameObject source, Tile targetTile) {
        yield return new WaitForSeconds(delay);

        GameObject vfx = GameObject.Instantiate(prefab);

        Transform VFXorigin = GetOrigin(source, targetTile);
        Vector3 VFXdirection = VFXorigin.position - targetTile.transform.position;
        VFXdirection.y = 0;
        float VFXrotation = (-Vector3.SignedAngle(VFXdirection, Vector3.forward, Vector3.up) + rotationOffset) % 360;
        action.SetVFX(vfx);
        vfx.transform.SetParent(VFXorigin);
        vfx.AddComponent<ConstantRotation>().SetRotation(new Vector3(0, VFXrotation, 0));
        vfx.transform.localPosition = offset;
    }

    private Transform GetOrigin(GameObject source, Tile targetTile) {
        switch (target) {
            case Target.GUN:
                return source.GetComponent<PlayerAttack>().GunMarker;
            case Target.SWORD:
                return source.GetComponent<PlayerAttack>().SwordMarker;
            case Target.LEFTHAND:
                return source.GetComponent<PlayerAttack>().LeftHandMarker;
            case Target.RIGHTHAND:
                return source.GetComponent<PlayerAttack>().RightHandMarker;
            case Target.SOURCETILE:
                return source.GetComponent<TacticsMove>().CurrentTile.transform;
            case Target.TARGETTILE:
                return targetTile.transform;
		}
        return null;
	}
}
