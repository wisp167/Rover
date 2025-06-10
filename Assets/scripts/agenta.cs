using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using static System.Math;

public class AgentController : Agent
{
    //shpere var
    [SerializeField] private Transform target;
    public int counter;
    public GameObject food;
    [SerializeField] private List<GameObject> spawnedSphereList = new List<GameObject>();

    //agent var
    //[SerializeField] private float moveSpeed = 4f;
    private Rigidbody rb;
    private float timelim = 1f;
    private float prev_step = 1f;
    public Car_Controller car;

    //environment var
    [SerializeField] private Transform environmentlocation;
    private Renderer cachedEnvRenderer;
    private Material envMaterial;
    public GameObject env;

    [SerializeField] private Terrain terrain;

    private RayPerceptionSensorComponent3D raySensor;

    private float CurrentGoalDist;
    private float TimeLimit;

    private float PastGoalDist = 0;
    private float LessonCounter = 0;
    private float stopTimer = 0f;
    private float WinCounter = 0f;
    private float AllCounter = 2f;
    //private float requiredStopDuration = 1f; // Time in seconds the agent must remain stopped
    private float targetProximityThreshold = 5f; // Distance threshold to consider the agent "close enough"

    // LIdar variables
    private List<LidarHit> lidarMemory = new List<LidarHit>();
    public GameObject memoryPointPrefab;
    private List<GameObject> memoryPointVisualizations = new List<GameObject>();
    //private int numberPointsToVisualize = 500;
    //private int  numberPointsToSend = 500;
    //private int realNumberPointsToSend = 50;

    private Dictionary<int, Color> tagColorMap = new Dictionary<int, Color>{
		    {-1, Color.white},
		    {0, Color.green},
		    {1, Color.yellow},
		    {2, Color.red}
    };
                        

    public override void Initialize(){
        rb = GetComponent<Rigidbody>();
	cachedEnvRenderer = env.GetComponent<Renderer>();
        envMaterial = cachedEnvRenderer.material;
	raySensor = GetComponentInChildren<RayPerceptionSensorComponent3D>();
	if(raySensor == null){
          Debug.LogError("RayPerceptionSensor3D component not found!");
	}
    }

    public struct LidarHit{
       public Vector3 hitPosition;
       public int hitTag;
       public LidarHit(Vector3 position, int tag){
          hitPosition = position;
	  hitTag = tag;
       }
    }

    /*
    void Update(){
	if(rb.linearVelocity.magnitude < 0.1f){
           AddReward((float)-0.001);
	}
        //Debug.Log(GetCumulativeReward());
        //return;
        float dist = 1;
        if(spawnedSphereList.Count!=0){
            dist = Vector3.Distance(transform.localPosition, spawnedSphereList[0].transform.localPosition);
        }
        //Debug.Log(transform.rotation);
        //Debug.Log(transform.localPosition);
        //Debug.Log(GetCumulativeReward());
	float rewardForMovingCloser = (prev_step - dist) * 0.1f;
        prev_step = dist;
	AddReward(rewardForMovingCloser);

	if(Time.time - timelim > 60){
            AddReward(-10f);
	    EndEpisode();
	}
        //if((dist > 500) || (Time.time-timelim > 60)){
        //    AddReward(-(dist-prev_step)-10f);
        //    EndEpisode();
        //}
        //AddReward(-(dist-prev_step));

	VisualizeRaycasts(); 
	VisualizeMemory();
    }
    */



    private void VisualizeRaycasts(){
        if (raySensor == null) return;

	var rayInput = raySensor.GetRayPerceptionInput();
	var rayOutputs = RayPerceptionSensor.Perceive(rayInput, false).RayOutputs;       
	    // Iterate over the RayOutputs array
    	foreach (var rayOutput in rayOutputs){
        	Vector3 start = rayOutput.StartPositionWorld;               float len = rayOutput.HitFraction;
        	Vector3 end = start+ (rayOutput.EndPositionWorld-start)*len;
		if (rayOutput.HasHit){
		   if(rayOutput.HitTaggedObject){
			   int tag = rayOutput.HitTagIndex;
			   switch(tag){
				case 0:
				   Debug.DrawLine(start, end, Color.green);                    
				   lidarMemory.Add(new LidarHit(end, tag));
				   break;
				case 1:
				   Debug.DrawLine(start, end, Color.yellow);
				   lidarMemory.Add(new LidarHit(end, tag));
				   break;
				case 2:
				   Debug.DrawLine(start, end, Color.red);
				   lidarMemory.Add(new LidarHit(end, tag));
				   break;
			   }
		   }
		}
            	else{
			lidarMemory.Add(new LidarHit(end, -1));
			
                	Debug.DrawLine(start, end, Color.white);
		}
        	//Debug.DrawLine(start, end, Color.red);
    	}
    }
    /*
    private void VisualizeMemory(){
	foreach(var point in memoryPointVisualizations){
            Destroy(point);
	}
	memoryPointVisualizations.Clear();

	int pointsToVisualize = Mathf.Min(lidarMemory.Count, numberPointsToVisualize);
	for(int i = lidarMemory.Count - pointsToVisualize; i < lidarMemory.Count; ++i){
            GameObject pointVis = Instantiate(memoryPointPrefab, lidarMemory[i].hitPosition,Quaternion.identity);  
	    Renderer renderer = pointVis.GetComponent<Renderer>();
	    if(renderer != null){
               renderer.material.color = tagColorMap[lidarMemory[i].hitTag];
	    }
	    memoryPointVisualizations.Add(pointVis);
	}
    }
    */
    private bool IsPositionValid(Vector3 position, float minDistance) {
        // First do a quick broad-phase check
        Collider[] nearbyColliders = Physics.OverlapSphere(position, minDistance);
        
        foreach (Collider col in nearbyColliders) {
            // Skip if not an obstacle
            if (!col.CompareTag("obstacle")) continue;

            // Calculate exact distance to collider
            Vector3 closestPoint = col.ClosestPoint(position);
            float actualDistance = Vector3.Distance(position, closestPoint);

            // Debug visualization
            Debug.DrawLine(position, closestPoint, 
                        actualDistance < minDistance ? Color.red : Color.green, 
                        1f);

            if (actualDistance < minDistance) {
                return false;
            }
        }
        
        return true;
    }
    /*
    private bool IsPositionValid(Vector3 position, float minDistance) {
        if(Physics.CheckSphere(position, minDistance, LayerMask.GetMask("obstacle"))) {
            return false;
        }
        return true;
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("obstacle");
        foreach(GameObject obstacle in obstacles) {
            Collider[] colliders = obstacle.GetComponents<Collider>();
            foreach(Collider collider in colliders) {
                Vector3 closestPoint = collider.ClosestPoint(position);
                Debug.Log($"{obstacle}, {position}, {Vector3.Distance(position, closestPoint)}");
                // Check distance to closest point
                if(Vector3.Distance(position, closestPoint) < minDistance) {
                    return false;
                }
            }
        }
        
        //foreach(GameObject obstacle in obstacles) {
        //    if(Vector3.Distance(position, obstacle.transform.position) < minDistance) {
        //        return false;
        //    }
        //}
        
        
        
        return true;
    }
    */

    /*
    private void CreateSphere(){
        for(int i = 0; i < counter; ++i){
            GameObject newsphere = Instantiate(food);
            newsphere.transform.parent = environmentlocation;                    
            float x = Random.Range(CurrentGoalDist, CurrentGoalDist+1), z = Random.Range(CurrentGoalDist, CurrentGoalDist+1);
            if(Random.Range(0, 2) == 1){
                x = -x;
            }
            if(Random.Range(0, 2) == 1){
                z = -z;
            }
            float height = terrain.SampleHeight(transform.localPosition + new Vector3(x, 0, z))+0.5f;
            Vector3 spherelocation = new Vector3(x+transform.localPosition.x, height, z+transform.localPosition.z);
            newsphere.transform.localPosition = spherelocation;
            spawnedSphereList.Add(newsphere);
        }
    }
    */
    private void CreateSphere() {
        for(int i = 0; i < counter; ++i) {
            GameObject newSphere = Instantiate(food);
            newSphere.transform.parent = environmentlocation;
            
            bool validPosition = false;
            int attempts = 0;
            Vector3 sphereLocation = Vector3.zero;
            
            // Try to find a valid position (max 100 attempts to prevent infinite loops)
            while(!validPosition && attempts < 300) {
                attempts++;
                
                // Generate random position within the terrain bounds
                float x = Random.Range(CurrentGoalDist, CurrentGoalDist+1);
                float z = Random.Range(CurrentGoalDist, CurrentGoalDist+1);
                
                if(Random.Range(0, 2) == 1) x = -x;
                if(Random.Range(0, 2) == 1) z = -z;
                
                // Get terrain height
                float height = terrain.SampleHeight(transform.localPosition + new Vector3(x, 0, z)) + 0.5f;
                sphereLocation = new Vector3(x + transform.localPosition.x, height, z + transform.localPosition.z);
                
                // Check distance to all obstacles
                validPosition = IsPositionValid(sphereLocation, 3*targetProximityThreshold);
            }
            
            newSphere.transform.localPosition = sphereLocation;
            spawnedSphereList.Add(newSphere);
            if(!validPosition){
                Debug.LogWarning("Failed to find valid position for sphere after 300 attempts");
            }
        }
    }



    public override void OnEpisodeBegin(){
        CurrentGoalDist = Academy.Instance.EnvironmentParameters.GetWithDefault("goal_distance", 50f);
        TimeLimit = Academy.Instance.EnvironmentParameters.GetWithDefault("time_limit", 30);
        if(CurrentGoalDist != PastGoalDist){  
        Debug.Log($"Curriculum changed! Goal Distance: {CurrentGoalDist}"); // Console log

            //Academy.Instance.EnvironmentParameters.Set("curriculum_level", CurrentGoalDist); // For TensorBoard
            LessonCounter += 1;
            if(LessonCounter > 9){
                LessonCounter-=1;
            }
            PastGoalDist = CurrentGoalDist;

        }
        stopTimer = 0f;
        terrain = Terrain.activeTerrain;
        float randomX = Random.Range((float)CurrentGoalDist+1f, (float)Max(terrain.terrainData.size.x-CurrentGoalDist, CurrentGoalDist+1f));
        float randomZ = Random.Range((float)CurrentGoalDist+1f, (float)Max(terrain.terrainData.size.z-CurrentGoalDist, CurrentGoalDist+1f));

        float terrainHeight = terrain.SampleHeight(new Vector3(randomX, 0, randomZ));

        Vector3 spawnPosition = new Vector3(randomX, terrainHeight + 4.5f, randomZ);
        //spawnPosition+=terrain.transform.position;
        transform.localPosition = spawnPosition;
        transform.rotation = Quaternion.identity;
        rb.rotation =  Quaternion.identity;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        if(Time.time-timelim < 2f){
            AllCounter-=1;
        }
        timelim = Time.time;
        transform.rotation = new UnityEngine.Quaternion(0, 0, 0, 0);
        if(spawnedSphereList.Count!=0){
            RemoveSphere(spawnedSphereList);
        }
        CreateSphere();
        prev_step = Vector3.Distance(transform.localPosition, spawnedSphereList[0].transform.localPosition);
    }

    private void RemoveSphere(List<GameObject> ToBeDeletedGameObjectList){
        foreach(GameObject i in ToBeDeletedGameObjectList){
            Destroy(i.gameObject);
        }
        ToBeDeletedGameObjectList.Clear();
    }


public override void CollectObservations(VectorSensor sensor)
{
    // Check if there are any spheres in the list
    if (spawnedSphereList.Count > 0)
    {
        Vector3 relativeTarget = spawnedSphereList[0].transform.localPosition - transform.localPosition;
        float distanceToTarget = relativeTarget.magnitude;

        bool CloseEnough = distanceToTarget <= targetProximityThreshold;
        sensor.AddObservation(CloseEnough ? 1f : 0f);

        Vector3 velocity2d = rb.linearVelocity;
        velocity2d.y = 0;
        Vector3 targetDirection2d = relativeTarget;
        targetDirection2d.y = 0;

        float angleSpeedToTarget = Vector3.SignedAngle(velocity2d, targetDirection2d, Vector3.up);
        sensor.AddObservation(angleSpeedToTarget / 180f);

        Vector3 roverForward2d = transform.forward;
        roverForward2d.y = 0;
        float angleToTarget = Vector3.SignedAngle(roverForward2d, targetDirection2d, Vector3.up);
        sensor.AddObservation(angleToTarget / 180f);
        sensor.AddObservation((rb.linearVelocity.normalized-relativeTarget.normalized).y);
        sensor.AddObservation((transform.forward.normalized-roverForward2d.normalized).y);
        sensor.AddObservation(Vector3.SignedAngle(rb.linearVelocity-velocity2d, relativeTarget-targetDirection2d, Vector3.up)/180f);
        sensor.AddObservation(Vector3.SignedAngle(transform.forward-roverForward2d, relativeTarget-targetDirection2d, Vector3.up)/180f);
        //Debug.Log($"Distance: {distanceToTarget/CurrentGoalDist} AngleSpeed: {angleSpeedToTarget/180f} Angle: {angleToTarget/180f}, Forawrd: {Vector3.SignedAngle(roverForward2d, targetDirection2d, Vector3.up)/180f}, y_axis: {(rb.linearVelocity.normalized-relativeTarget.normalized).y}, y_axis2: {(transform.forward.normalized-roverForward2d.normalized).y}");
    }
    else
    {
        sensor.AddObservation(0f); 
        sensor.AddObservation(0f);
        sensor.AddObservation(0f); 
        sensor.AddObservation(0f);
        sensor.AddObservation(0f);
        sensor.AddObservation(0f);
        sensor.AddObservation(0f);
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    Vector3 eulerAngles = transform.rotation.eulerAngles;

    float heading = NormalizeAngle(eulerAngles.y)/180f; // Yaw
    float pitch = NormalizeAngle(eulerAngles.x)/180f;   // Pitch
    float roll = NormalizeAngle(eulerAngles.z)/180f;    // Roll

    sensor.AddObservation(heading);
    sensor.AddObservation(pitch);
    sensor.AddObservation(roll);

    float maxSpeed = 20f;
    sensor.AddObservation(rb.linearVelocity.magnitude / maxSpeed);

    float maxAngularSpeed = Mathf.PI;
    sensor.AddObservation(rb.angularVelocity.magnitude / maxAngularSpeed);
}


    public override void OnActionReceived(ActionBuffers actions)
    {
        //float moveRotate =actions.ContinuousActions[0];
        //float moveForward =actions.ContinuousActions[1];
        //moveSpeed = 24f;
        //rb.MovePosition(transform.position + transform.forward*moveForward*moveSpeed*Time.deltaTime);
        //transform.Rotate(0f, moveRotate*moveSpeed/2, 0f, Space.Self);

        //Vector3 velocity = new Vector3(moveX, 0f, -moveZ)*Time.deltaTime*moveSpeed;
        //velocity = velocity.normalized*Time.deltaTime*moveSpeed;
        //transform.localPosition +=velocity;
        float moveInput = actions.ContinuousActions[1];
            float steerInput = actions.ContinuousActions[0];
        //Debug.Log($"Steer Input: {steerInput}, Move Input: {moveInput}");
        car.MoveInput(moveInput*(float)1);
            car.SteerInput(steerInput);
            car.Uupdate();
            car.LlateUpdate();
        //rb.MovePosition(car.carRb.position);
        //rb.MoveRotation(car.carRb.rotation);
        //transform.position = rb.position;
        //transform.rotation = rb.rotation;
        //Debug.Log(GetCumulativeReward());
        //return;
        float dist = 1;
        if(spawnedSphereList.Count!=0){
	        dist = (spawnedSphereList[0].transform.localPosition - transform.localPosition).magnitude; //More efficient.
            //dist = Vector3.Distance(transform.localPosition, spawnedSphereList[0].transform.localPosition);
        }
        //Debug.Log(transform.rotation);
        //Debug.Log(transform.localPosition);
        //Debug.Log(GetCumulativeReward());
        //float rewardForMovingCloser = (prev_step - dist) / CurrentGoalDist * 3f;
        float progress = Mathf.Clamp01(1 - (dist / CurrentGoalDist));
        float rewardForMovingCloser = (prev_step - dist)/CurrentGoalDist * (10 - LessonCounter + 0 * progress);
        //float movementReward = (prev_step - dist) * (1 + 2 * progress);
        prev_step = dist;
        //Debug.Log($"Distance: {dist}, Reward: {rewardForMovingCloser}");
        //Debug.Log(GetCumulativeReward());
        //Debug.Log(WinCounter/AllCounter);
        
        if(dist > targetProximityThreshold && rb.linearVelocity.magnitude < 0.1f){
            AddReward((float)-0.0001);
        }
        AddReward((float)-0.00001);
        AddReward(rewardForMovingCloser);
        /*
        if (spawnedSphereList.Count > 0)
        {
            Vector3 d = spawnedSphereList[0].transform.localPosition-transform.localPosition;
            //d.y = 0

            if (d.magnitude < targetProximityThreshold)
            {
                // Agent is close to the target
                if (rb.linearVelocity.magnitude < 5f) // Check if the agent has stopped
                {
                    stopTimer += Time.fixedDeltaTime; // Increment the stop timer
                    if (stopTimer >= requiredStopDuration)
                    {
                        // Agent has been stopped long enough near the target
                        Destroy(spawnedSphereList[0]);
                        spawnedSphereList.Remove(spawnedSphereList[0]);
                        AddReward(10f); // Reward for completing the task
                        EndEpisode();
                    }
                }
                else
                {
                    if(rb.linearVelocity.magnitude > 5f){
                        AddReward((5-rb.linearVelocity.magnitude)/500f);
                    }
                    else{
                        AddReward((5-rb.linearVelocity.magnitude)/25f);
                    }
                    stopTimer = 0f;
                }
            }
            else
            {
                // Agent is not close to the target, reset the stop timer
                stopTimer = 0f;
                AddReward(rewardForMovingCloser);
            }
        }
        */
        //Debug.Log(GetCumulativeReward());
        if(Time.time - timelim > TimeLimit){
            AddReward(-5f);
            AllCounter+=1;
            EndEpisode();
        }
        if(spawnedSphereList.Count != 0 && dist < targetProximityThreshold){
            Destroy(spawnedSphereList[0]);
            spawnedSphereList.Remove(spawnedSphereList[0]);
            AddReward(10f);
            if(spawnedSphereList.Count==0){
                //envMaterial.color = Color.green;
                AddReward(0.05f);
                AllCounter+=1;
                WinCounter+=1;
                EndEpisode();
            }
        }
        //if((dist > 500) || (Time.time-timelim > 60)){
        //    AddReward(-(dist-prev_step)-10f);
        //    EndEpisode();
        //}
        //AddReward(-(dist-prev_step));

	//VisualizeRaycasts(); 
	//VisualizeMemory();    
	
        //Debug.Log(rb.velocity);
        //rb = car.carRb;
        //transform.localPosition = rb.localPosition;
        //transform.rotation = rb.rotation;
        /*
        car.Move();
        car.AnimateWheels();
        car.Steer();
        car.Brake();
        */

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        //Debug.Log("AAA");
    }



    private void OnTriggerEnter(Collider other)
    {
        /*
        if(other.gameObject.tag == "Cyl"){
            spawnedSphereList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(10f);
            if(spawnedSphereList.Count==0){
                //envMaterial.color = Color.green;
                AddReward(0.05f);
                EndEpisode();
            }
            //EndEpisode();
        }
        */
        
        if(other.gameObject.tag == "obstacle"){
            //envMaterial.color = Color.red;
            AllCounter+=1;
            AddReward(-15f);
            EndEpisode();
        }
        if(other.gameObject.tag == "wall"){
            AllCounter+=1;
            AddReward(-10f);
            EndEpisode();
        }
    }
}
