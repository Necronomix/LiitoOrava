using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	[SerializeField] private float baseFlightAngleX = 25f;
	[SerializeField] private float movementSpeed= 25f;
	[SerializeField] private float rotationSpeed = 20f;
	[SerializeField] private float strafeSpeed = 20f;
	[SerializeField] private float maxFlightPower = 400f;
	[SerializeField] private float maximumAngle = 30f;
	[SerializeField] private float turnSpeed = 20f;
	[SerializeField] private float startingFlightPower = 100f;
	private float strafeDampifier = 0.1f;
	private float multiForUpAndDownComparedToSides = 2f;
	private float flightPower = 0f;
	private Dictionary<string, string> toOnGui = new Dictionary<string, string> ();


	public float MaxFlightPower {
		get {
			return maxFlightPower;
		}
	}

	public float FlightPower {
		get {
			return flightPower;
		}
	}

	private float flightPowerMulti;

	private bool flightHasDied = false;

	private float ScaledFlightForce
	{
		get
		{
			return Mathf.Pow(flightPower * 0.01f, 2);
		}
	}

	public float GetAxis(string axisName)
	{
		return Input.GetAxis (axisName);
	}



	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		passedTime = 0f;
	}


	bool IsOnFlight()
	{
		return !Physics.Raycast(transform.position, Vector3.down, flightHeight);
	}


#if DEBUG
	void OnGUI() {
		int i = 0;
		foreach (KeyValuePair<string, string> item in toOnGui) {
			GUI.Label(new Rect(10, 70 + i * 30, 500, 30), item.Key + item.Value);
			i++;
		}
		GUI.Label(new Rect(10, 10, 100, 20), ""+OnFlight);
	}
#endif



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
		flightPower = startingFlightPower;
		rigidbody.useGravity = false;
		//rigidbody.AddForce (Vector3.down * 0.1f);
	}

	void EndFlight()
	{
		rigidbody.useGravity = true;
		flightPower = 0f; 
		flightHasDied = true;
	}

	public static Vector3 NormalizeRotationZ (Vector3 rotation)
	{
		if (rotation.z > 180) {
			rotation = rotation - new Vector3 (0, 0, 360);
		}
		return rotation;
	}

	float CalculateFligthStrafe()
	{
		//The devil itself

		Quaternion up = new Quaternion ();
		float upAndDownAngle =  (0 * 0.0174533f) * 0.5f;

		float sinUDAngle = Mathf.Sin (upAndDownAngle);
		Vector3 n = transform.up;
		up.w = Mathf.Cos (upAndDownAngle);
		up.x = n.x * sinUDAngle;
		up.y = n.y * sinUDAngle;
		up.z = n.z * sinUDAngle;

		// get a "forward vector" for each rotation
		Vector3 forwardA = transform.rotation * Vector3.right;
		Vector3 forwardB = up * Vector3.right;
		
		// get a numeric angle for each vector, on the X-Z plane (relative to world forward)
		var angleA = Mathf.Atan2(forwardA.y, forwardA.z);//Mathf.Rad2Deg;
		var angleB = Mathf.Atan2(forwardB.y, forwardB.z);//Mathf.Rad2Deg;
		
		// get the signed difference in these angles
		float angleDiff = Mathf.DeltaAngle(Mathf.Rad2Deg * angleA, Mathf.Rad2Deg * angleB);
		ToOnGui ("AngleDiff", angleDiff.ToString ());
		
		float direction = Mathf.Sign(angleDiff);
		//return 0;
		return Mathf.Min(maximumAngle, Mathf.Abs(angleDiff)) * direction * strafeSpeed;
	}

	float ScaleMultiplierForAngle (Vector3 rotation)
	{
		return Mathf.Min(1, Mathf.Abs ((rotation.z))  / maximumAngle);
	}

	void ToOnGui (string key,  string str)
	{
		#if DEBUG
		if (toOnGui.ContainsKey (key)) {
			toOnGui[key] = str;
		} else {
			toOnGui.Add(key, str);
		}
		#endif
	}


	void OnCollisionEnter(Collision col)
	{
		if (col.collider.CompareTag ("Respawn")) return;
		EndFlight ();
	}


	void RotateBy (float angles, Vector3 axis, float f)
	{
		Quaternion targetX = new Quaternion ();
		//Something like this?
		float upAndDownAngle =  (angles * 0.0174533f) * 0.5f;
		//ToOnGui ("upAngle", upAndDownAngle.ToString ());
		float sinUDAngle = Mathf.Sin (upAndDownAngle);
		Vector3 n = axis;
		targetX.w = Mathf.Cos (upAndDownAngle);
		targetX.x = n.x * sinUDAngle;
		targetX.y = n.y * sinUDAngle;
		targetX.z = n.z * sinUDAngle;

		targetX = targetX * transform.rotation;
		ToOnGui ("angleX:", targetX.ToString ());
		transform.rotation = Quaternion.Slerp (transform.rotation, targetX, f);
	}

	// Update is called once per frame
	void FixedUpdate () {

		float timeChange = Time.fixedDeltaTime;
		passedTime += timeChange;
		if (passedTime > raycastFrequency && !onFlight) {
			passedTime -= raycastFrequency;
			HandleFlight ();
		}


		float mov = GetAxis ("Horizontal");
		float upAndDown = GetAxis ("Vertical");

		ToOnGui ("up:", upAndDown.ToString());
		if (!OnFlight) {
			rigidbody.MovePosition(transform.position + transform.forward * Time.deltaTime * upAndDown);
		}
		else {
			Vector3 newRot = new Vector3( upAndDown * rotationSpeed * 2 , 0, -mov * rotationSpeed);
			Vector3 oldRot = transform.rotation.eulerAngles;
			//oldRot = NormalizeRotationZ(oldRot);
			//if(Mathf.Abs(oldRot.z) > maximumAngle && (oldRot.z < 0 == newRot.z < 0))
			//	newRot = new Vector3(newRot.x, newRot.y, 0);
			//if((oldRot + newRot).x >= 85 || (oldRot + newRot).x <= 0)
			//	newRot = new Vector3(0, newRot.y, newRot.z);


			//Vector3 newRotation = newRot;
			//Rotate Z-axis
			//Quaternion targetZ = new Quaternion(); //Something like this?
			//
			//float angle = -mov * (maximumAngle * 0.0174533f) / 2.0f;
			//float sinAngle = Mathf.Sin(angle);
			//Vector3 n = transform.forward;
			//targetZ.w = Mathf.Cos(angle);
			//targetZ.x = n.x * sinAngle;
			//targetZ.y = n.y * sinAngle;
			//targetZ.z = n.z * sinAngle;
			//targetZ = targetZ * transform.rotation;
			//ToOnGui("angleZ:",targetZ.ToString());
			//transform.rotation = Quaternion.Slerp(transform.rotation, targetZ, Mathf.Abs(mov) * timeChange * rotationSpeed);
			//ToOnGui("z", rigidbody.rotation.eulerAngles.z.ToString());

			RotateBy (-mov * rotationSpeed, transform.forward, Mathf.Abs(mov) * timeChange);
			RotateBy (turnSpeed * upAndDown, transform.right, Mathf.Abs (upAndDown) * timeChange); 
			
		
			//Basics
			float strafe = CalculateFligthStrafe();
			float scaleFlightForce = ScaledFlightForce;
			float flightSpeed = scaleFlightForce * movementSpeed;

			//Scale them to timestep
			strafe = strafe * scaleFlightForce * strafeDampifier * timeChange;
			flightSpeed = timeChange * flightSpeed;
			ToOnGui("strafe",strafe.ToString());

			ToOnGui("flightSpeed",flightSpeed.ToString());
			//Apply actual movement
			rigidbody.MovePosition(transform.position + transform.right * strafe + transform.forward * flightSpeed);
			//rigidbody.MovePosition(transform.position + transform.forward * Time.deltaTime * flightSpeed);

			//This is for calculating flight force
			float distance = Mathf.Abs(transform.rotation.x - baseFlightAngleX);
			float x = rigidbody.rotation.eulerAngles.x;
			if(x > baseFlightAngleX && x < 180)
			{
				flightPower = Mathf.Min(maxFlightPower, (flightPower + distance * distance * distance * 0.005f  * timeChange));
			}
			else 
			{
				flightPower -= distance * distance * distance * 0.005f  * timeChange;
			}
			if(flightPower <= 0)
				EndFlight();
		}

	
	}
}
