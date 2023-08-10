# Disco Matrix Bot

Disco matrix verification and password reset bot. Created with [matrix-bot-sdk](https://www.npmjs.com/package/matrix-bot-sdk) and [matrix-bot-sdk-bot-template](https://github.com/turt2live/matrix-bot-sdk-bot-template).

## Running / Building

To build it: `npm run build`

To run it: `npm run dev`

To check the lint: `npm run lint`

To build the Docker image (requires root): `./build.sh`

To run the Docker image (after building, requires root): `./run.sh`
*Note that this will require a `config/production.yaml` file to exist as the Docker container runs in production mode.*

## Docker Notes

The container is about 1.1GB, so I would recommend pulling the repository on the server you deploy this to and building it there instead of trying to build the container on a build server, then uploading and downloading it again, which might take many minutes.

### Configuration

This template uses a package called `config` to manage configuration. The default configuration is offered
as `config/default.yaml`. Copy/paste this to `config/development.yaml` and `config/production.yaml` and edit
them accordingly for your environment.

