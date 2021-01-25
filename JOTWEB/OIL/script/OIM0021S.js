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

}

// 〇数値、.のみ入力可能
function CheckNumDot() {

    if ((event.keyCode < 48 || event.keyCode > 57) && event.keyCode != 46) {
        window.event.returnValue = false; // IEだと効かないので↓追加
        event.preventDefault(); // IEはこれで効く
    }
}
