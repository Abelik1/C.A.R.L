using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Numerics;
using Unity.VisualScripting;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float moveSpeed = 1f;
    private Rigidbody rb;
    private UnityEngine.Vector3 _initialPosition;
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

    }
    public float minDistance = 2.5f; // Minimum distance between Agent and Goal
    public override void OnEpisodeBegin()
    {
        _initialPosition = transform.localPosition;

        // const float minDistance = 3f; // Minimum distance between Agent and Goal
        UnityEngine.Vector3 agentPosition, goalPosition;

        // Generate random positions until they are far enough apart
        do
        {
            agentPosition = new UnityEngine.Vector3(Random.Range(-6.8f, 2.0f), 0, Random.Range(-2f, +1.8f));
            goalPosition = new UnityEngine.Vector3(Random.Range(-6.8f, 2.0f), 0, Random.Range(-2f, +1.8f));
        }
        while (Mathf.Abs(UnityEngine.Vector3.Distance(agentPosition, goalPosition)) < minDistance);

        // Assign the positions
        transform.localPosition = agentPosition;
        UnityEngine.Quaternion randomRotation = UnityEngine.Quaternion.Euler(0f, Random.Range(0, 360), 0f);
        transform.localRotation = randomRotation;
        targetTransform.localPosition = goalPosition;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(transform.localRotation.y);
        sensor.AddObservation(targetTransform.localPosition.x);
        sensor.AddObservation(targetTransform.localPosition.z);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];
        
        transform.Rotate(0f, moveRotate * moveSpeed , 0f, Space.Self);

        // Move forward in local space
        transform.Translate(UnityEngine.Vector3.forward * moveForward * moveSpeed * Time.deltaTime);
        AddReward(-0.01f);
        // Calculate the distance between the current position and the target position
        //float distanceToTarget = UnityEngine.Vector3.Distance(transform.localPosition, targetTransform.localPosition);

        // Define a reward based on the distance (you can adjust this logic as needed)
        //float reward = 1-distanceToTarget / 7;
        // AddReward(reward/10000);
        //AddReward(moveForward/10000);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions= actionsOut.ContinuousActions;
        continuousActions[0]= Input.GetAxisRaw("Horizontal");
        continuousActions[1]= Input.GetAxisRaw("Vertical");
    }
    int i = 0;
    UnityEngine.Vector3 vector;
    private void OnTriggerEnter(Collider other){
        
        if (other.TryGetComponent<Goal>(out Goal goal)){
            AddReward(5f);
            i +=1;
            Debug.Log("Collected goal, i = " + i);
            do
            {
                vector = new UnityEngine.Vector3(Random.Range(-6.8f,2.0f),0,Random.Range(-2f,+1.8f));
            }
            while (Mathf.Abs(UnityEngine.Vector3.Distance(transform.localPosition, vector)) < minDistance);
            targetTransform.localPosition = vector;
            if (i >= 7){
                Debug.Log("Episode ending, i = " + i);
                AddReward(10f);
                i=0;
                EndEpisode(); 
            }
        }
        if (other.TryGetComponent<Wall>(out Wall wall)){
        SetReward(-5f);
        
        EndEpisode();
        }
    }
}