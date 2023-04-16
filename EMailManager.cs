using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class EMailManager : MonoBehaviour
{
    //싱글톤
    public static EMailManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    public void SendMail(string toAddress, string body)
    {
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress("rjsdud3263@gmail.com"); // 보내는사람

        mail.To.Add(toAddress); // 받는 사람

        mail.Subject = "[텐텐 배틀 온라인] 비밀번호 변경관련 이메일 인증 코드";
        mail.Body = body;
        // 첨부파일 - 대용량은 안됨.
        //System.Net.Mail.Attachment attachment;
        //attachment = new System.Net.Mail.Attachment("D:\\Test\\2018-06-11-09-03-17-E7104.mp4"); // 경로 및 파일 선택
        //mail.Attachments.Add(attachment);
        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new System.Net.NetworkCredential("rjsdud3263@gmail.com", "pppppppp") as ICredentialsByHost; // 보내는사람 주소 및 비밀번호 확인
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
        delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        { return true; };

        smtpServer.Send(mail);

        Debug.Log("success");

    }

}
