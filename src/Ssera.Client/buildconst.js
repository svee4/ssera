
// fucked up script to define constant variables at build time
// variables are put in src/lib/const and should be exported into a nice usable api from there

import { argv } from "node:process";
import { writeFile, readFile } from "node:fs/promises";

const options = [
	"ApiDomain"
];

console.log(`Available options: ${options.join(", ")}`)

for (let i = 2; i < argv.length; i++)
{
	const arg = /** @type {string} */ argv[i];
	const index = arg.indexOf("=");
	const [name, value] = [arg.slice(0, index), arg.slice(index + 1)];

	if (!options.includes(name)) {
		console.warn(`Invalid argument '${name}'`);
		continue;
	}
	
	const file = `src/lib/const/${name}.ts`;
	await writeFile(file, `export const ${name} = ${value}`);
	console.log(await readFile(file))
}
