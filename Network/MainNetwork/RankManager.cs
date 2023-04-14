using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proyecto26;
//랭크 UI 컨트롤
public class RankManager : MonoBehaviour
{
    //버튼 컬러
    public Color activeColor;
    public Color inactiveColor;
    //버튼 색
    public Image expButton;
    public Image ratingButton;
    //랭크 UI
    public GameObject rankUI;
    //로딩패널
    public GameObject loadingPanel;
    User[] rankersInfo = new User[20];
    //닉네임 리스트
    public Text[] nickNames;
    //레벨들
    public Text[] levels;
    //프로필 이미지들
    public Image[] profileImages;
    void GetRankerInfo(bool isEXP)
    {
        if (isEXP)
        {
            RestClient.Get(url: "https://battle1010.firebaseio.com/users.json?orderBy=%22level%22&limitToLast=20").Then(response =>
             {
                 string data = response.Text;

                 //큰 중괄호 제거
                 data = data.Substring(1, data.Length - 2);
                 int prexEndIndex = 0;
                 for (int i = 0; i < rankersInfo.Length; i++)
                 {
                     int startIndex = data.IndexOf('{', prexEndIndex + 1);
                     int endIndex = data.IndexOf('}', prexEndIndex + 1);
                     prexEndIndex = endIndex;
                     string jsonData = data.Substring(startIndex, endIndex - startIndex + 1);
                     rankersInfo[i] = JsonUtility.FromJson<User>(jsonData);
                 }

                 ProcessRankers(isEXP);
             });
        }
        else
        {
            RestClient.Get(url: "https://battle1010.firebaseio.com/users.json?orderBy=%22rating%22&limitToLast=20").Then(response =>
            {
                string data = response.Text;

                //큰 중괄호 제거
                data = data.Substring(1, data.Length - 2);
                int prexEndIndex = 0;
                for (int i = 0; i < rankersInfo.Length; i++)
                {
                    int startIndex = data.IndexOf('{', prexEndIndex + 1);
                    int endIndex = data.IndexOf('}', prexEndIndex + 1);
                    prexEndIndex = endIndex;
                    string jsonData = data.Substring(startIndex, endIndex - startIndex + 1);
                    rankersInfo[i] = JsonUtility.FromJson<User>(jsonData);
                }

                ProcessRankers(isEXP);
            });
        }

    }
    //순위 매김.
    void ProcessRankers(bool isEXP)
    {
        if (isEXP)
        {
            for (int i = 0; i < rankersInfo.Length; i++)
            {
                for (int j = 0; j < rankersInfo.Length - i - 1; j++)
                {
                    if (rankersInfo[j].level < rankersInfo[j + 1].level)
                    {
                        //스왑
                        User temp = rankersInfo[j];
                        rankersInfo[j] = rankersInfo[j + 1];
                        rankersInfo[j + 1] = temp;
                    }
                    else if (rankersInfo[j].level == rankersInfo[j + 1].level)
                    {
                        //경험치 비교
                        if (rankersInfo[j].exp < rankersInfo[j + 1].exp)
                        {
                            //스왑
                            User temp = rankersInfo[j];
                            rankersInfo[j] = rankersInfo[j + 1];
                            rankersInfo[j + 1] = temp;
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < rankersInfo.Length; i++)
            {
                for (int j = 0; j < rankersInfo.Length - i - 1; j++)
                {
                    if (rankersInfo[j].rating < rankersInfo[j + 1].rating)
                    {
                        //스왑
                        User temp = rankersInfo[j];
                        rankersInfo[j] = rankersInfo[j + 1];
                        rankersInfo[j + 1] = temp;
                    }
                    else if (rankersInfo[j].rating == rankersInfo[j + 1].rating)
                    {
                        //경험치 비교
                        if (rankersInfo[j].exp < rankersInfo[j + 1].exp)
                        {
                            //스왑
                            User temp = rankersInfo[j];
                            rankersInfo[j] = rankersInfo[j + 1];
                            rankersInfo[j + 1] = temp;
                        }
                    }
                }
            }
        }
        ApplyRank(isEXP);
    }
    //화면에 나타나게
    void ApplyRank(bool isEXP)
    {
        for (int i = 0; i < rankersInfo.Length; i++)
        {
            nickNames[i].text = rankersInfo[i].nickName;
            if (isEXP)
                levels[i].text = "LV." + rankersInfo[i].level;
            else
            {
                levels[i].text = ((int)rankersInfo[i].rating).ToString();
            }
            profileImages[i].sprite = ProfileManager.Instance.profileImages[rankersInfo[i].profileNum];
        }
        loadingPanel.SetActive(false);
    }
    //랭크UI켜기
    public void ShowRankUI(bool isEXP)
    {
        rankUI.SetActive(true);
        //켤 때
        loadingPanel.SetActive(true);
        if (isEXP)
        {
            expButton.color = activeColor;
            ratingButton.color = inactiveColor;
        }
        else
        {
            ratingButton.color = activeColor;
            expButton.color = inactiveColor;
        }
        GetRankerInfo(isEXP);
    }
    public void CloseRankUI()
    {
        rankUI.SetActive(false);
    }

}
