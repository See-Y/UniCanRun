using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class QuadrupedAgent : Agent
{
    public Transform target;
    public Transform[] thighs;
    public Transform[] shins;
    public Rigidbody body;
    private float previousDistanceToTarget;


    public override void OnEpisodeBegin()
    {
        body.linearVelocity = Vector3.zero;
        transform.position = new Vector3(45, 1, 0);
        transform.rotation = Quaternion.Euler(0, 180, 0);
        foreach (var thigh in thighs) thigh.localRotation = Quaternion.identity;
        foreach (var shin in shins) shin.localRotation = Quaternion.identity;
        target.position = new Vector3(Random.Range(-5f, 5f), 0.5f, Random.Range(-5f, 5f));
        previousDistanceToTarget = Vector3.Distance(transform.position, target.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(body.linearVelocity);
        sensor.AddObservation(target.position - transform.position);
        foreach (var thigh in thighs) sensor.AddObservation(thigh.localRotation);
        foreach (var shin in shins) sensor.AddObservation(shin.localRotation);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        for (int i = 0; i < thighs.Length; i++)
        {
            float thighAction = actions.ContinuousActions[i] * 100f;
            float shinAction = actions.ContinuousActions[i + thighs.Length] * 100f;
            thighs[i].GetComponent<Rigidbody>().AddTorque(Vector3.up * thighAction);
            shins[i].GetComponent<Rigidbody>().AddTorque(Vector3.up * shinAction);
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        AddReward(previousDistanceToTarget - distanceToTarget);
        previousDistanceToTarget = distanceToTarget;

        if (distanceToTarget < 1.0f)
        {
            AddReward(1f);
            EndEpisode();
        }

        // If the agent falls off the platform, end the episode
        if (transform.position.y < -0.5f)
        {
            AddReward(-1f);
            EndEpisode();
        }


    }
}
