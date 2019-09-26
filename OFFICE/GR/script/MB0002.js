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
};

// ○リスト内容変更処理
function ListChange(pnlList, Line) {

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_SelectedIndex").value = Line
        document.getElementById("WF_ButtonClick").value = "WF_ListChange";
        document.getElementById("WF_DISP_SaveX").value = document.getElementById("pnlListArea_DR").scrollLeft;
        document.getElementById("WF_DISP_SaveY").value = document.getElementById("pnlListArea_DR").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ○左BOX用処理（DBクリック選択+値反映）
function List_Field_DBclick(obj, Line, fieldNM) {

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_SelectFIELD").value = obj.firstChild.id;
        document.getElementById("WF_FIELD").value = fieldNM;
        document.getElementById("WF_SelectLine").value = Line;
        document.getElementById("WF_LeftMViewChange").value = EXTRALIST;
        document.getElementById("WF_LeftboxOpen").value = "Open";
        document.getElementById("WF_ButtonClick").value = "WF_Field_DBClick";
        document.getElementById("WF_DISP_SaveX").value = document.getElementById("pnlListArea_DR").scrollLeft;
        document.getElementById("WF_DISP_SaveY").value = document.getElementById("pnlListArea_DR").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}
