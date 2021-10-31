const fs = require("fs");

function copyBootstrapCss(dest) {
    const src = "node_modules/bootstrap/dist/css/bootstrap.min.css";

    console.log(`Copy file ${src} to ${dest}`);
    fs.copyFileSync(src, dest);
}

function copyBootstrapJs(dest)  {
    const src = "node_modules/bootstrap/dist/js/bootstrap.bundle.min.js";

    console.log(`Copy file ${src} to ${dest}`);
    fs.copyFileSync(src, dest);
}

function createDirectory(dir) {
    console.log(`Create directory ${dir}`);
    fs.mkdirSync(dir, {recursive: true});
}

const projects = ["src/Client.Book", "src/Client.Episodes", "src/Client.Movie", "src/Client.ReadList", "src/Client.Reader", "src/Client.VideoPlayer", "src/Server"]

projects.forEach(function (project) {
    let dir = project + "/wwwroot/css";
    createDirectory(dir);
    copyBootstrapCss(dir + "/bootstrap.min.css");

    dir = project + "/wwwroot/js";
    createDirectory(dir);
    copyBootstrapJs(dir + "/bootstrap.bundle.min.js");
});