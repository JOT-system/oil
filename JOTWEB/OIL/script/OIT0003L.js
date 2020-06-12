﻿// ○OnLoad用処理（左右Box非表示）
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

            //再表示(仙台)
            document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "inlineblock";
            document.getElementById("WF_ButtonSendaiLOADCSV").value = "積込予定(仙)";

            //再表示(根岸)
            document.getElementById("WF_ButtonNegishiSHIPCSV").style.display = "inlineblock";
            document.getElementById("WF_ButtonNegishiSHIPCSV").value = "出荷予定(根)";
            document.getElementById("WF_ButtonNegishiLOADCSV").style.display = "inlineblock";
            document.getElementById("WF_ButtonNegishiLOADCSV").value = "積込予定(根)";

        //★東北支店/仙台
        } else if (document.getElementById('WF_BUTTONpermitcode').value === "1") {
            
            //再表示(仙台)
            //document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "inlineblock";
            document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "none";

            //非表示(根岸)
            document.getElementById("WF_ButtonNegishiSHIPCSV").style.display = "none";
            document.getElementById("WF_ButtonNegishiLOADCSV").style.display = "none";
        
        //★関東支店/五井/甲子/袖ヶ浦/根岸
        } else if (document.getElementById('WF_BUTTONpermitcode').value === "2") {

            //非表示(仙台)
            document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "none";

            //再表示(根岸)
            document.getElementById("WF_ButtonNegishiSHIPCSV").style.display = "inlineblock";
            document.getElementById("WF_ButtonNegishiLOADCSV").style.display = "inlineblock";
        
        //★中部支店/四日市/三重塩浜
        } else if (document.getElementById('WF_BUTTONpermitcode').value === "3") {

            //非表示(仙台)
            document.getElementById("WF_ButtonSendaiLOADCSV").style.display = "none";

            //非表示(根岸)
            document.getElementById("WF_ButtonNegishiSHIPCSV").style.display = "none";
            document.getElementById("WF_ButtonNegishiLOADCSV").style.display = "none";
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
        var chkStatus = trlst[i].getElementsByTagName("td")[2].innerHTML;
        let leftTableObj = document.getElementById("pnlListArea_DL").getElementsByTagName("table")[0];
        let leftRowObj = leftTableObj.rows[i];
        var chkObj = leftRowObj.querySelector("input[type=checkbox]"); //document.getElementById("chkpnlListAreaOPERATION" + (i + 1));
        if (chkObj === null) {
            continue;
        }

        if (chkStatus === "受注キャンセル"
            || chkStatus === "検収中"
            || chkStatus === "検収済"
            || chkStatus === "費用確定"
            || chkStatus === "経理未計上"
            || chkStatus === "経理計上") {
            chkObj.disabled = true;
            trlst[i].getElementsByTagName("td")[2].disabled = true;
        } else {
            chkObj.disabled = false;
            trlst[i].getElementsByTagName("td")[2].disabled = false;
        }
    }
}
