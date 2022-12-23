using UnityEngine;

public class VFXTool : MonoBehaviour
{
    public GameObject prefab; // Référence au préfabrication à lancer
    public string animationName; // Nom de l'animation à lancer

    void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            // Créer une instance du préfabrication à l'emplacement de l'objet sur lequel le script est attaché
            GameObject prefabInstance = Instantiate(prefab, transform.position, transform.rotation);

            // Obtenir le composant Animator de l'instance du préfabrication
            Animator animator = prefabInstance.GetComponent<Animator>();

            // Déclencher l'animation
            animator.SetTrigger(animationName);
        }
    }
}
