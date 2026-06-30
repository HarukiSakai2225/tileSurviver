using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    Transform tf;
    Camera cam;
    public PlayerObject playerObject;
    public float zoomSensitivity;

    // 追加: ズームの目標値
    public float targetOrthographicSize;

    
    public GameObject targetObj;
    private Vector3 targetPosition;
    Vector2 newPos;

    void Start()
    {
        tf = this.gameObject.GetComponent<Transform>();
        cam = this.gameObject.GetComponent<Camera>();
        playerObject = FindObjectOfType<PlayerObject>();

        // 初期ズームレベルを設定
        targetOrthographicSize = cam.orthographicSize;
    }

    public void SetTarget(GameObject target)
    {
        targetObj = target;
    }

    void FixedUpdate()
{

    // Mathf.Lerpを使用してズームを滑らかに変更
    cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthographicSize, Time.deltaTime * 5f);

    newPos = targetObj.transform.localPosition;
    targetPosition = new Vector3(newPos.x, newPos.y, -1);

    // Mathf.Lerpを使用してカメラ位置を滑らかに更新
    tf.position = Vector3.Lerp(tf.position, targetPosition, Time.deltaTime * 5f);
}

    void Update()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
    // ここでズーム速度を調整するためにDeltaTimeを使用
    targetOrthographicSize -= scrollInput * zoomSensitivity; // 乗数を調整して適切なズーム速度を見つけます
    targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, 1f, 25f);
    }

}
