
import { _decorator, Component, Node, SystemEvent, systemEvent, EventMouse, Vec3, Camera, math, EventTouch, Touch, Tween, tween, game, Label, view, director, ProgressBar, screen, Vec4, input, Input, Canvas, Button, instantiate, EventHandler, Sprite, Color, Prefab, Layout, renderer, ResolutionPolicy } from 'cc';
import { AccountTable } from '../Data/AccountTable';
import { QuestTriggerTable } from '../Data/QuestTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { CellSize, CellSizeBothX } from '../Map/MapManager';
import { NetworkManager } from '../NetworkManager';
import { QuestManager, QuestObject } from '../QuestManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { SceneManager } from '../SceneManager';
import { SoundMixer, SOUND_TYPE } from '../SoundMixer';
import { TimeObject } from '../Time/ITimeObject';
import { TimeManager } from '../Time/TimeManager';
import { EventManager } from '../Tools/EventManager';
import { GetChild, GetType, StringBuilder, TimeStringMinute, Type } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { User } from '../User/User';
import { ePopupEventType, PopupEvent } from './Common/PopupEvent';
import { PopupManager } from './Common/PopupManager';
import { SystemPopup } from './SystemPopup';
import { ToastMessage } from './ToastMessage';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = WindowResolution
 * DateTime = Tue Dec 28 2021 15:01:14 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = WindowResolution.ts
 * FileBasenameNoExtension = WindowResolution
 * URL = db://assets/Scripts/WindowResolution.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

 const DesignWindowX : number = 1280;
 const DesignWindowY : number = 720;

 const  CamOrthoHeight_Default : number = 360;
 const  CamOrthoHeight_Min : number = 360;
 const  CamOrthoHeight_Max : number = 960;
 const  CamFov_Min : number = 39.5;
 const  CamFov_Max : number = 80;


@ccclass('UICanvas')
export class UICanvas extends Component 
{
    private static instance : UICanvas = null

    @property(Node)
    OpenBtn : Node;

    @property(Node)
    SideSet : Node;

    @property(Node)
    PopupTransform : Node;

    @property(Node)
    UIRootTransform : Node;

    @property({range: Node[50], type:Node})
    scaleNodes : Node[] = [];

    @property(Camera)
    MainCam : Camera = null;

    public static mouseMove : boolean = false;
    public static globalMainCamRef : Camera = null; 
    public static camTween : Tween<Node> = null;
    private needSize : number = 0;
    private goldText : Label = null;
    public get GoldText() { return this.goldText; }
    private energyCurText : Label = null;
    private energyMaxText : Label = null;
    private energyExpText : Label = null;
    private levelText : Label = null;
    private nickText : Label = null;
    private expText : Label = null;
    private expBar : ProgressBar = null;
    private energyTimeObj : TimeObject = null;
    private static moveSchedule: Function = null;
    private static orthoTemp: number = 0;
    private tempPos: Vec3 = null;
    private tempRemain: Vec3 = null;

    onLoad() 
    {
        if(UICanvas.instance == null)
        {
            UICanvas.instance = this
        }

        view.resizeWithBrowserSize(true)
        
        if(this.MainCam != null)
        {
            UICanvas.globalMainCamRef = this.MainCam;
        }

        // systemEvent.on(SystemEvent.EventType.MOUSE_DOWN, this.onMouseDown)
        // systemEvent.on(SystemEvent.EventType.MOUSE_UP, this.onMouseUp)
        // systemEvent.on(SystemEvent.EventType.MOUSE_MOVE, this.onMouseMove)
        input.on(Input.EventType.MOUSE_WHEEL, this.onMouseWheel);
        input.on(Input.EventType.TOUCH_START, this.onTouchStart);
        input.on(Input.EventType.TOUCH_MOVE, this.onTouchMove);
        input.on(Input.EventType.TOUCH_END, this.onTouchEnd);
        input.on(Input.EventType.TOUCH_CANCEL, this.onTouchEnd);

        let goldNode = GetChild(this.UIRootTransform, ['LT','Profile-cash', 'Layout', 'coin', 'CoinCurAmount']);
        if(goldNode != null) {
            this.goldText = goldNode.getComponent(Label);
        }

        let energyCurNode = GetChild(this.UIRootTransform, ['LT','Profile-cash', 'Layout', 'battery', 'battery_node', 'strings', 'BatteryCurAmount']);
        if(energyCurNode != null) {
            this.energyCurText = energyCurNode.getComponent(Label);
        }

        let energyMaxNode = GetChild(this.UIRootTransform, ['LT','Profile-cash', 'Layout', 'battery', 'battery_node', 'strings', 'BatteryMaxAmount']);
        if(energyMaxNode != null) {
            this.energyMaxText = energyMaxNode.getComponent(Label);
        }

        let energyExpNode = GetChild(this.UIRootTransform, ['LT','Profile-cash', 'Layout', 'battery', 'battery_bg', 'BatteryExp']);
        if(energyExpNode != null) {
            this.energyExpText = energyExpNode.getComponent(Label);
        }

        if(energyExpNode != null) {
            this.energyTimeObj = energyExpNode.getComponent(TimeObject);
        }

        let levelTextNode = GetChild(this.UIRootTransform, ['RT','Profile-info', 'Layout', 'Layout', 'LabelNickname-001']);
        if(levelTextNode != null) {
            this.levelText = levelTextNode.getComponent(Label);
        }

        let nickTextNode = GetChild(this.UIRootTransform, ['RT','Profile-info', 'Layout', 'Layout', 'LabelNickname']);
        if(nickTextNode != null) {
            this.nickText = nickTextNode.getComponent(Label);
        }

        let expTextNode = GetChild(this.UIRootTransform, ['RT','Profile-info', 'Layout', 'ExpBar','ExpLayout', 'LabelExp']);
        if(expTextNode != null) {
            this.expText = expTextNode.getComponent(Label);
        }

        let expBarNode = GetChild(this.UIRootTransform, ['RT','Profile-info', 'Layout', 'ExpBar']);
        if(expBarNode != null) {
            this.expBar = expBarNode.getComponent(ProgressBar);
        }

        // var xmlHttp = new XMLHttpRequest();

        // xmlHttp.onreadystatechange = function() 
        // {
        //     if(this.readyState == this.DONE) 
        //     {
        //         console.log(JSON.parse(this.response))
        //     }
        // };
        // xmlHttp.open("POST", "http://192.168.1.25/api/data", true)
        // xmlHttp.send();
        UICanvas.CalcualteCamMinMaxPos(0, 0);
        this.RefershUI();
        NetworkManager.Instance.UIUpdater = this.RefershUI;
    }
    start() {
        this.node.on("size-changed", this.CameraPos);

        UICanvas.DefaultCamera();
        UICanvas.instance.OrthoRefresh();
    }

    private CameraPos() {
        if(UICanvas.instance.tempPos != null && UICanvas.instance.tempRemain != null) {
            if(UICanvas.camTween!=null)
            {
                UICanvas.camTween.stop();
            }
            const defaultY = CamOrthoHeight_Default * 2;
            const defaultScope = defaultY / 9;
            const defalutX = defaultScope * 16;
            let screenSize = screen.windowSize;
            const rsY = defaultY / screenSize.height;
            const rsX = defalutX / screenSize.width;
            UICanvas.globalMainCamRef.node.worldPosition = new Vec3(UICanvas.instance.tempPos.x - UICanvas.instance.tempRemain.x * 0.5 * rsY / rsX, UICanvas.instance.tempPos.y - UICanvas.instance.tempRemain.y * 0.5, UICanvas.instance.tempPos.z);
            UICanvas.instance.tempPos = null;
            UICanvas.instance.tempRemain = null;
            UICanvas.CamMovement(true);
        }
    }

    onEnable() {
        GameManager.Instance.resizeFunc = UICanvas.ResizeFunc;
    }

    onDisable() {
        GameManager.Instance.resizeFunc = null;
    }

    private static RefershCoin(): void {
        if (UICanvas.instance.GoldText == null) {
            return;
        }
        UICanvas.instance.GoldText.string = String(User.Instance.GOLD);
    }

    private static RefershEnergy(): void {
        if (UICanvas.instance.energyCurText == null) {
            return;
        }

        let energyMax = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).MAX_STAMINA
        let exp = User.Instance.GetNextEnergyExpire().exp

        UICanvas.instance.energyCurText.string = `${User.Instance.UserData.Energy}`;
        UICanvas.instance.energyMaxText.string = `${energyMax}`;
        if(exp > 0)
        {
            if(UICanvas.instance.energyTimeObj.Refresh == null)
            {
                // 탭 정지 상태에서 돌아올 때 오차를 없애기 위해 만료시각 기반으로 계산
                UICanvas.instance.energyTimeObj.curTime = exp
                UICanvas.instance.energyTimeObj.Refresh = () => 
                {
                    let remain = UICanvas.instance.energyTimeObj.curTime - TimeManager.GetTime()
                    
                    if(0 >= remain)
                    {
                        // 업데이터 해제
                        UICanvas.instance.energyTimeObj.Refresh = null
                        UICanvas.RefershEnergy()
                    } else {
                        // String.format("%02d:%02d", min, sec)
                        UICanvas.instance.energyExpText.string = TimeStringMinute(Math.floor(remain))
                    }
                }
            }
        }
    }

    private static RefershNick(): void {
        if (UICanvas.instance.nickText == null) {
            return;
        }
        UICanvas.instance.nickText.string = User.Instance.UserData.UserID;
    }

    private static RefershExp(): void {
        if (UICanvas.instance.expText == null) {
            return;
        }

        let accountLevelData = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).TOTAL_EXP
        let devider = TableManager.GetTable<AccountTable>(AccountTable.Name).Get(User.Instance.UserData.Level).EXP

        UICanvas.instance.expText.string = String(User.Instance.UserData.Exp - accountLevelData)
        UICanvas.instance.expBar.progress = Math.fround((User.Instance.UserData.Exp - accountLevelData) / (devider))
        //console.log(User.Instance.UserData.Exp - accountLevelData, "/", devider - accountLevelData," : ", (User.Instance.UserData.Exp - accountLevelData) / (devider - accountLevelData))
    }

    private static RefershLevel(): void {
        if (UICanvas.instance.levelText == null) {
            return;
        }
        UICanvas.instance.levelText.string = `Lv. ${User.Instance.UserData.Level}`;
    }

    private RefershUI() : void {
        if (UICanvas.instance == null) {
            return;
        }
        UICanvas.RefershCoin();
        UICanvas.RefershEnergy();
        UICanvas.RefershNick();
        UICanvas.RefershExp();
        UICanvas.RefershLevel();
    }

    static set ActiveUI(value : boolean)
    {
        UICanvas.instance.PopupTransform.active = value
        UICanvas.instance.UIRootTransform.active = value
    }

    onTouchStart(event : EventTouch)
    {
        if(event.target != null)
        return;

        UICanvas.mouseMove = true;
        if(UICanvas.camTween!=null)
        {
            UICanvas.camTween.stop();
        }
    }

    onTouchMove(event : EventTouch)
    {
        if(!UICanvas.mouseMove || event.target != null)
        return;

        UICanvas.CalcualteCamMinMaxPos(event.getDelta().x, event.getDelta().y)
    }

    onTouchEnd(event : EventTouch)
    {
        if(event.target != null)
        return;

        UICanvas.mouseMove = false;
        UICanvas.CamMovement();
    }

    onDestroy()
    {
        this.node.off("size-changed", this.CameraPos);

        NetworkManager.Instance.UIUpdater = null;
        // systemEvent.off(SystemEvent.EventType.MOUSE_DOWN, this.onMouseDown)
        // systemEvent.off(SystemEvent.EventType.MOUSE_UP, this.onMouseUp)
        // systemEvent.off(SystemEvent.EventType.MOUSE_MOVE, this.onMouseMove)
        input.off(Input.EventType.MOUSE_WHEEL, this.onMouseWheel);
        input.off(Input.EventType.TOUCH_START, this.onTouchStart);
        input.off(Input.EventType.TOUCH_MOVE, this.onTouchMove);
        input.off(Input.EventType.TOUCH_END, this.onTouchEnd);
        input.off(Input.EventType.TOUCH_CANCEL, this.onTouchEnd);

        if(UICanvas.instance == this)
        {
            UICanvas.instance = null;
        }
    }

    onMouseDown(event : EventMouse)
    {
        if(event.target != null)
            return;

        switch(event.getButton())
        {
            case EventMouse.BUTTON_LEFT:
                UICanvas.mouseMove = true;
                if(UICanvas.camTween!=null)
                {
                    UICanvas.camTween.stop();
                }
            break;
        }
    }

    onMouseUp(event : EventMouse)
    {
        switch(event.getButton())
        {
            case EventMouse.BUTTON_LEFT:
                UICanvas.mouseMove = false;
                UICanvas.CamMovement();
            break;
        }
    }

    onMouseMove(event : EventMouse)
    {
        if(UICanvas.mouseMove == false || UICanvas.globalMainCamRef == null || event.getButton() != EventMouse.BUTTON_LEFT)
        {
            return;
        }

        UICanvas.CalcualteCamMinMaxPos(event.getDelta().x, event.getDelta().y)
    }

    onMouseWheel(event : EventMouse)
    {
        if(UICanvas.globalMainCamRef == null)
        {
            return;
        }

        if(UICanvas.globalMainCamRef.projection == renderer.scene.CameraProjection.ORTHO) {
            if((UICanvas.globalMainCamRef.orthoHeight == CamOrthoHeight_Min && event.getScrollY() > 0) 
            || (UICanvas.globalMainCamRef.orthoHeight == CamOrthoHeight_Max && event.getScrollY() < 0)) {
                return;
            }

            var ortho = UICanvas.globalMainCamRef.orthoHeight;
            UICanvas.orthoTemp -= event.getScrollY() * 0.2;
            if(UICanvas.orthoTemp >= 1) {
                const tempData = Math.floor(UICanvas.orthoTemp);
                ortho += tempData;
                UICanvas.orthoTemp -= tempData;
            } else if(UICanvas.orthoTemp <= -1) {
                const tempData = Math.ceil(UICanvas.orthoTemp);
                ortho += tempData;
                UICanvas.orthoTemp -= tempData;
            }

            let changeOrtho = math.clamp(ortho, CamOrthoHeight_Min, CamOrthoHeight_Max);
            let isChange = UICanvas.globalMainCamRef.orthoHeight != changeOrtho;
            if(isChange) {
                UICanvas.globalMainCamRef.orthoHeight = changeOrtho;
                UICanvas.instance.OrthoRefresh();

                // if(UICanvas.moveSchedule != null) {
                //     UICanvas.instance.unschedule(UICanvas.moveSchedule);
                //     UICanvas.moveSchedule = null;
                // }
        
                // UICanvas.moveSchedule = UICanvas.instance.CameraMove;
                // UICanvas.instance.scheduleOnce(UICanvas.moveSchedule, 0.3);
            }
        }
        // UICanvas.globalMainCamRef.orthoHeight = UICanvas.globalMainCamRef.orthoHeight - (event.getScrollY() * 0.02);
        // UICanvas.globalMainCamRef.orthoHeight = 800;
        //OrthoHeight에 따른 카메라 이동 최대치 수정
    }

    private CameraMove() {
        if(UICanvas.camTween!=null)
        {
            UICanvas.camTween.stop();
        }
        UICanvas.CamMovement();
        UICanvas.instance.OrthoRefresh();
    }

    OrthoRefresh() {           
        //임시처리 - game Scene에서 카메라 축소/확대 상태가 다른 씬UI 에도 전이되는 버그
        let checkSceneName = SceneManager.Instance.GetSceneTargetName();
        if(checkSceneName != "game"){return;}
        UICanvas.instance.tempPos = new Vec3(UICanvas.globalMainCamRef.node.worldPosition);
        
        const orthoY = UICanvas.globalMainCamRef.orthoHeight * 2; 
        const orthoScope = orthoY / 9;
        const orthoX = orthoScope * 16;

        const curSize = view.getDesignResolutionSize();
        UICanvas.instance.tempRemain = new Vec3(curSize.width - orthoX, curSize.height - orthoY, 0)
        view.setDesignResolutionSize(orthoX, orthoY, ResolutionPolicy.FIXED_HEIGHT);
        UICanvas.instance.scaleNodes.forEach(element => {
            if(element == null) {
                return;
            }
            element.setScale(orthoX / 1280, orthoY / 720);  
        });
        if(PopupManager.GetInstance != null && PopupManager.GetInstance.CurBeacon != null) {
            PopupManager.GetInstance.CurBeacon.node.setScale(orthoX / 1280, orthoY / 720); 
        }
    }
    
    public static GetMinMax(): Vec4 {
        const mapSize = User.Instance.GetMapData();

        if(mapSize.x == 0 && mapSize.y == 0 && mapSize.z == 0 && mapSize.w == 0) {
            return new Vec4(0,0,0,0);
        }

        const orthoY = UICanvas.globalMainCamRef.orthoHeight * 2;
        const orthoScope = orthoY / 9;
        const orthoX = orthoScope * 16;
        const defaultY = CamOrthoHeight_Default * 2;
        const defaultScope = defaultY / 9;
        const defaultX = defaultScope * 16;
        // const orthoDiffX = orthoX / defaultX;
        // const orthoDiffY = orthoY / defaultY;
        const orthoRemainX = orthoX - defaultX;
        const orthoRemainY = orthoY - defaultY;
        
        const windowX = screen.resolution.width;
        const windowY = screen.resolution.height;
        // const remainX = (defaultX * 2 - windowX);
        const remainX = windowX - defaultX;
        // const remainY = windowY - defaultY;
        const scopeX = defaultX / windowX;
        const scopeY = defaultY / windowY;

        // console.log('orthoX', orthoX);
        // console.log('orthoY', orthoY);
        // console.log('windowX', windowX);
        // console.log('windowY', windowY);
        // console.log('scopeX', scopeX);
        // console.log('scopeY', scopeY);
        // console.log('remainX', remainX);
        // console.log('remainY', remainY);

        // const MinSizeX = mapSize.x * CellSize.width * orthoDiffX + orthoX - (remainX - orthoY) * scopeY * scopeX;//(180 - remainX) * scopeY;
        // const MinSizeY = mapSize.y * CellSize.height * orthoDiffY + orthoY - remainY / scopeY;// + (-80 - remainY) / scopeY;
        // const MinSizeX = (mapSize.x * CellSize.width / orthoDiffX / scopeY - (remainX + orthoX) * scopeY / orthoDiffX);//(180 - remainX) * scopeY;
        // const MinSizeY = (mapSize.y * CellSize.height+ (orthoY + remainY) / scopeY) + 160;// + (-80 - remainY) / scopeY;
                
        // const MapSizeX = (mapSize.z * CellSize.width / orthoDiffX / scopeY + (remainX + orthoX) * scopeY / orthoDiffX);
        // const MapSizeY = (mapSize.w * CellSize.height + (orthoY + remainY) / scopeY) + 160;// + (-80 - remainY) / scopeY;
        // const MapSizeX = mapSize.z * CellSize.width / orthoDiffX + orthoX - (remainX - orthoY) * scopeY * scopeX;
        // const MapSizeY = mapSize.w * CellSize.height / orthoDiffY + orthoY - remainY / scopeY;// + (-80 - remainY) / scopeY;

        // const MinSizeX = ((mapSize.x * CellSize.width + (defaultX + 140) * scopeY + 40 / scopeY)) - (remainX) * scopeY;//(180 - remainX) * scopeY;
        // const MinSizeY = (mapSize.y * CellSize.height + (CamOrthoHeight_Default + 80) * scopeY) - remainY * scopeY;// + (-80 - remainY) / scopeY;
        // const MapSizeX = (((mapSize.z - 1) * CellSize.width + 2 * CellSizeBothX + (defaultX + 140) * scopeY + 40 / scopeY)) + (remainX) * scopeY;
        // const MapSizeY = (mapSize.w * CellSize.height + (CamOrthoHeight_Default + 80) * scopeY) - remainY * scopeY * 0.575 + 500 / scopeY;// + (-80 - remainY) / scopeY;

        const SizeX = ((mapSize.z - 2) * CellSize.width + 2 * CellSizeBothX) * 0.5;

        // const MinSizeX = -SizeX + orthoX * 0.5 * scopeY + (orthoX - remainX * orthoDiffX / scopeX + orthoRemainX) * scopeY;
        const MinSizeX = -SizeX + (defaultX * 0.5 + remainX) * scopeY + orthoRemainX * scopeY / scopeX;
        const MinSizeY = mapSize.y * CellSize.height + defaultY * 0.5 + 180 + orthoRemainY;
        
        // const MapSizeX = SizeX + orthoX * 0.5 * scopeY + (orthoX + remainX * orthoDiffX / scopeX - orthoRemainX) * scopeY;
        const MapSizeX = SizeX + defaultX * 0.5 * scopeY;
        const MapSizeY = (mapSize.w + 1) * CellSize.height + defaultY * 0.5 + 620;

        return new Vec4(MinSizeX - 180 / scopeY, MinSizeY - 90 / scopeY, MapSizeX + 180 / scopeY, MapSizeY + 90 / scopeY);
    }

    public static CalcualteCamMinMaxPos(deltaX, deltaY)
    {
        const orthoY = UICanvas.globalMainCamRef.orthoHeight;

        const windowY = screen.resolution.height;
        const scopeY = orthoY * 2 / windowY;

        let camNode: Node = UICanvas.globalMainCamRef.node;
        const tempPosX = camNode.position.x - deltaX * scopeY;// / orthoDiffX;
        const tempPosY = camNode.position.y - deltaY * scopeY;// / orthoDiffY;

        const size = UICanvas.GetMinMax();
        // console.log(size);

        if(size.z - 180 / scopeY < size.x + 180 / scopeY) {
            var x = (size.x - size.z) * 0.5;
            size.z = size.z + x;
            size.x = size.z;
        }

        const mousePosX = math.clamp(tempPosX, size.x, size.z);
        const mousePosY = math.clamp(tempPosY, size.y, size.w);
        // const mousePosX = tempPosX;
        // const mousePosY = tempPosY;
        // console.log(size.x, size.y, size.z, size.w);

        camNode.position = new Vec3(mousePosX, mousePosY, 1000);
        // console.log(camNode.position);
        // camNode.position = new Vec3(tempPosX, tempPosY, 1000);
        // console.log(mousePosX, mousePosY);
    }

    public static CamMovement(isNow: boolean = false)
    {
        const size = UICanvas.GetMinMax();

        const orthoY = UICanvas.globalMainCamRef.orthoHeight;

        const windowY = screen.resolution.height;
        const scopeY = orthoY * 2 / windowY;

        if(size.z - 180 / scopeY < size.x + 180 / scopeY) {
            var x = (size.x - size.z) * 0.5;
            size.z = size.z + x;
            size.x = size.z;
            size.y = size.y + 90 / scopeY;
            size.w = size.w - 90 / scopeY;
        } else {
            size.x = size.x + 180 / scopeY;
            size.z = size.z - 180 / scopeY;
            size.y = size.y + 90 / scopeY;
            size.w = size.w - 90 / scopeY;
        }

        const mousePosX = math.clamp(UICanvas.globalMainCamRef.node.position.x, size.x, size.z);
        const mousePosY = math.clamp(UICanvas.globalMainCamRef.node.position.y, size.y, size.w);
        // console.log(UICanvas.globalMainCamRef.node.position);
        // console.log(screen.resolution.height);

        if (isNow) {
            UICanvas.globalMainCamRef.node.position = new Vec3(mousePosX, mousePosY, 1000);
            return;
        }

        UICanvas.camTween = tween(UICanvas.globalMainCamRef.node).to(1, {position : new Vec3(mousePosX, mousePosY, 1000)}, 
            {easing: 'cubicOut', onComplete : ()=>{UICanvas.camTween = null}});
        UICanvas.camTween.start();
    }

    public static ResizeFunc()
    {
        if(UICanvas.globalMainCamRef == null)
        {
            return;
        }

        const check = math.clamp(UICanvas.globalMainCamRef.orthoHeight, CamOrthoHeight_Min, CamOrthoHeight_Max);
        if(check != UICanvas.globalMainCamRef.orthoHeight) {
            UICanvas.globalMainCamRef.orthoHeight = check;
        }
        UICanvas.instance.OrthoRefresh();
        UICanvas.CamMovement(true);
    }

    public static DefaultCamera() {
        const size = UICanvas.GetMinMax();
        var x = (size.x - size.z) * 0.5;
        const posX = size.z + x;
        const posY = 400;
        UICanvas.globalMainCamRef.node.position = new Vec3(posX, posY, 1000);
    }

    // 일반 popup 형식 오픈
    onOpenNormalPopup(event, CustomEventData)
    {
        var customED = JSON.parse(CustomEventData);
        if(customED.tapIndex != null)
            this.openPopupByName(customED.target, customED.tapIndex);
        else
            this.openNoramlPopupByName(customED.target);
    }

    private openNoramlPopupByName(popupName : string, callback? : () => void)
    {
        let popup = PopupManager.OpenPopup(popupName);
        if(popup != null)
        {
            popup.Init();
        }

        // 사이드 버튼 원위치
        UICanvas.RefreshSideButton();
    }

    onOpenPopupBtn(event, CustomEventData)
    {
        var customED = JSON.parse(CustomEventData);
        if(customED.tapIndex != null)
            this.openPopupByName(customED.target, customED.tapIndex);
        else
            this.openPopupByName(customED.target);
    }

    private openPopupByName(popupName : string, tapIndex : number = 0, callback? : () => void)
    {
        PopupManager.OpenPopup(popupName, true, {index: tapIndex});

        // 사이드 버튼 원위치
        UICanvas.RefreshSideButton();
    }

    SystemPopupOK(event, CustomEventData)
    {
        var customED = JSON.parse(CustomEventData);
        
        SystemPopup.PopupOK(customED.title, customED.body);
    }

    SystemPopupOKCANCEL(event, CustomEventData)
    {
        var customED = JSON.parse(CustomEventData);
        
        let systempopup = SystemPopup.PopupOK_CANCEL(customED.title, customED.body, 
        () =>
        {
            
        }, 

        () =>
        {
            PopupManager.ClosePopup(systempopup);
        });
    }

    onToastMessage(event, CustomEventData)
    {
        //PopupManager.OpenPopup("AccountLevelUp")
    }

    onClickSideArrowBtn()
    {
        this.OpenBtn.active = !this.OpenBtn.active;
        this.SideSet.active = !this.SideSet.active;
    }

    OnClickMoveToGachaPageButton() {
        SoundMixer.DestroyAllClip();
        SceneManager.SceneChange("gacha", () => { 
            SoundMixer.PlayBGM(SOUND_TYPE.BGM_VILLAGE);

            // 튜토리얼 실행
            if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(105)) {
                TutorialManager.GetInstance.OnTutorialEvent(105,3);
            }
        });
    }
    /**
     * //업데이트 기대해달라는 문구
     */
     onClickExpectGameAlphaUpdate()
     {
         let emptyCheck = ToastMessage.isToastEmpty();
         if(emptyCheck == false){
             return;
         }
         
         let textData = TableManager.GetTable<StringTable>(StringTable.Name).Get(100000326);
         if(textData != null)
         {
             ToastMessage.Set(textData.TEXT);
         }
     }

    public static RefreshSideButton() {
        if(UICanvas.instance != null){
            UICanvas.instance.CheckSidBtnObj();    
        }
    }

    CheckSidBtnObj()
    {
        if(this.OpenBtn != null){
            this.OpenBtn.active = true;
        }
        if(this.SideSet != null){
            this.SideSet.active = false;
        }
    }
}