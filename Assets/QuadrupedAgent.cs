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
    private float actionScale = 90.0f;

    private int decisionPeriod = 5; // 행동 요청 주기 (FixedUpdate 호출 횟수 기준)
    private int stepCount = 0;      // 현재 스텝 카운트

    void FixedUpdate()
    {
        // 행동 요청 주기에 따라 RequestDecision 호출
        if (stepCount % decisionPeriod == 0)
        {
            RequestDecision();
        }
        else
        {
            RequestAction();
        }

        stepCount++;
    }

    public override void OnEpisodeBegin()
    {
        body.linearVelocity = Vector3.zero;
        body.position = new Vector3(45, 1, 0);
        transform.rotation = Quaternion.Euler(0, 180, 0);
        // thigh rotate 0,0,90
        foreach (var thigh in thighs) thigh.localRotation = Quaternion.Euler(0, 0, 90);
        // shin rotate 0,0,90
        foreach (var shin in shins) shin.localRotation = Quaternion.Euler(0, 0, 90);
        foreach (var thigh in thighs) thigh.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        foreach (var shin in shins) shin.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // 목표 위치 설정
        target.position = new Vector3(Random.Range(-5f, 5f)-45f, 0.5f, Random.Range(-5f, 5f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Body 상태: 속도만 추가
        sensor.AddObservation(body.linearVelocity / 10.0f); // 3개 (x, y, z 속도)
        sensor.AddObservation(body.angularVelocity / 10.0f); // 3개 (x, y, z 각속도)

        // 목표 상태
        sensor.AddObservation((target.position - body.position) / 10.0f); // 3개 (목표와의 상대적 거리)

        // 관절 상태
        foreach (var thigh in thighs)
        {
            // Thigh 상태
            Quaternion rotation = thigh.localRotation;
            sensor.AddObservation(new Vector3(rotation.x, rotation.y, rotation.w)); // 3개 (로컬 회전)
            Vector3 angularVelocity = thigh.GetComponent<Rigidbody>().angularVelocity;
            sensor.AddObservation(new Vector2(angularVelocity.x, angularVelocity.y)); // 2개 (X, Y 각속도)
        }

        foreach (var shin in shins)
        {
            // Shin 상태
            Quaternion rotation = shin.localRotation;
            sensor.AddObservation(rotation.x); // 1개 (X축 로컬 회전)
            Vector3 angularVelocity = shin.GetComponent<Rigidbody>().angularVelocity;
            sensor.AddObservation(angularVelocity.x); // 1개 (X축 각속도)
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        // Action 적용
        // Thigh 관절 제어 (각 관절에 대해 X, Y 회전)
        for (int i = 0; i < thighs.Length; i++)
        {
            // 행동에서 연속적 값 추출
            float xRotation = actions.ContinuousActions[i * 2 + 0] * actionScale; // X축
            float yRotation = actions.ContinuousActions[i * 2 + 1] * actionScale; // Y축

            // 목표 회전 생성
            Quaternion targetRotation = Quaternion.Euler(xRotation, yRotation, 0);

            // 회전을 적용 (제한된 범위 내에서 회전)
            thighs[i].transform.localRotation = Quaternion.RotateTowards(
                thighs[i].transform.localRotation,
                targetRotation,
                Time.deltaTime * 100.0f // 회전 속도
            );
        }

        // Shin 관절 제어 (각 관절에 대해 X 회전)
        for (int i = 0; i < shins.Length; i++)
        {
            // 행동에서 연속적 값 추출
            float xRotation = actions.ContinuousActions[8 + i] * actionScale; // X축

            // 목표 회전 생성
            Quaternion targetRotation = Quaternion.Euler(xRotation, 0, 0);

            // 회전을 적용
            shins[i].transform.localRotation = Quaternion.RotateTowards(
                shins[i].transform.localRotation,
                targetRotation,
                Time.deltaTime * 100.0f // 회전 속도
            );
        }

        // 보상 계산
        // 목표까지 거리
        float distanceToTarget = Vector3.Distance(body.position, target.position);
        float distanceReward = 1.0f / (1.0f + distanceToTarget); // 역함수 기반
        AddReward(distanceReward * 0.1f);

        // 기울기 보상
        float tiltPenalty = Vector3.Dot(body.transform.up, Vector3.up); // 올바른 사용법
        if(tiltPenalty <= 0.0f){
            Debug.Log("Fall");
            SetReward(-1.0f);
            EndEpisode();
        }
        float tiltReward = Mathf.Exp((tiltPenalty - 1.0f)*10.0f) - Mathf.Exp(-1.0f*10.0f); // 기울기가 적을수록 큰 보상
        AddReward(tiltReward * 0.01f);

        // 목표 방향 보상
        float alignmentReward = Vector3.Dot(body.transform.forward.normalized, (target.position - body.position).normalized);
        AddReward(alignmentReward * 0.01f);

        // 목표 도달 보상
        if (distanceToTarget < 1.0f){
            AddReward(10f);
            EndEpisode();
        }
        // If the agent falls off the platform, end the episode
        if (body.position.y < 0.3f || body.position.y > 1.5f){
            SetReward(-1f);
            EndEpisode();
        }
        // If the agent goes to far, end the episode
        if (distanceToTarget > 100f){
            SetReward(-1f);
            EndEpisode();
        }
    }
}
