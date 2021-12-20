FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

COPY *.sln ./
COPY AwsS3/*.csproj ./AwsS3/
RUN dotnet restore

COPY AwsS3/. ./AwsS3/

WORKDIR /app/AwsS3
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

COPY --from=build-env /app/AwsS3/out .

ENTRYPOINT [ "dotnet", "AwsS3.dll" ]