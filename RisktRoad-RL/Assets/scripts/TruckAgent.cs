using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TruckAgent : Agent
{
    public TruckController truckController;
    public TerrainSpawner terrainSpawner;
    public Rigidbody2D eggRb;
    public Rigidbody2D truckRb;
    public Transform truckTransform;
    public Transform eggTransform;

    private Vector3 truckStartPos;
    private Vector3 eggStartPos;
    private Quaternion eggStartRot;

    private float lastX;

    public override void Initialize()
    {
        truckStartPos = truckTransform.position;
        eggStartPos = eggTransform.position;
        eggStartRot = eggTransform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        //Debug.Log("Episode started");

        // Disable wheel joints temporarily
        truckController.frontWheel.enabled = false;
        truckController.backWheel.enabled = false;

        // Reset truck transform and rigidbody
        truckTransform.position = truckStartPos;
        truckTransform.rotation = Quaternion.identity;

        Rigidbody2D truckRb = truckTransform.GetComponent<Rigidbody2D>();
        truckRb.linearVelocity = Vector2.zero;
        truckRb.angularVelocity = 0f;

        truckController.accelerating = false;

        // Reset wheels rigidbodies
        Rigidbody2D frontRb = truckController.frontWheel.connectedBody;
        Rigidbody2D backRb  = truckController.backWheel.connectedBody;

        frontRb.linearVelocity = Vector2.zero;
        frontRb.angularVelocity = 0f;
        backRb.linearVelocity  = Vector2.zero;
        backRb.angularVelocity  = 0f;

        // Reset egg
        eggTransform.position = eggStartPos + Vector3.up * 0.1f;
        eggTransform.rotation = eggStartRot;
        eggRb.linearVelocity = Vector2.zero;
        eggRb.angularVelocity = 0f;

        // Reset terrain
        foreach (var segment in GameObject.FindGameObjectsWithTag("Terrain"))
        {
            Destroy(segment);
        }
        terrainSpawner.ResetTerrain();

        // Re-enable wheel joints
        truckController.frontWheel.enabled = true;
        truckController.backWheel.enabled = true;
    }





    public override void CollectObservations(VectorSensor sensor)
    {
        //Ray castes to sense the terrain ahead.
        float[] offsets = { 3f, 5f, 7f };
        float rayLength = 15f;
        float verticalOffset = 5f;

        foreach (float offset in offsets)
        {
            Vector2 origin = (Vector2)truckTransform.position + Vector2.right * offset + Vector2.up * verticalOffset;

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength);

            // Draw rays for visualization
            if (hit.collider != null)
            {
                //Debug.DrawRay(origin, Vector2.down * (origin.y - hit.point.y), Color.green);
            }
            else
            {
                //Debug.DrawRay(origin, Vector2.down * rayLength, Color.red);
            }

            // distance calculation
            float distanceToGround = hit ? (origin.y - hit.point.y) : rayLength;
            //Debug.Log($"Ray offset {offset:F1}: distance {distanceToGround:F2}");

            sensor.AddObservation(distanceToGround);
        }

        // Truck velocity
        sensor.AddObservation(truckRb.linearVelocity.x);
        sensor.AddObservation(truckRb.linearVelocity.y);

        sensor.AddObservation(truckRb.angularVelocity);

        // Normalized rotation (centered at 0)
        float rotationZ = truckTransform.eulerAngles.z;
        float rotationNorm = rotationZ / 180f;
        if (rotationNorm > 1f) rotationNorm -= 2f;
        sensor.AddObservation(rotationNorm);

        // Egg position difference
        sensor.AddObservation(eggTransform.position.x - truckTransform.position.x);
        sensor.AddObservation(eggTransform.position.y - truckTransform.position.y);

        //wheel on ground sensor.
        Vector2 frontForce = truckController.frontWheel.GetReactionForce(Time.fixedDeltaTime);
        Vector2 backForce  = truckController.backWheel.GetReactionForce(Time.fixedDeltaTime);

        sensor.AddObservation(frontForce.magnitude > 0.1f ? 1f : 0f); // 1 = touching, 0 = in air
        sensor.AddObservation(backForce.magnitude > 0.1f ? 1f : 0f);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        //The modle's action, accelerate or coast.
        int accelerate = actions.DiscreteActions[0]; // 0 = coast, 1 = accelerate
        truckController.accelerating = (accelerate == 1);

        // Reward based on forward progress
        float deltaX = truckTransform.position.x - lastX;
        AddReward(deltaX * 0.03f);
        lastX = truckTransform.position.x;

        //Debug.Log(truckRb.linearVelocity.x);

        // Punish for going to slow. To keep model from sitting still to avoid punishment.
        if (truckRb.linearVelocity.x <= 4.0f)
        {
            //Debug.Log("to slow");
            AddReward(-0.02f);
        }

        // 2. Penalty for tipping too far.
        float tilt = Mathf.Abs(truckTransform.eulerAngles.z);
        if (tilt > 50f && tilt < 300f)
        {
            //Debug.Log("tilt");
            AddReward(-0.075f); // small penalty each step
        }

        // 3. Penalty for egg leaving truck bed.
        float eggOffset = Mathf.Abs(eggTransform.position.y - truckTransform.position.y);
        if (eggOffset > 0.8f)
        {
            //Debug.Log("far");
            AddReward(-0.01f);
        }
        if (eggOffset > 1.3f)
        {
            //Debug.Log("big far");
            AddReward(-0.3f);
        }
    }

    //When egg hits the ground reset the game and punish model for crashing.
    public void GameOver()
    {
        AddReward(-1.0f);
        EndEpisode();
    }

    //Allows user to play the game.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // DiscreteActions[0] corresponds to acceleration action (0=coast, 1=accelerate)
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}
