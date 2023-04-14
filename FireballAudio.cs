using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballAudio : MonoBehaviour
{
    AudioSource myAudio;
    // Start is called before the first frame update
    void Start()
    {
        myAudio = GetComponent<AudioSource>();
        if(GameManager.Instance.sfxState == true)
        {
            myAudio.Play();
        }   
    }
    private void OnEnable()
    {
        myAudio = GetComponent<AudioSource>();
        if (GameManager.Instance.sfxState == true)
        {
            myAudio.Play();
        }
    }
}
