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
    } else if (document.getElementById("WF_LeftboxOpen").value == "TSHABANTABLEOpen") {
        document.getElementById("LF_LEFTBOX").style.width = "47em";
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
//        document.getElementById("WF_SelectFIELD").value = document.activeElement.id;
//        document.getElementById("MF_SUBMIT").value = "TRUE";
//        document.getElementById("WF_SelectedIndex").value = Line
//        document.getElementById("WF_ButtonClick").value = "WF_ListChange";
//        document.getElementById("WF_DISP_SaveX").value = document.getElementById("pnlListArea_DR").scrollLeft;
//        document.getElementById("WF_DISP_SaveY").value = document.getElementById("pnlListArea_DR").scrollTop;
//        document.body.style.cursor = "wait";
//        document.forms[0].submit();
	    let trlst = document.getElementById("pnlListArea_DL").getElementsByTagName("tr");
    	trlst[Line - 1].getElementsByTagName("th")[1].innerHTML ="更新" ; 
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
// ○項目変更時名称取得処理
function f_onchnage(obj, Line, fieldNM) {
    // 送信用FormData オブジェクトを用意
    var fd = new FormData();
    // XMLHttpRequest オブジェクトを作成
    var xhr = new XMLHttpRequest();
    // 「POST メソッド」「接続先 URL」を指定
    xhr.open("POST", "../GR/GRMA0006AJAX.ashx", true)
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
