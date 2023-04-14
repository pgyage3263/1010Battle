using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NetworkScripts
{
    public class GroundBlock : MonoBehaviour
    {
        public Color originColor;
        Color activeColor;
        //public Color highlightColor;
        //public Material redMat;
        //public Material blueMat;
        //갖고옴
        Material myVisualMat;
        Material myMat;
        MeshRenderer myMesh;

        public bool isFilled = false;
        int myIndex;
        //실제 매시
        Mesh mesh;
        //기존 벌티스들
        Vector3[] baseVertices;

        // Start is called before the first frame update
        void Start()
        {
            //mesh vertice 초기화
            mesh = GetComponent<MeshFilter>().mesh;
            baseVertices = mesh.vertices;

            //activeColor
            activeColor = Color.black;
            myMat = GetComponent<MeshRenderer>().material;
            myMesh = GetComponent<MeshRenderer>();
            myIndex = int.Parse(gameObject.name);
            
            //캐싱에서 비쥬얼블록 가져오기
            myVisualMat = BoardManager.Instance.visualBlocks[myIndex].GetComponent<MeshRenderer>().material;
        }

        // Update is called once per frame
        //void Update()
        //{

        //}
        //위에 블록이 움직이고있을 때(착륙아님)
        public bool Activate(bool isActive)
        {
            if (isFilled == true)
            {
                return false;
            }
            if (isActive)
            {
                myVisualMat.color = activeColor;
            }
            //더이상 위에있지 않을 때
            else
            {
                myVisualMat.color = originColor;
            }
            return true;
        }
        //제거시
        public bool OnDisappear(int playerNum, bool isSpecial)
        {
            //이미 제거됐다면 예외처리
            if (isFilled == false)
                return false;
            //투사체 생성
            //대각선 완료했을 때 투사체
            if (isSpecial)
            {
                BallCreateManager.Instance.ThrowBall(playerNum, 12, transform.position);
            }
            else
                BallCreateManager.Instance.ThrowBall(playerNum, myColorNum, transform.position);
            //HP처리 -> 맞았을 때로 변경
            //if (TurnManager.Instance.currentPlayerNum == 0)
            //    HPManager.Instance.HP_2p--;
            //else
            //    HPManager.Instance.HP_1p--;
            //비쥬얼적으로 색깔변경하는 부분
            //MeshRenderer끄기.
            //myMesh.enabled = false;
            //색 원래대로 변경
            myVisualMat.color = originColor;
            //현재는 의미 없는 코드
            //GetComponent<MeshRenderer>().material = myMat;
            //Color curCol = GetComponent<MeshRenderer>().material.color;
            //StartCoroutine("ChangingColor", curCol);
            //크기 변경 시작
            StartCoroutine("DownScaling");

            isFilled = false;
            myColorNum = 0;
            return true;
        }
        IEnumerator DownScaling()
        {
            //블록 사라질 때 애니메이션 커브
            AnimationCurve ac = BoardManager.Instance.disappearCurve;

            float timeDelay = 0.5f;
            float currentTime = 0.0f;
            do
            {
                currentTime += Time.fixedDeltaTime;
                //변화
                Vector3[] vertices = new Vector3[baseVertices.Length];
                for(int i=0; i< vertices.Length; i++)
                {
                    Vector3 vertex = baseVertices[i];
                    
                    float percent = currentTime/timeDelay;
                    //Evaluate
                    float result = ac.Evaluate(percent);

                    vertex.x = vertex.x * result;
                    vertex.y = vertex.y * result;
                    vertex.z = vertex.z * result;
                    vertices[i] = vertex;
                }
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                yield return new WaitForFixedUpdate();
            } while (currentTime < timeDelay) ;

            //변화
            Vector3[] finalVertices = new Vector3[baseVertices.Length];
            for (int i = 0; i < finalVertices.Length; i++)
            {
                Vector3 vertex = baseVertices[i];
                vertex.x = 0;
                vertex.y = 0;
                vertex.z = 0;
                finalVertices[i] = vertex;
            }
            mesh.vertices = finalVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

        }
        IEnumerator ChangingColor(Color curCol)
        {
            float timeDelay = 1.0f;
            float currentTime = 0.0f;
            while (currentTime < timeDelay)
            {
                currentTime += Time.fixedDeltaTime;
                Color col = curCol + (originColor- curCol) * (currentTime);
                myVisualMat.color = col;
                yield return new WaitForFixedUpdate();
            }
            myVisualMat.color = originColor;

        }
        //색깔번호
        int myColorNum = 0;
        //해당 GroundBlock에 착지
        public void LandBlock(int blockIdx)
        {
            //BoardManager에 이름 넘기기
            BoardManager.Instance.OnFillGroundBlock(int.Parse(gameObject.name), blockIdx);

        }
        //서버에서 받았을 때(착지):: 과연 TurnManager로 하는게 안전할까 생각해봐야겠다.
        //색깔별로로 해결..
        public void CompleteLandBlock(int blockIdx)
        {
            //StopCoroutine("ChangingColor");
            //다운스케일
            StopCoroutine("DownScaling");
            mesh.vertices = baseVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            myColorNum = blockIdx;
            GetComponent<MeshRenderer>().material = BlockCreateManager.Instance.blockMaterials[blockIdx];

            //비쥬얼적으로 색깔변경하는 부분
            //MeshRenderer 켜기.
            myMesh.enabled = true;

            //if (TurnManager.Instance.currentPlayerNum == 0)
            //{
            //    myColorNum = 1;
            //    GetComponent<MeshRenderer>().material = redMat;
            //}
            //else
            //{
            //    myColorNum = 2;
            //    GetComponent<MeshRenderer>().material = blueMat;
            //}
            isFilled = true;

        }
    }
}