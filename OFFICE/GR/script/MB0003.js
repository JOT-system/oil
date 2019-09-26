// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    // 更新ボタン活性／非活性
    if (document.getElementById("WF_MAPpermitcode").value == "TRUE") {
        // 活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        // 非活性
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    }

    // リストの共通イベント(ホイール、横スクロール)をバインド
    bindListCommonEvents(pnlListAreaId, IsPostBack);

};
