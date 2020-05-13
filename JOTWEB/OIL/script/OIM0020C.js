// ○OnLoad用処理（左右Box非表示）
function InitDisplay() {

    // 全部消す
    document.getElementById("RF_RIGHTBOX").style.width = "0em";

    if (document.getElementById('WF_LeftboxOpen').value === "Open") {
        document.getElementById("LF_LEFTBOX").style.display = "block";
    }

    addLeftBoxExtention(leftListExtentionTarget);

    if (document.getElementById('WF_RightboxOpen').value === "Open") {
        document.getElementById("RF_RIGHTBOX").style.width = "26em";
    }
    bindDragFileEvent();
}

function bindDragFileEvent() {
    let attachAreaObj = document.getElementById('divAttachmentArea');
    attachAreaObj.addEventListener('dragstart', function (event) { dragEventCancel(event); }, false);
    attachAreaObj.addEventListener('dragenter', function (event) { dragEventEnter(event); }, false);
    attachAreaObj.addEventListener('dragover', function (event) { dragOverEvent(event); }, false);
    attachAreaObj.addEventListener('dragleave', function (event) { dragEventLeave(event); }, false);
    attachAreaObj.addEventListener('drag', function (event) { dragEventCancel(event); }, false);
    attachAreaObj.addEventListener('drop', function () {
        return function (event) {
            let attachAreaObj = document.getElementById('divAttachmentArea');
            alert('まだ未実装です！');
            attachAreaObj.classList.remove('dragging');
            //masterDropEvent(event, bindObjInfo.kbn, bindObjInfo.acceptExtentions, handlerUrl);
        };
    }(), false);
}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function dragOverEvent(event) {
    //event.preventDefault();  //イベントをキャンセル
    event.preventDefault();
    event.dataTransfer.dropEffect = 'copy'; //ドラッグする文言を変更 CHROMEのみワーク
}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function dragEventCancel(event) {
    event.preventDefault();  //イベントをキャンセル
}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function dragEventEnter(event) {
    let attachAreaObj = document.getElementById('divAttachmentArea');
    event.preventDefault();  //イベントをキャンセル
    attachAreaObj;
    attachAreaObj.classList.add('dragging');
}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function dragEventLeave(event) {
    let attachAreaObj = document.getElementById('divAttachmentArea');
    event.preventDefault();  //イベントをキャンセル
    attachAreaObj.classList.remove('dragging');

}
