# Snowbow

A blog framework, compulsorily i18n support.

## Usage

Install [.NET 5.0 SDK](https://dotnet.microsoft.com/).

If you are in your site project directory:

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