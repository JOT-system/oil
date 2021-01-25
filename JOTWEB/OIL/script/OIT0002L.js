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

        //### 20201216 START 指摘票対応(No263)全体 #######################################
        // (過去)貨車連結順序表の場合
        if (document.getElementById('WF_PastLinkList').value === "TRUE") {
            //非活性 
            document.getElementById("WF_ButtonALLSELECT").disabled = "disabled";
            document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "disabled";
            document.getElementById("WF_ButtonLINE_LIFTED").disabled = "disabled";
            document.getElementById("WF_ButtonINSERT").disabled = "disabled";
        } else {
            //活性
            document.getElementById("WF_ButtonALLSELECT").disabled = "";
            document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "";
            document.getElementById("WF_ButtonLINE_LIFTED").disabled = "";
            document.getElementById("WF_ButtonINSERT").disabled = "";
        }
        //### 20201216 END   指摘票対応(No263)全体 #######################################

    } else {
        //非活性 
        document.getElementById("WF_ButtonALLSELECT").disabled = "disabled";
        document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "disabled";
        document.getElementById("WF_ButtonLINE_LIFTED").disabled = "disabled";
        document.getElementById("WF_ButtonINSERT").disabled = "disabled";
    }

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

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
        let chkObj = obj.querySelector("input");
        if (chkObj === null) {
            return;
        }
        if (chkObj.disabled === true) {
            return;
        }

        document.getElementById("WF_SelectedIndex").value = lineCnt
        document.getElementById("WF_ButtonClick").value = "WF_CheckBoxSELECT";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }

}
