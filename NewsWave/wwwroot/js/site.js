window.NewsWave = {
    showClientError(error) {
        const banner = document.getElementById("clientError");
        if (!banner) return;

        const message = error?.message || String(error || "Неизвестная ошибка");
        banner.textContent = "Интерфейс DevExtreme не загрузился: " + message + ". Доступна обычная форма.";
        banner.hidden = false;
        console.error(error);
    },

    enhancePage(initializer) {
        try {
            if (!window.jQuery)
                throw new Error("jQuery недоступен");
            if (!window.DevExpress)
                throw new Error("DevExtreme недоступен");

            initializer();
            document.documentElement.classList.add("dx-ready");
        } catch (error) {
            this.showClientError(error);
        }
    }
};

window.addEventListener("error", event => {
    if (event.error)
        window.NewsWave.showClientError(event.error);
});
