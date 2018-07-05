# MWInstaller
Packing/installation for Morrowind mods.

![Program Window](https://i.imgur.com/cOOOOpU.png)

See an example package list at [https://github.com/Petethegoat/ExampleModlist](https://github.com/Petethegoat/ExampleModlist).

#### Package List Example
```json
{
	"name": "Sample Modpack",
	"curator": "Petethegoat",
	"lastUpdated": "June 2018",
	"description": "This is an awesome example modpack.",
	"packages":
	[
		"http://coolfreehosting.com/expeditious_exit.json"
	]
}
```

#### Package Example
```json
{
	"name": "Expeditious Exit",
	"author": "NullCascade",
	"fileURL": "http://coolfreehosting.com/files/ExpeditiousExit.zip"
}
```

See a full list of fields in [Package.cs](Package.cs).
