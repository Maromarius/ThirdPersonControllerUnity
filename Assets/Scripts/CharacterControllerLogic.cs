using UnityEngine;
using System.Collections;

public class CharacterControllerLogic : MonoBehaviour {

  	[SerializeField]
  	private Animator animator;
	[SerializeField]
	private float meshMoveSpeed = 1f;
	[SerializeField]
	private float animSpeed = 1.5f;
  	[SerializeField]
  	private float directionDampTime = 0.15f;
	[SerializeField]
	private CameraLogic cam;
	[SerializeField]
	private float directionSpeed = 3f;

 	private float speed = 0f;
	private float direction = 0f;
  	private float horizontal = 0f;
  	private float vertical = 0f;


	private CharacterMotor motor;
	private CharacterState state;

	private Camera gamecam;
	private AnimatorStateInfo currentBaseState;
	private AnimatorStateInfo layer2CurrentState;




	// Use this for initialization
	void Start () {

		gamecam = gameObject.transform.Find("CameraRotationPoint").Find("Camera").camera;
	    animator = GetComponent<Animator>();

	    if(animator.layerCount >= 2)
	    {
	      animator.SetLayerWeight(1, 1);
	    }
		motor = gameObject.GetComponent<CharacterMotor>() as CharacterMotor;
		state = gameObject.GetComponent<CharacterState>() as CharacterState;
	}
	/*
	void OnAnimatorMove(){
		if(animator){

			Vector3 newposition = transform.position;

			if(animator.GetBool("Sprint")){

				meshMoveSpeed = 12f;

			}
		
			newposition.z += animator.GetFloat("Speed") * meshMoveSpeed * Time.deltaTime;
			newposition.x += animator.GetFloat("Direction") * meshMoveSpeed * Time.deltaTime;
			transform.position = newposition;

		}
	}
	*/
	// Update is called once per frame
	void Update () {

	    if(animator)
	    {
			horizontal = Input.GetAxis("Horizontal");
			vertical = Input.GetAxis("Vertical");
			// Get the input vector from keyboard or analog stick
			Vector3 directionVector = new Vector3(horizontal, 0, vertical);

			if(Input.GetButton("Jump") && motor.grounded){
				if(state.jump()){
				//networkView.RPC("setAnimationBool", RPCMode.AllBuffered, "Jump",true);
				animator.SetBool("Jump", true);
				motor.forceJump = true;
				}
			}
			else if(motor.grounded){
				//networkView.RPC("setAnimationBool", RPCMode.AllBuffered, "Jump",false);
				animator.SetBool("Jump", false);
			}


			animator.speed = animSpeed;
			currentBaseState = animator.GetCurrentAnimatorStateInfo(0);
			
			
			//speed = new Vector2(horizontal,vertical).sqrMagnitude;
			animator.SetFloat("Speed",vertical,directionDampTime, Time.deltaTime);
			animator.SetFloat("Direction", horizontal, directionDampTime, Time.deltaTime);

			if(vertical <= 0.0f){
				meshMoveSpeed = 0.5f;
			}
			else{
				meshMoveSpeed = 1f;
			}



			if(Input.GetButton("Sprint")){
				//networkView.RPC("setAnimationBool", RPCMode.AllBuffered, "Sprint",false);
				animator.SetBool("Sprint", true);
				meshMoveSpeed = 2f;
				
				if(gamecam.camera.fieldOfView > 40)	
				{
					gamecam.camera.fieldOfView += (-50 * Time.deltaTime);
				}


				//gamecam.camera.fieldOfView = 50f * Time.deltaTime;
			}
			else{
				//networkView.RPC("setAnimationBool", RPCMode.AllBuffered, "Sprint",false);
				animator.SetBool("Sprint", false);
				meshMoveSpeed = 1f;
				if(gamecam.camera.fieldOfView < 60)
				{
					gamecam.camera.fieldOfView += (60 * Time.deltaTime);
				}
			}


			motor.inputMoveDirection = transform.rotation * directionVector * meshMoveSpeed * 40 * Time.deltaTime;
	   
    	}

	}


	//-------------------------------------------------------------------------
	//-----------------------------NETWORK Functions----------------------------
	//-------------------------------------------------------------------------
	[RPC]
	void setAnimationBool(string animationName, bool value) {
		animator.SetBool(animationName, value);
	}
}