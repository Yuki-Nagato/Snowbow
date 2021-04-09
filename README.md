# Snowbow

A blog framework, compulsorily i18n support.

## Usage

Install [.NET 5.0 SDK](https://dotnet.microsoft.com/), [pandoc](https://pandoc.org/), and [Tidy](https://www.html-tidy.org/). Make sure they are in PATH:

```
$ dotnet --version
5.0.201
$ pandoc -v
pandoc 2.13
Compiled with pandoc-types 1.22, texmath 0.12.2, skylighting 0.10.5,
citeproc 0.3.0.9, ipynb 0.1.0.1
User data directory: /root/.local/share/pandoc
Copyright (C) 2006-2021 John MacFarlane. Web:  https://pandoc.org
This is free software; see the source for copying conditions. There is no
warranty, not even for merchantability or fitness for a particular purpose.
$ tidy -v
HTML Tidy for Linux version 5.6.0
```

To build your site, if you are in your site project directory:

```
dotnet run server
dotnet run build
```

Or if you want to specify the directories:

```
dotnet run --project path/to/Snowbow/Snowbow server -d path/to/site_project
dotnet run --project path/to/Snowbow/Snowbow build -d path/to/site_project
```

Server will listen on `http://127.0.0.1:4000`. Built result will be put in `./public/`.

## Example

A site project example is https://gitlab.com/Yuki-Nagato/yuki-nagato.gitlab.io .

The deployed result is https://blog.yuki-nagato.com/ .