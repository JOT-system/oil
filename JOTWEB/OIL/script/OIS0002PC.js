// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    //// 左ボックス
    //if (document.getElementById("WF_LeftboxOpen").value === "Open") {
    //    document.getElementById("LF_LEFTBOX").style.display = "block";
    //}

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    //// 左ボックス拡張機能追加
    //addLeftBoxExtention(leftListExtentionTarget);

}