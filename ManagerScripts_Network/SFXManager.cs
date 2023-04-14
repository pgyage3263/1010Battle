using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NetworkScripts
{
    public class SFXManager : MonoBehaviour
    {
        public static SFXManager Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
        bool isEnd = false;
        public AudioClip blockLandAudio;
        public AudioClip LineClearAudio;
        public AudioClip TurnChangeAudio;
        public AudioClip GameEndSound;
        public AudioClip PlayerGageSound;
        public AudioClip GameStartSound;
        AudioSource myAudio;
        private void Start()
        {
            myAudio = GetComponent<AudioSource>();
        }
        public void OnBlockLand()
        {
            if (isEnd == false && GameManager.Instance.sfxState == true)
            {
                myAudio.volume = 0.5f;
                myAudio.clip = blockLandAudio;
                myAudio.Play();
            }
        }
        public void OnTurnChange()
        {
            if (isEnd == false && GameManager.Instance.sfxState == true)
            {
                myAudio.volume = 0.6f;
                myAudio.clip = TurnChangeAudio;
                myAudio.Play();
            }
        }
        public void OnGameStart()
        {
            if (GameManager.Instance.sfxState == true)
            {
                myAudio.volume = 1.0f;
                myAudio.clip = GameStartSound;
                myAudio.Play();
            }
        }
        public void OnPlayerDeadGage()
        {
            if (GameManager.Instance.sfxState == true)
            {
                isEnd = true;
                myAudio.volume = 1.0f;
                myAudio.clip = PlayerGageSound;
                myAudio.Play();
            }
        }
        public void OnGameEnd()
        {
            if (GameManager.Instance.sfxState == true)
            {
                myAudio.volume = 1.0f;
                myAudio.clip = GameEndSound;
                myAudio.Play();
            }
        }
        public void OnLineClear()
        {
            //myAudio.clip = LineClearAudio;
            //myAudio.Play();
        }
    }
}
