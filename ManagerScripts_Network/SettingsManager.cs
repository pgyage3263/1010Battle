using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace NetworkScripts {
    public class SettingsManager : MonoBehaviourPunCallbacks
    {
        public AudioClip enter;
        public AudioClip quit;
        public GameObject SettingUI;
        AudioSource myAudio;
        private void Start()
        {
            myAudio = GetComponent<AudioSource>();
        }
        public void ShowSettingUI(bool isShow)
        {
            PlaySound(isShow);
            SettingUI.SetActive(isShow);
        }
        //항복
        public void GiveUp()
        {
            PlaySound(true);
            int winnerNum = (NetworkManager.Instance.myPlayerNum == 0) ? 1 : 0;
            photonView.RPC("GameEnd", RpcTarget.AllBuffered, winnerNum);
        }
        [PunRPC]
        public void GameEnd(int winnerNum)
        {
            int loseNum = (winnerNum == 0) ? 1 : 0;
            HPManager.Instance.PlayDieFX(loseNum);
            //네트워크로 보내기
            TurnManager.Instance.GameEnd(winnerNum);
        }
        //게임 종료
        public void ExitGame()
        {
            PlaySound(true);
            PhotonNetwork.Disconnect();
            Application.Quit();
        }
        public void PlaySound(bool isEnter)
        {
            if (GameManager.Instance.sfxState == true)
            {
                if (isEnter)
                {
                    myAudio.clip = enter;
                }
                else
                {
                    myAudio.clip = quit;
                }
                myAudio.Play();
            }
        }
    }
}