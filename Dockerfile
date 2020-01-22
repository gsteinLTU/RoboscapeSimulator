FROM node:lts

WORKDIR /source/roboscape-simulator

COPY package.json /source/roboscape-simulator

RUN cd /source/roboscape-simulator && npm i --only=production

COPY . .

EXPOSE 8000
CMD ["sh", "-c", "node index.js"]