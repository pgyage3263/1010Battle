using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using UnityEngine.UI;
//비밀번호 변경
public class PWChangeManager : MonoBehaviour
{
    //로그인 패널
    public GameObject loginPanel;
    //비밀번호 변경 패널
    public GameObject pwChangePanel;
    //4가지 단계
    public GameObject[] pages;
    //ID textfield
    public InputField idInput;
    //확인 코드
    public InputField codeInput;
    //비밀번호
    public InputField pwInput;
    //비밀번호확인
    public InputField pwCheckInput;
    //로딩 패널
    public GameObject loadingPanel;
    //메일 주소
    public Text mailAddressText;
    //유저
    User me;
    //코드
    string code = "";
    //비밀번호 변경 켜기
    public void ChangePWUI(bool isShow)
    {
        if (isShow)
        {
            pwChangePanel.SetActive(true);
            loginPanel.SetActive(false);
        }
        else
        {
            EndUI();
            loginPanel.SetActive(false);
            loginPanel.SetActive(true);
        }
    }
    //비밀번호 변경 끄기
    void EndUI()
    {
        me = null;
        code = "";
        //첫번째 페이지만 남김
        foreach (GameObject p in pages)
        {
            p.SetActive(false);
        }
        pages[0].SetActive(true);
        //패널 끄기
        pwChangePanel.SetActive(false);
        loadingPanel.SetActive(false);
        //기존 입력 정보 제거
        idInput.text = "";
        codeInput.text = "";
        pwInput.text = "";
        pwCheckInput.text = "";
        mailAddressText.text = "";
    }
    //eMail이 없는 오류
    void OnEMailInvalid()
    {
        MainNetworkManager.Instance.NoticeInfo("이메일이 등록되어 있지 않습니다. 개발자에게 문의하세요.");
        //끄기
        ChangePWUI(false);
    }
    //ID로 유저 갖고오기
    public void FirstNextPage()
    {
        string id = idInput.text;
        //유효한 ID인지 확인

        loadingPanel.SetActive(true);
        RestClient.Get<User>(url: "https://battle1010.firebaseio.com/users/" + id + ".json").Then(response =>
        {
            User user = response;
            //user가 들어오지 못했다면 계정이 없는 것
            if (user == null)
            {
                MainNetworkManager.Instance.NoticeInfo("유효하지 않은 계정입니다.");
                loadingPanel.SetActive(false);
                return;
            }
            //다음 단계로
            else
            {
                me = user;
                //이 유저에게 email이 없다면
                if (me.eMail.Equals(""))
                {
                    OnEMailInvalid();
                }
                //이메일이 있다면 -> 다음 단계로
                else
                {
                    SendEMailCode(me.eMail);
                }
            }
        }
        );
    }
    void SendEMailCode(string toAddress)
    {
        //코드 생성
        string code = "";
        for (int i = 0; i < 6; i++)
        {
            //문자 or 숫자
            int isNumber = Random.Range(0, 2);

            char randomC;
            if (isNumber == 0)
                randomC = (char)Random.Range(48, 58);
            else
                randomC = (char)Random.Range(65, 91);

            code += randomC;
        }
        this.code = code;
        //이메일 전송
        EMailManager.Instance.SendMail(toAddress, code);
        //로딩패널끄기
        loadingPanel.SetActive(false);
        mailAddressText.text = "이메일 주소: " + me.eMail;
        //다음 단계로
        pages[0].SetActive(false);
        pages[1].SetActive(true);
    }
    //2->3
    public void SecondNextPage()
    {
        string inputCode = codeInput.text;
        char[] inputCodeCharArr = inputCode.ToCharArray();
        inputCode = "";
        for (int i= 0; i < inputCodeCharArr.Length; i++)
        {
            int c = (int)inputCodeCharArr[i];
            //소문자 -> 대문자
            if (c >= 97 && c <= 122)
            {
                inputCodeCharArr[i] = (char)(c-32);
            }
            inputCode += inputCodeCharArr[i];
        }
        print(inputCode);
        //코드 확인
        if (code.Equals(inputCode))
        {
            MainNetworkManager.Instance.NoticeInfo("코드 인증 완료.");
            //다음 단계로
            pages[1].SetActive(false);
            pages[2].SetActive(true);
        }
        else
        {
            MainNetworkManager.Instance.NoticeInfo("코드가 일치하지 않습니다.");
        }
    }
    public void ThirdNextPage()
    {
        string pw = pwInput.text;
        char[] pwCharArr = pw.ToCharArray();

        //비번이랑 비번 확인이 서로 다를 경우
        if (false == pw.Equals(pwCheckInput.text))
        {
            MainNetworkManager.Instance.NoticeInfo("비밀번호와 비밀번호 확인이 다릅니다.");
            return;
        }
        foreach (char c in pwCharArr)
        {
            int asciiC = (int)c;
            if (asciiC < 48 || (asciiC > 57 && asciiC < 97) || asciiC > 122)
            {
                MainNetworkManager.Instance.NoticeInfo("비밀번호는 영문 소문자와 숫자로만 이루어져야 합니다.");
                return;
            }
        }
        //비밀번호가 15자리 이상일 경우
        if (pw.Length > 15)
        {
            MainNetworkManager.Instance.NoticeInfo("비밀번호는 15자리를 초과할 수 없습니다.");
            return;
        }
        //SHA256화
        string hashedPW = MainNetworkManager.Instance.SHA256Hash(pw);
        me.password = hashedPW;
        //서버에 저장
        RestClient.Put(url: "https://battle1010.firebaseio.com/users/" + me.id + ".json", me);

        //다음 단계로
        pages[2].SetActive(false);
        pages[3].SetActive(true);
    }

}
