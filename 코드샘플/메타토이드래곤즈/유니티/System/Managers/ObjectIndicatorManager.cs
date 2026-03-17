using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// to do object를 정의하는 group 및 seq 가 필요함. (내부에는 오브젝트를 가리키는 타겟 경로도 필요할 것 같은데 논의필요)

namespace SandboxNetwork
{
    public class ObjectIndicatorManager : MonoBehaviour
    {
        static ObjectIndicatorManager instance = null;
        public static ObjectIndicatorManager Instance
        {
            get
            {
                if (instance == null)
                    return null;

                return instance;
            }
        }


        [SerializeField] GameObject cloneTargetParent = null;//오브젝트를 복사해서 놓을 곳

        [SerializeField] GameObject targetCanvas = null;//캔버스 관리 위치
        [SerializeField] GameObject instanceParent = null;//프리펩 생성 부모
        [SerializeField] GameObject indicatorPrefab = null;//생성 프리펩

        [SerializeField] ObjectSeqController controller = null;

        public bool IsPlaying { private set; get; }
        
        // 완료한 목록 - 순차로 준다면 인트값 (group index 하나만 들고있어도 될듯)
        static List<int> clearList = new List<int>();
        
        // 시퀀스 데이터
        Dictionary<int, List<ObjectSeqData>> seqDataDic = new Dictionary<int, List<ObjectSeqData>>();//디자인 데이터 세팅 key : group , value : seq 구성 데이터

        List<ObjectSeqData> currentSeqList = new List<ObjectSeqData>();

        int currentGroupIndex = 0;
        int currentSeq = 0;
        int currentMaxSeq = 0;

        Coroutine seqCO = null;

        bool isFirstInit = false;
        bool isClicked = false;
        private void Awake()
        {
            instance = this;
        }
        public void Init()
        {
            if(!isFirstInit && controller != null)
            {
                controller.InitController();

                isFirstInit = true;
            }
        }

        public void StartSeq(int _groupIndex, int _seq = 1, bool isForce = false)//무조건 처음부터 실행하자
        {
            if (seqDataDic == null || seqDataDic.Count <= 0)
                return;

            if (IsPlaying)
                return;

            currentSeqList.Clear();

            SetActiveCanvas(false);

            IsPlaying = true;
            currentGroupIndex = _groupIndex;
            currentSeq = _seq;

            //데이터 테이블을 통해서 현재 group 값을 가지고 seq 데이터를 가져올 곳이 필요함.
            //테이블 안쓸거면 유니티 사이즈로 가져오는 방법도.

            if(seqDataDic.ContainsKey(_groupIndex))
                currentSeqList = seqDataDic[_groupIndex].ToList();

            if (currentSeqList == null || currentSeqList.Count <= 0)
            {
                IsPlaying = false;
                return;
            }

            currentMaxSeq = currentSeqList.Count;

            if (seqCO != null)
            {
                StopCoroutine(seqCO);
                seqCO = null;
            }
            seqCO = StartCoroutine(PlaySeq());
        }
        IEnumerator PlaySeq()
        {
            if (currentSeqList.Count <= 0) yield break;

            SetActiveCanvas(true);

            isClicked = false;

            bool instancingFlag = false;
            var tempGroupIndex = currentSeq;

            while (currentSeq <= currentMaxSeq)
            {
                //seq 기반 objectIndicator 생성 - 생성할때마다 다른 오브젝트를 지울지 말지
                var curSeqData = GetSeqDataDic(currentGroupIndex, currentSeq);
                if (curSeqData != null && instanceParent != null)
                {
                    if(!instancingFlag)
                    {
                        var copyTarget = Instantiate(curSeqData.target, cloneTargetParent.transform);
                        copyTarget.transform.position = curSeqData.target.transform.position;

                        copyTarget.GetComponent<RectTransform>().localScale = curSeqData.target.GetComponent<RectTransform>().localScale;
                        if (curSeqData.target.transform.parent.localScale != Vector3.one && curSeqData.target.GetComponent<RectTransform>().localScale == Vector3.one)
                            copyTarget.GetComponent<RectTransform>().localScale = curSeqData.target.transform.parent.localScale;
                        if (copyTarget.GetComponent<Animator>() != null)
                            copyTarget.GetComponent<Animator>().enabled = false;

                        var obj = Instantiate(indicatorPrefab, instanceParent.transform);
                        var objTargetComp = obj.GetComponent<ObjectTargetIndicator>();
                        if (objTargetComp == null)
                            Destroy(obj);

                        objTargetComp.SetData(copyTarget, curSeqData.anchor, StringData.GetStringByIndex(curSeqData.descStringIndex));

                        instancingFlag = true;
                    }

                    if(tempGroupIndex != currentSeq)
                    {
                        instancingFlag = false;
                        tempGroupIndex = currentSeq;
                    }

                    yield return new WaitUntil(() => isClicked);
                }
                else
                {
                    currentSeq++;
                    isClicked = true;
                }
            }

            SetActiveCanvas(false);

            SBFunc.RemoveAllChildrens(instanceParent.transform);
            SBFunc.RemoveAllChildrens(cloneTargetParent.transform);

            IsPlaying = false;
        }
        public void SetSeqDataDic(int group, ObjectSeqData seqData)
        {
            if (seqDataDic == null) return;

            if (!seqDataDic.ContainsKey(group))
            {
                List<ObjectSeqData> newList = new();
                newList.Add(seqData);

                seqDataDic.Add(group, newList);
            }
            else
            {
                var currentList = seqDataDic[group];
                if(currentList == null)
                    seqDataDic[group] = new();

                if (!currentList.Contains(seqData))
                    seqDataDic[group].Add(seqData);
            }
        }

        public ObjectSeqData GetSeqDataDic(int group, int seq)
        {
            ObjectSeqData result = null;
            if (seqDataDic.ContainsKey(group))
            {
                var list = seqDataDic[group];
                if (list == null || list.Count <= 0 || seq > list.Count)
                    return null;

                var data = list[seq - 1];
                if (data == null)
                    return null;

                if (data.seq == seq)
                    return data;
                else
                    return null;
            }

            return result;
        }
        void SetActiveCanvas(bool _isActive)
        {
            if (targetCanvas != null)
                targetCanvas.SetActive(_isActive);
        }

        public void OnClick()
        {
            currentSeq++;
            isClicked = true;
        }

//#if UNITY_EDITOR
//        public void Update()
//        {

//            TutorialTestCheat();

//        }
//#endif


//#if UNITY_EDITOR
//        void TutorialTestCheat()
//        {
//            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Town")
//            {
//                if (Input.GetKeyDown(KeyCode.Alpha1))
//                {
//                    seqDataDic.Clear();
//                    controller.InitController();
//                    StartSeq(1, 1, true);
//                }
//            }
//        }
//#endif
    }

}
