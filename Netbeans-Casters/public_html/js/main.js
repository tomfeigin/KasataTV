function getFlashMovie(movieName) {
    var isIE = navigator.appName.indexOf("Microsoft") != -1;
    return (isIE) ? window[movieName] : document[movieName];  
}
function setSrc() {
    getFlashMovie('video2').setProperty('src', 'rtmp://192.168.43.29/live?play=bob');
}

//$(document).ready(function() {
//    $('#btnVideo').click(function() {
//        alert("Befor");
//        setSrc();
//        alert("After");
//    });
//});
