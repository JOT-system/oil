﻿var kabe_sun = "pink";
var kabe_mon = "lightgrey";
var kabe_tue = "lightgrey";
var kabe_wed = "lightgrey";
var kabe_thu = "lightgrey";
var kabe_fri = "lightgrey";
var kabe_sat = "lightblue";
var firstAltMsg = "&nbsp";
var firstAltYMD = "&nbsp";

//行事テーブル(フラグ(1:祝日 2:記念日など),月,日,メッセージ)
var gyouji_tbl = new Array(
 1, 1, 1, "元旦",
 1, 1, 0, "成人の日",
 1, 2, 11, "建国記念の日",
 1, 4, 29, "みどりの日",
 1, 5, 3, "憲法記念日",
 1, 5, 4, "国民の休日",
 1, 5, 5, "こどもの日",
 1, 7, 0, "海の日",
 1, 9, 0, "敬老の日",
 1, 10, 0, "体育の日",
 1, 11, 3, "文化の日",
 1, 11, 23, "勤労感謝の日",
 1, 2, 23, "天皇誕生日",
 1, 3, 0, "春分の日",
 1, 9, 0, "秋分の日"
);
var kokuminLastCnt = gyouji_tbl.length / 4;
var saveBgColor = ""
var saveFgColor = ""

function carenda(num,calId) {
    var now = new Date();
    var year;
    var month;
    var date;
    var dValue = document.getElementById("dValue");
    document.getElementById("WF_ButtonSel").disabled = true;

    if (calId == undefined) { calId = 'WF_Calendar'; }
    // memo１日＝６０秒×６０分×２４時間＝８６４００秒、８６４００秒＝８６４０００００ミリ秒 
    switch (parseInt(num)) {
        case 0:
            //初期表示
            if (document.getElementById(calId).value == "") {
                year = now.getFullYear();
                month = now.getMonth() + 1;
                date = now.getDate();
            } else {
                var ymd = new Date(document.getElementById(calId).value);
                if (ymd != "Invalid Date") {
                    year = ymd.getFullYear();
                    month = ymd.getMonth() + 1;
                    date = ymd.getDate();
                } else {
                    year = now.getFullYear();
                    month = now.getMonth() + 1;
                    date = now.getDate();
                }
            }
            break;
        case 1:
            //前月表示
            var backMDate = new Date(parseInt(dValue.innerHTML) - 24 * 60 * 60 * 1000 * 1);
            if (backMDate.getMonth() == now.getMonth() && backMDate.getFullYear() == now.getFullYear()) {
                year = now.getFullYear();
                month = now.getMonth() + 1;
                date = now.getDate();
            } else {
                year = backMDate.getFullYear();
                month = backMDate.getMonth() + 1;
                date = -1;
            }
            break;
        case 2:
            //翌月表示
            var nextMDate = new Date(parseInt(dValue.innerHTML) + 24 * 60 * 60 * 1000 * 31);
            if (nextMDate.getMonth() == now.getMonth() && nextMDate.getFullYear() == now.getFullYear()) {
                year = now.getFullYear();
                month = now.getMonth() + 1;
                date = now.getDate();
            } else {
                year = nextMDate.getFullYear();
                month = nextMDate.getMonth() + 1;
                date = -1;
            }
            break;
    }

    dValue.innerHTML = (new Date(year, month - 1, 1)).getTime();

    var last_date = new Array(31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31);
    var editMsg;
    if (month == 2) {
        if (year % 4 == 0) {
            if ((year % 100 == 0) && (year % 400 != 0)) {
            } else {
                last_date[1] = 29;
            }
        }
    }

    editMsg = "";
    editMsg += "<table border=0 cellspacing=3><tr><td style='cursor:pointer;background:white' align='center' onclick='carenda(1)'><b style='font-size:large;'>&lt;&lt;</b></td><td colspan='5' align='center' style='background:white'><b stylr='font-size:x-large'>" + year + "年" + month + "月</b></td><td style='cursor:pointer;background:white' align='center' onclick='carenda(2)'><b style='font-size:large'>&gt;&gt;</b></td></tr>\n";
    editMsg += "<tr>" + defTD("日", "red", kabe_sun) + defTD("月", "black", kabe_mon) + defTD("火", "black", kabe_tue) + defTD("水", "black", kabe_wed) + defTD("木", "black", kabe_thu) + defTD("金", "black", kabe_fri) + defTD("土", "blue", kabe_sat) + "</tr>\n";
    editMsg += "<tr>";

    for (dayIndex = 0; dayIndex < (new Date(year, month - 1, 1)).getDay() ; dayIndex++) {
        editMsg += defTD("&nbsp;", "white", "white");
    }

    //行事テーブル（祝日）の再設定
    //成人の日
    gyouji_tbl[1 * 4 + 2] = getSyukujituDate(year, 1, 2);
    //海の日
    gyouji_tbl[7 * 4 + 2] = getSyukujituDate(year, 7, 3);
    //敬老の日
    gyouji_tbl[8 * 4 + 2] = getSyukujituDate(year, 9, 3);
    //体育の日
    gyouji_tbl[9 * 4 + 2] = getSyukujituDate(year, 10, 2);
    //みどりの日4/29→昭和の日 国民の休日5/4→みどりの日
    if (year <= 2006) {
        gyouji_tbl[3 * 4 + 3] = "みどりの日";
        gyouji_tbl[5 * 4 + 3] = "国民の休日";
    } else {
        gyouji_tbl[3 * 4 + 3] = "昭和の日";
        gyouji_tbl[5 * 4 + 3] = "みどりの日";
    }
    //春分の日
    gyouji_tbl[13 * 4 + 2] = shunbun(year);
    //秋分の日
    gyouji_tbl[14 * 4 + 2] = shubun(year);

    //当日と行事が重なる場合の初期設定
    if (date != -1) {
        firstAltMsg = "&nbsp";
        firstAltYMD = year + "/" + month + "/" + date;
        for (var j = 0; j < kokuminLastCnt; j++) {
            if (gyouji_tbl[j * 4 + 1] == month && gyouji_tbl[j * 4 + 2] == date) {
                firstAltMsg += gyouji_tbl[j * 4 + 3] + "&nbsp";
            }
        }
    }

    for (i = 1; i <= last_date[month - 1]; i++) {
        if (i != 1 && dayIndex == 0) {
            editMsg += "<tr>";
        }

        var kabeColor;
        var fontColor;
        var altYMD = year + "/" + month + "/" + i;
        var altMsg = "&nbsp";
        //曜日別基本設定
        switch (dayIndex) {
            case 0: fontColor = "red";
                kabeColor = kabe_sun; break;
            case 1: fontColor = "black";
                kabeColor = kabe_mon; break;
            case 2: fontColor = "black";
                kabeColor = kabe_tue; break;
            case 3: fontColor = "black";
                kabeColor = kabe_wed; break;
            case 4: fontColor = "black";
                kabeColor = kabe_thu; break;
            case 5: fontColor = "black";
                kabeColor = kabe_fri; break;
            case 6: fontColor = "blue";
                kabeColor = kabe_sat; break;
        }

        //行事の時
        for (var j = 0; j < kokuminLastCnt; j++) {
            if (gyouji_tbl[j * 4 + 1] == month && gyouji_tbl[j * 4 + 2] == i) {
                //祝日
                if (gyouji_tbl[j * 4] == 1) {
                    fontColor = "red";
                } else {
                    kabeColor = "lightgreen";
                }
                altMsg += gyouji_tbl[j * 4 + 3] + "&nbsp";
            }
        }

        //選択された日（オレンジに）
        var ymd2 = new Date(document.getElementById(calId).value);
        if (i == ymd2.getDate() && year == ymd2.getFullYear() && month == ymd2.getMonth() + 1) {
            fontColor = "darkorange";
        } else {
            //当日（指定なしの場合、当日をオレンジに）
            if (document.getElementById(calId).value == "") {
                if (i == now.getDate() && year == now.getFullYear() && month == now.getMonth() + 1) {
                    fontColor = "darkorange";
                }
            }
        }

        editMsg += defTD2(i, fontColor, kabeColor, altYMD, altMsg);

        if (dayIndex == 6) {
            editMsg += "</tr>\n";
        }
        dayIndex++; dayIndex %= 7;
    }
    if (dayIndex != 7) {
        editMsg += "</tr>\n";
    }
    editMsg += "</table>\n";

    document.getElementById("carenda").innerHTML = editMsg;
}

function defTD(str, iro, kabe) {
    return "<td style='cursor:default;width:35px;hight:30px;background:" + kabe + "; color:" + iro + ";' align='center' ><b style='font-size: large;'>" + str + "</b></td>";
}
function defTD2(str, iro, kabe, altYMD, altMsg) {
    var editHTML = "";
    editHTML += "<td id='" + altYMD + "' style='cursor:default;width:35px;hight:30px;background:" + kabe + "; color:" + iro + ";' align='center' ";
    editHTML += "onclick=setDate('" + altYMD + "') ";
    editHTML += "onMouseOver=setAltMsg('" + altYMD + "','" + altMsg + "');setColor('" + altYMD + "',1); ";
    editHTML += "onMouseOut=setAltMsg('" + firstAltYMD + "','" + firstAltMsg + "');setColor('" + altYMD + "',2);>";
    editHTML += "<b style='font-size: large'>" + str + "</b></td>";
    return editHTML;
}
function setAltMsg(altYMD, altMsg) {
    var editAltMsg = "";
    editAltMsg += "<b stryle='font-size: xx-large'>" + altYMD + "</b>";
    editAltMsg += "<br>";
    editAltMsg += "<b stryle='font-size: xx-large'>" + altMsg + "</b>";
    document.getElementById("altMsg").innerHTML = editAltMsg;
}
function setDate(altYMD) {
    document.getElementById("WF_Calendar").value = altYMD;
    document.getElementById("WF_ButtonSel").disabled = false;
    document.getElementById('WF_ButtonSel').click();
}
function setColor(altYMD, event) {
    var Element = document.getElementById(altYMD);

    if (event == 1) {
        saveBgColor = Element.style.background;
        saveFgColor = Element.style.color;

        Element.style.background = 'blue';
        Element.style.color = 'white';
    } else {
        Element.style.background = saveBgColor;
        Element.style.color = saveFgColor;
    }
}
//祝日の日にち取得(年、月、第？週の月曜日)
function getSyukujituDate(year, month, syuu) {
    var syuuCnt = 0;
    for (var i = 1; i <= 31; i++) {
        var date = (new Date(year, month - 1, i)).getDay();
        if ((new Date(year, month - 1, i)).getDay() == 1) {
            syuuCnt++;
        }
        if (syuuCnt == syuu) {
            return i;
        }
    }
}
//春分の日の取得(年)
function shunbun(y) {
    if (y < 1900 || y > 2099) return;
    switch (y % 4) {
        case 0:
            if (y <= 1956) return 21;
            if (y <= 2088) return 20;
            return 19;
        case 1:
            if (y <= 1989) return 21;
            return 20;
        case 2:
            if (y <= 2022) return 21;
            return 20;
        case 3:
            if (y <= 1923) return 22;
            if (y <= 2055) return 21;
            return 20;
    }
}
//秋分の日の取得(年)
function shubun(y) {
    if (y < 1900 || y > 2099) return;
    switch (y % 4) {
        case 0:
            if (y <= 2008) return 23;
            return 22;
        case 1:
            if (y <= 1917) return 24;
            if (y <= 2041) return 23;
            return 22;
        case 2:
            if (y <= 1946) return 24;
            if (y <= 2074) return 23;
            return 22;
        case 3:
            if (y <= 1979) return 24;
            return 23;
    }
}
