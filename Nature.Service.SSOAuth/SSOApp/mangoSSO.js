/**
* 网站应用登录sso的js脚本
**/

Nature.SSO = function (showLoginDiv,win) {
    this.SSOInfo = {
        window:win,
        showLoginDiv:showLoginDiv,          //打开登录窗口
        ssoUrl: "",                         //单点验证服务的网址
        resourceUrl: Nature.AjaxConfig.UrlResource,    //资源服务的网址
        appID: "",                          //网址应用ID
        debug: true                         //是否显示调试信息
        
    };
    
    var debugIndex = 0; //debug的序号
    var self = this;
    
    //第一次访问，没有提交过表单
    this.loginStart = function(callback) {
        self.getSSOinfo(function () {
            //判断是否登录app
            self.isLoginApp(function (userState) {
                //0：没有登录app网站 //1：可以正常访问 //2：不可以访问 //3：暂停访问 //4：被锁定不可以访问。 //5：登录超时
                switch (userState.state) {
                case "0":
                    //没有登录app，看sso端是否登录
                    self.writeDebug("没有登录app");
                    self.ssoLogin(callback);
                    break;
                case "1":
                    //已经登录app和sso，可以正常访问
                    self.writeDebug("登录app和sso，并且可以正常访问！");
                    //登录服务中心
                    self.LoginService(function (msg) {
                        self.writeDebug("登录服务中心返回" + msg + "，回调");
                        if (msg == "5") {
                            self.SSOInfo.showLoginDiv();
                        }
                        
                        callback(msg);
                    });
                    break;
                case "2":
                    //已经登录，但是不可以访问
                    self.writeDebug("登录app和sso，但是账号不允许访问！");
                    callback("账号被暂停，不能访问！");
                    break;
                case "3":
                    //已经登录，但是不可以访问
                    self.writeDebug("登录app和sso，但是账号被暂停，不能访问！");
                    callback("账号被暂停，不能访问！");
                    break;
                case "4":
                    //已经登录，但是不可以访问
                    self.writeDebug("登录app和sso，但是账号被锁定，不能访问！");
                    callback("账号被锁定，不能访问！");
                    break;
                case "5":
                    self.writeDebug("登录app，但是sso登录超时，准备弹出登录表单");
                    self.SSOInfo.showLoginDiv();
                    
                    break;
                }
            });
        });
    };

    //有参数，提交表单后返回
    this.formReturn = function(callback) {
        self.writeDebug("提交表单后返回：" + location.search);
        self.getSSOinfo(function() {
            //判断参数
            var urlSearch = $.getUrlParameter(self.SSOInfo.window.document);
            $("#txtUserCode", self.SSOInfo.window.document).val(urlSearch.userCode);
            switch (urlSearch.statusCode) {
            case "1":
                //登录sso成功
                self.writeDebug("提交表单后，登录sso成功");
                self.ssoLogin(function(state) {
                    if (state == "0") {
                        //登录服务中心
                        self.writeDebug("准备登录服务中心");

                        self.LoginService(function (msg) {
                            self.writeDebug("登录服务中心返回" + msg + "，回调");
                            if (msg == "5") {
                                self.SSOInfo.showLoginDiv();
                            }

                            callback(msg);
                        });
                        
                    } else {
                        //出现异常：账号暂停，或者账号被锁定不能访问，或者app和sso沟通出现异常
                        self.writeDebug("app和sso沟通出现异常");
                        alert(state);
                    }
                });
                break;
            case "2":
                //用户名密码不匹配
                self.writeDebug("用户名密码不匹配");
                self.SSOInfo.showLoginDiv();
                alert("登录账户和登录密码不一致，请检查后再次输入。（登录密码是区分大小写）");
                break;
            case "3":
                //验证码不对
                self.writeDebug("验证码不对");
                self.SSOInfo.showLoginDiv();
                alert("验证码不正确，请重新填写。（不区分大小写）");
                break;
            }
        });
    };
     
    
    //new 检查是否登录service 
    this.isLoginService = function (callback) {
        //检查是否已经登录，如果已经登录，则不必再次登录
        self.writeDebug("检查是否登录服务中心！");
        
        self.ajaxService({
            title: "检查是否登录服务中心",
            data: { action: "WhoAmIAjax", webappid: self.SSOInfo.appID },
            success: function (data) {
                self.writeDebug("登录服务中心返回信息：" + data.state + "，用户：" + data.userSsoID);
                callback(data);
            }
        });
         
    };

    // new 登录服务中心
    this.LoginService = function (callback) {
        //判断是否已经登录服务中心
        self.isLoginService(function (msg) {
            if (msg.state == "5") {
                //超时，
                callback("5");
            }else if (msg.state == "1") {
                //已经登录登录服务中心
                callback("0");
            }else if (msg.state == "0") {
                //没有登录服务中心，向sso申请登录标识
                self.writeDebug("没有登录服务中心，向sso申请登录标识");
                
                self.ajaxSso({
                    title: "向sso申请登录服务中心的标识",
                    data: { action: "LoginService", webappid: self.SSOInfo.appID },
                    success: function (data) {
                        self.writeDebug("得到反馈信息：msg：" + data.msg + "。 ");
                        if (data.msg == "0") {
                            self.writeDebug("准备登录服务中心");
                            self.ajaxService({
                                title: "登录服务中心",
                                data: { action: "login", guid: data.webappGuid, miwen: data.webappMiwen },
                                success: function (data5) {
                                    self.writeDebug("登录服务中心成功!");
                                    callback("0");
                                }
                            });
                            
                        } else
                            alert(data.msg);
                    }
                });
                 
            }
        });

    };
    
    //提交表单后，sso端成功登录的后续操作
    //没有登录app，看sso端是否登录
    this.ssoLogin = function (callback) {
        
        self.isLoginSSO(function(ssoInfo) {
            if (ssoInfo.guid == "") {
                //没有登录sso，弹出div表单登录sso
                self.writeDebug("没有登录sso，弹出div表单登录");
                self.SSOInfo.showLoginDiv();

            } else {
                //登录sso，调用app的登录功能，在app端做个标记
                self.writeDebug("已经登录sso，登录本地网站，做标识");
                self.loginApp(ssoInfo, function(userInfo) {
                    if (userInfo.msg == "0") {
                        //成功在app端做标记
                        self.writeDebug("您已经登录并且在app端做标记成功。UserSsoID:【" + userInfo.userSsoID + "】。UserAppID:【" + userInfo.userAppID + "】。");
                        //继续后续操作
                        //登录服务中心
                        self.LoginService(function (msg) {
                            self.writeDebug("登录服务中心返回" + msg + "，回调");
                            if (msg == "5") {
                                self.SSOInfo.showLoginDiv();
                            }

                            callback(msg);
                        });
                    } else {
                        //app向sso端询问的时候出现异常
                        self.writeDebug("app向sso端询问的时候出现异常：" + userInfo.msg);
                        callback(userInfo.msg);
                    }
                });
            }
        });
    };

    // 看sso端是否登录
    this.isLoginSSO = function(callback) {
        self.writeDebug("准备看sso端是否登录");

        //跨域访问，是否已经登录sso
        self.ajaxSso({
            title: "看sso端是否登录",
            data: { action: "IsOnline", webappID: self.SSOInfo.appID },
            success: function (data) {
                var guid = data.guid;
                if (guid == "")
                    self.writeDebug("得到反馈信息：没有登录sso，（执行回调函数）");
                else
                    self.writeDebug("得到反馈信息：guid：" + data.guid + "，（执行回调函数）");

                callback(data);
            }
        });
         
         
    };
     

    //退出其他网站
    this.LogouOtherApp = function() {
        self.writeDebug("获取需要同步退出的网址");
        
        self.ajaxSso({
            title: "获取需要同步退出的网址",
            data: { action: "GetAppURLGuid", webappid: self.SSOInfo.appID },
            success: function (data) {
                self.writeDebug("得到反馈信息：msg：" + data.msg + "；网站数量：" + data.webappUrls.length + "（执行回调函数）");
                if (data.msg == "0") {
                    for (var i = 0; i < data.webappUrls.length; i++) {
                        var url = "http://" + data.webappUrls[i] + "/SSOApp/WebApp.ashx?action=logout&callback=?";
                        self.writeDebug("退出" + data.WebAppID[i] + "_" + url);
                        $.getJSON(url, { guid: data.webappGuid[i], miwen: data.webappMiwen[i] }, function (data2) {
                            self.writeDebug(data2.userAppID + "退出成功！");

                        });
                    }
                } else
                    alert(data.msg);
            }
        });
         
    };

    //退出sso
    this.logoutSSO = function(callback) {
        //退出其他网站cookie
        self.LogouOtherApp();

        self.ajaxSso({
            title: "退出其他网站",
            data: { action: "Logout", webappid: self.SSOInfo.appID },
            success: function (data) {
                if (data.msg == "0") {
                    self.writeDebug("sso已经退出登录");

                    //去掉本地cookie
                    removeAppTicket(function () {
                        alert("已经退出登录！"); //，调用网站应用的登录功能
                        if (typeof (callback) != "undefined")
                            callback();
                        //location.href = location.pathname;    
                    });

                } else
                    alert(data.msg);
            }
        });
         

        function removeAppTicket(callback3) {

            self.ajaxApp({
                title: "去掉网站应用登录标记",
                data: { action: "logout" },
                success: function (data) {
                    self.writeDebug("已经去掉本地标识");
                    callback3(data);
                }
            });
           
        }
    };

    //输出debug信息
    this.writeDebug = function(msg) {
        if (self.SSOInfo.debug) {
            if (typeof parent != "undefined" && typeof parent.DebugSet != "undefined") {

                var debug = {
                    Title: msg,
                    UserId: "login",
                    StartTime: "0",
                    UseTime: "0毫秒",
                    Url: "",
                    Detail: []
                };

                parent.DebugSet(debug);

            } else {
                $("#divMsg", this.SSOInfo.window.document).append(++debugIndex + "、" + msg + "<br>");
            }
        }
    };
    
};