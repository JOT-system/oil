// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {
    //前回の表示位置に移動する
    document.all('RepDetail').scrollTop = Number(document.getElementById("WF_SaveY").value);
    document.all('RepDetail').scrollLeft = Number(document.getElementById("WF_SaveX").value);

    //Repeaterスケジュール日付部　スクロールイベント取得
    if (window.document.getElementById("RepHeaderL").addEventListener) window.document.getElementById("RepHeaderL").addEventListener('DOMMouseScroll', wheel, false);
    window.document.getElementById("RepHeaderL").onmousewheel = document.getElementById("RepHeaderL").nmousewheel = wheel;
};

// ○Repeater行変更処理
function Repeater_focus(LineCnt, ColCnt) {
    document.getElementById("WF_REP_LineCnt").value = LineCnt;
    document.getElementById("WF_REP_ColCnt").value = ColCnt;
};

// ○Repeater行変更処理
function Repeater_Change() {
    document.getElementById("WF_SaveX").value = document.all('RepDetail').scrollLeft;
    document.getElementById("WF_SaveY").value = document.all('RepDetail').scrollTop;

    document.getElementById("WF_ButtonClick").value = "WF_REP_TEXTchange";
    document.forms[0].submit();                             //aspx起動
};

// ○セレクタ情報取得処理（地域）
function SELarea_Change(VALarea) {
    document.getElementById("WF_SaveX").value = 0;
    document.getElementById("WF_SaveY").value = 0;
    document.getElementById("WF_SELECTAREA").value = VALarea;
    document.getElementById("WF_ButtonClick").value = "WF_SELECT_SW";
    document.forms[0].submit();                             //aspx起動
};

// ○セレクタ情報取得処理（日付）
function SELdate_Change(VALdate) {
    document.getElementById("WF_SaveX").value = 0;
    document.getElementById("WF_SaveY").value = 0;
    document.getElementById("WF_SELECTYYMMDD").value = VALdate;
    document.getElementById("WF_ButtonClick").value = "WF_SELECT_SW";
    document.forms[0].submit();                             //aspx起動
};

// ○Repeater処理（横スクロール、ヘッダを同時に移動する）
function f_Scroll() {
    y = document.all('RepDetail').scrollTop;
    x = document.all('RepDetail').scrollLeft;
    f_newXY(x, y);
}

function f_newXY(x, y) {
    document.all('RepHeaderC').scrollLeft = x;
    document.all('RepHeaderC').scrollTop = y;
    document.all('RepHeaderL').scrollLeft = x;
    document.all('RepHeaderL').scrollTop = y;
}

// ○ドロップ処理（処理抑止）
function f_dragEventCancel(event) {
    event.preventDefault();  //イベントをキャンセル
};

// ○DOM Scrollイベント無効処理（画面描画前）
function wheel(event) {
    var delta = 0;
    if (!event) /* For IE. */
        event = window.event;
    if (event.wheelDelta) { /* IE/Opera. */
        delta = event.wheelDelta / 120;
        if (window.opera)
            delta = -delta;
    } else if (event.detail) { /** Mozilla case. */
        delta = -event.detail / 3;
    }

    if (event.preventDefault) {
        event.preventDefault();
    }
    event.returnValue = false;
}