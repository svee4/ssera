FROM mcr.microsoft.com/dotnet/sdk:9.0 as sdk

WORKDIR /app

COPY . .
WORKDIR /app/src/Ssera.Api

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 as runtime

WORKDIR /app
COPY --from=sdk /app/src/Ssera.Api/out .

RUN useradd -ms /bin/bash dockeruser

RUN mkdir /volume
RUN chown -R dockeruser:dockeruser /volume

USER dockeruser:dockeruser

ENTRYPOINT ["dotnet", "Ssera.Api.dll"]
