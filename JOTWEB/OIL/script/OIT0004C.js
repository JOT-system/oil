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
    document.forms[0].style.display = 'none'; //高速化対応 一旦非表示にしDOM追加ごとの再描画を抑止
    bindSuggestSummary(suggestCol);
    bindDipsOiltypeStockList();
    document.forms[0].style.display = 'block'; //高速化対応 一旦非表示にしDOM追加ごとの再描画を抑止
    // 数字入力のみ可能にする共通関数KeyDown共通関数を仕込む
    let numInputBoxList = document.forms[0].querySelectorAll("#WF_INVENTORYDAYS, #pnlSuggestList input[type=text],#pnlStockList input[type=text]");
    bindNumericKeyPressOnly(numInputBoxList);
    //フォーカスを合わせる
    forcusObj();
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
/* 油種行の表示非表示切替イベントバインド */
function bindDipsOiltypeStockList() {
    let listDispObj = document.getElementById('lstDispStockOilType');
    if (listDispObj === null) {
        return;
    }
    let stockListObj = document.getElementById('divStockList');
    let opts = listDispObj.options;
    for (let i = 0; i < opts.length; i++) {
        let optItm = opts[i];
        let oilcode = optItm.value;
        let oilName = optItm.text;
        let optIdx = i;
        let stockRowTitle = stockListObj.querySelectorAll('div.oilTypeData[data-oilcode="' + oilcode + '"] > div.col1 > div > span')[0];
        if (stockRowTitle !== null) {
            stockRowTitle.dataset.tiptext = 'クリックして隠す';
            stockRowTitle.addEventListener('click', (function (oilcode, oilName, optIdx) {
                return function () {
                    DipsOiltypeStockList(oilcode, oilName, optIdx);
                };
            })(oilcode, oilName, optIdx), false);
        }
        if (optItm.selected === false) {
            optItm.selected = true;
            DipsOiltypeStockList(oilcode, oilName, optIdx);
        }
    }
}
/* 油種行の表示非表示切替処理 */
function DipsOiltypeStockList(oilcode, oilName, optIdx) {
    let listDispObj = document.getElementById('lstDispStockOilType');
    if (listDispObj === null) {
        return;
    }
    let showBox = document.getElementById('divEmptyBox');

    let targetOpt = listDispObj.options[optIdx];
    let stockListObj = document.getElementById('divStockList');
    let stockRow = stockListObj.querySelectorAll('div.oilTypeData[data-oilcode="' + oilcode + '"]')[0];
    if (targetOpt.selected) {
        stockRow.style.display = "none";
        targetOpt.selected = false;
        let divObj = document.createElement("div");
        let divObjid = 'stockDispShowButton' + oilcode;
        divObj.id = divObjid;
        divObj.innerHTML = oilName;
        divObj.dataset.tiptext = 'クリックして表示';
        divObj.addEventListener('click', (function (oilcode, oilName, optIdx, divObjid) {
            return function () {
                DipsOiltypeStockList(oilcode, oilName, optIdx);
                let removeObj = document.getElementById(divObjid);
                removeObj.parentNode.removeChild(removeObj);
            };
        })(oilcode, oilName, optIdx, divObjid), false);
        divObj.style.order = optIdx + 1;
        showBox.appendChild(divObj);

    } else {
        stockRow.style.display = "";
        targetOpt.selected = true;
    }
    let stockRows = stockListObj.querySelectorAll('div.oilTypeData');
    let lastStockRow = null;
    for (let i = 0; i < stockRows.length; i++) {
        stockRows[i].classList.remove('lastRow');
        if (stockRows[i].style.display !== 'none') {
            lastStockRow = stockRows[i];
        }
    }

    if (lastStockRow !== null) {
        lastStockRow.classList.add('lastRow');
    }

}
//〇フォーカス合わせ処理
function forcusObj() {
    let forcusIdObj = document.getElementById('hdnForcusObjId');
    // フォーカス処理不要の場合
    if (forcusIdObj === null) {
        return;
    }
    let targetId = forcusIdObj.value;
    forcusIdObj.name = "";
    // フォーカスを充てるオブジェクトが存在していない場合はそのまま終了
    let targetObj = document.getElementById(targetId);
    if (targetObj === null) {
        return;
    }
    // フォーカス処理必要な場合
    let msgBoxObj = document.getElementById('pnlCommonMessageWrapper');
    if (msgBoxObj === null) {
        setTimeout(function () {
            targetObj.focus({ preventScroll: false });
            targetObj.select();
        }, 10);
        return;
    }
    // ダイアログクローズ後のフォーカス
    let closeBtnObj = document.getElementById('btnCommonMessageOk');
    if (closeBtnObj === null) {
        return;
    }
    // ダイアログを閉じるタイミングでフォーカスを合わせる
    closeBtnObj.addEventListener('click', (function (targetId) {
        return function () {
            // 画像クリック時にテキストボックスのダブルクリックイベント発火
            document.getElementById(targetId).focus();
            document.getElementById(targetId).select();
        };
    })(targetId), false);
}
/* 数値のみのKeypressのみを許可するイベントバインド */
function bindNumericKeyPressOnly(targetTextBoxList) {
    for (let i = 0; i < targetTextBoxList.length; i++) {
        let textObj = targetTextBoxList[i];
        /* keypressはIeでは動かない */
        textObj.addEventListener('keypress', CheckNum);
        textObj.style.imeMode = 'disabled';
        /* 桁数 */
        if (textObj.id === 'WF_INVENTORYDAYS') {
            textObj.maxLength = 1;
        }
        if (textObj.id.indexOf('txtSuggestValue') !== -1) {
            textObj.maxLength = 3;
        }
        if (textObj.id.indexOf('txtSend') !== -1) {
            textObj.maxLength = 5;
        }
    }
}