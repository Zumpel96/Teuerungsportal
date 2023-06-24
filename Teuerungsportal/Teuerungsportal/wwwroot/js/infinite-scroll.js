function onDivScroll(dotNetHelper) {
    window.addEventListener('scroll', function (e) {
        let scrollTop = Math.max(
            window.scrollY,
            document.documentElement.scrollTop,
            document.body.scrollTop
        );

        let scrollHeight = Math.max(
            document.documentElement.scrollHeight,
            document.body.scrollHeight
        );

        let clientHeight = document.documentElement.clientHeight;
        
        let offset = 200;
        
        if (scrollTop + clientHeight >= scrollHeight - offset) {
            dotNetHelper.invokeMethodAsync('LoadMoreData');
        }
    });
}