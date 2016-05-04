/*document.getElementById("gamePlayer").style.height = (screen.height*8.6/10) + "px";
document.getElementById("tlkio").style.height = (screen.height*8.6/10) + "px";
document.getElementById("gamePlayer").style.width = (screen.width*7.4/10) + "px";
document.getElementById("tlkio").style.width = (screen.width*1.95/10) + "px";*/
function setDimentions() {
    document.getElementById("skynews").style.height = (screen.height*8/10) + "px";
    document.getElementById("gamePlayer").style.height = (screen.height*2.35/10) + "px";
    document.getElementById("tlkio").style.height = (screen.height*8/10) + "px";
    document.getElementById("gamePlayer").style.width = (screen.width*2/10) + "px";
    document.getElementById("skynews").style.width = (screen.width*7/10) + "px";
    document.getElementById("tlkio").style.width = (screen.width*2/10) + "px";
}

$(document).ready(function(){
    setTimeout(function(){
        setSrc();
        setDimentions();
        }, 3000);
});

function getFlashMovie(movieName) {
    var isIE = navigator.appName.indexOf("Microsoft") != -1;
    return (isIE) ? window[movieName] : document[movieName];  
}
function setSrc() {
    var video2 = getFlashMovie('gamePlayer');
    // alert("Video2 " + video2);
    var rtmpSrc = 'rtmp://52.18.24.244:1935/Kasata1?play=bob';
    /*var rtmpSrc = 'rtmp://ec2-52-33-38-73.us-west-2.compute.amazonaws.com/live?play=test';*/
    // alert(video2.getProperty('src'));
    video2.setProperty('src', rtmpSrc);
    // alert(video2.getProperty('src'));
}


//$(document).ready(function() {
//    $('#btnVideo').click(function() {
//        alert("Befor");
//        setSrc();
//        alert("After");
//    });
//});
