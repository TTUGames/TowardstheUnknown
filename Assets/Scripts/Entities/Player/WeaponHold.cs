using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Holds the player's weapons in its hands. In the prefab they hang from a finger bone, as the original's: they turn with
/// the fingers of each clip. Here, after the animation, each weapon is put back on its hand's bone at a fixed grip, and a
/// gun shot brings the barrel onto its target: over the moments before the shot, the spine turns to line the barrel up
/// with the point the projectile flies to (an attack whose legs keep the stance loses the turn of the clip's hips, so its
/// clip alone aims beside the target), and lets go at the shot, for the recoil. The clip pointing the gun far away (drawing
/// it, a shot at the sky) is left as animated. The gun goes in the right hand for an artifact whose clip shoots with it.
/// Turned off (the Classic edition's <see cref="EditionOnly"/>), the weapons go back to their finger
/// </summary>
[DefaultExecutionOrder(-50)] //Before what reads the weapons' place in LateUpdate: the WeaponTrail
public class WeaponHold : MonoBehaviour
{
    /// <summary>
    /// A weapon's place in a hand, in the hand bone's space
    /// </summary>
    [System.Serializable]
    private struct Grip
    {
        [Required] public Transform hand;
        public Vector3 position;
        public Quaternion rotation;

        public readonly void Hold(Transform weapon) => weapon.SetPositionAndRotation(hand.TransformPoint(position), hand.rotation * rotation);
    }

    [BoxGroup("Sword"), SerializeField, Required] private Transform sword;
    [BoxGroup("Sword"), SerializeField] private Grip swordGrip;

    [BoxGroup("Gun"), SerializeField, Required] private Transform gun;
    [BoxGroup("Gun"), SerializeField, Required, Tooltip("The end of the barrel, where the shot starts")] private Transform muzzle;
    [BoxGroup("Gun"), SerializeField, Tooltip("The direction the barrel shoots in, in the gun's space")] private Vector3 barrel = Vector3.left;
    [BoxGroup("Gun"), SerializeField] private Grip gunGrip;
    [BoxGroup("Gun"), SerializeField] private Grip gunRightGrip;

    [BoxGroup("Aim"), SerializeField, Tooltip("The bones turned to aim, from the pelvis up: each takes its share of the turn")] private Transform[] spine = System.Array.Empty<Transform>();
    [BoxGroup("Aim"), SerializeField, Min(0), SuffixLabel("s"), Tooltip("The barrel is on the target this long before the shot")] private float aimLead = 0.2f;
    [BoxGroup("Aim"), SerializeField, Min(0.01f), SuffixLabel("s"), Tooltip("Time taken to bring it there")] private float aimBlend = 0.12f;
    [BoxGroup("Aim"), SerializeField, Min(0.01f), SuffixLabel("s"), Tooltip("Time taken to let go at the shot")] private float release = 0.06f;
    [BoxGroup("Aim"), SerializeField, Range(0, 180), SuffixLabel("°"), Tooltip("The barrel is brought onto the target when the clip points it this close to it")]
    private float aimAngle = 80;
    [BoxGroup("Aim"), SerializeField, Range(0, 180), SuffixLabel("°"), Tooltip("The clip pointing the barrel this far from the target, the pose is left as animated")]
    private float freeAngle = 120;

    private Vector3 swordPosition, gunPosition;
    private Quaternion swordRotation, gunRotation;
    private bool rightHanded;
    private Tile aimed;
    // The body aimed at, found once per shot
    private Bounds? aimedBody;
    private float shotTime;

    private void Awake()
    {
        sword.GetLocalPositionAndRotation(out swordPosition, out swordRotation);
        gun.GetLocalPositionAndRotation(out gunPosition, out gunRotation);
    }

    private void OnDisable()
    {
        aimed = null;
        rightHanded = false;
        sword.SetLocalPositionAndRotation(swordPosition, swordRotation);
        gun.SetLocalPositionAndRotation(gunPosition, gunRotation);
    }

    /// <summary>
    /// The gun shoots at the tile in <paramref name="shotDelay"/> seconds, from the right hand or the left
    /// </summary>
    public void Aim(Tile tile, bool rightHand, float shotDelay)
    {
        if (!isActiveAndEnabled) return;
        rightHanded = rightHand;
        aimed = tile;
        aimedBody = ProjectileInfo.Body(tile);
        shotTime = Time.time + shotDelay;
    }

    public void StopAim() => aimed = null;

    // After the animation and its IK
    private void LateUpdate()
    {
        bool swordHeld = sword.gameObject.activeInHierarchy, gunHeld = gun.gameObject.activeInHierarchy;
        Grip grip = rightHanded ? gunRightGrip : gunGrip;
        if (gunHeld && aimed != null) TurnToTarget(grip);
        if (swordHeld) swordGrip.Hold(sword);
        if (gunHeld) grip.Hold(gun);
    }

    // In scaled time, as the attack's clock: a hit stop holds the aim
    private void TurnToTarget(Grip grip)
    {
        float time = Time.time;
        float pull = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(shotTime - aimLead - aimBlend, shotTime - aimLead, time))
            * Mathf.SmoothStep(1, 0, Mathf.InverseLerp(shotTime, shotTime + release, time));
        if (pull <= 0) return;
        grip.Hold(gun);
        Vector3 animated = gun.TransformDirection(barrel).normalized;
        Vector3 toTarget = AimPoint() - muzzle.position;
        if (toTarget.sqrMagnitude < 0.01f) return;
        pull *= Mathf.SmoothStep(1, 0, Mathf.InverseLerp(aimAngle, freeAngle, Vector3.Angle(animated, toTarget)));
        if (pull <= 0) return;
        //Turning the spine moves the muzzle, and with it the line to the target: a second pass settles it
        for (int pass = 0; pass < 2; pass++)
            for (int i = 0; i < spine.Length; i++)
            {
                Vector3 wanted = Vector3.Slerp(animated, (AimPoint() - muzzle.position).normalized, pull);
                Quaternion turn = Quaternion.FromToRotation(gun.TransformDirection(barrel), wanted);
                //Each bone takes its share of what is left: the last one all of it
                spine[i].rotation = Quaternion.Slerp(Quaternion.identity, turn, 1f / (spine.Length - i)) * spine[i].rotation;
                grip.Hold(gun);
            }
    }

    // Where the projectile flies to: the target's body, or its tile at the muzzle's height
    private Vector3 AimPoint()
    {
        Vector3 from = muzzle.position;
        if (aimedBody is Bounds body) return body.ClosestPoint(from);
        Vector3 center = aimed.transform.position;
        center.y = from.y;
        return center;
    }
}
