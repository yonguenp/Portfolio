
import { _decorator, Component, Label } from 'cc';
const { ccclass } = _decorator;

@ccclass('DamageObject')
export class DamageObject extends Component 
{
    get GetLabel() : Label
    {

        return this.getComponent(Label)
    }
}