# anici
## Why did I make this
The original reasoning was to make sure all my routes for [mikibot/miki.anilist](https://github.com/mikibot/miki.anilist) were working correctly.

## Building the app
```sh
$ git clone https://github.com/velddev/anilist-cli.git
$ cd src
$ dotnet publish -c Release -o ../build/
```

## Using the app
```sh
$ cd ../build
$ dotnet anilist-cli.dll [args]
```
