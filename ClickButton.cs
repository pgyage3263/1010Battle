using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickButton : MonoBehaviour
{
    public AudioClip openSound;
    public AudioClip closeSound;
    AudioSource myAudio;
    private void Start()
    {
        myAudio = GetComponent<AudioSource>();
    }
    public void PlayBtnSound(bool isOpen)
    {
        if (GameManager.Instance.sfxState == true)
        {
            if (isOpen) myAudio.clip = openSound;
            else myAudio.clip = closeSound;
            myAudio.Play();
        }
    }
}
