using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Proyecto26;
using UnityEngine.UI;
public class WaitingRoomManager : MonoBehaviourPunCallbacks
{
    //싱글톤
    public static WaitingRoomManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    //레벨 텍스트
    public Text[] levelText;
    //레이팅 텍스트
    public Text[] ratingText;
    //닉네임 텍스트
    public Text[] nickNameText;
    //프로필 이미지
    public Image[] profileImages;
    //로딩화면
    public GameObject[] loadingScreens;
    //방 텍스트
    public Text roomName;
    //Ready버튼
    public Button readyBtn;
    //Ready취소 버튼
    public Button readyCancelBtn;
    //Start버튼
    public Button startBtn;
    //Ready 표시
    public GameObject readyImg;
    //Start표시
    public GameObject startImg;
    //Chating
    public InputField chatInput;
    public Text chat;
    public RectTransform contentTransform;
    public ScrollRect chatSR;

    public GameObject loadingPanel;
    bool isMaster = false;

    //2P ready 유무
    bool ready_2p = false;
    //모두 초기화
    public void InitAll()
    {
        //채팅 초기화
        chatInput.text = "";
        chat.text = "";
        contentTransform.localPosition = new Vector3(contentTransform.localPosition.x, 0, contentTransform.localPosition.z);

        for (int i = 0; i < 2; i++)
        {
            levelText[i].text = "";
            ratingText[i].text = "";
            nickNameText[i].text = "";
            profileImages[i].sprite = null;
            profileImages[i].color = new Color(0, 0, 0, 0);
            loadingScreens[i].SetActive(false);
        }
        ready_2p = false;
        roomName.text = "";
        readyBtn.gameObject.SetActive(false);
        readyCancelBtn.gameObject.SetActive(false);
        startBtn.gameObject.SetActive(false);
        startImg.SetActive(false);
        readyImg.SetActive(false);
    }
    //플레이어의 정보를 가져옴.
    void GetPlayerInfo(int playerNum, string enemyID)
    {
        //로딩화면 띄우기
        loadingScreens[playerNum].SetActive(true);
        RestClient.Get<User>(url: "https://battle1010.firebaseio.com/users/" + enemyID + ".json").Then(response =>
         {
             User enemy = response;
             //레벨 넣기
             levelText[playerNum].text = "LV." + enemy.level;
             //레이팅 넣기
             ratingText[playerNum].text = ((int)enemy.rating).ToString();
             //닉네임 넣기
             nickNameText[playerNum].text = enemy.nickName;
             //프로필 넣기
             profileImages[playerNum].color = Color.white;
             profileImages[playerNum].sprite = ProfileManager.Instance.profileImages[enemy.profileNum];

             //로딩화면 제거
             loadingScreens[playerNum].SetActive(false);
         }
    );
    }
    //방장인 상태로 다른 플레이어가 들어왔을 경우
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        //방장일 경우
        if (PhotonNetwork.IsMasterClient)
        {
            //두번째 setup
            //id
            string enemyID = newPlayer.NickName.Split('_')[1];
            //조회 및 적용
            GetPlayerInfo(1, enemyID);

        }
    }
    //2P 입장에서 방장 정보 조회
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        roomName.text = PhotonNetwork.CurrentRoom.Name.Split('_')[0];
        //2P일 경우
        if (PhotonNetwork.IsMasterClient == false)
        {
            //1P
            isMaster = false;
            //두번째 setup
            //id
            string enemyID = PhotonNetwork.PlayerListOthers[0].NickName.Split('_')[1];
            //조회 및 적용
            GetPlayerInfo(0, enemyID);
            //start버튼 비활성화
            startBtn.gameObject.SetActive(false);
            //Ready버튼 활성화
            readyBtn.gameObject.SetActive(true);
            readyCancelBtn.gameObject.SetActive(false);
            //자신의 정보 기입
            User me = GameManager.Instance.GetUser();
            //레벨 넣기
            levelText[1].text = "LV." + me.level;
            //레이팅 넣기
            ratingText[1].text = ((int)me.rating).ToString();
            //닉네임 넣기
            nickNameText[1].text = me.nickName;
            //프로필 넣기
            profileImages[1].color = Color.white;
            profileImages[1].sprite = ProfileManager.Instance.profileImages[me.profileNum];
        }
        //1P일 경우
        else
        {
            //1P
            isMaster = true;
            //Ready버튼 비활성화
            readyBtn.gameObject.SetActive(false);
            readyCancelBtn.gameObject.SetActive(false);
            startBtn.gameObject.SetActive(true);
            startBtn.interactable = false;


            //자신의 정보 기입
            User me = GameManager.Instance.GetUser();
            //레벨 넣기
            levelText[0].text = "LV." + me.level;
            //레이팅 넣기
            ratingText[0].text = ((int)me.rating).ToString();
            //닉네임 넣기
            nickNameText[0].text = me.nickName;
            //프로필 넣기
            profileImages[0].color = Color.white;
            profileImages[0].sprite = ProfileManager.Instance.profileImages[me.profileNum];
        }
    }
    //레디했을 경우는 나가지 못하게하기
    public void LeaveRoom()
    {
        if (ready_2p == true && !PhotonNetwork.IsMasterClient)
        {
            MainNetworkManager.Instance.NoticeInfo("READY 상태에서는 방을 나갈 수 없습니다.");
            return;
        }
        PhotonNetwork.LeaveRoom();
        //InitAll();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (isMaster == false)
        {
            PhotonNetwork.LeaveRoom();
            //InitAll();
            MainNetworkManager.Instance.NoticeInfo("방장이 방을 나갔습니다.");
        }
        else
        {
            //2P 흔적 초기화
            ready_2p = false;
            levelText[1].text = "";
            ratingText[1].text = "";
            nickNameText[1].text = "";
            profileImages[1].sprite = null;
            profileImages[1].color = new Color(0, 0, 0, 0);
            loadingScreens[1].SetActive(false);
            startBtn.interactable = false;
            startImg.SetActive(false);
            readyImg.SetActive(false);
        }
    }
    //레디
    public void OnClickGameReady(bool isReady)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("Ready", RpcTarget.Others, isReady);
            ready_2p = isReady;
            if (isReady == true)
            {
                //레디버튼 대신 레디취소버튼
                readyBtn.gameObject.SetActive(false);
                readyCancelBtn.gameObject.SetActive(true);
                readyImg.SetActive(true);
            }
            else
            {
                //레디취소버튼 대신 레디버튼
                readyBtn.gameObject.SetActive(true);
                readyCancelBtn.gameObject.SetActive(false);
                readyImg.SetActive(false);
            }
        }
    }
    [PunRPC]
    public void Ready(bool isReady)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ready_2p = isReady;
            startBtn.interactable = isReady;
            readyImg.SetActive(isReady);
        }
    }
    //스타트
    public void OnClickGameStartBtn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("GoToInGame", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void GoToInGame()
    {
        loadingPanel.SetActive(true);
        startImg.SetActive(true);
        PhotonNetwork.LoadLevel(1);
    }


    //채팅
    public void SendChat()
    {
        //대화가 있는지 확인
        if (chatInput.text.Equals(""))
        {
            return;
        }
        string myMessage = "<color=\"#ce6730\">" + GameManager.Instance.GetUser().nickName + ":</color>" + chatInput.text;
        photonView.RPC("ReceiveChat", RpcTarget.All, myMessage);
        //초기화
        chatInput.text = "";
    }
    [PunRPC]
    public void ReceiveChat(string msg)
    {
        //내용 추가
        chat.text += "\n" + msg;
        //위치
        //Vector3 origin = contentTransform.localPosition;
        //origin.y += 100;
        //contentTransform.localPosition = origin;
        StartCoroutine("ScrollBottom");


    }
    IEnumerator ScrollBottom()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        chatSR.normalizedPosition = new Vector2(0, 0);

    }
}
