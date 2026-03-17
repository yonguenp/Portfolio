import { DataBase } from './DataBase';

/**
 * Predefined variables
 * Name = ElementData
 * DateTime = Mon Feb 21 2022 15:55:41 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = ElementData.ts
 * FileBasenameNoExtension = ElementData
 * URL = db://assets/Scripts/Data/ElementData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */

export enum eElementType {
    None = 0,
    FIRE = 1,
    WATER = 2,
    EARTH = 3,
    WIND = 4,
    LIGHT = 5,
    DARK = 6
}

export class ElementRateData extends DataBase {
    protected a_element: string = "";
    public get A_ELEMENT(): string {
        return this.a_element;
    }
    public set A_ELEMENT(value: string) {
        this.a_element = value;
    }
    protected t_fire: number = -1;
    public get T_FIRE(): number {
        return this.t_fire;
    }
    public set T_FIRE(value: number) {
        this.t_fire = value;
    }
    protected t_water: number = -1;
    public get T_WATER(): number {
        return this.t_water;
    }
    public set T_WATER(value: number) {
        this.t_water = value;
    }
    protected t_earth: number = -1;
    public get T_EARTH(): number {
        return this.t_earth;
    }
    public set T_EARTH(value: number) {
        this.t_earth = value;
    }
    protected t_wind: number = -1;
    public get T_WIND(): number {
        return this.t_wind;
    }
    public set T_WIND(value: number) {
        this.t_wind = value;
    }
    protected t_light: number = -1;
    public get T_LIGHT(): number {
        return this.t_light;
    }
    public set T_LIGHT(value: number) {
        this.t_light = value;
    }
    protected t_dark: number = -1;
    public get T_DARK(): number {
        return this.t_dark;
    }
    public set T_DARK(value: number) {
        this.t_dark = value;
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
