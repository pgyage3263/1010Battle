using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ImageManager : MonoBehaviour
{
    public static ImageManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    public Sprite[] profileSprites;
    public Image[] profile1P;
    public Image[] profile2P;
    public void ChangeImages(int playerNum, int imgNum)
    {
        if (playerNum == 0)
        {
            for (int i = 0; i < profile1P.Length; i++)
            {
                profile1P[i].sprite = profileSprites[imgNum];
            }
        }
        else
        {
            for (int i = 0; i<profile2P.Length; i++)
            {
                profile2P[i].sprite = profileSprites[imgNum];
            }
        }
    }
}
