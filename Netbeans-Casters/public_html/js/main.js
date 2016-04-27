/*document.getElementById("gamePlayer").style.height = (screen.height*8.6/10) + "px";
document.getElementById("tlkio").style.height = (screen.height*8.6/10) + "px";
document.getElementById("gamePlayer").style.width = (screen.width*7.4/10) + "px";
document.getElementById("tlkio").style.width = (screen.width*1.95/10) + "px";*/
function setDimentions() {
    document.getElementById("gamePlayer").style.height = (screen.height*8/10) + "px";
    document.getElementById("tlkio").style.height = (screen.height*8/10) + "px";
    document.getElementById("gamePlayer").style.width = (screen.width*7/10) + "px";
    document.getElementById("tlkio").style.width = (screen.width*2.35/10) + "px";
}

$(document).ready(function(){
    setDimentions();
    setTimeout(function(){
        setSrc();
        }, 3000);
});

function getFlashMovie(movieName) {
    var isIE = navigator.appName.indexOf("Microsoft") != -1;
    return (isIE) ? window[movieName] : document[movieName];  
}
function setSrc() {
    var video2 = getFlashMovie('gamePlayer');
    var rtmpSrc = 'rtmp://52.17.241.208:1935/pros?play=pros'
    /*var rtmpSrc = 'rtmp://ec2-52-33-38-73.us-west-2.compute.amazonaws.com/live?play=test';*/
    video2.setProperty('src', rtmpSrc);
}


//$(document).ready(function() {
//    $('#btnVideo').click(function() {
//        alert("Befor");
//        setSrc();
//        alert("After");
//    });
//});
