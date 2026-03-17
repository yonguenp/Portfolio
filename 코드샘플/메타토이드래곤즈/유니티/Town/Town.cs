using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using EasyMobile;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public struct BuildCompleteEvent
    {
        
        private static BuildCompleteEvent obj;
        public Building building;
        public eBuildingState eType;

        public static void Send(Building building, eBuildingState eventType)
        {
            obj.building = building;
            obj.eType = eventType;
            EventManager.TriggerEvent(obj);
        }
    }

    public class Town : MonoBehaviour
    {
        //2개의 타운을 방지함
        private static Town instance = null;
        public static Town Instance { get { return instance; } }

        //타운의 오브젝트 및 드래곤 등이 속할 Parent
        [SerializeField]
        private GameObject townBase = null;
        [SerializeField]
        private Transform bgSky = null;

        [SerializeField]
        private SpriteRenderer bgDay = null;
        [SerializeField]
        private SpriteRenderer bgNight = null;

        [SerializeField]
        private Transform bgSun = null;
        [SerializeField]
        private Transform bgMoon = null;
        [SerializeField]
        private Transform bgOrbit =  null;
        [SerializeField]
        private Transform bgStar = null;

        [SerializeField]
        private GameObject farGround_Day;
        [SerializeField]
        private GameObject farGround_Dusk;
        [SerializeField]
        private GameObject farGround_Night;
        [SerializeField]
        private GameObject clouds;
        [SerializeField] private Transform firstClouds;
        [SerializeField] private Transform secondClouds;
        [SerializeField] private Transform thirdClouds;
        private List<SpriteRenderer> cloudsRenderers = new List<SpriteRenderer>();

        [SerializeField]
        Color dayCloudColor = Color.white;
        [SerializeField]
        Color duskCloudColor = new Color(1.0f, 0.94f, 0.89f);
        [SerializeField]
        Color nightCloudColor = new Color(0.72f, 0.81f, 0.96f);

        [SerializeField]
        private Camera subCam =  null;
        [SerializeField]
        BGMPlayer bgmPlayer;
        private List<SpriteRenderer> fargrounds_day = new List<SpriteRenderer>();
        private List<SpriteRenderer> fargrounds_dusk = new List<SpriteRenderer>();
        private List<SpriteRenderer> fargrounds_night = new List<SpriteRenderer>();

        private static GameObject DragonsParent = null;
        
        public static Dictionary<int, TownDragonSpine> TownDragonsDic = new Dictionary<int, TownDragonSpine>();
        public Dictionary<int, TownDragonSpine> TownDragons = new Dictionary<int, TownDragonSpine>();
        // 타운 Input 동작 관련
        [SerializeField]
        private TownController townController = null;

        //현재 보이는 타운의 크기 => 변동값 측정하고 바꾸기
        private Vector2Int drawStart = Vector2Int.zero;
        private Vector2Int drawCount = Vector2Int.zero;
        private int openHeight = -1;

        //BG 관리
        private Dictionary<int, Dictionary<int, GameObject>> cellDics = new Dictionary<int, Dictionary<int, GameObject>>();
        private Dictionary<int, Dictionary<int, GameObject>> wallDics = new Dictionary<int, Dictionary<int, GameObject>>();
        private Dictionary<int, Dictionary<bool, GameObject>> elevatorDics = new Dictionary<int, Dictionary<bool, GameObject>>();
        private Dictionary<int, Dictionary<int, GameObject>> gemDungeonCellDics = new Dictionary<int, Dictionary<int, GameObject>>();
        private Dictionary<int, GameObject> guildCellDics  = new Dictionary<int, GameObject>();
        private Dictionary<int, Building> buildingDics = new Dictionary<int, Building>();
        private Dictionary<int, Locked> lockedDics = new Dictionary<int, Locked>();
        private Dictionary<int, GameObject> decoDics = new Dictionary<int, GameObject>();

        //Deco
        public GameObject head { get; private set; } = null;        
        private GameObject decoL = null;
        private GameObject decoR = null;

        //Guild Flag
        public GameObject guildTopLFlag { get; private set; } = null;
        public GameObject guildTopRFlag { get; private set; } = null;

        //Subway
        public TrainMove train { get; private set; } = null;

        public DozerBuilding dozer { get; private set; } = null;
        public Travel travel { get; private set; } = null;

        public ExchangeBuilding exchangeCenter { get; private set; } = null;
        /// <summary> key => Floor, value => Building </summary>
        public Dictionary<int, GemdungeonBuilding> Gemdungeon { get; private set; } = null;

        public GuildBuilding guildBuilding { get; private set; } = null;

        public int ConstructTag { get; set; } = 0;

        public bool IsCamZooming { get { return cam ? !cam.enabled : false; } }

        //Object 관리
        private Escalator escalatorL = null;
        private Escalator escalatorR = null;
        private Elevator elevatorL = null;
        private Elevator elevatorR = null;

        private readonly float cloudDist = 12.5f;
        private readonly float cloudbaseHeight = 1.51f;
        public Transform TownBaseTransoform { get { return townBase.transform; } }

        public int onDragonCount { get; private set; }
        private ProCamera2D cam= null;
        private Transform camZoomTr = null;
        private Vector3 lastestCamPos; // 줌 하기 전 위치
        private float lastestCamZoom;

        private List<SpriteRenderer> lightingObjects = new List<SpriteRenderer>();
        int curFollowTag = -1;
        public bool IsCamDragonFollow { get { return curFollowTag > 0; } }
        public int CurFollowDragonTag {
            get { return curFollowTag; }
            set {
                curFollowTag = value;
                foreach (var kv in TownDragons)
                {
                    if (kv.Value == null)
                        continue;
                    
                    kv.Value.SetOutline(kv.Key == curFollowTag);
                }
            } 
        }

        public bool IsIntroMode { get; private set; } = false;


        public Texture townViewTexture { get {
                RefreshSubCamProjection();
                return subCam.targetTexture; 
            } }


        Sequence daylightSequence = null;
        Sequence cloudSequences = null;
        //2개의 타운 방지용도
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                UIManager.Instance.InitUI(eUIType.Town);
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
                if (Advertising.IsInitialized())
                    AdvertiseManager.Instance.IsAdvertiseReady();  // 광고 매니저 enable 타이밍때 광고가 로드 되지 않아서 여기로 다시 옮김 - DJ
#endif

                UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
                Init();
            }
            else
            {
                Destroy(gameObject);
            }
		}

        private void OnEnable()
        {
            if (DragonsParent != null)
            {
                DragonsParent.SetActive(true);
                foreach (var dragon in TownDragons.Values)
                {
                    dragon.transform.SetParent(DragonsParent.transform);
                    dragon.SetActive(false);
                }
                TownDragons.Clear();
            }
        }

        private void OnDisable()
        {
            if (DragonsParent != null)
            {
                DragonsParent.SetActive(false);
                foreach (var dragon in TownDragons.Values)
                {
                    dragon.transform.SetParent(DragonsParent.transform);
                    dragon.SetActive(false);
                }

                TownDragons.Clear();
            }
        }
        private void OnDestroy()
        {
            if (daylightSequence != null)
                daylightSequence.Kill();

            if (this == instance)
            {
                instance = null;
            }

			if(cloudSequences != null)
            {
                cloudSequences.Kill();
            }
                
        }
            //

            //시작값 초기화 및 맵 크기 세팅.
        private void Init()
        {
            TownMap.SetMap(User.Instance.GetMapData());
            TownMap.SetOpenMap(User.Instance.GetOpenFloorData());
            cam = Camera.main.GetComponent<ProCamera2D>();
            drawStart.x = TownMap.X;
            drawStart.y = TownMap.Y;
            drawCount.x = TownMap.Width;
            drawCount.y = TownMap.Height - 1;
            openHeight = TownMap.OpenHeight;

            cellDics.Clear();
            wallDics.Clear();
            elevatorDics.Clear();
            lockedDics.Clear();
            buildingDics.Clear();
            lightingObjects.Clear();

            CheckCell();
            CheckBuilding();
            CheckElevator();
            CheckDeco();
            CheckDragon();
            CheckCamera();
            CheckBG();

            SetSubCamState(false);

            NotificationManager.Instance.RefreshNotifications();
        }

        public void OnBGMPlay()
        {
            bgmPlayer.Play();
        }
        //


        #region 드래곤 관련 작업
        private void CheckDragon()
        {
            SettingDragonVisible();
        }
        public void SettingDragonVisible()
        {
            DragonsVisible();
            if (guildBuilding != null)
                guildBuilding.RefreshTownDragon();
        }
        
        public TownDragonSpine GetTownDragon(int tag)
        {
            if (TownDragons.ContainsKey(tag))
                return TownDragons[tag];

            if(TownDragonsDic.ContainsKey(tag))
            {
                TownDragons[tag] = TownDragonsDic[tag];
                TownDragons[tag].SetActive(false);
                return TownDragons[tag];
            }

            var curDragon = User.Instance.DragonData.GetDragon(tag);
            if (curDragon == null)
                return null;

            var dragonData = curDragon.BaseData;
            if (dragonData == null)
                return null;

            if (!dragonData.IS_USE)
                return null;

            if(DragonsParent == null)
            {
                DragonsParent = new GameObject("TownDragonsParent");
                DontDestroyOnLoad(DragonsParent);
            }

            TownDragonSpine dragon = dragonData.LoadTownDragonSpine(curDragon, DragonsParent.transform);
            
            TownDragonsDic[tag] = TownDragons[tag] = dragon;            

            return dragon;
        }

        public void OnLogout()
        {
            if (DragonsParent != null)
                Destroy(DragonsParent);
        }

        void DragonsVisible()
        {
            if (TownDragons == null)
                return;

            var userdragons = User.Instance.DragonData.GetAllUserDragons();
            if (userdragons == null || userdragons.Count <= 0)
                return;

            int dragonCount = userdragons.Count;
            if (dragonCount <= 0)
                return;

            bool toyState = true;
            float toyPercent = 1f;
            var setting = PlayerPrefs.GetString("Setting_Toy", "");
            if (setting != "")
            {
                JObject toySettingData = JObject.Parse(setting);
                if (SBFunc.IsJTokenCheck(toySettingData["isOn"])) toyState = toySettingData["isOn"].ToObject<bool>();
                if (SBFunc.IsJTokenCheck(toySettingData["value"])) toyPercent = toySettingData["value"].ToObject<float>();
            }

            onDragonCount = 0;

            int maxtoycount = Mathf.Min(Mathf.FloorToInt(dragonCount * toyPercent), GameConfigTable.GetConfigIntValue("TOWN_DRAGON_LIMIT"));

            List<UserDragon> dragons = null;
            try
            {
                if (dragonCount > maxtoycount)
                {
                    dragons = userdragons.OrderBy(x =>
                    {
                        if (x.Tag >= 15000) return 5f;
                        else if (x.Tag >= 14000) return Random.Range(4f, 5f);
                        else if (x.Tag >= 13000) return Random.Range(2f, 5f);
                        else if (x.Tag >= 12000) return Random.Range(0.75f, 5f);
                        else return Random.Range(0f, 5f);
                    })
                    .ToList();
                }
                else
                {
                    dragons = new List<UserDragon>(userdragons);
                }
            }
            catch
            {
                Debug.LogError("dragon sort fail");
            }
            List<int> hideDragons = new List<int>();
            var travelData = User.Instance.GetLandmarkData<LandmarkTravel>();
            if (travelData != null)
            {
                if (travelData.TravelState == eTravelState.Travel)
                {
                    foreach (var travelDragon in travelData.TravelDragon)
                    {
                        if (travelDragon != null)
                        {
                            hideDragons.Add(travelDragon.Tag);
                        }
                    }
                }
            }
            var gemdungeonData = LandmarkGemDungeon.Get();
            if (gemdungeonData != null)
            {
                var it = gemdungeonData.FloorDatas.GetEnumerator();
                while (it.MoveNext())
                {
                    switch (it.Current.Value.State)
                    {
                        case eGemDungeonState.BATTLE:
                        case eGemDungeonState.END:
                            hideDragons.AddRange(it.Current.Value.Dragons);
                            break;
                        default:
                            continue;
                    }
                }
            }

            int hideCount = dragons.Count - maxtoycount;

            foreach (var data in dragons)
            {
                if (data.BaseData != null && !data.BaseData.IS_USE)
                    continue;

                if (hideCount > 0)
                {
                    hideCount--;
                    if (TownDragonsDic.ContainsKey(data.Tag))
                    {
                        TownDragonSpine tmpD = TownDragonsDic[data.Tag];
                        if (tmpD != null)
                        {
                            tmpD.ClearElevator();
                            DestroyImmediate(tmpD.gameObject);
                        }

                        TownDragonsDic.Remove(data.Tag);
                        TownDragons.Remove(data.Tag);
                    }
                    continue;
                }

                var dragon = GetTownDragon(data.Tag);
                if (dragon != null)
                {
                    dragon.SetActive(!hideDragons.Contains(data.Tag));
                    ++onDragonCount;
                }
            }
        }

        public bool IsTownAllDragonActive()
        {
            var setting = PlayerPrefs.GetString("Setting_Toy", "");
            if (setting == "") return true;
            bool toyState = true;
            JObject toySettingData = JObject.Parse(setting);
            if (SBFunc.IsJTokenCheck(toySettingData["isOn"])) toyState = toySettingData["isOn"].ToObject<bool>();
            return toyState;
        }

        public bool IsTownDragonActive(int tag)
        {
            if(TownDragons.ContainsKey(tag))
                return TownDragons[tag].IsActive();
            
            return false;
        }

        public void CamFollowDragon(int tag, Vector2 addPos = new Vector2())
        {
            if (IsTownDragonActive(tag))
            {
                cam.UpdateScreenSize((Screen.height / (float)Screen.width) * 3f);                
                CurFollowDragonTag = tag;
                if(IsCamDragonFollow)
                    CameraFollowObject(GetTownDragon(CurFollowDragonTag).Skeleton.transform, Vector2.up * 0.8f + addPos);
            }
            else
            {
                var setting = PlayerPrefs.GetString("Setting_Toy", "");
                if (setting == "") return;
                
                JObject toySettingData = JObject.Parse(setting);
                
                GetTownDragon(tag).SetActive(true);
                //SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), string.Format(StringData.GetStringByIndex(100002677), onDragonCount, onDragonCount + 1),
                //    () => {
                        cam.UpdateScreenSize((Screen.height / (float)Screen.width) * 3f);
                        var newToyPercent = (++onDragonCount) / (float)TownDragons.Count;
                        if (SBFunc.IsJTokenCheck(toySettingData["value"])) toySettingData["value"] = newToyPercent;
                        PlayerPrefs.SetString("Setting_Toy", toySettingData.ToString());
                        CurFollowDragonTag = tag;
                        if (IsCamDragonFollow)
                            CameraFollowObject(GetTownDragon(tag).Skeleton.transform, Vector2.up * 0.8f + addPos);                        
                //    },
                //    () => {
                //        TownDragons[tag].SetActive(false);
                //    },
                //    () => {
                //        TownDragons[tag].SetActive(false);
                //    }
                //);
            }
        }
        


        #endregion
        #region BG 관련 작업
        private void SetNightTime()
        {
            Color trclr = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            bgDay.color = trclr;
            bgNight.color = Color.white;
            bgDay.sortingOrder = -100;
            bgNight.sortingOrder = -99;

            farGround_Day.SetActive(false);
            farGround_Dusk.SetActive(false);
            farGround_Night.SetActive(true);

            foreach (var spr in cloudsRenderers)
            {
                spr.color = nightCloudColor;
            }

            foreach (var spr in fargrounds_night)
            {
                spr.color = Color.white;
            }

            foreach (var spr in fargrounds_dusk)
            {
                spr.color = trclr;
            }

            foreach (var spr in lightingObjects)
            {
                spr.color = new Color(180, 180, 180);
            }
        }

        private void SetDayTime()
        {
            Color trclr = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            bgDay.color = Color.white;
            bgNight.color = trclr;
            bgDay.sortingOrder = -99;
            bgNight.sortingOrder = -100;

            farGround_Day.SetActive(true);
            farGround_Dusk.SetActive(false);
            farGround_Night.SetActive(false);

            foreach (var spr in cloudsRenderers)
            {
                spr.color = dayCloudColor;
            }

            foreach (var spr in fargrounds_day)
            {
                spr.color = Color.white;
            }

            foreach (var spr in fargrounds_dusk)
            {
                spr.color = trclr;
            }

            foreach (var spr in lightingObjects)
            {
                spr.color = Color.white;
            }
        }

        private void AddNightToDayAction(float animTime)
        {
            Color trclr = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            float fadeInTime = animTime * 0.25f;
            float duskDelay = animTime * 0.25f;
            float fadeOutTime = animTime * 0.5f;
            int outOrder = -12;
            int inOrder = -11;

            Sequence seq = DOTween.Sequence();

            seq.AppendCallback(() => {
                farGround_Day.SetActive(false);
                farGround_Dusk.SetActive(true);
                farGround_Night.SetActive(true);

                foreach (var spr in fargrounds_night)
                {
                    spr.color = Color.white;
                    spr.sortingOrder = outOrder;
                }

                Color trclr = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                foreach (var spr in fargrounds_dusk)
                {
                    spr.color = trclr;
                    spr.sortingOrder = inOrder;
                }
            });
            
            foreach (var spr in fargrounds_night)
            {
                seq.Join(spr.DOFade(0.0f, fadeOutTime).SetEase(Ease.InCubic));
            }

            foreach (var spr in fargrounds_dusk)
            {
                seq.Join(spr.DOFade(1.0f, fadeInTime).SetEase(Ease.OutCubic));                
            }

            foreach (var spr in lightingObjects)
            {
                seq.Join(spr.DOColor(Color.white, animTime).SetEase(Ease.OutCubic));
            }

            foreach (var spr in cloudsRenderers)
            {
                seq.Join(spr.DOColor(duskCloudColor, fadeInTime).SetEase(Ease.OutCubic));
            }

            seq.AppendInterval(duskDelay);
            seq.AppendCallback(() => {
                farGround_Day.SetActive(true);
                farGround_Dusk.SetActive(true);
                farGround_Night.SetActive(false);

                foreach (var spr in fargrounds_dusk)
                {
                    spr.sortingOrder = outOrder;
                }

                Color trclr = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                foreach (var spr in fargrounds_day)
                {
                    spr.color = trclr;
                    spr.sortingOrder = inOrder;
                }
            });

            foreach (var spr in fargrounds_dusk)
            {
                seq.Join(spr.DOFade(0.0f, fadeOutTime).SetEase(Ease.InCubic));
            }

            foreach (var spr in fargrounds_day)
            {
                seq.Join(spr.DOFade(1.0f, fadeInTime).SetEase(Ease.OutCubic));
            }

            foreach (var spr in cloudsRenderers)
            {
                seq.Join(spr.DOColor(dayCloudColor, fadeInTime).SetEase(Ease.OutCubic));
            }

            daylightSequence.Join(seq);
        }

        private void AddDayToNightAction(float animTime)
        {
            Color trclr = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            float fadeInTime = animTime * 0.25f;
            float duskDelay = animTime * 0.25f;
            float fadeOutTime = animTime * 0.5f;
            int outOrder = -12;
            int inOrder = -11;

            Sequence seq = DOTween.Sequence();

            seq.AppendCallback(() => {
                farGround_Day.SetActive(true);
                farGround_Dusk.SetActive(true);
                farGround_Night.SetActive(false);

                foreach (var spr in fargrounds_day)
                {
                    spr.color = Color.white;
                    spr.sortingOrder = outOrder;
                }

                Color trclr = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                foreach (var spr in fargrounds_dusk)
                {
                    spr.color = trclr;
                    spr.sortingOrder = inOrder;
                }
            });

            foreach (var spr in fargrounds_day)
            {
                seq.Join(spr.DOFade(0.0f, fadeOutTime).SetEase(Ease.InCubic));
            }

            foreach (var spr in fargrounds_dusk)
            {
                seq.Join(spr.DOFade(1.0f, fadeInTime).SetEase(Ease.OutCubic));
            }

            foreach (var spr in lightingObjects)
            {
                seq.Join(spr.DOColor(new Color(180, 180, 180), animTime).SetEase(Ease.OutCubic));
            }

            foreach (var spr in cloudsRenderers)
            {
                seq.Join(spr.DOColor(duskCloudColor, fadeOutTime).SetEase(Ease.OutCubic));
            }

            seq.AppendInterval(duskDelay);
            seq.AppendCallback(() => {
                farGround_Day.SetActive(false);
                farGround_Dusk.SetActive(true);
                farGround_Night.SetActive(true);
                
                foreach (var spr in fargrounds_dusk)
                {
                    spr.sortingOrder = outOrder;
                }

                Color trclr = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                foreach (var spr in fargrounds_night)
                {
                    spr.color = trclr;
                    spr.sortingOrder = inOrder;
                }
            });

            foreach (var spr in fargrounds_dusk)
            {
                seq.Join(spr.DOFade(0.0f, fadeOutTime).SetEase(Ease.InCubic));
            }

            foreach (var spr in fargrounds_night)
            {
                seq.Join(spr.DOFade(1.0f, fadeInTime).SetEase(Ease.OutCubic));
            }

            foreach (var spr in cloudsRenderers)
            {
                seq.Join(spr.DOColor(nightCloudColor, fadeInTime).SetEase(Ease.OutCubic));
            }

            daylightSequence.Join(seq);
        }

        private void CheckBG()
        {
            if(bgSky == null || bgStar == null)
                return;
            //cloudDist
            fargrounds_day = farGround_Day.GetComponentsInChildren<SpriteRenderer>(true).ToList();
            cloudsRenderers.Clear();
            cloudsRenderers.AddRange(clouds.GetComponentsInChildren<SpriteRenderer>(true).ToList());
            
            fargrounds_dusk = farGround_Dusk.GetComponentsInChildren<SpriteRenderer>(true).ToList();            
            fargrounds_night = farGround_Night.GetComponentsInChildren<SpriteRenderer>(true).ToList();

            var scale = bgSky.localScale;
            float guildSpancing = GuildManager.Instance.GuildBuildingShowAble ? 0.4f : 0f ;
            scale.y = TownMap.EndOffsetY() / 12.8f + 0.1f + guildSpancing;
            bgSky.localScale = scale;
            
            var skyPos = new Vector3(0.0f, -0.5f, 0.0f);            
            var sunmoonMiddlePos = TownMap.GetHeadPos(drawCount.y + 1) + new Vector2(0.0f, 11.0f);
            var sunmoonReversePos = -sunmoonMiddlePos;
            var cloudPos = TownMap.GetHeadPos(drawCount.y);

            if (cloudSequences != null)
            {
                cloudSequences.Kill();
            }
            
            var cloudSequence = DOTween.Sequence();
            cloudSequences = cloudSequence;
            clouds.transform.position = cloudPos;

            cloudSequence.Append(clouds.transform.DOLocalMoveX(cloudDist * 5, 220.0f).SetEase(Ease.Linear));
            cloudSequence.AppendCallback(() =>
            {
                thirdClouds.localPosition = new Vector2(-cloudDist * 2, cloudbaseHeight);
            });
            cloudSequence.Append(clouds.transform.DOLocalMoveX(cloudDist * 10, 220.0f).SetEase(Ease.Linear));
            cloudSequence.AppendCallback(() =>
            {
                secondClouds.localPosition = new Vector2(-cloudDist * 3, cloudbaseHeight);
            });
            cloudSequence.Append(clouds.transform.DOLocalMoveX(cloudDist * 15, 220.0f).SetEase(Ease.Linear));
            cloudSequence.AppendCallback(() =>
            {
                firstClouds.localPosition = new Vector2(-cloudDist, cloudbaseHeight);
                secondClouds.localPosition = new Vector2(0, cloudbaseHeight);
                thirdClouds.localPosition = new Vector2(cloudDist, cloudbaseHeight);
            });
            cloudSequence.SetLoops(-1, LoopType.Restart).SetId("cloud");
            //cloud.transform.DOLocalMoveX(xPos, 150.0f);

            float animTime = 30.0f;


            if (daylightSequence != null)
                daylightSequence.Kill();

#if true
            Color trclr = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            if (SBFunc.RandomValue > 0.5f)//밤으로 시작
            {
                bgSky.localPosition = skyPos;
                bgStar.localPosition = new Vector3(0f, scale.y * 12.8f, 0f);                
                bgSun.parent.localPosition = sunmoonReversePos;
                bgMoon.parent.localPosition = sunmoonMiddlePos;
                bgMoon.localScale = new Vector3(1, -1, 1);


                SetNightTime();

                daylightSequence = DOTween.Sequence();

                //밤
                daylightSequence.AppendInterval(animTime);
                daylightSequence.AppendCallback(() =>
                {
                    SetNightTime();

                   // bgSun.localPosition = sunmoonReversePos;
                    //bgSun.transform.position = Vector3.zero;

                    bgDay.color = trclr;
                    bgNight.color = Color.white;
                    bgDay.sortingOrder = -99;
                    bgNight.sortingOrder = -100;
                });
                //아침               
                daylightSequence.Append(bgStar.DOLocalMoveY(scale.y * 12.8f * 2.0f, animTime));
                daylightSequence.Join(bgOrbit.DORotate(new Vector3(0,0,-180),animTime));

                daylightSequence.Join(bgDay.DOColor(Color.white, animTime));
                daylightSequence.Join(bgNight.DOColor(trclr, animTime));

                AddNightToDayAction(animTime);

                //낮
                daylightSequence.AppendInterval(animTime);
                daylightSequence.AppendCallback(() =>
                {
                    SetDayTime();

                    //bgMoon.transform.position = Vector3.zero;
                    //bgMoon.localPosition = sunmoonReversePos;

                    bgDay.color = Color.white;
                    bgNight.color = trclr;
                    bgDay.sortingOrder = -100;
                    bgNight.sortingOrder = -99;
                });

                //저녁
                daylightSequence.Append(bgStar.DOLocalMoveY(scale.y * 12.8f, animTime));
                daylightSequence.Join(bgOrbit.DORotate(new Vector3(0, 0, -360), animTime)); 

                daylightSequence.Join(bgDay.DOColor(trclr, animTime));
                daylightSequence.Join(bgNight.DOColor(Color.white, animTime));

                AddDayToNightAction(animTime);
                //밤

                daylightSequence.SetLoops(-1, LoopType.Restart);
            }
            else //낮으로 시작
            {
                bgSky.localPosition = skyPos;
                bgStar.localPosition = new Vector3(0f, scale.y * 12.8f * 2.0f, 0f);                
                bgSun.parent.localPosition = sunmoonMiddlePos;
                bgMoon.parent.localPosition = sunmoonReversePos;
                
                SetDayTime();

                daylightSequence = DOTween.Sequence();

                //낮
                daylightSequence.AppendInterval(animTime);
                daylightSequence.AppendCallback(() =>
                {
                    SetDayTime();

                    //bgMoon.localPosition = sunmoonReversePos;

                    bgDay.color = Color.white;
                    bgNight.color = trclr;
                    bgDay.sortingOrder = -100;
                    bgNight.sortingOrder = -99;
                });
                //저녁
                daylightSequence.Append(bgStar.DOLocalMoveY(scale.y * 12.8f, animTime));
                daylightSequence.Join(bgOrbit.DORotate(new Vector3(0, 0, -180), animTime));
                
                daylightSequence.Join(bgDay.DOColor(trclr, animTime));
                daylightSequence.Join(bgNight.DOColor(Color.white, animTime));

                AddDayToNightAction(animTime);

                //밤
                daylightSequence.AppendInterval(animTime);
                daylightSequence.AppendCallback(() =>
                {
                    SetNightTime();

                    //bgSun.localPosition = sunmoonReversePos;

                    bgDay.color = trclr;
                    bgNight.color = Color.white;
                    bgDay.sortingOrder = -99;
                    bgNight.sortingOrder = -100;
                });
                //아침
                daylightSequence.Append(bgStar.DOLocalMoveY(scale.y * 12.8f * 2.0f, animTime));
                daylightSequence.Join(bgOrbit.DORotate(new Vector3(0, 0, -360), animTime));

                daylightSequence.Join(bgDay.DOColor(Color.white, animTime));
                daylightSequence.Join(bgNight.DOColor(trclr, animTime));

                AddNightToDayAction(animTime);
                //낮

                daylightSequence.SetLoops(-1, LoopType.Restart);
            }
#else
            bgSky.localPosition = skyPos;
            bgStar.localPosition = new Vector3(0f, scale.y * 12.8f, 0f);
            bgSun.transform.position = Vector3.zero;
            bgMoon.transform.position = Vector3.zero;
            
            SetNightTime();
#endif
        }
        #endregion
        #region CellBG 관련 작업
        private void CheckCell()
        {
            var openData = User.Instance.GetOpenFloorData();
            GameObject wall = TownFactory.GetWall();
            GameObject locked = TownFactory.GetLocked();

            for (int floor = drawStart.y; floor <= drawCount.y; ++floor)
            {
                if (!cellDics.ContainsKey(floor))
                    cellDics.Add(floor, new Dictionary<int, GameObject>());

                if (openData.x <= floor && openData.y >= floor)
                {
                    for (var cell = drawStart.x; cell < drawCount.x; ++cell)
                    {
                        if (!cellDics[floor].ContainsKey(cell))
                        {
                            GameObject targetCell = GetCell(floor, cell);

                            if (targetCell != null)
                            {
                                var target = Instantiate(targetCell, townBase.transform);
                                if (floor >= 0)
                                    lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                                target.GetComponent<Cell>()?.Init(cell, floor);
                                target.transform.localPosition = TownMap.GetCellPos(cell, floor);
                                cellDics[floor].Add(cell, target);
                            }
                        }
                        else
                        {
                            cellDics[floor][cell].transform.localPosition = TownMap.GetCellPos(cell, floor);
                        }

                        if(cell != drawStart.x && floor >= 0)
                        {
                            if (!wallDics.ContainsKey(floor))
                                wallDics.Add(floor, new Dictionary<int, GameObject>());

                            if (!wallDics[floor].ContainsKey(cell))
                            {
                                var target = Instantiate(wall, townBase.transform);
                                if (floor >= 0)
                                    lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                                target.transform.localPosition = TownMap.GetWallPos(cell, floor);
                                wallDics[floor].Add(cell, target);
                            }
                            else
                            {
                                wallDics[floor][cell].transform.localPosition = TownMap.GetWallPos(cell, floor);
                            }
                        }
                    }

                    if(lockedDics != null)
                    {
                        if(lockedDics.ContainsKey(floor) && lockedDics[floor] != null)
                        {
                            lockedDics[floor].transform.localPosition = new Vector2(0f, TownMap.GetCellPosY(floor));
                            lockedDics[floor].gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    for (var cell = drawStart.x; cell < drawCount.x; ++cell)
                    {
                        if (!cellDics[floor].ContainsKey(cell))
                        {
                            GameObject targetCell = GetCell(floor, cell);

                            if (targetCell != null)
                            {
                                var target = Instantiate(targetCell, townBase.transform);
                                if(floor >= 0)
                                    lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                                target.transform.localPosition = TownMap.GetCellPos(cell, floor);
                                cellDics[floor].Add(cell, target);
                            }
                        }
                        else
                        {
                            cellDics[floor][cell].transform.localPosition = TownMap.GetCellPos(cell, floor);
                        }

                        if (cell != drawStart.x && floor >= 0)
                        {
                            if (!wallDics.ContainsKey(floor))
                                wallDics.Add(floor, new Dictionary<int, GameObject>());

                            if (!wallDics[floor].ContainsKey(cell))
                            {
                                var target = Instantiate(wall, townBase.transform);
                                if (floor >= 0)
                                    lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                                target.transform.localPosition = TownMap.GetWallPos(cell, floor);
                                wallDics[floor].Add(cell, target);
                            }
                            else
                            {
                                wallDics[floor][cell].transform.localPosition = TownMap.GetWallPos(cell, floor);
                            }
                        }
                    }

                    if (lockedDics != null)
                    {
                        if (!lockedDics.ContainsKey(floor))
                        {
                            var target = Instantiate(locked, townBase.transform);
                            var lTarget = target.GetComponent<Locked>();
                            if (lTarget != null)
                                lTarget.Resize();
                            target.transform.localPosition = new Vector2(0f, TownMap.GetCellPosY(floor));
                            lockedDics.Add(floor, lTarget);
                        } 
                        else if (lockedDics[floor] != null)
                        {
                            lockedDics[floor].transform.localPosition = new Vector2(0f, TownMap.GetCellPosY(floor));
                            lockedDics[floor].gameObject.SetActive(true);
                        }
                    }
                }
            }

            if (GuildManager.Instance.GuildBuildingShowAble)
            {
                int guildFloor = drawCount.y + 1;
                if (guildCellDics != null)
                {
                    //for (var cell = 0; cell < 3; ++cell)
                    //{
                    //    if (!guildCellDics.ContainsKey(cell))
                    //    {
                    //        GameObject targetCell = null;
                    //        targetCell = TownFactory.GetGuildCell(cell);

                    //        if (targetCell != null)
                    //        {
                    //            var target = Instantiate(targetCell, townBase.transform);
                    //            lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                    //            target.GetComponent<Cell>()?.Init(cell, guildFloor);
                    //            target.transform.localPosition = TownMap.GetGuildCellPos(cell, guildFloor);
                    //            guildCellDics[cell] = target;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        guildCellDics[cell].transform.localPosition = TownMap.GetGuildCellPos(cell, guildFloor);
                    //    }
                    //}

                }
            }
        }

        public GameObject GetCell(int floor, int cell)
        {
            switch (floor)
            {
                case -1:
                    return TownFactory.GetUnderCell(cell);
                //case 0:
                //    if (cell > 1)
                //    {
                //        return TownFactory.GetCell(cell);
                //    }
                //    else
                //    {
                //        return TownFactory.GetDozerCell(cell);
                //    }
                    
                default:
                    return TownFactory.GetCell(cell);
            }
        }

        /// <summary> Todo : 던전 Cell 구성해서 넘기기 </summary>
        public GameObject GetGemdungeonCell(int floor, int cell)
        {
            return TownFactory.GetGemUnderCell(cell);
        }

        #endregion
        #region Building 관련 작업
        private void CheckBuilding()
        {
            var gridData = User.Instance.GetActiveGridData();
            var floorIt = gridData.GetEnumerator();

            while(floorIt.MoveNext())
            {
                int floor = floorIt.Current.Key;
                var cellDic = floorIt.Current.Value;

                var cellIt = cellDic.GetEnumerator();
                while(cellIt.MoveNext())
                {
                    int cell = cellIt.Current.Key;
                    int tag = cellIt.Current.Value;

                    CheckBuilding(tag, floor, cell);
                }
            }

            CheckGemDungeon();
            CheckGuildBuilding();
        }
        private void CheckBuilding(int tag, int floor, int cell)
        {
            if (buildingDics == null)
                return;

            var openData = BuildingOpenData.GetWithTag(tag);
            var buildingData = User.Instance.GetUserBuildingInfoByTag(tag);
            if (openData == null || buildingData == null)
                return;

            switch (tag)
            {
                case (int)eLandmarkType.SUBWAY:
                {
                    CheckTrain(buildingData, cell, floor);
                    return;
                }
                case (int)eLandmarkType.Dozer:
                {
                    CheckDozer(buildingData, cell, floor);
                    return;
                }
                case (int)eLandmarkType.Travel:
                {
                    CheckTravel(buildingData, cell, floor);
                    return;
                }
                case (int)eLandmarkType.EXCHANGE:
                {
                    CheckExchange(buildingData, cell, floor);
                    return;
                }
            }

            if (buildingDics.ContainsKey(tag))
            {
                buildingDics[tag].Init(cell, floor);
                buildingDics[tag].transform.localPosition = TownMap.GetBuildingPos(cell, floor);
                return;
            }

            var prefab = TownFactory.GetBuilding(openData.BUILDING);
            if (prefab == null)
                return;

            var target = Instantiate(prefab, townBase.transform);
            target.transform.localPosition = TownMap.GetBuildingPos(cell, floor);
            var building = target.GetComponent<Building>();
            if (building != null)
            {
                building.Init(cell, floor, buildingData, tag, openData.BUILDING);
                buildingDics.Add(tag, building);
            }
            else
            {
                Destroy(target.gameObject);
            }
        }
        public Building GetBuilding(int buildingTag)
        {
            if(buildingDics.ContainsKey(buildingTag))
            {
                return buildingDics[buildingTag];
            }

            return null;
        }

        public List<Building> GetSameTypeBuildings(Building target)
        {
            List<Building> ret = new List<Building>();
            foreach(var building in buildingDics.Values)
            {
                if(building != null)
                {
                    if (building.BName == target.BName)
                    {
                        ret.Add(building);
                    }
                }
            }

            return ret;
        }

        public Building GetBuilding(int cell, int floor)
        {
            var it = buildingDics.GetEnumerator();
            while(it.MoveNext())
            {
                if (it.Current.Value == null)
                    continue;

                if (it.Current.Value.Cell == cell && it.Current.Value.Floor == floor)
                    return it.Current.Value;
            }

            return null;
        }

        public bool isActiveCell(int floor, int cell)
        {
			int maxHeight = (int)User.Instance.GetOpenFloorData().y;
            if (TownMap.X > cell || cell >= TownMap.Width || floor >= TownMap.Height || floor < TownMap.Y)
            {
                return false;
            }

            return maxHeight >= floor;
        }
        public bool IsGemDungeonCell(int floor, int cell)
        {
            if (TownMap.X > cell || cell >= TownMap.Width)
                return false;
            var itr = Gemdungeon.GetEnumerator();
            while(itr.MoveNext())
            {
                if (floor == itr.Current.Key)
                    return true;
            }
            return false;
        }
        
        public bool IsGuildPos (int cell, Vector2 inputPos)
        {
            if (TownMap.X > cell || cell >= TownMap.Width) // 선 체크 용
                return false;
            
            bool xCondition = false;

            float center = TownMap.Width / 2f;
            float fixedPos = (inputPos.x + TownMap.Width * SBDefine.CellSpancing * 0.5f) / SBDefine.CellSpancing;
            xCondition = Mathf.Abs(center - fixedPos) < SBDefine.GuildCellWidth / 2f;

            bool yCondition = false;

            float inputFloor = inputPos.y / SBDefine.FloorSpancing;
            float guildStartY = TownMap.Height+ SBDefine.GuildYStartSpancing;
            float guildEndY = guildStartY + SBDefine.GuildTopSpancing;
            yCondition = inputFloor> guildStartY && inputFloor < guildEndY;
            
            return xCondition && yCondition;
            
        }
        public bool isLockedFloor(int floor, int cell)
        {
            int maxHeight = (int)User.Instance.GetOpenFloorData().y;

            if (TownMap.X > cell || cell >= TownMap.Width || floor >= TownMap.Height || floor < TownMap.Y)
            {
                return false;
            }

            return maxHeight < floor;
        }


#endregion
        #region Elevator 관련
        private void CheckElevator()
        {
            Transform TopElevatorGearL = null;
            Transform BotElevatorGearL = null;
            Transform TopElevatorGearR = null;
            Transform BotElevatorGearR = null;

            for (int floor = drawStart.y; floor <= drawCount.y; ++floor)
            {
                if (!elevatorDics.ContainsKey(floor))
                    elevatorDics.Add(floor, new Dictionary<bool, GameObject>());

                GameObject elevatorBG = TownFactory.GetElevatorBG(floor);
                if (elevatorBG != null)
                {
                    if (!elevatorDics[floor].ContainsKey(true))
                    {
                        var targetL = Instantiate(elevatorBG, townBase.transform);
                        lightingObjects.AddRange(targetL.GetComponentsInChildren<SpriteRenderer>());

                        targetL.transform.localPosition = TownMap.GetElevatorPos(floor, true);
                        elevatorDics[floor].Add(true, targetL);
                    }
                    else
                    {
                        elevatorDics[floor][true].transform.localPosition = TownMap.GetElevatorPos(floor, true);
                    }

                    if (!elevatorDics[floor].ContainsKey(false))
                    {
                        var targetR = Instantiate(elevatorBG, townBase.transform);
                        lightingObjects.AddRange(targetR.GetComponentsInChildren<SpriteRenderer>());

                        targetR.transform.localPosition = TownMap.GetElevatorPos(floor, false);
                        elevatorDics[floor].Add(false, targetR);
                    }
                    else
                    {
                        elevatorDics[floor][false].transform.localPosition = TownMap.GetElevatorPos(floor, false);
                    }


                    if (floor == (TownMap.Height - 1))
                    {
                        TopElevatorGearL = elevatorDics[floor][true].transform.Find("pulley");
                        TopElevatorGearR = elevatorDics[floor][false].transform.Find("pulley");
                    }
                    else if (floor < 0)
                    {
                        BotElevatorGearL = elevatorDics[floor][true].transform.Find("pulley");
                        BotElevatorGearR = elevatorDics[floor][false].transform.Find("pulley");
                    }
                }

            }

            var top = drawCount.y + 1;

            if (decoL == null)
            {
                GameObject elevatorD = TownFactory.GetElevatorDeco();
                if (elevatorD != null)
                {
                    var target = Instantiate(elevatorD, townBase.transform);
                    lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                    target.transform.localPosition = TownMap.GetElevatorPos(top, true);
                    decoL = target;
                }
            }
            else
            {
                decoL.transform.localPosition = TownMap.GetElevatorPos(top, true);
            }

            if (decoR == null)
            {
                GameObject elevatorD = TownFactory.GetElevatorDeco();
                if (elevatorD != null)
                {
                    var target = Instantiate(elevatorD, townBase.transform);
                    lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                    target.transform.localPosition = TownMap.GetElevatorPos(top, false);
                    decoR = target;
                }
            }
            else
            {
                decoR.transform.localPosition = TownMap.GetElevatorPos(top, false);
            }

            if (elevatorL == null)
            {
                GameObject elevator = TownFactory.GetElevator();
                //elevator.GetComponent<Elevator>().Init();
                if (elevator != null)
                {
                    var target = Instantiate(elevator, townBase.transform);
                    target.transform.localPosition = new Vector2(-TownMap.GetElevatorPosX(), target.transform.localPosition.y);
                    elevatorL = target.GetComponent<Elevator>();
                    elevatorL.Init(TopElevatorGearL, BotElevatorGearL);
                    elevatorL.name = "ElevatorL";
                }
            }
            else
            {
                elevatorL.transform.localPosition = new Vector2(-TownMap.GetElevatorPosX(), elevatorL.transform.localPosition.y);
            }

            if (elevatorR == null)
            {
                GameObject elevator = TownFactory.GetElevator();
                //elevator.GetComponent<Elevator>().Init();
                if (elevator != null)
                {
                    var target = Instantiate(elevator, townBase.transform);
                    target.transform.localPosition = new Vector2(TownMap.GetElevatorPosX(), target.transform.localPosition.y);
                    elevatorR = target.GetComponent<Elevator>();
                    elevatorR.Init(TopElevatorGearR, BotElevatorGearR);
                    elevatorR.name = "ElevatorR";
                }
            }
            else
            {
                elevatorR.transform.localPosition = new Vector2(TownMap.GetElevatorPosX(), elevatorR.transform.localPosition.y);
            }

            if (GuildManager.Instance.GuildBuildingShowAble) 
            { 
                if (guildTopLFlag == null)
                {
                    GameObject guildTopL = TownFactory.GetGuildFlagTop();
                    if (guildTopL != null)
                    {
                        var target = Instantiate(guildTopL, townBase.transform);
                        lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                        target.transform.localPosition = TownMap.GetTopFlagPos(top, true);
                        guildTopLFlag = target;
                    }
                }
                else
                {
                    guildTopLFlag.transform.localPosition = TownMap.GetTopFlagPos(top, true);
                }
                if (guildTopRFlag == null)
                {
                    GameObject guildTopR = TownFactory.GetGuildFlagTop();
                    if (guildTopR != null)
                    {
                        var target = Instantiate(guildTopR, townBase.transform);
                        lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                        target.transform.localPosition = TownMap.GetTopFlagPos(top, false);
                        guildTopRFlag = target;
                    }
                }
                else
                {
                    guildTopRFlag.transform.localPosition = TownMap.GetTopFlagPos(top, false);
                }
                var flagCellL = guildTopLFlag.GetComponent<FlagCell>();
                var flagCellR = guildTopRFlag.GetComponent<FlagCell>();
                if (GuildManager.Instance.IsNoneGuild)
                {
                    flagCellR.SetGuildState(false);
                    flagCellL.SetGuildState(false);
                }
                else
                {
                    flagCellR.SetGuildState(true);
                    flagCellL.SetGuildState(true);
                    int markNo = GuildManager.Instance.MyGuildInfo.GetGuildMark();
                    int emblemNo = GuildManager.Instance.MyGuildInfo.GetGuildEmblem();
                    var flagImg = GuildResourceData.DEFAULT_FLAG;

                    Sprite markSprite = GuildResourceData.DEFAULT_MARK;
                    var mark = GuildResourceData.Get(markNo);
                    if (mark != null)
                        markSprite = mark.RESOURCE;

                    Sprite emblemSprite = GuildResourceData.DEFAULT_EMBLEM;
                    var emblem = GuildResourceData.Get(emblemNo);
                    if (emblem != null)
                        emblemSprite = emblem.RESOURCE;

                    flagCellL.SetFlagImage(flagImg, markSprite, emblemSprite);
                    flagCellR.SetFlagImage(flagImg, markSprite, emblemSprite);
                }
            }
        }
        public Elevator GetElevator(bool isRight)
        {
            return isRight ? elevatorR : elevatorL;
        }
#endregion
        #region Deco 및 Escalator 관련
        private void CheckDeco()
        {
            int top = drawCount.y + 1;
            for (var cell = drawStart.x; cell < drawCount.x; ++cell)
            {
                if(!decoDics.ContainsKey(cell))
                {
                    GameObject targetCell = TownFactory.GetTopCell(cell);
                    if (targetCell != null)
                    {
                        var topD = Instantiate(targetCell, townBase.transform);
                        lightingObjects.AddRange(topD.GetComponentsInChildren<SpriteRenderer>());

                        topD.transform.localPosition = TownMap.GetCellPos(cell, top);
                        decoDics.Add(cell, topD);
                    }
                }

                if (decoDics.ContainsKey(cell))
                {
                    decoDics[cell].transform.localPosition = TownMap.GetCellPos(cell, top);
                }
            }

            //목을 쳐내기
            if (head == null)
            {
                GameObject headPrefab = TownFactory.GetHead();
                if (headPrefab != null)
                {
                    head = Instantiate(headPrefab, townBase.transform);
                    lightingObjects.AddRange(head.GetComponentsInChildren<SpriteRenderer>());

                    head.transform.localScale = Vector3.one*0.8f;
                }


            }

            if (head != null)
            {
                //#if UNITY_EDITOR // 목을 쳐내고 해당부분에 여행사 공항을 추가할 예정이지만 현재 빈공간이라 휑하다는 피드백. 빌드에서는 대가리 나오게 임시처리
                //                head.transform.SetPositionAndRotation(TownMap.GetSliceHeadPos(drawStart.x), Quaternion.Euler(new Vector3(0f, 0f, 100f)));
                //#else
                //                head.transform.localPosition = TownMap.GetHeadPos(top);
                //#endif                


                if (GuildManager.Instance.GuildBuildingShowAble)
                {
                    //for (int i = 0; i < 3; ++i)
                    //{
                    //    if (guildDecoDics.ContainsKey(i) == false)
                    //    {
                    //        GameObject targetCell = TownFactory.GetGuildTopCell(i);
                    //        var topD = Instantiate(targetCell, townBase.transform);
                    //        lightingObjects.AddRange(topD.GetComponentsInChildren<SpriteRenderer>());

                    //        topD.transform.localPosition = TownMap.GetGuildTopPos(i, top);
                    //        guildDecoDics.Add(i, targetCell);
                    //    }
                    //    else
                    //    {
                    //        guildDecoDics[i].transform.localPosition = TownMap.GetGuildTopPos(i, top);
                    //    }
                    //}
                    head.transform.localPosition = TownMap.GetHeadPosForGuild(top);
                }
                else
                {
                    head.transform.localPosition = TownMap.GetHeadPos(top);
                }
            }

            if (escalatorL == null)
            {
                GameObject escalatorPrefab = TownFactory.GetEscalatorLeft();
                if (escalatorPrefab != null)
                {
                    var target = Instantiate(escalatorPrefab, townBase.transform);
                    target.transform.localPosition = TownMap.GetCellPos(TownMap.X, TownMap.UndergroundY);
                    escalatorL = target.GetComponent<Escalator>();
                }
            }
            else
            {
                escalatorL.transform.localPosition = TownMap.GetCellPos(TownMap.X, TownMap.UndergroundY);
            }

            if (escalatorR == null)
            {
                GameObject escalatorPrefab = TownFactory.GetEscalatorRight();
                if (escalatorPrefab != null)
                {
                    var target = Instantiate(escalatorPrefab, townBase.transform);
                    target.transform.localPosition = TownMap.GetCellPos(TownMap.Width - 1, TownMap.UndergroundY);
                    escalatorR = target.GetComponent<Escalator>();
                }
            }
            else
            {
                escalatorR.transform.localPosition = TownMap.GetCellPos(TownMap.Width - 1, TownMap.UndergroundY);
            }
        }
        public Escalator GetEscalator(bool isRight)
        {
            return isRight ? escalatorR : escalatorL;
        }
#endregion
        #region LandMark 관련
        private void CheckTrain(BuildInfo data, int cell, int floor)
        {
            if (data != null)
            {
                if (data.State == eBuildingState.LOCKED || data.State == eBuildingState.NOT_BUILT)
                    SetUndergroundState(true);
            }

            if (TownMap.Height <= floor)
                return;

            if(train == null)
            {
                var trainPrefab = TownFactory.GetBuilding("subway");
                if (trainPrefab != null)
                {
                    var trainObj = Instantiate(trainPrefab, townBase.transform);
                    trainObj.transform.localPosition = new Vector2(0f, TownMap.GetCellPos(0, drawStart.y).y);
                    train = trainObj.GetComponent<TrainMove>();
                    if (train != null)
                    {
                        train.Init(cell, floor, data, (int)eLandmarkType.SUBWAY, "subway");
                        train.MaskResize();
                        buildingDics.Add((int)eLandmarkType.SUBWAY, train);
                    }
                }
            }
            else
            {
                train.Init(cell, floor);
                train.MaskResize();
            }

            if (train != null)
            {
                bool isTrainMove = data.State == eBuildingState.NORMAL;
                if (cellDics.ContainsKey(floor))
                {
                    for (int i = drawStart.x; i < drawCount.x; ++i)
                    {
                        if (cellDics[floor].ContainsKey(i))
                        {
                            if (cellDics[floor][i] == null)
                                continue;

                            var under_light = cellDics[floor][i].transform.Find("under_out_light");
                            if (under_light != null)
                                under_light.gameObject.SetActive(isTrainMove);
                        }
                    }
                }
            }
        }
        private void CheckDozer(BuildInfo data, int cell, int floor)
        {
            if (TownMap.Height <= floor)
                return;
            
            if (dozer == null)
            {
                var dozerPrefab = TownFactory.GetBuilding("dozer");
                if (dozerPrefab != null)
                {
                    var dozerObj = Instantiate(dozerPrefab, townBase.transform);
                    dozerObj.transform.localPosition = TownMap.GetBuildingPos(cell, floor);
                    dozer = dozerObj.GetComponent<DozerBuilding>();
                    if (dozer != null)
                    {
                        dozer.Init(cell, floor, data, (int)eLandmarkType.Dozer, "dozer");
                        buildingDics.Add((int)eLandmarkType.Dozer, dozer);
                    }
                }
            }
            else
            {
                dozer.Init(cell, floor);
                dozer.transform.localPosition = TownMap.GetBuildingPos(cell, floor);
            }
        }
        private void CheckTravel(BuildInfo data, int cell, int floor)
        {
            if (TownMap.Height <= floor)
                return;

            if (travel == null)
            {
                var travelPrefab = TownFactory.GetBuilding("travel");
                if (travelPrefab != null)
                {
                    var travelObj = Instantiate(travelPrefab, townBase.transform);
                    travelObj.transform.localPosition = TownMap.GetBuildingPos(cell, floor);
                    travel = travelObj.GetComponent<Travel>();
                    if (travel != null)
                    {
                        travel.Init(cell, floor, data, (int)eLandmarkType.Travel, "travel");
                        buildingDics.Add((int)eLandmarkType.Travel, travel);

                    }
                }
            }
            else
            {
                travel.Init(cell, floor);
                travel.transform.localPosition = TownMap.GetBuildingPos(cell, floor);
            }
        }
        private void CheckExchange(BuildInfo data, int cell, int floor)
        {
            if (TownMap.Height <= floor)
                return;

            if (exchangeCenter == null)
            {
                var exchangeCenterPrefab = TownFactory.GetBuilding("exchangeBuilding");
                if (exchangeCenterPrefab != null)
                {
                    var exchangeCenterObj = Instantiate(exchangeCenterPrefab, townBase.transform);
                    exchangeCenterObj.transform.localPosition = TownMap.GetBuildingPos(cell, floor);
                    exchangeCenter = exchangeCenterObj.GetComponent<ExchangeBuilding>();
                    if (exchangeCenter != null)
                    {
                        exchangeCenter.Init(cell, floor, data, (int)eLandmarkType.EXCHANGE, "exchangeBuilding");
                        buildingDics.Add((int)eLandmarkType.EXCHANGE, exchangeCenter);

                    }
                }
            }
            else
            {
                exchangeCenter.Init(cell, floor);
                exchangeCenter.transform.localPosition = TownMap.GetBuildingPos(cell, floor);
            }
        }
        private void CheckGemDungeon()
        {
            var gemdungeonData = LandmarkGemDungeon.Get();
            if (gemdungeonData == null)
                return;

            var gemdungeonBuilding = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.GEMDUNGEON);
            var Lockcolor = new Color(0.38f, 0.38f, 0.38f, 1.0f);
            if (Gemdungeon == null)
                Gemdungeon = new Dictionary<int, GemdungeonBuilding>();

            if (gemDungeonCellDics == null)
                gemDungeonCellDics = new();
            Dictionary<int, List<Animator>> AnimGemDungeonListDic = new Dictionary<int, List<Animator>>
            {
                { SBDefine.GemDungeonDefaultFloor, new List<Animator>() }
            };
            if (gemdungeonData.FloorDatas.Count == 0 && gemdungeonBuilding != null)
            {
                bool isLock = gemdungeonBuilding.State == eBuildingState.LOCKED;
                if (Gemdungeon.TryGetValue(SBDefine.GemDungeonDefaultFloor, out GemdungeonBuilding floorBuilding))
                {
                    if (floorBuilding == null)
                        return;

                    floorBuilding.Init(0, SBDefine.GemDungeonDefaultFloor, gemdungeonBuilding, (int)eLandmarkType.GEMDUNGEON, "Gemdungeon");
                    floorBuilding.transform.localPosition = new Vector2(0f, TownMap.GetCellPos(0, SBDefine.GemDungeonDefaultFloor).y);

                    floorBuilding.RefreshBuildingAction();
                    foreach (var cell in gemDungeonCellDics[SBDefine.GemDungeonDefaultFloor])
                    {
                        AnimGemDungeonListDic[SBDefine.GemDungeonDefaultFloor].Add(cell.Value.GetComponent<Animator>());
                    }
                }
                else
                {
                    var gemdungeonPrefab = TownFactory.GetBuilding("gemdungeon");
                    if (gemdungeonPrefab != null)
                    {
                        var gemdungeonBuildObj = Instantiate(gemdungeonPrefab, townBase.transform);
                        gemdungeonBuildObj.transform.localPosition = new Vector2(0f, TownMap.GetCellPos(0, SBDefine.GemDungeonDefaultFloor).y);
                        var gemdungeonBuild = gemdungeonBuildObj.GetComponent<GemdungeonBuilding>();
                        if (gemdungeonBuild != null)
                        {
                            gemdungeonBuild.Init(0, SBDefine.GemDungeonDefaultFloor, gemdungeonBuilding, (int)eLandmarkType.GEMDUNGEON, "Gemdungeon");
                            Gemdungeon.Add(SBDefine.GemDungeonDefaultFloor, gemdungeonBuild);
                        }
                    }
                    if (false == gemDungeonCellDics.ContainsKey(SBDefine.GemDungeonDefaultFloor))
                        gemDungeonCellDics.Add(SBDefine.GemDungeonDefaultFloor, new());

                    for (var cell = drawStart.x; cell < drawCount.x; ++cell)
                    {
                        if (false == gemDungeonCellDics[SBDefine.GemDungeonDefaultFloor].ContainsKey(cell))
                        {
                            GameObject targetCell = GetGemdungeonCell(SBDefine.GemDungeonDefaultFloor, cell);
                            if (targetCell != null)
                            {
                                var target = Instantiate(targetCell, townBase.transform);
                                lightingObjects.AddRange(target.GetComponentsInChildren<SpriteRenderer>());

                                target.GetComponent<Cell>()?.Init(cell, SBDefine.GemDungeonDefaultFloor);
                                target.transform.localPosition = TownMap.GetCellPos(cell, SBDefine.GemDungeonDefaultFloor);
                                gemDungeonCellDics[SBDefine.GemDungeonDefaultFloor].Add(cell, target);
                                AnimGemDungeonListDic[SBDefine.GemDungeonDefaultFloor].Add(target.GetComponent<Animator>());
                            }
                        }
                        else
                        {
                            gemDungeonCellDics[SBDefine.GemDungeonDefaultFloor][cell].transform.localPosition = TownMap.GetCellPos(cell, SBDefine.GemDungeonDefaultFloor);
                        }
                    }
                }

                foreach (var obj in gemDungeonCellDics[SBDefine.GemDungeonDefaultFloor].Values)
                {
                    foreach (var render in obj.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (render != null)
                        {
                            if (!render.name.Contains("out"))
                                render.color = isLock ? Lockcolor : Color.white;
                        }
                    }
                }
            }
            else
            {
                var floorIt = gemdungeonData.FloorDatas.GetEnumerator();
                while (floorIt.MoveNext())
                {
                    var floor = floorIt.Current.Key;
                    if (Gemdungeon.TryGetValue(floor, out GemdungeonBuilding floorBuilding))
                    {
                        if (floorBuilding == null)
                            return;

                        floorBuilding.Init(0, floor, User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.GEMDUNGEON), (int)eLandmarkType.GEMDUNGEON, "Gemdungeon");
                        floorBuilding.transform.localPosition = new Vector2(0f, TownMap.GetCellPos(0, floor).y);
                        floorBuilding.RefreshBuildingAction();
                        foreach(var cell in gemDungeonCellDics[floor])
                        {
                            AnimGemDungeonListDic[floor].Add(cell.Value.GetComponent<Animator>());
                        }
                       
                    }
                    else
                    {
                        var gemdungeonPrefab = TownFactory.GetBuilding("gemdungeon");
                        if (gemdungeonPrefab != null)
                        {
                            var gemdungeonBuildObj = Instantiate(gemdungeonPrefab, townBase.transform);
                            gemdungeonBuildObj.transform.localPosition = new Vector2(0f, TownMap.GetCellPos(0, floor).y);
                            var gemdungeonBuild = gemdungeonBuildObj.GetComponent<GemdungeonBuilding>();
                            if (gemdungeonBuild != null)
                            {
                                gemdungeonBuild.Init(0, floor, User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.GEMDUNGEON), (int)eLandmarkType.GEMDUNGEON, "Gemdungeon");
                                Gemdungeon.Add(floor, gemdungeonBuild);
                            }
                        }

                        if (false == gemDungeonCellDics.ContainsKey(floor))
                            gemDungeonCellDics.Add(floor, new());
                        if (gemDungeonCellDics.ContainsKey(floor) == false)
                        {
                            gemDungeonCellDics.Add(floor, new());
                        }

                        for (var cell = drawStart.x; cell < drawCount.x; ++cell)
                        {
                            if (false == gemDungeonCellDics[floor].ContainsKey(cell))
                            {
                                GameObject targetCell = GetGemdungeonCell(floor, cell);
                                if (targetCell != null)
                                {
                                    var target = Instantiate(targetCell, townBase.transform);
                                    target.GetComponent<Cell>()?.Init(cell, floor);
                                    target.transform.localPosition = TownMap.GetCellPos(cell, floor);
                                    gemDungeonCellDics[floor].Add(cell, target);
                                    AnimGemDungeonListDic[floor].Add(target.GetComponent<Animator>());
                                }
                            }
                            else
                            {
                                gemDungeonCellDics[floor][cell].transform.localPosition = TownMap.GetCellPos(cell, floor);
                            }
                        }
                    }
                }
            }
            
            foreach (var floor in Gemdungeon.Keys)
            {
                Gemdungeon[floor].SetGemBuildingAnimation(AnimGemDungeonListDic[floor]);
            }
            List<int> gemDungeonFloor = new List<int>(gemdungeonData.FloorDatas.Keys);
            foreach (var floor in gemDungeonFloor)
            {
                bool isLock = gemdungeonData.FloorDatas[floor].State == eGemDungeonState.LOCKED;
                foreach (var obj in gemDungeonCellDics[floor].Values)
                {
                    foreach (var render in obj.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (render != null)
                        {
                            if (!render.name.Contains("out"))
                                render.color = isLock ? Lockcolor: Color.white;
                        }
                    }
                }
            }
        }

        private void CheckGuildBuilding()
        {
            if (GuildManager.Instance.GuildBuildingShowAble == false)
            {
                return;
            }

            if (guildBuilding == null)
            {
                var guildPrefab = TownFactory.GetBuilding("guild_building");
                if (guildPrefab != null)
                {
                    var guildBuildObj = Instantiate(guildPrefab, townBase.transform);
                    guildBuildObj.transform.localPosition = TownMap.GetGuildCellPos(1, drawCount.y + 1);
                    var guildBuild = guildBuildObj.GetComponent<GuildBuilding>();
                    if (guildBuild != null)
                    {
                        guildBuilding = guildBuild;
                    }
                }
            }
            else
            {
                guildBuilding.transform.localPosition = TownMap.GetGuildCellPos(1, drawCount.y + 1);
            }
           
        }
        #endregion
        #region Camera 관련
        private void CheckCamera()
        {
            var size = TownMap.MapCameraSize();

            var camera = GameObject.FindGameObjectWithTag("MainCamera");
            if(camera != null)
            {
                var proCameraNumericBoundaries = camera.GetComponent<ProCamera2DNumericBoundaries>();
                if(proCameraNumericBoundaries != null && proCameraNumericBoundaries.UseNumericBoundaries)
                {
                    proCameraNumericBoundaries.TopBoundary = size.w;
                    proCameraNumericBoundaries.LeftBoundary = size.x;
                    proCameraNumericBoundaries.RightBoundary = size.z;
                    proCameraNumericBoundaries.BottomBoundary = size.y;
                }
            }
        }

        public void ZoomToTarget(Transform targetTransform, float zoomTime, float zoomAmount, Vector3 addPosition, VoidDelegate ZoomEndCallback = null, float callBackDelay = 0f)
        {
            if (cam == null) return;

            if (cam.enabled)
            {
                lastestCamPos = cam.CameraTargetPosition;
                lastestCamZoom = Camera.main.orthographicSize;
            }

            //cam.GetComponent<ProCamera2DPanAndZoomCustom>().enabled = false;
            //cam.GetComponent<ProCamera2DNumericBoundariesBounce>().enabled = false;
            cam.enabled = false;
            cam.HorizontalFollowSmoothness = 0f;
            cam.VerticalFollowSmoothness = 0f;
            
            var sizeVec = UICanvas.Instance.GetCanvasRectTransform().sizeDelta;
            
            float camSize = Camera.main.orthographicSize;
            Sequence sequence = DOTween.Sequence();
            Vector3 fixedPos = targetTransform.position + addPosition + Vector3.back * 10; //카메라 z 위치
            sequence.Append(DOTween.To(x => Camera.main.orthographicSize = x, camSize, (sizeVec.y / sizeVec.x) / zoomAmount, zoomTime).SetEase(Ease.Linear));
            sequence.Join(cam.transform.DOMove(fixedPos, zoomTime).SetEase(Ease.Linear));
            sequence.AppendInterval(callBackDelay);
            sequence.AppendCallback(() => {
                ZoomEndCallback?.Invoke();
            });
        }

        public void ZoomBackToTarget(Vector3 moveVec, float zoomTime, bool zoomEnd = true , VoidDelegate ZoomEndCallback = null, bool isUIShowEvent=true)
        {
            float camSize = Camera.main.orthographicSize;
            DOTween.To(x => Camera.main.orthographicSize = x, camSize, cam.StartScreenSizeInWorldCoordinates.y / 2, zoomTime).SetEase(Ease.Linear).Play();
            Tween tween = cam.transform.DOMove(moveVec + Vector3.back *10, zoomTime).SetEase(Ease.Linear).OnComplete(() => {
                ZoomEndCallback?.Invoke();
                if (zoomEnd) { 
                    ZoomEndProcess(isUIShowEvent);
                }
            });            
        }
        
        public void ZoomBackToLastestPos(float zoomTime, bool zoomEnd = true, VoidDelegate ZoomEndCallback = null, bool isUIShowEvent = true)
        {
            float camSize = Camera.main.orthographicSize;
            DOTween.To(x => Camera.main.orthographicSize = x, camSize, cam.StartScreenSizeInWorldCoordinates.y / 2, zoomTime).SetEase(Ease.Linear).Play();
            Tween tween = cam.transform.DOMove(lastestCamPos, zoomTime).SetEase(Ease.Linear).OnComplete(() => {
                ZoomEndCallback?.Invoke();
                if (zoomEnd)
                {
                    ZoomEndProcess(isUIShowEvent);
                }
            });
        }

        public void ZoomBackToLastestView(float zoomTime, bool zoomEnd = true, VoidDelegate ZoomEndCallback = null, bool isUIShowEvent = true)
        {
            ZoomBackToTarget(lastestCamPos, zoomTime, zoomEnd, ZoomEndCallback, isUIShowEvent);
        }
        public void ZoomBackToLastestView(float zoomTime)
        {
            float camSize = Camera.main.orthographicSize;
            DOTween.To(x => Camera.main.orthographicSize = x, camSize, lastestCamZoom, zoomTime).SetEase(Ease.Linear).Play();
            Tween tween = cam.transform.DOMove(lastestCamPos + Vector3.back * 10, zoomTime).SetEase(Ease.Linear).OnComplete(() => {
                ZoomEndProcess(false);
            });
        }


        public void CameraFollowObject(Transform transform, Vector2 addPos)
        {
            if (cam == null) return;
            cam.GetComponent<ProCamera2DNumericBoundariesBounce>().enabled = false; // 카메라 이동 제한 범위 때문에 끔
            cam.GetComponent<ProCamera2DPanAndZoomCustom>().enabled = true;
            cam.GetComponent<ProCamera2DPanAndZoomCustom>().AllowPan = false;
            cam.GetComponent<ProCamera2DPanAndZoomCustom>().AllowZoom = false;
            if (camZoomTr == null) { 
                camZoomTr = cam.CameraTargets[0].TargetTransform;
            }
            cam.CameraTargets[0].TargetTransform = transform;
            cam.CameraTargets[0].TargetOffset = addPos;
            Button zoomFinishBtn = townController.townZoomFinishLayer;
            
            zoomFinishBtn.transform.parent.SetAsLastSibling();
            zoomFinishBtn.gameObject.SetActive(true);
            zoomFinishBtn.onClick.RemoveAllListeners();
            zoomFinishBtn.onClick.AddListener(()=>CameraUnFollowObject());
        }

        public void CameraUnFollowObject()
        {
            if (cam == null) return;
            CurFollowDragonTag = 0;
            townController.townZoomFinishLayer.gameObject.SetActive(false);
            cam.GetComponent<ProCamera2DNumericBoundariesBounce>().enabled = true;
            cam.GetComponent<ProCamera2DPanAndZoomCustom>().AllowPan = true;
            cam.GetComponent<ProCamera2DPanAndZoomCustom>().AllowZoom = true;
            if (camZoomTr != null) { 
                cam.CameraTargets[0].TargetTransform = camZoomTr;
                cam.CameraTargets[0].TargetOffset = Vector2.zero;
                camZoomTr = null;
            }

            Camera.main.GetComponent<ProCamera2D>().ResetSize();
        }

        public void ZoomEndProcess(bool isUIShow = true)
        {
            if (cam == null) return;
            cam.HorizontalFollowSmoothness = 0.05f;
            cam.VerticalFollowSmoothness = 0.05f;
            cam.CameraTargets[0].TargetTransform.position = cam.transform.position;
            cam.enabled = true;
            if (isUIShow)
            {
                UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            }
        }

        public void ZoomBuilding(Transform buildingTr, VoidDelegate callback = null, bool isFixedLeftPos = true)
        {
            var sizeVec = UICanvas.Instance.GetCanvasRectTransform().sizeDelta;
            var zoomFixedSize = 1115.5f / sizeVec.x * .67f; // 557.75 :  건물 스파인 프레임의 너비
                                                            // 프레임 너비가 450일때 PosRatio x 값은 2.27f; // 계산식은 1020.5 를 프레임 너비로 나눈 값이 zoomPosRatio의 x 값
            var zoomPosRatio =  new Vector3(isFixedLeftPos ? 1.83f : 0, SBDefine.FloorSpancing * 0.54f);
            ZoomToTarget(buildingTr, 0.2f, zoomFixedSize, zoomPosRatio, callback);
        }

        public void ZoomSubway(Transform buildingTr, VoidDelegate callback = null, bool isFixedLeftPos = true)
        {
            var sizeVec = UICanvas.Instance.GetCanvasRectTransform().sizeDelta;
            var zoomFixedSize = 771.8f / sizeVec.x * .57f; // 557.75 :  건물 스파인 프레임의 너비
                                                            // 프레임 너비가 450일때 PosRatio x 값은 2.27f; // 계산식은 1020.5 를 프레임 너비로 나눈 값이 zoomPosRatio의 x 값
            var zoomPosRatio = new Vector3(isFixedLeftPos ? 1.83f : 0, SBDefine.FloorSpancing * 0.54f);
            ZoomToTarget(buildingTr, 0.2f, zoomFixedSize, zoomPosRatio, callback);
        }

        public void ZoomBuilding(int tag, VoidDelegate callback = null, bool isFixedLeftPos = true)
        {
            //CameraUnFollowObject();
            if (GetBuilding(tag) != null) { 
                if (tag == (int)eLandmarkType.SUBWAY)
                {
                    ZoomSubway(GetBuilding(tag).transform, callback, false);
                }
                else if (tag == (int)eLandmarkType.GEMDUNGEON)
                {
                    ZoomSubway(GetBuilding(tag).transform, callback, false);
                }
                else
                {
                    ZoomBuilding(GetBuilding(tag).transform, callback, isFixedLeftPos);
                }
            }
        }

        public void RefreshSubCamProjection()  // 타운 관리 팝업에서 타운의 전경을 비추는 팝업의 찍는 범위를 변경 // 지을 수 있는 최대 층수에 따라 설정
        {
            int curMaxFloor = AreaExpansionData.GetLimitFloorByAreaLv(User.Instance.ExteriorData.ExteriorLevel);
            float camSize = curMaxFloor * 0.75f + 9.5f;
            subCam.transform.position = Vector3.back*10 + Vector3.up * (curMaxFloor * 1f + 3.5f);
            subCam.orthographicSize = camSize;
        }

        public void SetSubCamState(bool state)
        {
            if(subCam != null)
                subCam.gameObject.SetActive(state);
        }

        public void SetCamPanMoving(Vector3 startPos,Vector3 destPos, float moveTime, bool isEndForcedPos=false, Vector3? endForcedPos = null, VoidDelegate endCallBack=null, float endDelay = 0.0f)
        {   
            cam.GetComponent<ProCamera2DNumericBoundariesBounce>().enabled = false;
            cam.GetComponent<ProCamera2DPanAndZoomCustom>().AllowPan = false;
            cam.GetComponent<ProCamera2DPanAndZoomCustom>().AllowZoom = false;
            Transform camPanTr = cam.CameraTargets[0].TargetTransform;
            camPanTr.localPosition = startPos;
            int curMaxFloor = AreaExpansionData.GetLimitFloorByAreaLv(User.Instance.ExteriorData.ExteriorLevel);
            float camSize = curMaxFloor * 0.75f + 5.0f;
            Camera.main.orthographicSize = camSize;

            Sequence seq = DOTween.Sequence();

            seq.Append(camPanTr.DOLocalMove(destPos, moveTime).SetEase(Ease.InSine));
            seq.AppendCallback(() => {
                if (isEndForcedPos)
                {
                    if (endForcedPos == null)
                    {
                        endForcedPos = Vector3.zero;
                    }
                    camPanTr.localPosition = (Vector3)endForcedPos;
                }
                cam.GetComponent<ProCamera2DNumericBoundariesBounce>().enabled = true;
                cam.GetComponent<ProCamera2DPanAndZoomCustom>().AllowPan = true;
                cam.GetComponent<ProCamera2DPanAndZoomCustom>().AllowZoom = true;
            });
            seq.AppendInterval(endDelay);

            seq.AppendCallback(endCallBack.Invoke);
        }

        public void SetCamZoomTargetPos(Vector3 pos, bool isAllowBounce, bool isAllowPan)
        {
            cam.GetComponent<ProCamera2DNumericBoundariesBounce>().enabled = isAllowBounce;
            cam.GetComponent<ProCamera2DPanAndZoomCustom>().AllowPan = isAllowPan;
            Transform camPanTr = cam.CameraTargets[0].TargetTransform;
            camPanTr.localPosition = pos;
        }
        #endregion
        #region 맵 크기 변동시 작업
        public void RefreshMap()
        {
            TownMap.SetMap(User.Instance.GetMapData());
            TownMap.SetOpenMap(User.Instance.GetOpenFloorData());

            if (openHeight != TownMap.OpenHeight || drawStart.x != TownMap.X || drawStart.y != TownMap.Y || drawCount.x != TownMap.Width || drawCount.y != (TownMap.Height - 1))
                Refresh();
            else
                CheckBuilding();
        }

        private void Refresh()
        {
            RefreshDics();

            CheckBG();
            CheckCell();
            CheckBuilding();
            CheckElevator();
            CheckDeco();
            CheckCamera();            
        }

        private void RefreshDics()
        {
            var height = TownMap.Height - 1;
            var curHeight = drawCount.y;
            var width = TownMap.Width - 1;
            var curWidth = drawCount.x - 1;
            if (height != curHeight)
            {
                var active = height > curHeight;
                if (cellDics.ContainsKey(curHeight))
                {
                    if(!cellDics.ContainsKey(height))
                    {
                        cellDics.Add(height, cellDics[curHeight]);
                        cellDics.Remove(curHeight);
                    }
                    else
                    {
                        var temp = cellDics[height];
                        cellDics.Remove(height);
                        cellDics.Add(height, cellDics[curHeight]);
                        cellDics.Remove(curHeight);
                        cellDics.Add(curHeight, temp);
                        var tempIt = temp.GetEnumerator();
                        while (tempIt.MoveNext())
                        {
                            tempIt.Current.Value.SetActive(active && tempIt.Current.Key <= width);
                        }
                    }
                }
                if (wallDics.ContainsKey(curHeight))
                {
                    if(!wallDics.ContainsKey(height))
                    {
                        wallDics.Add(height, wallDics[curHeight]);
                        wallDics.Remove(curHeight);
                    }
                    else
                    {
                        var temp = wallDics[height];
                        wallDics.Remove(height);
                        wallDics.Add(height, wallDics[curHeight]);
                        wallDics.Remove(curHeight);
                        wallDics.Add(curHeight, temp);
                        var tempIt = temp.GetEnumerator();
                        while (tempIt.MoveNext())
                        {
                            tempIt.Current.Value.SetActive(active && tempIt.Current.Key <= width);
                        }
                    }
                }
                var lockIt = lockedDics.GetEnumerator();
                while (lockIt.MoveNext())
                {
                    if (lockIt.Current.Value == null)
                        continue;
                    lockIt.Current.Value.Resize();
                }
                if (lockedDics.ContainsKey(curHeight))
                {
                    if (!lockedDics.ContainsKey(height))
                    {
                        lockedDics.Add(height, lockedDics[curHeight]);
                        lockedDics.Remove(curHeight);
                    }
                    else
                    {
                        var temp = lockedDics[height];
                        lockedDics.Remove(height);
                        lockedDics.Add(height, lockedDics[curHeight]);
                        lockedDics.Remove(curHeight);
                        lockedDics.Add(curHeight, temp);
                        temp.gameObject.SetActive(active);
                    }
                }
                if (elevatorDics.ContainsKey(curHeight))
                {
                    if(!elevatorDics.ContainsKey(height))
                    {
                        elevatorDics.Add(height, elevatorDics[curHeight]);
                        elevatorDics.Remove(curHeight);
                    }
                    else
                    {
                        var temp = elevatorDics[height];
                        elevatorDics.Remove(height);
                        elevatorDics.Add(height, elevatorDics[curHeight]);
                        elevatorDics.Remove(curHeight);
                        elevatorDics.Add(curHeight, temp);
                        var tempIt = temp.GetEnumerator();
                        while (tempIt.MoveNext())
                        {
                            tempIt.Current.Value.SetActive(active);
                        }
                    }
                }
            }

            if (width != curWidth)
            {
                var active = width > curWidth;
                var cellIt = cellDics.GetEnumerator();
                while (cellIt.MoveNext())
                {
                    var curDics = cellIt.Current.Value;
                    if (curDics.ContainsKey(curWidth))
                    {
                        if (!curDics.ContainsKey(width))
                        {
                            curDics.Add(width, curDics[curWidth]);
                            curDics.Remove(curWidth);
                        }
                        else
                        {
                            var temp = curDics[width];
                            curDics.Remove(width);
                            curDics.Add(width, curDics[curWidth]);
                            curDics.Remove(curWidth);
                            curDics.Add(curWidth, temp);
                            temp.SetActive(active && cellIt.Current.Key <= height);
                        }
                    }
                }
                var wallIt = wallDics.GetEnumerator();
                while (wallIt.MoveNext())
                {
                    var curDics = wallIt.Current.Value;
                    if (curDics.ContainsKey(curWidth))
                    {
                        if (!curDics.ContainsKey(width))
                        {
                            curDics.Add(width, curDics[curWidth]);
                            curDics.Remove(curWidth);
                        }
                        else
                        {
                            var temp = curDics[width];
                            curDics.Remove(width);
                            curDics.Add(width, curDics[curWidth]);
                            curDics.Remove(curWidth);
                            curDics.Add(curWidth, temp);
                            temp.SetActive(active && wallIt.Current.Key <= height);
                        }
                    }
                }
                var lockIt = lockedDics.GetEnumerator();
                while(lockIt.MoveNext())
                {
                    if (lockIt.Current.Value == null)
                        continue;
                    lockIt.Current.Value.Resize();
                }
                if (decoDics.ContainsKey(curWidth))
                {
                    if (!decoDics.ContainsKey(width))
                    {
                        decoDics.Add(width, decoDics[curWidth]);
                        decoDics.Remove(curWidth);
                    }
                    else
                    {
                        var temp = decoDics[width];
                        decoDics.Remove(width);
                        decoDics.Add(width, decoDics[curWidth]);
                        decoDics.Remove(curWidth);
                        decoDics.Add(curWidth, temp);
                        temp.SetActive(active);
                    }
                }
            }

            drawStart.x = TownMap.X;
            drawStart.y = TownMap.Y;
            drawCount.x = TownMap.Width;
            drawCount.y = TownMap.Height - 1;
            openHeight = TownMap.OpenHeight;
        }
#endregion
        #region 타운 Input 관련 작업

        private void ModeChangeBuildings()
        {
            if (IsBuildingEditModeState() || IsBuildingConstructModeState())
            {
                foreach (var building in buildingDics.Values)
                {
                    if (building != null)
                    {
                        building.ClearProduct();
                    }
                }
            }
            else
            {
                foreach (var building in buildingDics.Values)
                {
                    if (building != null)
                    {
                        building.CheckProductAlarm();
                    }
                }
            }
        }

        public void ClearBuildingProductEffect()
        {
            foreach (var building in buildingDics.Values)
            {
                if (building != null)
                {
                    building.BlurCompleteUI();
                }
            }
        }

        public void SetConstructModeState(bool state)
        {
            if (townController == null) { return; }

            townController.SetConstructModeState(state);

            ModeChangeBuildings();
        }

        public void SetBuildingEditModeState(bool state)
        {
            if (townController == null) { return; }

            townController.SetBuildingEditModeState(state);

            ModeChangeBuildings();
        }

        public bool IsBuildingEditModeState()
        {
            if (townController == null) { return false; }

            return townController.IsBuildingEditModeState();
        }

        public bool IsBuildingConstructModeState()
        {
            if (townController == null) { return false; }

            return townController.IsConstructModeState();
        }

        public GameObject GetCellData(int floor, int cell)
        {
            GameObject resultCell = null;

            if (cellDics.ContainsKey(floor))
            {
                resultCell = cellDics[floor].GetValueOrDefault(cell, null);
            }

            return resultCell;    
        }

    #endregion
        #region 테스트
    #if DEBUG && HYEON_TEST
            void Update()
            {
                //if (Input.GetMouseButtonDown(0))
                //{
                //    hit_position = Input.mousePosition;
                //    camera_position = transform.position;

                //}
                //if (Input.GetMouseButton(0))
                //{
                //    current_position = Input.mousePosition;
                //    LeftMouseDrag();
                //}

                if(Input.GetKeyDown(KeyCode.D))
                {
                    TownMap.Dummy.x += 1;
                    RefreshMap();
                }

                if (Input.GetKeyDown(KeyCode.W))
                {
                    TownMap.Dummy.y += 1;
                    RefreshMap();
                }

                if (Input.GetKeyDown(KeyCode.A))
                {
                    if (TownMap.Dummy.x > 0)
                    {
                        TownMap.Dummy.x -= 1;
                        RefreshMap();
                    }
                }

                if (Input.GetKeyDown(KeyCode.S))
                {
                    if (TownMap.Dummy.y > 0)
                    {
                        TownMap.Dummy.y -= 1;
                        RefreshMap();
                    }
                }
            }
    #endif
    #endregion
        public void SetUndergroundState(bool bLocked)
        {
            Color color = Color.white;
            if (bLocked)
                color = new Color(0.38f, 0.38f, 0.38f, 1.0f);

            if(cellDics.ContainsKey(-1))
            {
                foreach(var obj in cellDics[-1].Values)
                {
                    foreach (var render in obj.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (render != null)
                        {
                            if(!render.name.Contains("out"))
                                render.color = color;
                        }
                    }
                }
            }

            if(escalatorL != null)
            {
                foreach (var render in escalatorL.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (render != null)
                    {
                        render.color = color;
                    }
                }
            }

            if (escalatorR != null)
            {
                foreach (var render in escalatorR.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (render != null)
                    {
                        render.color = color;
                    }
                }
            }
        }

        public void OnFoorTouchAction(int floor)
        {
            if (cellDics.ContainsKey(floor))
            {
                foreach (var obj in cellDics[floor].Values)
                {
                    foreach (var render in obj.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (render.sortingOrder >= 5)
                            continue;

                        if (DOTween.IsTweening(render))
                            continue;

                        Color origin = render.color;
                        render.color = Color.gray;                        
                        render.DOColor(origin, 1.0f);
                    }
                }
            }
        }


        // 튜토리얼 용
        public void SetPreconstruct(int buildingTag, int x, int y)
        {
            townController.CreatePreConstruction(buildingTag, x, y);
        }

        public void SetIntroModeState (bool state)
        {
            IsIntroMode = state;
        }
    }
}