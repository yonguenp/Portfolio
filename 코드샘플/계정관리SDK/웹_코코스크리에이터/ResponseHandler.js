/**
 * 표준형 리스폰스 핸들러
 */

var MessagePopup = require("MessagePopup");
var Samanda = require("Samanda");

// api type enum
var eApiType = cc.Enum({
    AUTH: 1,
    PROFILE: 2,
    NOTIFICATION: 3,    
});

module.exports = function(resp) {
    if ('object' === typeof resp) {
        var data = resp;
    } else {
        // parse JSON string -> object
        try {
            var data = JSON.parse(resp);
        } catch (e) {
            console.log('Syntax error in response body.');
            MessagePopup.openMessageBoxWithKey("POPUP_52");
            return;
        }
    }

    // total error?
    var rs = 99;
    if ('rs' in data && Number.isInteger(data.rs)) {
        var rs = data.rs;
    }    
    // if (false === "rs" in data || 0 !== data.rs) {
    //     let msg = data.msg || "서버 오류.";
    //     MessagePopup.openMessageBox(msg);
    //     return;
    // }

    if (false === data.hasOwnProperty("apis") ||
        false === Array.isArray(data["apis"])) {
        console.log("No api fields")
        return;
    }
    
    for (let i = 0; data.apis.length > i; ++i) {
        let row = data.apis[i];

        if ('object' !== typeof row || null === row) {
            console.log('invalid array item at idx #' + i);
            continue;
        }

        var api_type = parseInt(row["type"]);
        switch (api_type) {
            case eApiType.AUTH: {
                
                break;
            }
            case eApiType.PROFILE: {

                break;
            }                    
            case eApiType.NOTIFICATION: {
                if (0 !== rs) {
                    let msg = data.msg || "서버 오류.";
                    MessagePopup.openMessageBox(msg);
                    return;
                }

                Samanda.onResNotiBannerList(row);
                
                break;
            }
            
            default:
                break;
        }
    }
};