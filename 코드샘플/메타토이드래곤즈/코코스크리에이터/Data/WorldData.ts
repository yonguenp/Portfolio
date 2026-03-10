
import { _decorator } from 'cc';
import { DataBase } from './DataBase';

export class WorldData extends DataBase 
{
    protected num: number = -1;
    public get NUM(): number {
        return this.num;
    }
    public set NUM(value: number) {
        this.num = value;
    }

    protected name: number = -1;
    public get NAME(): number {
        return this.name;
    }
    public set NAME(value: number) {
        this.name = value;
    }

    protected background: string = "";
    public get BACKGROUND(): string {
        return this.background;
    }
    public set BACKGROUND(value: string) {
        this.background = value;
    }

    protected image: string = "";
    public get IMAGE(): string {
        return this.image;
    }
    public set IMAGE(value: string) {
        this.image = value;
    }

    protected star: number[] = Array<number>(3);
    public get STAR(): number[] {
        return this.star;
    }
    public set STAR(value: number[]) {
        this.star = value;
    }

    protected star_rewrad: number[] = Array<number>(3);
    public get STAR_REWARD(): number[] {
        return this.star_rewrad;
    }
    public set STAR_REWARD(value: number[]) {
        this.star_rewrad = value;
    }
}