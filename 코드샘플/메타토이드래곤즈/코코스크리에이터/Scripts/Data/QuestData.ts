import { DataBase } from "./DataBase";

export class QuestData extends DataBase {
    protected qtype : number = 0
    public get QTYPE() { return this.qtype;}
    public set QTYPE(value : number) { this.qtype = value;}

    protected icon : string = ""
    public get ICON() { return this.icon;}
    public set ICON(value : string) { this.icon = value;}

    protected subject : number = 0
    public get SUBJECT() { return this.subject;}
    public set SUBJECT(value : number) { this.subject = value;}

    protected noti : number = 0
    public get NOTI() { return this.noti;}
    public set NOTI(value : number) { this.noti = value;}

    protected sgroup : number = 0
    public get START_GROUP() { return this.sgroup;}
    public set START_GROUP(value : number) { this.sgroup = value;}

    protected cgroup : number = 0
    public get CONDITION_GROUP() { return this.cgroup;}
    public set CONDITION_GROUP(value : number) { this.cgroup = value;}

    protected reward_exp : number = 0
    public get REWARD_ACCOUNT_EXP() { return this.reward_exp;}
    public set REWARD_ACCOUNT_EXP(value : number) { this.reward_exp = value;}

    protected reward_gold : number = 0
    public get REWARD_GOLD() { return this.reward_gold;}
    public set REWARD_GOLD(value : number) { this.reward_gold = value;}

    protected reward_energy : number = 0
    public get REWARD_ENERGY() { return this.reward_energy;}
    public set REWARD_ENERGY(value : number) { this.reward_energy = value;}

    protected reward_items_id : Array<number> = []
    public get REWARD_ITEMS_ID() { return this.reward_items_id;}
    public set REWARD_ITEMS_ID(value : Array<number>) { this.reward_items_id = value;}

    protected reward_items_value : Array<number> = []
    public get REWARD_ITEMS_VALUE() { return this.reward_items_value;}
    public set REWARD_ITEMS_VALUE(value : Array<number>) { this.reward_items_value = value;}
}

export class QuestTriggerData extends DataBase {
    protected key : number = 0
    public get KEY() { return this.key;}
    public set KEY(value : number) { this.key = value}

    protected trigger_type : number = 0
    public get TRIGGER_TYPE() { return this.trigger_type;}
    public set TRIGGER_TYPE(value : number) { this.trigger_type = value;}

    protected group : number = 0
    public get GROUP() { return this.group;}
    public set GROUP(value : number) { this.group = value;}

    protected type : string = ""
    public get TYPE() { return this.type;}
    public set TYPE(value : string) { this.type = value;}

    protected type_key : string|number;
    public get TYPE_KEY() { return this.type_key;}
    public set TYPE_KEY(value : string|number) { this.type_key = value;}

    protected type_key_value : number = 0
    public get TYPE_KEY_VALUE() { return this.type_key_value;}
    public set TYPE_KEY_VALUE(value : number) { this.type_key_value = value;}
}