// ○OnLoad用処理(左右Box非表示)
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    document.getElementById("pnlListArea_DR").scrollLeft = Number(document.getElementById("WF_DISP_SaveX").value);
    document.getElementById("pnlListArea_DR").scrollTop = Number(document.getElementById("WF_DISP_SaveY").value);
    document.getElementById("pnlListTotalArea_FR").scrollLeft = Number(document.getElementById("WF_DISP_SaveX").value);

    // 左ボックス
    if (document.getElementById("WF_LeftboxOpen").value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    }

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }

    // 更新ボタン活性／非活性
    if (document.getElementById("WF_MAPpermitcode").value == "TRUE") {
        // 活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
        document.getElementById("WF_ButtonSAVE").disabled = "";
    } else {
        // 非活性
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
        document.getElementById("WF_ButtonSAVE").disabled = "disabled";
    }

    // 個別の場合、前頁・次頁・絞込を非表示
    if (document.getElementById("WF_ONLY").value == "TRUE") {
        document.getElementById("WF_SELSTAFFCODE_L").style.display = "none";
        document.getElementById("WF_SELSTAFFCODE").hidden = "hidden";
        document.getElementById("WF_SELSTAFFCODE_TEXT").style.display = "none";
        document.getElementById("WF_ButtonDOWN").hidden = "hidden";
        document.getElementById("WF_ButtonUP").hidden = "hidden";
        document.getElementById("WF_ButtonExtract").hidden = "hidden";
    } else {
        document.getElementById("WF_SELSTAFFCODE_L").style.display = "";
        document.getElementById("WF_SELSTAFFCODE").hidden = "";
        document.getElementById("WF_SELSTAFFCODE_TEXT").style.display = "";
        document.getElementById("WF_ButtonDOWN").hidden = "";
        document.getElementById("WF_ButtonUP").hidden = "";
        document.getElementById("WF_ButtonExtract").hidden = "";
    }

    // 画面表示切替
    if (document.getElementById("WF_DISP").value == "Adjust") {
        if (document.getElementById("WF_SELSTAFFCODE").hidden == false) {
            document.getElementById("WF_SELSTAFFCODE_L").style.display = "none";
            document.getElementById("WF_SELSTAFFCODE").hidden = "hidden";
            document.getElementById("WF_SELSTAFFCODE_TEXT").style.display = "none";
            document.getElementById("WF_ButtonExtract").hidden = "hidden";
        }
        document.getElementById("WF_ButtonUPDATE").hidden = "hidden";
        document.getElementById("WF_ButtonCSV").hidden = "hidden";
        document.getElementById("WF_ButtonPrint").hidden = "hidden";
        document.getElementById("WF_ButtonUPDATE2").hidden = "";
        document.getElementById("divListArea").style.display = "none";
        document.getElementById("divAdjustArea").style.display = "block";
    } else {
        if (document.getElementById("WF_SELSTAFFCODE").hidden == false) {
            document.getElementById("WF_SELSTAFFCODE_L").style.display = "";
            document.getElementById("WF_SELSTAFFCODE").hidden = "";
            document.getElementById("WF_SELSTAFFCODE_TEXT").style.display = "";
            document.getElementById("WF_ButtonExtract").hidden = "";
        }
        document.getElementById("WF_ButtonUPDATE").hidden = "";
        document.getElementById("WF_ButtonCSV").hidden = "";
        document.getElementById("WF_ButtonPrint").hidden = "";
        document.getElementById("WF_ButtonUPDATE2").hidden = "hidden";
        document.getElementById("divListArea").style.display = "block";
        document.getElementById("divAdjustArea").style.display = "none";
    }

    // 左ボックス拡張機能追加
    addLeftBoxExtention(leftListExtentionTarget);

    // リストの共通イベント
    bindTotalListCommonEvents(pnlListAreaId, pnlListTotalAreaId, IsPostBack);

    // 使用有無変更
    DispFormat();

    // 月間調整画面変更
    AdjustFormat();
};

// リストの共通イベント
function bindTotalListCommonEvents(listObjId, listTotalObjId, isPostBack) {

    var listObj = document.getElementById(listObjId);
    var listTotalObj = document.getElementById(listTotalObjId);
    // そもそもリストがレンダリングされていなければ終了
    if (listObj == null || listTotalObj == null) {
        return;
    }

    // 横スクロールイベントのバインド
    // 可変列ヘッダーテーブル、可変列データテーブルのオブジェクトを取得
    var headerTableObj = document.getElementById(listObjId + '_HR');
    var dataTableObj = document.getElementById(listObjId + '_DR');
    var footerTableObj = document.getElementById(listTotalObjId + '_FR');
    // 可変列の描画がない場合はそのまま終了
    if (headerTableObj == null || dataTableObj == null || footerTableObj == null) {
        return;
    }

    // スクロールイベントのバインド
    dataTableObj.addEventListener('scroll', (function (listObj) {
        return function () {
            commonListScroll(listObj, listTotalObj);
        };
    })(listObj), false);

    // スクロールを保持する場合
    if (isPostBack === '0') {
        // 初回ロード時は左スクロール位置を0とる
        setCommonListScrollXpos(listObj.id, '0');
    }
    // ポストバック時は保持したスクロール位置に戻す
    if (isPostBack === '1') {
        var xpos = getCommonListScrollXpos(listObj.id);
        dataTableObj.scrollLeft = xpos;
        footerTableObj.scrollLeft = xpos;
        var e = document.createEvent("UIEvents");
        e.initUIEvent("scroll", true, true, window, 1);
        dataTableObj.dispatchEvent(e);
        footerTableObj.dispatchEvent(e);
    }

    bindCommonListHighlight(listObj.id);
}

// ○リストデータ部スクロール共通処理（ヘッダー部、フッター部のスクロールを連動させる)
function commonListScroll(listObj, listTotalObj) {
    var rightHeaderTableObj = document.getElementById(listObj.id + '_HR');
    var rightDataTableObj = document.getElementById(listObj.id + '_DR');
    var leftDataTableObj = document.getElementById(listObj.id + '_DL');
    var rightFooterTableObj = document.getElementById(listTotalObj.id + '_FR');

    setCommonListScrollXpos(listObj.id, rightDataTableObj.scrollLeft);
    rightHeaderTableObj.scrollLeft = rightDataTableObj.scrollLeft;          // 左右連動させる
    leftDataTableObj.scrollTop = rightDataTableObj.scrollTop;               // 上下連動させる
    rightFooterTableObj.scrollLeft = rightDataTableObj.scrollLeft;          // 左右連動させる
}

// ○画面表示変更
function DispFormat() {

    var objTable = document.getElementById("pnlListArea_DR").children[0];
    document.getElementById("WF_INFO").innerText = ""

    for (var i = 0; i < objTable.rows.length; i++) {

        // 未承認メッセージ表示
        if (document.getElementById("txtpnlListAreaSTATUS" + (i + 1)) != undefined) {
            if (document.getElementById("txtpnlListAreaSTATUS" + (i + 1)).value == "01" ||
                document.getElementById("txtpnlListAreaSTATUS" + (i + 1)).value == "03" ||
                document.getElementById("txtpnlListAreaSTATUS" + (i + 1)).value == "09") {
                document.getElementById("WF_INFO").innerText = "承認されていない（未申請、否認）データが存在します。"
            }
        }

        // 承認済、申請中の場合の活性非活性判定
        if (document.getElementById("txtpnlListAreaSTATUS" + (i + 1)).value == "02" ||
            document.getElementById("txtpnlListAreaSTATUS" + (i + 1)).value == "10") {
            ChangeDisabled(document.getElementById("txtpnlListAreaPAYKBN" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaSHUKCHOKKBN" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaSTTIME" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaENDDATE" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaENDTIME" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaBINDSTDATE" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaBINDTIME" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaBREAKTIME" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaYENDTIME" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaRIYU" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaRIYUETC" + (i + 1)), true);
            ChangeDisabled(document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)), true);
            ChangeDisabled(document.getElementById("chkpnlListAreaDRAWALFLG" + (i + 1)), false);
        } else {
            ChangeDisabled(document.getElementById("txtpnlListAreaPAYKBN" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaSHUKCHOKKBN" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaSTTIME" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaENDDATE" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaENDTIME" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaBINDSTDATE" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaBINDTIME" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaBREAKTIME" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaYENDTIME" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaRIYU" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaRIYUETC" + (i + 1)), false);

            // 残業時間の有無によって申請、取り下げの状態を変更する
            if (document.getElementsByName("R_ORVERTIME" + (i + 1)) != undefined) {
                if (document.getElementsByName("R_ORVERTIME" + (i + 1))[0].textContent == "") {
                    if (document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)) != undefined) {
                        document.getElementById("hchkpnlListAreaENTRYFLG" + (i + 1)).innerText = ""
                        document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)).checked = false;
                        document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)).disabled = true;
                    }
                    ChangeDisabled(document.getElementById("chkpnlListAreaDRAWALFLG" + (i + 1)), true);
                } else {
                    ChangeDisabled(document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)), false);
                    ChangeDisabled(document.getElementById("chkpnlListAreaDRAWALFLG" + (i + 1)), true);
                }
            } else {
                ChangeDisabled(document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)), true);
                ChangeDisabled(document.getElementById("chkpnlListAreaDRAWALFLG" + (i + 1)), true);
            }
        }

        // 平日以外または、時間外計算対象外の場合、理由、理由２、申請チェックボックス、取下げチェックボックスは非活性
        if (document.getElementById("txtpnlListAreaHOLIDAYKBN" + (i + 1)).value != "0" ||
            document.getElementById("txtpnlListAreaSTAFFKBNTAISHOGAI" + (i + 1)).value != "") {
            ChangeDisabled(document.getElementById("txtpnlListAreaYENDTIME" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaRIYU" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaRIYUETC" + (i + 1)), true);
            ChangeDisabled(document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)), true);
            ChangeDisabled(document.getElementById("chkpnlListAreaDRAWALFLG" + (i + 1)), true);
        }

        // 申請チェックボックス
        if (document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)) != undefined) {
            if (document.getElementById("hchkpnlListAreaENTRYFLG" + (i + 1)).innerText == "1") {
                document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)).checked = true;
            } else {
                document.getElementById("chkpnlListAreaENTRYFLG" + (i + 1)).checked = false;
            }
        }

        // 取下げチェックボックス
        if (document.getElementById("chkpnlListAreaDRAWALFLG" + (i + 1)) != undefined) {
            if (document.getElementById("hchkpnlListAreaDRAWALFLG" + (i + 1)).innerText == "1") {
                document.getElementById("chkpnlListAreaDRAWALFLG" + (i + 1)).checked = true;
            } else {
                document.getElementById("chkpnlListAreaDRAWALFLG" + (i + 1)).checked = false;
            }
        }
    }
}

// ○活性非活性変更
function ChangeDisabled(obj, flag) {
    // オブジェクトが存在しない場合抜ける
    if (obj == undefined) {
        return;
    }

    obj.disabled = flag;
}


// ○リスト内容変更処理
function ListChange(pnlList, Line) {

    // 申請
    if (document.getElementById("chkpnlListAreaENTRYFLG" + Line).checked) {
        document.getElementById("hchkpnlListAreaENTRYFLG" + Line).innerText = "1";
    } else {
        document.getElementById("hchkpnlListAreaENTRYFLG" + Line).innerText = "0";
    }

    // 取下げ
    if (document.getElementById("chkpnlListAreaDRAWALFLG" + Line).checked) {
        document.getElementById("hchkpnlListAreaDRAWALFLG" + Line).innerText = "1";
    } else {
        document.getElementById("hchkpnlListAreaDRAWALFLG" + Line).innerText = "0";
    }

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_SelectedIndex").value = Line;
        document.getElementById("WF_ButtonClick").value = "WF_ListChange";
        document.getElementById("WF_DISP_SaveX").value = document.getElementById("pnlListArea_DR").scrollLeft;
        document.getElementById("WF_DISP_SaveY").value = document.getElementById("pnlListArea_DR").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}


// ○左BOX用処理（DBクリック選択+値反映）
function List_Field_DBclick(obj, Line, fieldNM) {

    if (document.getElementById("txtpnlListArea" + fieldNM + Line) != null) {
        if (document.getElementById("txtpnlListArea" + fieldNM + Line).disabled == true) {
            return;
        }
    }

    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_FIELD").value = fieldNM;
        document.getElementById("WF_SelectedIndex").value = Line;
        document.getElementById("WF_LeftMViewChange").value = EXTRALIST;
        document.getElementById("WF_LeftboxOpen").value = "Open";
        document.getElementById("WF_ButtonClick").value = "WF_Field_DBClick";
        document.getElementById("WF_DISP_SaveX").value = document.getElementById("pnlListArea_DR").scrollLeft;
        document.getElementById("WF_DISP_SaveY").value = document.getElementById("pnlListArea_DR").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}


// ○調整画面切り替え
function DtabChange() {
    
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_DtabChange";
        document.getElementById("WF_DISP_SaveX").value = document.getElementById("pnlListArea_DR").scrollLeft;
        document.getElementById("WF_DISP_SaveY").value = document.getElementById("pnlListArea_DR").scrollTop;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ○月間調整画面調整
function AdjustFormat() {

    var tbody = document.getElementById("tblAdjucstArea_tbody");

    if (document.getElementById("WF_SEL_CAMPCODE").value == "02") {
        document.getElementById("SHUKCHOKHLDNISSU_L").colSpan = "2";
        document.getElementById("SHUKCHOKNISSU_L").colSpan = "2";
        document.getElementById("SHUKCHOKNHLDNISSU_L").colSpan = "2";
        document.getElementById("SHUKCHOKNNISSU_L").colSpan = "2";

    } else if (document.getElementById("WF_SEL_CAMPCODE").value == "03") {
        var tr = document.getElementById("LINE_11");
        tbody.removeChild(tr);

        var beforl = document.getElementById("SHUKCHOKHLDNISSU_L");
        var befor = document.getElementById("SHUKCHOKHLDNISSU");
        var afterl = document.getElementById("SHUKCHOKNNISSU_L");
        var after = document.getElementById("SHUKCHOKNNISSU");
        beforl.replaceWith(afterl);
        befor.replaceWith(after);

    } else if (document.getElementById("WF_SEL_CAMPCODE").value == "04") {
        var tr = document.getElementById("LINE_11");
        tbody.removeChild(tr);

        var beforl = document.getElementById("WWORKTIME_L");
        var befor = document.getElementById("WWORKTIME");
        var afterl = document.getElementById("JIKYUSHATIME_L");
        var after = document.getElementById("JIKYUSHATIME");
        beforl.replaceWith(afterl);
        befor.replaceWith(after);

    } else if (document.getElementById("WF_SEL_CAMPCODE").value == "05") {
        var tr = document.getElementById("LINE_11");
        tbody.removeChild(tr);

        var beforl = document.getElementById("SHUKCHOKHLDNISSU_L");
        var befor = document.getElementById("SHUKCHOKHLDNISSU");
        var afterl = document.getElementById("SHUKCHOKNNISSU_L");
        var after = document.getElementById("SHUKCHOKNNISSU");
        beforl.replaceWith(afterl);
        befor.replaceWith(after);

        tr = document.getElementById("LINE_23");
        beforl = tr.cells[0];
        befor = tr.cells[1];
        afterl = document.getElementById("JIKYUSHATIME_L");
        after = document.getElementById("JIKYUSHATIME");
        beforl.replaceWith(afterl);
        befor.replaceWith(after);
        after.rowSpan = "2";
    }
}
