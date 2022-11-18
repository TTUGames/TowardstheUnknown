using System.Collections;
using UnityEngine;

public class PlayAnimationAction : Action {
	private Animator animator;
	private string animationName;

	public PlayAnimationAction(Animator animator, string animationName = "attacking") {
		this.animator = animator;
		this.animationName = animationName;
		animator.Play(animationName);
	}
	public override void Apply() {
		if(!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName)) {
			isDone = true;
		}
	}
}