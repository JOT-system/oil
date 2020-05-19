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


//}
//function setDispGuidance(flag) {

//}