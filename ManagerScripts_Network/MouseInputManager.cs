using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NetworkScripts
{
    public class MouseInputManager : MonoBehaviour
    {

        Camera mainCam;
        public GameObject movingBlock;
        public static MouseInputManager Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            mainCam = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            //터치 시
            if (Input.GetMouseButtonDown(0))
            {
                GetBlock();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                LandBlock();
            }
            else
            {
                //잡은게 없다면
                if (!movingBlock)
                    return;
            }
        }
        public void GetBlock()
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (TurnManager.Instance.isMyturn == true && Physics.Raycast(ray, out hitInfo, 100.0f, LayerMask.GetMask("Block")))
            {
                movingBlock = hitInfo.transform.gameObject;
                movingBlock.GetComponent<BlockMove>().OnTouchBlock(true);
            }
        }
        public void LandBlock()
        {
            if (movingBlock)
            {
                movingBlock.GetComponent<BlockMove>().OnTouchBlock(false);
                movingBlock = null;
            }
        }
        public void ExitBlock()
        {
            if (movingBlock)
            {
                movingBlock.GetComponent<BlockMove>().GoToIdle();
                movingBlock = null;
            }
        }
    }
}