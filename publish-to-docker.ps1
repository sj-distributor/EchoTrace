$networks = docker network ls
$networkExists = $networks | Where-Object { $_ -like "*docker-network*" }
if(!$networkExists )
{
   docker network create docker-network
}
docker  container stop EchoTrace
docker  container rm EchoTrace
docker  image rm EchoTrace:latest
docker build  --tag EchoTrace:latest . 
docker run -id --name EchoTrace --restart=no  --network=docker-network  -p 8006:80 -p 8106:443  -e ASPNETCORE_ENVIRONMENT=Development EchoTrace:latest 