
import { _decorator, Component, Node, Asset, Prefab, resources, SpriteFrame } from 'cc';
import { IManagerBase } from 'sb';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { ItemIconPath, CharIconPath, ElementIconPath, PartsIconPath, DragonGradeTagIconPath, BuildingIconPath, SkillIconPath, BuildingPath, PopupPrefabPath, PrefabClonePath, BuildingClonePath, DragonClonePath, MonsterClonePath, AudioClipBGMPath, AudioClipFXPath, StagePrefabPath, EffectPrefabPath, PrefabRootPath, AssetConstructor } from '../../Tools/SandboxTools';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = BattleSimulatorResourceManager
 * DateTime = Thu Apr 14 2022 13:47:36 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = BattleSimulatorResourceManager.ts
 * FileBasenameNoExtension = BattleSimulatorResourceManager
 * URL = db://assets/Scripts/Battle/Battle_simulator/BattleSimulatorResourceManager.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 @ccclass('BattleSimulatorResourceManager')
export class BattleSimulatorResourceManager extends Component implements IManagerBase {
    public static Name: string = "BattleSimulatorResourceManager";
    private static instance : BattleSimulatorResourceManager = null
    private isInit = false;

    //#region   Resources List
    private static LoadedResources : {[type : number] : any} = { }
    private loadCount = 0;
    private completeCount = 0;
    public get LoadComplete(): boolean {
        if(this.loadCount == 0 || this.completeCount == 0) {
            return false;
        }
        if(this.loadCount == this.completeCount) {
            return true;
        }
        return false;
    }

    prefabList : Prefab[] = [];

    //#endregion

    public static get Instance() 
    {
        if(BattleSimulatorResourceManager.instance == null) 
        {
            return BattleSimulatorResourceManager.instance = new BattleSimulatorResourceManager();
        }
        return BattleSimulatorResourceManager.instance;
    }

    GetManagerName(): string {
        return BattleSimulatorResourceManager.Name
    }
    Update(deltaTime: number): void { }

    Init()
    {
        if(this.isInit) {
            return;
        }
        this.isInit = true;
        GameManager.Instance.AddManager(this, false)
        //BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.ITEMICON_SPRITEFRAME, SpriteFrame, ItemIconPath)
        //BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.CHARACTORICON_SPRITEFRAME, SpriteFrame, CharIconPath)
        //BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.ELEMENTICON_SPRITEFRAME, SpriteFrame, ElementIconPath)
        //BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.PARTICON_SPRITEFRAME, SpriteFrame, PartsIconPath)
        //BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.DRAGON_GRADETAG_SPRITEFRAME, SpriteFrame, DragonGradeTagIconPath)
        //BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.BUILDINGICON_SPRITEFRAME, SpriteFrame, BuildingIconPath)
        //BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.SKILLICON_SPRITEFRAME, SpriteFrame, SkillIconPath)
        //BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.BUILDING_OBJECT, SpriteFrame, BuildingPath)
        BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.POPUP_PREFAB, Prefab, PopupPrefabPath)
        BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.UI_PREFAB, Prefab, PrefabClonePath)
        //BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.BUILDING_PREFAB, Prefab, BuildingClonePath)
        BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.DRAGON_PREFAB, Prefab, DragonClonePath)
        BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.MONSTER_PREFAB, Prefab, MonsterClonePath)
        BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.BGM_AUDIOCLIP, Asset, AudioClipBGMPath)
        BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.FX_AUDIOCLIP, Asset, AudioClipFXPath)
        BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.STAGE_PREFAB, Prefab, StagePrefabPath)
        BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.EFFECT_PREFAB, Prefab, EffectPrefabPath)
        BattleSimulatorResourceManager.instance.LoadDir(ResourcesType.PREFAB_ROOT, Prefab, PrefabRootPath)
    }

    SetPrefabList(list : Prefab[])
    {
        this.prefabList = [];
        this.prefabList = list;
    }

    private LoadDir<T extends Asset>(type : ResourcesType, cctype: AssetConstructor<T>, path : string)
    {
        this.loadCount++;
        resources.loadDir(path, cctype, null, (err, asset)=>
        {
            this.completeCount++;
            BattleSimulatorResourceManager.LoadedResources[type] = Object()
            if(err == null)
            {
                asset.forEach(element => 
                {
                    if(type < ResourcesType.POPUP_PREFAB)
                    BattleSimulatorResourceManager.LoadedResources[type][element.name] = element;
                    else    //팝업은 element.name = "" 이므로 예외가 필요
                    BattleSimulatorResourceManager.LoadedResources[type][(element as any).data.name] = element;
                });
            }
            else
                console.log(err + "LoadDir, : ", type);
        });
    }

    GetResource<T>(type : ResourcesType) : T[]
    {
        return BattleSimulatorResourceManager.LoadedResources[type] as T[]
    }

    GetResourceByName<T>(resourceName : string) : Prefab
    {
        let checkPrefab =  this.prefabList.filter(value => resourceName == value.data._name);
        if(checkPrefab == null || checkPrefab.length <= 0){
            return null;
        }
        return checkPrefab[0];
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/en/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/en/scripting/life-cycle-callbacks.html
 */
