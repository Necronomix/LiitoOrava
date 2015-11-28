using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MovementScript : MonoBehaviour {

	private bool onFlight = false;
	private bool OnFlight {
		get
		{
			return onFlight && !flightHasDied;
		}
		set
		{
			onFlight = value;
		}

	}
	private Rigidbody rigidbody;
	[SerializeField] private float raycastFrequency = 0.1f;
	[SerializeField] private float flightHeight = 1.5f;
//	private float lastCheck;
	private float passedTime;
	[SerializeField] private Vector3 realGravity = new Vector3 (0f, -9.807f, 0f);
	[SerializeField] private float baseFlightAngleX = 20f;
	private float movementSpeed= 25f;
	private float rotationSpeed = 10f;
	private float strafeSpeed = 10f;
	private float flightPower = 0f;
	private float flightPowerMulti;
	private bool flightHasDied = false;

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
		return true;
	}



	void OnGUI() {
		GUI.Label(new Rect(10, 10, 100, 20), ""+OnFlight);
		GUI.Label(new Rect(10, 40, 100, 20), "FlightPower"+flightPower);
	}

	void SetGravity ()
	{
		if (OnFlight) {

			Physics.gravity = new Vector3 (0, 0, 0);

		} else
			Physics.gravity = realGravity;
	}	

	void HandleFlight ()
	{
		bool flightNow = IsOnFlight ();
		if (!onFlight && flightNow)
			StartFlight ();
		OnFlight = flightNow;
		//if(flightNow)
		//	rigidbody.r
	}

	void StartFlight ()
	{
		flightPower = 100f;
		rigidbody.AddForce (Vector3.down * 0.1f);
	}

	public static Vector3 NormalizeRotationZ (Vector3 rotation)
	{
		if (rotation.z > 180) {
			rotation = rotation - new Vector3 (0, 0, 360);
		}
		return rotation;
	}

	float CalculateFligthStrafe(Vector3 rotation)
	{
		rotation = NormalizeRotationZ (rotation);
		
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
		if (!OnFlight) {
			rigidbody.MovePosition(transform.position + transform.forward * Time.deltaTime * upAndDown);
		}
		else {
			Vector3 newRot = new Vector3( upAndDown * rotationSpeed * 3 , 0, -mov * rotationSpeed) * timeChange;
			Vector3 oldRot = transform.rotation.eulerAngles;
			oldRot = NormalizeRotationZ(oldRot);
			if(Mathf.Abs(oldRot.z) > 30 && (oldRot.z < 0 == newRot.z < 0))
				newRot = new Vector3(newRot.x, newRot.y, 0);
			transform.Rotate( newRot) ;
			rigidbody.AddRelativeForce(0, 0, 2 * (flightPower / 100f), ForceMode.Force);
			rigidbody.AddForce(CalculateFligthStrafe(newRot), 0, 0);

			float distance = Mathf.Abs(transform.rotation.x - baseFlightAngleX);
			float x = rigidbody.rotation.eulerAngles.x;
			if(x > baseFlightAngleX && x < 180)
			{
				flightPower += distance * distance * distance * 0.005f  * timeChange;
			}
			else 
			{
				flightPower -= distance * distance * distance * 0.005f  * timeChange;
			}
			if(flightPower <= 0)
				flightHasDied = true;
			//rigidbody.rotation.z += mov;
		}
		SetGravity ();

	
	}
}
