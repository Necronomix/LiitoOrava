using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	[SerializeField] private MovementScript movementScript;
	[SerializeField] private Image flightGauge;
    [SerializeField]
    private ParticleSystem snowFlakes;

	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Space)) {
			Application.LoadLevel (Application.loadedLevel);
		} else if (Input.GetKey (KeyCode.Escape)) {
			Application.Quit();
		}
        float relative = movementScript.FlightPower / movementScript.MaxFlightPower;
        flightGauge.fillAmount = relative;
        if(relative > 0)
         snowFlakes.startSpeed = relative * 40f;

	}
}
