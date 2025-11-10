module.exports = [
"[externals]/bootstrap/dist/js/bootstrap.bundle.min.js [external] (bootstrap/dist/js/bootstrap.bundle.min.js, cjs, async loader)", ((__turbopack_context__) => {

__turbopack_context__.v((parentImport) => {
    return Promise.all([
  "server/chunks/ssr/[externals]_bootstrap_dist_js_bootstrap_bundle_min_e20f7461.js"
].map((chunk) => __turbopack_context__.l(chunk))).then(() => {
        return parentImport("[externals]/bootstrap/dist/js/bootstrap.bundle.min.js [external] (bootstrap/dist/js/bootstrap.bundle.min.js, cjs)");
    });
});
}),
];