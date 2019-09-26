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

    addLeftBoxExtention(leftListExtentionTarget);
    // ○画面切替用処理（表示/非表示切替「ヘッダー、ディティール」）
    if (document.getElementById('WF_IsHideDetailBox').value == "0") {
        document.getElementById("Operation").style.display = "none";
        document.getElementById("leftMenubox").style.display = "none";
        document.getElementById("STAFFSelect").style.display = "none";
        document.getElementById("divListArea").style.display = "none";

        document.getElementById("detailbox").style.display = "block";
        // 明細画面のスクロールをTOP表示に切替
        //f_ScrollTop(0, 0)
    } else {
        document.getElementById("Operation").style.display = "block";
        document.getElementById("leftMenubox").style.display = "block";
        document.getElementById("STAFFSelect").style.display = "block";
        document.getElementById("divListArea").style.display = "block";

        document.getElementById("detailbox").style.display = "none";
        /* 共通一覧のスクロールイベント紐づけ */
        bindListCommonEvents(pnlListAreaId, IsPostBack);
    };
};

// ○SETECTOR行情報取得処理（行情報退避用）
function SELECTOR_Click(NAME) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        document.getElementById("WF_SaveSX").value = document.all('STAFFSelect').scrollLeft;
        document.getElementById("WF_SaveSY").value = document.all('STAFFSelect').scrollTop;
        // Field名退避
        document.getElementById('WF_SELECTOR_Posi').value = NAME;
        document.getElementById('WF_SELECTOR_SW').value = "ON";
        document.getElementById('WF_ButtonClick').value = "WF_SELECTOR_SW_Click";
        document.body.style.cursor = "wait";
        document.forms[0].submit();                             //aspx起動
    } else {
        return false;
    };
};

// ○Repeater行情報取得処理（行情報退避用）
function Repeater_Gyou(Line) {
    // Field名退避
    document.getElementById('WF_REP_POSITION').value = Line;
};

// ○ダウンロード処理(ZIP)
function Z_DownLoad() {
    // リンク参照
    location.href = document.getElementById("WF_DownURL").value;
    document.getElementById("headerbox").style.visibility = "visible";
    document.getElementById("detailbox").style.visibility = "hidden";
    document.getElementById('WF_BOXChange').value = "headerbox";
};

// ○ダウンロード処理
function f_DownLoad() {
    window.open(document.getElementById("WF_DownURL").value, "view", "_blank");
};

