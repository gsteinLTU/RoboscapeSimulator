ARG BASE=node:latest
FROM $BASE

WORKDIR /source/roboscape-simulator

RUN apt-get update && apt-get install -y curl wget build-essential libssl-dev

COPY package*.json ./

RUN cd /source/roboscape-simulator && npm i --only=production

COPY ./ ./

ENV DEBUG=roboscape-sim:*

EXPOSE 8000
EXPOSE 9208

CMD ["sh", "-c", "node index.js"]