using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour {


	[SerializeField]
	private float distanceAway;
	[SerializeField]
	private float distanceUp;
	[SerializeField]
	private float smooth;
	[SerializeField]
	private Transform followXForm;

	private Vector3 targetPosition;


	// Use this for initialization
	void Start () {

		followXForm = GameObject.FindWithTag("Player").transform;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void LateUpdate(){

		targetPosition = followXForm.position + Vector3.up * distanceUp - followXForm.forward * distanceAway;
		Debug.DrawRay(followXForm.position, Vector3.up * distanceUp, Color.red);
		Debug.DrawRay(followXForm.position, -1f * followXForm.forward * distanceAway,Color.blue);
		Debug.DrawLine(followXForm.position, targetPosition, Color.magenta);

		transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime *smooth);

		transform.LookAt(followXForm);


	}
}
