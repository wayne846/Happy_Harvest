using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 我們要跟蹤的目標 (也就是玩家)
    public float smoothSpeed = 0.125f; // 平滑移動的數值 (0~1)，越小越延遲，越大越黏
    public Vector3 offset; // 攝影機與玩家的距離偏差 (通常是 Z 軸距離)

    void LateUpdate() // LateUpdate 確保在玩家移動算完之後，攝影機才動，避免畫面抖動
    {
        if (target == null) return;

        // 我們希望攝影機去的目標位置 (保留攝影機原本的 Z 軸，不然會穿過地圖看不到東西)
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

        // 簡單的平滑移動 (Lerp)
        // 如果你不想要平滑效果，想死死黏著玩家，就把下面這行改成 transform.position = desiredPosition;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}