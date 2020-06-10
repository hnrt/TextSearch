# How to set up Postgresql in a Docker environment

Image name and its version can be set to whatever you like.

```
docker build -t textsearchdb:1 .
```

Container name, password, and database name can be set to whatever you like, too.

```
mkdir data

docker run -d -e POSTGRES_PASSWORD=xyzzy2020! -e POSTGRES_DB=textsearch -v `pwd`/data:/var/lib/postgresql/data -p 5432:5432 --name tsdb textsearchdb:1
```

If you wish to restart the database on the host startup, do as follows.

```
docker update --restart=always tsdb
```