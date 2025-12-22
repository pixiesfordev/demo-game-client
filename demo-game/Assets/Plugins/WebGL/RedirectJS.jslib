var redirectToUrl = function(urlPtr) {
    var url = UTF8ToString(urlPtr);
    if (url && typeof url === 'string' && url.trim() !== '') {
      window.location.href = url;
    } else {
      console.error('Invalid URL');
    }
};

mergeInto(LibraryManager.library, {
    redirectToUrl: redirectToUrl,
});
