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


# �e�L�X�g�����v���O����

���̃��|�W�g���ɂ́A�w�肳�ꂽ�e�L�X�g�t���[�Y���܂ރt�@�C�����������郆�[�e�B���e�B�v���O�������܂܂�Ă��܂��B

���̃v���O�����͂������̕�������\������Ă��܂��B

- TextSearchCLI
- TextIndexApi

TextSearchCLI �́AC#�Ŏ������ꂽ�R�}���h���C���v���O�������W���[���ł��B���̃��W���[���́ATextIndexApi ���g�p���ăe�L�X�g���z�����擾���܂��B

TextIndexApi �́ASpring Boot ���g���� Java �Ŏ������ꂽ Web API ���W���[���ł��B���̃��W���[���́APostgreSQL �f�[�^�x�[�X�Ńe�L�X�g�C���f�b�N�X����ێ����܂��B

## �Z�b�g�A�b�v���@

### �f�[�^�x�[�X

1. PostgreSQL �f�[�^�x�[�X��p�ӂ��Ă��������B

1. psql �R�}���h���g���� TextSearch/TextIndexApi/postgresql/create_tables.sql �X�N���v�g�Ńe�[�u�����쐬���Ă��������B

### TextIndexApi

1. ���̊��ϐ����`���Ă�������:

    - SPRING_DATASOURCE_URL (�� `jdbc:postgresql://dbhost.example.com:5432/dbname`)
    - SPRING_DATASOURCE_USERNAME
    - SPRING_DATASOURCE_PASSWORD

1. gradle �Ńr���h���� jar �t�@�C�������s���Ă��������B�� `java -jar TextIndexApi.jar --server.port=8080`

### TextSearch

1. ���̊��ϐ����`���Ă�������:

    - TEXTINDEXAPI_URL (TextIndexApi �� URL: �� `http://apihost.example.com:8080`)

## ���s���@

1. �����Ώۂ̃t�@�C���̊g���q���w��: `TextSearchCLI.exe -ext c,cpp,h,rc,cs,java`
1. �����ΏۊO�̃f�B���N�g�����w��: `TextSearchCLI.exe -skip-dir bin,lib,obj`
1. �e�L�X�g�������쐬: `TextSearchCLI.exe -g �����O���[�v�� -index �f�B���N�g���p�X`
1. �t���[�Y�Ńt�@�C��������: `TextSearchCLI.exe -g �����O���[�v�� -query "�����t���[�Y"`
