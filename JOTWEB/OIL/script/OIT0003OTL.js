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
    let buttonObjList = document.querySelectorAll('#WF_ButtonOtSend:not(:disabled),#WF_ButtonReserved:not(:disabled),#WF_ButtonTakusou:not(:disabled)');

    if (document.getElementById('WF_MAPpermitcode').value === "TRUE") {
        //活性
        document.getElementById("WF_ButtonALLSELECT").disabled = "";
        document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "";
        for (let i = 0; i < buttonObjList.length; i++) {
            buttonObjList[i].disabled = "";
        }

    } else {
        //非活性 
        document.getElementById("WF_ButtonALLSELECT").disabled = "disabled";
        document.getElementById("WF_ButtonSELECT_LIFTED").disabled = "disabled";
        for (let i = 0; i < buttonObjList.length; i++) {
            buttonObjList[i].disabled = "disabled";
        }
    }
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

    // チェックボックス
    ChangeCheckBox();

    //// 使用有無初期設定
    //ChangeOrgUse();

    //// (帳票)ラジオボタン
    //reportRadioButton();
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
            || chkStatus === "輸送完了"
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

// ◯帳票(ラジオボタンクリック)
function reportRadioButton() {
    let chkObj = document.getElementById('rbLineBtn');
    let txtObj = document.getElementById('divRTrainNo'); //←表示非表示切替用

    if (chkObj === null) {
        txtObj.style.display = 'none';
        return;
    }

    if (chkObj.checked) {
        txtObj.style.display = 'block';
    } else {
        txtObj.style.display = 'none';
    }
}
// ○ダウンロード処理
function f_ExcelPrint() {
    // リンク参照
    window.open(document.getElementById("WF_PrintURL").value, "view", "_blank");
    let url2 = document.getElementById("WF_PrintURL2");
    if (url2 !== null) {
        window.open(url2.value, "view2", "_blank");
    }
}
/**
 * 更新確認ポップアップを閉じる
 * @return {undefined} なし
 * @description
 */
function closeModDownLoadConfirm() {
    let updateConfirmObj = document.getElementById('divModFileDlList');
    if (updateConfirmObj !== null) {
        updateConfirmObj.classList.remove('showModFileDlConfirm');
        let hdnUpdConfirmObj = document.getElementById('hdnModFileDlChkConfirmIsActive');
        hdnUpdConfirmObj.value = '';
    }
}
function closeOTLinkageSendConfirm() {
    let updateConfirmObj = document.getElementById('divOTLinkageSendList');
    if (updateConfirmObj !== null) {
        updateConfirmObj.classList.remove('showOTLinkageSendConfirm');
        let hdnUpdConfirmObj = document.getElementById('hdnOTLinkageSendChkConfirmIsActive');
        hdnUpdConfirmObj.value = '';
    }
}