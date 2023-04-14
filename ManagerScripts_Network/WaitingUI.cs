using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NetworkScripts
{
    public class WaitingUI : MonoBehaviour
    {
        float currentTime = 0.0f;
        public float timeDelay = 20.0f;
        public UnityEngine.UI.Text timeText;
        private void OnEnable()
        {
            currentTime = 0.0f;
        }
        private void Update()
        {
            currentTime += Time.deltaTime;
            if(currentTime > 20.0f)
            {
                TurnManager.Instance.GameEndClient(NetworkManager.Instance.myPlayerNum);
                gameObject.SetActive(false);
            }
            else
            {
                timeText.text = ((int)(timeDelay - currentTime)).ToString();
            }
        }
    }
}