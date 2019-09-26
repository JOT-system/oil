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
};

// ○Repeater用処理（ラジオボタン）
function Rep_ButtonChange(CheckItem, Position) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        document.getElementById("WF_REP_SW").value = CheckItem;
        document.getElementById("WF_REP_POSITION").value = Position;
        document.getElementById("WF_ButtonClick").value = "WF_REP_RADIO";
        document.body.style.cursor = "wait";
        document.forms[0].submit();                            //aspx起動
    }
}

// ○Repeater行情報取得処理（行情報退避）
function Repeater_Lines(Line) {
    // Field名退避
    document.getElementById('WF_REP_POSITION').value = Line;
}
