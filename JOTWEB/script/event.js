/**
 * F5,Ctrl+Rを無効
*/

function preventDefault(evt) {
    if (!evt) return false;
    if (evt.preventDefault) evt.preventDefault();
    else if (evt.returnValue !== void 0) evt.returnValue = false;
    else if (evt.stopPropagation) evt.stopPropagation();
    return false;
}
function getKeyCode(evt) {
    var which = evt.which, keyCode = evt.keyCode, charCode = evt.charCode;
    if (keyCode !== void 0) return keyCode;
    else if (charCode !== void 0) return charCode;
    else if (which !== void 0) return which;
    else return null;
}
var addEvent;
if (window.addEventListener) {
    if (window.event === void 0)
        addEvent = function (element, type, listener, useCapture) {
            var process;
            if (typeof useCapture !== 'boolean') useCapture = false;
            if (typeof listener === 'function')
                process = function (evt) {
                    if (listener.call(element, evt) === false) return preventDefault(evt);
                };
            else if ('handleEvent' in listener)
                process = function (evt) {
                    if (listener.handleEvent.call(listener, evt) === false) return preventDefault(evt);
                };
            else return null;
            element.addEventListener(type, process, useCapture);
            return process;
        };
    else
        addEvent = function (element, type, listener, useCapture) {
            var process;
            if (typeof useCapture !== 'boolean') useCapture = false;
            if (typeof listener === 'function')
                process = function () {
                    if (listener.call(element, event) === false) return preventDefault(event);
                };
            else if ('handleEvent' in listener)
                process = function () {
                    if (listener.handleEvent.call(listener, event) === false) return preventDefault(event);
                };
            else return null;
            element.addEventListener(type, process, useCapture);
            return process;
        };
}
else if (window.attachEvent) {
    if (window.event === void 0)
        addEvent = function (element, type, listener) {
            var process;
            if (typeof listener === 'function')
                process = function (evt) {
                    if (listener.call(element, evt) === false) return preventDefault(evt);
                };
            else if ('handleEvent' in listener)
                process = function (evt) {
                    if (listener.handleEvent.call(listener, evt) === false) return preventDefault(evt);
                };
            else return null;
            element.attachEvent('on' + type, process);
            return process;
        };
    else
        addEvent = function (element, type, listener) {
            var process;
            if (typeof listener === 'function')
                process = function () {
                    if (listener.call(element, event) === false) return preventDefault(event);
                };
            else if ('handleEvent' in listener)
                process = function () {
                    if (listener.handleEvent.call(listener, event) === false) return preventDefault(event);
                };
            else return null;
            element.attachEvent('on' + type, process);
            return process;
        };
}
else {
    if (window.event === void 0)
        addEvent = function (element, type, listener) {
            var process;
            if (typeof listener === 'function')
                process = function (evt) {
                    if (listener.call(element, evt) === false) return preventDefault(evt);
                };
            else if ('handleEvent' in listener)
                process = function (evt) {
                    if (listener.handleEvent.call(listener, evt) === false) return preventDefault(evt);
                };
            else return null;
            if ('on' + type in document) element['on' + type] = process;
            return process;
        };
    else
        addEvent = function (element, type, listener) {
            var process;
            if (typeof listener === 'function')
                process = function () {
                    if (listener.call(element, event) === false) return preventDefault(event);
                };
            else if ('handleEvent' in listener)
                process = function () {
                    if (listener.handleEvent.call(listener, event) === false) return preventDefault(event);
                };
            else return null;
            if ('on' + type in document) element['on' + type] = process;
            return process;
        };
}
addEvent(document, 'keydown', function (evt) {
    var key = getKeyCode(evt);
    if (key === 116 || evt.ctrlKey && key === 82) {
        return false;
    }
});