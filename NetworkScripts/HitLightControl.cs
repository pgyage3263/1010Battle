using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NetworkScripts
{
    public class HitLightControl : MonoBehaviour
    {
        public AnimationCurve ac;
        Light myLight;
        float originIntensity;
        // Start is called before the first frame update
        void Start()
        {
            myLight = GetComponent<Light>();
            originIntensity = myLight.intensity;
        }
        private void OnEnable()
        {
            currentTime = 0.0f;
        }
        public float timeDelay = 1.0f;
        float currentTime = 0.0f;
        // Update is called once per frame
        void Update()
        {
            if (currentTime > timeDelay)
            {
                gameObject.SetActive(false);
            }
            else
            {
                myLight.intensity = originIntensity * ac.Evaluate(currentTime);
                currentTime += Time.deltaTime;
            }
        }
    }
}