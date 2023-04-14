using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace NetworkScripts
{
    //블럭의 형태 정보
    struct BlockInfo
    {
        public bool[,] block;
    }
    public class BoardManager : MonoBehaviourPunCallbacks
    {
        public static BoardManager Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        public Dictionary<int, GroundBlock> blocksDic;
        //board의 block들
        public GroundBlock[] blocks;
        //겉 block들
        public GameObject[] visualBlocks;
        public GameObject profile_1P;
        public GameObject profile_2P;
        public GameObject gageFX_1P;
        public GameObject gageFX_2P;
        public GameObject touchShield;

        public AnimationCurve disappearCurve;
        //보드의 어디에 놓여지는지
        bool[,] board;
        //BlockInfo
        BlockInfo[] blockInfos;
        // Start is called before the first frame update
        void Start()
        {
            blocksDic = new Dictionary<int, GroundBlock>();
            //추가
            for (int i = 0; i < blocks.Length; i++)
                blocksDic.Add(i, blocks[i]);
            //보드 할당
            board = new bool[10, 10];
            //BlockInfo할당 및 초기화
            AddBlockInfo();
        }
        void AddBlockInfo()
        {
            //blockInfos 할당
            blockInfos = new BlockInfo[12];
            //ㅁ
            blockInfos[0].block = new bool[1, 1] { { true } };
            //ㅁ
            //ㅁ
            blockInfos[1].block = new bool[2, 1] { { true }, { true } };
            //ㅁ
            //ㅁ
            //ㅁ
            blockInfos[2].block = new bool[3, 1] { { true }, { true }, { true } };
            //ㅁㅁ
            //ㅁㅇ
            //ㅁㅇ
            blockInfos[3].block = new bool[3, 2] { { true, true }, { true, false }, { true, false } };
            //ㅁㅁ
            //ㅁㅇ
            blockInfos[4].block = new bool[2, 2] { { true, true }, { true, false } };
            //ㅁㅁㅁ
            //ㅇㅁㅇ
            blockInfos[5].block = new bool[2, 3] { { true, true, true }, { false, true, false } };
            //ㅁㅁ
            //ㅁㅁ
            blockInfos[6].block = new bool[2, 2] { { true, true }, { true, true } };
            //ㅁㅁㅁ
            //ㅁㅁㅁ
            //ㅁㅁㅁ
            blockInfos[7].block = new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } };
            //ㅇㅇㅁ
            //ㅇㅇㅁ
            //ㅁㅁㅁ
            blockInfos[8].block = new bool[3, 3] { { false, false, true }, { false, false, true }, { true, true, true } };
            //ㅇㅁ
            //ㅁㅇ
            blockInfos[9].block = new bool[2, 2] { { false, true }, { true, false } };
            //ㅇㅁ
            //ㅁㅇ
            blockInfos[10].block = new bool[2, 2] { { false, true }, { true, false } };
            //ㅁㅇㅁ
            //ㅇㅁㅇ
            blockInfos[11].block = new bool[2, 3] { { true, false, true }, { false, true, false } };
        }
        public void OnFillGroundBlock(int num, int blockIdx)
        {
            photonView.RPC("FillGroundBlock", RpcTarget.AllBuffered, num, blockIdx);
        }
        [PunRPC]
        public void FillGroundBlock(int num, int blockIdx)
        {
            blocksDic[num].CompleteLandBlock(blockIdx);
        }
        //블록을 놓았을 경우 호출됨.
        public void OnBlockLanded(int[] blockNums, int playerNum)
        {
            foreach (int num in blockNums)
            {
                board[num / 10, num % 10] = true;
            }
            CheckBlock(playerNum);

        }
        public GameObject autoTurnEndUI;
        IEnumerator AutoTurnEnd(int playerNum)
        {
            autoTurnEndUI.SetActive(true);
            //일정 시간동안 턴 종료 대기
            yield return new WaitForSeconds(0.3f);
            TurnManager.Instance.TurnEndButton(playerNum);
            //일정 시간동안 UI키고있음
            yield return new WaitForSeconds(1.4f);
            autoTurnEndUI.SetActive(false);
        }
        public void CheckTurnEnd()
        {
            //게임 끝났다면 체크하지 말기
            if (isEndProcess) return;

            //내 턴일 때
            if (TurnManager.Instance.isMyturn)
            {
                BlockPosition[] blockPositions;
                if (NetworkManager.Instance.myPlayerNum == 0)
                {
                    blockPositions = BlockCreateManager.Instance.blockPositions_1P;
                }
                else
                {
                    blockPositions = BlockCreateManager.Instance.blockPositions_2P;
                }
                //게임을 계속 진행할 수 있는지
                bool isPlayable = false;
                foreach (BlockPosition bp in blockPositions)
                {
                    //비어있지 않을 경우에 = 아직 놓지 않았을 경우
                    if (bp.isEmpty == false && bp.block != null)
                    {
                        int blockNum = bp.block.GetComponent<BlockMove>().blockNum;
                        int rotNum = bp.block.GetComponent<BlockMove>().rotNum;
                        //이 블럭을 놓을 수 있는지
                        bool isLandable = GetLandPossible(blockNum, rotNum);
                        //하나라도 놓을 수 있으면 계속 진행
                        if (isLandable == true)
                        {
                            isPlayable = true;
                            break;
                        }
                    }
                }
                //아무 것도 놓을 수 없다면 턴 종료
                if (isPlayable == false)
                {
                    StartCoroutine("AutoTurnEnd", NetworkManager.Instance.myPlayerNum);
                }

            }
        }
        //대각선 블록
        public int DestroyBlocks(bool isUpRight, int playerNum)
        {
            int destroyCount = 0;
            //우상향
            if (isUpRight)
            {
                for (int i = 0; i < 10; i++)
                {
                    board[9 - i, i] = false;
                    bool isSuccess = blocks[(9 - i) * 10 + i].OnDisappear(playerNum, true);
                    if (isSuccess) destroyCount++;
                }
            }
            //좌햐향
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    board[i, i] = false;
                    bool isSuccess = blocks[10 * i + i].OnDisappear(playerNum, true);
                    if (isSuccess) destroyCount++;
                }
            }
            SFXManager.Instance.OnLineClear();
            return destroyCount;
        }
        //블록이 사라질 때
        //행:true / 열: false, num: 몇번째?
        public int DestroyBlocks(bool isRow, int num, int playerNum)
        {
            bool isRage = false;
            int damage = 1;
            //공격자의 체력이 20이하일 때
            if ((playerNum == 0 && HPManager.Instance.HP_1p <= 20) || (playerNum == 1 && HPManager.Instance.HP_2p <= 20))
            {
                isRage = true;
                damage = 2;
            }

            int destroyCount = 0;
            if (isRow)
            {
                for (int i = 0; i < 10; i++)
                {
                    board[num, i] = false;
                    bool isSuccess = blocks[num * 10 + i].OnDisappear(playerNum, isRage);
                    if (isSuccess) destroyCount += damage;
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    board[i, num] = false;
                    bool isSuccess = blocks[10 * i + num].OnDisappear(playerNum, isRage);
                    if (isSuccess) destroyCount += damage;
                }
            }
            //사운드
            SFXManager.Instance.OnLineClear();
            return destroyCount;
        }
        //게임이 끝날 예정인지
        public bool isEndProcess = false;
        //마지막인지 확인
        void CheckBlock(int playerNum)
        {
            List<int> blockLineList = new List<int>();
            //대각선 검사
            //우상향
            for (int i = 0; i < 10; i++)
            {
                if (board[9 - i, i] == false)
                    break;
                else if (i == 9)
                {
                    blockLineList.Add(20);
                }
            }
            //좌하향
            for (int i = 0; i < 10; i++)
            {
                if (board[i, i] == false)
                    break;
                else if (i == 9)
                {
                    blockLineList.Add(21);
                }
            }
            //행검사(0~9)
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (board[i, j] == false)
                    {
                        break;
                    }
                    else if (j == 10 - 1)
                    {
                        //기록
                        blockLineList.Add(i);
                    }
                }
            }
            //열검사(10~19)
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (board[j, i] == false)
                    {
                        break;
                    }
                    else if (j == 10 - 1)
                    {
                        //기록
                        blockLineList.Add(10 + i);
                    }
                }
            }
            //대각선 검사

            //행 먼저 검사: X10 -> 열 검사: X(10-행 갯수)
            int hitCount = 0;
            //int rowCount = 0;
            //
            //가로 세로 라인
            //제거 => 마지막인지 확인가능.
            foreach (int lineNum in blockLineList)
            {
                if (lineNum < 10)
                {
                    hitCount += DestroyBlocks(true, lineNum, playerNum);
                    //rowCount++;
                    //hitCount += 10;
                }
                else if (lineNum < 20)
                {
                    hitCount += DestroyBlocks(false, lineNum - 10, playerNum);
                    //hitCount += (10 - rowCount);
                }
                else if (lineNum == 20)
                {
                    hitCount += 2 * DestroyBlocks(true, playerNum);
                }
                else if (lineNum == 21)
                {
                    hitCount += 2 * DestroyBlocks(false, playerNum);
                }
            }
            //2P 죽을 때
            if (playerNum == 0)
            {
                if (HPManager.Instance.HP_2p <= hitCount)
                {
                    Time.timeScale = 0.3f;
                    isEndProcess = true;
                    //모으는 이펙트
                    gageFX_2P.SetActive(true);
                    SFXManager.Instance.OnPlayerDeadGage();
                    //터치 못하게막기
                }
            }
            //1P 죽을 때
            else
            {
                if (HPManager.Instance.HP_1p <= hitCount)
                {
                    Time.timeScale = 0.3f;
                    isEndProcess = true;
                    //모으는 이펙트
                    gageFX_1P.SetActive(true);
                    SFXManager.Instance.OnPlayerDeadGage();
                    //터치 못하게막기
                    touchShield.SetActive(true);
                }
            }

        }
        //검사한 블록 출력
        void PrintBlock(bool[,] thisBlock, int blockNum, int rotNum)
        {
            //임시로 프린트해보기
            for (int i = 0; i < thisBlock.GetLength(0); i++)
            {
                string str = "";
                for (int j = 0; j < thisBlock.GetLength(1); j++)
                {
                    if (thisBlock[i, j] == true)
                    {
                        str += "ㅁ";
                    }
                    else str += "ㅇ";
                }
                print(str);
            }
            print("블록 번호: " + blockNum + ", 회전 횟수: " + rotNum);

        }
        //인자: 블록의 종류, 몇번 회전하는지
        public bool GetLandPossible(int blockNum, int rotNum)
        {
            //블록을 구해옴
            bool[,] thisBlock = GetFinalBlock(blockInfos[blockNum].block, rotNum);
            //디버그 프린트
            //PrintBlock(thisBlock, blockNum, rotNum);
            //보드에서 돌리기
            bool isLandable = TestLandBlock(thisBlock);
            return isLandable;
        }
        //회귀 함수로 회전한 결과물 블록의 형태 return
        bool[,] GetFinalBlock(bool[,] prevBlock, int rotNum)
        {
            //회전을 마친 경우
            if (rotNum == 0)
            {
                return prevBlock;
            }
            //회전 진행중
            else
            {
                int rowCount = prevBlock.GetLength(0);
                int colCount = prevBlock.GetLength(1);
                //행 => 열,  열 => 행
                bool[,] resultBlock = new bool[colCount, rowCount];
                //prev => after
                //행
                for (int i = 0; i < rowCount; i++)
                {
                    //열
                    for (int j = 0; j < colCount; j++)
                    {
                        resultBlock[colCount - j - 1, i] = prevBlock[i, j];
                    }
                }
                //회전 횟수
                rotNum--;
                //반환
                return GetFinalBlock(resultBlock, rotNum);
            }
        }
        bool TestLandBlock(bool[,] block)
        {
            //행 개수
            int rowCount = block.GetLength(0);
            //열 개수
            int colCount = block.GetLength(1);
            //보드의 열
            for (int i = 0; i < 10 - rowCount + 1; i++)
            {
                //보드의 행
                for (int j = 0; j < 10 - colCount + 1; j++)
                {
                    bool isLandable = true;
                    //보드 위치 하나에 대해서 블록 비교
                    for (int r = 0; r < rowCount; r++)
                    {
                        for (int c = 0; c < colCount; c++)
                        {
                            //print(rowCount + ", " + colCount);

                            //블락이 true인 부분이 board가 true라면 안됨.
                            if (block[r, c] == true && board[i + r, j + c] == true)
                            {
                                isLandable = false;
                                //print(2);
                            }
                        }
                    }
                    //맞는 부분이 있다면 반환
                    if (isLandable)
                    {
                        //print(1);
                        return true;
                    }
                    else
                    {
                        //print(0);
                    }

                }
            }
            //끝까지 맞는 부분이 없었을 경우
            return false;
        }
    }
}