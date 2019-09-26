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

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);

    // 画面表示切替
    if (document.getElementById("WF_DISP").value == "detailbox") {
        document.getElementById("headerbox").style.visibility = "hidden";
        document.getElementById("detailbox").style.visibility = "visible";

        // キーダウンイベント追加
        AddKeydownEvent();

        // 項目選択リスト色変更
        ListBoxChangeColor()

        //スクロール位置再現
        document.getElementById("WF_RepExcelList").scrollTop = Number(document.getElementById("WF_List_Top").value);
        document.getElementById("WF_RepDetail").scrollLeft = Number(document.getElementById("WF_Scroll_Left").value);
        document.getElementById("WF_RepDetail").scrollTop = Number(document.getElementById("WF_Scroll_Top").value);
    } else {
        document.getElementById("detailbox").style.visibility = "hidden";
        document.getElementById("headerbox").style.visibility = "visible";

        // リストの共通イベント(ホイール、横スクロール)をバインド
        bindListCommonEvents(pnlListAreaId, IsPostBack);
    }
}

// キーダウンイベント追加
function AddKeydownEvent() {

    document.addEventListener("keydown", (function () {
        return function () {
            if (window.event.keyCode == 107) {
                if (document.getElementById("MF_SUBMIT").value == "FALSE") {
                    document.getElementById("MF_SUBMIT").value = "TRUE";
                    document.getElementById("WF_ButtonClick").value = "WF_INSERT";
                    document.body.style.cursor = "wait";
                    document.forms[0].submit();
                    return false;
                }
            }

            if (window.event.keyCode == 109) {
                if (document.getElementById("MF_SUBMIT").value == "FALSE") {
                    document.getElementById("MF_SUBMIT").value = "TRUE";
                    document.getElementById("WF_ButtonClick").value = "WF_DELETE";
                    document.body.style.cursor = "wait";
                    document.forms[0].submit();
                    return false;
                }
            }
        }
    })(), false);
}


// エクセル切替(タイトル⇔行明細)
function ExcelSelectChange() {

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_ExcelChange";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}


// エクセル項目ヘッダーダブルクリック
function ExcelHeadDBClick(col) {

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_DELCOL").value = col;
        document.getElementById("WF_ButtonClick").value = "WF_ExcelHeadDBClick";
        document.getElementById("WF_List_Top").value = document.getElementById("WF_RepExcelList").scrollTop;
        document.getElementById("WF_Scroll_Left").value = document.getElementById("WF_RepDetail").scrollLeft;
        document.getElementById("WF_Scroll_Top").value = document.getElementById("WF_RepDetail").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// エクセル項目ダブルクリック
function ExcelDBClick(row, col) {

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_EXCEL_ROW").value = row;
        document.getElementById("WF_EXCEL_COL").value = col;
        document.getElementById("WF_ButtonClick").value = "WF_ExcelDBClick";
        document.getElementById("WF_List_Top").value = document.getElementById("WF_RepExcelList").scrollTop;
        document.getElementById("WF_Scroll_Left").value = document.getElementById("WF_RepDetail").scrollLeft;
        document.getElementById("WF_Scroll_Top").value = document.getElementById("WF_RepDetail").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}


// マウスダウン
function ExcelMouseDown(row, col, select) {

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("WF_DRAG_START").value = "FALSE";
        document.getElementById("WF_EXCEL_START_ROW").value = row;
        document.getElementById("WF_EXCEL_START_COL").value = col;
        if (select != "") {
            document.getElementById("WF_EXCEL_SELECT").value = select;
        }
        ListBoxChangeColor();
    }
}

// マウスドラッグ開始
function ExcelDragStart(select) {

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("WF_DRAG_START").value = "TRUE";
        document.getElementById("MF_SUBMIT").value = "TRUE";
        if (select != "") {
            document.getElementById("WF_EXCEL_SELECT").value = select;
        }
        document.getElementById("WF_ButtonClick").value = "WF_ExcelDragStart";
        document.getElementById("WF_List_Top").value = document.getElementById("WF_RepExcelList").scrollTop;
        document.getElementById("WF_Scroll_Left").value = document.getElementById("WF_RepDetail").scrollLeft;
        document.getElementById("WF_Scroll_Top").value = document.getElementById("WF_RepDetail").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// マウスアップ
function ExcelMouseUp(row, col) {

    if (document.getElementById("MF_SUBMIT").value == "FALSE" &&
        document.getElementById("WF_DRAG_START").value == "TRUE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_DRAG_START").value = "FALSE";
        document.getElementById("WF_EXCEL_END_ROW").value = row;
        document.getElementById("WF_EXCEL_END_COL").value = col;
        document.getElementById("WF_ButtonClick").value = "WF_ExcelMouseUp";
        document.getElementById("WF_List_Top").value = 0;
        document.getElementById("WF_Scroll_Left").value = document.getElementById("WF_RepDetail").scrollLeft;
        document.getElementById("WF_Scroll_Top").value = document.getElementById("WF_RepDetail").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// 項目選択リストのマウスカーソルによる色変更
function ListBoxChangeColor() {

    var objList = document.getElementById("WF_EXCEL_LIST");

    for (var i = 0; i < objList.rows.length; i++) {
        var objRow = objList.rows[i];
        var objCell = objList.rows[i].cells[0];
        var select = "WF_Rep_" + document.getElementById("WF_EXCEL_SELECT").value;

        if (objCell.id == select) {
            objCell.style.backgroundColor = "#1E90FF";
            objCell.style.color = "#FFFFFF";

            objRow.addEventListener("mousedown", (function (objRow) {
                return function () {
                    objRow.classList.remove("hover");
                    objRow.classList.remove("selecthover");
                    objRow.classList.add("select");
                };
            })(objRow), false);
            objRow.addEventListener("mouseup", (function (objRow) {
                return function () {
                    objRow.classList.remove("hover");
                    objRow.classList.add("selecthover");
                    objRow.classList.remove("select");
                };
            })(objRow), false);
            objRow.addEventListener("mouseover", (function (objRow) {
                return function () {
                    objRow.classList.remove("hover");
                    objRow.classList.add("selecthover");
                    objRow.classList.remove("select");
                };
            })(objRow), false);
            objRow.addEventListener("mouseout", (function (objRow) {
                return function () {
                    objRow.classList.remove("hover");
                    objRow.classList.remove("selecthover");
                    objRow.classList.remove("select");
                };
            })(objRow), false);
        } else {
            objCell.style.backgroundColor = "";
            objCell.style.color = "#000000";

            objRow.addEventListener("mouseover", (function (objRow) {
                return function () {
                    objRow.classList.add("hover");
                    objRow.classList.remove("selecthover");
                    objRow.classList.remove("select");
                };
            })(objRow), false);
            objRow.addEventListener("mouseout", (function (objRow) {
                return function () {
                    objRow.classList.remove("hover");
                    objRow.classList.remove("selecthover");
                    objRow.classList.remove("select");
                };
            })(objRow), false);
        }
    }
}
