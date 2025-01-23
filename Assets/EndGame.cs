using UnityEngine;

public class EndGame : MonoBehaviour
{
     public GameObject objectToAttach; // 부착할 GameObject (Inspector에서 설정)

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("other.name: " + other.name);
        // 에이전트를 특정 태그로 식별
        if (other.CompareTag("Finish"))
        {
            // 에이전트 밑에 객체 부착
            objectToAttach.transform.parent = other.transform;

            // 부착 위치를 에이전트의 로컬 좌표로 설정
            objectToAttach.transform.localPosition = new Vector3(0.0f, 5.0f, 0); // 원하는 위치로 변경 가능
            objectToAttach.transform.localRotation = Quaternion.Euler(180, 0, 90); // 0,90,0으로 바꿈

            Debug.Log("Object attached to the agent!");
        }
    }
}
