// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    document.getElementById("pnlListArea_DR").scrollLeft = Number(document.getElementById("WF_DISP_SaveX").value);
    document.getElementById("pnlListArea_DR").scrollTop = Number(document.getElementById("WF_DISP_SaveY").value);

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen").value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    } else {
        if (document.getElementById("WF_SelectFIELD").value != "") {
            document.getElementById(document.getElementById("WF_SelectFIELD").value).focus();
            document.getElementById("WF_SelectFIELD").value = "";
        }
    }

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    // 更新ボタン活性／非活性
    if (document.getElementById("WF_MAPpermitcode").value == "TRUE") {
        // 活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        // 非活性
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    }

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);

    // リストの共通イベント(ホイール、横スクロール)をバインド
    bindListCommonEvents(pnlListAreaId, IsPostBack, false, true, true, false);
    document.getElementById("MF_SUBMIT").value = "TRUE";
    // 使用有無初期設定
    ChangeOrgUse();
    document.getElementById("MF_SUBMIT").value = "FALSE";
};

// ○使用有無初期設定
function ChangeOrgUse() {

    var objTable = document.getElementById("pnlListArea_DR").children[0];

    for (var i = 0; i < objTable.rows.length; i++) {

        document.getElementById("rblORGUSEORGUSE" + (i + 1) + "_0").checked = false
        document.getElementById("rblORGUSEORGUSE" + (i + 1) + "_1").checked = false

        if (document.getElementById("lrblORGUSEORGUSE" + (i + 1)).innerText == "01") {
            document.getElementById("rblORGUSEORGUSE" + (i + 1) + "_0").checked = true
            ChangeDisabled(document.getElementById("txtpnlListAreaARRIVTIME" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaDISTANCE" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaYTODOKECODE" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaSEQ" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaJSRTODOKECODE" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaSHUKABASHO" + (i + 1)), false);
        } else {
            document.getElementById("rblORGUSEORGUSE" + (i + 1) + "_1").checked = true
            ChangeDisabled(document.getElementById("txtpnlListAreaARRIVTIME" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaDISTANCE" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaYTODOKECODE" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaSEQ" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaJSRTODOKECODE" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaSHUKABASHO" + (i + 1)), true);
        }
    }
}

// ○リスト内容変更処理
function ListChange(pnlList, Line) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        let trlst = document.getElementById("pnlListArea_DL").getElementsByTagName("tr");
        trlst[Line - 1].getElementsByTagName("th")[1].innerHTML = "更新";
    }
}

// ○左BOX用処理（DBクリック選択+値反映）
function List_Field_DBclick(obj, Line, fieldNM) {

    //非活性の場合、左選択肢を表示しない
    if (fieldNM == "SHUKABASHO") {
        if (document.getElementById("txtpnlListAreaSHUKABASHO" + (Line)) !== null) {
            if (document.getElementById("txtpnlListAreaSHUKABASHO" + (Line)).disabled == true) {
                return;
            }
        }
    }

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_FIELD").value = fieldNM;
        document.getElementById("WF_SelectLine").value = Line;
        document.getElementById("WF_SelectFIELD").value = obj.firstChild.id;
        document.getElementById("WF_LeftMViewChange").value = EXTRALIST;
        document.getElementById("WF_LeftboxOpen").value = "Open";
        document.getElementById("WF_ButtonClick").value = "WF_Field_DBClick";
        document.getElementById("WF_DISP_SaveX").value = document.getElementById("pnlListArea_DR").scrollLeft;
        document.getElementById("WF_DISP_SaveY").value = document.getElementById("pnlListArea_DR").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

//〇使用有無変更時処理
function selectUse(obj, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        if (document.getElementById("rblORGUSEORGUSE" + Line + "_0").checked) {
            document.getElementById("lrblORGUSEORGUSE" + Line).innerText = "01";
            ChangeDisabled(document.getElementById("txtpnlListAreaARRIVTIME" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaDISTANCE" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaYTODOKECODE" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaSEQ" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaJSRTODOKECODE" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaSHUKABASHO" + Line), false);
        } else if (document.getElementById("rblORGUSEORGUSE" + Line + "_1").checked) {
            document.getElementById("lrblORGUSEORGUSE" + Line).innerText = "02";
            ChangeDisabled(document.getElementById("txtpnlListAreaARRIVTIME" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaDISTANCE" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaYTODOKECODE" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaSEQ" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaJSRTODOKECODE" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaSHUKABASHO" + Line), true);
        }
    }
}
// ○活性非活性変更
function ChangeDisabled(obj, flag) {
    // オブジェクトが存在しない場合抜ける
    if (obj == undefined || obj == null) {
        return;
    }
    obj.disabled = flag;
}