// ○各ボタン押下処理
function ButtonClick(btn) {

    if (window.opener.closed) {
        window.close();
    } else {
        window.opener.document.getElementById("MF_ALERT").value = btn;
        window.opener.document.getElementById("WF_ButtonClick").value = document.getElementById("WF_ParentButton").value;
        window.close();
    }
};
