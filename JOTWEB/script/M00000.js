// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {
      document.getElementById("rightb").style.visibility = "hidden";
//    document.getElementById("rightb").style.visibility = "visible";
};

// ○GridView処理（Enter処理）
document.onkeydown = function (event) {
    if (window.event.keyCode == 13) {
        if (document.getElementById("MF_SUBMIT").value == "FALSE") {
            document.getElementById("MF_SUBMIT").value = "TRUE"
            document.getElementById("WF_ButtonClick").value = "WF_ButtonOK";
            document.body.style.cursor = "wait";
            document.forms[0].submit();                            //aspx起動
        };
    };
};

