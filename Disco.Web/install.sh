rm /etc/systemd/system/DiscoWeb.service;
cp ./DiscoWeb.service /etc/systemd/system/;
systemctl enable DiscoWeb.service;
systemctl start DiscoWeb.service;
systemctl status DiscoWeb.service;