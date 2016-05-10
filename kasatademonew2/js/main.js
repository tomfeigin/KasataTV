function setDimentions() {
    var block1 = $(".block1");
    var block2 = $(".block2");
    var livefrom = $('.livefrom');
//    var player = document.getElementById("player");
    var tlkio = $("#tlkio");
    
    var height = (screen.height*0.48);// + "px";
    var width = (screen.width*0.57);//\\ + "px";
    
//  block2.style.bottom = ((screen.height*0.75)  block2.clientHeight) + "px";
//    player.style.height = height;
//    player.style.width = width;
    
    block1.height(height);
    block1.width(width);
     
    
    var tlkIOWidth = screen.width*2/9.1  ; 
    tlkio.width(tlkIOWidth);// + "px";
    tlkio.height(height);
    block2.width(tlkio.width() /3 * 2);
    block2.height((tlkIOWidth / 18 * 9)); //"650px";
    livefrom.height(block2.height());
    livefrom.width(width);
    block2.css({'position':'absoulte', 'margin-right' : tlkio.width()/4.3});
}

function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

function getFlashMovie(movieName) {
    var isIE = navigator.appName.indexOf("Microsoft") != -1;
    return (isIE) ? window[movieName] : document[movieName];  
}

function getPlay() {
    var play = getParameterByName("play");
    if(!play) 
    {
        play = 'bob';
    }
    return play;
}

function setSrc(play) {
    if(!play) {
        play = getPlay();
    }
    var video2 = getFlashMovie('gamePlayer');
    // var rtmpSrc = 'rtmp://54.194.140.164:1935/pros?play=pros';
    var rtmpSrc = 'rtmp://52.18.24.244:1935/Kasata1?play=' + play;
    video2.setProperty('src', rtmpSrc);
    
   
    
    var sport = 'rtmp://
}

$(document).ready(function(){
    setDimentions();
    setTimeout(function(){
        setSrc();
        }, 3000);
});