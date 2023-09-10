mkdir -p ./storage;
# Detached, mount ./config and ./storage
docker run -d --restart=on-failure -it --init -v $(pwd)/config:/bot/config -v $(pwd)/storage:/bot/storage disco-matrix-bot:latest