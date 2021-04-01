// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    /* 全部消す */
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    /* 左ボックス */
    if (document.getElementById("WF_LeftboxOpen").value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    }

    /* 右ボックス */
    if (document.getElementById("WF_RightboxOpen").value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    /* 左ボックス拡張機能追加 */
    addLeftBoxExtention(leftListExtentionTarget);

}

/* 全選択ボタン押下時 */
function checkAll() {
    let chkObjList = document.querySelectorAll(".detailbox .grc0001Wrapper input[type=checkbox]");
    for (let i = 0; i < chkObjList.length; i++) {
        chkObjList[i].checked = true;
    }
}
/* 選択解除ボタン押下j */
function unCheck() {
    let chkObjList = document.querySelectorAll(".detailbox .grc0001Wrapper input[type=checkbox]");
    for (let i = 0; i < chkObjList.length; i++) {
        chkObjList[i].checked = false;
    }
}

/**
 * 日付文字列変換(YYYY/MM/DD形式)
 * @param {any} date 日付
 */
function getStringFormateDate(date) {
    return date.getFullYear() + "/" + ('0' + (date.getMonth() + 1)).slice(-2) + "/" + ('0' + date.getDate()).slice(-2);
}

/**
 * 日付変更ボタンクリック処理
 * @param {any} name 名称
 * @param {any} type 識別(1:当日分、2:当日迄、3:前日分、4:前日迄)
 */
function clickBtnDateChange(name, type) {
    var stTextBox = document.querySelectorAll(".detailbox .downLoadArea input[type=text][name*=txt" + name + "StYmd].calendarIcon")[0];
    var edTextBox = document.querySelectorAll(".detailbox .downLoadArea input[type=text][name*=txt" + name + "EdYmd].calendarIcon")[0];

    if (type === 1) {
        var onTheDay = new Date();
        stTextBox.value = getStringFormateDate(onTheDay);
        edTextBox.value = getStringFormateDate(onTheDay);
    }
    else if (type === 2) {
        var onTheDay = new Date();
        var fisrtDay = new Date(onTheDay.getFullYear(), onTheDay.getMonth(), 1);
        stTextBox.value = getStringFormateDate(fisrtDay);
        edTextBox.value = getStringFormateDate(onTheDay);
    }
    else if (type === 3) {
        var prevDay = new Date();
        prevDay.setDate(prevDay.getDate() - 1);
        stTextBox.value = getStringFormateDate(prevDay);
        edTextBox.value = getStringFormateDate(prevDay);

    }
    else if (type === 4) {
        var prevDay = new Date();
        prevDay.setDate(prevDay.getDate() - 1);
        var firstDay = new Date(prevDay.getFullYear(), prevDay.getMonth(), 1);
        stTextBox.value = getStringFormateDate(firstDay);
        edTextBox.value = getStringFormateDate(prevDay);
    }

    return;
}

/**
 * ロード時処理(共通処理により、カレンダーアイコン付きTextBoxの幅がcalc(100% + 1px)に補正されるのを指定幅に戻す)
 */
window.addEventListener('load', function () {
    /* 帳票条件エリアのカレンダーアイコン付きテキストボックスのstyleを削除 */
    let queryString = "#detailbox #downLoadArea input[type=text].calendarIcon"
    var targetTextBoxList = document.querySelectorAll(queryString);
    if (targetTextBoxList != null) {
        for (let i = 0; i < targetTextBoxList.length; i++) {
            let inputObj = targetTextBoxList[i];
            inputObj.removeAttribute('style')
        }
    }
});
