function setDimentions() {
    var block1 = $(".block1");
    var block2 = $(".block2");
    var livefrom = $('.livefrom');
//    var player = document.getElementById("player");
    var tlkio = $("#tlkio");
    
    var height = (screen.height*0.65);// + "px";
    var width = (screen.width*0.57);// + "px";
    
//    block2.style.bottom = ((screen.height*0.75) - block2.clientHeight) + "px";
//    player.style.height = height;
//    player.style.width = width;
    
    block1.height(height);
    block1.width(width);

    var tlkIOWidth = screen.width*2/10; 
    tlkio.width(tlkIOWidth);// + "px";
    tlkio.height(height);
    block2.width(tlkio.width() /3 * 2);
    block2.height((tlkIOWidth / 16 * 9));//+ "px";
    livefrom.height(block2.height());
    livefrom.width(width);
    setDimentions();
    block2.css({'position':'absoulte', 'margin-right' : tlkio.width()/3.3});
}

function getFlashMovie(movieName) {
    var isIE = navigator.appName.indexOf("Microsoft") != -1;
    return (isIE) ? window[movieName] : document[movieName];  
}
function setSrc() {
    var video2 = getFlashMovie('gamePlayer');
    // var rtmpSrc = 'rtmp://54.194.140.164:1935/pros?play=pros';
    var rtmpSrc = 'rtmp://52.18.24.244:1935/Kasata1?play=bob';
    video2.setProperty('src', rtmpSrc);
}

$(document).ready(function(){
    
    setTimeout(function(){
        setSrc();
        setDimentions();
        }, 3000);
});