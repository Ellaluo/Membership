# Membership API

## Docker image URL

https://hub.docker.com/r/edocker2018/membership/

## Running steps

```ps
docker pull edocker2018/membership
docker run --name=membership edocker2018/membership 
docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' membership
```

The output will be the IP address that the docker container is running on. For example, the output is `172.23.147.92`, then the swagger documentation can be visited at  `172.23.147.92/swagger`.
