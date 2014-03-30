using UnityEngine;
using System.Collections;

public class CameraLogic : MonoBehaviour {

	private float angH;
	private float angV;
	private Texture crosshairTexture;
	private GameObject camera_rotation_point;
	private GameObject character;

	// Use this for initialization
	void Start () {


		camera_rotation_point = gameObject.transform.parent.gameObject;
		character = camera_rotation_point.transform.parent.gameObject;
		//Get gui textures/components
		//crosshairTexture = Resources.Load("Textures/T_Aim_Dot") as Texture;
		//------------------------------------------
	
	}
	
	// Update is called once per frame
	void LateUpdate () {

		//CAMERA MOVEMENTS AND CHARACTER ROTATION
		//Calculate degrees of rotation based on input
		angH = Input.GetAxis("Right_X_axis_joystick") * -6.0f;
		angV = Input.GetAxis("Right_Y_axis_joystick") * -4.5f;
		
		//For Vertical Rotation
		//Rotate camera
		//camera.transform.RotateAround(camera_rotation_point.transform.position, new Vector3(0,0,1), angV);
		Vector3 currentAngles = camera_rotation_point.transform.localEulerAngles;
		currentAngles.x += angV;
		//Current HACK for preventing camera to go lower
		if(currentAngles.x<310 && currentAngles.x>200)currentAngles.x=310;
		if(currentAngles.x>60 && currentAngles.x<200)currentAngles.x=60;
		//Debug.Log ("Angle: "+currentAngles.x);
		camera_rotation_point.transform.localEulerAngles = currentAngles;
		
		//For Horizontal rotation
		if(angH != 0){//Rotate character
			currentAngles = character.transform.localEulerAngles;
			currentAngles.y += angH;
			character.transform.localEulerAngles = currentAngles;
		}

	
	}

	void OnGUI(){
		//Crosshair GUI
		if(crosshairTexture)
			GUI.DrawTexture(new Rect(Screen.width/2-10, Screen.height/2-10, 20,20), crosshairTexture);
	}

}
