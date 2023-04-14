using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NetworkScripts
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }
        //public AudioClip myTurnAudio;
        //public AudioClip enemyTurnAudio;
        public AudioClip winAudio;
        public AudioClip loseAudio;
        AudioSource myAudio;
        float originVol;
        private void Start()
        {
            myAudio = GetComponent<AudioSource>();
            originVol = myAudio.volume;
        }
        public void PlayBGM(bool isMyTurn)
        {
            //if (isMyTurn)
            //{
            //    StartCoroutine("ChangeAudio", myTurnAudio);
            //    ChangeAudio(myTurnAudio);
            //}
            //else
            //{
            //    StartCoroutine("ChangeAudio", enemyTurnAudio);
            //}
            //myAudio.Play();
        }
        IEnumerator ChangeAudio(AudioClip after)
        {
            while (myAudio.volume > 0.01f)
            {
                myAudio.volume -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            myAudio.clip = after;
            myAudio.volume = originVol;
            myAudio.Play();
        }
        public void EndGame(bool isWin)
        {
            if (GameManager.Instance.sfxState == true)
            {
                myAudio.volume = 1.0f;
                if (isWin) myAudio.clip = winAudio;
                else myAudio.clip = loseAudio;
                myAudio.Play();
            }
        }
    }
}
