// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    //document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    };

    addLeftBoxExtention(leftListExtentionTarget);

    if (document.getElementById('WF_RightboxOpen').value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };

    ////更新ボタン活性／非活性
    //if (document.getElementById('WF_MAPpermitcode').value === "TRUE") {
    //    //活性
    //    document.getElementById("WF_ButtonUPDATE").disabled = "";
    //} else {
    //    //非活性 
    //    document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    //};
    ///* 共通一覧のスクロールイベント紐づけ */
    //bindListCommonEvents(pnlListAreaId, IsPostBack);


}

