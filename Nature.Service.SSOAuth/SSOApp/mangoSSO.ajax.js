/*

单点登录js脚本的ajax访问部分

*/


/* new 获取sso的url和网站应用ID*/
Nature.SSO.prototype.getSSOinfo = function (callback) {
    var self = this;
    var ssoInfo = top._ssoInfo;/*判断父页是否有缓存*/
    if (typeof ssoInfo == "undefined") {
        self.writeDebug("没有sso的网址，ajax获取sso的网址");
        self.ajaxApp({
            title: "获取sso的网址",
            data: { action: "getssourl" },
            success: function (info) {
                self.SSOInfo.ssoUrl = info.SSOUrl;
                self.SSOInfo.appID = info.WebAppID;
                self.SSOInfo.debug = info.Debug == "True";

                top._ssoInfo = info;

                self.writeDebug("获取到sso的网址（执行回调函数）。ssoUrl：" + self.SSOInfo.ssoUrl + "；appID：" + self.SSOInfo.appID + "；debug：" + self.SSOInfo.debug);
                callback();
            }
        });
       
    } else {
        self.writeDebug("有sso的网址的缓存。ssoUrl：" + ssoInfo.SSOUrl + "；appID：" + ssoInfo.WebAppID + "；debug：" + ssoInfo.Debug == "True");
        self.SSOInfo.ssoUrl = ssoInfo.SSOUrl;
        self.SSOInfo.appID = ssoInfo.WebAppID;
        self.SSOInfo.debug = ssoInfo.Debug == "True";
        callback();
    }
};


/* new 判断是否登录app*/
Nature.SSO.prototype.isLoginApp = function (callback) {
    var self = this;
    self.writeDebug("判断是否登录网站应用");
    self.ajaxApp({
        title: "是否登录网站应用",
        data: { action: "WhoAmIAjax" },
        success: function (data) {
            self.writeDebug("获取到app登录状态：" + data.state + "（执行回调函数）");
            callback(data);
        }
    });
    
};

/* new 登录app*/
Nature.SSO.prototype.loginApp = function (data, callback) {
    var self = this;
    self.writeDebug("登录app，设置app的标识。沟通标识：" + data.miwen);
    data.miwen = decodeURIComponent(data.miwen);

    self.ajaxApp({
        title: "登录网站应用",
        data: { action: "login", miwen: data.miwen, guid: data.guid },
        success: function (data2) {
            self.writeDebug("写入本地标识：userSsoID：" + data2.userSsoID + "（执行回调函数）");
            callback(data2);
        }
    });

};





/* sso里的统一ajax 
本域，/SSOApp/WebApp.ashx，不缓存
*/
Nature.SSO.prototype.ajaxApp = function(info) {
    var self = this;
    
    info.dataType = "json";
    info.url = "/SSOApp/WebApp.ashx";

    self.ajax(info);
   
};

/* 跨域，sso服务器，mySelf.SSOInfo.ssoUrl + "/SSOAuth/SSOAuth.ashx，不缓存*/
Nature.SSO.prototype.ajaxSso = function(info) {
    var self = this;

    info.dataType = "jsonp";
    info.url = self.SSOInfo.ssoUrl + "/SSOAuth/SSOAuth.ashx";

    self.ajax(info);
};


/* 跨域，资源服务，mySelf.SSOInfo.resourceUrl + "/SSOAuth/SSOAuth.ashx，不缓存*/
Nature.SSO.prototype.ajaxService = function (info) {
    var self = this;

    info.dataType = "jsonp";
    info.url = self.SSOInfo.resourceUrl + "/SSOApp/WebApp.ashx";

    self.ajax(info);
};

/*   ajax 提交  */
Nature.SSO.prototype.ajax = function (info) {
    var self = this;
    $.ajax({
        type: "GET",
        dataType: info.dataType,
        url: info.url,
        data: info.data,
        cache: false,
        error: function () {
            var err = "获取" + info.title + "的时候发生错误！";
            self.writeDebug(err);
            alert(err);
        },
        success: function (data) {
            if (typeof (parent.DebugSet) != "undefined")
                if (typeof (data.debug) != "undefined") parent.DebugSet(data.debug);

            info.success(data);
        }
    });
}