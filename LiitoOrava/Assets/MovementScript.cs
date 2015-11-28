using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MovementScript : MonoBehaviour {

	private bool onFlight = false;
	private Rigidbody rigidbody;
	[SerializeField] private float raycastFrequency = 0.1f;
	[SerializeField] private float flightHeight = 1.5f;
//	private float lastCheck;
	private float passedTime;
	[SerializeField] private Vector3 realGravity = new Vector3 (0f, -9.807f, 0f);
	private float movementSpeed= 25f;
	private float rotationSpeed = 10f;
	private float strafeSpeed = 10f;

	public float GetAxis(string axisName)
	{
		return Input.GetAxis (axisName);
	}



	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		//lastCheck = Time.time;
		passedTime = 0f;
	
	}


	bool IsOnFlight()
	{
		return !Physics.Raycast (transform.position, Vector3.down, flightHeight);
	}



	void OnGUI() {
		GUI.Label(new Rect(10, 10, 100, 20), ""+onFlight);
	}

	void SetGravity ()
	{
		if (onFlight) {

			Physics.gravity = new Vector3 (0, -0.1f, 0);
		} else
			Physics.gravity = realGravity;
	}	

	void HandleFlight ()
	{
		bool flightNow = IsOnFlight ();
		onFlight = flightNow;
		//if(flightNow)
		//	rigidbody.r
	}


	float CalculateFligthStrafe(Vector3 rotation)
	{
		if (rotation.z > 180) {
			rotation = rotation - new Vector3 (0, 0, 360) ;

		}
		
		//if(rotation.z > 180) {
		//	//rotation.z - 180
		//	return 1;
		//}
		float direction = rotation.z < 0 ? 1 : -1;

		return strafeSpeed * Mathf.Abs((rotation.z)) / 30f;

	}

	// Update is called once per frame
	void Update () {

		float timeChange = Time.deltaTime;
		passedTime += timeChange;
		if (passedTime > raycastFrequency) {
			passedTime -= raycastFrequency;
			HandleFlight ();
		}


		float mov = GetAxis ("Horizontal");
		float upAndDown = GetAxis ("Vertical");
		//Physics.gravity = 
		if (!onFlight) {
			rigidbody.MovePosition(transform.position + transform.forward * Time.deltaTime * upAndDown);
		}
		else {
			Vector3 newRot =  rigidbody.rotation.eulerAngles + new Vector3( upAndDown * rotationSpeed * 3 , 0, -mov * rotationSpeed) * timeChange;
			rigidbody.rotation = Quaternion.Euler(newRot) ;
			rigidbody.AddRelativeForce(0, 0, 1, ForceMode.Force);
			rigidbody.AddForce(CalculateFligthStrafe(newRot), 0, 0);
			//rigidbody.rotation.z += mov;
		}
		SetGravity ();

	
	}
}
