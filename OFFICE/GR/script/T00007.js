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

    var unloadSum = 0;

    if (document.getElementById('WF_UNLOADCNT_IPPAN1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_IPPAN1').value)){
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_IPPAN1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_JUN1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_JUN1').value)){
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_JUN1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_LPG1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_LPG1').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_LPG1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_LNG1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_LNG1').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_LNG1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_CONT1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_CONT1').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_CONT1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_SANS1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_SANS1').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_SANS1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_CHIS1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_CHIS1').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_CHIS1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_SUIS1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_SUIS1').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_SUIS1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_META1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_META1').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_META1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_RATE1').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_RATE1').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_RATE1').value);
    }
    if (document.getElementById('WF_UNLOADCNT_IPPAN2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_IPPAN2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_IPPAN2').value);
    }
    if (document.getElementById('WF_UNLOADCNT_JUN2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_JUN2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_JUN2').value);
    }
    if (document.getElementById('WF_UNLOADCNT_LPG2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_LPG2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_LPG2').value);
    }
    if (document.getElementById('WF_UNLOADCNT_LNG2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_LNG2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_LNG2').value);
    }
    if (document.getElementById('WF_UNLOADCNT_CONT2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_CONT2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_CONT2').value);
    }
    if (document.getElementById('WF_UNLOADCNT_SANS2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_SANS2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_SANS2').value);
    }
    if (document.getElementById('WF_UNLOADCNT_CHIS2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_CHIS2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_CHIS2').value);
    }
    if (document.getElementById('WF_UNLOADCNT_SUIS2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_SUIS2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_SUIS2').value);
    }
    if (document.getElementById('WF_UNLOADCNT_META2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_META2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_META2').value);
    }
    if (document.getElementById('WF_UNLOADCNT_RATE2').value !== "" && isFinite(document.getElementById('WF_UNLOADCNT_RATE2').value)) {
        unloadSum = unloadSum + eval(document.getElementById('WF_UNLOADCNT_RATE2').value);
    }

    unloadSum = unloadSum + eval(document.getElementById('WF_UNLOAD').value);

    document.getElementById('WF_UNLOADCNTTTL').value = unloadSum;

    var haidisSum = 0;
    if (document.getElementById('WF_HAIDISTANCE_IPPAN1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_IPPAN1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_IPPAN1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_JUN1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_JUN1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_JUN1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_LPG1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_LPG1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_LPG1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_LNG1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_LNG1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_LNG1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_CONT1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_CONT1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_CONT1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_SANS1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_SANS1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_SANS1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_CHIS1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_CHIS1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_CHIS1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_SUIS1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_SUIS1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_SUIS1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_META1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_META1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_META1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_RATE1').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_RATE1').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_RATE1').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_IPPAN2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_IPPAN2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_IPPAN2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_JUN2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_JUN2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_JUN2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_LPG2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_LPG2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_LPG2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_LNG2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_LNG2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_LNG2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_CONT2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_CONT2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_CONT2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_SANS2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_SANS2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_SANS2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_CHIS2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_CHIS2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_CHIS2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_SUIS2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_SUIS2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_SUIS2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_META2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_META2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_META2').value);
    }
    if (document.getElementById('WF_HAIDISTANCE_RATE2').value !== "" && isFinite(document.getElementById('WF_HAIDISTANCE_RATE2').value)) {
        haidisSum = haidisSum + eval(document.getElementById('WF_HAIDISTANCE_RATE2').value);
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