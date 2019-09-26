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

// ○使用有無変更
function ChangeOrgUse() {

    var objTable = document.getElementById("pnlListArea_DR").children[0];

    for (var i = 0; i < objTable.rows.length; i++) {

        document.getElementById("rblORGUSEORGUSE" + (i + 1) + "_0").checked = false
        document.getElementById("rblORGUSEORGUSE" + (i + 1) + "_1").checked = false

        if (document.getElementById("lrblORGUSEORGUSE" + (i + 1)).innerText == "01") {
            document.getElementById("rblORGUSEORGUSE" + (i + 1) + "_0").checked = true
            ChangeDisabled(document.getElementById("txtpnlListAreaSTORICODE" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE01" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE02" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE03" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE04" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE05" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaYTORICODE" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaKTORICODE" + (i + 1)), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaSEQ" + (i + 1)), false);
        } else {
            document.getElementById("rblORGUSEORGUSE" + (i + 1) + "_1").checked = true
            ChangeDisabled(document.getElementById("txtpnlListAreaSTORICODE" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE01" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE02" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE03" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE04" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE05" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaYTORICODE" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaKTORICODE" + (i + 1)), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaSEQ" + (i + 1)), true);
        }
    }
}

// ○リスト内容変更処理
function ListChange(pnlList, Line) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        // オブジェクトが存在しない場合抜ける
        if (document.getElementById("pnlListArea_DL") == undefined) {
            return;
        }
        let trlst = document.getElementById("pnlListArea_DL").getElementsByTagName("tr");
        trlst[Line - 1].getElementsByTagName("th")[1].innerHTML = "更新";
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
        document.getElementById("WF_SelectFIELD").value = obj.firstChild.id;
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
//〇使用有無変更時処理
function selectUse(obj, Line, fieldNM) {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        if (document.getElementById("rblORGUSEORGUSE" + Line + "_0").checked) {
            document.getElementById("lrblORGUSEORGUSE" + Line).innerText = "01";
            ChangeDisabled(document.getElementById("txtpnlListAreaSTORICODE" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE01" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE02" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE03" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE04" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE05" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaYTORICODE" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaKTORICODE" + Line), false);
            ChangeDisabled(document.getElementById("txtpnlListAreaSEQ" + Line), false);
        } else if (document.getElementById("rblORGUSEORGUSE" + Line + "_1").checked) {
            document.getElementById("lrblORGUSEORGUSE" + Line).innerText = "02";
            ChangeDisabled(document.getElementById("txtpnlListAreaSTORICODE" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE01" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE02" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE03" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE04" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaTORITYPE05" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaYTORICODE" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaKTORICODE" + Line), true);
            ChangeDisabled(document.getElementById("txtpnlListAreaSEQ" + Line), true);
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
// ○項目変更時名称取得処理
function f_onchnage(obj, Line, fieldNM) {
    // 送信用FormData オブジェクトを用意
    var fd = new FormData();
    // XMLHttpRequest オブジェクトを作成
    var xhr = new XMLHttpRequest();
    // 「POST メソッド」「接続先 URL」を指定
    xhr.open("POST", "../GR/GRMC0003AJAX.ashx", true)
    xhr.setRequestHeader('content-type', 'application/x-www-form-urlencoded;charset=UTF-8');
    fd = 'INPARAM=' + obj.firstChild.value
        + '&ACTION=' + fieldNM
        + '&COMPANY=' + document.getElementById("WF_SEL_CAMPCODE").value
        + '&ROLE=' + document.getElementById("MF_ORG_ROLE").value;
    var nnode = obj.nextSibling;
    if (nnode.firstElementChild != null) {
        nnode.firstElementChild.disabled = "disabled";
    }
    // イベント設定
    // ⇒XHR 送信正常で実行されるイベント
    xhr.onload = function (e) {
        var rnode = obj.nextSibling;
        if (rnode.firstElementChild != null) {
            rnode.firstElementChild.disabled = "";
        }
        if (e.currentTarget.status == 200) {
            if (rnode.firstElementChild != null) {
                rnode.firstElementChild.value = xhr.responseText;
            } else {
                rnode.innerHTML = xhr.responseText;
            }

        } else {
            document.getElementById("WF_MESSAGE").textContent = "取得に失敗しました。";
            document.getElementById("WF_MESSAGE").style.color = "red";
            document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        }
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