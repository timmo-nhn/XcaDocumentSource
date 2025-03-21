window.resizable = {
    startResize: function (element, onResize) {
        var initialX = 0;
        var initialWidth = 0;
        var resizeHandler;

        element.addEventListener('mousedown', function (e) {
            initialX = e.clientX;
            initialWidth = element.getBoundingClientRect().width;

            resizeHandler = function (e) {
                var width = Math.max(100, initialWidth + e.clientX - initialX); // Minimum width is 100px
                onResize(width);
            };

            document.addEventListener('mousemove', resizeHandler);
            document.addEventListener('mouseup', function () {
                document.removeEventListener('mousemove', resizeHandler);
            });
        });
    }
};

window.getContainerWidth = (element) => {
    if (element) {
        return element.getBoundingClientRect().width;
    }
    return 0;
};
