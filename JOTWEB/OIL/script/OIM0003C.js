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

// ロード時処理(共通処理により、GridView内のアイコン付きTextBoxの幅がcalc(100% + 1px)に補正される為、100%に戻す)
window.addEventListener('load', function () {
    let queryString = "table#WF_OILTERMTBL > tbody > tr > td > span > input[type=text]"
    var targetTextBoxList = document.querySelectorAll(queryString);
    if (targetTextBoxList != null) {
        for (let i = 0; i < targetTextBoxList.length; i++) {
            let inputObj = targetTextBoxList[i];
            inputObj.style.width = "100%"
        }
    }
});
