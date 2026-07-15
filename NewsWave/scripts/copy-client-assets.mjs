import { copyFile, cp, mkdir, rm } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const modules = join(root, "node_modules");
const target = join(root, "wwwroot", "vendor");

await rm(target, { recursive: true, force: true });

const files = [
  ["bootstrap/dist/css/bootstrap.min.css", "bootstrap/bootstrap.min.css"],
  ["bootstrap/dist/js/bootstrap.bundle.min.js", "bootstrap/bootstrap.bundle.min.js"],
  ["jquery/dist/jquery.min.js", "jquery/jquery.min.js"],
  ["jquery-validation/dist/jquery.validate.min.js", "jquery/jquery.validate.min.js"],
  ["jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js", "jquery/jquery.validate.unobtrusive.min.js"],
  ["devextreme-dist/css/dx.fluent.blue.light.css", "devextreme/dx.fluent.blue.light.css"],
  ["devextreme-dist/js/dx.all.js", "devextreme/dx.all.js"],
  ["devextreme-dist/js/localization/dx.messages.ru.js", "devextreme/dx.messages.ru.js"]
];

for (const [source, destination] of files) {
  const destinationPath = join(target, destination);
  await mkdir(dirname(destinationPath), { recursive: true });
  await copyFile(join(modules, source), destinationPath);
}

await cp(
  join(modules, "devextreme-dist", "css", "icons"),
  join(target, "devextreme", "icons"),
  { recursive: true }
);

console.log("NewsWave client assets are ready.");
