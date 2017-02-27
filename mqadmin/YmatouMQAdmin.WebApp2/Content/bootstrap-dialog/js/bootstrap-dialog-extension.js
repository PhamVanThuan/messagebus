function infoDialog(message, url) {
    BootstrapDialog.show({
        title: "系统提示",
        type: BootstrapDialog.TYPE_INFO,
        //size: BootstrapDialog.SIZE_LARGE,
        message: message,
        buttons: [{
            label: '确定',
            action: function (dialogItself) {
                dialogItself.close();
                if (url != undefined && url != null && url != '')
                    window.location.href = url;
            }
        }]
    });
}

function successDialog(message, url) {
    BootstrapDialog.show({
        title: "系统提示",
        type: BootstrapDialog.TYPE_INFO,
        //size: BootstrapDialog.SIZE_LARGE,
        message: message,
        buttons: [{
            label: '确定',
            action: function (dialogItself) {
                dialogItself.close();
                if (url != undefined && url != null && url != '')
                    window.location.href = url;
            }
        }]
    });
}

function warningDialog(message, url) {
    BootstrapDialog.show({
        title: "系统提示",
        type: BootstrapDialog.TYPE_INFO,
        //size: BootstrapDialog.SIZE_LARGE,
        message: message,
        buttons: [{
            label: '确定',
            action: function (dialogItself) {
                dialogItself.close();
                if (url != undefined && url != null && url != '')
                    window.location.href = url;
            }
        }]
    });
}

function errorDialog(message, url) {
    BootstrapDialog.show({
        title: "系统提示",
        type: BootstrapDialog.TYPE_DANGER,
        //size: BootstrapDialog.SIZE_LARGE,
        message: message,
        buttons: [{
            label: '确定',
            action: function (dialogItself) {
                dialogItself.close();
                if (url != undefined && url != null && url != '')
                    window.location.href = url;
            }
        }]
    });
}

function confirmDialog(message, url) {
    BootstrapDialog.confirm({
        title: '系统提示',
        message: message,
        type: BootstrapDialog.TYPE_WARNING,
        closable: true,
        draggable: true,
        btnCancelLabel: '取消',
        btnOKLabel: '确定',
        btnOKClass: 'btn-warning',
        callback: function (result) {
            if (result) {
                alert('确定.');
                if (url != undefined && url != null && url != '')
                    window.location.href = url;
            } else {
                alert('取消.');
            }
        }
    });
}

function onBegin() {

    $("input[type='text']").each(function (i) {
        var txt = $.trim($("input[type='text']").eq(i).val());
        $("input[type='text']").eq(i).val(txt);
    });
    //$("input:submit").attr({ "disabled": "disabled" });
}

function onComplete() {
    //$("input:submit").removeAttr("disabled");
}