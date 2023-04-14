using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
//메인씬의 봇
//역할
//1. 로비 입장시 공개방생성(자신의 닉네임을 이름으로)
//2. 상대방이 Ready시 Start
public class MainBot : MonoBehaviourPunCallbacks
{
    public static MainBot Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    //공개방 생성
    public void CreateBotRoom(string roomName)
    {
        //최대 2인
        RoomOptions ro = new RoomOptions
        {
            MaxPlayers = 2
        };
        PhotonNetwork.CreateRoom(roomName, ro, TypedLobby.Default);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        string roomName = GameManager.Instance.GetUser().nickName;

        CreateBotRoom("초급봇" + Random.Range(0, 100));
    }
    //Ready시 Start
    public void OnOtherReady()
    {
        WaitingRoomManager.Instance.OnClickGameStartBtn();
    }
}
