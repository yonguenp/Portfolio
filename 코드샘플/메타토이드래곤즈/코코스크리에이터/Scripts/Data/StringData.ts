
import { _decorator, Component, Node } from 'cc';
import { DataBase } from './DataBase';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = StringData
 * DateTime = Tue Jan 11 2022 16:23:19 GMT+0900 (대한민국 표준시)
 * Author = ahnhyeon5017
 * FileBasename = StringData.ts
 * FileBasenameNoExtension = StringData
 * URL = db://assets/Scripts/Data/StringData.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class StringData extends DataBase {
    protected type: number = -1;
    public get TYPE(): number {
        return this.type;
    }
    public set TYPE(value: number) {
        this.type = value;
    }
    protected kor: string = "";
    public get KOR(): string {
        return this.kor;
    }
    public set KOR(value: string) {
        this.kor = value;
    }
    protected eng: string = "";
    public get ENG(): string {
        return this.kor;
    }
    public set ENG(value: string) {
        this.eng = value;
    }
    public get TEXT(): string {
        // Setting 데이터에 따른 처리적용
        let languageState = navigator.language;
        
        var LanguageData = localStorage.getItem('Setting_Language');
        if (LanguageData != null && LanguageData != undefined) {
            switch(LanguageData) {
                case 'kor':
                    languageState = "kor";
                    break;
                case 'eng':
                    languageState = "eng"
                    break;
                default:
            }
        }

        switch (languageState) {
            case "ko":
            case "kor":
            case "ko-KR":
                return this.kor;
            default:
                return this.eng;
        }
    }
}