using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Movement2 : MonoBehaviour
{

    private bool onFlight = false;
    private bool OnFlight
    {
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
    [SerializeField]
    private float raycastFrequency = 0.1f;
    [SerializeField]
    private float flightHeight = 1.5f;
    //	private float lastCheck;
    private float passedTime;
    [SerializeField]
    private float baseFlightAngleX = 25f;
    [SerializeField]
    private float movementSpeed = 25f;
    [SerializeField]
    private float rotationSpeed = 20f;
    [SerializeField]
    private float strafeSpeed = 20f;
    [SerializeField]
    private float maxFlightPower = 400f;
    [SerializeField]
    private float maximumAngle = 30f;
    [SerializeField]
    private float turnSpeed = 20f;
    private float flightPower = 0f;
    float strafe;
    float strafeAngle;
    [SerializeField]
    private GameObject rotatedObject;
    [SerializeField]
    private GameObject targetFront;

    [SerializeField]
    private float x_axis;
    [SerializeField]
    private float z_axis;
    [SerializeField]
    private float y_axis;

    public float MaxFlightPower
    {
        get
        {
            return maxFlightPower;
        }
    }

    public float FlightPower
    {
        get
        {
            return flightPower;
        }
    }

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
        return Input.GetAxis(axisName);
    }



    // Use this for initialization
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        //lastCheck = Time.time;
        passedTime = 0f;

    }


    bool IsOnFlight()
    {
        return !Physics.Raycast(transform.position, Vector3.down, flightHeight);
    }


#if DEBUG
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "" + OnFlight);

        GUI.Label(new Rect(10, 70, 500, 30), "FlightAngle " + transform.rotation.eulerAngles);
        GUI.Label(new Rect(10, 100, 500, 20), "FlightPower" + (int) flightPower + " Strafe: " + (int)strafe);
        GUI.Label(new Rect(10, 130, 500, 20), "Strafe angle :" + strafeAngle);
    }
#endif


    void HandleFlight()
    {
        bool flightNow = IsOnFlight();
        if (!onFlight && flightNow)
            StartFlight();
        OnFlight = flightNow;
        //if(flightNow)
        //	rigidbody.r
    }

    void StartFlight()
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

    public static Vector3 NormalizeRotationZ(Vector3 rotation)
    {
        if (rotation.z > 180)
        {
            rotation = rotation - new Vector3(0, 0, 360);
        }
        return rotation;
    }

    float CalculateFligthStrafe(Quaternion rotation)
    {


        strafeAngle = transform.localEulerAngles.z;// Quaternion.Angle(Quaternion.Euler(new Vector3(transform.position.x, transform.position.y, transform.position.z)) , rotation);

        float direction = strafeAngle < 0 ? 1 : -1;

        return 0;

    }

    float CalculateFligthStrafe(Vector3 rotation)
    {
        rotation = NormalizeRotationZ(rotation);

        float direction = rotation.z < 0 ? 1 : -1;

        return strafeSpeed * ScaledFlightForce * ScaleMultiplierForAngle(rotation) * direction;

    }

    float ScaleMultiplierForAngle(Vector3 rotation)
    {
        return Mathf.Min(1, Mathf.Abs((rotation.z)) / maximumAngle);
    }

    void FixedUpdate()
    {

        float timeChange = Time.fixedDeltaTime;
        passedTime += timeChange;
        if (passedTime > raycastFrequency && !onFlight)
        {
            passedTime -= raycastFrequency;
            HandleFlight();
        }


        float mov = GetAxis("Horizontal");
        float upAndDown = GetAxis("Vertical");

        if (!OnFlight)
        {
            rigidbody.MovePosition(transform.position + transform.forward * Time.deltaTime * upAndDown);
        }
        else
        {
            Vector3 newRot = new Vector3(upAndDown * rotationSpeed * 2, 0, -mov * rotationSpeed) * timeChange;
            Vector3 oldRot = rotatedObject.transform.rotation.eulerAngles;
            oldRot = NormalizeRotationZ(oldRot);
            if (Mathf.Abs(oldRot.z) > maximumAngle && (oldRot.z < 0 == newRot.z < 0))
                newRot = new Vector3(newRot.x, newRot.y, 0);
            if ((oldRot + newRot).x >= 85 || (oldRot + newRot).x <= 0)
                newRot = new Vector3(0, newRot.y, newRot.z);

            rotatedObject.transform.Rotate(newRot);

            strafe = CalculateFligthStrafe(oldRot);
            
            float flightSpeed = ScaledFlightForce * movementSpeed;


            rotatedObject.transform.rotation = Quaternion.Slerp(rotatedObject.transform.rotation, targetFront.transform.rotation, Time.deltaTime * 0.7f);
            Quaternion angle = Quaternion.Lerp(rotatedObject.transform.rotation, targetFront.transform.rotation, 1);
           

            rigidbody.MovePosition(transform.position + new Vector3(0, 0, strafe) * Time.deltaTime + transform.forward * Time.deltaTime * flightSpeed);

            float distance = Mathf.Abs(transform.rotation.x - baseFlightAngleX);
            float x = rotatedObject.transform.rotation.eulerAngles.x;
            if (x > baseFlightAngleX && x < 180)
            {
                flightPower = Mathf.Min(maxFlightPower, (flightPower + distance * distance * distance * 0.005f * timeChange));
            }
            else
            {
                flightPower -= distance * distance * distance * 0.005f * timeChange;
            }
            //if (flightPower <= 0)
            //    EndFlight();
        }



        //else
        //{
        //    // Vector3 newRot = new Vector3(upAndDown * rotationSpeed * 2, 0, -mov * rotationSpeed) * timeChange;
        //    // Vector3 oldRot = transform.rotation.eulerAngles;
        //    // oldRot = NormalizeRotationZ(oldRot);
        //    // if (Mathf.Abs(oldRot.z) > maximumAngle && (oldRot.z < 0 == newRot.z < 0))
        //    //     newRot = new Vector3(newRot.x, newRot.y, 0);
        //    // if ((oldRot + newRot).x >= 85 || (oldRot + newRot).x <= 0)
        //    //     newRot = new Vector3(0, newRot.y, newRot.z);
        //    // 
        //    strafe = CalculateFligthStrafe(transform.rotation);
        //    // Quaternion newRot = transform.rotation;
        //    Quaternion target = Quaternion.Euler(-mov * 10, 0, -upAndDown*10);


        //    target = target * transform.rotation;
        //    //transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation, (float)(Time.deltaTime * 2));
        //    // Quaternion upDown = Quaternion.AngleAxis(1000 * Time.deltaTime * upAndDown, Vector3.forward);
        //    // Quaternion rightLeft = Quaternion.AngleAxis(1000 * Time.deltaTime * mov, Vector3.left);

        //    transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 2);

        //    // transform.rotation = Quaternion.RotateTowards(transform.rotation, , Time.deltaTime * 1.5);

            
        //    rigidbody.AddForce(0, 0, strafe);
        //    //rigidbody.MovePosition(transform.position +  );
        //    // float flightSpeed = ScaledFlightForce * movementSpeed;
        //    //float scaler = ScaleMultiplierForAngle(NormalizeRotationZ(transform.rotation.eulerAngles));
        //    //if(scaler > 0.1f)
        //    //{
        //    //	flightSpeed /= (3 * scaler);
        //    //}
        //    //We will push forward the fllier
        //    //rigidbody.AddRelativeForce(0, 0, flightSpeed, ForceMode.Force);
        //    //rigidbody.MovePosition(transform.position + new Vector3(0, 0, strafe) * Time.deltaTime + transform.forward * Time.deltaTime * flightSpeed);


        //    float distance = Mathf.Abs(transform.rotation.x - baseFlightAngleX);
        //    float x = rigidbody.rotation.eulerAngles.x;
        //    if (x > baseFlightAngleX && x < 180)
        //    {
        //        flightPower = Mathf.Min(maxFlightPower, (flightPower + distance * distance * distance * 0.005f * timeChange));
        //    }
        //    else
        //    {
        //        flightPower -= distance * distance * distance * 0.005f * timeChange;
        //    }
        //    // if (flightPower <= 0)
        //    //     EndFlight();
        //}
    }
}
