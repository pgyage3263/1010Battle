using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
namespace NetworkScripts
{
    public class HPManager : MonoBehaviourPunCallbacks
    {
        public static HPManager Instance;
        public GameObject p1Fire;
        public GameObject p2Fire;
        public Animator[] hpBars;
        public Animator[] profileImages;
        public Text[] rageText;
        public Image[] hpImgs;
        public GameObject[] rageFX;
        AudioSource myAudio;

        bool rage_1P = false;
        bool rage_2P = false;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        public void PlayDieFX(int playerNum)
        {
            if (playerNum == 0) p1Fire.SetActive(true);
            else p2Fire.SetActive(true);
        }
        //상대의 HP 변동상황 받는 함수
        [PunRPC]
        public void ReceiveHPChange(int playerNum, int damage)
        {
            if (playerNum == 0)
            {
                //hpText 색 변경 => 공격력 2배시
                if (hp_1p-damage <= 20 && rage_1P == false)
                {
                    hpText_1p.color = Color.red;
                    rageFX[0].SetActive(true);
                    rageText[0].text = "RAGE";
                    rageText[0].color = Color.red;
                    if (GameManager.Instance.sfxState == true)
                        myAudio.Play();
                    rage_1P = true;
                }
                hp_1p -= damage;
                hpText_1p.text = hp_1p.ToString();
                //효과
                
                hpBars[0].SetTrigger("Hit");
                profileImages[0].SetTrigger("Hit");
                //fill Amount
                hpImgs[0].fillAmount = (float)hp_1p / maxHP;
                //CameraShake.Instance.ShakeCam();
                //hitLight_1p.SetActive(true);
                StartCoroutine("GameStop", Time.deltaTime);
                if (hp_1p <= 0)
                {
                    PlayDieFX(0);
                    Time.timeScale = 1.0f;
                    TurnManager.Instance.GameEndClient(1);
                }
            }
            else
            {
                //hpText 색 변경 => 공격력 2배시
                if (hp_2p-damage <= 20 && rage_2P == false)
                {
                    hpText_2p.color = Color.blue;
                    rageFX[1].SetActive(true);
                    rageText[1].text = "RAGE";
                    rageText[1].color = Color.blue;
                    if (GameManager.Instance.sfxState == true)
                        myAudio.Play();
                    rage_2P = true;
                }
                hp_2p -= damage;
                hpText_2p.text = hp_2p.ToString();
                //효과
                hpBars[1].SetTrigger("Hit");
                profileImages[1].SetTrigger("Hit");
                //fill Amount
                hpImgs[1].fillAmount = (float)hp_2p / maxHP;
                //CameraShake.Instance.ShakeCam();
                //hitLight_2p.SetActive(true);
                StartCoroutine("GameStop", Time.deltaTime);
                if (hp_2p <= 0)
                {
                    PlayDieFX(1);
                    Time.timeScale = 1.0f;
                    TurnManager.Instance.GameEndClient(0);
                }
            }
        }
        public void ReceiveDamage(int playerNum, int damage)
        {
            //타격 라이트
            if (playerNum == 0)
            {
                hitLight_1p.SetActive(true);
                ////상대 체력 20이하일 경우 2배
                //if (hp_2p <= 20)
                //{
                //    damage *= 2;
                //}
            }
            else
            {
                hitLight_2p.SetActive(true);
                ////상대 체력 20이하일 경우 2배
                //if (hp_1p <= 20)
                //{
                //    damage *= 2;
                //}
            }

            if (playerNum == NetworkManager.Instance.myPlayerNum)
            {
                    photonView.RPC("ReceiveHPChange", RpcTarget.AllBuffered, NetworkManager.Instance.myPlayerNum, damage);
            }
        }
        //최대 체력
        public int maxHP = 50;
        int hp_1p;
        public int HP_1p
        {
            get
            {
                return hp_1p;
            }
            set
            {

                //if(NetworkManager.Instance.myPlayerNum == 0)
                //{
                //    photonView.RPC("ReceiveHPChange", RpcTarget.AllBuffered, 0,1);
                //}
            }
        }
        int hp_2p;
        public int HP_2p
        {
            get
            {
                return hp_2p;
            }
            set
            {
                //if (NetworkManager.Instance.myPlayerNum == 1)
                //{
                //    photonView.RPC("ReceiveHPChange", RpcTarget.AllBuffered, 1,1);
                //}
            }
        }
        //HP Text UI
        public Text hpText_1p;
        public Text hpText_2p;
        //Hit Light
        public GameObject hitLight_1p;
        public GameObject hitLight_2p;
        IEnumerator GameStop(float time)
        {
            Time.timeScale = 0.0f;
            float currentTime = 0.0f;
            while (currentTime < time)
            {
                currentTime += Time.unscaledDeltaTime;
                yield return null;
            }
            Time.timeScale = 1.0f;
        }
        // Start is called before the first frame update
        void Start()
        {
            //기본 체력 설정
            hp_1p = maxHP;
            hp_2p = maxHP;
            hpText_1p.text = hp_1p.ToString();
            hpText_2p.text = hp_2p.ToString();
            myAudio = GetComponent<AudioSource>();
        }



    }
}