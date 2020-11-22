#!/bin/bash
# Author：
#   yuzliang
# Program：
#	This is registry installation script
# History：
#	2019-06-20, Repalce docker image
#	2019-06-11, Restart when adding the current register exists
#   2019-08-19, aws registrator 还原

localIpUrl=http://{aws ip}/latest/meta-data/local-ipv4
consulUrl=consul.xxx.com:8500

dockerName=$(  docker container ls -a -q -f name=registrator  )
if [ "$dockerName" == "" ]; then
	hostIp=$( curl $localIpUrl )
	echo "The host ip is $hostIp"
	echo "The consul url is $consulUrl"

	docker run -d  \
	--name=registrator \
	--net=host \
	--volume=/var/run/docker.sock:/tmp/docker.sock  \
	gliderlabs/registrator:latest  -cleanup -internal=false -ip=$hostIp  consul://$consulUrl

	echo "The registry is installed and started"

	unset hostIp
else
	dockerName=$(  docker container ls -q -f name=registrator  )
	if [ "$dockerName" == "" ]; then
		docker start registrator
		echo "Skip the installation, the registry has been restarted"
	else
		echo "Skip the installation, the registry is installed and started"
	fi
fi

unset localIpUrl
unset consulUrl
unset dockerName

exit 0