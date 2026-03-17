using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public enum eObjectAnchor
    {
        NONE,
        LEFT_TOP,
        LEFT_CENTER,
        LEFT_BOTTOM,
        RIGHT_TOP,
        RIGHT_CENTER,
        RIGHT_BOTTOM,
        CENTER_TOP,
        CENTER_BOTTOM,
    }

    [Serializable]
    public class ObjectSeqData
    {
        public int seq = 0;
        public int descStringIndex = 0;
        public eObjectAnchor anchor = eObjectAnchor.NONE;

        public GameObject target = null;
    }

    //데이터 테이블 가공하는 컴포넌트
    public class ObjectSeqController : MonoBehaviour
    {
        [SerializeField] List<ObjectSeqData> testList = new List<ObjectSeqData>();//임시



        public void InitController()//디자인 데이터를 ObjectSeqData 가공
        {
            if (ObjectIndicatorManager.Instance == null)
                return;

            foreach(var seqData in testList)
            {
                if (seqData == null)
                    continue;

                ObjectIndicatorManager.Instance.SetSeqDataDic(1, seqData);
            }
        }
    }
}
