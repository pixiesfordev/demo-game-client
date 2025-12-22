var callVoidFunc = function(funcNamePtr) {
    var funcName = UTF8ToString(funcNamePtr);

    // Safe access to context
    var context;
    try {
        context = (window.self !== window.top) ? parent : window;
    } catch (e) {
        console.warn("Accessing parent context failed, defaulting to window:", e);
        context = window;
    }

    if (typeof context[funcName] === 'function') {
        context[funcName]();
    } else {
        console.error("Function '" + funcName + "' not found or is not a function in context.");
    }
};

var callVoidFuncWithStringArgs = function(fnamePtr, argsPtr, argsCount) {
    var fname = UTF8ToString(fnamePtr);
    var args = [];

    for (var i = 0; i < argsCount; i++) {
        var argPtr = getValue(argsPtr + i * 4, '*');
        args.push(UTF8ToString(argPtr));
    }

    // Safe access to context
    var context;
    try {
        context = (window.self !== window.top) ? parent : window;
    } catch (e) {
        console.warn("Accessing parent context failed, defaulting to window:", e);
        context = window;
    }

    if (typeof context[fname] === "function") {
        context[fname].apply(null, args);
    } else {
        console.error("Function '" + fname + "' not found in context.");
    }
};

mergeInto(LibraryManager.library, {
    callVoidFunc: callVoidFunc,
    callVoidFuncWithStringArgs: callVoidFuncWithStringArgs
});