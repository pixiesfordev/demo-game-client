var setCookie = function(cookieJsonPtr) {
    // Convert the pointer to a JavaScript string.
    var cookieJson = UTF8ToString(cookieJsonPtr);
    // Parse the JSON into an object.
    var cookieObj = JSON.parse(cookieJson);

    // Build the cookie string.
    var cookieStr = cookieObj.name + "=" + cookieObj.value;
    
    // Handle the 'expires' property and convert to a valid date format
    if (cookieObj.expires) {
        var expiresDate = new Date(cookieObj.expires);
        if (!isNaN(expiresDate.getTime())) {
            // Set the expires as a valid date string
            cookieStr += "; expires=" + expiresDate.toUTCString();
        } else {
            console.warn("Invalid expires date format, skipping 'expires' attribute.");
        }
    }

    if (cookieObj.path) { cookieStr += "; path=" + cookieObj.path; }
    if (cookieObj.domain) { cookieStr += "; domain=" + cookieObj.domain; }
    if (cookieObj.secure) { cookieStr += "; secure"; }

    // Safe access to document
    var doc;
    try {
        doc = (window.self !== window.top) ? parent.document : document;
    } catch (e) {
        console.warn("Accessing parent.document failed, using current document:", e);
        doc = document;
    }

    doc.cookie = cookieStr;
};

var getCookie = function(cookieNamePtr) {
    var cookieName = UTF8ToString(cookieNamePtr);

    // Safe access to document
    var doc;
    try {
        doc = (window.self !== window.top) ? parent.document : document;
    } catch (e) {
        console.warn("Accessing parent.document failed, using current document:", e);
        doc = document;
    }

    var cookies = doc.cookie.split(';');

    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i].trim();
        if (cookie.indexOf(cookieName + '=') === 0) {
            var cookieValue = cookie.substring(cookieName.length + 1);

            // Allocate memory for the return string
            var bufferSize = lengthBytesUTF8(cookieValue) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(cookieValue, buffer, bufferSize);
            return buffer;
        }
    }

    // Return null pointer if cookie not found
    return 0; 
};

mergeInto(LibraryManager.library, {
    setCookie: setCookie,
    getCookie: getCookie
});
