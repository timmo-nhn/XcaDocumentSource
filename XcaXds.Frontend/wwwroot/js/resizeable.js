window.resizablePanes = {
    init: function () {
        // Select all resizable components on the page
        const containers = document.querySelectorAll(".resizable-container");

        containers.forEach(container => {
            const resizer = container.querySelector(".resizer");
            const leftPane = container.querySelector(".left-pane");
            const rightPane = container.querySelector(".right-pane");

            let isResizing = false;
            let minWidth = parseInt(getComputedStyle(container).getPropertyValue("--min-width"));

            function startResize(event) {
                isResizing = true;
                document.addEventListener("mousemove", resize);
                document.addEventListener("mouseup", stopResize);
            }

            function resize(event) {
                let containerOffset = container.getBoundingClientRect().left;
                let adjustedOffset = event.clientX - containerOffset;

                if (adjustedOffset < minWidth) adjustedOffset = minWidth;

                const leftWidth = parseFloat(adjustedOffset);
                if (leftWidth < 200) {
                    leftPane.getElementsByClassName("card-body")[0].style.display = "none";

                } else {
                    leftPane.getElementsByClassName("card-body")[0].style.display = "";
                }

                leftPane.style.width = adjustedOffset + "px";
                rightPane.style.width = `calc(100% - ${adjustedOffset}px)`;
            }
            function toggle(event) {
                let containerOffset = container.getBoundingClientRect().left;
                let adjustedOffset = event.clientX - containerOffset;

                if (adjustedOffset < minWidth) adjustedOffset = minWidth;
                if (leftPane.style.width == "0px" || leftPane.style.width == "1px" || leftPane.style.width == "2px" || leftPane.style.width == "3px") {
                    leftPane.style.width = 350 + "px";
                    leftPane.getElementsByClassName("card-body")[0].style.display = "";

                }
                else {
                    leftPane.style.width = 0 + "px";
                    leftPane.getElementsByClassName("card-body")[0].style.display = "none";

                }

                rightPane.style.width = `calc(100% - ${adjustedOffset}px)`;
            }
            function stopResize() {
                isResizing = false;
                document.removeEventListener("mousemove", resize);
                document.removeEventListener("mouseup", stopResize);
            }

            resizer.addEventListener("mousedown", startResize);
            resizer.addEventListener("dblclick", toggle);
        });
    }
};

window.addEventListener("load", () => {
    window.resizablePanes.init();
});
