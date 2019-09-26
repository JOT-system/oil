// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    };
    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };
    addLeftBoxExtention(leftListExtentionTarget);
    // ○画面切替用処理（表示/非表示切替「ヘッダー、ディティール」）
    if (document.getElementById('WF_IsHideDetailBox').value == "0") {
        document.getElementById("headerbox").style.display = "none";
        document.getElementById("detailbox").style.display = "block";
    } else {
        document.getElementById("headerbox").style.display = "block";
        document.getElementById("detailbox").style.display = "none";
        /* 共通一覧のスクロールイベント紐づけ */
        bindListCommonEvents(pnlListAreaId, IsPostBack);
    };

};