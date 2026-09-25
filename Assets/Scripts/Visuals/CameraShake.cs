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
        playerStats.HealthLost += Shake;
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.HealthLost -= Shake;
    }

    private void Shake(int healthLost)
    {
        animator.SetTrigger("isTakingDamage");
    }
}
