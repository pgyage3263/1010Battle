using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace NetworkScripts
{
    //블럭 포지션 구조체
    public struct BlockPosition
    {
        //기본 위치
        public Vector3 position;
        //비어있는지
        public bool isEmpty;
        //실제 블록
        public GameObject block;
        
    };
    public class BlockCreateManager : MonoBehaviourPunCallbacks
    {
        //블록 프리팹들
        public GameObject[] blockFactories;
        //블록 머트리얼들
        public Material[] blockMaterials;
        //싱글톤
        public static BlockCreateManager Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
        public Vector3[] blockVectors_1P;
        public Vector3[] blockVectors_2P;
        public BlockPosition[] blockPositions_1P;
        public BlockPosition[] blockPositions_2P;
        //BlockPosition State를 바꿈
        [PunRPC]
        public void ChangeBPState(int playerNum, int posIndex, bool isEmpty)
        {
            //print("playerNum: " + playerNum + ", posIndex: " + posIndex + ", isEmpty: " + isEmpty);
            if (playerNum == 0)
            {
                blockPositions_1P[posIndex].isEmpty = isEmpty;
                //자동 턴종료
                //내 턴일 때
                if (TurnManager.Instance.isMyturn && NetworkManager.Instance.myPlayerNum == 0)
                {
                    bool isAllEmpty = true;
                    foreach (BlockPosition bp in blockPositions_1P)
                    {
                        if (bp.isEmpty == false)
                        {
                            isAllEmpty = false;
                            break;
                        }
                    }
                    if (isAllEmpty == true)
                    {
                        TurnManager.Instance.TurnEndButton(0);
                        return;
                    }
                }
            }
            else
            {
                blockPositions_2P[posIndex].isEmpty = isEmpty;
                //자동 턴종료
                //내 턴일 때
                if (TurnManager.Instance.isMyturn && NetworkManager.Instance.myPlayerNum == 1)
                {
                    bool isAllEmpty = true;
                    foreach (BlockPosition bp in blockPositions_2P)
                    {
                        if (bp.isEmpty == false)
                        {
                            isAllEmpty = false;
                            break;
                        }
                    }
                    if (isAllEmpty == true)
                    {
                        TurnManager.Instance.TurnEndButton(1);
                        return;
                    }
                }
            }
            //자신이 블록을 놓았을 경우 자동 턴 종료 체크
            if(isEmpty == true && playerNum == NetworkManager.Instance.myPlayerNum)
                BoardManager.Instance.CheckTurnEnd();
        }
        //블록 생성 함수
        public int CreateBlock(int playerNum)
        {
            int targetBlockIndex;
            //33ban
            bool is33Ban = false;
            if(playerNum >= 100)
            {
                playerNum -= 100;
                is33Ban = true;
            }
            //1P일 경우
            if (playerNum == 0)
            {
                for (targetBlockIndex = 0; targetBlockIndex < blockPositions_1P.Length; targetBlockIndex++)
                {
                    //비어있는 위치 찾았다면 탈출
                    if (blockPositions_1P[targetBlockIndex].isEmpty == true)
                    {
                        break;
                    }
                    //비어있는 위치가 없다면
                    else if (targetBlockIndex == blockPositions_1P.Length - 1)
                    {
                        //false
                        return 100;
                    }
                }
                //블록 생성
                //랜덤 인덱스 생성
                int randIndex = Random.Range(0, blockFactories.Length + 2);
                if(randIndex == 7 && is33Ban == true)
                {
                    while (true)
                    {
                        randIndex = Random.Range(0, blockFactories.Length + 2);
                        if (randIndex != 7)
                            break;
                    }
                }
                int randRotIndex = Random.Range(0, 4);
                int randYRot = randRotIndex * 90;
                //blockPositions_1P[targetBlockIndex].isEmpty = false;
                photonView.RPC("ChangeBPState", RpcTarget.AllBuffered, 0, targetBlockIndex, false);
                //블록 생성
                GameObject block;
                if (randIndex >= blockFactories.Length)
                {
                    randIndex = 0;
                    block = PhotonNetwork.Instantiate(blockFactories[0].name, blockPositions_1P[targetBlockIndex].position, Quaternion.Euler(90, -randYRot, 0));
                }
                else
                    block = PhotonNetwork.Instantiate(blockFactories[randIndex].name, blockPositions_1P[targetBlockIndex].position, Quaternion.Euler(90, -randYRot, 0));

                //정보 전달
                block.GetComponent<BlockMove>().InitInfo(randIndex, randRotIndex);
                return randIndex;
            }
            //2P일 경우
            else
            {
                for (targetBlockIndex = 0; targetBlockIndex < blockPositions_1P.Length; targetBlockIndex++)
                {
                    //비어있는 위치 찾았다면 탈출
                    if (blockPositions_2P[targetBlockIndex].isEmpty == true)
                    {
                        break;
                    }
                    //비어있는 위치가 없다면
                    else if (targetBlockIndex == blockPositions_2P.Length - 1)
                    {
                        //false
                        return 100;
                    }
                }
                //블록 생성
                //랜덤 인덱스 생성
                int randIndex = Random.Range(0, blockFactories.Length + 2);
                if (randIndex == 7 && is33Ban == true)
                {
                    while (true)
                    {
                        randIndex = Random.Range(0, blockFactories.Length + 2);
                        if (randIndex != 7)
                            break;
                    }
                }
                //랜덤 회전값
                int randRotIndex = Random.Range(0, 4);
                int randYRot = randRotIndex * 90;
                photonView.RPC("ChangeBPState", RpcTarget.AllBuffered, 1, targetBlockIndex, false);
                GameObject block;
                //블록 생성 -> 동기화 생성
                if (randIndex >= blockFactories.Length)
                {
                    randIndex = 0;
                    block = PhotonNetwork.Instantiate(blockFactories[0].name, blockPositions_2P[targetBlockIndex].position, Quaternion.Euler(90, -randYRot, 0));
                }
                else
                    block = PhotonNetwork.Instantiate(blockFactories[randIndex].name, blockPositions_2P[targetBlockIndex].position, Quaternion.Euler(90, -randYRot, 0));

                //2p는 거꾸로기에 2를 더하거나 빼서 보냄(180도 회전)XXX 원래대로 보내는게 맞음
                //연산
                //randRotIndex = (randRotIndex >= 2) ? randRotIndex - 2 : randRotIndex + 2;
                //정보 전달
                block.GetComponent<BlockMove>().InitInfo(randIndex, randRotIndex);

                return randIndex;
            }
        }
        //블록이 놓여졌을 때 -> Turn과 관계없이 자신이 Land했을 경우만 불러오게하자. 추후 수정
        public void OnLandBlock(int posIndex, int playerNum)
        {
            if (playerNum == NetworkManager.Instance.myPlayerNum)
                photonView.RPC("ChangeBPState", RpcTarget.AllBufferedViaServer, playerNum, posIndex, true);
        }
        public void OnTurnEnd(int playerNum)
        {
            //1P
            if (playerNum == 0)
            {
                for (int i = 0; i < blockPositions_1P.Length; i++)
                {
                    if (blockPositions_1P[i].isEmpty == false)
                    {
                        blockPositions_1P[i].block.GetComponent<BlockMove>().DestroySelf();
                        blockPositions_1P[i].isEmpty = true;
                    }
                }
            }
            //2P
            else
            {
                for (int i = 0; i < blockPositions_2P.Length; i++)
                {
                    if (blockPositions_2P[i].isEmpty == false)
                    {
                        blockPositions_2P[i].block.GetComponent<BlockMove>().DestroySelf();
                        blockPositions_2P[i].isEmpty = true;
                    }
                }
            }
        }
        public void Init()
        {
            //blockPosition 초기화
            blockPositions_1P = new BlockPosition[3];
            blockPositions_2P = new BlockPosition[3];
            for (int i = 0; i < 3; i++)
            {
                blockPositions_1P[i].isEmpty = true;
                blockPositions_1P[i].position = blockVectors_1P[i];
                blockPositions_2P[i].isEmpty = true;
                blockPositions_2P[i].position = blockVectors_2P[i];
            }
            //초기 생성
            if (NetworkManager.Instance.myPlayerNum == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    bool is33Ban = false;

                    if (is33Ban == true)
                    {
                        CreateBlock(100);
                    }
                    else
                    {
                        int blockIdx = CreateBlock(0);
                        if(blockIdx == 7)
                        {
                            is33Ban = true;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    bool is33Ban = false;

                    if (is33Ban == true)
                    {
                        CreateBlock(101);
                    }
                    else
                    {
                        int blockIdx = CreateBlock(1);
                        if (blockIdx == 7)
                        {
                            is33Ban = true;
                        }
                    }
                }
            }

        }
        
    }
}
