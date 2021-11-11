const fs = require("fs");

const bootstrapBase = "node_modules/bootstrap/dist";
const cssDest = "src/Server/wwwroot/css";

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

console.log("Copy css");
copyBootstrapCss();