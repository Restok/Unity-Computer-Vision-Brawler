using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Demo script for boxing with the combination of FullBodyBipedIK and Aim IK.
	/// </summary>
	public class FBIKBoxing : MonoBehaviour {

		[Tooltip("The target we want to hit")]
		public Transform target;
		[Tooltip("The pin Transform is used to reference the exact hit point in the animation (used by AimIK to aim the upper body to follow the target)." +
		         "In Legacy and Generic modes you can just create and position a reference point in your animating software and include it in the FBX. " +
		         "Then in Unity if you added a GameObject with the exact same name under the character's root, it would be animated to the required position." +
		         "In Humanoid mode however, Mecanim loses track of any Transform that does not belong to the avatar, so in this case the pin point has to be manually set inside the Unity Editor.")]
		public Transform pin;
		[Tooltip("The Full Body Biped IK component")]
		public FullBodyBipedIK ik;
		[Tooltip("The Aim IK component. Aim IK is ust used for following the target slightly with the body.")]
		public AimIK aim;
		[Tooltip("The master weight")]
		public float weight;
		[Tooltip("The effector type of the punching hand")]
		public FullBodyBipedEffector effector;
		[Tooltip("Weight of aiming the body to follow the target")]
		public AnimationCurve aimWeight;
		public Transform playerHead;
		private Animator animator;
		private bool setTarget = true;
		void Start() {
			animator = GetComponent<Animator>();
		}

		void LateUpdate() {
			// Getting the weight of pinning the fist to the target
			float hitWeight = animator.GetFloat("HitWeight");
			// Pinning the first with FBIK
			ik.solver.GetEffector(effector).positionWeight = hitWeight * weight;
			if(hitWeight > 0 && setTarget)
            {
				Debug.Log("Setting target");
				ik.solver.GetEffector(effector).position = target.position;
				target.position = playerHead.position;
				setTarget = false;
			}
			else
            {
				setTarget = true;
            }


		}
    }
}
