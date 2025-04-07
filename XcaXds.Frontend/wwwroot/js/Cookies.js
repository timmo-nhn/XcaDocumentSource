window.cookieFunctions = {
    setCookie: function (cookieString) {
        document.cookie = cookieString;
    },
    getCookies: function () {
        return document.cookie;
    }
};
