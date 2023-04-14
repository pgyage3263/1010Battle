using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainCameraResolution : MonoBehaviour
{
    CanvasScaler cs;
    // Start is called before the first frame update
    void Start()
    {
        float wdh = 2.0f / 3.0f;
        cs = GetComponent<CanvasScaler>();
        if((float)Screen.width/Screen.height > wdh)
        {
            cs.referenceResolution = new Vector2(1200, 900);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
