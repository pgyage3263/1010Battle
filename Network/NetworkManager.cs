using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
namespace NetworkScripts {
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        public int myPlayerNum = 0;
        public GameObject canvas;
        // Start is called before the first frame update
        void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                myPlayerNum = 0;
                //방 룸리스트에서 안보이게
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            else
            {
                myPlayerNum = 1;
                //화면 돌리기
                Camera.main.transform.Rotate(new Vector3(0, 0, 180), Space.Self);
                canvas.transform.Rotate(new Vector3(0, 0, 180), Space.Self);
            }
            //Create
            BlockCreateManager.Instance.Init();
        }
        //상대방이 나갔을 경우
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            TurnManager.Instance.GameEnd(myPlayerNum);
        }
        GameObject errorPanel;
        //포톤 서버와 연결이 끊겼을 경우 -> 메인 네트워크로 이동
        public override void OnDisconnected(DisconnectCause cause)
        {
            //동접으로 끊어진게아니고 게임이 끝난 상태가 아닐 때
            if (!TurnManager.Instance.IsEnd() && !GameManager.Instance.CheckError())
            {
                GameManager.Instance.SetGameState(GameManager.GameState.Error);
                GameManager.Instance.SetUser(null);
                SceneManager.LoadScene(0);
                base.OnDisconnected(cause);
            }
        }
    }
}
