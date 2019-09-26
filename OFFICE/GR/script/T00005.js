// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    }else if (document.getElementById('WF_LeftboxOpen').value == "OpenSTbl") {
        document.getElementById("LF_LEFTBOX").style.width = "33em";
    }else if (document.getElementById('WF_LeftboxOpen').value == "OpenCTbl") {
        document.getElementById("LF_LEFTBOX").style.width = "48em";
    };
    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };

    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    };
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack);
    addLeftBoxExtention(leftListExtentionTarget);
};

// ○作業区分変更
function WorkKbnChange() {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        document.getElementById('WF_ButtonClick').value = "WF_WORKKBNChange";
        document.body.style.cursor = "wait";
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    };
}