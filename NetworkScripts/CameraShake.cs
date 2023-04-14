using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NetworkScripts
{
    //x,z축으로 흔들리게
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance;
        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }
        //카메라 쉐이크
        Vector3 originPos;
        float shakeLimit = 0.0f;
        float perShake = 0.02f;
        float originShake = 0.1f;

        //증감속 그래프
        public AnimationCurve camShakeGraph;
        // Start is called before the first frame update
        void Start()
        {
            originPos = transform.position;
        }
        // Update is called once per frame
        void Update()
        {
            if (isShake)
            {
                shakeTime += Time.deltaTime;
                if (shakeTime > shakeTimeDelay)
                {
                    isShake = false;
                    shakeLimit = 0;
                    transform.position = originPos;
                    return;
                }
                float shakeScale = shakeLimit * camShakeGraph.Evaluate(shakeTime * (1.0f/shakeTimeDelay));
                //절대값
                shakeScale = Mathf.Abs(shakeScale);
                transform.position = originPos + new Vector3(Random.Range(-shakeScale, shakeScale), 0, Random.Range(-shakeScale, shakeScale));
            }
        }
        bool isShake = false;
        float shakeTime = 0.0f;
        public float shakeTimeDelay = 1.0f;
        public void ShakeCam()
        {
            shakeTime = 0.0f;
            if (true == isShake)
            {
                shakeLimit += perShake;
            }
            else
            {
                isShake = true;
                shakeLimit = originShake;
            }

        }
    }
}