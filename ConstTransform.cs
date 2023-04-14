using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstTransform : MonoBehaviour
{
    Vector3 originPos;
    // Start is called before the first frame update
    void Start()
    {
        originPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = originPos;
    }
}
