/**
 * Predefined variables
 * Name = Ellipse
 * DateTime = Tue May 03 2022 17:49:48 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = Ellipse.ts
 * FileBasenameNoExtension = Ellipse
 * URL = db://assets/Scripts/Tools/Ellipse.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */

import { Vec2, Vec3 } from "cc";

export class SBObject {
    protected position: Vec3 = Vec3.ZERO;

    constructor(position: Vec3) {
        this.SetPosition(position);
    }

    SetPosition(position: Vec3): void {
        this.position = position;
    }
}

enum eCircleType {
    Circle,
    XEllipse,
    YEllipse
}

export class Circle extends SBObject {
    protected radiusType: eCircleType = eCircleType.Circle;

    protected radius: number = 0;
    protected radiusX: number = 0;
    protected radiusY: number = 0;

    protected f1: Vec3 = Vec3.ZERO;
    protected f2: Vec3 = Vec3.ZERO;

    constructor(position: Vec3, radiusX: number, radiusY: number) {
        super(position);
        this.SetEllipse(radiusX, radiusY)

    }

    SetEllipse(radiusX: number, radiusY: number, position: Vec3 = null): void {
        if(position == null) {
            position = this.position;
        } else {
            this.SetPosition(position);
        }

        this.radiusX = radiusX;
        this.radiusY = radiusY;

        this.f1 = new Vec3(position);
        this.f2 = new Vec3(position);
        if(radiusX == radiusY) {
            this.radius = radiusX;
            this.radiusType = eCircleType.Circle;
        } else if(radiusX > radiusY) {
            this.radius = radiusX;
            this.radiusType = eCircleType.XEllipse;
            let f = Math.sqrt(radiusX * radiusX - radiusY * radiusY);
            this.f1.x -= f;
            this.f2.x += f;
        } else {
            this.radius = radiusY;
            this.radiusType = eCircleType.YEllipse;
            let f = Math.sqrt(radiusY * radiusY - radiusX * radiusX);
            this.f1.y -= f;
            this.f2.y += f;
        }
    }

    IsContain(target: Vec3): boolean {
        switch(this.radiusType) {
            case eCircleType.Circle: {
                const distanceF1 = Vec2.distance(this.position, target);
                const x = Math.abs(this.position.x - target.x);
                const y = Math.abs(this.position.y - target.y);

                if((distanceF1 * distanceF1) >= (x * x + y * y)) {
                    return true;
                }
            } break;
            case eCircleType.XEllipse:
            case eCircleType.YEllipse: {
                const distanceF1 = Vec2.distance(target, this.f1);
                const distanceF2 = Vec2.distance(target, this.f2);
                if((distanceF1 + distanceF2) <= (2 * this.radius)) {
                    return true;
                }
            } break;
        }

        return false;
    }
}

/**
 * [1] Class member could be defined like this.
 * [2] Use `property` decorator if your want the member to be serializable.
 * [3] Your initialization goes here.
 * [4] Your update function goes here.
 *
 * Learn more about scripting: https://docs.cocos.com/creator/3.4/manual/en/scripting/
 * Learn more about CCClass: https://docs.cocos.com/creator/3.4/manual/en/scripting/decorator.html
 * Learn more about life-cycle callbacks: https://docs.cocos.com/creator/3.4/manual/en/scripting/life-cycle-callbacks.html
 */
