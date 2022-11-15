FROM node:16-alpine

ENV BOT_URL=http://127.0.0.1:3978/api/messages
ENV DIRECTLINE_DOMAIN=127.0.0.1


# Create app directory
RUN mkdir -p /home/node/app/node_modules && chown -R node:node /home/node/app
WORKDIR /home/node/app

# Install app dependencies
# A wildcard is used to ensure both package.json AND package-lock.json are copied
# where available (npm@5+)
COPY package*.json ./

USER node

RUN npm install
# If you are building your code for production
# RUN npm ci --only=production
# RUN npm ci --ignore-scripts && npm cache clean --force


# Bundle app source
COPY --chown=node:node . .

RUN npm run build && npm prune --omit=dev

EXPOSE 3000
CMD [ "node", "index.js" ]