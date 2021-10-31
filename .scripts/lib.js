const fs = require("fs");

const bootstrapBase = "node_modules/bootstrap/dist";
const cssDest = "src/Server/wwwroot/css";
const jsDest = "src/Server/wwwroot/js";

function copyBootstrapCss() {
    let src = bootstrapBase + "/css/bootstrap.min.css";
    let dest = cssDest + "/bootstrap.min.css";
    console.log(`Copy file ${src} to ${dest}`);
    fs.copyFileSync(src, dest);

    src = bootstrapBase + "/css/bootstrap.min.css.map";
    dest = cssDest + "/bootstrap.min.css.map";
    console.log(`Copy file ${src} to ${dest}`);
    fs.copyFileSync(src, dest);
}

function copyBootstrapJs()  {
    let src = bootstrapBase + "/js/bootstrap.bundle.min.js";
    let dest = jsDest + "/bootstrap.bundle.min.js";
    console.log(`Copy file ${src} to ${dest}`);
    fs.copyFileSync(src, dest);

    src = bootstrapBase + "/js/bootstrap.bundle.min.js.map";
    dest = jsDest + "/bootstrap.bundle.min.js.map";
    console.log(`Copy file ${src} to ${dest}`);
    fs.copyFileSync(src, dest);
}

console.log("Copy css");
copyBootstrapCss();

console.log("Copy js");
copyBootstrapJs();