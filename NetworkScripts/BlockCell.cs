using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NetworkScripts
{
    public class BlockCell : MonoBehaviour
    {
        //public Color originColor;
        //public Color trackingColor;
        //public Color BlockCol_1P;
        //public Color BlockCol_2P;

        public enum State
        {
            Idle,
            Grabbed
        };
        State blockcellState;
        public GameObject groundBlock;
        GroundBlock gbScript;
        public bool isLandPossible = false;
        Material myMat;
        //잡았을 경우 scale
        float graabedScale = 0.9f;
        // Start is called before the first frame update
        void Start()
        {
            //디졸브 처리
            myMat = GetComponent<MeshRenderer>().material;
            myMat.SetFloat("_SliceAmount", 1.0f);
            blockcellState = State.Idle;
        }
        float currentTime = 0.0f;
        bool isDissolve = false;
        // Update is called once per frame
        void Update()
        {
            //초기 디졸브 처리
            if (isDissolve)
            {
                if (currentTime > 1.5f)
                {
                    myMat.SetFloat("_SliceAmount", 0.0f);
                    isDissolve = false;
                }
                currentTime += Time.deltaTime;
                myMat.SetFloat("_SliceAmount", 1.5f - currentTime);
            }
            switch (blockcellState)
            {
                case State.Idle:
                    break;
                case State.Grabbed:
                    UpdateGrabbed();
                    break;
            }
        }
        //Material을 받아옴(초기 설정시에)
        public void GetMaterial(Material mat)
        {
            GetComponent<MeshRenderer>().material = mat;
            isDissolve = true;
        }
        void ChangeGroundBlock(GameObject go)
        {
            if (go == null)
            {
                groundBlock = null;
                gbScript = null;
            }
            else
            {
                groundBlock = go;
                gbScript = go.GetComponent<GroundBlock>();
            }
        }
        private void UpdateGrabbed()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 100.0f, LayerMask.GetMask("BoardBlock")))
            {
                //기존과 다를 경우
                if (groundBlock != hitInfo.transform.gameObject)
                {
                    if (groundBlock == null)
                    {
                        ChangeGroundBlock(hitInfo.transform.gameObject);
                        //빈칸일 때
                        if (gbScript.isFilled == false)
                        {
                            gbScript.Activate(true);
                            isLandPossible = true;
                        }
                        else
                        {
                            ChangeGroundBlock(null);
                            isLandPossible = false;
                        }
                    }
                    //groundBlock 교체 시
                    else
                    {
                        //해제
                        gbScript.Activate(false);
                        //변경
                        ChangeGroundBlock(hitInfo.transform.gameObject);
                        //빈칸일 때
                        if (gbScript.isFilled == false)
                        {
                            gbScript.Activate(true);
                            isLandPossible = true;
                        }
                        else
                        {
                            ChangeGroundBlock(null);
                            isLandPossible = false;
                        }
                    }
                }
                //기존이랑 같을 경우
                else
                {
                    gbScript.Activate(true);
                }
            }
            //RayCast 닿는 곳이 없을 때
            else
            {
                //이전의 groundBlock이 있다면
                if (groundBlock)
                {
                    //해제
                    gbScript.Activate(false);
                    //
                    ChangeGroundBlock(null);
                    isLandPossible = false;
                }
            }
        }
        //
        public void OnGrab(bool isGrabbed)
        {
            if (isGrabbed)
            {
                //스케일 변경
                transform.localScale = new Vector3(graabedScale, graabedScale, graabedScale);
                blockcellState = State.Grabbed;
            }
            else
            {
                //스케일 변경
                transform.localScale = Vector3.one;
                blockcellState = State.Idle;
                //밑에 Block이 있을 경우
                if (groundBlock)
                {
                    gbScript.Activate(false);
                    ChangeGroundBlock(null);
                }
            }
        }
        public void OnLand(int blockIdx)
        {
            gbScript.LandBlock(blockIdx);
        }
    }
}