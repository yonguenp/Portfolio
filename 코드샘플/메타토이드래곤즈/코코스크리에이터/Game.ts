
import { _decorator, Component, director } from 'cc';
import { GameManager } from './GameManager';
import { SceneManager } from './SceneManager';
import { SoundMixer, SOUND_TYPE } from './SoundMixer';
import { TutorialManager } from './Tutorial/TutorialManager';
const { ccclass } = _decorator;

/**
 * Predefined variables
 * Name = Game
 * DateTime = Mon Jan 10 2022 15:42:11 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = Game.ts
 * FileBasenameNoExtension = Game
 * URL = db://assets/Scripts/Game.ts
 *
 */
 
@ccclass('Game')
export class Game extends Component {
    gameManager: GameManager = null;
    onLoad() 
    {
        this.gameManager = GameManager.Instance;
    }

    start() 
    {
        // SoundMixer.EnableMouseClickSFX(this.node)
    }

    update (deltaTime: number) {
        this.gameManager.Update(deltaTime);
    }

    onDisable()
    {
        // SoundMixer.DisableMouseClickSFX(this.node)
    }

    onClickAdventure()
    {
        SoundMixer.DestroyAllClip()
        SceneManager.SceneChange("WorldAdventure", () => { 
            // 튜토리얼 실행
            if (TutorialManager.GetInstance.IsNowPlayingTutorialByGroup(106)) {
                TutorialManager.GetInstance.OnTutorialEvent(106,3);
            }
        });
    }
}
