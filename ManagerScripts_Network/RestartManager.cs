using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace NetworkScripts
{
    public class RestartManager : MonoBehaviourPunCallbacks
    {
        public static RestartManager Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            restartBtn.interactable = false;
            ExitBtn.interactable = false;
        }
        public void OnFinish()
        {
            restartBtn.interactable = true;
            ExitBtn.interactable = true;
            isFinish = true;
        }
        bool isFinish = false;
        bool isReq = false;
        bool isEnemyReq = false;
        //UI텍스트
        public Text uiText;
        //확인 버튼
        //재시작 버튼
        public Button restartBtn;
        public Button ExitBtn;
        //자신 -> 상대 재시작 요청
        public void RequestRestart()
        {
            isReq = true;
            //RPC뿌리기
            photonView.RPC("RequestEnemy", RpcTarget.OthersBuffered);
            restartBtn.interactable = false;
            CheckRestart();
            //UI띄우기
            uiText.gameObject.SetActive(true);
            uiText.text = "상대방에게 재대결을 요청했습니다.";
        }
        //나가기
        public void ExitGame()
        {
            PhotonNetwork.LoadLevel(0);
        }
        //상대 -> 자신 재시작 요청
        [PunRPC]
        public void RequestEnemy()
        {
            isEnemyReq = true;
            CheckRestart();
            //UI띄우기
            uiText.gameObject.SetActive(true);
            uiText.text = "상대방이 재대결을 요청했습니다.";
        }
        //상대가 나갔을 경우
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (isFinish)
            {
                base.OnPlayerLeftRoom(otherPlayer);
                isEnemyReq = false;
                //UI띄우기
                uiText.gameObject.SetActive(true);
                uiText.text = "상대방이 방을 나갔습니다.";
                restartBtn.interactable = false;
            }
        }
        //체크
        void CheckRestart()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (isReq && isEnemyReq)
                {
                    PhotonNetwork.SetMasterClient(PhotonNetwork.PlayerListOthers[0]);
                    photonView.RPC("Restart", RpcTarget.AllBufferedViaServer);
                }
            }
        }
        bool isRestart = false;
        //재시작
        [PunRPC]
        public void Restart()
        {
            if (isRestart == false)
            {
                isRestart = true;
                PhotonNetwork.LoadLevel(1);
            }
        }
    }
}
