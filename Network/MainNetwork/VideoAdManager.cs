using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using GoogleMobileAds.Api;
public class VideoAdManager : MonoBehaviour
{
    private RewardBasedVideoAd rewardBasedVideo;
    public UnityEngine.UI.Text star;
    public UnityEngine.UI.Text limitText;
    public UnityEngine.UI.Button adBtn;
    int curDay = 0;
    public void ShowAd()
    {
        if (rewardBasedVideo.IsLoaded())
        {
            UpdateCount();
            rewardBasedVideo.Show();
        }
        else
        {
            UpdateCount();
            MainNetworkManager.Instance.NoticeInfo("광고가 아직 로드되지 않았습니다.");
        }
    }
    void UpdateCount()
    {
        //다르면
        if (PlayerPrefs.GetInt("curDay") != curDay)
        {
            PlayerPrefs.SetInt("curDay", curDay);
            PlayerPrefs.SetInt("AdCount", 0);
        }
        limitText.text = "오늘 "+(5- PlayerPrefs.GetInt("AdCount"))+"회 남음";
        if(PlayerPrefs.GetInt("AdCount") >= 5)
        {
            adBtn.interactable = false;
        }
        else
        {
            adBtn.interactable = true;
        }
    }
    public void Start()
    {
        curDay = int.Parse(System.DateTime.Now.ToString("yyyyMMdd"));
        //print(curDay);
        if (!PlayerPrefs.HasKey("curDay"))
        {
            PlayerPrefs.SetInt("curDay", curDay);
            PlayerPrefs.SetInt("AdCount", 0);
        }
        //예전의 날짜와 같은지
        else
        {
            UpdateCount();
        }

        string appId;
#if UNITY_ANDROID
        appId = "ca-app-pub-5053720951454715~9202885647";
#elif UNITY_IPHONE
        appId = "ca-app-pub-5053720951454715~9560069715";
#else
        appId = "";
#endif

        MobileAds.Initialize(appId);

        // Get singleton reward based video ad reference.
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;

        // Called when an ad request has successfully loaded.
        rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        // Called when an ad is shown.
        rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
        // Called when the ad starts to play.
        rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        // Called when the ad click caused the user to leave the application.
        rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

        this.RequestRewardBasedVideo();
    }

    private void RequestRewardBasedVideo()
    {
        LoadAd();
    }
    void LoadAd()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-5053720951454715/3991677135";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-5053720951454715/8426456604";
#else
            string adUnitId = "unexpected_platform";
#endif
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, adUnitId);

    }
    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        UpdateCount();
        MonoBehaviour.print("HandleRewardBasedVideoLoaded event received");
    }

    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardBasedVideoFailedToLoad event received with message: "
                             + args.Message);
        //MainNetworkManager.Instance.NoticeInfo(args.Message);
    }

    public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoOpened event received");
    }

    public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoStarted event received");
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoClosed event received");
        this.RequestRewardBasedVideo();
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        //임시
        GameManager.Instance.EarnStar(10);
        PlayerPrefs.SetInt("AdCount", PlayerPrefs.GetInt("AdCount")+1);
        UpdateCount();
        star.text = GameManager.Instance.GetUser().money.ToString();
        MonoBehaviour.print(
            "HandleRewardBasedVideoRewarded event received for "
                        + amount.ToString() + " " + type);
    }

    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
    }
}