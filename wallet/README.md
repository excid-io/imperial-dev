# CloudWallet
## Install
### Requirements
.net 6.0
dotnet-ef (dotnet tool install --global dotnet-ef)
### Create database
dotnet-ef migrations add InitialCreate
dotnet-ef database update

## Install as an Ubuntu service
cloud-wallet.service

[Unit]
Description=Cloud Wallet

[Service]
WorkingDirectory= <path>/CloudWallet
ExecStart=/usr/bin/dotnet "<path>/CloudWallet/bin/Debug/net6.0/CloudWallet.dll"
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=cloud-wallet
User=apache
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target

sudo cp cloud-wallet.service /etc/systemd/system/
sudo systemctl enable cloud-wallet.service
sudo systemctl start cloud-wallet.service
sudo systemctl status cloud-wallet.service

sudo journalctl -fu cloud-wallet.service