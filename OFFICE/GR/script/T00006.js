// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    // 左ボックス

    // 右ボックス
    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };

    //光英送信ボタン活性／非活性
    if (document.getElementById('WF_IsHideKoueiButton').value == "0") {
        ////表示
        //document.getElementById("WF_ButtonPut").style.visibility = "visible";
        if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
            //活性
            document.getElementById("WF_ButtonPut").disabled = "";
        } else {
            //非活性 
            document.getElementById("WF_ButtonPut").disabled = "disabled";
        };
    } else {
        ////非表示 
        //document.getElementById("WF_ButtonPut").visibility = "hidden";
    };

    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack);
    addLeftBoxExtention(leftListExtentionTarget);

    // チェックボックス
    ChangeCheckBox();

};

// ○チェックボックス変更
function ChangeCheckBox() {

    var objTable = document.getElementById("pnlListArea_DL").children[0];

    var chkObjs = objTable.querySelectorAll("input[id^='chkpnlListAreaOPERATION']");
    var spnObjs = objTable.querySelectorAll("span[id^='hchkpnlListAreaOPERATION']");

    for (let i = 0; i < chkObjs.length; i++) {

        if (chkObjs[i] !== null) {
            if (spnObjs[i].innerText == "on") {
                chkObjs[i].checked = true;
            } else {
                chkObjs[i].checked = false;
            }
        }
    }
}
// ○ダウンロード処理
function f_DownLoad() {
    // リンク参照
    location.href = document.getElementById("WF_ZipURL").value;
};
