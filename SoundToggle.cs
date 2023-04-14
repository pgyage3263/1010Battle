using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SoundToggle : MonoBehaviour
{
    public Toggle sfxToggle;
    public Toggle bgmToggle;
    private void Start()
    {
        bgmToggle.isOn = GameManager.Instance.bgmState;
        sfxToggle.isOn = GameManager.Instance.sfxState;
    }
    public void ChangeBGMState(bool isPlay)
    {
        GameManager.Instance.ChangeBGMState(isPlay);
    }
    public void ChangeSFXState(bool isPlay)
    {
        GameManager.Instance.ChangeSFXState(isPlay);
    }
}
