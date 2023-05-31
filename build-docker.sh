#!/bin/bash
REGISTRY=registry.chaos-collision.de

echo "Logging in to registry $REGISTRY"

if [ -z "$REG_PW" ]; then
	echo "ENV variable REG_PW must be set with registry password"
	exit 1
fi

if [ -z "$REG_USER" ]; then
	echo "ENV variable REG_USER must be set with registry username"
	exit 1
fi

docker login $REGISTRY -p $REG_PW -u $REG_USER
docker buildx build --platform=linux/arm64  --build-arg DOTNET_ARCH="arm64" --build-arg BASE_IMAGE_ARCH="-arm64v8" -t $REGISTRY/helios:arm64-v1.0 -t $REGISTRY/helios:latest --push .
