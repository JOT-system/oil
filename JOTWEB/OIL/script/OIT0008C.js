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
//window.addEventListener('load', function () {
//    let queryString = "table#WF_COSTLISTTBL > tbody > tr > td > span > input[type=text]"
//    var targetTextBoxList = document.querySelectorAll(queryString);
//    if (targetTextBoxList != null) {
//        for (let i = 0; i < targetTextBoxList.length; i++) {
//            let inputObj = targetTextBoxList[i];
//            inputObj.removeAttribute("style")
//        }
//    }
//});

function selectAll(val) {
    let queryString = "table#WF_COSTDETAILTBL > tbody > tr > td > span > input[type=checkbox]"
    var targetCheckBoxList = document.querySelectorAll(queryString);
    if (targetCheckBoxList != null) {
        for (let i = 0; i < targetCheckBoxList.length; i++) {
            let checkBox = targetCheckBoxList[i];
            checkBox.checked = val
        }
    }
}

/* 
 * ○ドロップダウンリスト選択変更
 */
function selectChangeDdl(ddl) {

    var name = '' + ddl.name;
    if (ddl.id.indexOf('TRKBNLIST') !== -1) {
        var trKbn = document.getElementsByName(name.replace('TRKBNLIST', 'TRKBN'));
        var trKbnName = document.getElementsByName(name.replace('TRKBNLIST', 'TRKBNNAME'));
        if (trKbn.length > 0) {
            trKbn[0].value = ddl.value;
        }
        if (trKbnName.length > 0) {
            trKbnName[0].value = ddl.options[ddl.selectedIndex].innerHTML;
        }
    }
    if (ddl.id.indexOf('POSTOFFICENAMELIST') !== -1) {
        var postOfficeCode = document.getElementsByName(name.replace('POSTOFFICENAMELIST', 'POSTOFFICECODE'));
        var postOfficeName = document.getElementsByName(name.replace('POSTOFFICENAMELIST', 'POSTOFFICENAME'));
        if (postOfficeCode.length > 0) {
            postOfficeCode[0].value = ddl.value;
        }
        if (postOfficeName.length > 0) {
            postOfficeName[0].value = ddl.options[ddl.selectedIndex].innerHTML;
        }
    }
    if (ddl.id.indexOf('CONSIGNEENAMELIST') !== -1) {
        var consigneeCode = document.getElementsByName(name.replace('CONSIGNEENAMELIST', 'CONSIGNEECODE'));
        var consigneeName = document.getElementsByName(name.replace('CONSIGNEENAMELIST', 'CONSIGNEENAME'));
        if (consigneeCode.length > 0) {
            consigneeCode[0].value = ddl.value;
        }
        if (consigneeName.length > 0) {
            consigneeName[0].value = ddl.options[ddl.selectedIndex].innerHTML;
        }
    }
    //if (ddl.id.indexOf('ORDERINGOILNAMELIST') !== -1) {
    //    var keyCode = '' + ddl.value;
    //    var code1 = '';
    //    var code2 = '';
    //    var code3 = '';
    //    var oilCode = document.getElementsByName(name.replace('ORDERINGOILNAMELIST', 'OILCODE'));
    //    var oilName = document.getElementsByName(name.replace('ORDERINGOILNAMELIST', 'OILNAME'));
    //    var orderingType = document.getElementsByName(name.replace('ORDERINGOILNAMELIST', 'ORDERINGTYPE'));
    //    var orderingOilName = document.getElementsByName(name.replace('ORDERINGOILNAMELIST', 'ORDERINGOILNAME'));

    //    if (keyCode.indexOf('/') !== -1) {
    //        code1 = keyCode.split('/')[0]
    //        code2 = keyCode.split('/')[1]
    //        code3 = keyCode.split('/')[2]
    //    }
    //    if (oilCode.length > 0) {
    //        oilCode[0].value = code1;
    //    }
    //    if (oilName.length > 0) {
    //        oilName[0].value = code2;
    //    }
    //    if (orderingType.length > 0) {
    //        orderingType[0].value = code3;
    //    }
    //    if (orderingOilName.length > 0) {
    //        orderingOilName[0].value = ddl.options[ddl.selectedIndex].innerHTML;
    //    }
    //}
}

/*
 * 〇金額入力ブラーイベント 
 */
function amountOnBlur(amount) {
    var name = '' + amount.name;
    var amountValueStr = String(amount.value).replace(',', '');
    var amountValue = parseInt(amountValueStr);

    /* 税区分を取得 */
    var hdnTaxKbn = document.getElementById('HdnTaxKbn');
    /* 税率を取得 */
    var consumptionTaxLabel = document.getElementsByName(name.replace('AMOUNT', 'CONSUMPTIONTAX'));
    if (consumptionTaxLabel.length > 0) {
        var consumptionTaxValue = parseFloat(consumptionTaxLabel[0].value);
        /* 税区分が非課税以外の場合は、税額を計算 */
        var taxValue = 0;
        if (hdnTaxKbn.value != "3") {
            var taxValue = Math.round(amountValue * consumptionTaxValue);
        }
        /* 税額をセット */
        var taxLabel = consumptionTaxLabel[0].parentNode.childNodes[1];
        taxLabel.innerHTML = String(taxValue).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, '$1,');
        /* 総額を更新 */
        var totalLabel = consumptionTaxLabel[0].parentNode.parentNode.childNodes[8].childNodes[1];
        totalLabel.innerHTML = String(amountValue + taxValue).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, '$1,');
    }
    amount.value = String(amountValue).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, '$1,');
}

/*
 * 〇数値入力ブラーイベント
 */
function numberOnBlur(qt, digit) {
    var replaceNum = qt.value.toString().replace(',', '');
    var splitNum = replaceNum.split('.');
    var replaceNum = splitNum[0].replace(/(\d)(?=(\d\d\d)+(?!\d))/g, '$1,');
    if (digit > 0) {

        if (splitNum.length > 1) {
            replaceNum = replaceNum + '.' + String(splitNum[1] + '000').slice(0, digit);
        }
        else {
            replaceNum = replaceNum + '.' + String('000').slice(0, digit);
        }
    }

    qt.value = replaceNum;
}