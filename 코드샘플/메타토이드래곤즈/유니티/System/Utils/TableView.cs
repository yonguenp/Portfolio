using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SandboxNetwork
{
    public class TableViewDelegate
    {
        public delegate void ReuseDelegate(GameObject itemNode, ITableData item);
        public delegate void UnuseDelegate(GameObject itemNode);

        public List<ITableData> Items = null;
        public ReuseDelegate Reuse = null;
        public UnuseDelegate Unuse = null;

        public TableViewDelegate(List<ITableData> items, ReuseDelegate reuse = null, UnuseDelegate unuse = null)
        {
            Items = items;
            Reuse = reuse;
            Unuse = unuse;
        }

        public void SetDelegate(List<ITableData> items, ReuseDelegate reuse = null, UnuseDelegate unuse = null)
        {
            Items = items;
            Reuse = reuse;
            Unuse = unuse;
        }
    }

    public class TableView : MonoBehaviour
    {
        [Header("Default")]
        [SerializeField]
        protected eTableViewType tableType  = eTableViewType.Vertical;
        [SerializeField]
        protected GameObject itemTemplate = null;
        public GameObject ItemTemplate { get { return itemTemplate; } }
        [Space(10f)]
        [Header("Padding")]
        [SerializeField]
        protected float paddingX = 0f;
        [SerializeField]
        protected float paddingY = 0f;
        [Space(10f)]
        [Header("Spaceing")]
        [SerializeField]
        protected float spaceingX = 0f;
        [SerializeField]
        protected float spaceingY = 0f;
        protected ScrollRect scrollView = null;
        protected float itemWidth = 0f;
        protected float itemHeight = 0f;
        protected SBListPool<GameObject> itemPool = null;
        protected TableViewDelegate viewDelegate = null;
        protected List<ITableData> dataSource = null;
        protected float visibleWidth = 0f;
        protected float visibleHeight = 0f;
        protected int spawnCount = 0;
        protected Dictionary<int, GameObject> visibleNodes = null;
        protected float lastX = float.MinValue;
        protected float lastY = float.MinValue;
        protected HashSet<int> itemIndex = null;
        protected Vector3 vec3 = Vector3.zero;
        protected RectTransform contentTransform = null;
        protected float contentScaleX = 1f;
        protected float contentScaleY = 1f;
        protected Tweener moveTween = null;
        /// <summary> 연출 시간바꾸려면 상속받아서 바꾸기 or 상의하고 합의하에 바꾸기 </summary>
        protected virtual float MoveTweenDelay { get => 0.3f; }
        protected virtual void OnDestroy()
        {
            if (itemPool != null)
            {
                while (itemPool.Get() != null)
                {
                    continue;
                }
            }
            if (moveTween != null)
            {
                moveTween.Kill();
                moveTween = null;
            }
        }
        public virtual void OnStart()
        {
            if(itemPool == null)
            {
                itemPool = new SBListPool<GameObject>((GameObject item) =>
                {
                    if (item == null)
                        return;
                    if (item.transform.parent != contentTransform)
                    {
                        item.transform.SetParent(contentTransform, false);
                        item.transform.localScale = Vector3.one;
                    }
                    item.SetActive(true);
                }, (GameObject item) =>
                {
                    if (item == null)
                        return;

                    item.SetActive(false);
                    if (item.transform.parent != contentTransform)
                    {
                        item.transform.SetParent(contentTransform, false);
                        item.transform.localScale = Vector3.one;
                    }
                    //item.transform.SetParent(null, false);
                });
            }

            if (dataSource == null)
                dataSource = new List<ITableData>();
            if (visibleNodes == null)
                visibleNodes = new Dictionary<int, GameObject>();
            if (itemIndex == null)
                itemIndex = new HashSet<int>();

            scrollView = GetComponent<ScrollRect>();
            if (scrollView != null)
            {
                var scrollViewRect = scrollView.viewport.GetComponent<RectTransform>();
                if (scrollViewRect != null)
                {
                    visibleWidth = scrollViewRect.rect.width;
                    visibleHeight = scrollViewRect.rect.height;
                }

                contentScaleX = scrollView.content.localScale.x;
                contentScaleY = scrollView.content.localScale.y;

                contentTransform = scrollView.content;
            }

            if (itemTemplate != null)
            {
                var itemSize = itemTemplate.GetComponent<RectTransform>();
                if (itemSize != null)
                {
                    itemWidth = itemSize.rect.width;
                    itemHeight = itemSize.rect.height;
                }
            }
            itemPool.Put(itemTemplate);
            lastX = float.MinValue;
            lastY = float.MinValue;

            switch (tableType)
            {
                case eTableViewType.Vertical:
                {
                    scrollView.vertical = true;
                    scrollView.horizontal = false;
                    contentTransform.anchorMin = new Vector2(0.5f, 1f);
                    contentTransform.anchorMax = new Vector2(0.5f, 1f);
                    contentTransform.pivot = new Vector2(0.5f, 1f);
                    spawnCount = Mathf.RoundToInt(visibleHeight / itemHeight / contentScaleY) + 2;
                } break;
                case eTableViewType.Horizental:
                {
                    scrollView.vertical = false;
                    scrollView.horizontal = true;
                    contentTransform.anchorMin = new Vector2(0f, 0.5f);
                    contentTransform.anchorMax = new Vector2(0f, 0.5f);
                    contentTransform.pivot = new Vector2(0f, 0.5f);
                    spawnCount = Mathf.RoundToInt(visibleWidth / itemWidth / contentScaleX) + 2;
                } break;
            }

            PoolSpawn(spawnCount);
        }

        protected virtual void LateUpdate()
        {
            if(contentTransform != null) { 
                switch (tableType)
                {
                    case eTableViewType.Vertical:
                    {
                        var y = contentTransform.anchoredPosition.y / contentScaleY;
                        if (lastY != y)
                        {
                            lastY = y;
                            GetVisibleItemIndexVertical(y);

                            var keys = new List<int>(visibleNodes.Keys);
                            var count = keys.Count;
                            for (var i = 0; i < count; ++i)
                            {
                                var idx = keys[i];
                                if (!itemIndex.Contains(idx))
                                {
                                    viewDelegate.Unuse?.Invoke(visibleNodes[idx]);
                                    itemPool.Put(visibleNodes[idx]);
                                    visibleNodes.Remove(idx);
                                }
                            }

                            var it = itemIndex.GetEnumerator();
                            while (it.MoveNext())
                            {
                                IndexChange(it.Current);
                            }
                        }
                    }
                    break;
                    case eTableViewType.Horizental:
                    {
                        var x = contentTransform.anchoredPosition.x / contentScaleX;
                        if (lastX != x)
                        {
                            lastX = x;
                            GetVisibleItemIndexHorizental(x);

                            var keys = new List<int>(visibleNodes.Keys);
                            var count = keys.Count;
                            for (var i = 0; i < count; ++i)
                            {
                                var idx = keys[i];
                                if (!itemIndex.Contains(idx))
                                {
                                    viewDelegate.Unuse?.Invoke(visibleNodes[idx]);
                                    itemPool.Put(visibleNodes[idx]);
                                    visibleNodes.Remove(idx);
                                }
                            }

                            var it = itemIndex.GetEnumerator();
                            while (it.MoveNext())
                            {
                                IndexChange(it.Current);
                            }
                        }
                    }
                    break;
                }
            }
        }
        protected virtual void PoolSpawn(int count)
        {
            while (itemPool.Count < count)
            {
                itemPool.Put(Instantiate(itemTemplate));
            }
        }
        public virtual void SetDelegate(TableViewDelegate viewDelegate)
        {
            this.viewDelegate = viewDelegate;
        }
        public virtual void ReLoad(bool initPos = true)
        {
            dataSource = viewDelegate.Items;
            var size = contentTransform.sizeDelta;
            size.x = GetTotalWidth();
            size.y = GetTotalHeight();
            contentTransform.sizeDelta = size;
            switch (tableType)
            {
                case eTableViewType.Vertical:
                {
                    var children = SBFunc.GetChildren(scrollView.content.gameObject);
                    var cCount = children.Length;
                    for(var i = 0; i < cCount; ++i)
                    {
                        itemPool.Put(children[i]);
                    }

                    //scrollView.content.DetachChildren();
                    visibleNodes.Clear();

                    scrollView.StopMovement();

                    if (initPos)
                    {
                        scrollView.normalizedPosition = new Vector2(0.5f, 1f);
                    }

                    lastY = int.MinValue;
                } break;
                case eTableViewType.Horizental:
                {
                    var children = SBFunc.GetChildren(scrollView.content.gameObject);
                    var cCount = children.Length;
                    for (var i = 0; i < cCount; ++i)
                    {
                        itemPool.Put(children[i]);
                    }

                    //scrollView.content.DetachChildren();
                    visibleNodes.Clear();

                    scrollView.StopMovement();

                    if (initPos)
                    {
                        scrollView.normalizedPosition = new Vector2(0f, 0.5f);
                    }

                    lastX = int.MinValue;
                } break;
            }
        }

        protected virtual void GetVisibleItemIndexVertical(float y)
        {
            var itemSpancing = itemHeight + spaceingY;
            var minY = Mathf.Max(0, Mathf.FloorToInt(y / itemSpancing));
            var maxY = minY == 0 ? spawnCount : Mathf.CeilToInt((y + visibleHeight / contentScaleY) / itemSpancing);

            var totalCount = dataSource.Count;
            if ((minY + spawnCount) > totalCount)
            {
                minY = Mathf.Max(0, totalCount - spawnCount);
            }
            maxY = Mathf.Min(maxY, totalCount);

            itemIndex.Clear();
            for (var i = minY; i < maxY; ++i)
            {
                itemIndex.Add(i);
            }
        }

        protected virtual void GetVisibleItemIndexHorizental(float x)
        {
            var itemSpancing = itemWidth + spaceingX;
            var minX = Mathf.Max(0, Mathf.FloorToInt(-x / itemSpancing));
            var maxX = minX == 0 ? spawnCount : Mathf.CeilToInt((visibleWidth / contentScaleX - x) / itemSpancing);

            var totalCount = dataSource.Count;
            if ((minX + spawnCount) > totalCount)
            {
                minX = Mathf.Max(0, totalCount - spawnCount);
            }
            maxX = Mathf.Min(maxX, totalCount);

            itemIndex.Clear();
            for (var i = minX; i < maxX; i++)
            {
                itemIndex.Add(i);
            }
        }

        protected virtual void IndexChange(int idx)
        {
            if (!visibleNodes.ContainsKey(idx))
            {
                if (dataSource.Count <= idx)
                    return;

                vec3 = GetIndexPosition(idx);
                switch (tableType)
                {
                    case eTableViewType.Vertical:
                    {
                        PoolSpawn(1);

                        var node = itemPool.Get();
                        vec3.z = node.transform.localPosition.z;
                        node.transform.localPosition = vec3;

                        viewDelegate.Reuse?.Invoke(node, dataSource[idx]);
                        visibleNodes.Add(idx, node);
                        node.transform.SetAsLastSibling();
                    }
                    break;
                    case eTableViewType.Horizental:
                    {
                        PoolSpawn(1);

                        var node = itemPool.Get();
                        vec3.z = node.transform.localPosition.z;
                        node.transform.localPosition = vec3;

                        viewDelegate.Reuse?.Invoke(node, dataSource[idx]);
                        visibleNodes.Add(idx, node);
                        node.transform.SetAsLastSibling();
                    }
                    break;
                }
            }
        }
        protected virtual float GetTotalWidth()
        {
            if (tableType != eTableViewType.Horizental)
            {
                return 0f;
            }
            return paddingX * 2 + itemWidth * dataSource.Count + (spaceingX * Mathf.Max(0, dataSource.Count - 1));
        }
        protected virtual float GetTotalHeight()
        {
            if (tableType != eTableViewType.Vertical)
            {
                return 0f;
            }
            return paddingY * 2 + itemHeight * dataSource.Count + (spaceingY * Mathf.Max(0, dataSource.Count - 1));
        }

        public void SetPaddingMulti(float value)
        {
            paddingX *= value;
            paddingY *= value;
        }
        public void SetSpaceingMulti(float value)
        {
            spaceingX *= value;
            spaceingY *= value;
        }

        public void ScrollMoveTweenIndex(int index, eTableViewAnchor anchor = eTableViewAnchor.MIDDLE)
        {
            if (index < 0 || index >= dataSource.Count)
                return;

            if (dataSource[index] == null)
                return;

            ScrollMoveTweenItem(dataSource[index], anchor);
        }
        public virtual void ScrollMoveTweenItem(ITableData data, eTableViewAnchor anchor = eTableViewAnchor.MIDDLE, VoidDelegate _tweenCompleteCallback = null)
        {
            if (scrollView != null)
            {
                if (moveTween != null)
                {
                    moveTween.Kill();
                    moveTween = null;
                }

                var index = GetDataIndex(data);
                if (index < 0)
                    return;

                var pos = GetIndexPosition(index);
                switch (tableType)
                {
                    case eTableViewType.Vertical:
                    {
                        var totalH = GetTotalHeight();
                        var hPos = -pos.y * contentScaleY;
                        switch (anchor)
                        {
                            case eTableViewAnchor.FIRST:
                            {
                                hPos += visibleHeight * 0.5f - (itemHeight * 0.5f + paddingY) * contentScaleY;
                            } break;
                            case eTableViewAnchor.LAST:
                            {
                                hPos -= visibleHeight * 0.5f - (itemHeight * 0.5f + paddingY) * contentScaleY;
                            } break;
                            default:
                                break;
                        }

                        //최대 최소 값 보정.
                        if (hPos > totalH)
                            hPos = totalH;
                        if (hPos < 0)
                            hPos = 0;

                        moveTween = scrollView.content.DOLocalMoveY(hPos, MoveTweenDelay).OnComplete(()=> {
                            if (_tweenCompleteCallback != null)
                                _tweenCompleteCallback();
                        });
                    }
                    break;
                    case eTableViewType.Horizental:
                    {
                        var totalW = GetTotalWidth();
                        var wPos = -pos.x * contentScaleX;

                        switch (anchor)
                        {
                            case eTableViewAnchor.FIRST:
                            {
                                wPos -= visibleWidth * 0.5f - (itemWidth * 0.5f + paddingX) * contentScaleX;
                            } break;
                            case eTableViewAnchor.LAST:
                            {
                                wPos += visibleWidth * 0.5f - (itemWidth * 0.5f + paddingX) * contentScaleX;
                            } break;
                            default:
                                break;
                        }

                        //최대 최소 값 보정.
                        if (wPos < -totalW)
                            wPos = -totalW;
                        if (wPos > 0)
                            wPos = 0;

                        moveTween = scrollView.content.DOLocalMoveX(wPos, MoveTweenDelay).OnComplete(() => {
                            if (_tweenCompleteCallback != null)
                                _tweenCompleteCallback();
                        });
                    }
                    break;
                }
            }
        }
        public int GetDataIndex(ITableData data)
        {
            if (dataSource == null)
                return -1;

            return dataSource.FindIndex((e) => { return e == data; });
        }
        protected virtual Vector3 GetIndexPosition(int idx)
        {
            return tableType switch
            {
                eTableViewType.Vertical => new Vector3(paddingX, -(paddingY + (idx + 0.5f) * itemHeight + spaceingY * idx)),
                eTableViewType.Horizental => new Vector3(paddingX + (idx + 0.5f) * itemWidth + spaceingX * idx, paddingY),
                _ => Vector3.zero
            };
        }

        public bool IsVisibleNode(int _index)
        {
            return visibleNodes.ContainsKey(_index);
        }

        public Dictionary<int, GameObject> GetVisibleNodes()
        {
            return visibleNodes;
        }
    }
}
