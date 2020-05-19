// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {
    document.getElementById("rightb").style.visibility = "hidden";
    let guidanceButton = document.getElementById('guidanceOpenCloseButton');
    if (guidanceButton !== null) {
        bindShowCloseGuidance(guidanceButton);
    }
}
function bindShowCloseGuidance(objButton) {
    let menuBox = document.getElementById('Menuheaderbox');
    let guidanceAreaObj = document.getElementById('guidanceArea');
    menuBox.classList.add('showGuidance');
    objButton.textContent = '× ガイダンス非表示';
    objButton.addEventListener('click', (function (objButton, menuBox, guidanceAreaObj) {
        return function () {
            if (menuBox.classList.contains('showGuidance')) {
                menuBox.classList.remove('showGuidance');
                objButton.textContent = '＋ ガイダンス表示';
                guidanceAreaObj.style.height = '0px';
            } else {
                menuBox.classList.add('showGuidance');
                objButton.textContent = '× ガイダンス非表示';
                guidanceAreaObj.style.height = '';
            }
        };
    })(objButton, menuBox, guidanceAreaObj), true);


}

//function getDispGuigance() {
//    let dtm = localStorage.getItem("menu0001GuidanceSetDate");
//    let flg = localStorage.getItem("menu0001GuidanceFlag");
//    var dt = new Date();
//    var y = dt.getFullYear();
//    var m = ("00" + (dt.getMonth() + 1)).slice(-2);
//    var d = ("00" + dt.getDate()).slice(-2);
//    let currentDtm = y + m + d;
//    if (dtm === null) {
//        dtm = currentDtm;
//        localStorage.setItem('menu0001GuidanceSetDate', dtm);
//    }
//    if (dtm === currentDtm) {
//        if (flg !== null) {
//            return flg;
//        } else {
//            return '1';
//        }
//    } else {
//        localStorage.setItem('menu0001GuidanceSetDate', dtm);
//    }
//}
//function setDispGuidance(flag) {

//}