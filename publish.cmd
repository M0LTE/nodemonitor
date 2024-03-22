@echo off
ssh root@services-pi-3 systemctl stop nodemonitor
scp -r publish\* root@services-pi-3:/opt/nodemonitor/
ssh root@services-pi-3 systemctl start nodemonitor