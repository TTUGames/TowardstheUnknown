using UnityEngine;

/// <summary>
/// Shakes the camera when the player loses health
/// </summary>
[RequireComponent(typeof(Animator))]
public class CameraShake : MonoBehaviour
{
    private Animator animator;
    private PlayerStats playerStats;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerStats = GameScene.Player.Stats;
        playerStats.Hit += Shake;
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.Hit -= Shake;
    }

    private static readonly int TakingDamage = Animator.StringToHash("isTakingDamage");

    private void Shake(int healthLost)
    {
        if (healthLost > 0) animator.SetTrigger(TakingDamage);
    }
}
