// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {
    document.forms[0].style.visibility = 'hidden'; // 一旦レスポンス用
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
    if (document.getElementById('WF_MAPpermitcode').value === "TRUE"
        && document.getElementById('WF_OrderStatusFLG').value === "FALSE") {
        //更新ボタン活性／非活性(新規登録、更新で切り分け)
        if (document.getElementById('WF_CREATEFLG').value === "1") {
            //活性
            document.getElementById("WF_ButtonINSERT").disabled = "";
            document.getElementById("WF_ButtonUPDATE").disabled = "";
            //非活性 
            document.getElementById("WF_ButtonALLSELECT").disabled = "disabled";
            document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "disabled";
            document.getElementById("WF_ButtonLINE_LIFTED").disabled = "disabled";
            document.getElementById("WF_ButtonLINE_ADD").disabled = "disabled";
            document.getElementById("WF_ButtonCSV").disabled = "disabled";

            //受注営業所のアイコンの上にあるラベルを非表示とする。
            //document.getElementById("WF_OFFICECODE_DUMMY").style.display = "none";

        } else if (document.getElementById('WF_CREATEFLG').value === "2") {
            //非活性
            document.getElementById("WF_ButtonINSERT").disabled = "disabled";
            //活性 
            document.getElementById("WF_ButtonALLSELECT").disabled = "";
            document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "";
            document.getElementById("WF_ButtonLINE_LIFTED").disabled = "";
            document.getElementById("WF_ButtonLINE_ADD").disabled = "";
            document.getElementById("WF_ButtonCSV").disabled = "";
            document.getElementById("WF_ButtonUPDATE").disabled = "";

            //受注営業所のアイコンを非表示とする。
            //document.getElementById("WF_OFFICECODE_ICON").style.display = "none";
        }

    } else {
        //非活性 
        document.getElementById("WF_ButtonINSERT").disabled = "disabled";
        document.getElementById("WF_ButtonALLSELECT").disabled = "disabled";
        document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "disabled";
        document.getElementById("WF_ButtonLINE_LIFTED").disabled = "disabled";
        document.getElementById("WF_ButtonLINE_ADD").disabled = "disabled";
        //document.getElementById("WF_ButtonCSV").disabled = "disabled";
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    }
    /* フッターの高さ調整 */
    AdjustHeaderFooterContents('detailbox');
    /* 共通一覧のスクロールイベント紐づけ */
    document.getElementById(pnlListAreaId).style.display = "none"; // 一旦レスポンス用
    bindListCommonEvents(pnlListAreaId, IsPostBack, false, true, true,false);
    // チェックボックス
    ChangeCheckBox();
    // テキストボックスEnter縦移動イベントバインド
    commonBindEnterToVerticalTabStep();
    document.getElementById(pnlListAreaId).style.display = "block"; // 一旦レスポンス用
    document.forms[0].style.visibility = ''; // 一旦レスポンス用
}


// ○チェックボックス変更
function ChangeCheckBox() {

    var objTable = document.getElementById("pnlListArea_DL").children[0];

    var chkObjs = objTable.querySelectorAll("input[id^='chkpnlListAreaOPERATION']");
    var spnObjs = objTable.querySelectorAll("span[id^='hchkpnlListAreaOPERATION']");

    for (let i = 0; i < chkObjs.length; i++) {

        if (chkObjs[i] !== null) {
            if (spnObjs[i].innerText === "on") {
                chkObjs[i].checked = true;
            } else {
                chkObjs[i].checked = false;
            }
        }
    }

    var objTableDR = document.getElementById("pnlListArea_DR").children[0];
    var objLightTable = objTableDR.children[0];
    if (objLightTable === null) {
        return;
    }
    if (objLightTable === undefined) {
        return;
    }

    // 積置フラグ
    var chkObjsLight1 = objLightTable.querySelectorAll("input[id^='chkpnlListAreaSTACKINGFLG']");
    var spnObjsLight1 = objLightTable.querySelectorAll("span[id^='hchkpnlListAreaSTACKINGFLG']");

    for (let i = 0; i < chkObjsLight1.length; i++) {

        if (chkObjsLight1[i] !== null) {
            if (spnObjsLight1[i].innerText === "on") {
                chkObjsLight1[i].checked = true;
            } else {
                chkObjsLight1[i].checked = false;
            }
        }
    }

    //### 20201009 START 指摘票No165対応 ############################################################
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
    //### 20201009 END   指摘票No165対応 ############################################################

}


// ○チェックボックス選択
function SelectCheckBox(obj, lineCnt, fieldName) {

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {

        surfix = '';
        if (fieldName === 'STACKINGFLG') {
            surfix = 'STACKING'
        }
        //### 20201009 START 指摘票No165対応 ############################################################
        if (fieldName === 'WHOLESALEFLG') {
            surfix = 'WHOLESALE'
        }
        if (fieldName === 'INSPECTIONFLG') {
            surfix = 'INSPECTION'
        }
        if (fieldName === 'DETENTIONFLG') {
            surfix = 'DETENTION'
        }
        //### 20201009 END   指摘票No165対応 ############################################################

        document.getElementById("WF_SelectedIndex").value = lineCnt;
        document.getElementById("WF_ButtonClick").value = "WF_CheckBoxSELECT" + surfix;
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
        else if (fieldNM === "ORDERINGOILNAME") {
            //document.getElementById('WF_LeftMViewChange').value = 46;
            document.getElementById('WF_LeftMViewChange').value = 74;
        }
        else if (fieldNM === "SHIPPERSNAME") {
            document.getElementById('WF_LeftMViewChange').value = 42;
        }
        else if (fieldNM === "RETURNDATETRAIN") {
            document.getElementById('WF_LeftMViewChange').value = 17;
        }
        else if (fieldNM === "JOINT") {
            document.getElementById('WF_LeftMViewChange').value = 53;
        }
        else if (fieldNM === "ACTUALLODDATE" 
            || fieldNM === "JRINSPECTIONDATE") {
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
