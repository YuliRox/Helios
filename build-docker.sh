#!/bin/bash
REGISTRY=yulirox-pi:5000
docker buildx build --platform=linux/arm64  --build-arg DOTNET_ARCH="arm64" --build-arg BASE_IMAGE_ARCH="-arm64v8" -t $REGISTRY/helios:arm64-v1.0 --push .