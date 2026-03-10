import { _decorator, resources, Asset, SpriteFrame, Prefab, AssetManager } from 'cc';
import { GameManager } from './GameManager';
import { IManagerBase } from 'sb';
import { AssetConstructor, AudioClipBGMPath, AudioClipFXPath, BuildingClonePath, BuildingIconPath, CharIconPath, DragonClonePath, DragonGradeTagIconPath, EffectPrefabPath, ElementIconPath, ItemIconPath, MonsterClonePath, PartsIconPath, PopupPrefabPath, PrefabClonePath, PrefabRootPath, QuestIconPath, SkillIconPath, StagePrefabPath, TownPrefabPath, WorldPrefabPath } from './Tools/SandboxTools';

/**
 * Predefined variables
 * Name = ResourceManager
 * DateTime = Thu Jan 13 2022 16:11:27 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = ResourceManager.ts
 * FileBasenameNoExtension = ResourceManager
 * URL = db://assets/Scripts/ResourceManager.tss
 *
 */

 export enum ResourcesType
 {
    //AudioClip 1~
    BGM_AUDIOCLIP = 1,
    FX_AUDIOCLIP,

    //SpriteFrame 10 ~
    ITEMICON_SPRITEFRAME = 10,
    CHARACTORICON_SPRITEFRAME,
    BUILDINGICON_SPRITEFRAME,
    ELEMENTICON_SPRITEFRAME,
    SKILLICON_SPRITEFRAME,
    PARTICON_SPRITEFRAME,
    DRAGON_GRADETAG_SPRITEFRAME,
    QUESTICON_SPRITEFRAME,
     
    //Prefab 100 ~
    POPUP_PREFAB = 100,
    UI_PREFAB,
    BUILDING_PREFAB,
    DRAGON_PREFAB,
    MONSTER_PREFAB,
    STAGE_PREFAB,
    EFFECT_PREFAB,
    PREFAB_ROOT,
    TOWN_PREFAB,
    WORLD_PREFAB,
 }

export class ResourceManager implements IManagerBase
{
    public static Name: string = "ResourceManager";
    private static instance : ResourceManager = null
    private isInit = false;

    //#region   Resources List
    private static LoadedResources : {[type : number] : any} = { }
    private loadCount = 0;
    private completeCount = 0;
    private loadingResource = [];
    private loadResource = {};
    public get LoadComplete(): boolean {
        if(this.loadCount == 0 || this.completeCount == 0) {
            return false;
        }
        if(this.loadCount == this.completeCount) {
            return true;
        }
        return false;
    }

    public get LoadString(): string {
        if(this.loadingResource == null || this.loadingResource.length < 1) {
            return "";
        }

        switch(this.loadingResource[0]) {
            case ResourcesType.BGM_AUDIOCLIP: {
                return "Loading BGM";
            } break;
            case ResourcesType.FX_AUDIOCLIP: {
                return "Loading Sound";
            } break;

            case ResourcesType.ITEMICON_SPRITEFRAME: {
                return "Loading Icon";
            } break;
            case ResourcesType.CHARACTORICON_SPRITEFRAME: {
                return "Loading Icon";
            } break;
            case ResourcesType.BUILDINGICON_SPRITEFRAME: {
                return "Loading Icon";
            } break;
            case ResourcesType.ELEMENTICON_SPRITEFRAME: {
                return "Loading Icon";
            } break;
            case ResourcesType.SKILLICON_SPRITEFRAME: {
                return "Loading Icon";
            } break;
            case ResourcesType.PARTICON_SPRITEFRAME: {
                return "Loading Icon";
            } break;
            case ResourcesType.DRAGON_GRADETAG_SPRITEFRAME: {
                return "Loading Icon";
            } break;
            case ResourcesType.QUESTICON_SPRITEFRAME: {
                return "Loading Icon";
            } break;

            case ResourcesType.POPUP_PREFAB: {
                return "Loading UI";
            } break;
            case ResourcesType.UI_PREFAB: {
                return "Loading UI";
            } break;
            case ResourcesType.BUILDING_PREFAB: {
                return "Loading Town";
            } break;
            case ResourcesType.DRAGON_PREFAB: {
                return "Loading Dragon";
            } break;
            case ResourcesType.MONSTER_PREFAB: {
                return "Loading Monster";
            } break;
            case ResourcesType.STAGE_PREFAB: {
                return "Loading Battle";
            } break;
            case ResourcesType.EFFECT_PREFAB: {
                return "Loading Battle";
            } break;
            case ResourcesType.PREFAB_ROOT: {
                return "Loading UI";
            } break;
            case ResourcesType.TOWN_PREFAB: {
                return "Loading Town";
            } break;
            case ResourcesType.WORLD_PREFAB: {
                return "Loading Battle";
            } break;
        }
        return "";
    }

    //#endregion

    public static get Instance() 
    {
        if(ResourceManager.instance == null) 
        {
            return ResourceManager.instance = new ResourceManager();
        }
        return ResourceManager.instance;
    }

    GetManagerName(): string {
        return ResourceManager.Name
    }
    Update(deltaTime: number): void { }

    Init()
    {
        if(this.isInit) {
            return;
        }
        this.isInit = true;
        GameManager.Instance.AddManager(this, false);
        this.InitTown();
        this.InitIcon();
        this.InitDragon();
        this.InitUI();
        this.InitSound();
        this.InitStage();
        this.InitEffect();
    }

    public InitStage() {
        ResourceManager.instance.LoadDir(ResourcesType.STAGE_PREFAB, Prefab, StagePrefabPath);
        ResourceManager.instance.LoadDir(ResourcesType.WORLD_PREFAB, Prefab, WorldPrefabPath);
    }

    public InitEffect() {
        ResourceManager.instance.LoadDir(ResourcesType.EFFECT_PREFAB, Prefab, EffectPrefabPath);
    }

    public InitDragon() {
        ResourceManager.instance.LoadDir(ResourcesType.DRAGON_PREFAB, Prefab, DragonClonePath);
        ResourceManager.instance.LoadDir(ResourcesType.MONSTER_PREFAB, Prefab, MonsterClonePath);
    }

    public InitUI() {
        ResourceManager.instance.LoadDir(ResourcesType.UI_PREFAB, Prefab, PrefabClonePath);
        ResourceManager.instance.LoadDir(ResourcesType.POPUP_PREFAB, Prefab, PopupPrefabPath);
    }
    
    public InitTown() {
        ResourceManager.instance.LoadDir(ResourcesType.TOWN_PREFAB, Prefab, TownPrefabPath);
        ResourceManager.instance.LoadDir(ResourcesType.BUILDING_PREFAB, Prefab, BuildingClonePath);
    }

    public InitIcon() {
        ResourceManager.instance.LoadDir(ResourcesType.DRAGON_GRADETAG_SPRITEFRAME, SpriteFrame, DragonGradeTagIconPath);
        ResourceManager.instance.LoadDir(ResourcesType.SKILLICON_SPRITEFRAME, SpriteFrame, SkillIconPath);
        ResourceManager.instance.LoadDir(ResourcesType.ITEMICON_SPRITEFRAME, SpriteFrame, ItemIconPath);
        ResourceManager.instance.LoadDir(ResourcesType.CHARACTORICON_SPRITEFRAME, SpriteFrame, CharIconPath);
        ResourceManager.instance.LoadDir(ResourcesType.ELEMENTICON_SPRITEFRAME, SpriteFrame, ElementIconPath);
        ResourceManager.instance.LoadDir(ResourcesType.PARTICON_SPRITEFRAME, SpriteFrame, PartsIconPath);
        ResourceManager.instance.LoadDir(ResourcesType.QUESTICON_SPRITEFRAME, SpriteFrame, QuestIconPath);
        ResourceManager.instance.LoadDir(ResourcesType.BUILDINGICON_SPRITEFRAME, SpriteFrame, BuildingIconPath);
    }

    public InitSound() {
        ResourceManager.instance.LoadDir(ResourcesType.BGM_AUDIOCLIP, Asset, AudioClipBGMPath);
        ResourceManager.instance.LoadDir(ResourcesType.FX_AUDIOCLIP, Asset, AudioClipFXPath);
    }

    private LoadDir<T extends Asset>(type : ResourcesType, cctype: AssetConstructor<T>, path : string)
    {
        this.loadCount++;
        if(this.loadingResource == null) {
            this.loadingResource = [];
        }
        if(this.loadResource == null) {
            this.loadResource = [];
        }
        if(this.loadResource[type] != undefined && this.loadResource[type] == true) {
            console.log("Completed Load");
            return;
        }
        this.loadingResource.push(type);
        resources.loadDir(path, cctype, null, (err, asset)=>
        {
            this.completeCount++;
            ResourceManager.LoadedResources[type] = Object()
            if(err == null)
            {
                this.loadResource[type] = true;
                this.loadingResource = this.loadingResource.filter(element => element != type);
                asset.forEach(element => 
                {
                    if(type < ResourcesType.POPUP_PREFAB)
                        ResourceManager.LoadedResources[type][element.name] = element;
                    else    //팝업은 element.name = "" 이므로 예외가 필요
                        ResourceManager.LoadedResources[type][(element as any).data.name] = element;
                });
            }
            else
                console.log(err + "LoadDir, : ", type);
        });
    }

    GetResource<T>(type : ResourcesType) : T[]
    {
        return ResourceManager.LoadedResources[type] as T[]
    }
}