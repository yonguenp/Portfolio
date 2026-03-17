
import { _decorator, Node, Button, instantiate, Label, EventHandler, UITransform, math, Vec3, Animation, UIOpacity, EventMouse, Sprite } from 'cc';
import { BuildingBaseData, BuildingLevelData } from '../Data/BuildingData';
import { BuildingBaseTable, BuildingLevelTable } from '../Data/BuildingTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { GameManager } from '../GameManager';
import { CellSize, MapManager } from '../Map/MapManager';
import { NetworkManager } from '../NetworkManager';
import { ResourceManager, ResourcesType } from '../ResourceManager';
import { SoundMixer, SOUND_TYPE } from '../SoundMixer';
import { DataManager } from '../Tools/DataManager';
import { TimeString } from '../Tools/SandboxTools';
import { TutorialManager } from '../Tutorial/TutorialManager';
import { eLandmarkType, User } from '../User/User';
import { BuildingConstructionClone } from './BuildingConstructionClone';
import { PopupManager } from './Common/PopupManager';
import { ItemFrame } from './ItemSlot/ItemFrame';
import { SystemPopup } from './SystemPopup';
import { ToastMessage } from './ToastMessage';
import { UICanvas } from './UICanvas';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BuildingTypeCommon
 * DateTime = Tue Jan 18 2022 19:43:24 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = BuildingTypeCommon.ts
 * FileBasenameNoExtension = BuildingTypeCommon
 * URL = db://assets/Scripts/UI/BuildingTypeCommon.ts
 *
 */
 
@ccclass('BuildingTypeCommon')
export class BuildingTypeCommon extends SystemPopup
{
    protected buildingBaseInfo : BuildingBaseData = null
    buildingLevelInfo : BuildingLevelData = null

    @property(Label)
    labelBuildingName : Label = null

    @property(Label)
    labelBuildingDesc : Label = null

    @property(Label)
    labelReqTime : Label = null

    @property(Sprite)
    sprReqCost : Sprite = null

    @property(Label)
    labelReqCost : Label = null

    @property(Node)
    materialContentNode : Node = null

    @property(Node)
    CostMessageNode : Node = null

    @property(Node)
    CostNode : Node = null

    @property(Sprite)
    buildingIcon : Sprite = null;

    @property(Button)
    okBtn: Button = null;

    private myMap: MapManager = null;
    private myResource: ResourceManager = null;
    private isSufficientMaterial : boolean = true;

    private ManagerCheck() {
        if(this.myMap == null) {
            this.myMap = GameManager.GetManager(MapManager.Name);
        }
        if(this.myResource == null) {
            this.myResource = GameManager.GetManager<ResourceManager>(ResourceManager.Name);
        }
    }
    
    setMessage(title? : string, body? : string)
    {
        this.ManagerCheck();

        let levelInfoArray : BuildingLevelData[] = []
        let levelTableDatas = TableManager.GetTable<BuildingLevelTable>(BuildingLevelTable.Name).GetAll();
        levelTableDatas.forEach((element : BuildingLevelData)=>
        {
            if(element.BUILDING_GROUP == title)
            {
                levelInfoArray.push(element)
            }
        })

        levelInfoArray.sort((a, b)=> a.LEVEL - b.LEVEL)

        this.buildingBaseInfo = TableManager.GetTable<BuildingBaseTable>(BuildingBaseTable.Name).Get(title);
        this.buildingLevelInfo = levelInfoArray[0];
        
        this.labelBuildingName.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(this.buildingBaseInfo._NAME).TEXT;
        this.labelBuildingDesc.string = TableManager.GetTable<StringTable>(StringTable.Name).Get(this.buildingBaseInfo._DESC).TEXT;
        this.labelReqTime.string = TimeString(this.buildingLevelInfo.UPGRADE_TIME);

        const needCount = this.buildingLevelInfo.NEED_ITEM.length;
        for(let i = 0; i < needCount; i++)
        {
            let clone : Node = instantiate(this.myResource.GetResource(ResourcesType.UI_PREFAB)["item"])
            clone.parent = this.materialContentNode
            let itemFrame = clone.getComponent(ItemFrame);
            itemFrame.setFrameReceipeInfo(this.buildingLevelInfo.NEED_ITEM[i].ITEM_NO, levelInfoArray[0].NEED_ITEM[i].ITEM_COUNT);
            if(this.isSufficientMaterial) {
                this.isSufficientMaterial = itemFrame.IsSufficientAmount;
            }
        }

        if(this.buildingLevelInfo.COST_NUM > 0)
        {
            this.CostMessageNode.active = false
            this.CostNode.active = true
            this.labelReqCost.string = this.buildingLevelInfo.COST_NUM.toString();

            let userGold = User.Instance.GOLD;
            if(userGold < this.buildingLevelInfo.COST_NUM){
                this.isSufficientMaterial = false;
            }
        }

        if(this.buildingIcon != null){
            this.buildingIcon.spriteFrame = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.BUILDINGICON_SPRITEFRAME)[this.buildingLevelInfo.BUILDING_GROUP];
        }

        if(!this.isSufficientMaterial) {
            if(this.okBtn != null) {
                this.okBtn.interactable = false;
                if(this.sprReqCost != null) {
                    this.sprReqCost.color = this.okBtn.disabledColor;
                }
            }
        }
    }

    onConstruct(event, CustomEventData? : string)
    {
        if(!this.isSufficientMaterial){
            let popup = PopupManager.OpenPopup("SystemPopupOK") as SystemPopup;
            popup.setMessage(StringTable.GetString(100000618, "오류"), StringTable.GetString(100000619, "재료 비용 부족"));
            this.ClosePopup();
            return;
        }

        this.ManagerCheck();
        let landmarkIndex = ["dozer", "travel", "subway"].indexOf(this.buildingBaseInfo.Index as string)
        
        if(landmarkIndex >= 0 )
        {
            let tags = [eLandmarkType.COINDOZER, eLandmarkType.WORLDTRIP, eLandmarkType.SUBWAY]
            let params = 
            {
                tag:tags[landmarkIndex]
            }
            NetworkManager.Send("building/construct", params, ()=>
            {
                PopupManager.ForceUpdate()
                this.ClosePopup()
            })
            return
        }

        UICanvas.ActiveUI = false;
        let myMap = this.myMap;
        let tempData = myMap.tempData;
        let gridData = User.Instance.GetGridData();
        const floorKeys = Object.keys(gridData);
        let needSize = this.buildingBaseInfo.SIZE
        
        let buildClone = new Node();
        buildClone.addComponent(UITransform).setAnchorPoint(0, 0)
        buildClone.getComponent(UITransform).setContentSize(new math.Size(CellSize.x * needSize, CellSize.y))
        let buildCloneSprite = buildClone.addComponent(UIOpacity);
        buildCloneSprite.opacity = 120;
        let image : Node = instantiate(this.myResource.GetResource<Node>(ResourcesType.BUILDING_PREFAB)[this.buildingBaseInfo.Index]);
        if(image != null) {
            let parent = buildClone;
            image.setParent(parent, false);
            image.setPosition(Vec3.ZERO);
            let animation = image.getComponent(Animation);
            if(animation != null) {
                animation.stop();
                animation.enabled = false;
            }
            image.active = false;
        }

        DataManager.AddData("buildClone", buildClone);

        for(let i = 0; i < floorKeys.length; i++)
        {
            const floorkey = floorKeys[i];
            const cellKeys = Object.keys(gridData[floorkey]);
            for(let j = 0; j < cellKeys.length; j++)
            {
                const cellkey = cellKeys[j];
                if(gridData[floorkey][cellkey] == 0)
                {
                    let targetNode: Node = null;
                    if(tempData[floorkey] == undefined) {
                        tempData[floorkey] = {};
                    }

                    let cell = myMap.GetCell(floorkey, cellkey);
                    if(tempData[floorkey][cellkey] == undefined) {
                        targetNode = instantiate(myMap.default_building);
                        tempData[floorkey][cellkey] = targetNode;

                        targetNode.parent = myMap.emptyNode;
                        targetNode.worldPosition = new Vec3(cell.node.worldPosition);
                    } else {
                        targetNode = tempData[floorkey][cellkey];
                        targetNode.worldPosition = new Vec3(cell.node.worldPosition);
                    }
                    
                    let targetBtn = targetNode.getComponent(Button);
                    if(targetBtn != null) {
                        targetBtn.enabled = true;
                        targetBtn.interactable = true;
                    }
                    
                    let uiTransform = targetNode.getComponent(UITransform);
                    if(uiTransform != null) {
                        uiTransform.setContentSize(cell.GetCellSize());
                    }
                    
                    let newEvent = new EventHandler();
                    newEvent.target = this.node;
                    newEvent.component = "BuildingTypeCommon"
                    newEvent.handler = "addButtonSet";
                    
                    let object = {
                        cellKey: cellkey,
                        cellKeys: cellKeys,
                        floorkey: floorkey,
                        needSize: needSize,
                        buildClone: buildClone,
                        buildImage: image,
                        newEvent: newEvent,
                        targetBtn: targetBtn,
                        baseData: this.buildingBaseInfo,
                        myResource: this.myResource,
                        myMap: this.myMap,
                    };
                    targetNode.on(Node.EventType.MOUSE_ENTER, this.MouseEnter, object);
                    targetNode.on(Node.EventType.MOUSE_LEAVE, this.MouseLeave, object);
                    targetNode.on(Node.EventType.MOUSE_DOWN, this.onMouseDown);
                    targetNode.on(Node.EventType.MOUSE_UP, this.onMouseUp);
                    targetNode.on(Node.EventType.MOUSE_MOVE, this.onMouseMove);
                    // if(needSize != 1)
                    // {
                    //     let object = {
                    //         cellKey: cellKey,
                    //         cellKeys: cellKeys,
                    //         floorkey: floorkey,
                    //         needSize: needSize,
                    //         buildClone: buildClone,
                    //         newEvent: newEvent,
                    //         targetBtn: targetBtn,
                    //         baseData: this.buildingBaseInfo,
                    //         buildCloneSprite: buildCloneSprite,
                    //     };
                    //     //사이즈가 1이 아니면 이벤트 심어줌
                    // }
                    // else
                    // {
                    //     newEvent.customEventData = JSON.stringify({canConst : true, floorkey : floorkey, cellKey : cellKey, size : needSize });
                    //     targetBtn.clickEvents.push(newEvent);
                    //     targetNode.on(Node.EventType.MOUSE_ENTER, this.MouseEnter, object);
                    //     targetNode.on(Node.EventType.MOUSE_LEAVE, this.MouseLeave, object);
                    // }
                }
            }
        }

        this.schedule(this.CellPositionChange, 0.2);

        // 튜토리얼 실행
        TutorialManager.GetInstance.OnTutorialEvent(102,5);
    }

    CellPositionChange() {
        let myMap = GameManager.GetManager<MapManager>(MapManager.Name);
        if (myMap == null || myMap.tempData == null) {
            return;
        }
        let tempData = myMap.tempData;
        const floorKeys = Object.keys(tempData);
        const floorCount = floorKeys.length;
        for(var y = 0 ; y < floorCount ; y++) {
            const floorkey = floorKeys[y];
            let cellData = tempData[floorkey];
            const cellKeys = Object.keys(cellData);
            const cellCount = cellKeys.length;
            for(var x = 0 ; x < cellCount ; x++) {
                const cellkey = cellKeys[x];
                let targetNode = tempData[floorkey][cellkey];
                let uiTransform: UITransform = targetNode.getComponent(UITransform);
                let cell = myMap.GetCell(floorkey, cellkey);
                if(cell != null) {
                    if(targetNode != null) {
                        targetNode.worldPosition = cell.node.worldPosition;
                    } 
                    if(uiTransform != null) {
                        uiTransform.setAnchorPoint(cell.GetAnchorPoint());
                        uiTransform.setContentSize(cell.GetCellSize());
                    }
                }
            }
        }
    }

    MouseLeave(event: EventMouse) {
        this['targetBtn'].clickEvents = [];
        this['buildClone'].active = false;
        this['buildImage'].active = false;
    }
 
    MouseEnter(event: EventMouse) {
        let sizeCount = 0; //확보한 셀
        let myMap = this['myMap']; //셀 번호
        let myResource = this['myResource']; //셀 번호
        let tempData = myMap.tempData;
        let gridData = User.Instance.GetGridData();
        let cellKey = this['cellKey']; //셀 번호
        let curCellnum = cellKey; //셀 번호
        let cellKeys = this['cellKeys']; //셀 번호
        let floorkey = this['floorkey']; //셀 번호
        let needSize = this['needSize']; //셀 번호
        let buildClone = this['buildClone']; //셀 번호
        let newEvent = this['newEvent']; //셀 번호
        let targetBtn = this['targetBtn']; //셀 번호
        this['buildImage'].active = true;
        console.log(event);
        console.log(buildClone);
        console.log(UICanvas.globalMainCamRef);

        //오른쪽 우선
        if(cellKeys.length != cellKeys.indexOf(cellKey))
        {
            for(let i = cellKeys.indexOf(cellKey); i < cellKeys.length; i++)
            {
                if(gridData[floorkey][i] == 0)
                    sizeCount++
                else
                    break;

                if(sizeCount >= needSize)
                    break;    
            }
        }

        if(needSize > sizeCount && cellKeys.indexOf(cellKey) > 0)
        {
            //시작점 부터 왼쪽으로 필요만큼
            //건설 지점 (curCellnum)가 기준으로 변경됨
            for(let i = cellKeys.indexOf(cellKey) - 1; i >= 0; i--)
            {
                if(gridData[floorkey][i] == 0)
                {
                    sizeCount++
                    curCellnum = cellKeys[i];
                }

                if(sizeCount >= needSize)
                    break;
            }
        }

        // 확보한 칸 수 검사
        if(needSize == sizeCount)
        {
            let posX = 0;
            if(this.myMap.IsStartCell(cellKey)) {
                posX = 24;
            }
            //확보됨
            buildClone.parent = myMap.emptyNode
            buildClone.worldPosition = new Vec3(tempData[floorkey][curCellnum].worldPosition.x + posX, tempData[floorkey][curCellnum].worldPosition.y, tempData[floorkey][curCellnum].worldPosition.z);
            buildClone.setSiblingIndex(9999)
            buildClone.active = true;

            newEvent.customEventData = JSON.stringify({canConst : needSize == sizeCount, floorkey : floorkey, cellKey : needSize == sizeCount ? curCellnum : cellKey, size : needSize });
            targetBtn.clickEvents.push(newEvent);
        }
        else
        {
            buildClone.active = false;

            let event = new EventHandler();
            event.target = this.node;
            event.component = "BuildingTypeCommon"
            event.handler = "onClickNoSize";
            targetBtn.clickEvents.push(event);
        }
    }

    addButtonSet(event, CustomEventData)
    {
        let clone = DataManager.GetData<Node>("buildClone");
        if(clone != null) {
            DataManager.DelData("buildClone");
        }
        let prevNode = DataManager.GetData<Node>("SelectConstructNode");
        if(prevNode != null) {
            prevNode.removeFromParent();
            DataManager.DelData("SelectConstructNode");
        }
        let prevTargetNode = DataManager.GetData<Node>("PrevConstructNode");
        if(prevTargetNode != null) {
            prevTargetNode.getComponent(Button).enabled = true;
            DataManager.DelData("PrevConstructNode");
        }

        let myMap = this.myMap;
        let myResource = this.myResource;
        let tempData = myMap.tempData;

        let jsonData = JSON.parse(CustomEventData)
        if(!jsonData.canConst)
        {
            //UI가 OFF 이므로 안보임
            ToastMessage.Set(StringTable.GetString(100000630), null, -52);
        }
        else
        {
            let posX = 0;
            let targetSize = jsonData['targetSize'];
            if(this.myMap.IsStartCell(jsonData['cellKey']) && targetSize == undefined) {
                posX = 24;
            }
            let targetNode: Node = tempData[jsonData['floorkey']][jsonData['cellKey']];
            let buttonSet : Node = instantiate(myResource.GetResource(ResourcesType.UI_PREFAB)["constructionClone"]);
            buttonSet.setParent(myMap.emptyNode, false);
            buttonSet.worldPosition = new Vec3(targetNode.worldPosition.x + posX, targetNode.worldPosition.y, targetNode.worldPosition.z);
            buttonSet.setSiblingIndex(10000)
            let image : Node = instantiate(myResource.GetResource<Node>(ResourcesType.BUILDING_PREFAB)[this.buildingBaseInfo.Index]);
            let parentClone = buttonSet.getChildByName('container');
            if(image != null) {
                let parent = buttonSet;
                if(parentClone != null) {
                    parent = parentClone;
                }
                image.setParent(parent, false);
                image.setPosition(Vec3.ZERO);
                let animation = image.getComponent(Animation);
                if(animation != null) {
                    animation.stop();
                    animation.enabled = false;
                }
            }
            let uiTransform = buttonSet.getComponent(UITransform);
            if(uiTransform != null) {
                if(targetSize != undefined) {
                    uiTransform.setContentSize(CellSize.x * jsonData['size'], CellSize.y);
                } else {
                    uiTransform.setContentSize(CellSize.x * jsonData['size'], CellSize.y);
                }
            }
            
            let clone = buttonSet.getComponent(BuildingConstructionClone);
            if(clone != null) {
                clone.cb_offConstruct = this.offConstruct;
                clone.setBuildingBaseInfo(this.buildingBaseInfo, jsonData['cellKey'], jsonData['floorkey']);
                // buttonSet.on(Node.EventType.MOUSE_DOWN, this.onMouseDown);
                // buttonSet.on(Node.EventType.MOUSE_UP, this.onMouseUp);
                // buttonSet.on(Node.EventType.MOUSE_MOVE, this.onMouseMove);
            }

            DataManager.AddData("SelectConstructNode", buttonSet);
            DataManager.AddData("PrevConstructNode", targetNode);
            DataManager.AddData("ConstructWPos", buttonSet.worldPosition);

            UICanvas.mouseMove = false;
            UICanvas.globalMainCamRef.node.worldPosition = new Vec3(buttonSet.worldPosition.x, buttonSet.worldPosition.y, UICanvas.globalMainCamRef.node.worldPosition.z);

            let gridData = User.Instance.GetGridData();
            const floorKeys = Object.keys(gridData);

            for(let i = 0; i < floorKeys.length; i++)
            {
                const floorkey = floorKeys[i];
                const cellKeys = Object.keys(gridData[floorkey]);
                for(let j = 0; j < cellKeys.length; j++)
                {
                    const cellKey = cellKeys[j];
                    if(gridData[floorkey][cellKey] == 0)
                    {
                        targetNode.getComponent(Button).enabled = false;
                    }                    
                }
            }

            SoundMixer.PlaySoundFX(SOUND_TYPE.FX_BATCH);
        }
    }

    onClickNoSize(event) {
        StringTable.GetString(100000630);
    }

    offConstruct()
    {
        this.unscheduleAllCallbacks();
        let clone = DataManager.GetData<Node>("buildClone");
        if(clone != null) {
            clone.removeFromParent();
            DataManager.DelData("buildClone");
        }
        DataManager.DelData("ConstructWPos");

        let myMap = GameManager.GetManager(MapManager.Name) as MapManager;
        delete myMap.tempData;
        myMap.tempData = {};
        let emptyNode = myMap.emptyNode;
        if(emptyNode != null) {
            emptyNode.removeAllChildren();
        }
    }

    onMouseDown(event : EventMouse)
    {
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
        console.log("build")
        switch(event.getButton())
        {
            case EventMouse.BUTTON_LEFT:
                UICanvas.mouseMove = false;
                UICanvas.CamMovement()
            break;
        }
    }

    onMouseMove(event : EventMouse)
    {
        if(UICanvas.mouseMove == false || UICanvas.globalMainCamRef == null)
        {
            return;
        }

        UICanvas.CalcualteCamMinMaxPos(event.getDelta().x, event.getDelta().y)
    }
}