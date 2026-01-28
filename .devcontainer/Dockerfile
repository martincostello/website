FROM mcr.microsoft.com/devcontainers/dotnet:latest@sha256:83b3ef06623ee1ca350b12359f6be24cda4d206727d21412646f3cbdd5de21c1

# Suppress an apt-key warning about standard out not being a terminal. Use in this script is safe.
ENV APT_KEY_DONT_WARN_ON_DANGEROUS_USAGE=DontWarn

# Install Google Chrome
RUN wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | sudo apt-key add -
RUN echo 'deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main' | sudo tee /etc/apt/sources.list.d/google-chrome.list
RUN sudo apt-get update
RUN sudo apt-get --yes install google-chrome-stable 2>&1

# Install Firefox
RUN sudo apt-get --yes install firefox 2>&1
