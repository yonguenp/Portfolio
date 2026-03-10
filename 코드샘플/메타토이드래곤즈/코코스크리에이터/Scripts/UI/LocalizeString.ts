
import { _decorator, Component, Label, CCInteger } from 'cc';
import { EventListener } from 'sb';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { SettingEvent } from '../Settings/SettingManager';
import { EventManager } from '../Tools/EventManager';
const { ccclass, property, executionOrder } = _decorator;
 
@ccclass('LocalizeString')
@executionOrder(-1)
export class LocalizeString extends Component implements EventListener<SettingEvent>
{
    @property(CCInteger)
    index : number = 0

    onEnable() {
        EventManager.AddEvent(this);

        this.RefreshLabel();
    }

    onDisable() {
        EventManager.RemoveEvent(this);
    }

    start()
    {
        let label = this.getComponent(Label)
        if(label != null)
        {
            let labelData = TableManager.GetTable<StringTable>(StringTable.Name).Get(this.index);
            if(labelData != null)
            {
                let string = labelData.TEXT.replace('\\n','\n');
                label.string = string;
            }
        }
        // this.enabled = false
    }

    RefreshLabel() {
        let label = this.getComponent(Label)
        if(label != null)
        {
            let labelData = TableManager.GetTable<StringTable>(StringTable.Name).Get(this.index);
            if(labelData != null)
            {
                let string = labelData.TEXT.replace('\\n','\n');
                label.string = string;
            }
        }
    }

    GetID(): string {
        return 'SettingEvent';
    }

    OnEvent(eventType: SettingEvent): void {
        if(eventType.Data['type'] == undefined || eventType.Data['value'] == undefined) {
            return;
        }

        if (eventType.Data['type'] == 'Setting_Language') {
            this.RefreshLabel();
        }
    }
}