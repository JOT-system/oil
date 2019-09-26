// ○ディテール(PDF内容表示)処理
function FileDisplay(filename) {
    document.getElementById('WF_FileDisplay').value = filename;
    document.forms[0].submit();                            //aspx起動
}

// ○ダウンロード処理
function f_DownLoad() {
    // リンク参照
    location.href = document.getElementById("WF_HELPURL").value;
};