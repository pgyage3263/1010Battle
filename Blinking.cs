using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Blinking : MonoBehaviour
{
    float currentTime = 0.0f;
    public float timeDelay = 0.5f;
    public Text myText;
    // Start is called before the first frame update
    void Start()
    {
        myText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime < timeDelay)
        {
            if(!myText.enabled)
                myText.enabled = true;
        }
        else if (currentTime > 1.0f)
        {
            currentTime = 0.0f;
        }
        else
        {
            if (myText.enabled)
                myText.enabled = false;
        }
    }
}
