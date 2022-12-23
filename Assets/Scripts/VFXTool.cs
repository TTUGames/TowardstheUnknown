using UnityEngine;

public class VFXTool : MonoBehaviour
{
    public GameObject prefab; // Référence au préfabrication à lancer
    public string animationName; // Nom de l'animation à lancer
    private Animator animator; // Référence au composant Animator de l'objet sur lequel le script est attaché

    void Start()
    {
        // Obtenir une référence au composant Animator de l'objet sur lequel le script est attaché
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Input.GetKeyDown("v"))
        {
            // Créer une instance du préfabrication à l'emplacement de l'objet sur lequel le script est attaché
            GameObject prefabInstance = Instantiate(prefab, transform.position, transform.rotation);

            animator.GetComponent<Animator>().Play(animationName);
        }
    }
}
