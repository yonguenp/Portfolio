
import { _decorator } from 'cc';
import { DataBase } from './DataBase';
 
export class TutorialData extends DataBase {
    
    protected key: number = -1;
    public get Index(): number { return this.key }
    public set Index(value: number) { this.key = value }

    protected group: number = -1;
    public get GROUP(): number { return this.group }
    public set GROUP(value: number) { this.group = value }

    protected sequence: number = -1;
    public get SEQUENCE(): number { return this.sequence }
    public set SEQUENCE(value: number) { this.sequence = value }

    protected quest_cond_key: number = -1;
    public get QUEST_COND_KEY(): number { return this.quest_cond_key }
    public set QUEST_COND_KEY(value: number) { this.quest_cond_key = value }

    protected event_type: number = -1;
    public get EVENT_TYPE(): number { return this.event_type }
    public set EVENT_TYPE(value: number) { this.event_type = value }

    protected delay: number = -1;
    public get DELAY(): number { return this.delay }
    public set DELAY(value: number) { this.delay = value }

    protected _desc: number = -1;
    public get _DESC(): number { return this._desc }
    public set _DESC(value: number) { this._desc = value }
}