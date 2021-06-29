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

/**
 * 現在経年自動計算
 * @param {any} getdate
 */
function getDateBlur(getdate) {
    /* 現在経年の要素取得 */
    var progressYear = document.getElementById("WF_PROGRESSYEAR");

    /* 取得年月日が日付形式であるかチェック（日付でなければ終了） */
    if (!((new String(getdate.value)).match(/^[0-9]{4}\/[0-9]{1,2}\/[0-9]{1,2}$/))) {
        return;
    }

    var getDateYear = new Number(new String(getdate.value).split("/")[0]);
    var nowYear = (new Date()).getFullYear();
    var yearDiff = nowYear - getDateYear;

    /* 現在年 */
    if (yearDiff > 0) {
        progressYear.value = nowYear - getDateYear;
    } else {
        progressYear.value = 0
    }
}