using UnityEngine;

public class FreezePositionDuringAnimation : StateMachineBehaviour
{
    private Vector3 startPosition; // Stocke la position de départ de l'objet

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Lorsque l'animation commence, on stocke la position actuelle de l'objet
        startPosition = animator.transform.position;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // On empêche la variation des positions X et Z de l'objet
        animator.transform.position = new Vector3(startPosition.x, animator.transform.position.y, startPosition.z);
    }
}
