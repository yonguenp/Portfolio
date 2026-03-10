
import { _decorator, Label } from 'cc';
import { Popup } from './Common/Popup';
import { PopupManager } from './Common/PopupManager';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = SystemPopup
 * DateTime = Wed Dec 29 2021 19:49:09 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = SystemPopup.ts
 * FileBasenameNoExtension = SystemPopup
 * URL = db://assets/Scripts/SystemPopup.ts
 *
 */
 
@ccclass('SystemPopup')
export class SystemPopup extends Popup 
{
    Init(data?: any) { super.Init(data); }
    onClose() { }
    private okCallback: (CustomEventData? : string) => void = null;
    private cancelCallback : (CustomEventData? : string) => void = null;
    private xCallback : (CustomEventData? : string) => void = null;

    static PopupOK(title : string, body : string , ok_cb? : (CustomEventData? : string) => void) : SystemPopup
    {
        let systemPopup = PopupManager.OpenPopup("SystemPopupOK", false) as SystemPopup
        systemPopup.setMessage(title, body);
        systemPopup.setCallback(ok_cb);
        return systemPopup;
    }

    static PopupOK_CANCEL(title : string, body : string, ok_cb? : (CustomEventData? : string) => void, cancel_cb? : (CustomEventData? : string) => void, x_cb? : (CustomEventData? : string) => void) : SystemPopup
    {
        let systemPopup = PopupManager.OpenPopup("SystemPopupOKCANCEL", false) as SystemPopup
        systemPopup.setMessage(title, body);
        systemPopup.setCallback(ok_cb, cancel_cb, x_cb);

        return systemPopup;
    }

    setMessage(title : string, body : string)
    {
        this.node.getChildByName("body").getChildByName("labelTitle").getComponent(Label).string = title;
        this.node.getChildByName("body").getChildByName("labelBody").getComponent(Label).string = body;
    }

    setCallback(ok_cb? : (CustomEventData? : string) => void, cancel_cb? : (CustomEventData? : string) => void, x_cb? : (CustomEventData? : string) => void)
    {
        if(ok_cb != null)
        {
            this.okCallback = ok_cb;
        }

        if(cancel_cb != null)
        {
            this.cancelCallback = cancel_cb;
        }

        if(x_cb != null)
        {
            this.xCallback = x_cb;
        }
    }

    okCB(event, CustomEventData)
    {
        if(this.okCallback == null)
        {
            this.ClosePopup();
            return;
        }
        this.okCallback();
    }

    cancelCB(event, CustomEventData)
    {
        if(this.cancelCallback == null)
        {
            this.ClosePopup();
            return;
        }
        this.cancelCallback();
    }

    xCB(event, CustomEventData)
    {
        if(this.xCallback == null)
        {
            this.ClosePopup();
            return;
        }
        this.xCallback();
    }
}