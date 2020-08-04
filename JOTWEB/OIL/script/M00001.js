// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {
    document.getElementById("rightb").style.visibility = "hidden";
    //左ナビゲーションのクリックイベントバインド
    bindLeftNaviClick();
    //ガイダンス開閉のイベントバインド
    let guidanceButton = document.getElementById('guidanceOpenCloseButton');
    if (guidanceButton !== null) {
        bindShowCloseGuidance(guidanceButton);
    }
    // ポストバック時のスクロール位置復元
    let menuVscrollObj = document.getElementById('hdnPaneAreaVScroll');
    let menuPaneArea = document.querySelector('#Menuheaderbox > .menuMain');
    if (menuVscrollObj !== null) {
        if (menuPaneArea !== null) {
            if (menuVscrollObj.value !== '') {
                menuPaneArea.scrollTop = menuVscrollObj.value;
                menuVscrollObj.value = '';
            }
            
        }
    }
    //〆状況ペインの幅調整
    let closeBranchAll = document.querySelectorAll('.cycleBillingStatusDeptBranch > div');
    let closeBottom = document.querySelector('.cycleBillingStatusBottom');
    if (closeBranchAll !== null) {
        if (closeBottom !== null) {
            let branchSize = 0;
            for (let i = 0; i < closeBranchAll.length; i++) {
                let closeBranch = closeBranchAll[i];
                branchSize = branchSize + closeBranch.clientWidth;
            }
            closeBottom.style.width = branchSize + "px";
        }
    }
}
/**
 * 左ナビゲーションクリックイベントバインド
 * @return {undefined} なし
 */
function bindLeftNaviClick() {
    /* 左ナビ全体のDivを取得 */
    let leftNavObj = document.getElementById('divLeftNav');
    /* 左ナビ未描画なら終了 */
    if (leftNavObj === null) {
        return;
    }
    /* ラベルタグ（左ナビボタン風デザイン）のオブジェクトを取得 */
    let labelObjList = leftNavObj.querySelectorAll("div[data-hasnext='1'] > label");
    /* 左ナビボタンが描画されてなければそのまま終了 */
    if (labelObjList === null) {
        return;
    }
    if (labelObjList.length === 0) {
        return;
    }
    /* 左ナビボタンのループ */
    for (let i = 0; i < labelObjList.length; i++) {
        let targetLabel = labelObjList[i];
        let parentDiv = targetLabel.parentNode;
        let posicol = parentDiv.dataset.posicol;
        let rowline = parentDiv.dataset.rowline;
        // ダイアログを閉じるタイミングでフォーカスを合わせる
        targetLabel.addEventListener('click', (function (posicol, rowline) {
            return function () {
                let hdnPosiColObj = document.getElementById('hdnPosiCol');
                hdnPosiColObj.value = posicol;
                let hdnRowLineObj = document.getElementById('hdnRowLine');
                hdnRowLineObj.value = rowline;
                commonDispWait();
                ButtonClick('WF_ButtonLeftNavi'); /* 共通サブミット処理、VB側ロード時のSelectケースで割り振らせる */ 
            };
        })(posicol, rowline), false);

    }
}
/**
 * 左ナビゲーションクリックイベントバインド
 * @param {Element} objButton 対象のボタンオブジェクト
 * @return {undefined} なし
 */
function bindShowCloseGuidance(objButton) {
    let menuBox = document.getElementById('Menuheaderbox');
    let guidanceAreaObj = document.getElementById('guidanceList');
    let guidanceWrapObj = document.getElementById('guidanceArea');
    let flag = getDispGuigance();
    if (flag === '1') {
        menuBox.classList.add('showGuidance');
        objButton.textContent = '× ガイダンス非表示';
    } else {
        objButton.textContent = '＋ ガイダンス表示';
        guidanceAreaObj.style.display = 'none';
        guidanceWrapObj.style.height = '30px';
    }
    objButton.addEventListener('click', (function (objButton, menuBox, guidanceAreaObj, guidanceWrapObj) {
        return function () {
            if (menuBox.classList.contains('showGuidance')) {
                menuBox.classList.remove('showGuidance');
                objButton.textContent = '＋ ガイダンス表示';
                guidanceAreaObj.style.display = 'none';
                guidanceWrapObj.style.height = '30px';
                setDispGuidance('0');
            } else {
                menuBox.classList.add('showGuidance');
                objButton.textContent = '× ガイダンス非表示';
                guidanceAreaObj.style.display = '';
                guidanceWrapObj.style.height = '';
                setDispGuidance('1');
            }
        };
    })(objButton, menuBox, guidanceAreaObj, guidanceWrapObj), true);


}
/**
 * ローカルストレージよりガイダンスの表示/非表示設定を取得
 * @return {undefined} なし
 */
function getDispGuigance() {
    let dtm = localStorage.getItem("menu0001GuidanceSetDate");
    let flg = localStorage.getItem("menu0001GuidanceFlag");
    var dt = new Date();
    var y = dt.getFullYear();
    var m = ("00" + (dt.getMonth() + 1)).slice(-2);
    var d = ("00" + dt.getDate()).slice(-2);
    let currentDtm = y + m + d;
    if (dtm === null) {
        dtm = currentDtm;
        localStorage.setItem('menu0001GuidanceSetDate', dtm);
    }
    if (dtm === currentDtm) {
        if (flg === null) {
            flg = '1';
        }
    } else {
        flg = '1';
        localStorage.setItem('menu0001GuidanceSetDate', currentDtm);
        localStorage.setItem("menu0001GuidanceFlag", flg);
    }
    return flg;
}
/**
 * 左ナビゲーションクリックイベントバインド
 * @param {string} flag 設定するフラグ
 * @return {undefined} なし
 */
function setDispGuidance(flag) {
    localStorage.setItem("menu0001GuidanceFlag", flag);
}
/**
 * 左ナビゲーションクリックイベントバインド
 * @param {string} refreshMarkObjId リフレッシュフラグを格納するオブジェクト
 * @return {undefined} なし
 */
function refreshPane(refreshMarkObjId) {
    let refreshObj = document.getElementById(refreshMarkObjId);
    let menuVscrollObj = document.getElementById('hdnPaneAreaVScroll');
    let menuPaneArea = document.querySelector('#Menuheaderbox > .menuMain');
    if (refreshObj === null) {
        return;
    }
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        refreshObj.value = '1';
        if (menuVscrollObj !== null) {
            if (menuPaneArea !== null) {
                menuVscrollObj.value = menuPaneArea.scrollTop;
            }
        }
        document.forms[0].submit();
    }

}