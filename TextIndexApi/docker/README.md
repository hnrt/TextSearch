# How to set up TextIndexApi to run in a docker environment

Image name and its version can be set to whatever you like.

```
docker build -t textsearchapi:1 .
```

Container name can be set to whatever you like, too.

Environment variables of SPRING_DATASOURCE_URL, SPRING_DATASOURCE_USERNAME, and SPRING_DATASOURCE_PASSWORD need to be set appropriately.

```
docker run -d -e SPRING_DATASOURCE_URL=jdbc:postgresql://dbhost.example.com:5432/textsearch -e SPRING_DATASOURCE_USERNAME=postgres -e SPRING_DATASOURCE_PASSWORD=xyzzy2020! -p 8080:8080 --name tsapi textsearchapi:1
```

If you wish to host both API and database containers in the same docker network,
a network needs to be created in advance.

```
docker network create --driver bridge tsnet
```

And then `--network tsnet` needs to be added to the docker run command for both.
Also the database host part for SPRING_DATASOURCE_URL needs to be replaced with the name of the database container.

If you wish to restart the api on the host startup, do as follows.

```
docker update --restart=always tsapi
```
