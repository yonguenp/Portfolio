
import { _decorator, Component, Node, Prefab, instantiate } from 'cc';
import { PartOptionTable } from '../Data/PartOptionTable';
import { StringTable } from '../Data/StringTable';
import { TableManager } from '../Data/TableManager';
import { User, UserPart } from '../User/User';
import { Popup } from './Common/Popup';
import { DragonPartInfoSlot } from './DragonPartInfoSlot';
const { ccclass, property } = _decorator;

/**
 * Predefined variables
 * Name = DragonPartInfoAllPopup
 * DateTime = Tue Mar 29 2022 10:40:30 GMT+0900 (대한민국 표준시)
 * Author = wonjun_gs
 * FileBasename = DragonPartInfoAllPopup.ts
 * FileBasenameNoExtension = DragonPartInfoAllPopup
 * URL = db://assets/Scripts/UI/DragonPartInfoAllPopup.ts
 * ManualUrl = https://docs.cocos.com/creator/3.4/manual/en/
 *
 */
 /**
  * 드래곤 능력치 정보에서 돋보기(상세보기)클릭시, 능력 정보 팝업 표시 UI
  * 드래곤 파츠의 총 능력치가 몇 개가 될지 몰라서 스탯 갯수를 받아온 다음에 슬롯 프리펩으로 데이터 세팅 따로 해서
  * 스크롤뷰 형식으로 세팅함.
  */

 export enum StatTypePriority
 {
     ATK,
     DEF,
     HP,
     CRITICAL,
 }

@ccclass('DragonPartInfoAllPopup')
export class DragonPartInfoAllPopup extends Popup {
    
    @property(Node)
    contentNode : Node = null;

    @property(Prefab)
    slotPrefab : Prefab = null;

    partOptionTable : PartOptionTable = null;
    stringTable : StringTable = null;
    Init(data?: any)
    {
        if(this.partOptionTable == null){
            this.partOptionTable = TableManager.GetTable<PartOptionTable>(PartOptionTable.Name);
        }

        if(this.stringTable == null){
            this.stringTable =  TableManager.GetTable<StringTable>(StringTable.Name);
        }

        let tempDragonTag = data.dragonTag;//현재 드래곤 태그값이 넘어옴
        let DragonData = User.Instance.DragonData.GetDragon(tempDragonTag);
        if(DragonData == null){
            return;
        }

        let dragonPartsList = DragonData.GetPartsList();//size 6배열 고정으로 날아옴
        this.SetDetailData(dragonPartsList);

        super.Init(data);
    }

    //모든 파츠 데이터의 옵션 정제하기 - 임시 객체 빼서 전체로 때려넣는 로직 필요
    SetDetailData(userpartList : UserPart[])
    {
        if(userpartList == null || userpartList.length <= 0){
            return;
        }

        let allPartData = {};//각 타입 기준으로 생성된 스탯 스크롤 프리펩 세팅

        userpartList.forEach((element)=>{
            if(element == null){
                return;
            }
            let partDesignData = element.GetPartDesignData();
            if(partDesignData == null){
                return;
            }

            let stat_type_main = partDesignData.STAT_TYPE;
            let mainPartValue = element.GetValue();

            let tempData = allPartData[stat_type_main] as number;
            if(tempData != undefined)
            {
                allPartData[stat_type_main] = (mainPartValue + tempData);
            }
            else{
                allPartData[stat_type_main] = mainPartValue;
            }
            

            let subOption = element.PartOptionList;
            let subOptionKey = Object.keys(subOption);
            subOptionKey.forEach((key)=>{
                let PartOptionData = this.partOptionTable.Get(key);
                if(PartOptionData == null){
                    return;
                }

                let stat_type_sub = PartOptionData.STAT_TYPE;
                let subOptionValue = subOptionKey[key] as number;

                let tempSubData = allPartData[stat_type_sub] as number;
                if(tempSubData != undefined){
                    allPartData[stat_type_sub] = (subOptionValue + tempSubData);
                }
                else{
                    allPartData[stat_type_sub] = subOptionValue;
                }
                
            });
        });

        //---- make prefab
        if(this.contentNode == null || this.slotPrefab == null){
            return;
        }

        this.contentNode.removeAllChildren();

        let arr = [];
        Object.keys(allPartData).map(function(key){  
            arr.push({[key]:allPartData[key]})  
            return arr;  
        });  

        let sortAllpartData = arr.sort(function(a,b){
            let akey = Object.keys(a)[0];
            let bkey = Object.keys(b)[0];

            let sortFunc = (statTypeStr)=>{
                let tempNumber = -1;
                switch(statTypeStr)
                {
                    case ('ATK' || 'atk'):
                        tempNumber = Number(StatTypePriority.ATK);
                        break;
                    case ('DEF' || 'def'):
                        tempNumber = Number(StatTypePriority.DEF);
                        break;
                    case ('HP' || 'hp'):
                        tempNumber = Number(StatTypePriority.DEF);
                        break;
                    case ('CRI' || 'cri' || 'critical' || 'CRITICAL'):
                        tempNumber = Number(StatTypePriority.CRITICAL);
                        break;
                    default:
                        break;
                }
                return tempNumber;
            };
            
            let aPriority = sortFunc(akey);
            let bPriority = sortFunc(bkey);

            return aPriority - bPriority;
        });

        if(sortAllpartData == null || sortAllpartData.length <= 0){
            return;
        }

        sortAllpartData.forEach((element)=>{
            let statStr = Object.keys(element)[0];
            let statvalue = element[statStr];

            let clone = instantiate(this.slotPrefab);
            clone.parent = this.contentNode;

            let modifyStatStr = this.GetStringByType(statStr);
            clone.getComponent(DragonPartInfoSlot).SetData(modifyStatStr,statvalue as number);
        });

        // let allStatDataKeys = Object.keys(allPartData);//key 가 stat_type str 값
        // allStatDataKeys.forEach((allpartElement)=>{
        //     let statvalue = allPartData[allpartElement];
        //     let statStr = allpartElement;

        //     let clone = instantiate(this.slotPrefab);
        //     clone.parent = this.contentNode;

        //     let modifyStatStr = this.GetStringByType(statStr);
        //     clone.getComponent(DragonPartInfoSlot).SetData(modifyStatStr,statvalue as number);
        // });
    }

    GetStringByType(statTypeStr : string) : string
    {
        let tempStr = statTypeStr;
        if(this.stringTable == null)
        {
            return tempStr;
        }

        switch(statTypeStr)
        {
            case ('ATK' || 'atk'):
                tempStr = this.stringTable.Get(100000178).TEXT
                break;
            case ('DEF' || 'def'):
                tempStr = this.stringTable.Get(100000179).TEXT
                break;
            case ('HP' || 'hp'):
                tempStr = this.stringTable.Get(100000180).TEXT
                break;
            case ('CRI' || 'cri'):
                tempStr = this.stringTable.Get(100000181).TEXT
                break;
            default:
                break;
        }
        return tempStr;
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
