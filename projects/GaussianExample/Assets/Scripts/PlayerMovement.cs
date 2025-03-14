using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // ����ƶ��ٶ�
    public float moveSpeed = 5f;
    // ���������
    public float mouseSensitivity = 100f;
    // ����� Transform
    public Transform cameraTransform;
    // �����������ת��
    private float xRotation = 0f;

    void Start()
    {
        // �������
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // �������ӽ�
        HandleMouseLook();

        // ���̿����ƶ�
        HandleMovement();
    }

    void HandleMouseLook()
    {
        // ��ȡ�������
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ���������������ת
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // ������ת�Ƕ�
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // �������������ת
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // ��ȡ�������루WASD��
        float moveX = Input.GetAxis("Horizontal"); // ����
        float moveZ = Input.GetAxis("Vertical");    // ǰ��

        // �����ƶ����򣨻�����ҵı�������ϵ��
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // ƽ�����
        transform.Translate(move * moveSpeed * Time.deltaTime);
    }
}