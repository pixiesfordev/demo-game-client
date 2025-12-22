var isMobileBrowser = function () {
  if (navigator.userAgentData && typeof navigator.userAgentData.mobile === 'boolean') {
    return navigator.userAgentData.mobile ? 1 : 0;
  }
  if ('maxTouchPoints' in navigator) {
    return navigator.maxTouchPoints > 0 ? 1 : 0;
  }
  if (window.matchMedia && window.matchMedia('(pointer:coarse)').matches) {
    return 1;
  }
  var ua = navigator.userAgent || '';
  return /Android|iPhone|iPod|Opera Mini|IEMobile|WPDesktop|Mobile/i.test(ua) ? 1 : 0;
};


var copyText = function (textPtr) {
    var text = UTF8ToString(textPtr);
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).catch(function(err){
            console.error("Clipboard write failed", err);
        });
    } else {
        var ta = document.createElement('textarea');
        ta.value = text;
        ta.style.position = 'fixed';
        ta.style.top = '0';
        ta.style.left = '0';
        ta.style.opacity = '0';
        document.body.appendChild(ta);
        ta.focus();
        ta.select();
        try {
            document.execCommand('copy');
        } catch (err) {
            console.error("ExecCommand copy failed", err);
        }
        document.body.removeChild(ta);
    }
};

var getTimeZoneOffsetHours = function () {
    // JS getTimezoneOffset 回傳「當地時間 – UTC（分鐘）」的負值，
    // (UTC+8) 回傳 -480，所以取負並除以 60 得到 +8
    return -new Date().getTimezoneOffset() / 60;
};

mergeInto(LibraryManager.library, {
    isMobileBrowser: isMobileBrowser,
    copyText:        copyText,
    getTimeZoneOffsetHours: getTimeZoneOffsetHours,
});
