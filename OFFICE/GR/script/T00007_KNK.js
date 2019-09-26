// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen").value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    }

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonNIPPOEDIT").disabled = "";
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonNIPPOEDIT").disabled = "disabled";
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    };

    //日報ボタン活性／非活性
    if (document.getElementById('WF_NIPPObtn').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonNIPPOEDIT").hidden = "";
        document.getElementById("WF_ButtonNIPPO").hidden = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonNIPPOEDIT").hidden = "hidden";
        document.getElementById("WF_ButtonNIPPO").hidden = "hidden";
    };

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);

    // リストの共通イベント(ホイール、横スクロール)をバインド
    bindListCommonEvents(pnlListAreaId, IsPostBack, false, true, true, false);

};

// ○ディテール(タブ切替)処理
function DtabChange(tabNo) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        document.getElementById('WF_DTABChange').value = tabNo;
        document.getElementById('WF_ButtonClick').value = "WF_DTABChange";
        document.body.style.cursor = "wait";
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    }
}

function TTL_SUM() {

    var kaitenSum = 0;

    if (document.getElementById('WF_KAITENCNT_WHITE1').value !== "" && isFinite(document.getElementById('WF_KAITENCNT_WHITE1').value)){
        kaitenSum = kaitenSum + eval(document.getElementById('WF_KAITENCNT_WHITE1').value);
    }
    if (document.getElementById('WF_KAITENCNT_BLACK1').value !== "" && isFinite(document.getElementById('WF_KAITENCNT_BLACK1').value)){
        kaitenSum = kaitenSum + eval(document.getElementById('WF_KAITENCNT_BLACK1').value);
    }
    if (document.getElementById('WF_KAITENCNT_LPG1').value !== "" && isFinite(document.getElementById('WF_KAITENCNT_LPG1').value)) {
        kaitenSum = kaitenSum + eval(document.getElementById('WF_KAITENCNT_LPG1').value);
    }
    if (document.getElementById('WF_KAITENCNT_LNG1').value !== "" && isFinite(document.getElementById('WF_KAITENCNT_LNG1').value)) {
        kaitenSum = kaitenSum + eval(document.getElementById('WF_KAITENCNT_LNG1').value);
    }
    if (document.getElementById('WF_KAITENCNT_WHITE2').value !== "" && isFinite(document.getElementById('WF_KAITENCNT_WHITE2').value)) {
        kaitenSum = kaitenSum + eval(document.getElementById('WF_KAITENCNT_WHITE2').value);
    }
    if (document.getElementById('WF_KAITENCNT_BLACK2').value !== "" && isFinite(document.getElementById('WF_KAITENCNT_BLACK2').value)) {
        kaitenSum = kaitenSum + eval(document.getElementById('WF_KAITENCNT_BLACK2').value);
    }
    if (document.getElementById('WF_KAITENCNT_LPG2').value !== "" && isFinite(document.getElementById('WF_KAITENCNT_LPG2').value)) {
        kaitenSum = kaitenSum + eval(document.getElementById('WF_KAITENCNT_LPG2').value);
    }
    if (document.getElementById('WF_KAITENCNT_LNG2').value !== "" && isFinite(document.getElementById('WF_KAITENCNT_LNG2').value)) {
        kaitenSum = kaitenSum + eval(document.getElementById('WF_KAITENCNT_LNG2').value);
    }

    kaitenSum = kaitenSum + eval(document.getElementById('WF_KAITEN').value);

    document.getElementById('WF_KAITENCNTTTL').value = kaitenSum;

    var haidisSum = 0;
    if (document.getElementById('WF_HAIDISTANCE_WHITE1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_WHITE1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_WHITE1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_BLACK1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_BLACK1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_BLACK1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_LPG1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_LPG1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_LPG1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_LNG1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_LNG1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_LNG1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_WHITE2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_WHITE2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_WHITE2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_BLACK2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_BLACK2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_BLACK2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_LPG2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_LPG2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_LPG2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_LNG2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_LNG2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_LNG2').value);
    }

    haidisSum = haidisSum + eval(document.getElementById('WF_HAIDIS').value);

    document.getElementById('WF_HAIDISTANCETTL').value = haidisSum;
};

// ○項目変更
function ItemChange(fieldNM) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_ButtonClick').value = "WF_LeftBoxSelectClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    }
}