using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    // �����ƶ��ٶ�
    public float moveSpeed = 5f;

    void Update()
    {
        // �ô������������ Z ��������ͨ������ǰ���ƶ�
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }
}