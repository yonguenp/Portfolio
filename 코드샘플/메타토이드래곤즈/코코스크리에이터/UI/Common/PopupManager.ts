import { _decorator, Node, instantiate } from 'cc';
import { GameManager } from '../../GameManager';
import { ResourceManager, ResourcesType } from '../../ResourceManager';
import { SoundMixer, SOUND_TYPE } from '../../SoundMixer';
import { PopupBeacon } from './PopupBeacon';
import { Popup } from './Popup';

/**
 * Predefined variables
 * Name = NewComponent
 * DateTime = Thu Dec 30 2021 14:13:47 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = NewComponent.ts
 * FileBasenameNoExtension = NewComponent
 * URL = db://assets/Scripts/NewComponent.ts
 *
 */

export class PopupManager 
{
    private static instance : PopupManager = null;
    private popupStack : Popup[] = [];
    private popupBeacon : PopupBeacon = null;
    public get CurBeacon() {
        return this.popupBeacon;
    }

    static get GetInstance() : Readonly <PopupManager>
    {
        if(PopupManager.instance == null)
        {
            PopupManager.instance = new PopupManager();
        }

        return PopupManager.instance;
    }

    Init(beacon : PopupBeacon, isInit: boolean = true)
    {
        this.popupBeacon = beacon;
        if(isInit) {
            this.popupStack = [];
        }
    }

    /**
     * @name string 팝업프리펩 이름
     * @autoClose boolean 이미 열려있다면 닫고 다시 열것인지
     */
    // static OpenPopup(name : string, autoClose : boolean = true) :  Popup
    // {
    //     let result = null;

    //     if(autoClose && PopupManager.GetPopupByName(name) != null)
    //     {
    //         PopupManager.ClosePopup(PopupManager.GetPopupByName(name));
    //     }

    //     let prefab = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.POPUP_PREFAB)[name];
    //     if(prefab != null)
    //     {
    //         let newNode = instantiate(prefab);
    //         newNode.parent = PopupManager.instance.defaultPopupParent;
    //         let curPopup = newNode.getComponent(Popup);
    //         PopupManager.instance.popupStack.push(curPopup);
    //         result = curPopup;
    //     }

    //     SoundMixer.PlaySoundFX(SOUND_TYPE.FX_NOTICE)
    //     return result;
    // }

    /**
     * @name string 팝업프리펩 이름
     * @autoClose boolean 이미 열려있다면 닫고 다시 열것인지
     */
    static OpenPopup<T extends Popup>(name : string, autoClose : boolean = true, jsonData: any = null) : T
    {
        let result: T = null;

        if(autoClose && PopupManager.GetPopupByName(name) != null)
        {
            PopupManager.ClosePopup(PopupManager.GetPopupByName(name));
        }

        let prefab = GameManager.GetManager<ResourceManager>(ResourceManager.Name).GetResource(ResourcesType.POPUP_PREFAB)[name];
        if(prefab != null)
        {
            let newNode = instantiate(prefab);
            newNode.parent = PopupManager.instance.popupBeacon.node;
            let curPopup: T = newNode.getComponent(Popup) as T;
            curPopup.Init(jsonData);
            PopupManager.instance.popupStack.push(curPopup);
            result = curPopup;
        }

        return result;
    }

    /**
     * @popupObj Popup 인덱스를 찾고싶은 팝업 대상
     * @returns 씬에 있는 팝업이라면 인덱스 번호, 없으면 -1
     */
    static GetPopupIndex(popupObj : Popup) : number
    {
        return PopupManager.instance.popupStack.indexOf(popupObj);
    }

    /**
     * @returns 화면에 뿌려진 팝업 스택 카운트
     */
    static GetPopupStackCount() : number
    {
        return PopupManager.instance.popupStack.length;
    }

    /**
     * @index number 프리펩 이름으로 화면에 뿌려진 팝업 가져오기
     */
    static GetPopupByIndex(index : number) : Popup
    {
        return PopupManager.instance.popupStack[index];
    }

    /**
     * @name string 프리펩 이름으로 화면에 뿌려진 팝업 가져오기
     */
    static GetPopupByName(name : string) : Popup
    {
        let result = null;
        PopupManager.instance.popupStack.forEach((popupObj)=>{
            if(popupObj.node == null || popupObj.node == undefined)
            {
                return;
            }
            if(popupObj.node.name === name)
            {
                result = popupObj;
            }
        });
        return result;
    }

    /**
     * @returns 가장 앞의 팝업 가져오기
     */
    static GetCurrentPopup() : Popup
    {
        return PopupManager.instance.popupStack[PopupManager.instance.popupStack.length-1];
    }

    static ForceUpdate()
    {
        PopupManager.instance.popupStack.forEach((popup)=>
        {
            popup.ForceUpdate()
        })
    }

    static ClosePopup(popupObj : Popup) : boolean
    {
        let index = this.GetPopupIndex(popupObj);
        if(index == -1)
            return false;

        PopupManager.instance.popupStack.splice(index, 1);
        popupObj.node?.destroy();

        return true;
    }

    static AllClosePopup() {
        if(PopupManager.GetInstance == null || PopupManager.instance.popupStack == null || PopupManager.instance.popupStack.length < 1) {
            return;
        }

        while(PopupManager.instance.popupStack.length > 0) {
            let curPop = PopupManager.instance.popupStack.pop();
            if(curPop != null) {
                curPop.node.destroy();
            }
        } 
    }

    static AllStackPop() {
        if(PopupManager.GetInstance == null || PopupManager.instance.popupStack == null || PopupManager.instance.popupStack.length < 1) {
            return;
        }

        while(PopupManager.instance.popupStack.length > 0) {
            PopupManager.instance.popupStack.pop();
        } 
    }

    Update(dt: number) {
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.3/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.3/manual/en/scripting/ccclass.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.3/manual/en/scripting/life-cycle-callbacks.html
 */
