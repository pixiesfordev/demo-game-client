var callVoidFuncWithStringArgs = function(fname, stringArgsArray) {
    var args = [];
    if (stringArgsArray && stringArgsArray.length > 0) {
        for (var i = 0; i < stringArgsArray.length; i++) {
            args.push(stringArgsArray[i]);
        }
    }
    
    if (typeof window[fname] === "function") {
        window[fname].apply(window, args);
    } else {
        console.error("Function " + fname + " not found.");
    }
};

mergeInto(LibraryManager.library, {
    callVoidFuncWithStringArgs: callVoidFuncWithStringArgs
});