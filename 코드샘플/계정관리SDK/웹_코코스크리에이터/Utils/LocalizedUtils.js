var PopupTemplate = require("PopupTemplate");
cc.Class({
    extends: cc.Component,

    properties: {
        LocalizedKey : '',        
    },

    onEnable : function() {   
        if(PopupTemplate.self == null)
        {
            cc.resources.load("Data/Popup", function (err, file) {            
                PopupTemplate.Init(file.text);
                this.applyLocalized();
            });
        }
        else
        {
            this.applyLocalized();
        }
    },

    applyLocalized : function()
    {
        var TargetLabel = this.node.getComponent(cc.Label);
        if(TargetLabel)
        {
            TargetLabel.string = PopupTemplate.GetText(this.LocalizedKey);
        }
    }
});
