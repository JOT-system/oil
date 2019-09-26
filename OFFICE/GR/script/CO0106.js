// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    // リストの共通イベント(ホイール、横スクロール)をバインド
//    bindListCommonEvents(pnlListAreaId, IsPostBack);
    // 初期表示実行
    if (document.getElementById('WF_ButtonClick').value == "WF_INITIALIZE") {
        document.getElementById("WF_MESSAGE").textContent = "状態確認中...　しばらくお待ちください。";
        document.getElementById("WF_MESSAGE").style.color = "blue";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    };
};

// ○Repeater行情報取得処理（ラジオボタン切り替え時のON、OFF設定）
function ButtonCntl(BtnValue) {
    if (document.getElementById("WF_SUBMIT").value == "FALSE") {
        document.getElementById("WF_SUBMIT").value = "TRUE"
        document.getElementById("WF_ButtonALLSELECT").disabled = true;
        document.getElementById("WF_ButtonALLCANCEL").disabled = true;
        document.getElementById("WF_ButtonPSTAT").disabled = true;
        document.getElementById("WF_ButtonSSTAT").disabled = true;
        document.getElementById("WF_ButtonOSTAT").disabled = true;
        document.getElementById("WF_ButtonOSTOP").disabled = true;
        document.getElementById("WF_ButtonOSTART").disabled = true;
        document.getElementById("WF_ButtonSSTOP").disabled = true;
        document.getElementById("WF_ButtonSSTART").disabled = true;
        document.getElementById("WF_ButtonSEND").disabled = true;
        document.getElementById("WF_ButtonEND").disabled = true;
        if (BtnValue == "SEND") {
            document.getElementById("WF_SEND_BTN").value = BtnValue;
        }
        if (BtnValue == "OSTOP") {
            document.getElementById("WF_OSTOP_BTN").value = BtnValue;
        }
        if (BtnValue == "OSTART") {
            document.getElementById("WF_OSTART_BTN").value = BtnValue;
        }
        if (BtnValue == "SSTOP") {
            document.getElementById("WF_SSTOP_BTN").value = BtnValue;
        }
        if (BtnValue == "SSTART") {
            document.getElementById("WF_SSTART_BTN").value = BtnValue;
        }
        if (BtnValue == "STAT") {
            document.getElementById("WF_STAT_BTN").value = BtnValue;
        }
        if (BtnValue == "PSTAT") {
            document.getElementById("WF_PSTAT_BTN").value = BtnValue;
        }
        if (BtnValue == "SSTAT") {
            document.getElementById("WF_SSTAT_BTN").value = BtnValue;
        }
        if (BtnValue == "OSTAT") {
            document.getElementById("WF_OSTAT_BTN").value = BtnValue;
        }
        document.body.style.cursor = "wait";
        CO0106.submit();                            //aspx起動
    }
}
