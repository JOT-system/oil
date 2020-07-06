// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
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
            document.getElementById("WF_ButtonRegister").disabled = "";
            //非活性 
            document.getElementById("WF_ButtonALLSELECT").disabled = "disabled";
            document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "disabled";
            document.getElementById("WF_ButtonLINE_LIFTED").disabled = "disabled";
            document.getElementById("WF_ButtonLINE_ADD").disabled = "disabled";
            document.getElementById("WF_ButtonCSV").disabled = "disabled";
            document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
            document.getElementById("pnlListArea").display = "none";
        } else if (document.getElementById('WF_CREATEFLG').value === "2") {
            //非活性
            document.getElementById("WF_ButtonRegister").disabled = "disabled";
            //活性 
            document.getElementById("WF_ButtonALLSELECT").disabled = "";
            document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "";
            document.getElementById("WF_ButtonLINE_LIFTED").disabled = "";
            document.getElementById("WF_ButtonLINE_ADD").disabled = "";
            document.getElementById("WF_ButtonCSV").disabled = "";
        }

    } else {
        //非活性 
        document.getElementById("WF_ButtonRegister").disabled = "disabled";
        document.getElementById("WF_ButtonALLSELECT").disabled = "disabled";
        document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "disabled";
        document.getElementById("WF_ButtonLINE_LIFTED").disabled = "disabled";
        document.getElementById("WF_ButtonLINE_ADD").disabled = "disabled";
    }

    if (document.getElementById('WF_PANELFLG').value === "1") {
        //活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    }
    /* フッターの高さ調整 */
    AdjustHeaderFooterContents('detailbox');
    /* 共通一覧のスクロールイベント紐づけ */
    //bindListCommonEvents(pnlListAreaId, IsPostBack);
    bindListCommonEvents(pnlListAreaId, IsPostBack, false, true, true, false);
    // テキストボックスEnter縦移動イベントバインド
    commonBindEnterToVerticalTabStep();
    // チェックボックス
    ChangeCheckBox();
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
}


// ○チェックボックス選択
function SelectCheckBox(obj, lineCnt) {

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("WF_SelectedIndex").value = lineCnt
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
        
        if (fieldNM === "TANKNUMBER") {
            //document.getElementById('WF_LeftMViewChange').value = 20;
            document.getElementById('WF_LeftMViewChange').value = 55;
        }
        else if (fieldNM === "PREORDERINGOILNAME") {
            document.getElementById('WF_LeftMViewChange').value = 46;
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
