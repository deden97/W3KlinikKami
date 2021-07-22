/// <reference path="jquery-3.6.0.js" />
function showMenu() {
    let idM = document.getElementById('menu');
    let btn = document.getElementById('btnEdit');
    let btnL, btnR;
    if (idM.style.visibility == "hidden") {
        idM.style.visibility = "visible";
        btnL = 0;
        btnR = 0;
    } else {
        idM.style.visibility = "hidden";
        btnL = "25px";
        btnR = "25px";
    }

    btn.style.borderBottomLeftRadius = btnL;
    btn.style.borderBottomRightRadius = btnR;
}