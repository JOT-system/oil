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

    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value === "TRUE") {
        //活性
        document.getElementById("WF_ButtonALLSELECT").disabled = "";
        document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "";
        document.getElementById("WF_ButtonORDER_CANCEL").disabled = "";
        document.getElementById("WF_ButtonINSERT").disabled = "";

        //★石油部/情報システム部
        if (document.getElementById('WF_BUTTONpermitcode').value === "0") {

            //OT連携選択ボタン(表示)
            document.getElementById("WF_ButtonOTLinkageINSERT").style.display = "inlineblock";

            ////再表示(仙台)
            //document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "inlineblock";
            //document.getElementById("WF_ButtonSendaiLOADCSV").value = "積込予定(仙)";

            ////再表示(根岸)
            //document.getElementById("WF_ButtonNegishiSHIPCSV").style.display = "inlineblock";
            //document.getElementById("WF_ButtonNegishiSHIPCSV").value = "出荷予定(根)";
            //document.getElementById("WF_ButtonNegishiLOADCSV").style.display = "inlineblock";
            //document.getElementById("WF_ButtonNegishiLOADCSV").value = "積込予定(根)";

        //★東北支店/仙台
        } else if (document.getElementById('WF_BUTTONpermitcode').value === "1") {

            //OT連携選択ボタン(表示)
            document.getElementById("WF_ButtonOTLinkageINSERT").style.display = "inlineblock";

            ////再表示(仙台)
            ////document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "inlineblock";
            //document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "none";

            ////非表示(根岸)
            //document.getElementById("WF_ButtonNegishiSHIPCSV").style.display = "none";
            //document.getElementById("WF_ButtonNegishiLOADCSV").style.display = "none";
        
        //★関東支店/五井/甲子/袖ヶ浦/根岸
        } else if (document.getElementById('WF_BUTTONpermitcode').value === "2") {

            // ●袖ヶ浦は非表示
            if (document.getElementById('WF_BUTTONofficecode').value === "011203") {
                //OT連携選択ボタン(非表示)
                document.getElementById("WF_ButtonOTLinkageINSERT").style.display = "none";
            } else {
                //OT連携選択ボタン(表示)
                document.getElementById("WF_ButtonOTLinkageINSERT").style.display = "inlineblock";
            }

            ////非表示(仙台)
            //document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "none";

            ////再表示(根岸)
            //document.getElementById("WF_ButtonNegishiSHIPCSV").style.display = "inlineblock";
            //document.getElementById("WF_ButtonNegishiLOADCSV").style.display = "inlineblock";
        
        //★中部支店/四日市/三重塩浜
        } else if (document.getElementById('WF_BUTTONpermitcode').value === "3") {

            // ●三重塩浜は非表示
            if (document.getElementById('WF_BUTTONofficecode').value === "012402") {
                //OT連携選択ボタン(非表示)
                document.getElementById("WF_ButtonOTLinkageINSERT").style.display = "none";
            } else {
                //OT連携選択ボタン(表示)
                document.getElementById("WF_ButtonOTLinkageINSERT").style.display = "inlineblock";
            }

            ////非表示(仙台)
            //document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "none";

            ////非表示(根岸)
            //document.getElementById("WF_ButtonNegishiSHIPCSV").style.display = "none";
            //document.getElementById("WF_ButtonNegishiLOADCSV").style.display = "none";
        }
    } else {
        //非活性 
        document.getElementById("WF_ButtonALLSELECT").disabled = "disabled";
        document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "disabled";
        document.getElementById("WF_ButtonORDER_CANCEL").disabled = "disabled";
        document.getElementById("WF_ButtonINSERT").disabled = "disabled";
        //非表示
        document.getElementById("WF_ButtonNegishiSHIPCSV").style.display = "none";
        document.getElementById("WF_ButtonNegishiLOADCSV").style.display = "none";

    }
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

    // チェックボックス
    ChangeCheckBox();

    // 使用有無初期設定
    ChangeOrgUse();

    // (帳票)ラジオボタン
    reportRadioButton();
}

// ○チェックボックス変更
function ChangeCheckBox() {

    var objTable = document.getElementById("pnlListArea_DL").children[0];

    var chkObjs = objTable.querySelectorAll("input[id^='chkpnlListAreaOPERATION']");
    var spnObjs = objTable.querySelectorAll("span[id^='hchkpnlListAreaOPERATION']");

    for (let i = 0; i < chkObjs.length; i++) {

        if (chkObjs[i] !== null) {
            if (spnObjs[i].innerText === "on") {
                chkObjs[i].checked = true;
            } else {
                chkObjs[i].checked = false;
            }
        }
    }
}


// ○チェックボックス選択
function SelectCheckBox(obj, lineCnt) {

    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        let chkObj = obj.querySelector("input");
        if (chkObj === null) {
            return;
        }
        if (chkObj.disabled === true) {
            return;
        }

        document.getElementById("WF_SelectedIndex").value = lineCnt;
        document.getElementById("WF_ButtonClick").value = "WF_CheckBoxSELECT";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }

}


// ○使用有無変更
function ChangeOrgUse(obj, lineCnt) {

    // 一覧の内容を取得(右側のリスト)
    let trlst = document.getElementById("pnlListArea_DR").getElementsByTagName("tr");

    for (let i = 0; i < trlst.length; i++) {
        // 一覧の項目(ステータス)の値を取得
        var chkStatus = trlst[i].getElementsByTagName("td")[8].innerHTML;
        let leftTableObj = document.getElementById("pnlListArea_DL").getElementsByTagName("table")[0];
        let leftRowObj = leftTableObj.rows[i];
        var chkObj = leftRowObj.querySelector("input[type=checkbox]"); //document.getElementById("chkpnlListAreaOPERATION" + (i + 1));
        if (chkObj === null) {
            continue;
        }

        if (chkStatus === "受注キャンセル"
            || chkStatus === "輸送完了"
            || chkStatus === "検収済"
            || chkStatus === "費用確定"
            || chkStatus === "経理未計上"
            || chkStatus === "経理計上") {
            chkObj.disabled = true;
            trlst[i].getElementsByTagName("td")[8].disabled = true;
        } else {
            chkObj.disabled = false;
            trlst[i].getElementsByTagName("td")[8].disabled = false;
        }
    }
}

// ◯帳票(ラジオボタンクリック)
function reportRadioButton() {
    let chkObj = document.getElementById('rbLineBtn');
    let txtObj = document.getElementById('divRTrainNo'); //←表示非表示切替用

    if (chkObj === null) {
        txtObj.style.display = 'none'
        //return;
    } else {
        if (chkObj.checked) {
            txtObj.style.display = 'block'
        } else {
            txtObj.style.display = 'none'
        }
    }

    // ### 20201106 START OT積込指示(月末)対応 ##################################
    let chkObj2 = document.getElementById('rbOTLoadBtn');
    let txtObj2 = document.getElementById('divEndMonthChk'); //←表示非表示切替用

    if (chkObj2 === null) {
        txtObj2.style.display = 'none'
        //return;
    } else {
        if (chkObj2.checked) {
            txtObj2.style.display = 'block'
        } else {
            txtObj2.style.display = 'none'
        }
    }
    // ### 20201106 END   OT積込指示(月末)対応 ##################################

    let chkObj3 = document.getElementById('rbTankDispatchBtn');
    let chkObj3_2 = document.getElementById('rbActualShipBtn');
    let chkObj3_3 = document.getElementById('rbConcatOederBtn');
    let txtObj3 = document.getElementById('divTrainNo'); //←表示非表示切替用

    let isVisitTxtObj3 = false
    if (chkObj3 !== null) {
        isVisitTxtObj3 = chkObj3.checked | isVisitTxtObj3
    }
    if (chkObj3_2 !== null) {
        isVisitTxtObj3 = chkObj3_2.checked | isVisitTxtObj3
    }
    if (chkObj3_3 !== null) {
        isVisitTxtObj3 = chkObj3_3.checked | isVisitTxtObj3
    }

    if (isVisitTxtObj3) {
        txtObj3.style.display = 'block'
    } else {
        txtObj3.style.display = 'none'
    }
}

// ○ダウンロード処理
function f_ExcelPrint() {
    // リンク参照
    let urlObj = document.getElementById("WF_PrintURL");
    if (urlObj !== null) {
        if (isJSON(urlObj.value)) {
            let urlList = JSON.parse(urlObj.value);
            for (i = 0; i < urlList.length; i++) {
                if (urlList[i].url !== null) {
                    win = window.open(urlList[i].url, "view" + i, "_blank");
                    win.unload = function () {

                    }
                }
            }
        } else {
            if (urlObj.value !== null) {
                window.open(urlObj.value, "view", "_blank");
            }
        }
    }
}

// JSON判定
function isJSON(arg) {
    arg = (typeof arg === "function") ? arg() : arg;
    if (typeof arg !== "string") {
        return false;
    }
    try {
        arg = (!JSON) ? eval("(" + arg + ")") : JSON.parse(arg);
        return true;
    } catch (e) {
        return false;
    }
};