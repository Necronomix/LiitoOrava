﻿using UnityEngine;
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
	[SerializeField] private float baseFlightAngleX = 25f;
	[SerializeField] private float movementSpeed= 25f;
	[SerializeField] private float rotationSpeed = 20f;
	[SerializeField] private float strafeSpeed = 20f;
	[SerializeField] private float maxFlightPower = 400f;
	[SerializeField] private float flightSpeedMultiplier = 2f;
	[SerializeField] private float maximumAngle = 30f;
	[SerializeField] private float turnSpeed = 20f;
	private float flightPower = 0f;
	private float flightPowerMulti;

	private bool flightHasDied = false;

	private float ScaledFlightForce
	{
		get
		{
			return flightPower * 0.01f;
		}
	}

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
		return !Physics.Raycast(transform.position, Vector3.down, flightHeight);
	}



	void OnGUI() {
		GUI.Label(new Rect(10, 10, 100, 20), ""+OnFlight);
		GUI.Label(new Rect(10, 40, 100, 20), "FlightPower"+flightPower);
		GUI.Label(new Rect(10, 70, 100, 20), "Velocity "+rigidbody.velocity);
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
		rigidbody.useGravity = false;
		//rigidbody.AddForce (Vector3.down * 0.1f);
	}

	void EndFlight()
		{
			rigidbody.useGravity = true;
		flightHasDied = true;
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
		float direction = rotation.z > 0 ? 1 : -1;

		return strafeSpeed * ScaledFlightForce * ScaleMultiplierForAngle (rotation) * direction;

	}

	float ScaleMultiplierForAngle (Vector3 rotation)
	{
		return Mathf.Abs ((rotation.z))  / maximumAngle;
	}



	// Update is called once per frame
	void Update () {

		float timeChange = Time.deltaTime;
		passedTime += timeChange;
		if (passedTime > raycastFrequency && !onFlight) {
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
			Vector3 newRot = new Vector3( upAndDown * rotationSpeed * 2 , 0, -mov * rotationSpeed) * timeChange;
			Vector3 oldRot = transform.rotation.eulerAngles;
			oldRot = NormalizeRotationZ(oldRot);
			if(Mathf.Abs(oldRot.z) > maximumAngle && (oldRot.z < 0 == newRot.z < 0))
				newRot = new Vector3(newRot.x, newRot.y, 0);
			transform.Rotate( newRot) ;

			float strafe = CalculateFligthStrafe(newRot);
			rigidbody.AddForce(strafe, 0, 0);
			float flightSpeed = flightSpeedMultiplier * ScaledFlightForce;
			//float scaler = ScaleMultiplierForAngle(NormalizeRotationZ(transform.rotation.eulerAngles));
			//if(scaler > 0.1f)
			//{
			//	flightSpeed /= (3 * scaler);
			//}
			//We will push forward the fllier
			rigidbody.AddRelativeForce(0, 0, flightSpeed, ForceMode.Force);

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
			//rigidbody.rotation.z += mov;
		}
		//SetGravity ();

	
	}
}
