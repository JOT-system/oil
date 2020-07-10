// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {
    document.getElementById("rightb").style.visibility = "hidden";
    bindLeftNaviClick();
}
/**
 * 左ナビゲーションクリックイベントバインド
 * @return {undefined} なし
 */
function bindLeftNaviClick() {
    /* 左ナビ全体のDivを取得 */
    let leftNavObj = document.getElementById('divLeftNav');
    /* 左ナビ未描画なら終了 */
    if (leftNavObj === null) {
        return;
    }
    /* ラベルタグ（左ナビボタン風デザイン）のオブジェクトを取得 */
    let labelObjList = leftNavObj.querySelectorAll("div[data-hasnext='1'] > label");
    /* 左ナビボタンが描画されてなければそのまま終了 */
    if (labelObjList === null) {
        return;
    }
    if (labelObjList.length === 0) {
        return;
    }
    /* 左ナビボタンのループ */
    for (let i = 0; i < labelObjList.length; i++) {
        let targetLabel = labelObjList[i];
        let parentDiv = targetLabel.parentNode;
        let posicol = parentDiv.dataset.posicol;
        let rowline = parentDiv.dataset.rowline;
        // ダイアログを閉じるタイミングでフォーカスを合わせる
        targetLabel.addEventListener('click', (function (posicol, rowline) {
            return function () {
                let hdnPosiColObj = document.getElementById('hdnPosiCol');
                hdnPosiColObj.value = posicol;
                let hdnRowLineObj = document.getElementById('hdnRowLine');
                hdnRowLineObj.value = rowline;
                ButtonClick('WF_ButtonLeftNavi'); /* 共通サブミット処理、VB側ロード時のSelectケースで割り振らせる */ 
            };
        })(posicol, rowline), false);

    }
}