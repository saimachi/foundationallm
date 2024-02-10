const filters = {
	publicDirectory(path: string) {
		const config = useRuntimeConfig();
		// Only append the base URL if the path is not an absolute URL.
		if (path.startsWith('http')) {
			return path;
		}
		return config.app.baseURL + path;
	}
};

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.config.globalProperties.$filters = filters;
});
