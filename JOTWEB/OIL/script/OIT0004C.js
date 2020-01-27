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
    let suggestCol = document.forms[0].querySelectorAll("div.dataColumn > div.values");
    bindSuggestSummary(suggestCol);
}

// 〇提案数の合計計算イベントバインド(暫定関数)
function bindSuggestSummary(suggestColumnDivList) {
    //日付・列車列のループ
    
    for (let i = 0; i < suggestColumnDivList.length; i++) {
        let suggestColDiv = suggestColumnDivList[i];
        /* 加算対象のテキストボックス */
        let suggestColTextList = suggestColDiv.querySelectorAll("div.num:not([data-oilcode=Summary]) input[type=text]");
        /* 合計値格納テキストボックス */
        let summaryColText = suggestColDiv.querySelectorAll("div.num[data-oilcode=Summary] input[type=text]")[0];

        for (let j = 0; j < suggestColTextList.length; j++) {
            let targetText = suggestColTextList[j];
            /* テキストボックス変更イベントをバインド */
            targetText.addEventListener('change', (function (suggestColTextList, summaryColText) {
                return function () {
                    summarySuggestValues(suggestColTextList, summaryColText);
                };
            })(suggestColTextList, summaryColText), false);
            /* バインド時一度実行する */
            if (j === 0) {
                summarySuggestValues(suggestColTextList, summaryColText);
            }
        }

    }
}
// 〇提案数の合計計算イベント
//   引数:suggestColTextList ・・・ 加算対象テキストボックス
//        summaryColText     ・・・ 合計テキストボックス
function summarySuggestValues(suggestColTextList, summaryColText) {
    let suggestColSummary = 0;
    for (let i = 0; i < suggestColTextList.length; i++) {
        let suggestColTextId = suggestColTextList[i].id;
        let suggestColText = document.getElementById(suggestColTextId);
        if (suggestColText !== null) {
            let itemVal = suggestColText.value.replace(/,/g, '');
            if (!isNaN(itemVal)) {
                suggestColSummary = suggestColSummary + Number(itemVal);
            }
        }
    }
    let summaryColTextObj = document.getElementById(summaryColText.id);
    if (summaryColTextObj !== null) {
        summaryColTextObj.value = suggestColSummary;
    }

}