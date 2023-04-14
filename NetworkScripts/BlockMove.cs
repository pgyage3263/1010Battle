using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace NetworkScripts
{
    public class BlockMove : MonoBehaviourPunCallbacks
    {
        //블록을 터치하고, 움직이고, 착지시키는 스크립트.

        //블록의 종류를 어떻게 받아오지?

        //블록의 위치 속성
        //현재 블록의 상태: 놓여진 상태, 터치했을 때(움직이고 있을 때), 착지
        public enum BlockState
        {
            Idle,
            Move,
            Land
        };
        //블락 인덱스
        public int blockIdx;
        public BlockState blockState;
        Vector3 originPos;
        int posIndex;
        //블럭-손가락 거리
        float finger2BlockDist = 0.5f;
        BlockCell[] blockCells;
        //1P, 2P 블록 머트리얼 가져오기
        //public Material redMat;
        //public Material blueMat;
        Camera mainCam;
        // Start is called before the first frame update
        void Start()
        {
            mainCam = Camera.main;
            originPos = transform.position;
            InitIndex();

        }
        public int blockNum;
        public int rotNum;
        int playerNum;
        public void InitInfo(int blockNum, int rotNum)
        {
            this.blockNum = blockNum;
            this.rotNum = rotNum;
        }
        public void InitIndex()
        {
            //posIndex 찾기
            if (originPos.x < -1.0f) posIndex = 0;
            else if (originPos.x < 1.0f) posIndex = 1;
            else posIndex = 2;
            //posIndex = posNum;
            blockState = BlockState.Idle;
            if ((photonView.IsMine && PhotonNetwork.IsMasterClient) || (!photonView.IsMine && !PhotonNetwork.IsMasterClient))
            {
                InitColor(0);
                BlockCreateManager.Instance.blockPositions_1P[posIndex].block = gameObject;
            }
            else
            {
                InitColor(1);
                BlockCreateManager.Instance.blockPositions_2P[posIndex].block = gameObject;
            }
        }
        public void InitColor(int playerNum)
        {
            this.playerNum = playerNum;
            blockCells = GetComponentsInChildren<BlockCell>();
            //자신 Block의 Material로 넣기.
            foreach (BlockCell bc in blockCells)
            {
                Material blockMat = BlockCreateManager.Instance.blockMaterials[blockIdx];
                bc.GetMaterial(blockMat);
                //bc.GetComponent<MeshRenderer>().material = blueMat;
            }
            ////2P일 경우
            //if (playerNum == 1)
            //{
            //    foreach (BlockCell bc in blockCells)
            //    {
            //        bc.GetMaterial(blueMat);
            //        //bc.GetComponent<MeshRenderer>().material = blueMat;
            //    }
            //}
            //else
            //{
            //    foreach (BlockCell bc in blockCells)
            //    {
            //        bc.GetMaterial(redMat);
            //    }
            //}
        }
        // Update is called once per frame
        void Update()
        {
            switch (blockState)
            {
                case BlockState.Idle:
                    UpdateIdle();
                    break;
                case BlockState.Move:
                    UpdateMove();
                    break;
                case BlockState.Land:
                    UpdateLand();
                    break;
            }
        }
        public void OnTouchBlock(bool isPressed)
        {
            //눌렀을 때
            if (isPressed)
            {
                //내 턴이 아닐경우 예외처리
                if (playerNum != TurnManager.Instance.currentPlayerNum)
                {
                    return;
                }
                blockState = BlockState.Move;
                transform.localScale = Vector3.one;
                //BlockCell들 활성화
                foreach (BlockCell bc in blockCells)
                {
                    bc.OnGrab(true);
                }
            }
            //땟을 때
            else
            {
                //착지할 상황이 되는지 확인
                for (int i = 0; i < blockCells.Length; i++)
                {
                    if (blockCells[i].isLandPossible == false)
                        break;

                    //전부 true일 경우
                    else if (i == blockCells.Length - 1)
                    {
                        //해당 묶음에 들어있는 블록들의 블록넘버들
                        int[] blockNums = new int[blockCells.Length];
                        //착륙시키기
                        for (int j = 0; j < blockCells.Length; j++)
                        {
                            blockNums[j] = int.Parse(blockCells[j].groundBlock.name);
                            blockCells[j].OnLand(blockIdx);
                        }
                        photonView.RPC("NoticeLandAndDestroySelf", RpcTarget.AllBuffered, blockNums, NetworkManager.Instance.myPlayerNum);
                        return;
                    }
                }
                //착지 못할 경우 -> 다시 원상복귀
                GoToIdle();
            }
        }
        
        [PunRPC]
        public void NoticeLandAndDestroySelf(int[] blockNums, int playerNum)
        {
            //사운드
            SFXManager.Instance.OnBlockLand();
            //착륙을 알림
            BoardManager.Instance.OnBlockLanded(blockNums, playerNum);
            BlockCreateManager.Instance.OnLandBlock(this.posIndex, playerNum);
            //자기자신 제거
            Destroy(gameObject);
        }
        public void GoToIdle()
        {
            blockState = BlockState.Idle;
            transform.localScale = Vector3.one * 0.5f;
            transform.position = originPos;
            //BlockCell들 비활성화
            foreach (BlockCell bc in blockCells)
            {
                bc.OnGrab(false);
            }

        }
        private void UpdateIdle()
        {

        }
        //만약 지금 놓으면 어디 놓일지 계산하고 있어야한다.
        private void UpdateMove()
        {
            if (photonView.IsMine)
            {
                //위치 이동
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                transform.position = new Vector3(ray.origin.x, 3, ray.origin.z) + mainCam.transform.up * finger2BlockDist;
            }
        }
        private void UpdateLand()
        {

        }
        //블록 자신 파괴시
        public void DestroySelf()
        {
            foreach (BlockCell bc in blockCells)
            {
                //쏘는  playerNum이니까 반대로
                BallCreateManager.Instance.ThrowBall((playerNum + 1) % 2, blockIdx, bc.transform.position);
            }
            Destroy(gameObject);
        }


    }
}