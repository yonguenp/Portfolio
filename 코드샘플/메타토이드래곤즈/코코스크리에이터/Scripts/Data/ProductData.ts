
import { _decorator, Component, Node } from 'cc';
import { DataBase } from './DataBase';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = ProductData
 * DateTime = Wed Jan 12 2022 16:08:58 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = ProductData.ts
 * FileBasenameNoExtension = ProductData
 * URL = db://assets/Scripts/Data/ProductData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */


export class ProductData extends DataBase {
    protected productkey : number = 0
    public get ProductKey()
    {
        return this.productkey;
    }
    public set ProductKey(value : number)
    {
        this.productkey = value
    }

    protected buildinglevel : number = 0
    public get BuildingLevel()
    {
        return this.buildinglevel;
    }
    public set BuildingLevel(value : number)
    {
        this.buildinglevel = value
    }

    protected productIcon : string = ""
    public get ProductIcon()
    {
        return this.productIcon;
    }
    public set ProductIcon(value : string)
    {
        this.productIcon = value
    }

    protected productitemID : number = 0
    public get ProductItemID()
    {
        return this.productitemID;
    }
    public set ProductItemID(value : number)
    {
        this.productitemID = value
    }

    protected productamount : number = 0
    public get ProductAmount()
    {
        return this.productamount;
    }
    public set ProductAmount(value : number)
    {
        this.productamount = value
    }

    protected productreqtime : number = 0
    public get ProductReqTime()
    {
        return this.productreqtime;
    }
    public set ProductReqTime(value : number)
    {
        this.productreqtime = value
    }

    protected needitemLength : number = 0
    public get NeedItemLength()
    {
        return this.needitemLength;
    }

    protected needitemID : number[] = []
    public get NeedItemIDArray()
    {
        return this.needitemID;
    }
    public set NeedItemIDArray(value : number[])
    {
        this.needitemLength = value.length
        this.needitemID = value
    }

    protected needitemAmount : number[] = []
    public get NeedItemAmountArray()
    {
        return this.needitemAmount;
    }
    public set NeedItemAmountArray(value : number[])
    {
        this.needitemAmount = value
    }

    protected needgold : number = 0
    public get NeedGold()
    {
        return this.needgold;
    }
    public set NeedGold(value : number)
    {
        this.needgold = value
    }
}
export class ProductAutoData extends DataBase {
    protected building_group : string = "";
    public get BUILDING_GROUP() : string
    {
        return this.building_group;
    }
    public set BUILDING_GROUP(value : string)
    {
        this.building_group = value
    }

    protected level : number = 0
    public get LEVEL() : number
    {
        return this.level;
    }
    public set LEVEL(value : number)
    {
        this.level = value
    }

    protected type : string = "";
    public get TYPE() : string
    {
        return this.type;
    }
    public set TYPE(value : string)
    {
        this.type = value
    }

    protected value : number = 0
    public get VALUE() : number
    {
        return this.value;
    }
    public set VALUE(value : number)
    {
        this.value = value
    }

    protected term : number = 0
    public get TERM() : number
    {
        return this.term;
    }
    public set TERM(value : number)
    {
        this.term = value
    }

    protected num : number = 0
    public get NUM() : number
    {
        return this.num;
    }
    public set NUM(value : number)
    {
        this.num = value
    }

    protected max_time : number = 0
    public get MAX_TIME() : number
    {
        return this.max_time;
    }
    public set MAX_TIME(value : number)
    {
        this.max_time = value
    }
}