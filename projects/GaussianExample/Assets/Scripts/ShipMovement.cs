using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipMovement : MonoBehaviour
{
    // 船的移动速度
    public float moveSpeed = 5f;
    // 引用 bridgeCross 游戏对象
    public GameObject bridgeCross;
    // 引用 xiquScene 游戏对象
    public GameObject xiquScene;
    // 引用黑屏的 Image 对象
    public Image blackScreen;
    // 触发转场效果的碰撞体位置（可以根据实际情况修改）
    public Transform triggerPoint;

    // 记录开始计时的时间
    private float startTime;
    // 标记是否开始计时
    private bool isTiming = false;
    // 标记 bridgeCross 上一帧的激活状态
    private bool wasBridgeCrossActive = true;
    // 船要回到的指定位置
    private Vector3 returnPosition = new Vector3(1.87f, -0.629999995f, -0.639999986f);

    void Update()
    {
        // 检查 bridgeCross 是否刚刚关闭
        if (wasBridgeCrossActive && !bridgeCross.activeSelf)
        {
            // 将船移动到指定位置
            transform.position = returnPosition;
            // 重置计时状态
            isTiming = false;
        }

        // 更新 bridgeCross 上一帧的激活状态
        wasBridgeCrossActive = bridgeCross.activeSelf;

        // 检查 bridgeCross 是否隐藏且 xiquScene 是否激活
        if (!bridgeCross.activeSelf && xiquScene.activeSelf)
        {
            if (!isTiming)
            {
                // 开始计时
                startTime = Time.time;
                isTiming = true;
            }

            // 计算已经过去的时间
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < 3f)
            {
                // 在三秒内继续移动
                MoveShip();
            }
            else
            {
                // 三秒后停止移动
                StopShip();
            }
        }
        else
        {
            // 其他情况匀速行驶
            isTiming = false;
            MoveShip();
        }
    }

    // 船移动的方法
    void MoveShip()
    {
        // 让船沿着 X 轴负方向移动
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }

    // 船停止的方法
    void StopShip()
    {
        // 这里可以添加其他停止相关的逻辑，例如将速度设为 0
    }

    // 触发转场效果的方法
    void OnTriggerEnter(Collider other)
    {
        if (other.transform == triggerPoint)
        {
            StartCoroutine(TransitionEffect());
        }
    }

    // 转场效果的协程
    IEnumerator TransitionEffect()
    {
        // 显示黑屏
        blackScreen.enabled = true;
        // 关闭 bridgeCross 场景
        bridgeCross.SetActive(false);
        // 等待 1.5 秒
        yield return new WaitForSeconds(1.5f);
        // 打开 xiquScene 场景
        xiquScene.SetActive(true);
        // 隐藏黑屏
        blackScreen.enabled = false;
    }
}