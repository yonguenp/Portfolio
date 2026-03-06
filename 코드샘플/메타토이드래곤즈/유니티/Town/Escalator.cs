using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class Escalator : MonoBehaviour
    {
        [SerializeField]
        private Transform start = null;
        [SerializeField]
        private Transform end = null;

        public Vector3 GetStartPos()
        {
            var pos = start.position;
            pos.z = 0;
            return pos;
        }
        public Vector3 GetEndPos()
        {
            var pos = end.position;
            pos.z = 0;
            return pos;
        }
    }
}