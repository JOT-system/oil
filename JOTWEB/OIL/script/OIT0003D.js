// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    //document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    }

    addLeftBoxExtention(leftListExtentionTarget);

    if (document.getElementById('WF_RightboxOpen').value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value === "TRUE") {
        //更新ボタン活性／非活性(新規登録、更新で切り分け)
        if (document.getElementById('WF_CREATEFLG').value === "1") {
            //活性
            document.getElementById("WF_ButtonINSERT").disabled = "";
            //非活性 
            //タブ１
            document.getElementById("WF_ButtonALLSELECT_TAB1").disabled = "disabled";
            document.getElementById("WF_ButtonSELECT_LIFTED_TAB1").disabled = "disabled";
            document.getElementById("WF_ButtonLINE_LIFTED_TAB1").disabled = "disabled";
            document.getElementById("WF_ButtonLINE_ADD_TAB1").disabled = "disabled";
            //document.getElementById("WF_ButtonCSV").disabled = "disabled";
            document.getElementById("WF_ButtonUPDATE_TAB1").disabled = "disabled";

        } else if (document.getElementById('WF_CREATEFLG').value === "2") {
            //非活性
            document.getElementById("WF_ButtonINSERT").disabled = "disabled";
            //活性 
            //タブ１
            document.getElementById("WF_ButtonALLSELECT_TAB1").disabled = "";
            document.getElementById("WF_ButtonSELECT_LIFTED_TAB1").disabled = "";
            document.getElementById("WF_ButtonLINE_LIFTED_TAB1").disabled = "";
            document.getElementById("WF_ButtonLINE_ADD_TAB1").disabled = "";
            //document.getElementById("WF_ButtonCSV").disabled = "";
            document.getElementById("WF_ButtonUPDATE_TAB1").disabled = "";
        }

    } else {
        //非活性 
        document.getElementById("WF_ButtonINSERT").disabled = "disabled";
        //タブ１
        document.getElementById("WF_ButtonALLSELECT_TAB1").disabled = "disabled";
        document.getElementById("WF_ButtonSELECT_LIFTED_TAB1").disabled = "disabled";
        document.getElementById("WF_ButtonLINE_LIFTED_TAB1").disabled = "disabled";
        document.getElementById("WF_ButtonLINE_ADD_TAB1").disabled = "disabled";
        document.getElementById("WF_ButtonUPDATE_TAB1").disabled = "disabled";
    }

    // 上部 表示/非表示イベントバインド
    let showHideButtonObj = document.getElementById('hideHeader');
    if (showHideButtonObj !== null) {
        //クリックイベントのバインド
        showHideButtonObj.addEventListener('click',
            function () {
                hideHeader_click();
            });
        //ロード時は必ず上部 表示/非表示処理を行う
        showHideHeader();
    }

    /* 共通一覧のスクロールイベント紐づけ */
    /* 対象の一覧表IDを配列に格納 */
    let arrListId = new Array();
    if (typeof pnlListAreaId1 !== 'undefined') {
        arrListId.push(pnlListAreaId1);
    }
    if (typeof pnlListAreaId2 !== 'undefined') {
        arrListId.push(pnlListAreaId2);
    }
    if (typeof pnlListAreaId3 !== 'undefined') {
        arrListId.push(pnlListAreaId3);
    }
    if (typeof pnlListAreaId4 !== 'undefined') {
        arrListId.push(pnlListAreaId4);
    }
    /* 対象の一覧表IDをループ */
    for (let i = 0, len = arrListId.length; i < len; ++i) {
        let listObj = document.getElementById(arrListId[i]);
        // 対象の一覧表が未存在（レンダリングされていなければ）ならスキップ
        if (listObj === null) {
            continue;
        }
        // 一覧表のイベントバインド
        bindListCommonEvents(arrListId[i], IsPostBack,true);
        // チェックボックス変更
        ChangeCheckBox(arrListId[i]);
    }


}


// ○チェックボックス変更
// 20200115(三宅弘)複数の一覧表に対応するように引数を加え対応しました
function ChangeCheckBox(listId) {
    var objDataLeftSide = document.getElementById(listId + "_DL");
    if (objDataLeftSide === null) {
        return;
    }
    var objTable = objDataLeftSide.children[0];

    var chkObjs = objTable.querySelectorAll("input[id^='chk" + listId + "OPERATION']");
    var spnObjs = objTable.querySelectorAll("span[id^='hchk" + listId + "OPERATION']");

    for (let i = 0; i < chkObjs.length; i++) {

        if (chkObjs[i] !== null) {
            if (spnObjs[i].innerText === "on") {
                chkObjs[i].checked = true;
            } else {
                chkObjs[i].checked = false;
            }
        }
    }
}


// ○チェックボックス選択
function SelectCheckBox(obj, lineCnt) {

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("WF_SelectedIndex").value = lineCnt;
        document.getElementById("WF_ButtonClick").value = "WF_CheckBoxSELECT";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }

}

// ○左Box用処理（左Box表示/非表示切り替え）
function ListField_DBclick(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;

        if (fieldNM === "TANKNO") {
            document.getElementById('WF_LeftMViewChange').value = 20;
        }
        else if (fieldNM === "OILNAME") {
            document.getElementById('WF_LeftMViewChange').value = 24;
        }
        else if (fieldNM === "RETURNDATETRAIN") {
            document.getElementById('WF_LeftMViewChange').value = 17;
        }
        document.getElementById('WF_LeftboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_Field_DBClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ○一覧用処理（チェンジイベント）
function ListField_Change(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_ButtonClick').value = "WF_ListChange";
        document.forms[0].submit();
    }
}
// 〇表示/非表示ボタンクリック時
function hideHeader_click() {
    let headerStateObj = document.getElementById('hdnDispHeaderItems');
    //表示/非表示のフラグ切替
    headerStateObj.value = Math.abs(Number(headerStateObj.value) - 1);
    //切替処理の実行
    showHideHeader();
} 
// 〇上部表示/非表示処理
function showHideHeader() {
    let headerStateObj = document.getElementById('hdnDispHeaderItems');
    let showHideButtonObj = document.getElementById('hideHeader');
    let headerObj = document.getElementById('headerDispArea');
    let detailBoxOjb = document.getElementById('detailbox');
    // 操作対象のオブジェクトが無い場合はそのまま終了
    if (headerStateObj === null) {
        return;
    }
    if (showHideButtonObj === null) {
        return;
    }
    if (headerObj === null) {
        return;
    }
    if (detailBoxOjb === null) {
        return;
    }
    // ヘッダーの表示/非表示切替
    showHideButtonObj.classList.remove('hideHeader');
    headerObj.classList.remove('hideHeader');
    if (headerStateObj.value === '0') {
        //ヘッダー非表示の場合(対象のCssクラスにhideHeader付与)
        showHideButtonObj.classList.add('hideHeader');
        headerObj.classList.add('hideHeader');
    }
    /* 下部の高さを定義 */
    let top = detailBoxOjb.offsetTop;
    let footer = 22.22;
    detailBoxOjb.style.height = "calc(100% - " + top + "px)";
    /* 一覧表の幅をヘッダー有無で可変にする為、ウィンドウのリサイズイベントを発火 */
    var resizeEvent = window.document.createEvent('UIEvents');
    resizeEvent.initUIEvent('resize', true, false, window, 0);
    window.dispatchEvent(resizeEvent);
}