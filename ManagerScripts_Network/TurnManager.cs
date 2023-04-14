using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Proyecto26;
namespace NetworkScripts
{
    //시간은 각자 30초씩
    //턴 종료시, 혹은 시간이 다되었을 경우 넘어감
    public class TurnManager : MonoBehaviourPunCallbacks
    {
        public static TurnManager Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
        bool isStart = false;
        //시간 이미지
        public Image[] timeGage;
        public Text[] timeText;
        //이미지 소스
        public Sprite warningTimeBar;
        public Sprite originTimeBar;
        public GameObject messagePanel;
        public Text message;
        float turnTime = 0.0f;
        //메시지 판넬 시간
        public float messagePanelTime = 1.0f;
        //1P, 2P 턴 종료 버튼
        public Button[] turnEndButtons;
        public GameObject[] fakeTurnEndBtns;
        public GameObject[] SettingButtons;
        public Button[] emotionButtons;
        public GameObject[] myTurnFX;
        //프로필 UI들 (회전 시킬 것들)
        public GameObject[] turnUIs;
        public Text[] nickNames;
        public Text[] startPanelNickNames;
        public Text[] winLose;
        public Text[] levels;
        public bool isMyturn = false;
        public GameObject startUI;
        public GameObject loadingPanel;


        AudioSource myAudio;
        //startCount
        int startCount = 0;
        float gameTime = 0;
        // Start is called before the first frame update
        void Start()
        {
            //TimeScale
            Time.timeScale = 1.0f;

            photonView.RPC("CheckIn", RpcTarget.AllBufferedViaServer);
            InitUIRotation();
            myAudio = GetComponent<AudioSource>();
            turnEndButtons[0].interactable = true;
            turnEndButtons[1].interactable = false;
        }
        //둘 다 레벨에 도착했는지 확인
        [PunRPC]
        public void CheckIn()
        {
            startCount++;
            if (startCount >= 6)
            {
                StartCoroutine("StartDelay");
                startUI.SetActive(true);
                loadingPanel.SetActive(false);
                //SFX
                SFXManager.Instance.OnGameStart();
            }
        }
        //게임 시작UI관련
        IEnumerator StartDelay()
        {
            yield return new WaitForSeconds(4.0f);
            GameStart();
            startUI.SetActive(false);
        }
        User myUser;
        User otherUser;
        void GetWinLose(string id, bool isMine)
        {
            RestClient.Get<User>(url: "https://battle1010.firebaseio.com/users/" + id + ".json").Then(response =>
            {
                if (isMine)
                {
                    myUser = response;
                    GameManager.Instance.SetUser(myUser);
                    //1P
                    if (PhotonNetwork.IsMasterClient)
                    {
                        //winLose[0].text = (myUser.winCount + myUser.loseCount) + "전" + myUser.winCount + "승" + myUser.loseCount + "패";
                        winLose[0].text = "레이팅 " + ((int)myUser.rating).ToString();
                        levels[0].text = "LV." + myUser.level.ToString();
                        ImageManager.Instance.ChangeImages(0, myUser.profileNum);
                    }
                    //2P
                    else
                    {
                        //winLose[1].text = (myUser.winCount + myUser.loseCount) + "전" + myUser.winCount + "승" + myUser.loseCount + "패";
                        winLose[1].text = "레이팅 " + ((int)myUser.rating).ToString();
                        levels[1].text = "LV." + myUser.level.ToString();
                        ImageManager.Instance.ChangeImages(1, myUser.profileNum);
                    }
                }
                else
                {
                    otherUser = response;
                    //1P
                    if (PhotonNetwork.IsMasterClient)
                    {
                        //winLose[1].text = (otherUser.winCount + otherUser.loseCount) + "전" + otherUser.winCount + "승" + otherUser.loseCount + "패";
                        winLose[1].text = "레이팅 " + ((int)otherUser.rating).ToString();
                        levels[1].text = "LV." + otherUser.level.ToString();
                        ImageManager.Instance.ChangeImages(1, otherUser.profileNum);
                    }
                    //2P
                    else
                    {
                        //winLose[0].text = (otherUser.winCount + otherUser.loseCount) + "전" + otherUser.winCount + "승" + otherUser.loseCount + "패";
                        winLose[0].text = "레이팅 " + ((int)otherUser.rating).ToString();
                        levels[0].text = "LV." + otherUser.level.ToString();
                        ImageManager.Instance.ChangeImages(0, otherUser.profileNum);
                    }
                }
                photonView.RPC("CheckIn", RpcTarget.AllBufferedViaServer);
            }
        );
        }
        void SetPlayer(string id, User user)
        {
            RestClient.Put(url: "https://battle1010.firebaseio.com/users/" + id + ".json", user);
            loadingPanel.SetActive(false);
        }
        //상대방 ID
        public string otherId;
        string otherNickName;
        //초기 UI회전
        void InitUIRotation()
        {
            string myNickName = PhotonNetwork.NickName.Split('_')[0];
            string myId = PhotonNetwork.NickName.Split('_')[1];
            otherNickName = PhotonNetwork.PlayerListOthers[0].NickName.Split('_')[0];
            otherId = PhotonNetwork.PlayerListOthers[0].NickName.Split('_')[1];
            //print(otherId);
            //상대 승패 알아오기
            GetWinLose(otherId, false);
            GetWinLose(GameManager.Instance.GetUser().id, true);
            //1P의 경우
            if (PhotonNetwork.IsMasterClient)
            {
                isMyturn = true;
                turnEndButtons[1].gameObject.SetActive(false);
                fakeTurnEndBtns[1].SetActive(true);
                SettingButtons[1].gameObject.SetActive(false);
                emotionButtons[1].enabled = false;
                startPanelNickNames[0].text = myNickName;
                startPanelNickNames[1].text = otherNickName;
                nickNames[0].text = myNickName;
                nickNames[1].text = otherNickName;
                User myUser = GameManager.Instance.GetUser();
                //winLose[0].text = myUser.winCount + myUser.loseCount + "전" + myUser.winCount + "승" + myUser.loseCount + "패";
                winLose[0].text = "레이팅 " + ((int)myUser.rating).ToString();
            }
            //2P의 경우
            else
            {
                turnEndButtons[0].gameObject.SetActive(false);
                //턴 버튼 관련
                turnEndButtons[1].interactable = false;
                myTurnText[1].SetActive(false);
                enemyTurnText[1].SetActive(true);

                fakeTurnEndBtns[0].SetActive(true);
                SettingButtons[0].gameObject.SetActive(false);
                emotionButtons[0].enabled = false;
                messagePanel.transform.Rotate(0, 0, 180);
                foreach (GameObject ui in turnUIs)
                {
                    ui.transform.Rotate(0, 0, 180);
                }
                startPanelNickNames[0].text = otherNickName;
                startPanelNickNames[1].text = myNickName;
                nickNames[0].text = otherNickName;
                nickNames[1].text = myNickName;
                User myUser = GameManager.Instance.GetUser();
                //winLose[1].text = myUser.winCount + myUser.loseCount + "전" + myUser.winCount + "승" + myUser.loseCount + "패";
                winLose[1].text = "레이팅 " + ((int)myUser.rating).ToString();

                //HP Image fill origin 변경
                HPManager.Instance.hpImgs[0].fillOrigin = 1;
                HPManager.Instance.hpImgs[1].fillOrigin = 0;
            }
        }
        // Update is called once per frame
        void Update()
        {
            //플레이시간
            gameTime += Time.deltaTime;
            //이번 차례의 시간체크
            CheckTime();
        }
        public float turnDelay = 25.0f;
        bool isPlayWarn = false;
        public GameObject waitingUI;
        void CheckTime()
        {
            if (isEnd || BoardManager.Instance.isEndProcess) return;
            //게임이 시작되었을 경우에
            if (isStart)
            {
                //내 턴일 경우에만 턴 엔드 날려줌
                if (turnTime > turnDelay)
                {
                    //내 턴일 경우에
                    if (currentPlayerNum == NetworkManager.Instance.myPlayerNum)
                    {
                        photonView.RPC("EndTurn", RpcTarget.AllBufferedViaServer, currentPlayerNum);
                    }
                    else
                    {
                        turnTime += Time.deltaTime;
                        //3초 후까지 응답이 없다면
                        if (!waitingUI.activeSelf && turnTime - turnDelay > 3.0f)
                        {
                            //UI띄우기
                            waitingUI.SetActive(true);

                        }
                        return;
                    }
                }
                else
                {
                    turnTime += Time.deltaTime;
                    timeGage[currentPlayerNum].fillAmount = (turnDelay - turnTime) / turnDelay;
                    timeText[currentPlayerNum].text = ((int)(turnDelay - turnTime)).ToString();
                    if (turnDelay - turnTime < 5.0f)
                    {
                        if (timeText[currentPlayerNum].color != Color.red)
                            timeText[currentPlayerNum].color = Color.red;
                        if (timeGage[currentPlayerNum].sprite != warningTimeBar)
                            timeGage[currentPlayerNum].sprite = warningTimeBar;
                    }
                    //else timeText[currentPlayerNum].color = Color.white;
                }
                //비프음과 54321연출
                if (isMyturn && isPlayWarn == false && turnDelay - turnTime <= 5.0f)
                {
                    if (GameManager.Instance.sfxState == true)
                        myAudio.Play();
                    isPlayWarn = true;
                }
                //messagePanel 관련
                if (turnTime > messagePanelTime)
                {
                    messagePanel.SetActive(false);
                }
            }
        }
        public int CalculateReward()
        {
            if (gameTime > 180.0f)
            {
                return 5;
            }
            else if (gameTime > 120.0f)
            {
                return 4;
            }
            else if (gameTime > 60.0f)
            {
                return 3;
            }
            else return 0;
        }
        public int CalculateExp()
        {
            if (gameTime > 120.0f)
            {
                return 2;
            }
            else if (gameTime > 60.0f)
            {
                return 1;
            }
            else return 0;
        }
        public GameObject levelUpText;
        //경험치 처리
        void ProcessEXP(User user, int expAmount)
        {
            //다음 레벨로 진입하기 위해선 현재 레벨만큼의 경험치가 필요
            user.exp += expAmount;
            //현재 레벨보다 높을 때
            if (user.exp > user.level)
            {
                //자신의 user일 떄
                if (user.id.Equals(myUser.id))
                    levelUpText.SetActive(true);
                //레벨업 처리
                user.level++;
                user.exp -= user.level;
            }
        }
        
        //게임 시작시
        public void GameStart()
        {
            isStart = true;
            myTurnFX[0].SetActive(true);
            myTurnFX[1].SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                //UI띄우기 -> 임시
                messagePanel.SetActive(true);
                message.text = "나의 턴";
                AudioManager.Instance.PlayBGM(true);
            }
            else
            {
                //UI띄우기 -> 임시
                messagePanel.SetActive(true);
                message.text = "게임 시작";
                AudioManager.Instance.PlayBGM(false);
            }
            SFXManager.Instance.OnTurnChange();
            //myAudio.Play();
        }
        public GameObject endPanel;
        public Text[] endPanelWinLose;
        public GameObject[] winLoseImage;
        public Text reward;
        bool isEnd = false;
        public GameObject touchShield;
        public bool IsEnd()
        {
            return isEnd;
        }
        public void GameEndClient(int winnerNum)
        {
            photonView.RPC("GameEnd", RpcTarget.AllBuffered, winnerNum);
        }
        //레이팅 계산
        //최종 레이팅 차이 반환
        float GetRatingDiff(float myRating, float enemyRating, int isWin)
        {
            //가중치 k는 20
            int k = 20;
            //이길 확률 구하기
            float expectVal = 1.0f / (1 + Mathf.Pow(10, (enemyRating - myRating) / 400.0f));
            //최종 레이팅 구하기
            float rating = myRating + k * (isWin - expectVal);
            //차이 반환
            return (rating - myRating);
        }
        //레벨, 경험치, 레이팅 텍스트 및 이미지
        public Text levelText;
        public Text ratingText;
        public Image expImage;
        public GameObject botUI;
        //게임 끝났을 때 -> 한번만 들어와야한다.
        //인자: 승자(0:1P, 1:2P)
        [PunRPC]
        public void GameEnd(int winnerNum)
        {
            if (isEnd == true) return;
            isEnd = true;
            SFXManager.Instance.OnGameEnd();
            touchShield.SetActive(true);
            //경고음 관련
            isPlayWarn = false;
            if (myAudio.isPlaying)
                myAudio.Stop();
            //봇전의 경우
            string currentRoomName = PhotonNetwork.CurrentRoom.Name.Split('_')[0];
            bool isBot = false;
            if (currentRoomName.Contains("봇"))
            {
                isBot = true;
            }
            //레이팅 차이
            float ratingDiff = 0.0f;
            //승자의 경우
            if (NetworkManager.Instance.myPlayerNum == winnerNum)
            {
                loadingPanel.SetActive(true);
                User myUser = GameManager.Instance.GetUser();
                //승패 기록
                int rewardMoney = 0;
                //패배, 승리 증감
                //봇전일 경우
                if (isBot)
                {
                    //UI
                    botUI.SetActive(true);
                    //경험치 & 레벨
                    ProcessEXP(myUser, CalculateExp() / 2);
                    //상금
                    rewardMoney = CalculateReward() / 2;
                }
                else
                {
                    myUser.winCount++;
                    //경험치 & 레벨
                    ProcessEXP(myUser, CalculateExp());
                    ProcessEXP(otherUser, CalculateExp() / 2);
                    //상금
                    rewardMoney = CalculateReward();
                    //레이팅 차이 구함
                    ratingDiff = GetRatingDiff(myUser.rating, otherUser.rating, 1);
                    //자신과 상대방의 레이팅 기록
                    otherUser.rating += GetRatingDiff(otherUser.rating, myUser.rating, 0);
                    myUser.rating += ratingDiff;
                }
                //상대 패 처리
                otherUser.loseCount++;
                //재화 기록
                myUser.money += rewardMoney;
                reward.text = rewardMoney.ToString();
                //레이팅 표시
                ratingText.text = (int)myUser.rating + "( +" + ratingDiff.ToString("F1") + ")";
                //기록
                SetPlayer(otherUser.id, otherUser);
                SetPlayer(myUser.id, myUser);
                //승자UI
                //endPanel.SetActive(true);
                winLoseImage[0].SetActive(true);
                winLoseImage[1].SetActive(false);
                endPanelWinLose[0].text = "승리";
                endPanelWinLose[1].text = "<color=\"#ce6730\">" + otherNickName + "</color> 님과의 대결에서 승리했습니다.";
                //레벨, 경험치 표시
                //레벨
                levelText.text = myUser.level.ToString();
                //이미지
                expImage.fillAmount = (float)myUser.exp / (myUser.level + 1);
                //AudioManager.Instance.EndGame(true);
                //RestartManager.Instance.OnFinish();
                StartCoroutine("ProcessEnd", true);
            }
            else
            {
                //패자UI
                EmotionManager.Instance.OnClickSpeechBubble(4);
                //endPanel.SetActive(true);
                reward.text = "0";
                winLoseImage[0].SetActive(false);
                winLoseImage[1].SetActive(true);
                endPanelWinLose[0].text = "패배";
                endPanelWinLose[1].text = "<color=\"#ce6730\">" + otherNickName + "</color> 님과의 대결에서 패배했습니다.";
                //레벨, 경험치 표시
                User myUser = GameManager.Instance.GetUser();
                ProcessEXP(myUser, CalculateExp() / 2);
                //봇전의 경우
                if (isBot)
                {
                    //UI
                    botUI.SetActive(true);
                    ratingText.text = (int)myUser.rating + "(+ 0)";
                }
                else
                {
                    ratingDiff = GetRatingDiff(myUser.rating, otherUser.rating, 0);
                    myUser.rating += ratingDiff;
                    ratingText.text = (int)myUser.rating + "( " + ratingDiff.ToString("F1") + ")";
                }
                //레벨
                levelText.text = myUser.level.ToString();
                //이미지
                expImage.fillAmount = (float)myUser.exp / (myUser.level + 1);


                //AudioManager.Instance.EndGame(false);
                //RestartManager.Instance.OnFinish();
                StartCoroutine("ProcessEnd", false);
            }
            RestartManager.Instance.OnFinish();
            //광고
            //AdsManager.Instance.ShowAd();
        }
        IEnumerator ProcessEnd(bool isWin)
        {
            yield return new WaitForSeconds(1.0f);
            endPanel.SetActive(true);
            AudioManager.Instance.EndGame(isWin);
            //광고
            AdsManager.Instance.ShowAd();
        }
        public int currentPlayerNum = 0;
        public void TurnEndButton(int playerNum)
        {
            //예외
            if (playerNum != currentPlayerNum)
            {
                print("잘못된 턴 종료");
                return;
            }
            photonView.RPC("EndTurn", RpcTarget.AllBufferedViaServer, playerNum);
        }
        //myTurn Text 0->1P 1->2P
        public GameObject[] myTurnText;
        //enemyTurn Text
        public GameObject[] enemyTurnText;
        //이미지 Lerp한 상승
        IEnumerator TimeLerp(int playerNum)
        {
            float time = 0.0f;
            while (true)
            {
                if (time > 1.0f)
                {
                    break;
                }
                time += Time.fixedDeltaTime;
                timeGage[playerNum].fillAmount = Mathf.Lerp(timeGage[playerNum].fillAmount, 1.0f, time);
                yield return new WaitForFixedUpdate();
            }
            timeGage[currentPlayerNum].fillAmount = 1.0f;

        }
        [PunRPC]
        public void EndTurn(int playerNum)
        {
            waitingUI.SetActive(false);
            //예외
            if (playerNum != currentPlayerNum)
            {
                print("잘못된 턴 종료");
                return;
            }
            //손에 들고있는 블록 원위치
            MouseInputManager.Instance.ExitBlock();
            //남아있는 블록들 자기 자신 공격
            BlockCreateManager.Instance.OnTurnEnd(playerNum);
            //시간 복구
            turnTime = 0.0f;
            //상대 시간 시각화 초기화 => 이미지는 Lerp하게 상승
            //timeGage[currentPlayerNum].fillAmount = 1.0f;
            StartCoroutine("TimeLerp", currentPlayerNum);
            timeText[currentPlayerNum].text = "25";
            timeText[currentPlayerNum].color = Color.white;
            timeGage[currentPlayerNum].sprite = originTimeBar;
            //기존 플레이어 턴 종료 버튼 비활성화
            turnEndButtons[playerNum].interactable = false;
            //텍스트 변경(기존 플레이어)
            myTurnText[playerNum].SetActive(false);
            enemyTurnText[playerNum].SetActive(true);
            //기존 턴 이펙트 비활성화
            myTurnFX[currentPlayerNum].SetActive(false);
            //턴 바꾸기 -> 임시
            currentPlayerNum = (playerNum == 0) ? 1 : 0;
            //신규 턴 이펙트 활성화
            myTurnFX[currentPlayerNum].SetActive(true);
            //myAudio.Play();
            SFXManager.Instance.OnTurnChange();
            //경고음 관련
            isPlayWarn = false;
            if (myAudio.isPlaying)
                myAudio.Stop();
            //새 턴 플레이어 턴 종료 버튼 활성화
            turnEndButtons[currentPlayerNum].interactable = true;
            //텍스트 변경
            myTurnText[currentPlayerNum].SetActive(true);
            enemyTurnText[currentPlayerNum].SetActive(false);
            //블록 생성 -> 임시
            if (playerNum == NetworkManager.Instance.myPlayerNum)
            {
                //message.text = "상대 턴";
                AudioManager.Instance.PlayBGM(false);
                isMyturn = false;

                bool is33Ban = false;
                for (int i = 0; i < 3; i++)
                {
                    if (is33Ban == true)
                    {
                        //100을 더함 = 33을 밴
                        BlockCreateManager.Instance.CreateBlock(100 + playerNum);
                    }
                    else
                    {
                        int blockIdx = BlockCreateManager.Instance.CreateBlock(playerNum);
                        if (blockIdx == 7) is33Ban = true;
                    }
                }
            }
            else
            {
                //UI띄우기 -> 임시
                message.text = "나의 턴";
                messagePanel.SetActive(true);
                AudioManager.Instance.PlayBGM(true);
                isMyturn = true;
                //자동 턴 종료 체크
                BoardManager.Instance.CheckTurnEnd();
            }

        }
    }
}