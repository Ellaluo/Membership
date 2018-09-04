FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 65337
EXPOSE 44366

FROM microsoft/dotnet:2.1-sdk-nanoserver-1709 AS build
WORKDIR /src
COPY Membership/Membership.csproj Membership/
RUN dotnet restore Membership/Membership.csproj
COPY . .
WORKDIR /src/Membership
RUN dotnet build Membership.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Membership.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Membership.dll"]
