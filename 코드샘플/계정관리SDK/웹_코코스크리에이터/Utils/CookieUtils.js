/**
 * cookie cooker
 */
module.exports = {
    setCookieUntil : function(key, value, expireMSec) {
        var exp;
        if ('string' === typeof expireMSec) {
            exp = expireMSec;
        } else {
            var date = new Date();
            date.setTime(expireMSec);
            exp = date.toUTCString();
        }

        // double safe
        if ('number' === typeof value) {
            value = value.toString();
        } else if ('string' !== typeof value) {
            value = '';
        }

        document.cookie = key + '=' + value + ';Expires=' + exp + 
                        ';Path=/';//;SameSite=None;Secure;';
                        //';Path=/;SameSite=None;Secure;';

        require('AppUtils').SetAppData(key, value);
    },

    setCookieValidMonth : function(key, value)
    {
        return this.setCookie(key, value, 30 * 24 * 60 * 60 * 1000);
    },

    setCookie : function(key, value, remainMSec) {
        return this.setCookieUntil(key, value,
                                new Date().getTime() + remainMSec);
    },

    getCookie : function(key) {
        var match = document.cookie.match(`(^|;) ?${key}=([^;]*)(;|$)`);
        return (match && 2 < match.length) ? match[2] : null;
    },

    eraseCookie : function(name) {
        document.cookie = name + '=;Max-Age=-99999999;';
    },

    getKrTomorrowInUTC : function() {
        // KTC 시차
        const ktcOffset = -9;
        
        // 로컬 date
        var d = new Date();
        // 로컬이 한국보다 몇 시간 느린가
        var timeDiff = d.getTimezoneOffset() / 60 - ktcOffset;
        
        // 한국 24시는 로컬 시각으로 24 - diff
        var newHours = 24 - diff;
        // 이미 지나갔다면 하루 추가
        if (d.getHours() >= newHours) {
            newHours += 24;
        }

        d.setHours(newHours, 0, 0, 0);
        return d.toUTCString();
    },

    getLocalTomorrowInUTC : function() {
        var d = new Date();
        d.setHours(24, 0, 0, 0);
        return d.toUTCString();
    },

    clearUserCookies : function() {
        this.eraseCookie('tok');        
    }
}