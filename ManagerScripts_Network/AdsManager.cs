using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
namespace NetworkScripts
{
    public class AdsManager : MonoBehaviour
    {
        public static AdsManager Instance;
        private InterstitialAd interstitial;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        public void Start()
        {
#if UNITY_ANDROID
            string appId = "ca-app-pub-5053720951454715~9202885647";
#elif UNITY_IPHONE
            string appId = "ca-app-pub-5053720951454715~9560069715";
#else
            string appId = "unexpected_platform";
#endif

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(appId);
            this.RequestInterstitial();
        }


        private void RequestInterstitial()
        {
#if UNITY_ANDROID
            string adUnitId = "ca-app-pub-5053720951454715/2837048570";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-5053720951454715/7304946623";
#else
        string adUnitId = "unexpected_platform";
#endif

            // Initialize an InterstitialAd.
            this.interstitial = new InterstitialAd(adUnitId);
            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the interstitial with the request.
            this.interstitial.LoadAd(request);
        }

        public void ShowAd()
        {
            if (this.interstitial.IsLoaded())
            {
                this.interstitial.Show();
            }
        }
    }
}
