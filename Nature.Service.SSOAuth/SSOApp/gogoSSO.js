/*
*  利用gogo的登录的登录
*  1、查看app是否登录
*  2、查看sso是否登录
*  3、查看gogo是否登录
*  4、模拟sso登录
*  5、登录resource
*  6、本地登录
*/


//var Nature = {};
Nature.SSOgogo = function (win) {
    var ssoInfo = {
        window: win,
        ssoURL : "",
        appID: "",
        debug: true,
        debugIndex: 0,
        
        showLoginDiv: {}
        
    };
    var mySelf = this;

    //第一次访问，没有提交过表单
    this.loginStart = function (showLoginDiv, callback) {
        ssoInfo.showLoginDiv = showLoginDiv;
        this.getSSOinfo(function() {
            writeDebug("获取配置信息完毕，开始判断是否登录app");
            //判断是否登录app
            isLoginApp(function(userState) {
                //0：没有登录app网站
                //1：可以正常访问
                //2：不可以访问
                //3：暂停访问
                //4：被锁定不可以访问。
                //5：登录超时
                switch (userState.state) {
                case "0":
                    //没有登录app，看sso端是否登录
                    writeDebug("没有登录app，看sso端是否登录");
                    ssoLogin(showLoginDiv, callback);
                    break;
                case "1":
                    //已经登录，可以正常访问
                    writeDebug("登录app和sso，并且可以正常访问！");
                    //登录服务中心
                    LoginService(function() {
                        callback("0");
                    });
                    break;
                case "2":
                    //已经登录，但是不可以访问
                    writeDebug("登录app和sso，但是账号不允许访问！");
                    callback("账号被暂停，不能访问！");
                    break;
                case "3":
                    //已经登录，但是不可以访问
                    writeDebug("登录app和sso，但是账号被暂停，不能访问！");
                    callback("账号被暂停，不能访问！");
                    break;
                case "4":
                    //已经登录，但是不可以访问
                    writeDebug("登录app和sso，但是账号被锁定，不能访问！");
                    callback("账号被锁定，不能访问！");
                    break;
                case "5":
                    writeDebug("登录app，但是sso登录超时，准备弹出登录表单");
                    showLoginDiv();
                    break;
                }
            });
        });
    };

    //有参数，提交表单后返回
    this.formReturn = function(showLoginDiv, callback) {
        writeDebug("提交表单后返回：" + location.search);
        getSSOinfo(function() {
            //判断参数
            var urlSearch = $.getUrlParameter(mySelf.SSOInfo.window.document);
            $("#txtUserCode", mySelf.SSOInfo.window.document).val(urlSearch.userCode);
            switch (urlSearch.statusCode) {
            case "1":
                //登录sso成功
                writeDebug("提交表单后，登录sso成功");
                ssoLogin(showLoginDiv, function(state) {
                    if (state == "0") {
                        //登录服务中心
                        writeDebug("开始登录服务中心");
                        LoginService(function () {
                            writeDebug("登录服务中心成功，回调");
                            callback("0");
                        });

                    } else {
                        //出现异常：账号暂停，或者账号被锁定不能访问，或者app和sso沟通出现异常
                        writeDebug("app和sso沟通出现异常");
                        alert(state);
                    }
                });
                break;
            case "2":
                //用户名密码不匹配
                writeDebug("用户名密码不匹配");
                showLoginDiv();
                alert("登录账户和登录密码不一致，请检查后再次输入。（登录密码是区分大小写）");
                break;
            case "3":
                //验证码不对
                writeDebug("验证码不对");
                showLoginDiv();
                alert("验证码不正确，请重新填写。（不区分大小写）");
                break;
            }
        });
    };

    //提交表单后，sso端成功登录的后续操作
    //没有登录app，看sso端是否登录
    var ssoLogin = function(showLoginDiv, callback) {
        ssoIsLogin(function(ssoInfo1) {
            if (ssoInfo1.guid == "") {
                //没有登录sso，弹出div表单登录sso
                writeDebug("没有登录sso，弹出div表单登录");
                showLoginDiv();

            } else {
                //登录sso，调用app的登录功能，在app端做个标记
                writeDebug("已经登录sso，登录本地网站，做标识");
                loginApp(ssoInfo1, function(userInfo) {
                    if (userInfo.msg == "0") {
                        //成功在app端做标记
                        writeDebug("您已经登录并且在app端做标记成功。UserSsoID:【" + userInfo.userSsoID + "】。UserAppID:【" + userInfo.userAppID + "】。");
                        //继续后续操作
                        //登录服务中心
                        LoginService(function () {
                            callback("0");
                        });
                    } else {
                        //app向sso端询问的时候出现异常
                        writeDebug("app向sso端询问的时候出现异常：" + userInfo.msg);
                        callback(userInfo.msg);
                    }
                });
            }
        });
    };

    //获取sso的url和网站应用ID
    this.getSSOinfo = function(callback) {
        $.get("/SSOApp/WebApp.ashx?action=getssourl", function(data) {
            var info = eval("(" + data + ")");
            ssoInfo.ssoURL = info.SSOUrl;
            ssoInfo.appID = info.WebAppID;
            ssoInfo.debug = info.Debug == "True";

            writeDebug("获取到配置信息（执行回调函数）。ssoURL：" + ssoInfo.ssoURL + "；appID：" + ssoInfo.appID + "；debug：" + ssoInfo.debug);

            callback(info);
        });
    };

    //判断是否登录app
    var isLoginApp = function(callback) {
        $.ajax({
            type: "GET",
            dataType: "json",
            cache: false,
            error: function() {
                alert("获取是否登录网站应用的时候发生错误！");
            },
            url: "/SSOApp/WebApp.ashx?action=WhoAmIAjax",
            success: function(data) {
                writeDebug("获取到app登录状态：" + data.state + "（执行回调函数）");
                callback(data);
            }
        });
    };

    //没有登录app，看sso端是否登录
    var ssoIsLogin = function(callback) {
        writeDebug("准备看sso端是否登录");

        //跨域访问，是否已经登录sso
        $.getJSON(ssoInfo.ssoURL + "/SSOAuth/SSOAuth.ashx?action=IsOnline&callback=?", { webappID: ssoInfo.appID }, function (data) {
            writeDebug("得到反馈信息：guid：" + data.guid + "（执行回调函数）");
            callback(data);
        });

    };

    //登录网站应用
    var loginApp = function(data, callback) {
        //url编码
        writeDebug("登录app，设置app的标识。沟通标识：" + data.miwen);
        data.miwen = decodeURIComponent(data.miwen);

        var myData = {
            miwen: data.miwen,
            guid: data.guid
        };

        $.ajax({
            type: "GET",
            dataType: "json",
            cache: false,
            data: myData,
            error: function() {
                alert("在网站应用做登录标记的时候发生意外！");
            },
            url: "/SSOApp/WebApp.ashx?action=login",
            success: function(data2) {
                writeDebug("写入本地标识：userSsoID：" + data2.userSsoID + "（执行回调函数）");
                callback(data2);
            }
        });
    };

    
    //登录服务中心
    var LoginService = function(callback) {
        var appUrl = "";

        //判断是否已经登录服务中心
        writeDebug("判断是否已经登录服务中心");
        $.getJSON(Nature.AjaxConfig.UrlResource + "/SSOApp/WebApp.ashx?action=WhoAmIAjax&webappid=" + ssoInfo.appID + "&callback=?", function (data) {
            if (data.state != "1") {
                loginS();
            }
            else if (data.userSsoID != "0") {
                //登录了，写入cookie
                writeDebug("准备写入服务端的cookie");
                Nature.SSOgogo.WriteServiceCK(Nature.AjaxConfig.UrlResource + '/', callback);
            } else {
                loginS();
            }
        });

        //写入cookie
        var writrCookie = function (data2) {
            var bro = $.browser;

            if (bro.msie || bro.mozilla) {
                /* IE */
                if (data2.msg == "0")
                    writeDebug(data2.ticketAppID + "_" + data2.configAppID + "登录成功！");
                else {
                    alert(data2.msg);
                    writeDebug(data2.ticketAppID + "_" + data2.configAppID + data2.msg);
                }
                //alert(data2.configAppID + appUrl);

                var onload = " onload=Nature.SSOgogo.WriteServiceCK('" + appUrl + "')";
                var uu = $("<iframe width=\"1px\" height=\"1px;\" style=\"display:none;\" scrolling=\"no\" src='" + appUrl + "/loginsso.htm' " + onload + " >");
                $(document.body).append(uu);

                window.setTimeout(callback, 1000);

            } else {
                /*chrome*/
                writeDebug("准备写入服务端的cookie");
                Nature.SSOgogo.WriteServiceCK(appUrl, callback);
            }
        };
            
        //登录服务中心
        var loginS = function() {
            writeDebug("获取服务中心的网址和票据");
            $.getJSON(ssoInfo.ssoURL + "/SSOAuth/SSOAuth.ashx?action=LoginService&webappid=" + ssoInfo.appID + "&callback=?", function (data) {
                writeDebug("得到反馈信息：msg：" + data.msg + "。（执行回调函数）");
                if (data.msg == "0") {
                    var url = data.webappUrls + "/SSOApp/WebApp.ashx?action=login&callback=?";
                    writeDebug("登录" + data.WebAppID + "_" + url);
                    appUrl = data.webappUrls;
                    $.ajax({
                        type: "GET",
                        dataType: "jsonp",
                        cache: false,
                        url: url,
                        data: { guid: data.webappGuid, miwen: data.webappMiwen },
                        error: function(e, xhr, opt) {
                            //alert("opt.url " + opt + ": " + xhr + " " + e);
                            //alert("opt.url " + opt.url + ": " + xhr.status + " " + xhr.statusText);
                            writeDebug("登录服务中心的时候发生错误！" + "登录" + data.WebAppID + "_" + url);
                            alert("登录服务中心的时候发生错误！" + "登录" + data.WebAppID + "_" + url);
                        },
                        success: function(data3) {
                            writrCookie(data3);
                        }
                    });

                } else
                    alert(data.msg);
            });
        };

    };
    
    
    //退出其他网站
    var logouOtherApp = function() {
        writeDebug("获取需要同步退出的网址");
        $.getJSON(ssoInfo.ssoURL + "/SSOAuth/SSOAuth.ashx?action=GetAppURLGuid&webappid=" + ssoInfo.appID + "&callback=?", function (data) {
            writeDebug("得到反馈信息：msg：" + data.msg + "；网站数量：" + data.webappUrls.length + "（执行回调函数）");
            if (data.msg == "0") {
                for (var i = 0; i < data.webappUrls.length; i++) {
                    var url = "http://" + data.webappUrls[i] + "/SSOApp/WebApp.ashx?action=logout&callback=?";
                    writeDebug("退出" + data.WebAppID[i] + "_" + url);
                    $.getJSON(url, { guid: data.webappGuid[i], miwen: data.webappMiwen[i] }, function(data2) {
                        writeDebug(data2.userAppID + "退出成功！");

                    });
                }
            } else
                alert(data.msg);
        });
    };

    //退出sso
    this.logoutSSO = function(callback) {
        //去掉其他网站cookie
        LogouOtherApp();

        $.getJSON(ssoInfo.ssoURL + "/SSOAuth/SSOAuth.ashx?action=Logout&webappid=" + ssoInfo.appID + "&callback=?", function (data) {
            if (data.msg == "0") {
                writeDebug("sso已经退出登录");

                //去掉本地cookie
                removeAppTicket();

                alert("已经退出登录！"); //，调用网站应用的登录功能
                if (typeof(callback) != "undefined")
                    callback();

                $("#btnLogout").hide();
                //location.href = location.pathname;
            } else
                alert(data.msg);
        });

        function removeAppTicket() {

            $.ajax({
                type: "GET",
                dataType: "json",
                cache: false,
                error: function() {
                    alert("在去掉网站应用登录标记的时候发生意外！");
                },
                url: "/SSOApp/WebApp.ashx?action=logout",
                success: function(data2) {
                    writeDebug("已经去掉本地标识");
                }
            });
        }
    };

    //输出debug信息
    var writeDebug = function(msg) {
        if (ssoInfo.debug) {
            $("#divMsg").append(++ssoInfo.debugIndex + "、" + msg + "<br>");
        }
    };
};

//读取ck并且写入本地
Nature.SSOgogo.WriteServiceCK = function(url, callback) {
    $.ajax({
        type: "GET",
        dataType: "jsonp",
        cache: false,
        url: url + "/ssoapp/WebApp.ashx",
        data: { action: "getck" },
        error: function() {
            alert("获取服务端的用户ID（ck）的时候出现异常！");
        },
        success: function(data3) {
            var userAppIDck = data3.ck;
            //alert(userAppIDck);
            //写入本地cookies
            writeDebug("把服务端的cookie写入本地cookie");
            $.cookie("userIDserviceByck", userAppIDck, { path: '/' });

            if (typeof(callback) != "undefined") {
                writeDebug("写入cookie完毕，回调");
                callback();
            }
        }
    });
    
    //输出debug信息
    var writeDebug = function (msg) {
        $("#divMsg").append( "、" + msg + "<br>");
    };
};
