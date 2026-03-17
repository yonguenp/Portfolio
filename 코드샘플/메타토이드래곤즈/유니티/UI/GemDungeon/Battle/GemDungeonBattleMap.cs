using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class GemDungeonBattleMap : BattleMap
    {

        private readonly float cellValue = 3.3f;
        protected override void OnEnable()
        {
            EventManager.AddListener(this);
            BoxCollider2D[] col = ColliderGroup.GetComponentsInChildren<BoxCollider2D>();
            if (col.Length >= 2)
            {
                StageMinY = col[0].bounds.max.y;
                StageMaxY = col[1].bounds.min.y;
            }
        }

        private void Start()
        {
            ResizeAndPosCollider();
        }

        void ResizeAndPosCollider()
        {
            BoxCollider2D[] col = ColliderGroup.GetComponentsInChildren<BoxCollider2D>();
            
            if(col.Length >= 4)
            {
                
                var scaleValue = TownMap.Width * cellValue;
                col[0].GetComponent<BoxCollider2D>().size = new Vector2(scaleValue, col[0].transform.localScale.y);
                col[1].GetComponent<BoxCollider2D>().size = new Vector2(scaleValue, col[1].transform.localScale.y);

                var height = col[0].transform.localPosition.y - col[1].transform.localPosition.y;

                col[2].transform.localPosition = new Vector2(-scaleValue/2f, col[2].transform.localPosition.y);
                col[3].transform.localPosition = new Vector2(scaleValue/2f, col[3].transform.localPosition.y);

                col[2].GetComponent<BoxCollider2D>().size = col[3].GetComponent<BoxCollider2D>().size = new Vector2(1, height);
                
            }
            
        }

        protected override void LateUpdate()
        {
            //base.LateUpdate();
        }

        protected override void SetScaleByY(Transform child)
        {
            
        }
    }
}


