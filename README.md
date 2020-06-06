# Text Search Program

This repository contains a utility program that searches for files that contain a designated text phrase.

The program consists of some parts:

- TextSearchCLI
- TextIndexApi

TextSearchCLI is a command line program module implemented in C#. This module uses TextIndexApi to get text distribution information.

TextIndexApi is a web API module implemented in Java with Spring Boot. This module maintains text index information with PostgreSQL database.

## How to set up

### Database

1. Prepare PostgreSQL database.

1. Create tables with TextSearch/TextIndexApi/postgresql/create_tables.sql by using psql command.

### TextIndexApi

1. Define environment variables:

    - SPRING_DATASOURCE_URL (e.g. `jdbc:postgresql://dbhost.example.com:5432/dbname`)
    - SPRING_DATASOURCE_USERNAME
    - SPRING_DATASOURCE_PASSWORD

1. Run the jar file built with gradle. e.g. `java -jar TextIndexApi.jar --server.port=8080`

### TextSearch

1. Define an environment variable:

    - TEXTINDEXAPI_URL (URL for TextIndexApi: e.g. `http://apihost.example.com:8080`)

## How to run

1. Specify file extensions to be indexed: `TextSearchCLI.exe -ext c,cpp,h,rc,cs,java`
1. Specify directory to be skipped for indexing: `TextSearchCLI.exe -skip-dir bin,lib,obj`
1. Build text index: `TextSearchCLI.exe -g IndexGroupName -index DirectoryPath`
1. Search files for a phrase: `TextSearchCLI.exe -g IndexGroupName -query "phrase"`


# テキスト検索プログラム

このリポジトリには、指定されたテキストフレーズを含むファイルを検索するユーティリティプログラムが含まれています。

このプログラムはいくつかの部分から構成されています。

- TextSearchCLI
- TextIndexApi

TextSearchCLI は、C#で実装されたコマンドラインプログラムモジュールです。このモジュールは、TextIndexApi を使用してテキスト分布情報を取得します。

TextIndexApi は、Spring Boot を使って Java で実装された Web API モジュールです。このモジュールは、PostgreSQL データベースでテキストインデックス情報を保持します。

## セットアップ方法

### データベース

1. PostgreSQL データベースを用意してください。

1. psql コマンドを使って TextSearch/TextIndexApi/postgresql/create_tables.sql スクリプトでテーブルを作成してください。

### TextIndexApi

1. 次の環境変数を定義してください:

    - SPRING_DATASOURCE_URL (例 `jdbc:postgresql://dbhost.example.com:5432/dbname`)
    - SPRING_DATASOURCE_USERNAME
    - SPRING_DATASOURCE_PASSWORD

1. gradle でビルドした jar ファイルを実行してください。例 `java -jar TextIndexApi.jar --server.port=8080`

### TextSearch

1. 次の環境変数を定義してください:

    - TEXTINDEXAPI_URL (TextIndexApi の URL: 例 `http://apihost.example.com:8080`)

## 実行方法

1. 索引対象のファイルの拡張子を指定: `TextSearchCLI.exe -ext c,cpp,h,rc,cs,java`
1. 索引対象外のディレクトリを指定: `TextSearchCLI.exe -skip-dir bin,lib,obj`
1. テキスト索引を作成: `TextSearchCLI.exe -g 索引グループ名 -index ディレクトリパス`
1. フレーズでファイルを検索: `TextSearchCLI.exe -g 索引グループ名 -query "検索フレーズ"`
