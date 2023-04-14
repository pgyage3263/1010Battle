using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    AudioSource myAudio;

    public bool bgmState = true;
    public bool sfxState = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            myAudio = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    //BGM State
    public void ChangeBGMState(bool isPlay)
    {
        bgmState = isPlay;
        if (isPlay) myAudio.Play();
        else myAudio.Stop();
    }
    //SFX State
    public void ChangeSFXState(bool isPlay)
    {
        sfxState = isPlay;
    }
    public enum GameState
    {
        Main,
        Lobby,
        Room,
        Error
    };
    GameState gameState = GameState.Main;
    public void SetGameState(GameState gameState)
    {
        this.gameState = gameState;
    }
    public bool CheckInRoom()
    {
        if (gameState == GameState.Room)
        {
            return true;
        }
        else return false;
    }
    public bool CheckError()
    {
        if (gameState == GameState.Error)
        {
            gameState = GameState.Main;
            return true;
        }
        else return false;
    }
    public string myID;
    private User myUser;
    public User GetUser()
    {
        return myUser;
    }
    bool mySW = false;
    bool isTrack = false;
    public void SetUser(User user)
    {
        //처음 Set할 경우
        if (myUser == null && user != null)
        {
            currentTime = 0.0f;
            //sw반대로
            user.connectSW = !user.connectSW;
            mySW = user.connectSW;
            isTrack = true;
        }
        myUser = user;
        if (user != null)
        {
            myID = user.id;
            //서버에 저장
            RestClient.Put(url: "https://battle1010.firebaseio.com/users/" + user.id + ".json", user);
            //예외처리
        }
        else
        {
            isTrack = false;
        }
    }
    public void EarnStar(int count)
    {
        myUser.money += count;
        SetUser(myUser);
    }
    //재접속 감지
    void TrackSW()
    {
        RestClient.Get<User>(url: "https://battle1010.firebaseio.com/users/" + myUser.id + ".json").Then(response =>
        {
            if (response.connectSW != mySW)
            {
                isTrack = false;
                //종료
                PhotonNetwork.Disconnect();
                gameState = GameState.Error;
                SetUser(null);
                myID = null;
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
        );
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            gameState = GameState.Error;
            SetUser(null);
            //에디터일 때만 로드씬 막음

            //GameState가 Room이였을 경우만?
#if !UNITY_EDITOR
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
#endif

        }
    }
    float currentTime = 0.0f;
    public float timeDelay = 5.0f;
    private void Update()
    {
        if (isTrack)
        {
            currentTime += Time.deltaTime;
            if (currentTime > timeDelay)
            {
                currentTime = 0.0f;
                TrackSW();
            }
        }
    }
}