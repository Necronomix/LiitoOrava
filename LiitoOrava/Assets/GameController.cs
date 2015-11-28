using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	[SerializeField] private MovementScript movementScript;
	[SerializeField] private Image flightGauge;

	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Space)) {
			Application.LoadLevel (Application.loadedLevel);
		} else if (Input.GetKey (KeyCode.Escape)) {
			Application.Quit();
		}
		flightGauge.fillAmount = movementScript.FlightPower / movementScript.MaxFlightPower;

	}
}
