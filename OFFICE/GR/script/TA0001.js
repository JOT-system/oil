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
        document.getElementById("headerbox").style.visibility = "hidden";
        document.getElementById("detailbox").style.visibility = "visible";
        // 明細画面のスクロールをTOP表示に切替
        f_ScrollTop(0, 0)
    } else {
        document.getElementById("headerbox").style.visibility = "visible";
        document.getElementById("detailbox").style.visibility = "hidden";
        /* 共通一覧のスクロールイベント紐づけ */
        bindListCommonEvents(pnlListAreaId, IsPostBack);
    };

};
// ○Repeater処理（スクロール切替）
function f_ScrollTop(x, y) {
    document.all('WF_DViewRep1_Area').scrollTop = x;
    document.all('WF_DViewRep1_Area').scrollLeft = y;

};
// ○Repeater行変更処理
function Repeater_focus(LineCnt, ColCnt) {
    document.getElementById("WF_REP_LineCnt").value = LineCnt;
    document.getElementById("WF_REP_ColCnt").value = ColCnt;
};

// ○SETECTOR行情報取得処理（行情報退避用）
function SELECTOR_Click(CODE) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        // Field名退避
        document.getElementById('WF_ButtonClick').value = "WF_SELECTOR_SW_Click";
        document.getElementById('WF_SELECTOR_Posi').value = CODE;
        document.body.style.cursor = "wait";
        document.forms[0].submit();                             //aspx起動
    } else {
        return false;
    };
};

