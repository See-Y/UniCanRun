using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections;

public class QuadrupedAgent : Agent
{
    public Transform target;
    public Transform[] thighs;
    public Transform[] shins;
    public Rigidbody body;
    private Vector3 previousPosition;    // 이전 위치
    private float noMovementTimer = 0f;  // 정지 시간 측정
    private const float NO_MOVEMENT_THRESHOLD = 5f;  // 허용되는 최대 정지 시간
    private const float MIN_MOVEMENT = 0.01f;  // 최소 이동 거리
    private float actionScale = 90.0f;

    private int decisionPeriod = 5; // 행동 요청 주기 (FixedUpdate 호출 횟수 기준)
    private int stepCount = 0;      // 현재 스텝 카운트

    private const int THIGH_ACTION_SIZE = 8; // 4개 다리 * 2(X,Y축)

    void FixedUpdate()
    {
        // 타겟 움직임 업데이트
        float time = Time.time;
        float xOffset = Mathf.Sin(time * 0.0f) * 2f; // X축 움직임
        float zOffset = Mathf.Cos(time * 0.0f) * 2f; // Z축 움직임
        
        Vector3 basePosition = new Vector3(-45f, 0.5f, 0f);
        target.position = basePosition + new Vector3(xOffset, 0f, zOffset);

        // 기존 행동 요청 로직
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
        // 1. 물리 초기화 순서 변경
        foreach (var thigh in thighs)
        {
            var rb = thigh.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        foreach (var shin in shins)
        {
            var rb = shin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 2. 메인 바디 초기화
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        
        // 3. 위치와 회전 초기화 (한 프레임 대기)
        StartCoroutine(DelayedPositionReset());
    }

    private IEnumerator DelayedPositionReset()
    {
        // 한 물리 프레임 대기
        yield return new WaitForFixedUpdate();
        
        // 위치 초기화
        body.MovePosition(new Vector3(45f, 1, 0));
        body.MoveRotation(Quaternion.Euler(0, 180, 0));
        
        // 관절 초기화
        foreach (var thigh in thighs)
        {
            thigh.localRotation = Quaternion.Euler(0, 0, 90);
            var joint = thigh.GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                joint.targetRotation = Quaternion.Euler(0, 0, 90);
            }
        }
        
        foreach (var shin in shins)
        {
            shin.localRotation = Quaternion.Euler(0, 0, 90);
            var joint = shin.GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                joint.targetRotation = Quaternion.Euler(0, 0, 90);
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log("CollectObservations");
        // Body 상태
        sensor.AddObservation(body.linearVelocity / 10.0f); // 3개 (x, y, z 속도)
        sensor.AddObservation(body.angularVelocity / 10.0f); // 3개 (x, y, z 각속도)
        sensor.AddObservation(body.transform.up); // 3개 (균형 상태)

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
        // Thigh 관절 제어 (각 관절에 대해 X, Y 회전)
        for (int i = 0; i < thighs.Length; i++)
        {
            int baseIndex = i * 2;
            float xRotation = actions.ContinuousActions[baseIndex] * actionScale;
            float yRotation = actions.ContinuousActions[baseIndex + 1] * actionScale / 9;

            Quaternion targetRotation = Quaternion.Euler(xRotation, yRotation, 0);
            ConfigurableJoint joint = thighs[i].GetComponent<ConfigurableJoint>();
            joint.targetRotation = targetRotation;
        }

        // Shin 관절 제어 (각 관절에 대해 X 회전)
        for (int i = 0; i < shins.Length; i++)
        {
            int shinIndex = THIGH_ACTION_SIZE + i;
            float xRotation = actions.ContinuousActions[shinIndex] * actionScale;

            Quaternion targetRotation = Quaternion.Euler(xRotation, 0, 0);
            ConfigurableJoint joint = shins[i].GetComponent<ConfigurableJoint>();
            joint.targetRotation = targetRotation;
        }

        // 1. 기본적인 생존 보상 (매 스텝마다 작은 양수)
        AddReward(0.01f);

        // 2. 균형 보상 (기존보다 강화)
        float tiltPenalty = Vector3.Dot(body.transform.up, Vector3.up);
        if(tiltPenalty <= 0.1f){
            AddReward(-1.0f);
            EndEpisode();
        }
        float tiltReward = Mathf.Exp((tiltPenalty - 1.0f)*5.0f) - Mathf.Exp(-1.0f*5.0f);
        AddReward(tiltReward * 0.5f);

        // 3. 진행 보상 (목표를 향한 이동)
        Vector3 targetDirection = (target.position - body.position).normalized;
        // 목표 방향에 대한 보상
        float dotProduct = Vector3.Dot(body.transform.right, targetDirection);  // forward 대신 -right 사용
        float targetReward = dotProduct;  // -1 to 1 범위의 선형 보상
        if(dotProduct <= 0.2f){
            AddReward(-1.0f);
            //EndEpisode();
        }
        AddReward(targetReward * 0.5f);
        
        // 목표 방향으로의 이동 속도 보상
        float progressReward = Vector3.Dot(body.linearVelocity.normalized, targetDirection) * body.linearVelocity.magnitude;
        AddReward(progressReward*2.0f);  // 0.05f -> 0.01f
        //Debug.Log("progressReward: " + progressReward);

        // 거리에 따른 보상
        float distanceToTarget = Vector3.Distance(body.position, target.position);
        float distanceReward = Mathf.Exp(-distanceToTarget / 20.0f) - Mathf.Exp(-10.0f);
        AddReward(distanceReward); // 0.5f -> 0.8f

        float movement = Vector3.Distance(previousPosition, body.position);
        if (movement <= MIN_MOVEMENT)  // 충분한 움직임이 없는 경우
        {
            noMovementTimer += Time.fixedDeltaTime;
            if (noMovementTimer >= NO_MOVEMENT_THRESHOLD)
            {
                AddReward(-1f);  // 큰 페널티
                //EndEpisode();
                return;
            }
        }
        else
        {
            noMovementTimer = 0f;  // 움직임이 있으면 타이머 리셋
        }
        previousPosition = body.position;


        // 4. 에너지 효율성 보상 (과도한 동작 억제)
        // float energyPenalty = 0f;
        // foreach (var thigh in thighs)
        // {
        //     energyPenalty += thigh.GetComponent<Rigidbody>().angularVelocity.magnitude;
        //     break;
        // }
        // foreach (var shin in shins)
        // {
        //     energyPenalty += shin.GetComponent<Rigidbody>().angularVelocity.magnitude;
        //     break;
        // }
        // AddReward(energyPenalty);

        // 5. 목표 도달 보상 (성공)
        if (distanceToTarget < 1.0f){
            AddReward(10.0f);
            EndEpisode();
        }

        // 6. 실패 조건
        if (body.position.y < 0.5f || body.position.y > 1.5f){
            AddReward(-1f);
            EndEpisode();
        }
        if (distanceToTarget > 100f){
            SetReward(-1f);
            EndEpisode();
        }

        // thigh의 y가 -1.4보다 작으면 실패
        foreach (var thigh in thighs)
        {
            if (thigh.localPosition.y < -1.4f)
            {
                AddReward(-1f);
                EndEpisode();
            }
        }
    }
}

