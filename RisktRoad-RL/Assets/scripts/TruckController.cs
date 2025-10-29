using UnityEngine;

public class TruckController : MonoBehaviour
{
    public WheelJoint2D frontWheel;
    public WheelJoint2D backWheel;

    [Header("Engine Settings")]
    public float motorTorque = 800f;
    public float maxSpeed = 15f;
    public float acceleration = 2f;

    [Header("Coasting Settings")]
    public float deceleration = 0.98f;

    [Header("Control Input")]
    public bool accelerating = false; // <- AI or player sets this

    private JointMotor2D frontMotor;
    private JointMotor2D backMotor;

    void Start()
    {
        frontMotor = frontWheel.motor;
        backMotor = backWheel.motor;

        frontWheel.useMotor = true;
        backWheel.useMotor = true;
    }

    void FixedUpdate()
    {
        float frontAngularSpeed = frontWheel.connectedBody.angularVelocity;
        float backAngularSpeed = backWheel.connectedBody.angularVelocity;
        float currentSpeed = Mathf.Max(Mathf.Abs(frontAngularSpeed), Mathf.Abs(backAngularSpeed));

        if (accelerating)
        {
            float targetSpeed = -maxSpeed;
            frontMotor.motorSpeed = Mathf.Lerp(frontMotor.motorSpeed, targetSpeed, Time.fixedDeltaTime * acceleration);
            backMotor.motorSpeed = Mathf.Lerp(backMotor.motorSpeed, targetSpeed, Time.fixedDeltaTime * acceleration);

            frontMotor.maxMotorTorque = motorTorque;
            backMotor.maxMotorTorque = motorTorque;

            frontWheel.motor = frontMotor;
            backWheel.motor = backMotor;

            frontWheel.useMotor = true;
            backWheel.useMotor = true;
        }
        else
        {
            frontWheel.useMotor = false;
            backWheel.useMotor = false;

            frontWheel.connectedBody.angularVelocity *= deceleration;
            backWheel.connectedBody.angularVelocity *= deceleration;
        }
    }
}
