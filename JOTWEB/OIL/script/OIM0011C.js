// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    }

    addLeftBoxExtention(leftListExtentionTarget);

    if (document.getElementById('WF_RightboxOpen').value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

}

// 〇数値のみ入力可能
function CheckNum() {
    if (event.keyCode < 48 || event.keyCode > 57) {
        window.event.returnValue = false; // IEだと効かないので↓追加
        event.preventDefault(); // IEはこれで効く
    }
}
