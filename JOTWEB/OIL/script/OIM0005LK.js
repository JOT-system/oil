// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
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
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    }
    // 共通一覧のスクロールイベント紐づけ 
    bindListCommonEvents(pnlListAreaId, IsPostBack, true);

    // ハイライト表示（削除データ行） 
    SetDeletRowHighlight();

    // maxlength設定 
    SetMaxLength(document.getElementById("WF_GridPosition").value);

    //Enterキーで下のテキスト
    commonBindEnterToVerticalTabStep();
}

// 〇数値のみ入力可能
function CheckNum(pnlList, Line, fieldNM) {
    if (fieldNM === "PROCKBN") {
        // 1,2,3のみ入力可能　
        if (event.keyCode < 49 || event.keyCode > 51) {
            window.event.returnValue = false; // IEだと効かないので↓追加
            event.preventDefault(); // IEはこれで効く
        }
    }
    if (fieldNM === "MIDDLEOILCODE") {
        // 1,2,5のみ入力可能　
        if (event.keyCode != 49 && event.keyCode != 50 && event.keyCode != 53) {
            window.event.returnValue = false; // IEだと効かないので↓追加
            event.preventDefault(); // IEはこれで効く
        }
    }
}

// ○左Box用処理（左Box表示/非表示切り替え）
function ListField_DBclick(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;

        if (fieldNM === "PROCKBN") {
            document.getElementById('WF_LeftMViewChange').value = 999;
        }
        else if (fieldNM === "MIDDLEOILCODE") {
            document.getElementById('WF_LeftMViewChange').value = 29;
        }
        document.getElementById('WF_LeftboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_Field_DBClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ○一覧用処理（チェンジイベント）
function ListField_Change(pnlList, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value === "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_GridDBclick').value = Line;
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_ButtonClick').value = "WF_ListChange";
        document.forms[0].submit();
    }
}

// ○行のハイライト表示（削除データ行は灰色）
function SetDeletRowHighlight() {
    let generatedTables = document.querySelectorAll("div[data-generated='1']");
    if (generatedTables === null) {
        return;
    }
    if (generatedTables.length === 0) {
        return;
    }
    for (let i = 0, len = generatedTables.length; i < len; ++i) {
        let generatedTable = generatedTables[i];
        let panelId = generatedTable.id;
        // 情報フィールドが存在するかチェック
        let kaisouStatusFieldName = 'DELFLG';
        let infoHeader = generatedTable.querySelector("th[cellfieldname='" + kaisouStatusFieldName + "']");
        if (infoHeader === null) {
            //存在しない場合はスキップ
            continue;
        }
        // リストの列番号取得
        let colIdx = infoHeader.cellIndex;
        // 右可変行オブジェクトの取得
        let dataAreaDrObj = document.getElementById(panelId + "_DR");
        //右可変行が未存在なら終了
        if (dataAreaDrObj === null) {
            return;
        }
        let rightTableObj = dataAreaDrObj.querySelector('table');
        if (rightTableObj === null) {
            return;
        }
        let leftTableObj = document.getElementById(panelId + "_DL").querySelector('table');
        for (let rowIdx = 0, rowlen = rightTableObj.rows.length; rowIdx < rowlen; rowIdx++) {
            // ありえないがデータ列のインデックス（最大カラム数）が情報カラムの位置より小さい場合
            if (rightTableObj.rows[rowIdx].cells.length < colIdx) {
                // ループの終了
                break;
            }

            let cellObj = rightTableObj.rows[rowIdx].cells[colIdx];
            if (cellObj.textContent === '0') {
                continue;
            }
            rightTableObj.rows[rowIdx].classList.add('hasDeleteRowValue');
            leftTableObj.rows[rowIdx].classList.add('hasDeleteRowValue');
        }
    }
}

// ○テキストBOXのmaxlength設定
function SetMaxLength(stpos) {
    var objTable = document.getElementById("pnlListArea_DR").children[0];
    for (var i = stpos; i < stpos + objTable.rows.length - 1; i++) {
        var objProcKbn = document.getElementById("txtpnlListAreaPROCKBN" + i)
        if (objProcKbn != null) {
            objProcKbn.maxLength = 1;
        }

        var objMiddleOilCode = document.getElementById("txtpnlListAreaMIDDLEOILCODE" + i)
        if (objMiddleOilCode != null) {
            objMiddleOilCode.maxLength = 1;
        }
    }
}

/**
 * COMMONをコピー
 *  リストテーブルのEnterキーで下のテキストにタブを移すイベントバインド
 * @return {undefined} なし
 * @description 
 */
function commonBindEnterToVerticalTabStep() {
    let generatedTables = document.querySelectorAll("div[data-generated='1']");
    if (generatedTables === null) {
        return;
    }
    if (generatedTables.length === 0) {
        return;
    }
    let focusObjKey = document.forms[0].id + "ListFocusObjId";
    if (sessionStorage.getItem(focusObjKey) !== null) {
        if (IsPostBack === undefined) {
            sessionStorage.removeItem(focusObjKey);
        }
        if (IsPostBack === '1') {
            focusObjId = sessionStorage.getItem(focusObjKey);
            setTimeout(function () {
                document.getElementById(focusObjId).focus();
                sessionStorage.removeItem(focusObjKey);
            }, 10);
        } else {
            sessionStorage.removeItem(focusObjKey);
        }

    }

    for (let i = 0, len = generatedTables.length; i < len; ++i) {
        let generatedTable = generatedTables[i];
        let panelId = generatedTable.id;
        //生成したテーブルオブジェクトのテキストボックス確認
        let textBoxes = generatedTable.querySelectorAll('input[type=text]');
        //テキストボックスが無ければ次の描画されたリストテーブルへ
        if (textBoxes === null) {
            continue;
        }
        // テキストボックスのループ
        for (let j = 0; j < textBoxes.length; j++) {
            let textBox = textBoxes[j];
            let lineCnt = textBox.attributes.getNamedItem("rownum").value;
            let fieldName = textBox.id.substring(("txt" + panelId).length);
            fieldName = fieldName.substring(0, fieldName.length - lineCnt.length);
            let nextTextFieldName = fieldName;
            if (textBoxes.length === j + 1) {
                // 最後のテキストボックスは先頭のフィールド
                nextTextFieldName = textBoxes[0].id.substring(("txt" + panelId).length);
                lineCnt = textBoxes[0].attributes.getNamedItem("rownum").value;
                nextTextFieldName = nextTextFieldName.substring(0, nextTextFieldName.length - lineCnt.length);
            } else if (textBoxes.length > j + 1) {
                nextTextFieldName = textBoxes[j + 1].id.substring(("txt" + panelId).length);
                lineCnt = textBoxes[j + 1].attributes.getNamedItem("rownum").value;
                nextTextFieldName = nextTextFieldName.substring(0, nextTextFieldName.length - lineCnt.length);
            }

            textBox.dataset.fieldName = fieldName;
            textBox.dataset.nextTextFieldName = nextTextFieldName;
            textBox.addEventListener('keypress', (function (textBox, panelId) {
                return function () {
                    if (event.key === 'Enter') {
                        if (commonKeyEnterProgress === false) {
                            commonKeyEnterProgress = true; //Enter連打抑止
                            commonListEnterToVerticalTabStep(textBox, panelId);
                            return setTimeout(function () {
                                commonKeyEnterProgress = false;　///Enter連打抑止
                            }, 10); // 5ミリ秒だと連打でフォーカスパニックになったので10ミリ秒に
                        }
                    }
                };
            })(textBox, panelId), true);
        }
    }
}
/**
 *  リストテーブルのEnterキーで下のテキストにタブを移すイベント
 * @param {Node} textBox テキストボックス
 * @param {string} panelId テキストボックス
 * @return {undefined} なし
 * @description 
 */
function commonListEnterToVerticalTabStep(textBox, panelId) {
    let curLineCnt = Number(textBox.attributes.getNamedItem("rownum").value);
    let fieldName = textBox.dataset.fieldName;
    let nextTextFieldName = textBox.dataset.nextTextFieldName;
    let found = false;
    let focusNode;
    let maxLineCnt = 999;
    let targetObjPrefix = "txt" + panelId + fieldName;
    while (found === false) {
        curLineCnt = curLineCnt + 1;
        let targetObj = targetObjPrefix + curLineCnt;
        focusNode = document.getElementById(targetObj);
        if (focusNode !== null) {
            found = true;
        } else {
            // COMMONとは、ここが違う
            curLineCnt = Number(document.getElementById("WF_GridPosition").value) - 1;

            targetObjPrefix = "txt" + panelId + nextTextFieldName;
        }

        // 無限ループ抑止
        if (maxLineCnt === curLineCnt) {
            found = true;
        }
    }

    var parentNodeObj = textBox.parentNode;
    if (parentNodeObj.hasAttribute('onchange')) {
        var focusObjKey = document.forms[0].id + "ListFocusObjId";
        sessionStorage.setItem(focusObjKey, focusNode.id);
    }
    //var retValue = sessionStorage.getItem(forcusObjKey);
    //if (retValue === null) {
    //    retValue = '';
    //}
    focusNode.focus();
    return;
}

