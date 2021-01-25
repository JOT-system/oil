// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    //document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen").value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    }

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);

}

// ロード時処理(共通処理により、GridView内のアイコン付きTextBoxの幅がcalc(100% + 1px)に補正される為、100%に戻す)
window.addEventListener('load', function () {
    let queryString = "table#WF_COSTLISTTBL > tbody > tr > td > span > input[type=text]"
    var targetTextBoxList = document.querySelectorAll(queryString);
    if (targetTextBoxList != null) {
        for (let i = 0; i < targetTextBoxList.length; i++) {
            let inputObj = targetTextBoxList[i];
            inputObj.removeAttribute("style")
        }
    }
});

function selectAll(val) {
    let queryString = "table#WF_COSTLISTTBL > tbody > tr > td > input[type=checkbox]"
    var targetCheckBoxList = document.querySelectorAll(queryString);
    if (targetCheckBoxList != null) {
        for (let i = 0; i < targetCheckBoxList.length; i++) {
            let checkBox = targetCheckBoxList[i];
            checkBox.checked = val
        }
    }
}

// ○営業所ボタン押下処理
function OfficeButtonClick(hiddnId) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        // 要素のID、営業所コードを設定
        if (hiddnId != null) {
            // 押下されたボタンを設定
            document.getElementById("WF_ButtonClick").value = "WF_ButtonRELOAD";
            document.getElementById("WF_OFFICEHDN_ID").value = hiddnId
            document.getElementById("MF_SUBMIT").value = "TRUE";
            document.body.style.cursor = "wait";
            document.forms[0].submit();
        } else {
            // 選択中の営業所ボタンを探し、clickイベントを発火
            let queryString = "input[id^='WF_OFFICEBTN_']"
            var buttunList = document.querySelectorAll(queryString);
            if (buttunList != null) {
                for (let i = 0; i < buttunList.length; i++) {
                    var classStr = buttunList[i].getAttribute("class");
                    if (classStr.indexOf('selected') != -1) {
                        // clickイベントを生成し発火
                        var evt = document.createEvent("HTMLEvents");
                        evt.initEvent('click', true, true);
                        return buttunList[i].dispatchEvent(evt);
                    }
                }
            }
        }
    } else {
        return false;
    }
}
