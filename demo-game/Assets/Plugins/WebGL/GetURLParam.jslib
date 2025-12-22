var getURLParam = function(keyPtr) {
    var key = UTF8ToString(keyPtr);
    var val = "";

    try {
        var params = new URLSearchParams(window.location.search);
        var tmp = params.get(key);
        if (tmp !== null) val = tmp;
    } catch (err) {
        console.error("GetURLParam 解析失敗:", err);
    }

    return allocateUTF8(val);
};

mergeInto(LibraryManager.library, {
    getURLParam: getURLParam
});
