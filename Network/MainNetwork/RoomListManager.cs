using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;
public class RoomListManager : MonoBehaviour
{
    public static RoomListManager Instance;
    public GameObject[] roomButtons;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    public List<RoomInfo> roomList = new List<RoomInfo>();
    // Start is called before the first frame update
    //생성할 수 있는 방인지 체크
    public bool CheckRoomName(string roomName)
    {
        foreach(RoomInfo ro in roomList)
        {
            string thisRoomName = ro.Name;
            //비밀 방이라면
            if (ro.Name.Contains("_"))
                thisRoomName = thisRoomName.Split('_')[0];
            //이미 존재하는 방 이름이라면
            if (thisRoomName.Equals(roomName))
            {
                return false;
            }
        }
        return true;
    }
    public void RefreshRoomList(List<RoomInfo> refreshRooms)
    {
        //업데이트 될 방들
        foreach (RoomInfo newRoom in refreshRooms)
        {
            //bool isExist = false;
            if (roomList.Contains(newRoom))
            {
                //제거해야 할 방이라면
                if (newRoom.RemovedFromList)
                {
                    roomList.Remove(newRoom);
                    //isExist = true;
                }
                else
                {
                    roomList[roomList.IndexOf(newRoom)] = newRoom;
                }
            }
            //새로운 방
            else if(!newRoom.RemovedFromList)
            {
                roomList.Add(newRoom);
            }
            ////기존 방들
            //foreach (RoomInfo room in roomList)
            //{
            //    //중복 -> 제거
            //    if (newRoom.Name.Equals(room.Name))
            //    {
            //        //제거해야 할 방이라면
            //        if (room.RemovedFromList)
            //        {
            //            roomList.Remove(room);
            //            isExist = true;
            //            break;
            //        }
            //        roomList.Remove(room);
            //        isExist = true;
            //        break;
            //    }
            //}
            //새로운 방이라면 추가
            //if (isExist == false)
            //{
            //    roomList.Add(newRoom);
            //}

        }
        //방 접속자가 0명이면 삭제
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount == 0)
            {
                roomList.Remove(roomList[i]);
                //앞으로 당겨질 경우 처리 못한 부분 방지를 위해
                i--;
            }
        }
        //버튼들 새로고침
        RefreshButtons();
    }
    void RefreshButtons()
    {
        //roomList.count < roomButtons.Length 라는 전제하에
        for(int i=0; i<roomList.Count; i++)
        {
            roomButtons[i].SetActive(true);
            RoomButton roomButton = roomButtons[i].GetComponent<RoomButton>();
            //roomButton.roomText.text = roomList[i].Name;
            string thisRoomName = roomList[i].Name;
            //비밀 방의 경우 비밀번호 보이지 않게
            //비밀 방 시각화
            if (thisRoomName.Contains("_"))
            {
                roomButton.roomText.text = thisRoomName.Split('_')[0];
                roomButton.privateRoomObj.SetActive(true);
            }
            else
            {
                roomButton.roomText.text = thisRoomName;
                roomButton.privateRoomObj.SetActive(false);
            }
            
            roomButton.countOfPlayersInRoom.text = roomList[i].PlayerCount + "/2";
            //open이 아닐 경우
            if (roomList[i].IsOpen == false)
            {
                //비활성화
                roomButton.GetComponent<Button>().interactable = false;
                //시각화
                roomButton.disabledObj.SetActive(true);

            }
            else
            {
                //활성화
                roomButton.GetComponent<Button>().interactable = true;
                //시각화
                roomButton.disabledObj.SetActive(false);

            }
        }
        for(int i=roomList.Count; i<roomButtons.Length; i++)
        {
            roomButtons[i].SetActive(false);
        }
    }
    public GameObject privateRoomEnterUI;
    public GameObject publicRoomEnterUI;
    public Text roomNoticeText;
    public Text privateroomNoticeText;
    public void OnClickRoomButton(Text roomNameText)
    {
        roomName = roomNameText.text;
        foreach (RoomInfo ro in roomList)
        {
            string thisRoomName = ro.Name;
            //방 찾음
            if (ro.Name.Contains(roomName))
            {
                //비밀 방이라면
                if (ro.Name.Contains("_"))
                {
                    privateroomNoticeText.text = "\"" + roomName + "\" 방에 입장하시겠습니까?";
                    //비밀방 입장 UI띄우기
                    MainNetworkManager.Instance.ShowPrivateRoomEnterUI(true);
                    //privateRoomEnterUI.SetActive(true);
                    return;
                }
                //일반 방이라면
                else
                {
                    roomNoticeText.text = "\"" + roomName + "\"방에 입장하시겠습니까?";
                    publicRoomEnterUI.SetActive(true);
                    return;
                }
            }
        }
        //MainNetworkManager.Instance.JoinRoom(roomName);
    }
    string roomName;
    //인자: 비밀번호
    public void EnterPrivateRoom(string roomPw)
    {
        privateRoomEnterUI.SetActive(false);
        //print(roomName + "_" + roomPw);
        MainNetworkManager.Instance.JoinRoom(roomName +"_" + roomPw);
    }
    public void EnterPublicRoom()
    {
        publicRoomEnterUI.SetActive(false);
        MainNetworkManager.Instance.JoinRoom(roomName);
    }
}
