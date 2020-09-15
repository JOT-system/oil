// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    //document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen").value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    }

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);
    let bypassFlagObj = document.getElementById('WF_LoadAfterBackOrForward');
    if (bypassFlagObj !== null) {
        if (bypassFlagObj.value !== '') {
            document.getElementById("MF_SUBMIT").value = "TRUE";
            document.forms[0].style.visibility = 'hidden';
            document.forms[0].submit();
        }
    }
}