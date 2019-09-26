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
};

//〇会社コード切替時処理
function CompChange() {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        //押下されたボタンを設定
        document.getElementById("WF_ButtonClick").value = "WF_CompChange";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    } else {
        return false;
    }
}