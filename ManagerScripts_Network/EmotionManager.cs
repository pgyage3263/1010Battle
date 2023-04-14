using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
namespace NetworkScripts
{
    //말풍선 관련한 매니저클래스
    public class EmotionManager : MonoBehaviourPunCallbacks
    {
        public static EmotionManager Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        public AnimationCurve emotionScaleAC;
        public GameObject emotionPanel;
        public GameObject emotionScaler;
        public GameObject[] speechBubble;
        public Text[] speechText;
        public string[] speechWords;
        
        bool[] isBubbleActive = new bool[2] { false, false };
        float[] currentTime = new float[2];

        public Button[] speechButtons;
        public float bubbleTimeDelay = 2.0f;

        //비밀 커맨드
        //2초에 5번 누르면 나오기로.
        int secretCount = 0;
        float limitTime = 2.0f;
        public void CalculateCommand()
        {
            StartCoroutine("SecretCommand");
        }
        IEnumerator SecretCommand()
        {
            secretCount++;
            if(secretCount >= 5)
            {
                secretCount = 0;
                OnClickSpeechBubble(5);
            }
            yield return new WaitForSeconds(limitTime);
            if(secretCount>=0)
                secretCount--;
        }
        public void ShowEmotionUI(bool isShow)
        {
            CalculateCommand();
            emotionTime = 0.0f;
            emotionPanel.SetActive(isShow);
        }
        //감정표현 시간 제한
        IEnumerator SpeechWait()
        {
            foreach(Button btn in speechButtons)
            {
                btn.interactable = false;
            }
            yield return new WaitForSeconds(5.0f);
            foreach (Button btn in speechButtons)
            {
                btn.interactable = true;
            }
        }
        //말풍선 클릭시
        public void OnClickSpeechBubble(int speechNum)
        {
            //동기화해줌
            photonView.RPC("ShowSpeechBubble", RpcTarget.AllBuffered, NetworkManager.Instance.myPlayerNum, speechNum);
            //5초 대기
            StartCoroutine("SpeechWait");
            //UI닫아줌
            ShowEmotionUI(false);
        }
        [PunRPC]
        public void ShowSpeechBubble(int playerNum, int speechNum)
        {
            currentTime[playerNum] = 0.0f;
            isBubbleActive[playerNum] = true;
            if(speechNum == 5)
            {
                speechText[playerNum].text = "사랑해";
            }
            else
                speechText[playerNum].text = speechWords[speechNum];
            speechBubble[playerNum].SetActive(true);
        }
        Vector3 speechBubbleChange = new Vector3(-1, 1, 1);
        float emotionTime = 0.0f;
        private void Update()
        {
            if (emotionPanel.activeSelf)
            {
                emotionTime += Time.deltaTime;
                emotionScaler.transform.localScale = Vector3.one * emotionScaleAC.Evaluate(emotionTime/2);
            }
            for (int i = 0; i < 2; i++)
            {
                if (isBubbleActive[i] == true)
                {
                    //시간이 다 되었을 경우
                    if (bubbleTimeDelay < currentTime[i])
                    {
                        isBubbleActive[i] = false;
                        //말풍선 닫기
                        speechBubble[i].SetActive(false);
                        continue;
                    }
                    else
                    {
                        //시간 증가
                        speechBubble[i].transform.localScale = speechBubbleChange * emotionScaleAC.Evaluate(currentTime[i]/bubbleTimeDelay);
                        currentTime[i] += Time.deltaTime;
                    }
                }
            }
        }
    }
}
