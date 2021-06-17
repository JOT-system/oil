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


    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value === "TRUE") {
        //活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    }
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);
    /* 削除済みレコードの背景色を変更 */
    changeRowBGColor();
}

/* 削除済みレコードの背景色を変更 */
function changeRowBGColor() {
    /* 削除フラグ表示位置の取得 */
    let delflgIdx = document.getElementById("WF_DELFLG_INDEX").value;
    /* 一覧のヘッダ部分を取得 */
    let dlTrList = document.getElementById("pnlListArea_DL").getElementsByTagName("tr");
    /* 一覧の内容を取得 */
    let drTrList = document.getElementById("pnlListArea_DR").getElementsByTagName("tr");
    /*
     * 削除フラグ表示位置が初期化されていて、値がNON以外の場合は
     * 各行から削除フラグの値を取得し、それが削除済みの場合は行の背景色を灰色に設定する。
     */
    if (delflgIdx != "NON") {
        let didx = parseInt(new String(delflgIdx));
        for (let i = 0; i < drTrList.length; i++) {
            /* 削除フラグを取得 */
            let delflg = drTrList[i].getElementsByTagName("td")[didx].innerText;
            if (delflg == "1") {
                dlTrList[i].style.backgroundColor = "gray";
                drTrList[i].style.backgroundColor = "gray";
            }
        }
    }


}
