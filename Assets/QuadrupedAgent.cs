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
    private float actionScale = 2.0f;

    private void Start()
    {
        Time.timeScale = 10.0f; // 시뮬레이션 속도를 10배로 증가
        Time.fixedDeltaTime = 0.02f / Time.timeScale; // FixedUpdate 주기를 조정
    }


    public override void OnEpisodeBegin()
    {
        body.linearVelocity = Vector3.zero;
        body.position = new Vector3(45, 1, 0);
        body.rotation = Quaternion.Euler(0, 180, 0);
        foreach (var thigh in thighs) thigh.localRotation = Quaternion.identity;
        foreach (var shin in shins) shin.localRotation = Quaternion.identity;
        target.position = new Vector3(Random.Range(-5f, 5f)-45f, 0.5f, Random.Range(-5f, 5f));
        previousDistanceToTarget = Vector3.Distance(body.position, target.position);
    }

    public override void CollectObservations(VectorSensor sensor)
{
    // 각 데이터 추가 전에 디버그 출력
    sensor.AddObservation(body.position); // 3개

    sensor.AddObservation(body.linearVelocity); // 3개

    sensor.AddObservation(target.position - body.position); // 3개

    foreach (var thigh in thighs)
    {
        sensor.AddObservation(thigh.localRotation); // 4개
    }

    foreach (var shin in shins)
    {
        sensor.AddObservation(shin.localRotation); // 4개
    }

}


    public override void OnActionReceived(ActionBuffers actions)
    {
        for (int i = 0; i < thighs.Length; i++)
        {
            float thighAction = actions.ContinuousActions[i] * actionScale;
            float shinAction = actions.ContinuousActions[i + thighs.Length] * actionScale;
            thighs[i].GetComponent<Rigidbody>().AddTorque(Vector3.up * thighAction);
            shins[i].GetComponent<Rigidbody>().AddTorque(Vector3.up * shinAction);
        }

        float distanceToTarget = Vector3.Distance(body.position, target.position);
        AddReward(0.1f*(previousDistanceToTarget - distanceToTarget));
        previousDistanceToTarget = distanceToTarget;

        float tiltPenalty = Vector3.Dot(body.up, Vector3.up); // 1에 가까울수록 직립
    AddReward(tiltPenalty * 0.01f);


        if (distanceToTarget < 1.0f)
        {
            AddReward(1f);
            EndEpisode();
        }

        // If the agent falls off the platform, end the episode
        if (body.position.y < 0.3f || body.position.y > 1.5f)
        {
            AddReward(-1f);
            EndEpisode();
        }

        // If the agent goes to far, end the episode
        if (distanceToTarget > 100f)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    private int stepCounter = 0;

    private void FixedUpdate()
    {
        stepCounter++;
        if (stepCounter % 5 == 0) // 매 5번 호출마다 RequestDecision
        {
            RequestDecision();
        }
    }
}
