using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Proyecto26;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
public class MainNetworkManager : MonoBehaviourPunCallbacks
{
    //버전
    public string version;
    //최대 동시접속자 수
    public int maxPlayerCount = 100;
    //메인, 로그인, 회원가입, 룸리스트, 대기방 순
    public GameObject[] panels;
    //로그인시
    public InputField loginIdInput;
    public InputField loginPwInput;
    public Toggle loginToggle;
    //회원가입시
    public InputField regIdInput;
    public InputField regPwInput;
    public InputField regPwInputConfirm;
    public InputField regNickNameInput;
    public InputField regEMailInput;
    //상태 알리는 텍스트
    public Text consoleText;
    //로딩 패널
    public GameObject loadingPanel;
    //싱글톤
    public static MainNetworkManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.CheckInRoom())
        {
            loadingPanel.SetActive(true);
            UpdateUser();
            //룸리스트로 이동
            WaitingRoomManager.Instance.LeaveRoom();
        }
        else if (GameManager.Instance.CheckError())
        {
            NoticeInfo("오류가 발생했습니다.");
        }
    }
    private void Update()
    {
        //실시간 유저 수 반영
        if (PhotonNetwork.IsConnected)
        {
            currentUserCount.text = PhotonNetwork.CountOfPlayers.ToString();
        }
    }
    void UpdateUser()
    {
        RestClient.Get<User>(url: "https://battle1010.firebaseio.com/users/" + GameManager.Instance.myID + ".json").Then(response =>
        {
            User myUser = response;
            GameManager.Instance.SetUser(myUser);
            playerStat.text = (myUser.winCount + myUser.loseCount) + "전 " + myUser.winCount + "승 " + myUser.loseCount + "패";
            playerLevel.text = "LV." + myUser.level.ToString();
            playerRating.text = ((int)myUser.rating).ToString();
        }
        );
    }
    public void NoticeInfo(string info)
    {
        consoleText.text = "SYSTEM: " + info;
    }
    //SHA256
    public string SHA256Hash(string data)
    {
        SHA256 sha = new SHA256Managed();
        byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte b in hash)
        {
            stringBuilder.AppendFormat("{0:x2}", b);
        }
        return stringBuilder.ToString();

    }
    string initialNotice;
    void CheckVersion(string id, string pw)
    {
        loadingPanel.SetActive(true);
        NoticeInfo("버전 체크중");
        RestClient.Get<Version>(url: "https://battle1010.firebaseio.com/version.json").Then(response =>
        {
            Version v = response;

            if (version.Equals(v.version))
            {
                loadingPanel.SetActive(false);
                //특정 조건 만족시
                if (id.Equals("wooring") || id.Equals("testkun"))
                {
                    CheckMessage(id, pw);
                }
                else
                {
                    LoginCheck(id, pw);
                }

                initialNotice = v.notice;
            }
            else
            {
                NoticeInfo(v.message);
                loadingPanel.SetActive(false);
            }
        }
        );
    }
    //메세지 갖고오기
    void CheckMessage(string id, string pw)
    {
        string date = System.DateTime.Now.ToString("yyyyMMdd");
        //print(date);
        loadingPanel.SetActive(true);
        RestClient.Get<JsonString>(url: "https://battle1010.firebaseio.com/messages/" + date + ".json").Then(response =>
            {
                if (response == null)
                {
                    initialNotice = "오늘은 메세지가 없어요 ㅠ.ㅠ";
                }
                else
                {
                    string message = response.message;
                    initialNotice = message;
                }
                loadingPanel.SetActive(false);
                LoginCheck(id, pw);
            }
        );
    }
    void CheckVersion(User user)
    {
        loadingPanel.SetActive(true);
        NoticeInfo("버전 체크중");
        RestClient.Get<Version>(url: "https://battle1010.firebaseio.com/version.json").Then(response =>
        {
            Version v = response;

            if (version.Equals(v.version))
            {
                loadingPanel.SetActive(false);
                RegisterCheck(user);
                initialNotice = v.notice;
            }
            else
            {
                NoticeInfo(v.message);
                loadingPanel.SetActive(false);
            }
        }
       );
    }
    //서버로부터 User 객체 갖고옴
    void LoginCheck(string id, string pw)
    {
        string hashedPW = SHA256Hash(pw);
        loadingPanel.SetActive(true);
        NoticeInfo("로그인 중");
        RestClient.Get<User>(url: "https://battle1010.firebaseio.com/users/" + id + ".json").Then(response =>
        {
            User user = response;
            //user가 들어오지 못했다면 계정이 없는 것
            if (user == null)
            {
                NoticeInfo("로그인 실패(계정 오류)");
                loadingPanel.SetActive(false);
                return;
            }
            //로그인 확인
            else if (user.password.Equals(hashedPW))
            {
                if (loginToggle.isOn == true)
                {
                    PlayerPrefs.SetString("myID", id);
                    PlayerPrefs.SetString("myPW", pw);
                }
                else
                {
                    PlayerPrefs.SetString("myID", "");
                    PlayerPrefs.SetString("myPW", "");
                }
                NoticeInfo("로그인 성공");
                GameManager.Instance.SetUser(user);
                loadingPanel.SetActive(false);
                ConnectServer();
            }
            else
            {
                NoticeInfo("로그인 실패(패스워드 오류)");
                loadingPanel.SetActive(false);
                return;
            }
        }
        );
    }
    void RegisterNickNameCheck(User newUser)
    {
        RestClient.Get<NickName>(url: "https://battle1010.firebaseio.com/nickNames/" + newUser.nickName + ".json").Then(response =>
        {
            NickName nameConfirm = response;
            //등록되지 않은 닉네임의 경우
            if (nameConfirm == null)
            {
                NickName myNickName = new NickName();
                //닉네임 넣기
                RestClient.Put(url: "https://battle1010.firebaseio.com/nickNames/" + newUser.nickName + ".json", myNickName);
                //계정 넣기 -> SetUser와 중첩되는부분.
                //RestClient.Put(url: "https://battle1010.firebaseio.com/users/" + newUser.id + ".json", newUser);
                NoticeInfo("회원가입 완료. " + newUser.nickName + "님 환영합니다.");
                //어차피 여기서 계정 넣어짐
                GameManager.Instance.SetUser(newUser);
                //PlayerPrefs
                PlayerPrefs.SetInt("RegisterCount", PlayerPrefs.GetInt("RegisterCount") + 1);
                loadingPanel.SetActive(false);
                ConnectServer();
                return;
            }
            else
            {
                NoticeInfo("이미 존재하는 닉네임입니다.");
                loadingPanel.SetActive(false);
                return;
            }
        });
    }
    void RegisterCheck(User newUser)
    {
        NoticeInfo("중복 아이디 확인 중");
        loadingPanel.SetActive(true);
        RestClient.Get<User>(url: "https://battle1010.firebaseio.com/users/" + newUser.id + ".json").Then(response =>
        {
            User userConfirm = response;
            //user가 들어오지 못했다면 계정이 없는 것
            if (userConfirm == null)
            {
                NoticeInfo("중복 닉네임 확인 중");
                //닉네임 체크
                RegisterNickNameCheck(newUser);

                return;
            }
            else
            {
                NoticeInfo("이미 존재하는 ID입니다.");
                loadingPanel.SetActive(false);
                return;
            }
        }
        );
    }
    //로그인시
    public void Login()
    {
        string id = loginIdInput.text;
        string pw = loginPwInput.text;
        //id가 없을경우 뺌
        if (id.Length == 0)
        {
            NoticeInfo("ID를 입력해주세요.");
            return;
        }
        //서버와 연결하여 유효한 연결인지 확인

        //유효하지 않다면 팝업띄우고 다시 입력시킴

        //유효하다면 로그인 처리 및 룸리스트로 이동
        //LoginCheck(id, pw);
        CheckVersion(id, pw);
    }
    public void Register()
    {
        //기기별 회원가입횟수
        if (PlayerPrefs.HasKey("RegisterCount"))
        {
            if (PlayerPrefs.GetInt("RegisterCount") >= 5)
            {
#if !UNITY_EDITOR
                NoticeInfo("기기당 최대 회원가입 횟수(5회)를 초과했습니다.");
                return;
#endif
            }
        }
        else
        {
            PlayerPrefs.SetInt("RegisterCount", 0);
        }
        string id = regIdInput.text;
        string pw = regPwInput.text;
        string nickName = regNickNameInput.text;
        string eMail = regEMailInput.text;
        //아이디, 비밀번호, 닉네임 입력안되었을경우
        if (regIdInput.text.Length == 0)
        {
            NoticeInfo("아이디를 입력해주세요.");
            return;
        }
        if (regPwInput.text.Length == 0)
        {
            NoticeInfo("비밀번호를 입력해주세요.");
            return;
        }
        if (regNickNameInput.text.Length == 0)
        {
            NoticeInfo("닉네임을 입력해주세요.");
            return;
        }
        if(regEMailInput.text.Length == 0)
        {
            NoticeInfo("이메일을 입력해주세요. (비밀번호 찾기용)");
            return;
        }
        //비번이랑 비번 확인이 서로 다를 경우
        if (false == regPwInput.text.Equals(regPwInputConfirm.text))
        {
            NoticeInfo("비밀번호와 비밀번호 확인이 다릅니다.");
            return;
        }
        //id가 영문과 숫자로만 이루어지지 않았을 경우, 4~15자리 사이
        char[] idCharArr = id.ToCharArray();
        char[] pwCharArr = pw.ToCharArray();
        //이메일
        bool isEmailValid = Regex.IsMatch(eMail, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
        if (!isEmailValid)
        {
            NoticeInfo("이메일을 올바른 형태로 입력해주세요.");
            return;
        }
        //길이
        if (idCharArr.Length < 4 || idCharArr.Length > 15)
        {
            NoticeInfo("ID는 4~15자리로 입력해야합니다.");
            return;
        }
        char[] nickNameCharArr = nickName.ToCharArray();
        if (nickName.Length > 10)
        {
            NoticeInfo("닉네임은 10글자 이하로 입력해야합니다.");
            return;
        }
        
        //닉네임
        foreach (char c in nickNameCharArr)
        {
            int asciiC = (int)c;
            if ((asciiC >= 33 && asciiC <= 47) || (asciiC >= 58 && asciiC <= 64) || (asciiC >= 91 && asciiC <= 96) || (asciiC >= 123 && asciiC <= 126))
            {
                NoticeInfo("닉네임은 한글, 영어, 숫자로만 입력해주세요.");
                return;
            }
        }
        foreach (char c in idCharArr)
        {
            int asciiC = (int)c;
            if (asciiC < 48 || (asciiC > 57 && asciiC < 97) || asciiC > 122)
            {
                NoticeInfo("ID는 영문 소문자와 숫자로만 이루어져야 합니다.");
                return;
            }
        }
        foreach (char c in pwCharArr)
        {
            int asciiC = (int)c;
            if (asciiC < 48 || (asciiC > 57 && asciiC < 97) || asciiC > 122)
            {
                NoticeInfo("비밀번호는 영문 소문자와 숫자로만 이루어져야 합니다.");
                return;
            }
        }
        //비밀번호가 15자리 이상일 경우
        if (pw.Length > 15)
        {
            NoticeInfo("비밀번호는 15자리를 초과할 수 없습니다.");
            return;
        }
        //id와 닉네임이 이미 존재하지 않다면 등록
        else
        {
            User user = new User(id, SHA256Hash(pw), nickName);
            user.eMail = eMail;
            //id와 닉네임이 이미 존재하지 않다면 등록
            CheckVersion(user);
            return;
        }
    }
    //패널이동
    public void ChangePanel(int nextPanelIndex)
    {
        //로그인 패널로 이동시
        if (nextPanelIndex == 1)
        {
            if (PlayerPrefs.HasKey("myID"))
            {
                loginIdInput.text = PlayerPrefs.GetString("myID");
                loginToggle.isOn = true;
            }
            if (PlayerPrefs.HasKey("myPW"))
                loginPwInput.text = PlayerPrefs.GetString("myPW");
        }
        foreach (GameObject go in panels)
        {
            if (go.activeSelf) go.SetActive(false);
        }
        panels[nextPanelIndex].SetActive(true);
    }
    //서버와 연결
    public void ConnectServer()
    {
        NoticeInfo("서버 접속중");
        loadingPanel.SetActive(true);
        //공지사항 띄우기
        if (initialNotice != "")
            ShowNoticeUI(true, initialNotice);
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        NoticeInfo("서버 접속");
        print("ConnectedToMaster");
        //동시접속자가 일정 이상일 경우
        if (PhotonNetwork.CountOfPlayers > maxPlayerCount)
        {
            NoticeInfo("서버 최대 동시접속자를 초과하여 접속할 수 없습니다. 잠시 후 시도해주세요.");
            loadingPanel.SetActive(false);
            PhotonNetwork.Disconnect();
            return;
        }
        PhotonNetwork.NickName = GameManager.Instance.GetUser().nickName + "_" + GameManager.Instance.GetUser().id;
        PhotonNetwork.JoinLobby();
    }
    public Text characterName;
    public Text waitingRoomCharacterName;
    public Text currentUserCount;
    public Text playerStat;
    public Text playerLevel;
    public Text playerRating;
    public Text playerMoney;
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        NoticeInfo("로비 접속");
        WaitingRoomManager.Instance.InitAll();
        loadingPanel.SetActive(false);
        GameManager.Instance.SetGameState(GameManager.GameState.Lobby);
        print("JoinedLobby");
        ChangePanel(3);
        User myUser = GameManager.Instance.GetUser();
        characterName.text = myUser.nickName;
        waitingRoomCharacterName.text = characterName.text;
        playerStat.text = myUser.winCount + myUser.loseCount + "전 " + myUser.winCount + "승 " + myUser.loseCount + "패";
        playerLevel.text = "LV." + myUser.level.ToString();
        playerRating.text = ((int)myUser.rating).ToString();
        ProfileManager.Instance.InitProfile(myUser.profileNum);
        playerMoney.text = myUser.money.ToString();
        currentUserCount.text = PhotonNetwork.CountOfPlayers.ToString();
    }
    public void LeaveLobby()
    {
        //포톤 네트워크 탈출
        PhotonNetwork.Disconnect();
        //게임 매니저 싱글톤 정보 제거
        GameManager.Instance.SetUser(null);
        GameManager.Instance.myID = null;
        //씬 다시 불러오기
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    //입력 정보 & 개인 정보 초기화\
    //방 입장시
    public InputField roomName;
    public InputField roomPw;
    public GameObject createRoomUI;
    public Button showRoomBtn;
    //비밀방인지
    public Toggle privateRoomToggle;
    public void CreateRoom()
    {
        string creatingRoom = roomName.text;
        //방의 이름이 비어있으면 안됨
        if (creatingRoom.Length == 0 || creatingRoom.Length > 10)
        {
            NoticeInfo("방 이름은 1글자 이상 10글자 미만이어야 합니다.");
            return;
        }
        //_문자 포함하면 안됨
        if (creatingRoom.Contains("_"))
        {
            NoticeInfo("방 이름은 '_'문자를 포함할 수 없습니다.");
            return;
        }
        //"봇" 포함하면 안됨
        if (creatingRoom.Contains("봇"))
        {
            NoticeInfo("방 이름은 '봇'을 포함할 수 없습니다.");
            return;
        }
        //이미 같은 이름의 방이 있다면 못하게
        if (false == RoomListManager.Instance.CheckRoomName(creatingRoom))
        {
            NoticeInfo("방 생성 실패: 이미 같은 이름의 방이 있습니다.");
            return;
        }
        //비밀 방일 때
        if (privateRoomToggle.isOn == true)
        {
            //글자수 0개면 안됨
            if (roomPw.text.Length == 0 || roomPw.text.Length > 10)
            {
                NoticeInfo("비밀번호는 1글자 이상 10글자 미만이여야 합니다.");
                return;
            }
            //_포함하면 안됨.
            if (roomPw.text.Contains("_"))
            {
                NoticeInfo("비밀번호는 '_'문자를 포함할 수 없습니다.");
                return;
            }
            creatingRoom += "_" + roomPw.text;
            //print(creatingRoom);
        }
        //다시 뒤돌아올 때 어색함 방지
        ShowCreateRoomUI(false);
        roomName.text = "";

        //최대 2인
        RoomOptions ro = new RoomOptions
        {
            MaxPlayers = 2
        };
        PhotonNetwork.CreateRoom(creatingRoom, ro, TypedLobby.Default);
        //대기방 패널로
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();

    }
    public GameObject tutorialUI;
    public GameObject[] tutorialPages;
    public Button nextPage;
    public Button prevPage;
    int page = 0;
    public void ShowTutorialUI(bool isShow)
    {
        tutorialUI.SetActive(isShow);
        tutorialPages[page].SetActive(false);
        this.page = 0;
        nextPage.interactable = true;
        prevPage.interactable = false;
        tutorialPages[page].SetActive(true);
    }
    public void ChangeTutorialPage(bool isUp)
    {
        tutorialPages[page].SetActive(false);
        if (isUp) page++;
        else page--;
        if (page == tutorialPages.Length - 1)
        {
            prevPage.interactable = true;
            nextPage.interactable = false;
        }
        else if (page == 0)
        {
            nextPage.interactable = true;
            prevPage.interactable = false;
        }
        else
        {
            nextPage.interactable = true;
            prevPage.interactable = true;
        }
        tutorialPages[page].SetActive(true);
    }
    public GameObject profileUI;
    public void ShowProfileUI(bool isOn)
    {
        if (isOn)
        {
            ProfileManager.Instance.UpdateProfile(GameManager.Instance.GetUser().profileStorage);
        }
        profileUI.SetActive(isOn);
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.JoinLobby();
    }
    public GameObject settingUI;
    public void ShowSettingUI(bool isShow)
    {
        settingUI.SetActive(isShow);
    }
    public void ChangeBGMState(bool isPlay)
    {
        GameManager.Instance.ChangeBGMState(isPlay);
    }
    public void ChangeSFXState(bool isPlay)
    {
        GameManager.Instance.ChangeSFXState(isPlay);
    }
    //공지사항 UI 띄우거나 없앨 때
    public GameObject noticeUI;
    //공지사항 텍스트
    public Text noticeText;
    public void ShowNoticeUI(bool isShow, string message)
    {
        //메시지 띄우기
        if (isShow)
        {
            noticeText.text = message;
        }
        noticeUI.SetActive(isShow);
    }
    public void ShowNoticeUI(bool isShow)
    {
        noticeUI.SetActive(isShow);
    }
    public GameObject adUI;
    //광고UI 띄우거나 없앨 때
    public void ShowAdUI(bool isShow)
    {
        adUI.SetActive(isShow);
    }
    //방 생성 UI띄우거나 없앨 때
    public void ShowCreateRoomUI(bool isShow)
    {
        createRoomUI.SetActive(isShow);
        showRoomBtn.interactable = !isShow;
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        currentUserCount.text = PhotonNetwork.CountOfPlayers.ToString();
        RoomListManager.Instance.RefreshRoomList(roomList);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("joinroomFailed");
        NoticeInfo("방 입장 실패");
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        NoticeInfo("방 입장 성공");
        GameManager.Instance.SetGameState(GameManager.GameState.Room);
        //대기방으로 이동
        ChangePanel(4);
        print("OnJoinedRoom");
    }
    //public override void OnPlayerEnteredRoom(Player newPlayer)
    //{
    //    base.OnPlayerEnteredRoom(newPlayer);
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        photonView.RPC("GoToInGame", RpcTarget.AllBufferedViaServer);
    //    }
    //}
    public InputField privateRoomPW;
    public GameObject privateRoomEnterUI;
    public void ShowPrivateRoomEnterUI(bool isShow)
    {
        if (isShow)
        {
            privateRoomPW.text = "";
        }
        privateRoomEnterUI.SetActive(isShow);
    }
    public void EnterPrivateRoom()
    {
        ShowPrivateRoomEnterUI(false);
        RoomListManager.Instance.EnterPrivateRoom(privateRoomPW.text);
    }
    public GameObject publicRoomEnterUI;
    public void ShowGeneralRoomEnterUI(bool isShow)
    {
        publicRoomEnterUI.SetActive(isShow);
    }
    public void EnterPublicRoom()
    {
        ShowPrivateRoomEnterUI(false);
        RoomListManager.Instance.EnterPublicRoom();
    }
}
