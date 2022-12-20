using UnityEngine;

public class VfxAnimation : MonoBehaviour
{
    // R�f�rence au VFX
    public GameObject vfx;

    // R�f�rence � l'animation
    public Animation animation;

    void Start()
    {
        // Lorsque l'animation est lanc�e, on active le VFX
        animation.Play();
        vfx.SetActive(true);
    }

    void Update()
    {
        // Si la touche "espace" est enfonc�e, on met en pause l'animation et le VFX
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animation.enabled = !animation.enabled;
            vfx.SetActive(animation.enabled);
        }
    }
}