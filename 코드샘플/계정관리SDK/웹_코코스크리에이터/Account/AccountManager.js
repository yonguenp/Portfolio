
const eAccountType = cc.Enum({
    SANDBOX_ACCOUNT: 0,
	GOOGLE_ACCOUNT: 1,
	FACEBOOK_ACCOUNT: 2,
	NAVER_ACCOUNT: 3,
	KAKAO_ACCOUNT: 4,
    APPLE_ACCOUNT: 5,
    INSTAGRAM_ACCOUNT: 6,
    GUEST_ACCOUNT: 7,
    
	NONE_ACCOUNT: 8
});

const AccountData = class
{
    has_innocentaccount = false;

    user_session_token = '';

    user_token = '';
    user_nick_name = '';
    user_account_no = '';
    user_account_type = eAccountType.NONE_ACCOUNT;    
    
    email_state = -1;
    email_address = '';
    links_platform = new Array();
    profile_img = '';

    ClearData()
    {
        this.user_token = '';
        this.user_account_no = '';
        this.SetNickName('');
        this.user_account_type = eAccountType.NONE_ACCOUNT;    

        this.has_innocentaccount = false;

        this.ClearSubData();
    }

    ClearSubData()
    {
        this.email_state = -1;
        this.email_address = '';
        this.SetAccountLink(new Array());
        this.profile_img = '';
    }

    SetNickName(nick)
    {
        this.user_nick_name = nick;
        require('AppUtils').sendNickName(this.user_nick_name);
    }

    SetAccountLink(links)
    {
        this.links_platform = links;
        require('AppUtils').sendLinks(this.links_platform);
    }

    PushAccountLink(link)
    {
        this.links_platform.push(link);
        require('AppUtils').sendLinks(this.links_platform);
    }

    SetAccountData(token, account_no, account_type, nick)
    {
        this.ClearData();

        this.user_token = token;
        this.SetNickName(nick);
        this.user_account_no = account_no;        
        this.user_account_type = account_type;

        this.has_innocentaccount = this.InnocentAccountCheck(); 
        if(!this.has_innocentaccount)
        {
            console.log('clear data : InnocentAccountCheck is false');
            this.ClearData();
        }    
    }

    InnocentAccountCheck()
    {
        if(!this.user_token)
        {
            console.log('[InnocentAccountCheck] - user_token: ' + this.user_token);
            return false;
        }
        if(!this.user_account_no)
        {
            console.log('[InnocentAccountCheck] - user_account_no: ' + this.user_account_no);
            return false;
        }
        if(!this.user_nick_name)
        {   
            console.log('[InnocentAccountCheck] - user_nick_name: ' + this.user_nick_name);
            return false;
        }
        if(this.user_account_type === eAccountType.NONE_ACCOUNT)
        {   
            console.log('[InnocentAccountCheck] - user_account_type: ' + eAccountType.NONE_ACCOUNT + this.user_account_type);
            return false;
        }

        return true;            
    }

    
}

const AccountInfo = class
{
    static self = null;
    data = new AccountData();

    static getInstance() {
        if (null === AccountInfo.self) {
            var self = new AccountInfo();
            AccountInfo.self = self;
        }
        
        return AccountInfo.self;
    }

    SetAccountData(data)
    {
        this.data.SetAccountData(data["tok"], data["ano"], data["typ"], data["nic"]);        
        if(data.hasOwnProperty("acc") && this.data.has_innocentaccount)
        {
            var acc_data = data["acc"];
            var email_state = acc_data.hasOwnProperty("email_state") ? parseInt(acc_data["email_state"]) : -1;
            var email_address = acc_data.hasOwnProperty("email") ? acc_data["email"] : '';            
            var links = new Array();
            if(acc_data.hasOwnProperty("links")) 
            { 
                acc_data["links"].forEach(l =>{
                    links.push(l);
                });
            }
            var profile_img = acc_data.hasOwnProperty("profile_img") ? acc_data["profile_img"] : '';
            
            this.data.email_state = email_state;
            this.data.email_address = email_address;
            this.data.SetAccountLink(links);
            this.data.profile_img = profile_img;
        }

        console.log('has_innocentaccount : ' + this.data.has_innocentaccount);
    }

    ClearData()
    {
        this.data.ClearData();
    }

    MakeAccountFormData()
    {
        var formData = new FormData();

        this.AppendFormData(formData);

        return formData;
    }

    AppendFormData(formData)
    {
        if(this.data.has_innocentaccount)
        {
            formData.append('ano', this.data.user_account_no);
            formData.append('tok', this.data.user_token);
            formData.append('nic', this.data.user_nick_name);
            formData.append('typ', this.data.user_account_type);
        }
        else
        {
            this.data.InnocentAccountCheck();
            console.log('AppendFormData fail');
        }
    }

    AppendHttpParam(url)
    {
        if(this.data.has_innocentaccount)
        {
            url += '?ano=' + this.data.user_account_no + '&tok=' + this.data.user_token + '&nic=' + this.data.user_nick_name + '&typ=' + this.data.user_account_type;
        }
        else
        {
            console.log('AppendHttpParam fail :' + url);
        }

        return url;
    }

    GetAccountNo()
    {
        if(this.data.has_innocentaccount)
            return this.data.user_account_no;

        return null;
    }

    GetNickName()
    {
        return this.data.user_nick_name;
    }

    NeedMakeNickName()
    {
        return this.data.user_nick_name.indexOf("#") != -1;
    }

    GetAccountType()
    {
        return this.data.user_account_type;
    }

    GetUserProfilePath()
    {
        return this.data.profile_img;
    }

    GetUserLinkPlatforms()
    {
        return this.data.links_platform;
    }

    GetUserEmailState()
    {
        return this.data.email_state;
    }

    GetUserEmailAddress()
    {
        return this.data.email_address;
    }

    ChangeNickName(nick_name)
    {
        if(this.data.has_innocentaccount)
        {
            this.data.SetNickName(nick_name);
        }
    }

    ChangeProfile(url)
    {
        this.data.profile_img = url;
    }

    SetSessionTokenAccount(tok, ano)
    {
        this.data.user_session_token = tok;
        this.data.user_account_no = ano;
    }

    GetSessionTokenAccount()
    {
        return this.data.user_session_token;
    }

    GetUserTokenAccount()
    {
        return this.data.user_token;
    }
}

module.exports = AccountInfo.getInstance();