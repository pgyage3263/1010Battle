using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proyecto26;
public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        for (int i = 29; i >= 0; i--)
        {
            profiles[i].transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = priceOfProfiles[i].ToString();
        }
    }
    public GameObject[] profiles;
    public int[] priceOfProfiles;
    public GameObject loadingPanel;
    public Sprite[] profileImages;
    public Image[] mainProfiles;
    List<int> myProfiles;

    public Text star;
    public void UpdateProfile(int bitField)
    {
        star.text = GameManager.Instance.GetUser().money.ToString();
        myProfiles = new List<int>();
        for (int i = 29; i >= 0; i--)
        {
            int num = (int)Mathf.Pow(2, i);
            //걸렸을 경우
            if (bitField > num)
            {
                bitField -= num;
                ChangeState(i);
                myProfiles.Add(i);
            }
        }
    }
    //샀다는걸로
    void ChangeState(int index)
    {
        profiles[index].transform.GetChild(1).gameObject.SetActive(false);
        profiles[index].transform.GetChild(2).gameObject.SetActive(true);
    }
    public void InitProfile(int profileNum)
    {
        mainProfiles[0].sprite = profileImages[profileNum];
        mainProfiles[1].sprite = profileImages[profileNum];
    }
    public void PurchaseProfile(int index)
    {
        loadingPanel.SetActive(true);
        MainNetworkManager.Instance.NoticeInfo("구매 요청 중");
        User myUser = GameManager.Instance.GetUser();
        RestClient.Get<User>(url: "https://battle1010.firebaseio.com/users/" + myUser.id + ".json").Then(response =>
        {
            User me = response;

            //돈이 부족할 경우
            if (priceOfProfiles[index] > me.money)
            {
                MainNetworkManager.Instance.NoticeInfo("구매 실패(별 부족)");
            }
            //이미 가지고 있을 경우
            else if (myProfiles.Contains(index))
            {
                MainNetworkManager.Instance.NoticeInfo("구매 실패(이미 소유)");
            }
            //구매 성공
            else
            {
                myUser.profileStorage += (int)Mathf.Pow(2, index);
                myUser.money -= priceOfProfiles[index];
                GameManager.Instance.SetUser(myUser);
                MainNetworkManager.Instance.NoticeInfo("구매 성공");
                UpdateProfile(myUser.profileStorage);
            }
            //로딩 풀기
            loadingPanel.SetActive(false);
        });
    }
    public void EquipProfile(int index)
    {
        loadingPanel.SetActive(true);
        MainNetworkManager.Instance.NoticeInfo("장착 요청 중");
        User myUser = GameManager.Instance.GetUser();
        myUser.profileNum = index;
        GameManager.Instance.SetUser(myUser);
        UpdateProfile(myUser.profileStorage);
        InitProfile(index);
        //로딩 풀기
        loadingPanel.SetActive(false);
        MainNetworkManager.Instance.NoticeInfo("장착 성공");
    }
}
