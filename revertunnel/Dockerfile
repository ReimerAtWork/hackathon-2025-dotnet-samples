# Dockerfile for reverse SSH tunnel
FROM ubuntu:22.04

RUN apt-get update && \
    apt-get install -y autossh openssh-client && \
    rm -rf /var/lib/apt/lists/*

# Copy SSH private key into the image
COPY hackathon-proxy_key2.pem /root/hackathon-proxy_key2.pem
RUN chmod 600 /root/hackathon-proxy_key2.pem

# Run the static reverse SSH tunnel command with autossh and autoaccept host key
CMD ["autossh", "-N", "-T", "-R", "0.0.0.0:8080:google.com:80", "-i", "/root/hackathon-proxy_key2.pem", "-o", "StrictHostKeyChecking=no", "azureuser@team-awesome-hackathon.northeurope.cloudapp.azure.com"]
