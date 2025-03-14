using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipMovement : MonoBehaviour
{
    // �����ƶ��ٶ�
    public float moveSpeed = 5f;
    // ���� bridgeCross ��Ϸ����
    public GameObject bridgeCross;
    // ���� xiquScene ��Ϸ����
    public GameObject xiquScene;
    // ���ú����� Image ����
    public Image blackScreen;
    // ����ת��Ч������ײ��λ�ã����Ը���ʵ������޸ģ�
    public Transform triggerPoint;

    // ��¼��ʼ��ʱ��ʱ��
    private float startTime;
    // ����Ƿ�ʼ��ʱ
    private bool isTiming = false;
    // ��� bridgeCross ��һ֡�ļ���״̬
    private bool wasBridgeCrossActive = true;
    // ��Ҫ�ص���ָ��λ��
    private Vector3 returnPosition = new Vector3(1.87f, -0.629999995f, -0.639999986f);

    void Update()
    {
        // ��� bridgeCross �Ƿ�ոչر�
        if (wasBridgeCrossActive && !bridgeCross.activeSelf)
        {
            // �����ƶ���ָ��λ��
            transform.position = returnPosition;
            // ���ü�ʱ״̬
            isTiming = false;
        }

        // ���� bridgeCross ��һ֡�ļ���״̬
        wasBridgeCrossActive = bridgeCross.activeSelf;

        // ��� bridgeCross �Ƿ������� xiquScene �Ƿ񼤻�
        if (!bridgeCross.activeSelf && xiquScene.activeSelf)
        {
            if (!isTiming)
            {
                // ��ʼ��ʱ
                startTime = Time.time;
                isTiming = true;
            }

            // �����Ѿ���ȥ��ʱ��
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < 3f)
            {
                // �������ڼ����ƶ�
                MoveShip();
            }
            else
            {
                // �����ֹͣ�ƶ�
                StopShip();
            }
        }
        else
        {
            // �������������ʻ
            isTiming = false;
            MoveShip();
        }
    }

    // ���ƶ��ķ���
    void MoveShip()
    {
        // �ô����� X �Ḻ�����ƶ�
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }

    // ��ֹͣ�ķ���
    void StopShip()
    {
        // ��������������ֹͣ��ص��߼������罫�ٶ���Ϊ 0
    }

    // ����ת��Ч���ķ���
    void OnTriggerEnter(Collider other)
    {
        if (other.transform == triggerPoint)
        {
            StartCoroutine(TransitionEffect());
        }
    }

    // ת��Ч����Э��
    IEnumerator TransitionEffect()
    {
        // ��ʾ����
        blackScreen.enabled = true;
        // �ر� bridgeCross ����
        bridgeCross.SetActive(false);
        // �ȴ� 1.5 ��
        yield return new WaitForSeconds(1.5f);
        // �� xiquScene ����
        xiquScene.SetActive(true);
        // ���غ���
        blackScreen.enabled = false;
    }
}