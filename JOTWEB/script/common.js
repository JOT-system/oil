/**
 * @fileoverview システム共通JavaScript処理
 */

/**
 * 戻るボタン押下を無効
 * @param {object} なし
 * @return {undefined} なし
 */
window.addEventListener('load', function () {
    /* ブラウザ戻るボタンの禁止(無反応化) */
    if (window.history && window.history.pushState) {
        history.pushState('nohb', null, '');
        window.addEventListener('popstate', function (e) {
            if (!e.state) {
                //. もう一度履歴を操作して終了
                history.pushState('nohb', null, '');
                return;
            }
        });
    }
});

// 処理後カーソルを戻す
function AutoCursor() {
    document.body.style.cursor = "auto";
}

// ポップアップ確認
function ConfirmWindow() {

    if (document.getElementById("MF_SUBMIT").value == "FALSE" &&
        document.getElementById("MF_ALERT").value == "TRUE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";        //親画面を操作不可にする
        document.getElementById("MF_ALERT").value == "FALSE";

        //確認ウィンドウ表示
        var btn = document.getElementById("MF_AGAIN").value;
        var msg = document.getElementById("MF_ALT_MSG").value;
        var left = (screen.width - 450) / 2;
        var top = (screen.height - 200) / 2;
        var param = "status=0, scrollbars=0, directories=0, menubar=0, resizable=0, toolbar=0, location=0, width=450, height=200, left=" + left + ", top=" + top;
        var win = window.open("GRCO0108CONFIRM.aspx?MSGbtn=" + btn + "&MSGtext=" + msg, "_blank", param);

        //0.5秒置きに子画面の状況を確認する
        var interval = setInterval(function () {
            if (win.closed) {
                clearInterval(interval);
                if (document.getElementById("MF_ALERT").value == "OK") {
                    document.body.style.cursor = "wait";
                    document.forms[0].submit();
                } else {
                    document.getElementById("MF_SUBMIT").value = "FALSE";
                    document.getElementById("MF_ALERT").value = "FALSE";
                    return false;
                }
            } else {
                if (!win.document.hasFocus()) {
                    //子画面にフォーカスを充てる
                    win.focus();
                }
            }
        }, 500);

        //確認ポップアップ画面(confirm()はEdgeだと非表示に設定できるため中止)
        //if (window.confirm(document.getElementById("MF_ALT_MSG").value)) {
        //    document.getElementById("MF_SUBMIT").value = "TRUE";
        //    document.getElementById("MF_ALERT").value = "OK";
        //    document.getElementById("WF_ButtonClick").value = document.getElementById("MF_AGAIN").value;
        //    document.body.style.cursor = "wait";
        //    document.forms[0].submit();
        //} else {
        //    document.getElementById("MF_ALERT").value = "FALSE";
        //    return false;
        //}
    } else {
        document.getElementById("MF_ALERT").value = "FALSE";
        return false;
    }
}

// 左ボックスの拡張機能（ソート、フィルタ）を追加
// {TargetListBoxes} 以下のデータを配列としてもつ {リストボックスのID, ソート機能フラグ, フィルタ機能フラグ}
//                    ※ソート機能フラグ(0:無し, 1:名称のみ, 2:コードのみ, 3:両方)
//                    ※フィルタ機能フラグ(0:無し, 1:設定)
// 拡張機能によりリストが切れてしまう場合、各画面のリストボックスを<p>タグから<a>タグに変更してみてください。
function addLeftBoxExtention(TargetListBoxes) {
    // 引数未指定や配列がない場合は終了
    if (TargetListBoxes == null) {
        return;
    }
    if (TargetListBoxes.length == 0) {
        return;
    }
    // 左ボックスがない場合はそのまま終了
    var LeftBoxObj = document.getElementById('LF_LEFTBOX');
    if (LeftBoxObj == null) {
        return;
    }
    // 対象一覧のループ
    for (let i = 0; i < TargetListBoxes.length; i++) {
        // オブジェクトの存在チェック(存在しない場合はスキップ)
        if (document.getElementById(TargetListBoxes[i][0]) == null) {
            continue;
        }

        // リストボックスの取得、および拡張機能のフラグを取得
        var ListObj = document.getElementById(TargetListBoxes[i][0]);
        var SortFlag = TargetListBoxes[i][1];       //ソート機能フラグ
        var FilterFlag = TargetListBoxes[i][2];     //フィルタ機能フラグ
        var SubmitParam = TargetListBoxes[i][3];     //フィルタ機能　パラメータ

        // フラグが両方無しの場合意味がないので終了
        if (SortFlag == '0' && FilterFlag == '0') {
            return;
        }

        // ソート拡張機能を追加
        if (SortFlag == '1' || SortFlag == '2' || SortFlag == '3') {
            addLeftBoxSort(ListObj, SortFlag);
        }

        // フィルタ拡張機能を追加
        if (FilterFlag == '1') {
            addLeftBoxFilter(ListObj);
        } else if (FilterFlag == '2') {
            addLeftBoxsubmit(ListObj, SubmitParam)
        }

 //       // ソートデフォルトを名称検索状態にする
 //       if (SortFlag == '1' || SortFlag == '3') {
 //           var nameSortChkObj = document.getElementById('WF_LeftBoxSortNameASC');
 //           if (nameSortChkObj != null) {
 //               nameSortChkObj.click();
 //           }
 //       }

        // 1リストしか存在しえないので見つかったら処理終了
        return;
    }
}

// 左ボックスソート拡張機能を追加
//  ※ソート機能フラグ(0:無し, 1:名称のみ, 2:コードのみ, 3:両方)
function addLeftBoxSort(ListObj, SortFlag) {
    // オブジェクトの存在チェック(存在しない場合はスキップ)
    if (ListObj == null || ListObj == undefined) {
        return;
    }

    // ソートラジオボタンオブジェクトをクライアントサイドで生成するタグ
    var orderChooseTable = '<table id="WF_LeftBoxSortType">\n';

    // コード検索用ラジオボタン追加
    if (SortFlag == '2' || SortFlag == '3') {
        orderChooseTable = orderChooseTable + '  <tr>\n' +
            '    <td><input name="WF_LeftBoxSort" id="WF_LeftBoxSortCodeASC"  type="radio" value="CodeASC" />\n' +
            '        <label for="WF_LeftBoxSortCodeASC">コード昇順</label>\n' +
            '    </td>\n' +
            '    <td><input name="WF_LeftBoxSort" id="WF_LeftBoxSortCodeDESC" type="radio" value="CodeDesc" />\n' +
            '        <label for="WF_LeftBoxSortCodeDESC">コード降順</label>\n' +
            '    </td>\n' +
            '  </tr>\n';
    }

    // 名称検索用ラジオボタン追加
    if (SortFlag == '1' || SortFlag == '3') {
//        let checkVal = ''; // 名称検索のみの場合はNameAscにデフォルトチェックをあてる
//        if (SortFlag == '1') {
//            checkVal = 'checked="checked"';
//        }
        orderChooseTable = orderChooseTable + '  <tr>\n' +
            '    <td><input name="WF_LeftBoxSort" id="WF_LeftBoxSortNameASC"  type="radio" value="NameASC"  />\n' +
            '        <label for="WF_LeftBoxSortNameASC">名称昇順</label>\n' +
            '    </td>\n' +
            '    <td><input name="WF_LeftBoxSort" id="WF_LeftBoxSortNameDESC" type="radio" value="NameDesc" />\n' +
            '        <label for="WF_LeftBoxSortNameDESC">名称降順</label>\n' +
            '    </td>\n' +
            '  </tr>\n';
    }
    orderChooseTable = orderChooseTable + '</table>\n';

    // 上記で作成したタグをリストボックス前に挿入
    // ListObj.insertAdjacentHTML('beforebegin', orderChooseTable);
    // リストボックス前ではなく、ボタンの後に挿入(ダブルクリックイベント防止)
    document.getElementById('button').insertAdjacentHTML('beforeend', orderChooseTable);

    // ラジオボタンのイベントバインド(挿入したラジオボタンすべて)
    var objId = ListObj.id;
    var rbLists = document.getElementsByName('WF_LeftBoxSort');
    for (let i = 0; i < rbLists.length; i++) {
        var rbObj = rbLists[i];
        rbObj.onclick = (function (objId, rbObj) {
            return function () {
                leftListBoxSort(objId, rbObj);
            };
        })(objId, rbObj);
    }
}

// 左ボックスソートイベント
function leftListBoxSort(listBoxObjId, rbObj) {
    // オブジェクトの存在チェック(存在しない場合はスキップ)
    var sortBaseNode = document.getElementById(listBoxObjId);
    if (sortBaseNode === null) {
        return;
    }

    // 1件のみ0件はソートの意味がないのでそのまま終了
    if (sortBaseNode.length <= 1) {
        return;
    }

    var sortClone = sortBaseNode.cloneNode(true);
    sortClone.value = sortBaseNode.value;

    // リストボックスの選択肢ループ
    var optionArray = Array.prototype.slice.call(sortClone.options);

    // チェックボックスの値によって上記定義のソートメソッドを実行
    switch (rbObj.value) {
        case 'CodeASC':
            optionArray.sort(compareValueAsc);
            break;
        case 'CodeDesc':
            optionArray.sort(compareValueDesc);
            break;
        case 'NameASC':
            optionArray.sort(compareTextAsc);
            break;
        case 'NameDesc':
            optionArray.sort(compareTextDesc);
            break;
    }

    for (let i = 0; i < optionArray.length; i++) {
        sortClone.appendChild(sortClone.removeChild(optionArray[i]));
    }

    sortBaseNode.parentNode.replaceChild(sortClone, sortBaseNode);

    // フィルタ機能が有効な場合、画面で見えているクローンにも反映させる
    var cloneList = document.getElementById('WF_ListBoxCLONE');
    if (cloneList != null) {
        leftListBoxFilter(cloneList, listBoxObjId);
    }

    // コード昇順
    function compareValueAsc(a, b) {
        if (a.value > b.value) {
            return 1;
        } else if (a.value < b.value) {
            return -1
        } else {
            return 0
        }
    }

    // 名称昇順
    function compareTextAsc(a, b) {
        var displayStringAPart = a.textContent.substring(a.textContent.indexOf(':'));
        var displayStringBPart = b.textContent.substring(b.textContent.indexOf(':'));
        if (displayStringAPart > displayStringBPart) {
            return 1;
        } else if (displayStringAPart < displayStringBPart) {
            return -1
        } else {
            return 0
        }
    }

    // コード降順
    function compareValueDesc(a, b) {
        if (a.value < b.value) {
            return 1;
        } else if (a.value > b.value) {
            return -1
        } else {
            return 0
        }
    }

    // 名称降順
    function compareTextDesc(a, b) {
        var displayStringAPart = a.textContent.substring(a.textContent.indexOf(':'));
        var displayStringBPart = b.textContent.substring(b.textContent.indexOf(':'));
        if (displayStringAPart < displayStringBPart) {
            return 1;
        } else if (displayStringAPart > displayStringBPart) {
            return -1
        } else {
            return 0
        }
    }
}


// 左ボックスフィルタ拡張機能を追加
function addLeftBoxFilter(ListObj) {
    if (ListObj == null || ListObj == undefined) {
        return;
    }

    // フィルタテキスト及びフィルタ実行ボタンを生成するタグ
    var filterTable = '<table id="WF_LeftBoxFilter">\n' +
        '  <tr>\n' +
        '    <td><input id="WF_LEFTBOXFILTER" type="text" value="" title="Filter Condition" />\n' +
        '    </td>\n' +
        '    <td><input id="WF_buttonFilter" type="button" value="検 索" />\n' +
        '    </td>\n' +
        '  </tr>\n' +
        '</table>\n';

    // サーバーより取得したリストボックスでの選択肢の表示非表示をCSSでOnOffできないので
    // 隠して、リストボックスのクローンを生成しクローンで選択肢の追加削除を行う準備

    // サーバーより取得したリストボックスをspanタグで括り隠す
    let wrapper = document.createElement('span');
    wrapper.style.display = 'none';
    ListObj.parentNode.appendChild(wrapper);
    // サーバーより取得したリストボックスのクローンをID=WF_ListBoxCLONEとして生成
    var listClone = '<select id="WF_ListBoxCLONE" size="4">' + ListObj.innerHTML + '</select>';
    wrapper.appendChild(ListObj);
    //wrapper.insertAdjacentHTML('beforebegin', filterTable); //(ダブルクリックイベント防止)
    document.getElementById('button').insertAdjacentHTML('beforeend', filterTable);
    wrapper.insertAdjacentHTML('beforebegin', listClone);

    // フィルタボタンのイベントの紐づけ
    var leftFilterButton = document.getElementById('WF_buttonFilter');
    var leftListClone = document.getElementById('WF_ListBoxCLONE');
    leftFilterButton.onclick = (function (leftListClone, listBoxObj) {
        return function () {
            leftListBoxFilter(leftListClone, listBoxObj.id);
        };
    })(leftListClone, ListObj);

    // クローンのリストボックスのスタイルを指定
    leftListClone.className = "WF_ListBoxArea";

    // リストボックスのクローンにて選択されてイベントをバインド
    // クローンリストが選択されていたら、隠している本物のリストの選択肢も同じ状態にする。
    leftListClone.onchange = (function (leftListClone, ListObj) {
        return function () {
            var baseList = document.getElementById(ListObj.id);
            baseList.value = leftListClone.value;
        };
    })(leftListClone, ListObj);

    // リストボックスのクローンのダブルクリックイベントバインド
    // 本物のリストのダブルクリックイベントを発火させる
    leftListClone.ondblclick = (function (ListObj) {
        return function () {
            ListboxDBclick();
        };
    })(ListObj);
}


// 左ボックスフィルタイベント
function leftListBoxFilter(leftListClone, listBoxObjId) {
    var filterCond = document.getElementById('WF_LEFTBOXFILTER').value.trim();
    if (filterCond == "") {
        filterCond = '.*';
    } else {
        filterCond = '.*' + filterCond.replace(/[\\^$.*+?()[\]{}|]/g, '\\$&') + '.*';
    }

    var listBoxObjBase = document.getElementById(listBoxObjId);
    var listBoxObjClone = listBoxObjBase.cloneNode(true);
    listBoxObjClone.value = listBoxObjBase.value;

    // 一旦画面表示上の選択ボックスクリア 
    for (let i = leftListClone.options.length - 1; i >= 0; i--) {
        leftListClone.remove(i);
    }

    // 検索条件にて絞り込み
    var reg = new RegExp(filterCond, "i");
    for (let i = 0; i < listBoxObjClone.length; i++) {
        var optionElm = listBoxObjClone.options[i];
        // 検索条件が未設定の場合はすべて対象、それ以外は検索条件に一致すること
        var targetText = optionElm.textContent;
        if (reg.test(targetText)) {
            optionClone = optionElm.cloneNode(true);
            leftListClone.appendChild(optionClone);
            if (optionElm.selected) {
                optionClone.selected = true;
            }
        }
    }

    listBoxObjBase.parentNode.replaceChild(listBoxObjClone, listBoxObjBase);
}

// 左ボックス検索処理拡張機能を追加
//  ※機能フラグ(0:無し, 1:項目指定)
function addLeftBoxsubmit(ListObj, SubmitParam) {
    // オブジェクトの存在チェック(存在しない場合はスキップ)
    if (ListObj == null || ListObj == undefined) {
        return;
    }

    // 名称検索用ラジオボタン追加
    var orderChooseTable = '<table id="WF_LeftBoxSubmit">\n' +
            '<tr>\n' +
        '    <td><input name="WF_LeftBoxParam" id="WF_LeftBoxParam"  type="text" value="' + SubmitParam + '"  title="Filter Param" />\n' +
        '    </td>\n' +
        '    <td><input id="WF_LeftBoxSubmit" type="button" value="検 索" onclick="ButtonClick(\'WF_LeftBoxSubmit\')" />\n' +
        '    </td>\n' +
        '  </tr>\n';

    orderChooseTable = orderChooseTable + '</table>\n';

    // 上記で作成したタグをリストボックス前に挿入
    // ListObj.insertAdjacentHTML('beforebegin', orderChooseTable);
    // リストボックス前ではなく、ボタンの後に挿入(ダブルクリックイベント防止)
    document.getElementById('button').insertAdjacentHTML('beforeend', orderChooseTable);
}


// ○左Box用処理（左Box表示/非表示切り替え）
function REF_Field_DBclick(repfield, fieldNM, tabNo) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD_REP').value = repfield;
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_LeftMViewChange').value = tabNo;
        document.getElementById('WF_LeftboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_Field_DBClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    };
};
// ○左Box用処理（左Box表示/非表示切り替え）
function Field_DBclick(fieldNM, tabNo) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_LeftMViewChange').value = tabNo;
        document.getElementById('WF_LeftboxOpen').value = "Open";

        document.getElementById("WF_ButtonClick").value = "WF_Field_DBClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    };
};

// ○左BOX用処理（DBクリック選択+値反映）
function ListboxDBclick() {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LeftboxOpen').value = "";
        document.getElementById("WF_ButtonClick").value = "WF_ListboxDBclick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}
// ○左BOX用処理（DBクリック選択+値反映）
function WF_TableF_DbClick(index) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_LeftboxOpen').value = "";
        document.getElementById('WF_TBL_SELECT').value = index;
        document.getElementById("WF_ButtonClick").value = "WF_ListboxDBclick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}
// ○左BOX用処理（TextBox変更時、名称取得）
function TextBox_change(fieldNM) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_FIELD').value = fieldNM;
        document.getElementById('WF_ButtonClick').value = "WF_LeftBoxSelectClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ○右Box用処理（右Box表示/非表示切り替え）
function r_boxDisplay() {
    if (document.getElementById("RF_RIGHTBOX").style.width == "0em") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
        document.getElementById('WF_RightboxOpen').value = "Open";
        document.getElementById("WF_ButtonClick").value = "WF_RIGHT_VIEW_DBClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    } else {
        document.getElementById("RF_RIGHTBOX").style.width = "0em";
        document.getElementById('WF_RightboxOpen').value = "";
    };
};

// ○右BOX用処理（ラジオボタン）
function rightboxChange(tabNo) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_RightViewChange').value = tabNo;
        document.getElementById('WF_RightboxOpen').value = "Open";
        document.getElementById('WF_ButtonClick').value = "WF_RadioButonClick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ○右BOX用処理（メモ変更）
function MEMOChange() {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "WF_MEMOChange";
        document.getElementById('WF_RightboxOpen').value = "Open";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}


// ○ドロップ処理（ドラッグドロップ入力）
function f_dragEvent(e) {
    document.getElementById("WF_MESSAGE").textContent = "ファイルアップロード開始";
    document.getElementById("WF_MESSAGE").style.color = "blue";
    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";

    // ドラッグされたファイル情報を取得
    var files = e.dataTransfer.files;

    // 送信用FormData オブジェクトを用意
    var fd = new FormData();

    // ファイル情報を追加する
    for (var i = 0; i < files.length; i++) {
        fd.append("files", files[i]);
    }

    // XMLHttpRequest オブジェクトを作成
    var xhr = new XMLHttpRequest();

    // 「POST メソッド」「接続先 URL」を指定
    xhr.open("POST", "../GR/GRCO0100XLSUP.ashx", false)

    // イベント設定
    // ⇒XHR 送信正常で実行されるイベント
    xhr.onload = function (e) {
        if (e.currentTarget.status == 200) {
            document.getElementById('WF_ButtonClick').value = "WF_EXCEL_UPLOAD";
            document.body.style.cursor = "wait";
            document.forms[0].submit();                             //aspx起動
        } else {
            document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
            document.getElementById("WF_MESSAGE").style.color = "red";
            document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        }
    };

    // ⇒XHR 送信ERRで実行されるイベント
    xhr.onerror = function (e) {
        document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
    };

    // ⇒XHR 通信中止すると実行されるイベント
    xhr.onabort = function (e) {
        document.getElementById("WF_MESSAGE").textContent = "通信を中止しました。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
    };

    // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
    xhr.ontimeout = function (e) {
        document.getElementById("WF_MESSAGE").textContent = "タイムアウトエラーが発生しました。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
    };

    // 「送信データ」を指定、XHR 通信を開始する
    xhr.send(fd);
}

// ○メッセージクリア
function MsgClear() {
    document.getElementById("WF_MESSAGE").innerText = "";
}

// ○ヘルプBox用処理
function HelpDisplay() {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById("WF_ButtonClick").value = "HELP";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    };
};

// ○ドロップ処理（処理抑止）
function f_dragEventCancel(event) {
    event.preventDefault();  //イベントをキャンセル
};


// ○ダウンロード処理
function f_ExcelPrint() {
    // リンク参照
    window.open(document.getElementById("WF_PrintURL").value, "view", "_blank");
};

function f_PDFPrint() {
    // リンク参照
    window.open(document.getElementById("WF_PrintURL").value + "?" + (new Date()).getTime(), "view", "_blank");
};

// ○各ボタン押下処理
function ButtonClick(btn) {
    //サーバー未処理（MF_SUBMIT="FALSE"）のときのみ、SUBMIT
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        //押下されたボタンを設定
        document.getElementById("WF_ButtonClick").value = btn;
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    } else {
        return false;
    }
};

// ○ディテール(タブ切替)処理
function DtabChange(tabNo) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_DTAB_CHANGE_NO').value = tabNo;
        document.getElementById('WF_ButtonClick').value = "WF_DTAB_Click";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}

// ○ディテール(PDF表示)処理
function DtabPDFdisplay(filename) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_DTAB_PDF_DISP_FILE').value = filename;
        document.getElementById('WF_ButtonClick').value = "WF_DTAB_PDF_Click";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    };
};

// ○ディテール(PDF表示)処理
function PDFselectChange() {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE";
        document.getElementById('WF_ButtonClick').value = "WF_DTAB_PDF_Change";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    };
};


/**
 * 列名(cellfiedlname)及び、対象のパネルIDを元に対象の列ID、テーブルオブジェクトを返却
 * @param {string}colName カラム名称(ヘッダーのcellfiedlnameの設定値)
 * @param {string}listId 対象パネルオブジェクトのID
 * @return {object} 戻りオブジェクト.ColumnNo=対象カラム番号,戻りオブジェクト.TargetTable=対象のデータテーブル
 * @example 使用方法 呼出し側で 
 * var [ご自由な変数] = getTargetColumnNoTable('USDBR', 'WF_LISTAREA');
 * var [ご自由なcellObj] = [ご自由な変数].TargetTable.rows[ご自由な行No].cells[[ご自由な変数].ColumnNo];
 * →[ご自由なcellObj].textContent とするとセルの文字が取り出せたりします
 */
function getTargetColumnNoTable(colName, listId) {
    var listArea = document.getElementById(listId);
    // 表エリアの描画なし
    if (listArea === null) {
        return null; // そのまま終了
    }
    var leftHeaderDiv = document.getElementById(listArea.id + "_HL");
    var rightHeaderDiv = document.getElementById(listArea.id + "_HR");
    var leftDataDiv = document.getElementById(listArea.id + "_DL");
    var rightDataDiv = document.getElementById(listArea.id + "_DR");
    if (leftHeaderDiv === null && rightHeaderDiv === null) {
        return null; // そのまま終了
    }
    // 左固定列のカラム名検索
    if (leftHeaderDiv !== null && leftHeaderDiv.getElementsByTagName("table") !== null) {
        let leftHeaderTable = leftHeaderDiv.getElementsByTagName("table")[0];
        let leftHeaderRow = leftHeaderTable.rows[0]
        for (let i = 0; i < leftHeaderRow.cells.length; i++) {
            let targetCell = leftHeaderRow.cells[i];
            if (targetCell.getAttribute("cellfiedlname") === colName) {
                let retDataTable = leftDataDiv.getElementsByTagName("table")[0];
                let retVal = { ColumnNo: i, TargetTable: retDataTable };
                return retVal;
            }
        }
    }
    // 右動的列のカラム名検索
    if (rightHeaderDiv !== null && rightHeaderDiv.getElementsByTagName("table") !== null) {
        let rightHeaderTable = rightHeaderDiv.getElementsByTagName("table")[0];
        let rightHeaderRow = rightHeaderTable.rows[0]
        for (let i = 0; i < rightHeaderRow.cells.length; i++) {
            let targetCell = rightHeaderRow.cells[i];
            if (targetCell.getAttribute("cellfiedlname") === colName) {
                let retDataTable = rightDataDiv.getElementsByTagName("table")[0];
                let retVal = { ColumnNo: i, TargetTable: retDataTable };
                return retVal;
            }
        }
    }
    // ここまで来た場合は検索結果なしnull返却
    return null;
}
/**
 * リストの共通イベント(ホイール、横スクロール)をバインド
 * @param {string}listObjId リストオブジェクトのID
 * @param {string}isPostBack 各ページで'<%= if(IsPostBack = True, "1", "0") %>'を指定（外部スクリプトではサーバータグが使用できない為)
 * @param {boolean}adjustHeight 高さを調整するか
 * @param {boolean}keepHScrollWhenPostBack 省略可 ポストバック時に横スクロールを保持するか(True:保持(デフォルト),False:保持しない)
 * @param {boolean}resetXposFirstLoad 省略可 初回ロード時にスクロールバー位置の記憶をリセットするか(True:リセット(デフォルト),False:保持))
 * @param {boolean}useWheelEvent 省略可 マウスホイールイベントを使用するか(True:使用する(デフォルト),False:使用しない))
 * @return {undefined} なし
 * @example 使用方法  
 * bindListCommonEvents('<%= Me.WF_LISTAREA.ClientId %>','<%= if(IsPostBack = True, "1", "0") %>');
 */
function bindListCommonEvents(listObjId, isPostBack, adjustHeight, keepHScrollWhenPostBack, resetXposFirstLoad, useWheelEvent) {
    // 第3引数が未指定の場合
    if (adjustHeight === undefined) {
        adjustHeight = false;
    }
    // 第4引数が未指定の場合
    if (keepHScrollWhenPostBack === undefined) {
        keepHScrollWhenPostBack = true;
    }
    // 第5引数が未指定の場合
    if (resetXposFirstLoad === undefined) {
        resetXposFirstLoad = true;
    }
    // 第6引数が未指定の場合
    if (useWheelEvent === undefined) {
        useWheelEvent = true;
    }

    var listObj = document.getElementById(listObjId);
    // そもそもリストがレンダリングされていなければ終了
    if (listObj === null) {
        return;
    }
    // Mouseホイールイベントのバインド
    if (useWheelEvent) {
        var mousewheelevent = 'onwheel' in listObj ? 'wheel' : 'onmousewheel' in listObj ? 'mousewheel' : 'DOMMouseScroll';
        listObj.addEventListener(mousewheelevent, commonListMouseWheel, true);
    }
    // 横スクロールイベントのバインド
    // 可変列ヘッダーテーブル、可変列データテーブルのオブジェクトを取得
    var headerTableObj = document.getElementById(listObjId + '_HR');
    var dataTableObj = document.getElementById(listObjId + '_DR');
    // 可変列の描画がない場合はそのまま終了
    if (headerTableObj === null || dataTableObj === null) {
        return;
    }
    // スクロールイベントのバインド
    dataTableObj.addEventListener('scroll', (function (listObj) {
        return function () {
            commonListScroll(listObj);
        };
    })(listObj), false);

    // 画面キーダウンイベントのバインド
    // GridView処理（矢印処理）
    if (useWheelEvent) {
        document.addEventListener('keydown', (function () {
            return function () {
                var objSubmit = document.getElementById('MF_SUBMIT');
                var objEventHandler = document.getElementById('WF_ButtonClick');
                //var objMouseWheel = document.getElementById('WF_MouseWheel');
                // ↑キー押下時
                if (window.event.keyCode === 38) {
                    if (objSubmit.value === 'FALSE') {
                        // リストの現在見えている位置が最上部の場合はポストバックせず終了
                        var listPosition = document.getElementById("WF_GridPosition");
                        if (listPosition !== null) {
                            if (listPosition.value === '' || listPosition.value === '1') {
                                return false;
                            }
                        }
                        objSubmit.value = 'TRUE';
                        //objMouseWheel.value = '-';
                        objEventHandler.value = "WF_MouseWheelDown";
                        document.body.style.cursor = "wait";
                        document.forms[0].submit();  //aspx起動
                        return false;
                    };
                };
                // ↓キー押下時
                if (window.event.keyCode === 40) {
                    if (objSubmit.value === 'FALSE') {
                        objSubmit.value = 'TRUE';
                        //objMouseWheel.value = '+';
                        objEventHandler.value = "WF_MouseWheelUp";
                        document.body.style.cursor = "wait";
                        document.forms[0].submit();  //aspx起動
                        return false;
                    };
                };
            };
        })(), false);
    }

    // スクロールを保持する場合
    if (isPostBack === '0' && keepHScrollWhenPostBack && resetXposFirstLoad) {
        // 初回ロード時は左スクロール位置を0とる
        setCommonListScrollXpos(listObj.id, '0');
    }
    // ポストバック時は保持したスクロール位置に戻す
    if (isPostBack === '1' && keepHScrollWhenPostBack) {
        var xpos = getCommonListScrollXpos(listObj.id);
        dataTableObj.scrollLeft = xpos;
        var e = document.createEvent("UIEvents");
        e.initUIEvent("scroll", true, true, window, 1);
        dataTableObj.dispatchEvent(e);
    }
    //高さ調整
    if (adjustHeight === true) {
        /* 現在の表示を調整 */
        commonListAdjustHeight(listObj.id);
        /* リサイズイベントにバインド */
        window.addEventListener('resize', function () {
            commonListAdjustHeight(listObj.id);
        }, false);
    }
    bindCommonListHighlight(listObj.id);
}
/* 共通リストのハイライトイベント */
function bindCommonListHighlight(listObjId) {
    // 可変列ヘッダーテーブル、可変列データテーブルのオブジェクトを取得
    var leftDataDivObj = document.getElementById(listObjId + '_DL');
    var rightDataDivObj = document.getElementById(listObjId + '_DR');
    if (leftDataDivObj === null || rightDataDivObj === null) {
        return;
    }
    var leftTrList = leftDataDivObj.getElementsByTagName('tr');
    var rightTrList = rightDataDivObj.getElementsByTagName('tr');
    for (let i = 0; i < leftTrList.length; i++) {
        var leftTr = leftTrList[i];
        var rightTr = null;
        if (rightTrList !== null) {
            rightTr = rightTrList[i];
        }
        // 左のEventListener設定
        leftTr.addEventListener('mouseover', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.add("hover");
                rightTr.classList.add("hover");
            };
        })(leftTr, rightTr), false);
        // 左のEventListener設定
        leftTr.addEventListener('mouseout', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.remove("hover");
                rightTr.classList.remove("hover");
            };
        })(leftTr, rightTr), false);
        // 右のEventListener設定
        rightTr.addEventListener('mouseover', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.add("hover");
                rightTr.classList.add("hover");
            };
        })(leftTr, rightTr), false);
        // 右のEventListener設定
        rightTr.addEventListener('mouseout', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.remove("hover");
                rightTr.classList.remove("hover");
            };
        })(leftTr, rightTr), false);
    }
}
/**
 * リストデータ部スクロール共通処理（ヘッダー部のスクロールを連動させる)
 * @param {object}listObj リスト全体のオブジェクト
 * @return {undefined} なし
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function commonListScroll(listObj) {
    var rightHeaderTableObj = document.getElementById(listObj.id + '_HR');
    var rightDataTableObj = document.getElementById(listObj.id + '_DR');
    var leftDataTableObj = document.getElementById(listObj.id + '_DL');

    setCommonListScrollXpos(listObj.id, rightDataTableObj.scrollLeft);
    rightHeaderTableObj.scrollLeft = rightDataTableObj.scrollLeft; // 左右連動させる
    leftDataTableObj.scrollTop = rightDataTableObj.scrollTop; // 上下連動させる
}
/**
 * リストの高さを調節する
 * @param {string}listId リスト全体のオブジェクトID
 * @return {string} リスト設定文字
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function commonListAdjustHeight(listId) {
    var userAgent = window.navigator.userAgent.toLowerCase();
    var browserAjust = -1;
    if (userAgent.indexOf('msie') !== -1 ||
        userAgent.indexOf('trident') !== -1) {
        //IE
    } else if (userAgent.indexOf('edge') !== -1) {
        //Edge
    } else if (userAgent.indexOf('chrome') != -1) {
        //Chrome
        //browserAjust = -10;

    } else if (userAgent.indexOf('safari') != -1) {
        //Safari
    } else if (userAgent.indexOf('firefox') != -1) {
        //FireFox
    } else if (userAgent.indexOf('opera') != -1) {
        //Opera
    }

    var listObj = document.getElementById(listId);
    var listObjParent = listObj.parentNode;
    var parentRect = listObjParent.getBoundingClientRect();
    var listRect = listObj.getBoundingClientRect();

    var listHeight = parentRect.top + listObjParent.clientHeight - listRect.top;

    //alert(parentBottom);
    listObj.style.height = (listHeight + browserAjust) + 'px';
}
/**
 * リストの横スクロール位置をwebStrage(セッションストレージ)に保持した値より取得する
 * @param {string}listId リスト全体のオブジェクトID
 * @return {string} リスト設定文字
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function getCommonListScrollXpos(listId) {
    var saveKey = document.forms[0].id + listId + "xScrollPos";
    var retValue = sessionStorage.getItem(saveKey);
    if (retValue === null) {
        retValue = '';
    }
    return retValue;
}
/**
 * リストの横スクロール位置をwebStrage(セッションストレージ)に保持する
 * @param {string}listId リスト全体のオブジェクトID
 * @param {string}val リストに保持する値
 * @return {undefined} なし
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function setCommonListScrollXpos(listId, val) {
    var saveKey = document.forms[0].id + listId + "xScrollPos";
    sessionStorage.setItem(saveKey, val);
}

/**
 * 一覧表のマウスホイールイベント
 * @param {Event}event 未使用
 * @example サーバーにポストしスクロール分の一覧データを表示
 */
function commonListMouseWheel(event) {
    var objSubmit = document.getElementById("MF_SUBMIT");
    //var objMouseWheel = document.getElementById("WF_MouseWheel");
    var objEventHandler = document.getElementById('WF_ButtonClick');
    if (objSubmit.value === "FALSE") {
        if (window.event.wheelDelta < 0) {
            //objMouseWheel.value = "+";
            objEventHandler.value = "WF_MouseWheelUp";
        } else {
            // リストの現在見えている位置が最上部の場合はポストバックせず終了
            var listPosition = document.getElementById("WF_GridPosition");
            if (listPosition !== null) {
                if (listPosition.value === '' || listPosition.value === '1') {
                    return false;
                }
            }
            //objMouseWheel.value = "-";
            objEventHandler.value = "WF_MouseWheelDown";

        }
        objSubmit.value = "TRUE";
        document.body.style.cursor = "wait";
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    }
}
/**
 * リストのソートイベント
 * @param {string}listId 対象リストのID
 * @param {string}fieldId ソート対象のフィールド
 * @example ソート設定を記載しサーバーへサブミット
 */
function commonListSortClick(listId, fieldId) {
    var objSubmit = document.getElementById('MF_SUBMIT');
    var formId = document.forms[0].id;
    var sortOrderObj = document.getElementById('hdnListSortValue' + formId + listId);
    var objEventHandler = document.getElementById('WF_ButtonClick');
    var listPosition = document.getElementById('WF_GridPosition');
    if (objSubmit === null || sortOrderObj === null) {
        return false;
    }

    var sortOrderValue = sortOrderObj.value;
    if (sortOrderValue === '') {
        sortOrderValue = fieldId + ' ASC';
    } else {
        var sortOrderValueArray = [];
        if (sortOrderValue !== '') {
            sortOrderValueArray = sortOrderValue.split(',');
        }
        var keyValueSort = {};
        for (var i = 0; i < sortOrderValueArray.length; i++) {
            var sortOrder = sortOrderValueArray[i];
            keyValueSort[sortOrder.split(' ')[0]] = { sort: i, value: sortOrder.split(' ')[1] };
        }

        if (keyValueSort[fieldId]) {
            if (keyValueSort[fieldId].value === "ASC") {
                keyValueSort[fieldId].value = "DESC";
            } else if ((keyValueSort[fieldId].value === "DESC")) {
                delete keyValueSort[fieldId];
            }
        } else {
            keyValueSort[fieldId] = { sort: 9999, value: "ASC" };
        }
        var retArray = [];
        for (key in keyValueSort) {
            retArray.push({ field: key, sort: keyValueSort[key].sort, value: keyValueSort[key].value });
        }
        retArray.sort(function (a, b) {
            if (a.sort < b.sort) return -1;
            if (a.sort > b.sort) return 1;
            return 0;
        });
        sortOrderValue = '';
        for (var i = 0; i < retArray.length; i++) {
            if (sortOrderValue === '') {
                sortOrderValue = retArray[i].field + ' ' + retArray[i].value;
            } else {
                sortOrderValue = sortOrderValue + ',' + retArray[i].field + ' ' + retArray[i].value;
            }
        }
    }
    sortOrderObj.value = sortOrderValue;
    objSubmit.value = "TRUE";
    objEventHandler = "WF_LIST_SORTING";
    document.body.style.cursor = "wait";
    document.forms[0].submit();                            //aspx起動
}

/**
 *  上部一覧表のリストダブルクリックイベント
 * @param {object} obj TR(行)オブジェクト
 * @param {string} lineCnt 行No
 * @return {undefined} なし
 * @description 詳細エリアのタブ変更時イベント
 */
function ListDbClick(obj, lineCnt) {
    var objSubmit = document.getElementById('MF_SUBMIT');
    var objListDbClick = document.getElementById('WF_GridDBclick');
    var objEventHandler = document.getElementById('WF_ButtonClick');
    // 対象のオブジェクトが存在していない場合は終了
    if (objSubmit === null || objListDbClick === null) {
        return;
    }
    // SUBMITフラグを見て処理実行
    if (objSubmit.value === 'FALSE') {
        objSubmit.value = 'TRUE';
        objListDbClick.value = lineCnt;
        var objIsHideDetailBox = document.getElementById('WF_IsHideDetailBox');
        if (objIsHideDetailBox !== null) {
            objIsHideDetailBox.value = '0';
        }
        objEventHandler.value = "WF_GridDBclick";
        document.body.style.cursor = "wait";
        document.forms[0].submit();
    }
}
/**
 * ポップアップの背面操作禁止を解除
 * @param {string} modalWapperId ポップアップのID
 * @return {undefined} なし
 */
function commonCloseModal(modalWapperId) {
    var disableElemType = 'select,input:not([type="hidden"]),textarea,button';
    var popUpInnerObjects = null;
    var popUpInnerObjectsId = new Array();
    if (modalWapperId !== '') {
        var keepElemType = '{0} select,{0} input:not([type="hidden"]),{0} textarea,{0} button';
        keepElemType = keepElemType.split('{0}').join("#" + modalWapperId);
        popUpInnerObjects = document.forms[0].querySelectorAll(keepElemType);
        if (popUpInnerObjects !== null) {
            for (let i = 0, len = popUpInnerObjects.length; i < len; ++i) {
                popUpInnerObjectsId.push(popUpInnerObjects[i].id);
            }
        }
    }
    document.forms[0].removeAttribute('data-showmodal');
    var inputItems = document.forms[0].querySelectorAll(disableElemType);
    for (let i = 0, len = inputItems.length; i < len; ++i) {
        let inputItem = inputItems[i];
        if (popUpInnerObjectsId.indexOf(inputItem.id) >= 0) {
            continue;
        }
        inputItem.tabIndex = null;
        inputItem.removeAttribute('tabIndex');
        let indexVal = inputItem.getAttribute('data-orgtabindex');
        if (indexVal !== null) {
            inputItem.tabIndex = indexVal;
            inputItem.removeAttribute('data-orgtabindex');
        }
    }
}
/**
 * ポップアップの背面操作を禁止
  * @param {string} modalWapperId ポップアップのID
 * @return {undefined} なし
 */
function commonDisableModalBg(modalWapperId) {
    var disableElemType = 'select,input:not([type="hidden"]),textarea,button,div.firstPage,div.lastPage';
    var popUpInnerObjects = null;
    var popUpInnerObjectsId = new Array();
    if (modalWapperId !== '') {
        var keepElemType = '{0} select,{0} input:not([type="hidden"]),{0} textarea,{0} button';
        keepElemType = keepElemType.split('{0}').join("#" + modalWapperId);
        popUpInnerObjects = document.forms[0].querySelectorAll(keepElemType);
        if (popUpInnerObjects !== null) {
            for (let i = 0, len = popUpInnerObjects.length; i < len; ++i) {
                popUpInnerObjectsId.push(popUpInnerObjects[i].id);
            }
        }
    }
    var inputItems = document.forms[0].querySelectorAll(disableElemType);
    for (let i = 0, len = inputItems.length; i < len; ++i) {
        let inputItem = inputItems[i];
        if (popUpInnerObjectsId.indexOf(inputItem.id) >= 0) {
            continue;
        }
        let indexVal = inputItem.tabIndex;
        if (inputItem.hasAttribute('tabIndex')) {
            inputItem.dataset.orgtabindex = indexVal; //('data-orgtabindex', indexVal);
        }
        inputItem.tabIndex = '-1';
    }
    // keydownイベントの無効化
    if (modalWapperId !== '') {
        var modalWapperObj = document.getElementById(modalWapperId);
        if (modalWapperObj !== null) {
            modalWapperObj.tabIndex = '-1';
            modalWapperObj.style.outline = 'none';
            // 画面キーダウンイベントのバインド
            modalWapperObj.addEventListener('keydown', (function (event) {
                return function (event) {
                    // ↑キー押下時
                    if (window.event.keyCode === 38) {
                        window.event.stopPropagation(); //フォームのキーダウンイベントに↑キー伝達抑止
                    };
                    // ↓キー押下時
                    if (window.event.keyCode === 40) {
                        window.event.stopPropagation(); //フォームのキーダウンイベントに↓キー伝達抑止
                    };
                };
            })(event), false);
        }
    }
}
/**
 *  ウェイト画面表示
 * @return {undefined} なし
 * @description 
 */
function commonDispWait() {
    var hasElm = document.getElementById('comloading');
    if (hasElm !== null) {
        document.body.removeChild(hasElm);
    }
    // ウエイトスクリーン用半透明の大枠オブジェクト
    var lodingObj;
    lodingObj = document.createElement('div');
    lodingObj.id = 'comloading';
    lodingObj.classList.add('comloading');
    // ウエイトスクリーン用のフォーカス移動抑止のオブジェクト
    var forsubObj;
    forsubObj = document.createElement('input');
    forsubObj.id = 'comlodingtextbox';
    forsubObj.type = 'text';
    forsubObj.classList.add('comlodingtextbox');
    forsubObj.tabindex = '1';
    lodingObj.appendChild(forsubObj);
    // ウェイトスクリーン用のアニメーション枠
    var lodingMsgObj = document.createElement('div');
    lodingMsgObj.classList.add('comloadingmsg');
    // 子要素追加
    var lodingMsgChild1Obj = document.createElement('div');
    var lodingMsgChild2Obj = document.createElement('div');
    var lodingMsgChild3Obj = document.createElement('div');
    lodingMsgObj.appendChild(lodingMsgChild1Obj);
    lodingMsgObj.appendChild(lodingMsgChild2Obj);
    lodingMsgObj.appendChild(lodingMsgChild3Obj);
    //lodingMsgObj.innerText = 'Loading.....';
    lodingObj.appendChild(lodingMsgObj);
    document.body.appendChild(lodingObj);
    // テキストボックスにフォーカスを合わせておく
    forsubObj = document.getElementById('comlodingtextbox');
    forsubObj.select();
    forsubObj.onblur = (function (forsubObj) {
        return function () {
            forsubObj.select();
        }
    }(forsubObj));
    commonDisableModalBg('comloading');
}
/**
 *  ウェイト画面非表示
 * @return {undefined} なし
 * @description 
 */
function commonHideWait() {
    var hasElm = document.getElementById('comloading');
    if (hasElm !== null) {
        commonCloseModal('');
        document.body.removeChild(hasElm);
    }
}
