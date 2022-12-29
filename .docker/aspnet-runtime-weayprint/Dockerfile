FROM mcr.microsoft.com/dotnet/aspnet:6.0.12

RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        python3-pip \
        python3-cffi \
        python3-brotli \
        libpango-1.0-0 \
        libharfbuzz0b \
        libpangoft2-1.0-0 \
    && rm -rf /var/lib/apt/lists/*