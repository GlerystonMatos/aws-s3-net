FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY *.sln ./
COPY AwsS3/*.csproj ./AwsS3/
RUN dotnet restore

COPY AwsS3/. ./AwsS3/

WORKDIR /app/AwsS3
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

COPY --from=build /app/AwsS3/out .

ENTRYPOINT [ "dotnet", "AwsS3.dll" ]