const Samanda = require("./Samanda");

const POPUP_DATA = class
{
    static self = null;
    PopupInfo = null;
    LoadDone = false;

    constructor() {  
        
    }
    
    GetPopupData(key)
    {
        if(this.PopupInfo)
        {
            return this.PopupInfo.get(key);
        }
        
        return null;
    }

    static Get(key) {        
        if (null === POPUP_DATA.self) {
            return null;
        }

        return POPUP_DATA.self.GetPopupData(key);
    }

    static GetText(key) {
        if (null === POPUP_DATA.self) {
            return key;
        }

        var data =  POPUP_DATA.self.GetPopupData(key);
        if(data == null)
            return key;
        var index = 10;
        switch(Samanda.getLanguageCode())
        {
            case 'en':
                index = 12;
                break;
            case 'ko':
                index = 4;
                break;
        }
        return data[index];
    }

    static Init(data)
    {
        if (null === POPUP_DATA.self) {
            console.log('POPUP_DATA INIT START');
            POPUP_DATA.self = new POPUP_DATA(); 
            
            POPUP_DATA.self.PopupInfo = new Map();
            var lines = data.split('\n');
        
            lines.forEach(function(line) {
                var vals = line.split('\t');            
                
                while(vals.length <= 14)
                {
                    vals.push('');
                }

                //kr
                vals[4] = vals[4].trim();
                vals[4] = vals[4].replace(/\\n/gi, '\n');
                vals[3] = vals[3].trim();
                vals[3] = vals[3].replace(/\\n/gi, '\n');
                //en
                vals[12] = vals[12].trim();
                vals[12] = vals[12].replace(/\\n/gi, '\n');
                vals[13] = vals[13].trim();
                vals[13] = vals[13].replace(/\\n/gi, '\n');

                if(vals[14].length === 0 || !vals[14].trim())
                {
                    vals[14] = vals[1];
                }
                //console.log('vals : ' + vals);
                POPUP_DATA.self.PopupInfo.set(vals[1], vals);
            }.bind(this));

            POPUP_DATA.self.LoadDone = true;
            console.log('POPUP_DATA INIT DONE');
        }
    }
};

module.exports = POPUP_DATA;

cc.Class({
    extends: cc.Component,

    properties: {
        Background : cc.Sprite,

        Title : cc.Label,
        Message : cc.Label,
        Info : cc.Label,

        EditBox1 : cc.Node,
        EditBox2 : cc.Node,
        EditBox3 : cc.Node,

        YesBtn : cc.Button,
        NoBtn : cc.Button,
    },

    start () {

    },

    setPopupWithData : function(key, param)
    {
        this.Background.node.width = this.node.parent.width * 2;
        this.Background.node.height = this.node.parent.height * 2;

        var data = POPUP_DATA.Get(key);
        
        if(data == null)
        {
            console.log('' + key + ' is null');
            data = new Array();
            data.push('');
            data.push(key);
            data.push('0');
            data.push('');
            data.push(key);
            data.push('');
            data.push('');
            data.push('');
            data.push('');
            data.push('');
            data.push('0');
            data.push('');
            data.push(key);
            data.push(key);
            data.push('');
        }
    
        var type = parseInt(data[2]);
        
        var msg_index = 12;
        var title_index = 13;
        switch(Samanda.getLanguageCode())
        {
            case 'en':
                msg_index = 12;
                title_index = 13;
                break;
            case 'ko':
                msg_index = 4;
                title_index = 3;
                break;
        }
        
        this.Title.string = data[title_index];
        this.Title.node.active = data[title_index];

        this.Message.string = data[msg_index];
        this.Message.node.active = data[msg_index];
            
        if(type & 1)
        {
            this.Info.string = param ? param : data[5];
            this.Info.node.active = data[5];
        }    
        
        if(type & 2)
        {
            this.EditBox1.active = true;
            this.EditBox1.getChildByName('EditBox_1').getComponent(cc.EditBox).placeholder = data[6];
        }    
        
        if(type & 4)
        {
            this.EditBox2.active = true;
            this.EditBox2.getChildByName('EditBox_1').getComponent(cc.EditBox).placeholder = data[6];
            this.EditBox2.getChildByName('EditBox_2').getComponent(cc.EditBox).placeholder = data[7];
        }
        
        if(type & 8)
        {
            this.EditBox3.active = true;
            this.EditBox3.getChildByName('EditBox_1').getComponent(cc.EditBox).placeholder = data[6];
            this.EditBox3.getChildByName('EditBox_2').getComponent(cc.EditBox).placeholder = data[7];
            this.EditBox3.getChildByName('EditBox_3').getComponent(cc.EditBox).placeholder = data[8];
        }

        var btnType = parseInt(data[10]);
        switch(btnType)
        {
            case 0:
                this.YesBtn.node.active = true;
                break;
            case 1:
                this.YesBtn.node.active = true;
                this.NoBtn.node.active = true;
                break;
        }

    },

    onPressExit : function()
    {        
        Samanda.cancelPopup = null;
        if (this.close_callback1 != null && "function" === typeof this.close_callback1) {
            this.close_callback1(this);
        }
        this.node.destroy();
    },

    onPressYes : function()
    {        
        Samanda.cancelPopup = null;
        if (this.close_callback1 != null && "function" === typeof this.close_callback1) {
            this.close_callback1(this);
        }
        this.node.destroy();
    },

    onPressNo : function()
    {        
        Samanda.cancelPopup = null;
        if (this.close_callback2 != null && "function" === typeof this.close_callback2) {
            this.close_callback2(this);
        }
        this.node.destroy();
    },

    onCancel : function()
    {
        if(this.NoBtn.node.active == true)
            this.onPressNo();
        else
            this.onPressExit();
    },
});
