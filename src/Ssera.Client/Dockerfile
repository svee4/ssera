FROM node:22 AS build

WORKDIR /app

COPY . .

RUN npm ci 
RUN npm run build
RUN npm prune

FROM node:22 AS run

ENV NODE_ENV=production

WORKDIR /app
COPY --from=build /app/build ./build
COPY --from=build /app/package.json ./package.json
COPY --from=build /app/node_modules ./node_modules

RUN useradd -ms /bin/bash dockeruser
USER dockeruser:dockeruser

ENTRYPOINT ["node", "build"]
