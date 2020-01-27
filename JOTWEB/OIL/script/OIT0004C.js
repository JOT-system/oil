// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    //document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    }

    addLeftBoxExtention(leftListExtentionTarget);

    if (document.getElementById('WF_RightboxOpen').value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }
    //当画面のテキストボックスは全て数字の為共通関数を通す(共通関数は小数点拒否の為、要確認)
    //let txtObjList = document.forms[0].querySelectorAll("#headerboxOnly input[type=text]");

}

