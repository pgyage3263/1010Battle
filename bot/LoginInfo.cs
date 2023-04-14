using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginInfo : MonoBehaviour
{
    public static LoginInfo Instance;
    public string id;
    public string pw;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void GetLoginInfo(string id, string pw)
    {
        this.id = id;
        this.pw = pw;
    }

}
