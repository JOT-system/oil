// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";
    document.getElementById("leftbox").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        if (document.getElementById('WF_FIELD').value == "WF_GSHABAN" ||
            document.getElementById('WF_FIELD').value == "WF_CONTCHASSIS") {
            document.getElementById("leftbox").style.width = "51em";
        } else {
            document.getElementById("LF_LEFTBOX").style.width = "26em";
        };
    };
    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };
    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
    };

    //光英受信ボタン活性／非活性
    if (document.getElementById('WF_IsHideKoueiButton').value == "0") {
        //表示
        document.getElementById("WF_ButtonGet").style.visibility = "visible";
        if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
            //活性
            document.getElementById("WF_ButtonGet").disabled = "";
        } else {
            //非活性 
            document.getElementById("WF_ButtonGet").disabled = "disabled";
        };
    } else {
        //非表示 
        document.getElementById("WF_ButtonGet").style.visibility = "hidden";
    };

    // ○画面切替用処理（表示/非表示切替「ヘッダー、ディティール」）
    if (document.getElementById('WF_IsHideDetailBox').value == "0") {
        document.getElementById("headerbox").style.visibility = "hidden";
        document.getElementById("detailbox").style.visibility = "visible";
        // スクロールをTOP表示に切替
        f_ScrollTop(0, 0)

        //クリアボタン活性／非活性
        if (document.getElementById('WF_IsKoueiData').value == "1") {
            //非活性
            document.getElementById("WF_CLEAR").disabled = "disabled";
        } else {
            //活性
            document.getElementById("WF_CLEAR").disabled = "";
        };
    } else {
        document.getElementById("headerbox").style.visibility = "visible";
        document.getElementById("detailbox").style.visibility = "hidden";
    };


    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack);
    addLeftBoxExtention(leftListExtentionTarget);
};

// ○左BOX[テーブル]用処理（DBクリック選択+値反映）
function TableDBclick() {
    if (document.getElementById("MF_SUBMIT").value == "FALSE") {
        document.getElementById("MF_SUBMIT").value = "TRUE"
        if (document.getElementById('WF_SelectedIndex').value != "") {
            document.getElementById('WF_LeftboxOpen').value = "";
            document.getElementById("WF_ButtonClick").value = "WF_ListboxDBclick";
            document.body.style.cursor = "wait";
            document.forms[0].submit();
        }
    };
};
//○左BOX行情報取得処理（行情報退避用）
function Leftbox_Gyou(Line) {
    // Field名退避
    document.getElementById('WF_SelectedIndex').value = Line;
};

// ○Repeater行情報取得処理（行情報退避用）
function Repeater_Gyou(Line) {
    // Field名退避
    document.getElementById('WF_REP_POSITION').value = Line;
};

// ○Repeater処理（スクロール切替）
function f_ScrollTop(x, y) {
    document.all('WF_DViewRep1_Area').scrollTop = x;
    document.all('WF_DViewRep1_Area').scrollLeft = y;

};
// ○DetailBox入力・変更監視処理
function f_Rep1_Change(type) {
    document.getElementById("WF_REP_Change").value = type;
};


// ○ドロップ処理（ドラッグドロップ入力）
function f_dragEvent(e) {
    document.getElementById("WF_MESSAGE").textContent = "ファイルアップロード開始";
    document.getElementById("WF_MESSAGE").style.color = "blue";
    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";

    // ドラッグされたファイル情報を取得
    var files = e.dataTransfer.files;

    // 送信用FormData オブジェクトを用意
    var fd = new FormData();

    // ファイル情報をチェックする
    var csvFlg = [0, 0, 0];
    var xlsCnt = 0;
    var upLoadFile = ""
    for (var i = 0; i < files.length; i++) {
        var f = files[i].name
        if (f.match(/.csv/i) == null && f.match(/.xls/i) == null && f.match(/.xlsx/i) == null) {
            document.getElementById("WF_MESSAGE").textContent = "「" + f + "」は、アップロードできないファイルです。";
            document.getElementById("WF_MESSAGE").style.color = "red";
            document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
            return false;
        }
        if (f.toLowerCase().match(/^jx/) != null) {
            csvFlg[0] = 1;
        }
        if (f.toLowerCase().match(/^tg/) != null) {
            csvFlg[1] = 1;
        }
        if (f.toLowerCase().match(/^cosmo/) != null) {
            csvFlg[2] = 1;
        }
        if (f.match(/.xls/i) != null || f.match(/.xlsx/i) != null) {
            xlsCnt += 1;
        }
    }
    //Excel/CSVファイルは、複数アップロードできない
    if (files.length > 1) {
        document.getElementById("WF_MESSAGE").textContent = "複数ファイル同時にアップロードできません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    //Excelとcsvは混在してアップロードできない
    if (xlsCnt >= 1 && (csvFlg[0] == 1 || csvFlg[1] == 1 || csvFlg[2] == 1)) {
        document.getElementById("WF_MESSAGE").textContent = "Excelとcsvは、同時にアップロードできません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    //特定のcsv何れでもない場合はアップロードできない
    if (xlsCnt == 0 && (csvFlg[0] == 0 && csvFlg[1] == 0 && csvFlg[2] == 0)) {
        document.getElementById("WF_MESSAGE").textContent = "対象外csvファイルは、アップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    } if (xlsCnt == 1) {
        upLoadFile = "EXCEL"
    }
    if (csvFlg[0] == 1 || csvFlg[1] == 1 || csvFlg[2] == 1) {
        upLoadFile = "KOUEI"
    }

    // ファイル情報を追加する
    for (var i = 0; i < files.length; i++) {
        fd.append("files", files[i]);
    }

    // XMLHttpRequest オブジェクトを作成
    var xhr = new XMLHttpRequest();
    // 「POST メソッド」「接続先 URL」を指定
    xhr.open("POST", "../GR/GRCO0104XLSUPMULTI.ashx", false)

    // イベント設定
    // ⇒XHR 送信正常で実行されるイベント
    xhr.onload = function (e) {
        if (e.currentTarget.status == 200) {
            document.getElementById('WF_ButtonClick').value = "WF_UPLOAD_" + upLoadFile;
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

