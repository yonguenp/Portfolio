
import { _decorator } from 'cc';
import { DataBase } from './DataBase';

export class StageBaseData extends DataBase
{
    protected world: number = -1;
    public get WORLD(): number { return this.world }
    public set WORLD(value: number) { this.world = value }

    protected type: number = -1;
    public get TYPE(): number { return this.type }
    public set TYPE(value: number) { this.type = value }

    protected difficult: number = -1;
    public get DIFFICULT(): number { return this.difficult }
    public set DIFFICULT(value: number) { this.difficult = value }

    protected stage: number = -1;
    public get STAGE(): number { return this.stage }
    public set STAGE(value: number) { this.stage = value }

    protected name: number = -1;
    public get NAME(): number { return this.name }
    public set NAME(value: number) { this.name = value }

    protected image: string = "";
    public get IMAGE(): string { return this.image }
    public set IMAGE(value: string) { this.image = value }

    protected costtype: string = "";
    public get COST_TYPE(): string { return this.costtype }
    public set COST_TYPE(value: string) { this.costtype = value }

    protected costvalue: number = -1;
    public get COST_VALUE(): number { return this.costvalue }
    public set COST_VALUE(value: number) { this.costvalue = value }

    protected clearcount: number = -1;
    public get CLEAR_COUNT(): number { return this.clearcount }
    public set CLEAR_COUNT(value: number) { this.clearcount = value }

    protected time: number = -1;
    public get TIME(): number { return this.time }
    public set TIME(value: number) { this.time = value }

    protected spawn: number = -1;
    public get SPAWN(): number { return this.spawn }
    public set SPAWN(value: number) { this.spawn = value }

    protected accountexp: number = -1;
    public get ACCOUNT_EXP(): number { return this.accountexp }
    public set ACCOUNT_EXP(value: number) { this.accountexp = value }

    protected charexp: number = -1;
    public get CHAR_EXP(): number { return this.charexp }
    public set CHAR_EXP(value: number) { this.charexp = value }

    protected rewardgold: number = -1;
    public get REWARD_GOLD(): number { return this.rewardgold }
    public set REWARD_GOLD(value: number) { this.rewardgold = value }

    protected rewarditem: number = -1;
    public get REWARD_ITEM(): number { return this.rewarditem }
    public set REWARD_ITEM(value: number) { this.rewarditem = value }

    protected rewarditemcount: number = -1;
    public get REWARD_ITEM_COUNT(): number { return this.rewarditemcount }
    public set REWARD_ITEM_COUNT(value: number) { this.rewarditemcount = value }

    protected rewardstar: number[] = Array<number>(3);
    public get REWARD_STAR(): number[] { return this.rewardstar }
    public set REWARD_STAR(value: number[]) { this.rewardstar = value }

    protected chargecosttype: string = "";
    public get CHARGE_COST_TYPE(): string { return this.chargecosttype }
    public set CHARGE_COST_TYPE(value: string) { this.chargecosttype = value }

    protected chargecostvalue: number = -1;
    public get CHARGE_COST_VALUE(): number { return this.chargecostvalue }
    public set CHARGE_COST_VALUE(value: number) { this.chargecostvalue = value }

    protected chargecount: number = -1;
    public get CHARGE_COUNT(): number { return this.chargecount }
    public set CHARGE_COUNT(value: number) { this.chargecount = value }

    protected chargemax: number = -1;
    public get CHARGE_MAX(): number { return this.chargemax }
    public set CHARGE_MAX(value: number) { this.chargemax = value }

    protected unlockmission: number = -1;
    public get UNLOCK_MISSION(): number { return this.unlockmission }
    public set UNLOCK_MISSION(value: number) { this.unlockmission = value }
}