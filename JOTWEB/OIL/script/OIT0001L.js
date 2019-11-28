// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    };

    addLeftBoxExtention(leftListExtentionTarget);

    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };

    ////更新ボタン活性／非活性
    //if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
    //    //活性
    //    document.getElementById("WF_ButtonUPDATE").disabled = "";
    //} else {
    //    //非活性 
    //    document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    //};
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack);

    // チェックボックス
    ChangeCheckBox();
};

// ○チェックボックス変更
function ChangeCheckBox() {

    var objTable = document.getElementById("pnlListArea_DL").children[0];

    var chkObjs = objTable.querySelectorAll("input[id^='chkpnlListAreaOPERATION']");
    var spnObjs = objTable.querySelectorAll("span[id^='hchkpnlListAreaOPERATION']");

    for (let i = 0; i < chkObjs.length; i++) {

        if (chkObjs[i] !== null) {
            if (spnObjs[i].innerText == "on") {
                chkObjs[i].checked = true;
            } else {
                chkObjs[i].checked = false;
            }
        }
    }
}


// ○チェックボックス選択
function SelectCheckBox(obj, lineCnt) {

    var objTable = document.getElementById("pnlListArea_DL").children[0];

    var chkObjs = objTable.querySelectorAll("input[id^='chkpnlListAreaOPERATION']");
    var spnObjs = objTable.querySelectorAll("span[id^='hchkpnlListAreaOPERATION']");

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {

        for (let i = 0; i < chkObjs.length; i++) {
            if (i == lineCnt - 1) {
                if (chkObjs[i].checked == true) {
                    document.getElementById("WF_SelectedIndex").value = i + 1;
                    document.getElementById("WF_FIELD").value = "on";
                } else {
                    spnObjs[i].innerText = "";
                    document.getElementById("WF_SelectedIndex").value = i + 1;
                    document.getElementById("WF_FIELD").value = "";
                }
            }
        }

        document.getElementById("WF_ButtonClick").value = "WF_CheckBoxSELECT";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }

}
