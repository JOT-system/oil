// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    //document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen").value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    }

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);
    // チェックボックス
    ChangeCheckBox();
}

// ○チェックボックス
function ChangeCheckBox() {
    var objTableDR = document.getElementById("pnlListArea_DR").children[0];
    var objLightTable = objTableDR.children[0];
    if (objLightTable === null) {
        return;
    }
    if (objLightTable === undefined) {
        return;
    }

    // 未卸可否フラグ
    var chkObjsLight2 = objLightTable.querySelectorAll("input[id^='chkpnlListAreaWHOLESALEFLG']");
    var spnObjsLight2 = objLightTable.querySelectorAll("span[id^='hchkpnlListAreaWHOLESALEFLG']");
    // 交検可否フラグ
    var chkObjsLight3 = objLightTable.querySelectorAll("input[id^='chkpnlListAreaINSPECTIONFLG']");
    var spnObjsLight3 = objLightTable.querySelectorAll("span[id^='hchkpnlListAreaINSPECTIONFLG']");
    // 留置可否フラグ
    var chkObjsLight4 = objLightTable.querySelectorAll("input[id^='chkpnlListAreaDETENTIONFLG']");
    var spnObjsLight4 = objLightTable.querySelectorAll("span[id^='hchkpnlListAreaDETENTIONFLG']");

    for (let i = 0; i < chkObjsLight2.length; i++) {

        if (chkObjsLight2[i] !== null) {
            if (spnObjsLight2[i].innerText === "on") {
                chkObjsLight2[i].checked = true;
            } else {
                chkObjsLight2[i].checked = false;
            }
        }
    }

    for (let i = 0; i < chkObjsLight3.length; i++) {

        if (chkObjsLight3[i] !== null) {
            if (spnObjsLight3[i].innerText === "on") {
                chkObjsLight3[i].checked = true;
            } else {
                chkObjsLight3[i].checked = false;
            }
        }
    }

    for (let i = 0; i < chkObjsLight4.length; i++) {

        if (chkObjsLight4[i] !== null) {
            if (spnObjsLight4[i].innerText === "on") {
                chkObjsLight4[i].checked = true;
            } else {
                chkObjsLight4[i].checked = false;
            }
        }
    }
}

// ○チェックボックス選択
function SelectCheckBox(obj, lineCnt, fieldName) {

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        let chkObj = obj.querySelector("input");
        if (chkObj === null) {
            return;
        }
        if (chkObj.disabled === true) {
            return;
        }

        surfix = '';
        if (fieldName === 'WHOLESALEFLG') {
            surfix = 'WHOLESALE'
        }
        if (fieldName === 'INSPECTIONFLG') {
            surfix = 'INSPECTION'
        }
        if (fieldName === 'DETENTIONFLG') {
            surfix = 'DETENTION'
        }

        document.getElementById("WF_SelectedIndex").value = lineCnt;
        document.getElementById("WF_ButtonClick").value = "WF_CheckBoxSELECT" + surfix;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }

}

// ○左Box用処理（左Box表示/非表示切り替え）
function ListField_Dbclick(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;

        if (fieldNM === "ORDER_ACTUALACCDATE"
            || fieldNM === "ORDER_ACTUALEMPARRDATE") {
            document.getElementById('WF_LeftMViewChange').value = 17;
        }
        document.getElementById('WF_LeftboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_Field_DbClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}