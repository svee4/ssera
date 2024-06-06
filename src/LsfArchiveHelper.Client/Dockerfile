FROM node:21 AS build
ARG BUILDCONST

WORKDIR /app

COPY . .

RUN ls -Ah1

RUN node buildconst.js $BUILDCONST
RUN rm buildconst.js

RUN npm ci 
RUN npm run build
RUN npm prune

FROM node:21 AS run

ENV NODE_ENV=production

WORKDIR /app
COPY --from=build /app/build ./build
COPY --from=build /app/package.json ./package.json
COPY --from=build /app/node_modules ./node_modules

RUN useradd -ms /bin/bash dockeruser
USER dockeruser:dockeruser

ENTRYPOINT ["node", "build"]
