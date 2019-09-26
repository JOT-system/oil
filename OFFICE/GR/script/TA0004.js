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
    if (document.getElementById('WF_SELECTOR_Chg').value == '0') {
        document.all('ORGSelect').scrollTop = Number(document.getElementById("WF_SaveSY").value);
        document.all('ORGSelect').scrollLeft = Number(document.getElementById("WF_SaveSX").value);
    };
    if (document.getElementById('WF_SELECTOR_Chg').value == '1') {
        document.all('STAFFSelect').scrollTop = Number(document.getElementById("WF_SaveSY").value);
        document.all('STAFFSelect').scrollLeft = Number(document.getElementById("WF_SaveSX").value);
    };
    if (document.getElementById('WF_SELECTOR_Chg').value == '2') {
        document.all('GSHABANSelect').scrollTop = Number(document.getElementById("WF_SaveSY").value);
        document.all('GSHABANSelect').scrollLeft = Number(document.getElementById("WF_SaveSX").value);
    };
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
function SELECTOR_Click(tabNo, NAME) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        // Field名退避
        document.getElementById('WF_SELECTOR_Chg').value = tabNo;
        document.getElementById('WF_SELECTOR_SW').value = "ON";
        document.getElementById('WF_ButtonClick').value = "WF_SELECTOR_SW_Click";
        if (tabNo == '0') {
            document.getElementById("WF_SaveSX").value = document.all('ORGSelect').scrollLeft;
            document.getElementById("WF_SaveSY").value = document.all('ORGSelect').scrollTop;
            document.getElementById('WF_SELECTOR_PosiORG').value = NAME;
        };
        if (tabNo == '1') {
            document.getElementById("WF_SaveSX").value = document.all('STAFFSelect').scrollLeft;
            document.getElementById("WF_SaveSY").value = document.all('STAFFSelect').scrollTop;
            document.getElementById('WF_SELECTOR_PosiSTAFF').value = NAME;
        };
        if (tabNo == '2') {
            document.getElementById("WF_SaveSX").value = document.all('GSHABANSelect').scrollLeft;
            document.getElementById("WF_SaveSY").value = document.all('GSHABANSelect').scrollTop;
            document.getElementById('WF_SELECTOR_PosiGSHABAN').value = NAME;
        };
        document.body.style.cursor = "wait";
        document.forms[0].submit();                             //aspx起動
    } else {
        return false;
    };
};

// ○セレクター用処理（ラジオボタン）
function selectorChange(tabNo) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        document.getElementById("WF_SaveSX").value = 0;
        document.getElementById("WF_SaveSY").value = 0;

        document.getElementById('WF_SELECTOR_Chg').value = tabNo;
        document.getElementById('WF_ButtonClick').value = "WF_SELECTOR_CHG";
        document.body.style.cursor = "wait";
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    };
};

