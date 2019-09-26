// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    // 左ボックス

    // 右ボックス
    if (document.getElementById("WF_RightboxOpen").value == "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    };


    //更新ボタン活性／非活性
    if (document.getElementById("WF_MAPpermitcode").value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonRELEASE").hidden = "";
        document.getElementById("WF_ButtonJOURNAL").hidden = "";
        document.getElementById("WF_ButtonBUMON").hidden = "";
                
    } else {
        //非活性 
        document.getElementById("WF_ButtonRELEASE").hidden = "hidden";
        document.getElementById("WF_ButtonJOURNAL").hidden = "hidden";
        document.getElementById("WF_ButtonBUMON").hidden = "hidden";
    };

    //更新ボタン活性／非活性
    if (document.getElementById("WF_MAPpermitcode2").value == "TRUE") {
        //活性
        document.getElementById("WF_ButtonLIMIT").disabled = "";
    } else {
        //非活性 
        document.getElementById("WF_ButtonLIMIT").disabled = "disabled";
    };

    document.getElementById("WF_FORCE").value = "FALSE"
    if (document.getElementById("WF_NGREC").value != "" || document.getElementById("WF_UNAPPLIED").value != "") {
        var MegStr = ""
        if (document.getElementById("WF_NGREC").value != "") {
            MegStr = MegStr + "不完全な勤務データがあります。\n"
            MegStr = MegStr + "正しく入力してください。\n【"
            MegStr = MegStr + document.getElementById("WF_NGREC").value;
            MegStr = MegStr + "】\n\n"
        }
        if (document.getElementById("WF_UNAPPLIED").value != "") {
            MegStr = MegStr + "申請していない（未申請）残業申請があります。\n"
            MegStr = MegStr + "申請・承認を行って下さい。\n【"
            MegStr = MegStr + document.getElementById("WF_UNAPPLIED").value;
            MegStr = MegStr + "】"
        }
        alert(MegStr);
    } else {
        //現時点(2018/10）で強制締可は行わないのでelseのみ
        if (document.getElementById("WF_LIMITKBN").value == "強制締可") {
            if (document.getElementById("WF_UNAPPROVED").value != "") {
                var MegStr = "承認されていない（未承認）残業申請があります。\n"
                MegStr = MegStr + "「ＯＫ」：代行承認し締処理続行。「キャンセル」処理を中断\n【"
                MegStr = MegStr + document.getElementById("WF_UNAPPROVED").value;
                MegStr = MegStr + "】"
                result = confirm(MegStr);
                if (result == true) {
                    document.getElementById("WF_FORCE").value = "TRUE"
                    ButtonClick("WF_ButtonLIMIT");
                };
            };
        } else {
            if (document.getElementById("WF_UNAPPROVED").value != "") {
                var MegStr = "承認されていない（未承認）残業申請があります。\n"
                MegStr = MegStr + "【"
                MegStr = MegStr + document.getElementById("WF_UNAPPROVED").value;
                MegStr = MegStr + "】"
                alert(MegStr);
            };
        };
    };
};
// ○ダウンロード処理(ZIP)
function Z_DownLoad() {
    // リンク参照
    location.href = document.getElementById("WF_DownURL").value;
    //document.getElementById("headerbox").style.visibility = "visible";
    //document.getElementById("detailbox").style.visibility = "hidden";
};

// ○ダウンロード処理
function f_DownLoad() {
    window.open(document.getElementById("WF_DownURL").value, "view", "_blank");
};
