// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    };
    if (document.getElementById('WF_LeftboxOpen').value == "OpenTbl") {
        document.getElementById("LF_LEFTBOX").style.width = "40em";
    };
    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };
    //再開ボタン活性／非活性
    if (document.getElementById('WF_Restart').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonRESTART").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonRESTART").disabled = "disabled";
    };
    addLeftBoxExtention(leftListExtentionTarget);

};


function setSTYMD(YMD) {
    document.getElementById("WF_STYMD").value = YMD;
    document.getElementById("WF_ENDYMD").value = YMD;
}