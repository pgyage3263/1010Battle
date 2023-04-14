using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//스크린에 맞춰 Camera의 사이즈를 조절한다.(화면 짤림방지)
public class CameraInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float factor = 0.0f, camSize = 0.0f, res = 0.0f;
        res = (float)Screen.height / Screen.width;
        if (res > 16.0f / 9.0f)
        {
            factor = 45.0f / 16.0f;
            camSize = res * factor;
            GetComponent<Camera>().orthographicSize = camSize;
        }
    }
}
