
// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("LF_LEFTBOX").style.width = "0em";
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value == "Open") {
        document.getElementById("LF_LEFTBOX").style.width = "26em";
    };

    if (document.getElementById('WF_RightboxOpen').value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };
    //更新ボタン活性／非活性
    if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonUPDATE").disabled = "";
        document.getElementById("WF_ButtonNEW").disabled = "";
        document.getElementById("WF_ButtonSAVE").disabled = "";
        document.getElementById("WF_ButtonDownload").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
        document.getElementById("WF_ButtonNEW").disabled = "disabled";
        document.getElementById("WF_ButtonSAVE").disabled = "disabled";
        document.getElementById("WF_ButtonDownload").disabled = "disabled";
    };
    /* 共通一覧のスクロールイベント紐づけ */
    bindListCommonEvents(pnlListAreaId, IsPostBack);
    addLeftBoxExtention(leftListExtentionTarget);
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
    var csvFlg = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
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
        if (f.toLowerCase() == "日報.csv") {
            csvFlg[2] = 1;
        }
        if (f.toLowerCase() == "配送.csv") {
            csvFlg[3] = 1;
        }
        if (f.toLowerCase() == "給油.csv") {
            csvFlg[4] = 1;
        }
        if (f.toLowerCase().match(/^jot/) != null) {
            csvFlg[5] = 1;
        }
        if (f.toLowerCase().match(/^cosmo/) != null) {
            csvFlg[6] = 1;
        }
        if (f.toLowerCase() == "syasai.csv") {
            csvFlg[7] = 1;
        }
        if (f.toLowerCase() == "yotei.csv") {
            csvFlg[8] = 1;
        }
        if (f.toLowerCase() == "exsyasai.csv") {
            csvFlg[9] = 1;
        }
        if (f.match(/.xls/i) != null || f.match(/.xlsx/i) != null) {
            xlsCnt += 1;
        }
    }
    //Excelファイルは、複数アップロードできない
    if (xlsCnt > 1) {
        document.getElementById("WF_MESSAGE").textContent = "Excelは、複数ファイル同時にアップロードできません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    //Excelとcsvは混在してアップロードできない
    if (xlsCnt >= 1 && (csvFlg[0] == 1 || csvFlg[1] == 1 || csvFlg[2] == 1 || csvFlg[3] == 1 || csvFlg[4] == 1 || csvFlg[5] == 1 || csvFlg[6] == 1)) {
        document.getElementById("WF_MESSAGE").textContent = "Excelとcsvは、同時にアップロードできません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    //csv（光英）は、yotei.csv と syasai.csv を同時にアップロードすること
    if ((csvFlg[7] == 1 && csvFlg[8] == 1) || (csvFlg[7] == 0 && csvFlg[8] == 0)) {
    } else {
        document.getElementById("WF_MESSAGE").textContent = "「syasai.csv」「yotei.csv」を同時にアップロードしてください。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    };
    //csv（矢崎）は、日報.csv と 配送.csv と 給油.csvを同時にアップロードすること
    if ((csvFlg[2] == 1 && csvFlg[3] == 1 && csvFlg[4] == 1) || (csvFlg[2] == 0 && csvFlg[3] == 0 && csvFlg[4] == 0)) {
    } else {
        document.getElementById("WF_MESSAGE").textContent = "「日報.csv」「配送.csv」「給油.csv」を同時にアップロードしてください。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }

    if (xlsCnt == 1) {
        upLoadFile = "EXCEL"
    }
    if (csvFlg[0] == 1 ) {
        upLoadFile = "JX_KOUEI"
    }
    if (csvFlg[1] == 1) {
        upLoadFile = "TG_KOUEI"
    }
    if (csvFlg[2] == 1 && csvFlg[3] == 1 && csvFlg[4] == 1) {
        upLoadFile = "YAZAKI"
    }
    if (csvFlg[5] == 1) {
        upLoadFile = "JOT_KOUEI"
    }
    if (csvFlg[6] == 1) {
        upLoadFile = "COSMO_KOUEI"
    }
    if (csvFlg[7] == 1 && csvFlg[8] == 1) {
        upLoadFile = "KOUEI"
    }
    if (csvFlg[9] == 1) {
        upLoadFile = "EX_KOUEI"
    }
    if (csvFlg[0] == 1 &&csvFlg[2] == 1 && csvFlg[3] == 1 && csvFlg[4] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と矢崎は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[7] == 1 && csvFlg[8] == 1 && csvFlg[2] == 1 && csvFlg[3] == 1 && csvFlg[4] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と矢崎は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[1] == 1 && csvFlg[2] == 1 && csvFlg[3] == 1 && csvFlg[4] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(TG)と矢崎は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[5] == 1 && csvFlg[2] == 1 && csvFlg[3] == 1 && csvFlg[4] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JOT)と矢崎は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[9] == 1 && csvFlg[2] == 1 && csvFlg[3] == 1 && csvFlg[4] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(EX)と矢崎は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[6] == 1 && csvFlg[2] == 1 && csvFlg[3] == 1 && csvFlg[4] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(COSMO)と矢崎は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[0] == 1 && csvFlg[5] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と光英(JOT)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[0] == 1 && csvFlg[9] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と光英(EX)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[7] == 1 && csvFlg[8] == 1 && csvFlg[5] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と光英(JOT)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[7] == 1 && csvFlg[8] == 1 && csvFlg[9] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と光英(EX)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[1] == 1 && csvFlg[5] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(TG)と光英(JOT)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[1] == 1 && csvFlg[9] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(TG)と光英(EX)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[6] == 1 && csvFlg[5] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(COSMO)と光英(JOT)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[6] == 1 && csvFlg[9] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(COSMO)と光英(EX)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[9] == 1 && csvFlg[5] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(EX)と光英(JOT)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[0] == 1 && csvFlg[1] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と光英(TG)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[7] == 1 && csvFlg[8] == 1 && csvFlg[1] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と光英(TG)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[0] == 1 && csvFlg[6] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と光英(COSMO)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[7] == 1 && csvFlg[8] == 1 && csvFlg[6] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(JX)と光英(COSMO)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
    }
    if (csvFlg[1] == 1 && csvFlg[6] == 1) {
        document.getElementById("WF_MESSAGE").textContent = "光英(TG)と光英(COSMO)は、同時にアップロード出来ません。";
        document.getElementById("WF_MESSAGE").style.color = "red";
        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
        return false;
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



