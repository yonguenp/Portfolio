
import { _decorator, Vec3 } from 'cc';
import { BattleTileX } from '../Character/BattleData';
import { DragonDefaultSpeed } from '../Character/Dragon';
import { WaveBase, WaveEvent } from './WaveBase';

/**
 * Predefined variables
 * Name = WaveMove
 * DateTime = Thu Feb 17 2022 18:17:24 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = WaveMove.ts
 * FileBasenameNoExtension = WaveMove
 * URL = db://assets/Scripts/Battle/WaveMove.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
export enum eWaveMoveState {
    Start = 0,
    MoveFormation,
    MoveDragon,
    Delay,
    End,
    Exit
}
 
export class WaveMove extends WaveBase {
    private time = 1;
    private battleState: eWaveMoveState = eWaveMoveState.Start;
    private formationX: number = -1;
    GetID(): string {
        return 'WaveMove';
    }

    Start() {
        this.battleState = eWaveMoveState.Start;
        const dragonsCount = this.stageData.Dragons.length;

        for(var i = 0 ; i < dragonsCount ; i++) {
            let dragon = this.stageData.Dragons[i];
            if(dragon == null || dragon.Death) {
                continue;
            }
            dragon.Node.SetSpeed(DragonDefaultSpeed * this.stageData.StageSpeed);
        }
    }
    End(): any {
        return super.End();
    }
    
    Update(dt: number): string {
        this.stageData.UpdateCharacter(dt);
        this.stageData.UpdateTime(dt);
        switch(this.battleState) {
            case eWaveMoveState.Start: {
                this.time = 1;
                this.battleState = eWaveMoveState.MoveFormation;
                let dragons = this.stageData.Dragons;
                const dragonsCount = dragons.length;
                var tempPosX: number = 0; 
                var tempPosY: number = 0;
                var completeCount = 0;
                for(var i = 0 ; i < dragonsCount ; i++) {
                    let dragon = dragons[i];
                    if(dragon == null || dragon.Node == null || dragon.Death) {
                        continue;
                    }

                    const localPos = dragon.Node.node.getPosition();
                    tempPosX += localPos.x;
                    tempPosY += localPos.y;

                    if(dragon.Death) {
                        continue;
                    }

                    if(dragon.Node.node.position.x == dragon.StartPos.x && dragon.Node.node.position.y == dragon.StartPos.y) {
                        completeCount++;
                    }
                    dragon.Node.Controller.onEnterEvent('right');
                    dragon.Node.Controller.onController(0);
                    dragon.Node.AnimationStart('walk');
                    dragon.Node.Controller.onExitEvent('up');
                    dragon.Node.Controller.onExitEvent('down');
                    dragon.Node.Controller.onExitEvent('left');
                    dragon.Node.Controller.onExitEvent('right');
                }
                this.formationX = tempPosX / dragonsCount;
                if(completeCount == dragonsCount) {
                    this.battleState = eWaveMoveState.MoveDragon;
                }
            } break;
            case eWaveMoveState.MoveFormation: {
                this.stageData.MoveRight(dt * 0.75);
                const dragonsCount = this.stageData.Dragons.length;
        
                var completeCount = 0;
                for(var i = 0 ; i < dragonsCount ; i++) {
                    let dragon = this.stageData.Dragons[i];
                    if(dragon == null || dragon.Death) {
                        completeCount++;
                        continue;
                    }
                    
                    var dirX = 0;
                    var dirY = 0;
                    const goalX = this.formationX + ((dragonsCount - 1) * 0.5 - dragon.Pos.x) * BattleTileX;
                    const goalY = dragon.StartPos.y;
                    const goalZ = dragon.StartPos.z;
                    let curMove = dragon.Node.node.getPosition();
                    const curPosX = curMove.x;
                    const curPosY = curMove.y;
                    if(curPosX == goalX && curPosY == goalY) {
                        completeCount++;
                        continue;
                    } else if(curPosX >= goalX) {
                        dirX = 1;
                    } else if(curPosX < goalX) {
                        dirX = 2;
                    }
                    if(curPosY >= goalY) {
                        dirY = 1;
                    } else if(curPosY < goalY) {
                        dirY = 2;
                    }
                    // this.goalX = move.x;
                    // this.goalY = move.y;
                    // const curPos = this.target.GetWorldPosition();
                    // var number = 0;
                    // if(curPos.x >= this.goalX) {
                    //     number = number | eMoveState.Left;
                    // } else if(curPos.x < this.goalX) {
                    //     number = number | eMoveState.Right;
                    // }
                    dragon.Node.AnimationStart('walk');
                    dragon.Node.SetSpeed(DragonDefaultSpeed * this.stageData.StageSpeed);
                    dragon.Node.Controller.MoveTarget(dt, new Vec3(goalX, goalY, goalZ), false, 100 * this.stageData.StageSpeed);
                    
                    var isGoalX = false;
                    var isGoalY = false;
                    curMove = dragon.Node.node.getPosition();
                    const curMoveX = curMove.x;
                    const curMoveY = curMove.y;
                    if(dirX == 2 && curMoveX >= goalX) {
                        isGoalX = true;
                    } else if(dirX == 1 && curMoveX <= goalX) {
                        isGoalX = true;
                    }
                    if(dirY == 2 && curMoveY >= goalY) {
                        isGoalY = true;
                    } else if(dirY == 1 && curMoveY <= goalY) {
                        isGoalY = true;
                    }
        
                    if(isGoalY) {
                        dragon.Node.Controller.onExitEvent('up');
                        dragon.Node.Controller.onExitEvent('down');
                        var temp = new Vec3(dragon.Node.node.position);
                        temp.y = goalY;
                        dragon.Node.node.position = temp;
                    }
                    if(isGoalX) {
                        dragon.Node.Controller.onExitEvent('left');
                        dragon.Node.Controller.onEnterEvent('right');
                        dragon.Node.Controller.onController(0);
                        dragon.Node.Controller.onExitEvent('right');
                        var temp = new Vec3(dragon.Node.node.position);
                        temp.x = goalX;
                        dragon.Node.node.position = temp;
                    }
                    if(isGoalX && isGoalY) {
                        completeCount++;
                    }
                }
        
                if(completeCount == dragonsCount) {
                    this.battleState = eWaveMoveState.MoveDragon;
                }
            } break;
            case eWaveMoveState.MoveDragon: {
                this.stageData.MoveRight(dt * 1.75);
                const dragonsCount = this.stageData.Dragons.length;
        
                var completeCount = 0;
                for(var i = 0 ; i < dragonsCount ; i++) {
                    let dragon = this.stageData.Dragons[i];
                    if(dragon == null || dragon.Death) {
                        completeCount++;
                        continue;
                    }
                    
                    var dirX = 0;
                    const goalX = dragon.StartPos.x;
                    const curPos = dragon.Node.node.position;
                    if(curPos.x == goalX) {
                        completeCount++;
                        continue;
                    } else if(curPos.x >= goalX) {
                        dirX = 1;
                        dragon.Node.AnimationStart('walk');
                        dragon.Node.Controller.onEnterEvent('left');
                        dragon.Node.Controller.onExitEvent('right');
                    } else if(curPos.x < goalX) {
                        dirX = 2;
                        dragon.Node.AnimationStart('walk');
                        dragon.Node.Controller.onEnterEvent('right');
                        dragon.Node.Controller.onExitEvent('left');
                    };
                    dragon.Node.SetSpeed(DragonDefaultSpeed * this.stageData.StageSpeed);
                    dragon.Node.Controller.onController(dt, false, 100 * this.stageData.StageSpeed);
                    
                    var isGoalX = false;
                    const curMovePos = dragon.Node.node.position;
                    if(dirX == 2 && curMovePos.x >= goalX) {
                        isGoalX = true;
                    } else if(dirX == 1 && curMovePos.x <= goalX) {
                        isGoalX = true;
                    }
        
                    if(isGoalX) {
                        completeCount++;
                        dragon.Node.Controller.onExitEvent('left');
                        dragon.Node.Controller.onEnterEvent('right');
                        dragon.Node.Controller.onController(0);
                        dragon.Node.Controller.onExitEvent('right');
                        dragon.Node.node.setPosition(new Vec3(goalX, curMovePos.y, curMovePos.z));
                    }
                }
        
                if(completeCount == dragonsCount) {
                    this.battleState = eWaveMoveState.Delay;
                }
            } break;
            case eWaveMoveState.Delay: {
                this.stageData.MoveRight(dt);
                this.time -= dt;
                if(this.time <= 0 && this.stageData.IsWaveStart()) {
                    this.battleState = eWaveMoveState.End;
                    return "";
                }
                this.stageData.UpdateCharacter(dt);
            } break;
            case eWaveMoveState.End: {
                this.stageData.MoveRight(dt);
                this.battleState = eWaveMoveState.Exit;
                WaveEvent.TriggerEvent({ state: 'WaveBattle' });
            } break;
        }
        return "";
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
