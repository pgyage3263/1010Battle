using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SerializeField]
public class User
{
    //생성자
    public User(string id, string password, string nickName)
    {
        this.id = id;
        this.password = password;
        this.nickName = nickName;
    }
    //회원 정보
    public string id;
    public string password;
    public string nickName;
    public string eMail;
    //인게임 정보
    public int level = 1;
    public int money = 100;
    public int exp = 0;
    public float rating = 1000.0f;
    public int winCount = 0;
    public int loseCount = 0;
    //프로필 이미지
    public int profileNum = 0;
    public int profileStorage = 1;
    //스위치 변수
    public bool connectSW = false;
}
