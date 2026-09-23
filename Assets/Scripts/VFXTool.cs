using UnityEngine;
using UnityEngine.InputSystem;

public class VFXTool : MonoBehaviour
{
    public GameObject VFXPrefab;
    public float VFXDelay = 0f;
    [Space]
    public string animationName; 
    public float animationDelay = 0f;

    private void OnEnable() => GameInput.Controls.Debug.PlayVFX.performed += OnPlayVFX;
    private void OnDisable() => GameInput.Controls.Debug.PlayVFX.performed -= OnPlayVFX;

    private void OnPlayVFX(InputAction.CallbackContext context)
    {
        Invoke(nameof(StartAnimation), animationDelay);
        Invoke(nameof(StartVFX), VFXDelay);
    }
    void StartVFX()
    {
        Instantiate(VFXPrefab);
    }
    void StartAnimation()
    {
        GetComponent<Animator>().Play(animationName);
    }
}
