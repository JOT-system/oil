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
    if (document.getElementById('WF_MAPpermitcode').value === "TRUE") {
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
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    }
    /* フッターの高さ調整 */
    AdjustHeaderFooterContents('detailbox');
    /* 共通一覧のスクロールイベント紐づけ */
    document.getElementById(pnlListAreaId).style.display = "none"; // 一旦レスポンス用
    bindListCommonEvents(pnlListAreaId, IsPostBack);
    // チェックボックス
    ChangeCheckBox();
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
        else if (fieldNM === "ORDERINGOILNAME") {
            document.getElementById('WF_LeftMViewChange').value = 46;
        }
        else if (fieldNM === "SHIPPERSNAME") {
            document.getElementById('WF_LeftMViewChange').value = 42;
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
