var sendAction = function (actionPtr) {
    var action = UTF8ToString(actionPtr);
	console.log(`SendAction: ${action}`);
    window.parent.postMessage({ action: action }, '*');
};

var sendActionWithParam = function (actionPtr, paramPtr) {
    var action = UTF8ToString(actionPtr);	
    var param  = UTF8ToString(paramPtr);
	console.log(`SendActionWithParam: action=${action} param=${param}`);
    window.parent.postMessage({ action: action, param: param }, '*');
};

mergeInto(LibraryManager.library, {
    sendAction:         sendAction,
    sendActionWithParam: sendActionWithParam
});
