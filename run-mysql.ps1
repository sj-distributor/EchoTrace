$volumes = docker volume ls
$volumeExists = $volumes | Where-Object { $_ -like "*mysql-vol*" }
if (!$volumeExists)
{
    docker volume create mysql-vol
}
$networks = docker network ls
$networkExists = $networks | Where-Object { $_ -like "*docker-network*" }
if(!$networkExists )
{
   docker network create docker-network
}
docker pull mysql:8.0
docker run  --name mysql-server --network=docker-network  --env=MYSQL_ROOT_PASSWORD=123456  --volume=mysql-vol:/var/lib/mysql -p 3306:3306 --restart=no -d mysql:8.0
Install-Module -Name 'Carbon' -AllowClobber
Import-Module 'Carbon'
Set-CHostsEntry -IPAddress 127.0.0.1 -HostName 'mysql-server' -Description "docker mysql server"