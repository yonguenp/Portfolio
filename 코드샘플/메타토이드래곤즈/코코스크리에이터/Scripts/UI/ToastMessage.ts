
import { _decorator, Label, tween, Sprite } from 'cc';
import { Popup } from './Common/Popup';
import { PopupManager } from './Common/PopupManager';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = ToastMessage
 * DateTime = Fri Dec 31 2021 15:14:34 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = ToastMessage.ts
 * FileBasenameNoExtension = ToastMessage
 * URL = db://assets/Scripts/UI/ToastMessage.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 interface Toast { string : string, lifetime? : number }
@ccclass('ToastMessage')
export class ToastMessage extends Popup
{
    init() { }
    onClose() { }
    static toastQueue : Toast[] = [];
    static ticket : boolean = false;
    static lastMessage: string = "";
    lifetime : number = 0
    curtime : number = 0

    //부르는 즉시 메세지 한줄? 정도 나타나고(Fade In) 2~3초 뒤에 Fade Out되는 간단 메세지

    static Set(str : string, lifetime : number = null, diffyLevel : number = 0)
    {
        if(!ToastMessage.isToastEmpty() && ToastMessage.lastMessage == str) {
             return;
        }
        let toast : Toast = { string : str, lifetime : lifetime };
        ToastMessage.toastQueue.push(toast);
        ToastMessage.lastMessage = str;
        let toastObj = ToastMessage.Open();
        if(toastObj != null && toastObj.node != null && diffyLevel)
        {
            console.log("????");
            let originPos = toastObj.node.worldPosition;
            toastObj.node.setWorldPosition(originPos.x + diffyLevel, originPos.y, originPos.z);
        }
    }

    private static Open() : Popup
    {
        if(ToastMessage.ticket || ToastMessage.toastQueue.length == 0)
        {
            return null;
        }

        let toast = ToastMessage.toastQueue[0]

        let toastObj = PopupManager.OpenPopup("ToastMessage", false) as ToastMessage
        toastObj.node.getChildByName("body").getChildByName("labelString").getComponent(Label).string = toast.string;
        toastObj.lifetime = toast.lifetime == null ? 3 : toast.lifetime;
        ToastMessage.ticket = true;
        
        tween(toastObj.getComponentInChildren(Sprite).color).to(0.5, { a : 255 }).delay(toastObj.lifetime - 1).to(0.5, { a : 0 }, {onComplete : ()=>{toastObj.TryRemove()}}).union().start()

        return toastObj;
    }

    private TryRemove()
    {
        ToastMessage.ticket = false;
        ToastMessage.toastQueue.splice(0,1);
        PopupManager.ClosePopup(this);
        ToastMessage.Open()

    }

    static isToastEmpty():boolean{
        return ToastMessage.toastQueue.length == 0;
    }
}