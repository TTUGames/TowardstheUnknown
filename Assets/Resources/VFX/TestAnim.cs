using UnityEngine;

public class VfxAnimation : MonoBehaviour
{
    // Référence au VFX
    public GameObject vfx;

    // Référence à l'animation
    public Animation animation;

    void Start()
    {
        // Lorsque l'animation est lancée, on active le VFX
        animation.Play();
        vfx.SetActive(true);
    }

    void Update()
    {
        // Si la touche "espace" est enfoncée, on met en pause l'animation et le VFX
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animation.enabled = !animation.enabled;
            vfx.SetActive(animation.enabled);
        }
    }
}