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
    document.all('STAFFSelect').scrollTop = Number(document.getElementById("WF_SaveSY").value);
    document.all('STAFFSelect').scrollLeft = Number(document.getElementById("WF_SaveSX").value);
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack);
    addLeftBoxExtention(leftListExtentionTarget);
};

// ○Repeater行変更処理
function Repeater_focus(LineCnt, ColCnt) {
    document.getElementById("WF_REP_LineCnt").value = LineCnt;
    document.getElementById("WF_REP_ColCnt").value = ColCnt;
};

// ○SETECTOR行情報取得処理（行情報退避用）
function SELECTOR_Click( NAME) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        // Field名退避
        document.getElementById('WF_ButtonClick').value = "WF_SELECTOR_SW_Click";
        document.getElementById("WF_SaveSX").value = document.all('STAFFSelect').scrollLeft;
        document.getElementById("WF_SaveSY").value = document.all('STAFFSelect').scrollTop;
        document.getElementById('WF_SELECTOR_Posi').value = NAME;
        document.body.style.cursor = "wait";
        document.forms[0].submit();                             //aspx起動
    } else {
        return false;
    };
};


