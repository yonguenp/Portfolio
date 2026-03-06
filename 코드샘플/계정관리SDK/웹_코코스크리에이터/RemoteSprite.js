cc.Class({
    extends: cc.Sprite,

    properties: {
        texture_url: {
            default: '',            
            tooltip: 'Enter TextureUrl'           
        },
        remoteCall: false,
    },

    start () {
        
    },

    onLoad () {   
        if(this.texture_url && !this.remoteCall)     
        {
            this.setRemoteSprite(this.texture_url);
        }
    },

    setRemoteSprite : function(url, callback = null, useProductURL = true)
    {
        this.remoteCall = true;
        this.texture_url = url;
        this.loadedcallback = callback;

        this.default_url = "../resources/";
        if (CC_DEBUG) {
            this.default_url = Samanda.getSamandaUrl() + 'resources/';            
        }
        
        if(useProductURL)
        {
            try {
                var NetworkManager = require("NetworkManager");

                console.log(NetworkManager);

                if(NetworkManager.product_type === undefined || NetworkManager.product_type == null)
                    this.default_url += "0/";
                else
                    this.default_url += NetworkManager.product_type + "/";
            }
            catch(e)
            {
                console.log('[error] setRemoteSprite useProductURL : ' + e);                
            }
        
        }
        
        if(this.texture_url)
        {
            console.log('[load] try remote srpite start : ' + this.default_url + this.texture_url);
            var loadCallBack = this._loadCallBack.bind(this);
            
            cc.assetManager.loadRemote(this.default_url + this.texture_url, loadCallBack);
        }
        else
        {
            this.spriteFrame = null;
            if(this.loadedcallback)
                this.loadedcallback();
            console.log('[error]check url : ' + this.default_url + this.texture_url);
        }
    },

    _loadCallBack: function (err, res) {
        if (err) {
            console.log('[error]loadRemoteTexture : ' + this.default_url + this.texture_url + ' | ' + err)
            return;
        }

        console.log('[load] done');        
        this.spriteFrame = new cc.SpriteFrame(res);

        if(this.loadedcallback)
            this.loadedcallback();
    },

    // update (dt) {},
});
