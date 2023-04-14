using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkScripts
{
    //투사체를 날리는 싱글톤
    public class BallCreateManager : MonoBehaviour
    {
        public static BallCreateManager Instance;

        //public GameObject redBall;
        //public GameObject blueBall;
        //public GameObject specialBall;

        public GameObject[] fireBalls;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void ThrowBall(int playerNum, int myColor, Vector3 pos)
        {
            //투사체
            GameObject ball;
            ball = Instantiate(fireBalls[myColor], pos, Quaternion.identity);
            ////투사체 종류
            //if (myColor == 1)
            //{
            //    ball = Instantiate(redBall, pos, Quaternion.identity);
            //}
            //else if (myColor == 2)
            //{
            //    ball = Instantiate(blueBall, pos, Quaternion.identity);
            //}
            //else if (myColor == 3)
            //{
            //    ball = Instantiate(specialBall, pos, Quaternion.identity);
            //}
            //else return;

            //투사체 타겟
            if (playerNum == 0)
            {
                ball.GetComponent<EffectSettings>().Target = BoardManager.Instance.profile_2P;
            }
            else if (playerNum == 1)
            {
                ball.GetComponent<EffectSettings>().Target = BoardManager.Instance.profile_1P;
            }

        }
    }
}
