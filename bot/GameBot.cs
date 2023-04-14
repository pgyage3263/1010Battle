using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//인게임에서의 Bot의 역할
//1. 자신의 턴이 시작되었을 경우, 2초 대기
//  1) 블록 세개 전수조사(공격력) 후 가장 높은 공격력을 가지는 블록을 해당 위치에 넣음, 2초 대기
//  2) 나의 턴일 경우, 두개"", 2초 대기
//  3) 나의 턴일 경우, 한개"", 2초 대기
//2. 게임이 끝났을 경우
//  1) 재도전걸기
//  2) 플레이어가 나갔을 경우 나가기
namespace NetworkScripts
{
    //블록 숫자들과 공격력
    public struct LandCandidate
    {
        public List<int> blockNums;
        public int attackPower;
        public GameObject block;
    }
    struct BlockCount
    {
        public GameObject block;
        public int count;
    }
    public class GameBot : MonoBehaviour
    {
        public static GameBot Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
        //2초 대기 후 다음으로
        IEnumerator DelaySeconds(int currentIndex)
        {
            yield return new WaitForSeconds(3);
            CheckBlocks(--currentIndex);
        }
        //나의 턴이 시작되었을 때 호출되게
        public void StartTurn()
        {
            StartCoroutine("DelaySeconds", 3);
        }
        //초기:2 -> 1 -> 0
        public void CheckBlocks(int index)
        {
            //내 턴이 아닐경우 끝내기
            if (TurnManager.Instance.isMyturn == false || TurnManager.Instance.IsEnd() == true)
                return;

            //블록을 놓을 수 있는 후보자들
            List<LandCandidate> candidates = new List<LandCandidate>();
            //후보자들을 넣기
            BoardManager.Instance.PutCandidateList(candidates);
            //고르기
            //공격할 수 있는게 있는지
            bool isAttackPossible = false;
            int finalIndex = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                int power = candidates[i].attackPower;
                //어떤 곳이 가장 강력한지 체크
                if (isAttackPossible == false && power > 0)
                {

                    isAttackPossible = true;
                    if (candidates[finalIndex].attackPower < power)
                    {
                        finalIndex = i;
                    }
                }
            }
            //공격할수 있는게 없는 경우
            if (isAttackPossible == false)
            {
                //놓을 수 있는 위치가 적은 블록의 랜덤 인덱스로 대체
                //블록 단위로 쪼개기
                List<BlockCount> blockCounts = new List<BlockCount>();
                foreach (LandCandidate item in candidates)
                {
                    bool isExist = false;
                    //여태까지의 블록에 없는지
                    for(int i=0; i<blockCounts.Count; i++)
                    {
                        if (blockCounts[i].block == item.block)
                        {
                            //Count ++
                            BlockCount bc = blockCounts[i];
                            bc.count++;
                            blockCounts[i] = bc;
                            isExist = true;
                            break;
                        }
                    }
                    //등록
                    if(isExist == false)
                    {
                        BlockCount bc = new BlockCount();
                        bc.block = item.block;
                        bc.count = 1;
                        blockCounts.Add(bc);
                    }

                }
                //가장 놓을 수 있는게 적은 블록 찾기
                GameObject leastBlock = null;
                int leastCount = 0;
                foreach(BlockCount bc in blockCounts)
                {
                    if (leastBlock == null)
                    {
                        leastBlock = bc.block;
                        leastCount = bc.count;
                    }
                    else
                    {
                        if(leastCount > bc.count)
                        {
                            leastBlock = bc.block;
                            leastCount = bc.count;
                        }
                    }
                }
                //해당 블록에대한 인덱스들 뽑기
                List<int> finalIndexes = new List<int>();
                for(int i = 0; i< candidates.Count; i++)
                {
                    if(candidates[i].block == leastBlock)
                    {
                        finalIndexes.Add(i);
                    }
                }

                finalIndex = finalIndexes[Random.Range(0, finalIndexes.Count)];
            }
            //공격 가능한 경우
            else
            {
                //0: 감사 3: ㅋㅋㅋ
                int randIdx = 3 * Random.Range(0, 2);
                EmotionManager.Instance.BOTClickSpeechBubble(randIdx);
                //finalIndex에 해당하는 블록들로 공격
            }
            //출력
            print(finalIndex + ", Count: " + candidates.Count);
            print("예상 공격력: " + candidates[finalIndex].attackPower);
            //해당 블록 후보 갖고오기
            LandCandidate lc = candidates[finalIndex];
            //착륙시키기
            lc.block.GetComponent<BlockMove>().BotLandBlock(lc.blockNums.ToArray());

            //List 비워주기
            candidates.Clear();

            //그 다음
            if (index > 0)
            {
                StartCoroutine("DelaySeconds", index);
            }
        }
    }
}